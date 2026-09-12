using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class AutomationExecutionLockTests
{
    [Fact]
    public async Task TryAcquireAsync_AcquiresAndReleasesLockWithoutPasswordException()
    {
        await using var db = new AisamContextFactory().CreateDbContext([]);
        const long testLockKey = 0x414953414D47454E; // AISAMGEN

        var lockObj = await AutomationExecutionLock.TryAcquireAsync(db, testLockKey, default);
        Assert.NotNull(lockObj);

        // Verify second attempt on same lock fails while held
        var contender = await AutomationExecutionLock.TryAcquireAsync(db, testLockKey, default);
        Assert.Null(contender);

        // Release first lock
        await lockObj.DisposeAsync();

        // Verify lock can be re-acquired after release
        await using var recovered = await AutomationExecutionLock.TryAcquireAsync(db, testLockKey, default);
        Assert.NotNull(recovered);
    }
}
