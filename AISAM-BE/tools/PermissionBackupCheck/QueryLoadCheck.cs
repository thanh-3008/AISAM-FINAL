using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using AISAM.Repositories;
using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;
using Npgsql;

internal static class QueryLoadCheck
{
    public static async Task RunAsync(NpgsqlDataSource source, Guid actor, Guid workspace, string output)
    {
        var connection = new NpgsqlConnectionStringBuilder(source.ConnectionString);
        if (connection.Host != "127.0.0.1" || connection.Port != 55439 ||
            !connection.Database!.StartsWith("permission_restore_", StringComparison.Ordinal))
            throw new InvalidOperationException("Load check requires the isolated restore database.");

        const int workers = 20, iterations = 10, pageSize = 20;
        var creators = Enumerable.Range(0, 4).Select(i => new User { Email = $"t11-{Guid.NewGuid():N}@example.test" }).ToArray();
        await using (var seed = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(source).Options))
        {
            await seed.Database.OpenConnectionAsync();
            var profile = await seed.Brands.Where(b => b.WorkspaceId == workspace && !b.IsDeleted).Select(b => b.ProfileId).FirstAsync();
            var brands = Enumerable.Range(0, 2).Select(i => new Brand { WorkspaceId = workspace, ProfileId = profile, Name = $"T11 load brand {i}" }).ToArray();
            seed.Brands.AddRange(brands);
            seed.Users.AddRange(creators);
            var teams = Enumerable.Range(0, 2).Select(i => new Team { WorkspaceId = workspace, Name = $"T11 load team {i}" }).ToArray();
            seed.Teams.AddRange(teams);
            for (var i = 0; i < creators.Length; i++)
            {
                seed.WorkspaceMembers.Add(new WorkspaceMember { WorkspaceId = workspace, UserId = creators[i].Id, IsActive = true,
                    Role = i < 2 ? WorkspaceMemberRoleEnum.ContentCreator : i == 2 ? WorkspaceMemberRoleEnum.Manager : WorkspaceMemberRoleEnum.Viewer });
            }
            await seed.SaveChangesAsync();
            for (var i = 0; i < creators.Length; i++)
                seed.TeamMembers.Add(new TeamMember { TeamId = teams[i == 1 ? 1 : 0].Id, UserId = creators[i].Id, IsActive = true });
            for (var i = 0; i < 2; i++) seed.TeamBrands.Add(new TeamBrand { TeamId = teams[i].Id, BrandId = brands[i].Id });
            await seed.SaveChangesAsync();
            for (var batch = 0; batch < 20; batch++)
            {
                seed.Contents.AddRange(Enumerable.Range(0, 500).Select(i => new Content
                {
                    WorkspaceId = workspace, ProfileId = profile, BrandId = brands[i % 2].Id,
                    PrimaryCreatorId = creators[i % 2].Id, TeamId = teams[i % 2].Id, Title = $"T11 load {batch * 500 + i}", TextContent = "Synthetic load fixture"
                }));
                await seed.SaveChangesAsync();
                seed.ChangeTracker.Clear();
            }
            var account = new SocialAccount { WorkspaceId = workspace, ProfileId = profile, Platform = SocialPlatformEnum.Facebook,
                AccountId = $"t11-{Guid.NewGuid():N}", IsActive = false };
            seed.SocialAccounts.Add(account);
            await seed.SaveChangesAsync();
            for (var creatorIndex = 0; creatorIndex < 2; creatorIndex++)
            {
                var channel = new SocialIntegration { WorkspaceId = workspace, ProfileId = profile, BrandId = brands[creatorIndex].Id,
                    SocialAccountId = account.Id, Platform = SocialPlatformEnum.Facebook, IsActive = false };
                seed.SocialIntegrations.Add(channel);
                await seed.SaveChangesAsync();
                var creatorId = creators[creatorIndex].Id;
                var contentIds = await seed.Contents.Where(c => c.PrimaryCreatorId == creatorId).OrderBy(c => c.Id).Take(100).Select(c => c.Id).ToArrayAsync();
                var posts = contentIds.Select((id, i) => new Post { ContentId = id, IntegrationId = channel.Id,
                    ExternalPostId = $"synthetic-{i}", PublishedAt = DateTime.UtcNow, Status = ContentStatusEnum.Published }).ToArray();
                seed.Posts.AddRange(posts);
                await seed.SaveChangesAsync();
                foreach (var post in posts)
                {
                    seed.PerformanceReports.Add(new PerformanceReport { PostId = post.Id, ReportDate = DateTime.UtcNow.Date.AddDays(-1),
                        Engagement = 1, Impressions = 100, Reach = 80 });
                    seed.PerformanceReports.Add(new PerformanceReport { PostId = post.Id, ReportDate = DateTime.UtcNow.Date,
                        Engagement = 7, Impressions = 1000, Reach = 800 });
                }
                await seed.SaveChangesAsync();
                seed.ChangeTracker.Clear();
            }
        }
        var samples = new ConcurrentBag<double>();
        var counts = new ConcurrentBag<int>();
        var wall = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0, workers).Select(async _ =>
        {
            await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(source).Options);
            await db.Database.OpenConnectionAsync();
            var access = new AccessControlService(db);
            var brands = (await access.GetAccessibleBrandIdsAsync(actor, workspace)).ToArray();
            db.PermissionWorkspaceId = workspace;
            db.PermissionActorId = actor;
            db.PermissionOwner = true;
            db.PermissionBrandIds = brands;
            db.PermissionScopeEnabled = true;
            for (var i = 0; i < iterations; i++)
            {
                var timer = Stopwatch.StartNew();
                var total = await db.Contents.CountAsync();
                var rows = await db.Contents.AsNoTracking().OrderBy(c => c.Id)
                    .Skip((i % Math.Max(1, (total + pageSize - 1) / pageSize)) * pageSize)
                    .Take(pageSize).Select(c => new { c.Id, c.WorkspaceId, c.BrandId }).ToListAsync();
                if (rows.Any(c => c.WorkspaceId != workspace || !brands.Contains(c.BrandId)) ||
                    rows.Select(c => c.Id).Distinct().Count() != rows.Count)
                    throw new InvalidOperationException("Scoped list leaked a resource or duplicated a row.");
                samples.Add(timer.Elapsed.TotalMilliseconds);
                counts.Add(total);
            }
        }));
        if (counts.Distinct().Count() != 1) throw new InvalidOperationException("Static fixture count changed during load.");
        if (counts.First() < 10_000) throw new InvalidOperationException("Load dataset is too small.");
        // Traverse every page once to detect omissions/duplicates across page boundaries.
        await using (var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(source).Options))
        {
            await db.Database.OpenConnectionAsync();
            db.PermissionWorkspaceId = workspace; db.PermissionActorId = actor;
            db.PermissionOwner = true; db.PermissionScopeEnabled = true;
            db.PermissionBrandIds = (await new AccessControlService(db).GetAccessibleBrandIdsAsync(actor, workspace)).ToArray();
            var seen = new HashSet<Guid>();
            for (var offset = 0; offset < counts.First(); offset += 100)
            {
                var ids = await db.Contents.AsNoTracking().OrderBy(c => c.Id).Skip(offset).Take(100).Select(c => c.Id).ToListAsync();
                foreach (var id in ids) if (!seen.Add(id)) throw new InvalidOperationException("Duplicate across page boundaries.");
            }
            if (seen.Count != counts.First()) throw new InvalidOperationException("Pagination omitted rows.");
        }
        var ordered = samples.Order().ToArray();
        var analyticsSamples = new ConcurrentBag<double>();
        await Task.WhenAll(Enumerable.Range(0, workers).Select(async worker =>
        {
            var creator = creators[worker % 2].Id;
            await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(source).Options);
            await db.Database.OpenConnectionAsync();
            db.PermissionBrandIds = (await new AccessControlService(db).GetAccessibleBrandIdsAsync(creator, workspace)).ToArray();
            db.PermissionWorkspaceId = workspace; db.PermissionActorId = creator;
            db.PermissionCreator = true; db.PermissionScopeEnabled = true;
            if (await db.Contents.CountAsync() != 5000 || await db.Contents.AnyAsync(c => c.PrimaryCreatorId != creator))
                throw new InvalidOperationException("Creator scope leaked or omitted seeded content under concurrency.");
            var analytics = new MemberPerformanceService(db, new AccessControlService(db));
            for (var iteration = 0; iteration < 5; iteration++)
            {
                var timer = Stopwatch.StartNew();
                var result = await analytics.GetAsync(creator, workspace, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddMinutes(1));
                if (result.Items.Count != 1 || result.Items[0].MemberId != creator || result.Items[0].ContentsCreated != 5000)
                    throw new InvalidOperationException("Analytics leaked attribution or returned an incorrect fixture count.");
                var row = result.Items[0];
                if (row.CreatorPublishedPosts != 100 || row.PostsWithInsights != 100 || row.Engagement != 700 ||
                    row.Impressions != 100000 || row.Reach != 80000 || row.EngagementRate != .7m)
                    throw new InvalidOperationException("Published-post analytics leaked scope or failed latest-report aggregation.");
                analyticsSamples.Add(timer.Elapsed.TotalMilliseconds);
            }
        }));
        var analyticsOrdered = analyticsSamples.Order().ToArray();
        var report = new
        {
            measuredAtUtc = DateTime.UtcNow, kind = "local-postgresql-scoped-list",
            workers, iterations, requests = samples.Count, pageSize, visibleContents = counts.First(),
            p50Ms = ordered[(int)Math.Ceiling(ordered.Length * .50) - 1],
            p95Ms = ordered[(int)Math.Ceiling(ordered.Length * .95) - 1],
            maxMs = ordered[^1], elapsedMs = wall.Elapsed.TotalMilliseconds,
            scopeChecksPassed = true,
            creatorConcurrentChecks = workers, syntheticActorsIncludingExistingOwner = 5,
            analyticsRequests = analyticsSamples.Count,
            syntheticPosts = 200, syntheticInsightReports = 400, latestInsightAggregationPassed = true,
            analyticsP95Ms = analyticsOrdered[(int)Math.Ceiling(analyticsOrdered.Length * .95) - 1],
            limitations = "10,000 synthetic Content, 200 synthetic published posts and 400 reports; Owner list and Creator MemberPerformance; excludes HTTP, upload and provider latency. Not a staging load acceptance."
        };
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"PASS local scoped-list load: {samples.Count} operations, {workers} workers, {report.visibleContents} visible contents, p95={report.p95Ms:F1}ms");
        Console.WriteLine($"PASS local Creator analytics: {report.analyticsRequests} operations, p95={report.analyticsP95Ms:F1}ms; 5000 attributed contents each");
    }
}
