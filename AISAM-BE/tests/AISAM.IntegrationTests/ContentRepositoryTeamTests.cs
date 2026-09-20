using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public sealed class ContentRepositoryTeamTests
{
    [Fact]
    public async Task ContentQueriesReturnAssignedTeamIdentity()
    {
        await using var context = new AisamContext(
            new DbContextOptionsBuilder<AisamContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options);

        var workspace = new Workspace { Name = "Demo workspace" };
        var profile = new Profile { Name = "Demo profile" };
        var brand = new Brand { Name = "Shared brand", WorkspaceId = workspace.Id, ProfileId = profile.Id };
        var team = new Team { Name = "Growth Team", WorkspaceId = workspace.Id };
        var content = new Content
        {
            WorkspaceId = workspace.Id,
            ProfileId = profile.Id,
            BrandId = brand.Id,
            TeamId = team.Id,
            Title = "Campaign draft",
            TextContent = "Draft"
        };

        context.AddRange(workspace, profile, brand, team, content);
        await context.SaveChangesAsync();

        var repository = new ContentRepository(context);
        var page = await repository.GetPagedByWorkspaceIdAsync(
            workspace.Id,
            new PaginationRequest { Page = 1, PageSize = 20 });
        var item = Assert.Single(page.Data);

        Assert.Equal(team.Id, item.TeamId);
        Assert.Equal("Growth Team", item.TeamName);

        var detail = await repository.GetByIdAsync(content.Id);
        Assert.NotNull(detail);
        Assert.Equal("Growth Team", detail.TeamName);
    }
}
