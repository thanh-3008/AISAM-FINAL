using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AISAM.Services.Service;

public class HolidayService : IHolidayService
{
    private readonly AisamContext _context;
    private readonly IAIService _aiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(
        AisamContext context,
        IAIService aiService,
        IMemoryCache cache,
        ILogger<HolidayService> logger)
    {
        _context = context;
        _aiService = aiService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResponse<IEnumerable<HolidayEventDto>>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"UpcomingHolidays_{days}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<HolidayEventDto>? cachedHolidays) && cachedHolidays != null)
        {
            return GenericResponse<IEnumerable<HolidayEventDto>>.CreateSuccess(cachedHolidays, "Upcoming holidays retrieved from cache.");
        }

        var today = DateTime.UtcNow.Date;
        var futureDate = today.AddDays(days);

        var holidays = await _context.HolidayEvents
            .AsNoTracking()
            .Where(h => h.IsActive && h.ExactDate >= today && h.ExactDate <= futureDate)
            .OrderBy(h => h.ExactDate)
            .Select(h => new HolidayEventDto
            {
                Id = h.Id,
                Name = h.Name,
                LocalName = h.LocalName,
                ExactDate = h.ExactDate,
                Year = h.Year,
                CountryCode = h.CountryCode
            })
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, holidays, TimeSpan.FromHours(12));

        return GenericResponse<IEnumerable<HolidayEventDto>>.CreateSuccess(holidays, "Upcoming holidays retrieved successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> GetSuggestionAsync(
        Guid workspaceId, Guid profileId, Guid userId, Guid brandId, Guid holidayId, CancellationToken cancellationToken = default)
    {
        var holiday = await _context.HolidayEvents.FindAsync(new object[] { holidayId }, cancellationToken);
        if (holiday == null)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Holiday event not found.", HttpStatusCode.NotFound);
        }

        var holidayName = holiday.LocalName ?? holiday.Name;
        var prompt = $"Hãy gợi ý một bài viết caption mạng xã hội ngắn gọn, sáng tạo và thu hút nhân dịp {holidayName}. Caption nên liên kết tự nhiên ý nghĩa của ngày lễ này với thương hiệu, gợi mở tương tác từ khách hàng.";

        var aiResponse = await _aiService.GenerateDraftAsync(profileId, workspaceId, userId, new CreateDraftRequest
        {
            BrandId = brandId,
            Prompt = prompt,
            AdType = AdTypeEnum.TextOnly
        }, cancellationToken);
        
        if (!aiResponse.Success || aiResponse.Data == null)
        {
            return GenericResponse<ContentResponseDto>.CreateError(aiResponse.Message ?? "Failed to generate holiday caption.", (HttpStatusCode)aiResponse.StatusCode);
        }

        var contentId = aiResponse.Data.ContentId;
        
