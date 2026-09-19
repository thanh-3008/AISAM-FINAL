using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository;

public sealed class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly AisamContext _context;

    public DeviceTokenRepository(AisamContext context)
    {
        _context = context;
    }

    public async Task<DeviceToken> RegisterOrUpdateAsync(
        Guid profileId,
        string token,
        string platform,
        string? deviceName,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.DeviceTokens
            .FirstOrDefaultAsync(d => d.Token == token, cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (existing == null)
        {
            var deviceToken = new DeviceToken
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Token = token,
                Platform = platform,
                DeviceName = deviceName,
                IsActive = true,
                CreatedAt = utcNow,
                LastActiveAt = utcNow
            };

            _context.DeviceTokens.Add(deviceToken);
            await _context.SaveChangesAsync(cancellationToken);
            return deviceToken;
        }

        existing.ProfileId = profileId;
        existing.Platform = platform;
        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            existing.DeviceName = deviceName;
        }
        existing.IsActive = true;
        existing.LastActiveAt = utcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> UnregisterAsync(string token, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DeviceTokens
            .FirstOrDefaultAsync(d => d.Token == token, cancellationToken);

        if (existing == null)
        {
            return false;
        }

        existing.IsActive = false;
        existing.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<DeviceToken>> GetActiveTokensByProfileIdAsync(
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeviceTokens
            .AsNoTracking()
            .Where(d => d.ProfileId == profileId && d.IsActive)
            .OrderByDescending(d => d.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetActiveTokensByProfileIdsAsync(
        IEnumerable<Guid> profileIds,
        CancellationToken cancellationToken = default)
    {
        var idList = profileIds.Distinct().ToList();
        if (idList.Count == 0)
        {
            return Array.Empty<DeviceToken>();
        }

        return await _context.DeviceTokens
            .AsNoTracking()
            .Where(d => idList.Contains(d.ProfileId) && d.IsActive)
            .OrderByDescending(d => d.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeactivateTokensAsync(
        IEnumerable<string> invalidTokens,
        CancellationToken cancellationToken = default)
    {
        var tokens = invalidTokens.Distinct().ToList();
        if (tokens.Count == 0) return;

        var records = await _context.DeviceTokens
            .Where(d => tokens.Contains(d.Token))
            .ToListAsync(cancellationToken);

        if (records.Count == 0) return;

        foreach (var record in records)
        {
            record.IsActive = false;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
