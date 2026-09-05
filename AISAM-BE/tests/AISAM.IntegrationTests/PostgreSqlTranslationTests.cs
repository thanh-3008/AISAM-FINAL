using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

/// <summary>
/// Static SQL-translation checks for C-01 and C-02 flagged query patterns.
///
/// WHAT THIS PROVES:
/// Each query compiles to valid PostgreSQL SQL via Npgsql.EntityFrameworkCore.PostgreSQL
/// without requiring a live database connection.  ToQueryString() uses the Npgsql SQL
/// generator to produce the actual SQL text that would be sent on the wire.
///
/// WHAT THIS DOES NOT PROVE:
/// - Actual query results against real data
/// - Index usage / query performance
/// - Concurrency / locking behavior (H-05 is NOT covered)
/// - Any runtime behavior that depends on data distribution or PostgreSQL settings
/// </summary>
public sealed class PostgreSqlTranslationTests : IDisposable
{
    private readonly AisamContext _context;

    public PostgreSqlTranslationTests()
    {
        // Connection string intentionally points at a non-existent host.
        // ToQueryString() compiles SQL without opening a connection.
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=aisam_translation_check;Username=test;Password=test")
            .Options;

        var accessScope = new AccessScope
        {
            Enforced = true,
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = WorkspaceMemberRoleEnum.Owner,
            BrandIds = new[] { Guid.NewGuid(), Guid.NewGuid() },
            MemberIds = new[] { Guid.NewGuid() },
        };

        _context = new AisamContext(options, accessScope);
    }

    public void Dispose() => _context.Dispose();

    // ─── C-01: Nullable Brand/Product Contains ────────────────────

