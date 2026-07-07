using AISAM.Repositories;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AISAM.API.Infrastructure
{
    public static class DevDataSeeder
    {
        public static void SeedDevData(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DevDataSeeder");
            logger.LogWarning("WARNING: DevDataSeeder is activated. Auto-upgrading Free users to Premium!");

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AisamContext>();

            var freeSubs = dbContext.Subscriptions
                .Include(s => s.Workspace)
                .Where(s => s.Plan == SubscriptionPlanEnum.Free && s.Workspace != null)
                .ToList();

            foreach (var sub in freeSubs)
            {
                var wsType = sub.Workspace!.WorkspaceType;

                sub.Plan = SubscriptionPlanEnum.Premium;
                sub.QuotaPostsPerMonth = wsType == WorkspaceTypeEnum.Personal ? 1_000 : 20_000;
                sub.QuotaAIContentPerDay = 200;
                sub.QuotaAIImagesPerDay = 30;
                sub.QuotaPlatforms = 3;
                sub.QuotaAccounts = 5;
                sub.AnalysisLevel = 2;
                sub.QuotaAdBudgetMonthly = 10_000_000m;
                sub.QuotaAdCampaigns = 10;
                sub.EndDate = DateTime.UtcNow.AddYears(1);

                var wallet = dbContext.CreditWallets.FirstOrDefault(w => w.WorkspaceId == sub.WorkspaceId);
                if (wallet != null)
                {
                    var credits = wsType == WorkspaceTypeEnum.Personal ? 2_000L : 50_000L;
                    wallet.Balance = credits;
                }
            }

            dbContext.SaveChanges();
            logger.LogWarning("DevDataSeeder: Auto-upgrade completed.");
        }
    }
}