        try
        {
            var content = await _context.Contents.FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);
            if (content != null)
            {
                content.Status = ContentStatusEnum.PendingApproval;
                content.GeneratedSource = $"HolidayEvent:{holidayId}";
                // Draft text is left empty in Content until approved, relying on AiGeneration matching the logic in AIService.
                // However, to make it immediately viewable in our new flow (since we use PendingApproval), 
                // we should copy the generated text if available.
                if (!string.IsNullOrWhiteSpace(aiResponse.Data.GeneratedText))
                {
                    content.TextContent = aiResponse.Data.GeneratedText;
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update generated source and status for Content {ContentId}.", contentId);
            return GenericResponse<ContentResponseDto>.CreateError("Generated caption successfully but failed to update status. Please try again.", HttpStatusCode.InternalServerError);
        }

        var updatedContent = await _context.Contents
            .Include(c => c.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);

        if (updatedContent == null) return GenericResponse<ContentResponseDto>.CreateError("Content not found after generation.", HttpStatusCode.NotFound);

        var dto = new ContentResponseDto
        {
            Id = updatedContent.Id,
            ProfileId = updatedContent.ProfileId,
            BrandId = updatedContent.BrandId,
            BrandName = updatedContent.Brand?.Name,
            ProductId = updatedContent.ProductId,
            AdType = updatedContent.AdType,
            Title = updatedContent.Title,
            TextContent = updatedContent.TextContent,
            ImageUrl = updatedContent.ImageUrl,
            VideoUrl = updatedContent.VideoUrl,
            StyleDescription = updatedContent.StyleDescription,
            ContextDescription = updatedContent.ContextDescription,
            RepresentativeCharacter = updatedContent.RepresentativeCharacter,
            IsAiGenerated = updatedContent.IsAiGenerated,
            Status = updatedContent.Status,
            CreatedAt = updatedContent.CreatedAt,
            UpdatedAt = updatedContent.UpdatedAt
        };

        return GenericResponse<ContentResponseDto>.CreateSuccess(dto, "Holiday caption suggested and saved successfully.");
    }
    public async Task<GenericResponse<ContentResponseDto>> GenerateHolidayVideoAsync(
        Guid workspaceId, Guid profileId, Guid userId, Guid brandId, Guid holidayId, CancellationToken cancellationToken = default)
    {
        var holiday = await _context.HolidayEvents.FindAsync(new object[] { holidayId }, cancellationToken);
        if (holiday == null)
        {
            return GenericResponse<ContentResponseDto>.CreateError("Holiday event not found.", HttpStatusCode.NotFound);
        }

        var holidayName = holiday.LocalName ?? holiday.Name;
        var prompt = $"Hãy gợi ý một kịch bản video ngắn gọn, sáng tạo và thu hút nhân dịp {holidayName}. Kịch bản nên liên kết tự nhiên ý nghĩa của ngày lễ này với thương hiệu, gợi mở tương tác từ khách hàng.";

        var aiResponse = await _aiService.GenerateDraftAsync(profileId, workspaceId, userId, new CreateDraftRequest
        {
            BrandId = brandId,
            Prompt = prompt,
            AdType = AdTypeEnum.VideoText
        }, cancellationToken);
        
        if (!aiResponse.Success || aiResponse.Data == null)
        {
            return GenericResponse<ContentResponseDto>.CreateError(aiResponse.Message ?? "Failed to generate holiday video script.", (HttpStatusCode)aiResponse.StatusCode);
        }

        var contentId = aiResponse.Data.ContentId;
        
        try
        {
            var content = await _context.Contents.FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);
            if (content != null)
            {
                content.Status = ContentStatusEnum.PendingApproval; // Will be picked up by video generator
                content.GeneratedSource = $"HolidayEventVideo:{holidayId}";
                if (!string.IsNullOrWhiteSpace(aiResponse.Data.GeneratedText))
                {
                    content.TextContent = aiResponse.Data.GeneratedText;
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update generated source and status for Content {ContentId}.", contentId);
            // Even if it fails, the script is generated. Return 500 so client knows it might need manual intervention.
            return GenericResponse<ContentResponseDto>.CreateError("Generated video script successfully but failed to update status. Please try again.", HttpStatusCode.InternalServerError);
        }

        var updatedContent = await _context.Contents
            .Include(c => c.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);

        if (updatedContent == null) return GenericResponse<ContentResponseDto>.CreateError("Content not found after generation.", HttpStatusCode.NotFound);

        var dto = new ContentResponseDto
        {
            Id = updatedContent.Id,
            ProfileId = updatedContent.ProfileId,
            BrandId = updatedContent.BrandId,
            BrandName = updatedContent.Brand?.Name,
            ProductId = updatedContent.ProductId,
            AdType = updatedContent.AdType,
            Title = updatedContent.Title,
            TextContent = updatedContent.TextContent,
            ImageUrl = updatedContent.ImageUrl,
            VideoUrl = updatedContent.VideoUrl,
            StyleDescription = updatedContent.StyleDescription,
            ContextDescription = updatedContent.ContextDescription,
            RepresentativeCharacter = updatedContent.RepresentativeCharacter,
            IsAiGenerated = updatedContent.IsAiGenerated,
            Status = updatedContent.Status,
            CreatedAt = updatedContent.CreatedAt,
            UpdatedAt = updatedContent.UpdatedAt
        };

        return GenericResponse<ContentResponseDto>.CreateSuccess(dto, "Holiday video script suggested and saved successfully.");
    }

    public async Task<GenericResponse<ContentResponseDto>> GetCustomEventSuggestionAsync(
        Guid workspaceId, Guid profileId, Guid userId, AISAM.Common.Dtos.Request.GenerateCustomEventContentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
        {
            return GenericResponse<ContentResponseDto>.CreateError("Event name is required.", HttpStatusCode.BadRequest);
        }

        // Sanitize EventName to prevent prompt injection
        var sanitizedEventName = request.EventName.Length > 100 ? request.EventName.Substring(0, 100) : request.EventName;
        sanitizedEventName = System.Text.RegularExpressions.Regex.Replace(sanitizedEventName, @"[{}<>]", "");

        var prompt = $"Hãy gợi ý nội dung mạng xã hội sáng tạo nhân dịp sự kiện đặc biệt: {sanitizedEventName}. Nội dung nên liên kết tự nhiên ý nghĩa của sự kiện này với thương hiệu, gợi mở tương tác.";

        var aiResponse = await _aiService.GenerateDraftAsync(profileId, workspaceId, userId, new CreateDraftRequest
        {
            BrandId = request.BrandId,
            Prompt = prompt,
            AdType = request.AdType
        }, cancellationToken);
        
        if (!aiResponse.Success || aiResponse.Data == null)
        {
            return GenericResponse<ContentResponseDto>.CreateError(aiResponse.Message ?? "Failed to generate custom event content.", (HttpStatusCode)aiResponse.StatusCode);
        }

        var contentId = aiResponse.Data.ContentId;
        
        try
        {
            var content = await _context.Contents.FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);
            if (content != null)
            {
                content.Status = ContentStatusEnum.PendingApproval;
                content.GeneratedSource = $"CustomEvent:{request.EventName}";
                if (!string.IsNullOrWhiteSpace(aiResponse.Data.GeneratedText))
                {
                    content.TextContent = aiResponse.Data.GeneratedText;
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update generated source and status for Content {ContentId}.", contentId);
            return GenericResponse<ContentResponseDto>.CreateError("Generated content successfully but failed to update status. Please try again.", HttpStatusCode.InternalServerError);
        }

        var updatedContent = await _context.Contents
            .Include(c => c.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contentId, cancellationToken);

        if (updatedContent == null) return GenericResponse<ContentResponseDto>.CreateError("Content not found after generation.", HttpStatusCode.NotFound);

        var dto = new ContentResponseDto
        {
            Id = updatedContent.Id,
            ProfileId = updatedContent.ProfileId,
            BrandId = updatedContent.BrandId,
            BrandName = updatedContent.Brand?.Name,
            ProductId = updatedContent.ProductId,
            AdType = updatedContent.AdType,
            Title = updatedContent.Title,
            TextContent = updatedContent.TextContent,
            ImageUrl = updatedContent.ImageUrl,
            VideoUrl = updatedContent.VideoUrl,
            StyleDescription = updatedContent.StyleDescription,
            ContextDescription = updatedContent.ContextDescription,
            RepresentativeCharacter = updatedContent.RepresentativeCharacter,
            IsAiGenerated = updatedContent.IsAiGenerated,
            Status = updatedContent.Status,
            CreatedAt = updatedContent.CreatedAt,
            UpdatedAt = updatedContent.UpdatedAt
        };

        return GenericResponse<ContentResponseDto>.CreateSuccess(dto, "Custom event content suggested and saved successfully.");
    }
}
