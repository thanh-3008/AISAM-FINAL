using System.Net;
using System.Text.Json;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public sealed class AutomationReadAuthorizationSecurityTests
{
    [Fact]
    public async Task OwnerReadsAllPlansItemsAndWholePlanCredits()
    {
        await using var fixture = await ReadFixture.CreateAsync();
        await fixture.Security.Resolve(WorkspaceMemberRoleEnum.Owner);

        var list = await fixture.Service.GetAllAsync(fixture.Security.Workspace.Id);
        var detail = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.MixedPlan.Id);

        Assert.Equal(2, list.Data!.Count);
        Assert.Equal(4, detail.Data!.Items.Count);
        Assert.Equal(4, detail.Data.TotalItems);
        Assert.Equal(1000, detail.Data.EstimatedCredits);
        Assert.Equal(888, detail.Data.UsedCredits);
        Assert.Equal(777, detail.Data.ReservedCredits);
        Assert.Equal(666, detail.Data.ReleasedCredits);
    }

    [Fact]
    public async Task ManagerReadsOnlyAnalyticsVisibleItemsAndScopedAggregates()
    {
        await using var fixture = await ReadFixture.CreateAsync();
        await fixture.Security.Resolve(WorkspaceMemberRoleEnum.Manager);

        var list = await fixture.Service.GetAllAsync(fixture.Security.Workspace.Id);
        var detail = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.MixedPlan.Id);
        var hidden = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.HiddenPlan.Id);

        var plan = Assert.Single(list.Data!);
        Assert.Equal(fixture.MixedPlan.Id, plan.Id);
        Assert.Equal(new[] { "manager-and-creator", "manager-only" }, plan.Items.Select(item => item.Topic).Order());
        Assert.Equal(2, plan.TotalItems);
        Assert.Equal(2, plan.ValidItems);
        Assert.Equal(0, plan.FailedItems);
        Assert.Equal(44, plan.EstimatedCredits);
        Assert.Equal(4, plan.UsedCredits);
        Assert.Null(plan.ReservedCredits);
        Assert.Null(plan.ReleasedCredits);
        Assert.Equal(2, detail.Data!.Items.Count);
        Assert.Equal((int)HttpStatusCode.NotFound, hidden.StatusCode);
        AssertWholePlanCreditsOmitted(plan);
    }

    [Fact]
    public async Task CreatorReadsOnlyItemsLinkedToOwnPrimaryCreatorContent()
    {
        await using var fixture = await ReadFixture.CreateAsync();
        await fixture.Security.Resolve(WorkspaceMemberRoleEnum.ContentCreator);

        var list = await fixture.Service.GetAllAsync(fixture.Security.Workspace.Id);
        var detail = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.MixedPlan.Id);
        var hidden = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.HiddenPlan.Id);

        var plan = Assert.Single(list.Data!);
        Assert.Equal(new[] { "creator-only-private-brand", "manager-and-creator" }, plan.Items.Select(item => item.Topic).Order());
        Assert.Equal(2, plan.TotalItems);
        Assert.Equal(2, plan.ValidItems);
        Assert.Equal(33, plan.EstimatedCredits);
        Assert.Equal(3, plan.UsedCredits);
        Assert.Null(plan.ReservedCredits);
        Assert.Null(plan.ReleasedCredits);
        Assert.Equal(2, detail.Data!.Items.Count);
        Assert.Equal((int)HttpStatusCode.NotFound, hidden.StatusCode);
        AssertWholePlanCreditsOmitted(plan);
    }

    [Fact]
    public async Task ViewerReadsNoAutomationPlansOrDetails()
    {
        await using var fixture = await ReadFixture.CreateAsync();
        await fixture.Security.Resolve(WorkspaceMemberRoleEnum.Viewer);

        var list = await fixture.Service.GetAllAsync(fixture.Security.Workspace.Id);
        var detail = await fixture.Service.GetByIdAsync(fixture.Security.Workspace.Id, fixture.MixedPlan.Id);

        Assert.Empty(list.Data!);
        Assert.Equal((int)HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Null(detail.Data);
    }

    private static void AssertWholePlanCreditsOmitted(object value)
    {
        var json = JsonSerializer.Serialize(value, JsonSerializerOptions.Web);
        Assert.DoesNotContain("reservedCredits", json);
        Assert.DoesNotContain("releasedCredits", json);
    }

    private sealed class ReadFixture : IAsyncDisposable
    {
        public PermissionSecurityTests.Fixture Security { get; private init; } = null!;
        public AutomationPlan MixedPlan { get; private init; } = null!;
        public AutomationPlan HiddenPlan { get; private init; } = null!;
        public AutomationService Service => new(
            new AutomationRepository(Security.Db),
            null!,
            null!,
            null!,
            accessScope: Security.Db.AccessScope);

        public static async Task<ReadFixture> CreateAsync()
        {
            var security = await PermissionSecurityTests.Fixture.CreateAsync();
            var privateBrand = new Brand
            {
                WorkspaceId = security.Workspace.Id,
                ProfileId = security.Profile.Id,
                Name = "Creator private Brand"
            };
            var privateContent = new Content
            {
                WorkspaceId = security.Workspace.Id,
                ProfileId = security.Profile.Id,
                BrandId = privateBrand.Id,
                PrimaryCreatorId = security.Creator.Id,
                TextContent = "Private creator content"
            };
            security.Db.AddRange(privateBrand, privateContent);

            var mixed = NewPlan(security, "Mixed", totalItems: 4);
            mixed.Items.Add(NewItem(mixed, security, security.OwnContent, security.AllowedChannel, 1, "manager-and-creator", 11, 1));
            mixed.Items.Add(NewItem(mixed, security, privateContent, security.DeniedChannel, 2, "creator-only-private-brand", 22, 2));
            mixed.Items.Add(NewItem(mixed, security, security.OtherContent, security.AllowedChannel, 3, "manager-only", 33, 3));
            mixed.Items.Add(NewItem(mixed, security, security.OtherContent, security.DeniedChannel, 4, "neither", 44, 4));

            var hidden = NewPlan(security, "Hidden", totalItems: 1);
            hidden.Items.Add(NewItem(hidden, security, security.OtherContent, security.DeniedChannel, 1, "hidden", 55, 5, createCalendar: false));
            security.Db.AutomationPlans.AddRange(mixed, hidden);
            security.Db.SaveChanges();

            return new ReadFixture { Security = security, MixedPlan = mixed, HiddenPlan = hidden };
        }

        private static AutomationPlan NewPlan(PermissionSecurityTests.Fixture security, string name, int totalItems)
            => new()
            {
                WorkspaceId = security.Workspace.Id,
                ProfileId = security.Profile.Id,
                Name = name,
                TotalItems = totalItems,
                ValidItems = totalItems,
                EstimatedCredits = 1000,
                ReservedCredits = 777,
                UsedCredits = 888,
                ReleasedCredits = 666
            };

        private static AutomationItem NewItem(
            AutomationPlan plan,
            PermissionSecurityTests.Fixture security,
            Content content,
            SocialIntegration integration,
            int row,
            string topic,
            int estimated,
            int used,
            bool createCalendar = true)
        {
            ContentCalendar? calendar = null;
            if (createCalendar)
            {
                calendar = new ContentCalendar
                {
                    WorkspaceId = security.Workspace.Id,
                    ProfileId = security.Profile.Id,
                    ContentId = content.Id,
                    IntegrationId = integration.Id,
                    ScheduledDate = DateTime.UtcNow.AddDays(row)
                };
                security.Db.ContentCalendars.Add(calendar);
            }
            return new AutomationItem
            {
                AutomationPlanId = plan.Id,
                RowIndex = row,
                Platform = integration.Platform.ToString(),
                IdempotencyKey = Guid.NewGuid().ToString("N"),
                BrandId = content.BrandId,
                ContentId = content.Id,
                ContentCalendarId = calendar?.Id,
                Topic = topic,
                Status = AutomationItemStatusEnum.Scheduled,
                EstimatedCredits = estimated,
                UsedCredits = used,
                ScheduledAt = calendar?.ScheduledDate ?? DateTime.UtcNow.AddDays(row)
            };
        }

        public ValueTask DisposeAsync() => Security.DisposeAsync();
    }
}
