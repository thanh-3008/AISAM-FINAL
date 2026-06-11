using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class CreditWalletRepositoryTests
{
    [Fact]
    public void Model_ConfiguresUniqueWalletPerWorkspace()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(CreditWallet));
        var workspaceIndex = Assert.Single(entityType!.GetIndexes().Where(index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(CreditWallet.WorkspaceId)));

        Assert.True(workspaceIndex.IsUnique);
    }

    [Fact]
    public async Task Repository_ReusesExistingWalletForWorkspace()
    {
        await using var context = CreateContext();
        var repository = new CreditWalletRepository(context);
        var workspaceId = Guid.NewGuid();
        context.Workspaces.Add(new AISAM.Data.Model.Workspace
        {
            Id = workspaceId,
            Name = "Wallet Workspace",
            WorkspaceType = AISAM.Data.Enumeration.WorkspaceTypeEnum.Business
        });
        await context.SaveChangesAsync();

        var created = await repository.AddAsync(new CreditWallet
        {
            WorkspaceId = workspaceId,
            Balance = 25
        });
        var fetched = await repository.GetByWorkspaceIdAsync(workspaceId);

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(25, fetched.Balance);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }
}