    [Fact]
    public void C01_NullableBrandContains_TranslatesToPostgreSql()
    {
        var brandIds = _context.AccessScope.BrandIds;
        var workspaceId = _context.AccessScope.WorkspaceId;

        // This mirrors ConversationRepository.QueryForWorkspaceAccess: brand scope filter
        var query = _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId)
            .Where(c =>
                c.BrandId.HasValue &&
                brandIds.Contains(c.BrandId.Value) &&
                c.Brand != null &&
                c.Brand.WorkspaceId == workspaceId &&
                !c.Brand.IsDeleted &&
                (!c.ProductId.HasValue ||
                    c.Product != null &&
                    !c.Product.IsDeleted &&
                    c.Product.BrandId == c.BrandId));

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("SELECT", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C01_NullableBrandContains_TranslatesToPostgreSql), sql);
    }

    // ─── C-01: Nested ChatMessages.Any(...) ───────────────────────

    [Fact]
    public void C01_NestedChatMessagesAny_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        var userId = _context.AccessScope.UserId;

        // This mirrors ContentCreator access in ConversationRepository
        var query = _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId)
            .Where(c =>
                c.Profile.UserId == userId ||
                c.ChatMessages.Any(m =>
                    !m.IsDeleted &&
                    m.Content != null &&
                    m.Content.WorkspaceId == workspaceId &&
                    m.Content.PrimaryCreatorId == userId));

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        // The nested Any() should become an EXISTS subquery
        Assert.Contains("EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C01_NestedChatMessagesAny_TranslatesToPostgreSql), sql);
    }

    // ─── C-01: Creator-linked content lookup ──────────────────────

    [Fact]
    public void C01_CreatorLinkedContentLookup_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        var userId = _context.AccessScope.UserId;

        // Content query scoped to creator 
        var query = _context.Contents
            .Where(c => c.WorkspaceId == workspaceId && !c.IsDeleted)
            .Where(c => c.PrimaryCreatorId == userId);

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        OutputSql(nameof(C01_CreatorLinkedContentLookup_TranslatesToPostgreSql), sql);
    }

    // ─── C-01: Manager/Viewer null-Brand exclusion ─────────────────

    [Fact]
    public void C01_ManagerViewerNullBrandExclusion_TranslatesToPostgreSql()
    {
        var brandIds = _context.AccessScope.BrandIds;
        var workspaceId = _context.AccessScope.WorkspaceId;
        var memberIds = _context.AccessScope.MemberIds;

        // Manager scope: must see only conversations from managed members within allowed brands
        var query = _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId)
            .Where(c =>
                c.BrandId.HasValue &&
                brandIds.Contains(c.BrandId.Value) &&
                c.Brand != null &&
                c.Brand.WorkspaceId == workspaceId &&
                !c.Brand.IsDeleted &&
                (!c.ProductId.HasValue ||
                    c.Product != null &&
                    !c.Product.IsDeleted &&
                    c.Product.BrandId == c.BrandId))
            .Where(c => memberIds.Contains(c.Profile.UserId));

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        OutputSql(nameof(C01_ManagerViewerNullBrandExclusion_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: Correlated Any ─────────────────────────────────────

    [Fact]
    public void C02_CorrelatedAnyContentCreator_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        var userId = _context.AccessScope.UserId;

        // Content where user is creator or has related generation
        var query = _context.Contents
            .Where(c => c.WorkspaceId == workspaceId && !c.IsDeleted)
            .Where(c => c.PrimaryCreatorId == userId ||
                        c.AiGenerations.Any(g => !g.IsDeleted && g.Content.WorkspaceId == workspaceId));

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_CorrelatedAnyContentCreator_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: Content/Calendar joins ─────────────────────────────

    [Fact]
    public void C02_ContentCalendarJoins_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;

        var query = _context.ContentCalendars
            .Where(cc => cc.WorkspaceId == workspaceId)
            .Include(cc => cc.Content)
                .ThenInclude(c => c.Brand)
            .Include(cc => cc.Integration);

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("SELECT", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_ContentCalendarJoins_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: Scoped aggregate projections ───────────────────────

    [Fact]
    public void C02_ScopedAggregateProjections_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;

        // Aggregate: content count by ad type
        var query = _context.Contents
            .Where(c => c.WorkspaceId == workspaceId && !c.IsDeleted)
            .GroupBy(c => c.AdType)
            .Select(g => new { AdType = g.Key, Count = g.Count() });

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("GROUP BY", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_ScopedAggregateProjections_TranslatesToPostgreSql), sql);
    }

    // ─── C-01: GetActiveByWorkspaceId nullable equality ───────────

    [Fact]
    public void C01_GetActiveByWorkspace_NullableEquality_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        Guid? brandId = Guid.NewGuid();
        Guid? productId = null;

        // Mirrors ConversationRepository.GetActiveByWorkspaceIdAsync
        var query = _context.Conversations
            .Where(c => c.WorkspaceId == workspaceId)
            .Where(c =>
                c.BrandId == brandId &&
                c.ProductId == productId &&
                c.AdType == AdTypeEnum.TextOnly &&
                c.IsActive &&
                !c.IsDeleted);

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        // When productId is null, EF should translate to IS NULL check
        Assert.Contains("IS NULL", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C01_GetActiveByWorkspace_NullableEquality_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: Content paged with ILike search (EF.Functions.ILike) ──

    [Fact]
    public void C02_ContentSearchWithILike_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        var pattern = "%test%";

        var query = _context.Contents
            .AsNoTracking()
            .Where(c => c.WorkspaceId == workspaceId && !c.IsDeleted)
            .Where(c => c.Title != null && EF.Functions.ILike(c.Title, pattern));

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("ILIKE", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_ContentSearchWithILike_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: VideoJobIds Contains (ids.Contains) ────────────────

    [Fact]
    public void C02_VideoJobIdsContains_TranslatesToPostgreSql()
    {
        var workspaceId = _context.AccessScope.WorkspaceId;
        var jobIds = new List<string> { "job1", "job2", "job3" };

        // Mirrors ConversationRepository.GetGenerationsByVideoJobIdsAsync
        var query = _context.AiGenerations
            .Where(g => !g.IsDeleted &&
                        g.Content.WorkspaceId == workspaceId &&
                        g.VideoJobId != null &&
                        jobIds.Contains(g.VideoJobId))
            .OrderByDescending(g => g.CreatedAt);

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        // Contains should become IN (...)
        Assert.Contains("IN (", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_VideoJobIdsContains_TranslatesToPostgreSql), sql);
    }

    // ─── C-02: Daily created aggregate (GroupBy date) ─────────────

    [Fact]
    public void C02_DailyCreatedAggregate_TranslatesToPostgreSql()
    {
        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow;

        var query = _context.Contents
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to && !c.IsDeleted)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() });

        var sql = query.ToQueryString();

        Assert.NotNull(sql);
        Assert.NotEmpty(sql);
        Assert.Contains("GROUP BY", sql, StringComparison.OrdinalIgnoreCase);
        OutputSql(nameof(C02_DailyCreatedAggregate_TranslatesToPostgreSql), sql);
    }

    // ─── Helper ───────────────────────────────────────────────────

    private static void OutputSql(string testName, string sql)
    {
        // XUnit output: visible in test runner output
        Console.WriteLine($"\n=== {testName} ===");
        Console.WriteLine(sql);
        Console.WriteLine("=== END ===\n");
    }
}
