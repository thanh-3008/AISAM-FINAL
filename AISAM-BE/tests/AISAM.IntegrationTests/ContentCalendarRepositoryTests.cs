using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class ContentCalendarRepositoryTests
{
    [Fact]
    public async Task GetDueSchedulesAsync_ReturnsOnlyPendingSchedulesWhoseScheduledAtIsPast()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new ContentCalendarRepository(context);

        var result = await repository.GetDueSchedulesAsync(DateTime.UtcNow, 10);

        var schedule = Assert.Single(result);
        Assert.Equal(fixture.DueSchedule.Id, schedule.Id);
    }

    [Fact]
    public async Task GetUpcomingByProfileIdAsync_SortsAscendingAndSkipsDeletedSchedules()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new ContentCalendarRepository(context);

        var result = await repository.GetUpcomingByProfileIdAsync(fixture.Profile.Id, 10);

        Assert.Equal(2, result.Count);
        Assert.Equal(fixture.UpcomingSoon.Id, result[0].Id);
        Assert.Equal(fixture.UpcomingLater.Id, result[1].Id);
    }

    [Fact]
    public async Task AddAsync_DetachesSchedule_WhenSaveFails()
    {
        var interceptor = new FailFirstSaveInterceptor();
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .AddInterceptors(interceptor)
            .Options;
        await using var context = new AisamContext(options);
        var repository = new ContentCalendarRepository(context);
        var failedSchedule = new ContentCalendar
        {
            ContentId = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddHours(1)
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(failedSchedule));

        Assert.Equal(EntityState.Detached, context.Entry(failedSchedule).State);

        var laterSchedule = new ContentCalendar
        {
            ContentId = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddHours(2)
        };
        await repository.AddAsync(laterSchedule);
        Assert.Equal(1, await context.ContentCalendars.CountAsync());
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static ContentCalendarRepositoryFixture SeedFixture(AisamContext context)
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            Name = "Owner",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Workspace",
            WorkspaceType = WorkspaceTypeEnum.Personal
        };
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Name = "Brand"
        };
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            Brand = brand,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Scheduled content"
        };
        var due = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            ScheduledAt = DateTime.UtcNow.AddMinutes(-5),
            IntegrationId = Guid.NewGuid(),
            Status = ScheduleStatusEnum.Pending
        };
        var upcomingSoon = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            ScheduledAt = DateTime.UtcNow.AddMinutes(10),
            IntegrationId = Guid.NewGuid(),
            Status = ScheduleStatusEnum.Pending
        };
        var upcomingLater = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            ScheduledAt = DateTime.UtcNow.AddHours(1),
            IntegrationId = Guid.NewGuid(),
            Status = ScheduleStatusEnum.Pending
        };
        var completed = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            ScheduledAt = DateTime.UtcNow.AddMinutes(-15),
            IntegrationId = Guid.NewGuid(),
            Status = ScheduleStatusEnum.Completed
        };
        var deleted = new ContentCalendar
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            ProfileId = profile.Id,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            ScheduledAt = DateTime.UtcNow.AddMinutes(20),
            IntegrationId = Guid.NewGuid(),
            Status = ScheduleStatusEnum.Pending,
            IsDeleted = true
        };

        context.Users.Add(owner);
        context.Profiles.Add(profile);
        context.Workspaces.Add(workspace);
        context.Brands.Add(brand);
        context.Contents.Add(content);
        context.ContentCalendars.AddRange(due, upcomingSoon, upcomingLater, completed, deleted);
        context.SaveChanges();

        return new ContentCalendarRepositoryFixture(profile, due, upcomingSoon, upcomingLater);
    }

    private sealed record ContentCalendarRepositoryFixture(
        Profile Profile,
        ContentCalendar DueSchedule,
        ContentCalendar UpcomingSoon,
        ContentCalendar UpcomingLater);

    private sealed class FailFirstSaveInterceptor : SaveChangesInterceptor
    {
        private bool _shouldFail = true;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (_shouldFail)
            {
                _shouldFail = false;
                throw new DbUpdateException("Simulated insert failure.");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}




