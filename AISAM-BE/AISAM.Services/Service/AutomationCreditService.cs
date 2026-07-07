using System.Net;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Services.Service;

public sealed class AutomationCreditService : IAutomationCreditService
{
    private readonly AisamContext _context;
    private readonly ICreditService _credits;
    public AutomationCreditService(AisamContext context, ICreditService credits) { _context = context; _credits = credits; }

    public async Task<GenericResponse<bool>> ReserveAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.AutomationPlans.Include(value => value.Items).FirstOrDefaultAsync(value => value.Id == planId, cancellationToken);
        if (plan is null) return GenericResponse<bool>.CreateError("Automation plan not found.", HttpStatusCode.NotFound);
        if (plan.ReservedCredits > 0) return GenericResponse<bool>.CreateSuccess(true);
        var amount = Math.Max(0, plan.EstimatedCredits - plan.UsedCredits);
        if (amount == 0) return GenericResponse<bool>.CreateSuccess(true);
        var userId = await _context.Profiles.Where(value => value.Id == plan.ProfileId).Select(value => value.UserId).FirstAsync(cancellationToken);
        var check = await _credits.EnsureCreditsAvailableAsync(plan.WorkspaceId, userId, amount, cancellationToken: cancellationToken);
        if (!check.Success) return GenericResponse<bool>.CreateError(check.Message ?? "Insufficient credits.", (HttpStatusCode)check.StatusCode, check.Error?.ErrorCode);
        var wallet = await _context.CreditWallets.FirstAsync(value => value.WorkspaceId == plan.WorkspaceId, cancellationToken);
        wallet.ReservedBalance += amount;
        plan.ReservedCredits = amount;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true);
    }

    public async Task<GenericResponse<bool>> SettleAsync(Guid itemId, Guid userId, CreditActionEnum action, int amount, int expectedItemUsedCredits, CancellationToken cancellationToken = default)
    {
        var item = await _context.AutomationItems.Include(value => value.AutomationPlan).FirstAsync(value => value.Id == itemId, cancellationToken);
        await _context.Entry(item.AutomationPlan).ReloadAsync(cancellationToken);
        if (item.AutomationPlan.Status == AutomationPlanStatusEnum.Cancelled)
            return GenericResponse<bool>.CreateError("Automation plan was cancelled; credits were not charged.");
        if (item.UsedCredits >= expectedItemUsedCredits) return GenericResponse<bool>.CreateSuccess(true);
        var wallet = await _context.CreditWallets.FirstAsync(value => value.WorkspaceId == item.AutomationPlan.WorkspaceId, cancellationToken);
        var released = Math.Min(amount, item.AutomationPlan.ReservedCredits);
        wallet.ReservedBalance = Math.Max(0, wallet.ReservedBalance - released);
        item.AutomationPlan.ReservedCredits -= released;
        var charge = await _credits.ConsumeCreditsAsync(item.AutomationPlan.WorkspaceId, userId, action, amount, cancellationToken: cancellationToken);
        if (!charge.Success)
        {
            wallet.ReservedBalance += released; item.AutomationPlan.ReservedCredits += released;
            return GenericResponse<bool>.CreateError(charge.Message ?? "Unable to settle credits.", (HttpStatusCode)charge.StatusCode, charge.Error?.ErrorCode);
        }
        var delta = expectedItemUsedCredits - item.UsedCredits;
        item.UsedCredits = expectedItemUsedCredits;
        item.AutomationPlan.UsedCredits += delta;
        await _context.SaveChangesAsync(cancellationToken);
        return GenericResponse<bool>.CreateSuccess(true);
    }

    public async Task ReleaseAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.AutomationPlans.FirstOrDefaultAsync(value => value.Id == planId, cancellationToken);
        if (plan is null || plan.ReservedCredits <= 0) return;
        var wallet = await _context.CreditWallets.FirstAsync(value => value.WorkspaceId == plan.WorkspaceId, cancellationToken);
        var amount = plan.ReservedCredits;
        wallet.ReservedBalance = Math.Max(0, wallet.ReservedBalance - amount);
        plan.ReleasedCredits += amount; plan.ReservedCredits = 0;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
