using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public class AutomationCreatorTests
{
    [Fact]
    public void MultipleDestinationsRemainPendingUntilEveryOutcomeIsKnown()
    {
        var item = new AutomationItem { AutomationPlan = new() { AutoApprove = true }, Status = AutomationItemStatusEnum.Scheduled };
        ContentCalendar[] destinations = [new() { Status = ScheduleStatusEnum.Completed }, new() { Status = ScheduleStatusEnum.Processing }];
        AutomationOperationsBackgroundService.ApplyPublicationResults(item, destinations);
        Assert.Equal(AutomationItemStatusEnum.Scheduled, item.Status);
        destinations[1].Status = ScheduleStatusEnum.Failed;
        destinations[1].LastError = "SOCIAL_REAUTH_REQUIRED";
        AutomationOperationsBackgroundService.ApplyPublicationResults(item, destinations);
        Assert.Equal(AutomationItemStatusEnum.PublishFailed, item.Status);
        Assert.False(item.AutomationPlan.AutoApprove);
        Assert.Equal("SOCIAL_REAUTH_REQUIRED", item.LastError);
    }

    private sealed class DeniedAccess : AISAM.Services.Access.IAccessControlService
    {
        public Task<AISAM.Services.Access.AccessDecision> CheckAsync(AISAM.Services.Access.AccessRequest request, CancellationToken ct = default)
            => Task.FromResult(new AISAM.Services.Access.AccessDecision(false, 403, "ACCESS_DENIED_CHANNEL"));
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid actor, Guid workspace, CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    [Fact]
    public async Task RevokedBrandPausesGenerationWithoutCallingProviders()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var user = new User { Email = "creator@example.test", IsActive = true };
        var profile = new Profile { UserId = user.Id };
        var workspace = Guid.NewGuid();
        var brand = new Brand { WorkspaceId = workspace, ProfileId = profile.Id };
        var plan = new AutomationPlan { WorkspaceId = workspace, ProfileId = profile.Id, CreatedByUserId = user.Id,
            AutoApprove = true, Status = AutomationPlanStatusEnum.Generating };
        var item = new AutomationItem { AutomationPlan = plan, Brand = brand, BrandId = brand.Id, Platform = "facebook", Status = AutomationItemStatusEnum.Pending };
        db.AddRange(user, profile, brand, plan, item, new WorkspaceMember { WorkspaceId = workspace, UserId = user.Id, IsActive = true });
        await db.SaveChangesAsync();
        var service = new AutomationGenerationService(db, null!, null!, null!, null!, new Credits(),
            Options.Create(new ImageProviderSettings()), Options.Create(new VideoProviderSettings()),
            NullLogger<AutomationGenerationService>.Instance, new DeniedAccess());
        await service.ProcessNextAsync();
        Assert.Equal(AutomationItemStatusEnum.NeedsAttention, item.Status);
        Assert.Equal(AutomationPlanStatusEnum.PartiallyFailed, plan.Status);
        Assert.False(plan.AutoApprove);
        Assert.Empty(await db.Contents.ToListAsync());
        Assert.Single(await db.Notifications.ToListAsync());
    }

    [Fact]
    public async Task RevokedPublishPermissionPreventsAutoApprovalAndScheduling()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var workspace = Guid.NewGuid();
        var brand = new Brand { WorkspaceId = workspace };
        var content = new Content { WorkspaceId = workspace, BrandId = brand.Id, Status = ContentStatusEnum.Draft };
        var plan = new AutomationPlan { WorkspaceId = workspace, AutoApprove = true, Status = AutomationPlanStatusEnum.AwaitingApproval };
        var item = new AutomationItem { AutomationPlan = plan, Brand = brand, BrandId = brand.Id, Content = content, ContentId = content.Id,
            Platform = "facebook", Status = AutomationItemStatusEnum.AwaitingApproval };
        db.AddRange(brand, content, plan, item, new SocialIntegration { WorkspaceId = workspace, BrandId = brand.Id,
            Platform = SocialPlatformEnum.Facebook, IsActive = true });
        await db.SaveChangesAsync();
        await new AutomationApprovalService(db, null!, new DeniedAccess()).ApproveAsync(workspace, plan.Id, Guid.NewGuid());
        Assert.Equal(AutomationItemStatusEnum.NeedsAttention, item.Status);
        Assert.False(plan.AutoApprove);
        Assert.Equal(ContentStatusEnum.Draft, content.Status);
        Assert.Empty(await db.Approvals.ToListAsync());
        Assert.Empty(await db.ContentCalendars.ToListAsync());
        Assert.Single(await db.Notifications.ToListAsync());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task UnknownOrRevokedCreatorStopsBeforeProviders(bool knownCreator, bool activeMembership)
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var user = new User { Email = "automation@example.test" };
        var workspace = Guid.NewGuid();
        var plan = new AutomationPlan { WorkspaceId = workspace,
            CreatedByUserId = knownCreator ? user.Id : null, Status = AutomationPlanStatusEnum.Generating };
        var item = new AutomationItem { AutomationPlanId = plan.Id, AutomationPlan = plan,
            Status = AutomationItemStatusEnum.Pending, Platform = "facebook" };
        db.AddRange(user, plan, item, new WorkspaceMember { WorkspaceId = workspace,
            UserId = user.Id, IsActive = activeMembership });
        await db.SaveChangesAsync();
        var credits = new Credits();
        // Null providers make accidental downstream execution fail this test.
        var service = new AutomationGenerationService(db, null!, null!, null!, null!, credits,
            Options.Create(new ImageProviderSettings()), Options.Create(new VideoProviderSettings()),
            NullLogger<AutomationGenerationService>.Instance);
        await service.ProcessNextAsync();
        Assert.Equal(AutomationItemStatusEnum.NeedsAttention, item.Status);
        Assert.Equal("AUTOMATION_ACCESS_REVOKED", item.LastError!);
        Assert.Empty(await db.Contents.ToListAsync());
        Assert.Equal(0, credits.Releases);
    }

    private sealed class Credits : IAutomationCreditService
    {
        public int Releases { get; private set; }
        public Task ReleaseAsync(Guid planId, CancellationToken cancellationToken = default)
        { Releases++; return Task.CompletedTask; }
        public Task<GenericResponse<bool>> ReserveAsync(Guid planId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Must not reserve credits.");
        public Task<GenericResponse<bool>> SettleAsync(Guid itemId, Guid userId, CreditActionEnum action, int amount,
            int expectedItemUsedCredits, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Must not charge credits.");
    }
}
