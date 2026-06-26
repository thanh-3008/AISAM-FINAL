using AISAM.Common;
using AISAM.Common.Dtos.Admin;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.Services.Service;

public class PlanService : IPlanService
{
    private readonly AisamContext _context;

    public PlanService(AisamContext context) { _context = context; }

    public async Task<GenericResponse<List<AdminPlanDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);
        return GenericResponse<List<AdminPlanDto>>.CreateSuccess(plans);
    }

    public async Task<GenericResponse<AdminPlanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        if (plan == null) return GenericResponse<AdminPlanDto>.CreateError("Plan not found.", HttpStatusCode.NotFound);
        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<AdminPlanDto>> CreateAsync(AdminCreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = new SubscriptionPlan
        {
            Name = request.Name, PlanType = request.PlanType, Price = request.Price,
            Currency = request.Currency, BillingCycle = request.BillingCycle,
            CreditsPerCycle = request.CreditsPerCycle, PostQuotaPerCycle = request.PostQuotaPerCycle,
            MemberLimit = request.MemberLimit, MaxCreditBalance = request.MaxCreditBalance,
            IsActive = true, SortOrder = request.SortOrder
        };
        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<AdminPlanDto>> UpdateAsync(Guid id, AdminUpdatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        if (plan == null) return GenericResponse<AdminPlanDto>.CreateError("Plan not found.", HttpStatusCode.NotFound);
        if (request.Name != null) plan.Name = request.Name;
        if (request.PlanType.HasValue) plan.PlanType = request.PlanType.Value;
        if (request.Price.HasValue) plan.Price = request.Price.Value;
        if (request.Currency != null) plan.Currency = request.Currency;
        if (request.BillingCycle != null) plan.BillingCycle = request.BillingCycle;
        if (request.CreditsPerCycle.HasValue) plan.CreditsPerCycle = request.CreditsPerCycle.Value;
        if (request.PostQuotaPerCycle.HasValue) plan.PostQuotaPerCycle = request.PostQuotaPerCycle.Value;
        if (request.MemberLimit.HasValue) plan.MemberLimit = request.MemberLimit.Value;
        if (request.MaxCreditBalance.HasValue) plan.MaxCreditBalance = request.MaxCreditBalance.Value;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<AdminPlanDto>.CreateSuccess(MapToDto(plan));
    }

    public async Task<GenericResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(new object[] { id }, cancellationToken);
        if (plan == null) return GenericResponse<bool>.CreateError("Plan not found.", HttpStatusCode.NotFound);
        plan.IsDeleted = true; plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true, "Plan deleted.");
    }

    private static AdminPlanDto MapToDto(SubscriptionPlan p) => new()
    {
        Id = p.Id, Name = p.Name, PlanType = p.PlanType, Price = p.Price,
        Currency = p.Currency, BillingCycle = p.BillingCycle, CreditsPerCycle = p.CreditsPerCycle,
        PostQuotaPerCycle = p.PostQuotaPerCycle, MemberLimit = p.MemberLimit,
        MaxCreditBalance = p.MaxCreditBalance, IsActive = p.IsActive, SortOrder = p.SortOrder,
        CreatedAt = p.CreatedAt
    };
}
