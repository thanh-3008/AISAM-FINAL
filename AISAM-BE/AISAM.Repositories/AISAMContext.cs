using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories
{
    public class AisamContext : DbContext
    {
        public AisamContext(DbContextOptions<AisamContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SocialAccount> SocialAccounts { get; set; }
        public DbSet<SocialIntegration> SocialIntegrations { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<WorkspaceInvitation> WorkspaceInvitations { get; set; }
        public DbSet<CreditWallet> CreditWallets { get; set; }
        public DbSet<CreditUsageRecord> CreditUsageRecords { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamBrand> TeamBrands { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdCampaign> AdCampaigns { get; set; }
        public DbSet<AdSet> AdSets { get; set; }
        public DbSet<AdCreative> AdCreatives { get; set; }
        public DbSet<PerformanceReport> PerformanceReports { get; set; }
        public DbSet<ContentCalendar> ContentCalendars { get; set; }
        public DbSet<AiGeneration> AiGenerations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ContentTemplate> ContentTemplates { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<VideoGenerationJob> VideoGenerationJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity indexes and constraints
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<int>().HasDefaultValue(UserRoleEnum.User);
                entity.HasIndex(u => u.Role);
                entity.HasIndex(u => u.CreatedAt);
            });

            // Session entity configuration
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.RefreshToken);
                entity.HasIndex(s => s.ExpiresAt);
                entity.HasIndex(s => s.IsActive);
                entity.HasOne(s => s.User)
                      .WithMany(u => u.Sessions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Brand entity configuration
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.ProfileId);
                entity.HasIndex(b => b.WorkspaceId);
                entity.HasIndex(b => b.Name);
                entity.HasOne(b => b.Profile)
                      .WithMany(p => p.Brands)
                      .HasForeignKey(b => b.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Workspace)
                      .WithMany(w => w.Brands)
                      .HasForeignKey(b => b.WorkspaceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Content entity configuration
            modelBuilder.Entity<Content>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.AdType).HasConversion<int>();
                entity.Property(c => c.Status).HasConversion<int>().HasDefaultValue(ContentStatusEnum.Draft);
                entity.HasIndex(c => c.BrandId);
                entity.HasIndex(c => c.WorkspaceId);
                entity.HasIndex(c => c.ProductId);
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasOne(c => c.Brand)
                      .WithMany(b => b.Contents)
                      .HasForeignKey(c => c.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Product)
                      .WithMany(p => p.Contents)
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.Workspace).WithMany(w => w.Contents).HasForeignKey(c => c.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            });

            // Product entity configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.BrandId);
                entity.HasIndex(p => p.Name);
                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SocialAccount entity configuration
            modelBuilder.Entity<SocialAccount>(entity =>
            {
                entity.HasKey(sa => sa.Id);
                entity.Property(sa => sa.Platform).HasConversion<int>();
                entity.HasIndex(sa => sa.ProfileId);
                entity.HasIndex(sa => sa.WorkspaceId);
                entity.HasIndex(sa => sa.Platform);
                entity.HasIndex(sa => sa.AccountId);
                entity.HasIndex(sa => sa.IsActive);
                entity.HasOne(sa => sa.Profile)
                      .WithMany(p => p.SocialAccounts)
                      .HasForeignKey(sa => sa.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(sa => sa.Workspace).WithMany(w => w.SocialAccounts).HasForeignKey(sa => sa.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            });

            // SocialIntegration entity configuration
            modelBuilder.Entity<SocialIntegration>(entity =>
            {
                entity.HasKey(si => si.Id);
                entity.HasIndex(si => si.ProfileId);
                entity.HasIndex(si => si.WorkspaceId);
                entity.HasIndex(si => si.BrandId);
                entity.HasIndex(si => si.SocialAccountId);
                entity.HasOne(si => si.Profile)
                      .WithMany(p => p.SocialIntegrations)
                      .HasForeignKey(si => si.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(si => si.Workspace).WithMany(w => w.SocialIntegrations).HasForeignKey(si => si.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(si => si.Brand)
                      .WithMany(b => b.SocialIntegrations)
                      .HasForeignKey(si => si.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(si => si.SocialAccount)
                      .WithMany(sa => sa.SocialIntegrations)
                      .HasForeignKey(si => si.SocialAccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Post entity configuration
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Status).HasConversion<int>().HasDefaultValue(ContentStatusEnum.Published).HasSentinel(ContentStatusEnum.Draft);
                entity.HasIndex(p => p.ContentId);
                entity.HasIndex(p => p.IntegrationId);
                entity.HasIndex(p => p.PublishedAt);
                entity.HasIndex(p => p.ExternalPostId);
                entity.HasOne(p => p.Content)
                      .WithMany(c => c.Posts)
                      .HasForeignKey(p => p.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.Integration)
                      .WithMany(i => i.Posts)
                      .HasForeignKey(p => p.IntegrationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Asset entity configuration
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AssetType).HasConversion<int>();
                entity.HasIndex(a => a.UploadedBy);
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UploadedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Profile entity configuration
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ProfileType).HasConversion<int>();
                entity.Property(p => p.Name).HasMaxLength(255).IsRequired();
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.SubscriptionId);
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Profiles)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.Subscription)
                      .WithMany()
                      .HasForeignKey(p => p.SubscriptionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Workspace foundation configuration
            modelBuilder.Entity<Workspace>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Name).HasMaxLength(255).IsRequired();
                entity.Property(w => w.WorkspaceType).HasConversion<int>();
                entity.Property(w => w.Status).HasConversion<int>().HasDefaultValue(WorkspaceStatusEnum.Active).HasSentinel((WorkspaceStatusEnum)0);
                entity.Property(w => w.MemberLimit).HasDefaultValue(1);
                entity.HasIndex(w => w.WorkspaceType);
                entity.HasIndex(w => w.Status);
                entity.HasIndex(w => w.SubscriptionExpiredAt);
                entity.HasIndex(w => w.ArchivedAt);
                entity.HasIndex(w => w.DeletedAt);
            });

            modelBuilder.Entity<WorkspaceMember>(entity =>
            {
                entity.HasKey(wm => wm.Id);
                entity.Property(wm => wm.Role).HasConversion<int>();
                entity.Property(wm => wm.QuotaMode).HasConversion<int>().HasDefaultValue(MemberQuotaModeEnum.SharedPool).HasSentinel((MemberQuotaModeEnum)0);
                entity.HasIndex(wm => wm.WorkspaceId);
                entity.HasIndex(wm => wm.UserId);
                entity.HasIndex(wm => new { wm.WorkspaceId, wm.UserId }).IsUnique();
                entity.HasIndex(wm => new { wm.WorkspaceId, wm.Role });
                entity.HasIndex(wm => wm.WorkspaceId)
                      .IsUnique()
                      .HasFilter("\"role\" = 1 AND \"is_active\" = TRUE");
                entity.HasIndex(wm => wm.IsActive);
                entity.HasOne(wm => wm.Workspace)
                      .WithMany(w => w.Members)
                      .HasForeignKey(wm => wm.WorkspaceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(wm => wm.User)
                      .WithMany(u => u.WorkspaceMembers)
                      .HasForeignKey(wm => wm.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WorkspaceInvitation>(entity =>
            {
                entity.HasKey(invitation => invitation.Id);
                entity.Property(invitation => invitation.Email).HasMaxLength(255).IsRequired();
                entity.Property(invitation => invitation.Token).HasMaxLength(500).IsRequired();
                entity.Property(invitation => invitation.Role).HasConversion<int>();
                entity.Property(invitation => invitation.QuotaMode).HasConversion<int>().HasDefaultValue(MemberQuotaModeEnum.SharedPool).HasSentinel((MemberQuotaModeEnum)0);
                entity.HasIndex(invitation => invitation.Token).IsUnique();
                entity.HasIndex(invitation => new { invitation.WorkspaceId, invitation.Email });
                entity.HasIndex(invitation => invitation.ExpiresAt);
                entity.HasOne(invitation => invitation.Workspace)
                      .WithMany(workspace => workspace.Invitations)
                      .HasForeignKey(invitation => invitation.WorkspaceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(invitation => invitation.InvitedByUser)
                      .WithMany(user => user.SentWorkspaceInvitations)
                      .HasForeignKey(invitation => invitation.InvitedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CreditWallet>(entity =>
            {
                entity.HasKey(wallet => wallet.Id);
                entity.HasIndex(wallet => wallet.WorkspaceId).IsUnique();
                entity.HasOne(wallet => wallet.Workspace)
                      .WithOne(workspace => workspace.CreditWallet)
                      .HasForeignKey<CreditWallet>(wallet => wallet.WorkspaceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CreditUsageRecord>(entity =>
            {
                entity.HasKey(record => record.Id);
                entity.Property(record => record.Action).HasConversion<int>();
                entity.Property(record => record.Status).HasConversion<int>();
                entity.HasIndex(record => record.WorkspaceId);
                entity.HasIndex(record => record.UserId);
                entity.HasIndex(record => record.AiGenerationId);
                entity.HasIndex(record => record.CreatedAt);
                entity.HasOne(record => record.Workspace)
                      .WithMany(workspace => workspace.CreditUsageRecords)
                      .HasForeignKey(record => record.WorkspaceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(record => record.User)
                      .WithMany()
                      .HasForeignKey(record => record.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(record => record.AiGeneration)
                      .WithMany()
                      .HasForeignKey(record => record.AiGenerationId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Team entity configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Status).HasConversion<int>().HasDefaultValue(TeamStatusEnum.Active);
                entity.HasIndex(t => t.ProfileId);
                entity.HasIndex(t => t.Name);
                entity.HasIndex(t => t.Status);
                entity.HasOne(t => t.Profile)
                      .WithMany(p => p.Teams)
                      .HasForeignKey(t => t.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TeamMember entity configuration
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(tm => tm.Id);
                entity.HasIndex(tm => tm.TeamId);
                entity.HasIndex(tm => tm.UserId);
                entity.HasOne(tm => tm.Team)
                      .WithMany(t => t.TeamMembers)
                      .HasForeignKey(tm => tm.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(tm => tm.User)
                      .WithMany(u => u.TeamMembers)
                      .HasForeignKey(tm => tm.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TeamBrand entity configuration
            modelBuilder.Entity<TeamBrand>(entity =>
            {
                entity.HasKey(tb => tb.Id);
                entity.HasIndex(tb => tb.TeamId);
                entity.HasIndex(tb => tb.BrandId);
                entity.HasIndex(tb => tb.IsActive);
                entity.HasOne(tb => tb.Team)
                      .WithMany(t => t.TeamBrands)
                      .HasForeignKey(tb => tb.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(tb => tb.Brand)
                      .WithMany(b => b.TeamBrands)
                      .HasForeignKey(tb => tb.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Subscription entity configuration
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Plan).HasConversion<int>();
                entity.HasIndex(s => s.ProfileId);
                entity.HasIndex(s => s.WorkspaceId);
                entity.HasIndex(s => s.IsActive);
                entity.HasOne(s => s.Profile)
                      .WithMany()
                      .HasForeignKey(s => s.ProfileId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(s => s.Workspace)
                      .WithMany(w => w.Subscriptions)
                      .HasForeignKey(s => s.WorkspaceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Approval entity configuration
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.ContentId);
                entity.HasIndex(a => a.ApproverProfileId);
                entity.HasOne(a => a.Content)
                      .WithMany(c => c.Approvals)
                      .HasForeignKey(a => a.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.ApproverProfile)
                      .WithMany(p => p.Approvals)
                      .HasForeignKey(a => a.ApproverProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // AdCampaign entity configuration
            modelBuilder.Entity<AdCampaign>(entity =>
            {
                entity.HasKey(ac => ac.Id);
                entity.HasIndex(ac => ac.ProfileId);
                entity.HasIndex(ac => ac.WorkspaceId);
                entity.HasIndex(ac => ac.BrandId);
                entity.HasIndex(ac => ac.Name);
                entity.HasOne(ac => ac.Profile)
                      .WithMany(p => p.AdCampaigns)
                      .HasForeignKey(ac => ac.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ac => ac.Workspace).WithMany(w => w.AdCampaigns).HasForeignKey(ac => ac.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ac => ac.Brand)
                      .WithMany(b => b.AdCampaigns)
                      .HasForeignKey(ac => ac.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AdSet entity configuration
            modelBuilder.Entity<AdSet>(entity =>
            {
                entity.HasKey(ads => ads.Id);
                entity.HasIndex(ads => ads.CampaignId);
                entity.HasOne(ads => ads.Campaign)
                      .WithMany(ac => ac.AdSets)
                      .HasForeignKey(ads => ads.CampaignId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AdCreative entity configuration
            modelBuilder.Entity<AdCreative>(entity =>
            {
                entity.HasKey(adc => adc.Id);
                entity.HasIndex(adc => adc.ContentId);
                entity.HasOne(adc => adc.Content)
                      .WithMany(c => c.AdCreatives)
                      .HasForeignKey(adc => adc.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Ad entity configuration
            modelBuilder.Entity<Ad>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.AdSetId);
                entity.HasIndex(a => a.CreativeId);
                entity.HasOne(a => a.AdSet)
                      .WithMany(ads => ads.Ads)
                      .HasForeignKey(a => a.AdSetId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Creative)
                      .WithMany(adc => adc.Ads)
                      .HasForeignKey(a => a.CreativeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerformanceReport entity configuration
            modelBuilder.Entity<PerformanceReport>(entity =>
            {
                entity.HasKey(pr => pr.Id);
                entity.HasIndex(pr => pr.PostId);
                entity.HasIndex(pr => pr.ReportDate);
                entity.HasOne(pr => pr.Post)
                      .WithMany(p => p.PerformanceReports)
                      .HasForeignKey(pr => pr.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ContentCalendar entity configuration
            modelBuilder.Entity<ContentCalendar>(entity =>
            {
                entity.HasKey(cc => cc.Id);
                entity.HasIndex(cc => new { cc.ContentId, cc.IntegrationId })
                      .IsUnique()
                      .HasFilter("\"status\" IN (0, 1)");
                entity.HasIndex(cc => cc.ProfileId);
                entity.HasIndex(cc => cc.WorkspaceId);
                entity.HasIndex(cc => cc.ScheduledDate);
                entity.HasIndex(cc => cc.IntegrationId);
                entity.HasIndex(cc => cc.ScheduledAt);
                entity.HasIndex(cc => cc.Status);
                entity.Property(cc => cc.Status).HasConversion<int>().HasDefaultValue(ScheduleStatusEnum.Pending);
                entity.HasOne(cc => cc.Content)
                      .WithMany(c => c.ContentCalendars)
                      .HasForeignKey(cc => cc.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cc => cc.Profile)
                      .WithMany(p => p.ContentCalendars)
                      .HasForeignKey(cc => cc.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cc => cc.Integration)
                      .WithMany()
                      .HasForeignKey(cc => cc.IntegrationId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(cc => cc.Workspace).WithMany(w => w.ContentCalendars).HasForeignKey(cc => cc.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            });

            // AiGeneration entity configuration
            modelBuilder.Entity<AiGeneration>(entity =>
            {
                entity.HasKey(ag => ag.Id);
                entity.Property(ag => ag.Status).HasConversion<int>().HasDefaultValue(AiStatusEnum.Pending);
                entity.HasIndex(ag => ag.ContentId);
                entity.HasIndex(ag => ag.Status);
                entity.HasOne(ag => ag.Content)
                      .WithMany(c => c.AiGenerations)
                      .HasForeignKey(ag => ag.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification entity configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Type).HasConversion<int>();
                entity.HasIndex(n => n.ProfileId);
                entity.HasIndex(n => n.WorkspaceId);
                entity.HasIndex(n => n.Type);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);
                entity.HasOne(n => n.Profile)
                      .WithMany(p => p.Notifications)
                      .HasForeignKey(n => n.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(n => n.Workspace).WithMany(w => w.Notifications).HasForeignKey(n => n.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            });

            // Payment entity configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Status).HasConversion<int>().HasDefaultValue(PaymentStatusEnum.Pending);
                entity.Property(p => p.PaymentType).HasConversion<int>().HasDefaultValue(PaymentTypeEnum.Subscription).HasSentinel((PaymentTypeEnum)0);
                entity.Property(p => p.CreditPackCode).HasConversion<int>();
                entity.Property(p => p.RequestedPlan).HasConversion<int>();
                entity.Property(p => p.Amount).HasPrecision(10, 2);
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.WorkspaceId);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.PaymentType);
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.Subscription)
                      .WithMany()
                      .HasForeignKey(p => p.SubscriptionId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Workspace)
                      .WithMany(w => w.Payments)
                      .HasForeignKey(p => p.WorkspaceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ContentTemplate entity configuration
            modelBuilder.Entity<ContentTemplate>(entity =>
            {
                entity.HasKey(ct => ct.Id);
                entity.HasIndex(ct => ct.BrandId);
                entity.HasIndex(ct => ct.TemplateType);
                entity.HasIndex(ct => ct.IsActive);
                entity.HasOne(ct => ct.Brand)
                      .WithMany()
                      .HasForeignKey(ct => ct.BrandId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog entity configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(al => al.Id);
                entity.HasIndex(al => al.ActorId);
                entity.HasIndex(al => al.TargetTable);
                entity.HasIndex(al => al.CreatedAt);
                entity.HasOne(al => al.Actor)
                      .WithMany()
                      .HasForeignKey(al => al.ActorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Conversation entity configuration
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.AdType).HasConversion<int>();
                entity.HasIndex(c => c.ProfileId);
                entity.HasIndex(c => c.WorkspaceId);
                entity.HasIndex(c => c.BrandId);
                entity.HasIndex(c => c.ProductId);
                entity.HasIndex(c => c.IsActive);
                entity.HasIndex(c => c.CreatedAt);
                entity.HasOne(c => c.Profile)
                      .WithMany(p => p.Conversations)
                      .HasForeignKey(c => c.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Brand)
                      .WithMany()
                      .HasForeignKey(c => c.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(c => c.Workspace).WithMany(w => w.Conversations).HasForeignKey(c => c.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
            });

            // ChatMessage entity configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(cm => cm.Id);
                entity.Property(cm => cm.SenderType).HasConversion<int>();
                entity.HasIndex(cm => cm.ConversationId);
                entity.HasIndex(cm => cm.CreatedAt);
                entity.HasOne(cm => cm.Conversation)
                      .WithMany(c => c.ChatMessages)
                      .HasForeignKey(cm => cm.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cm => cm.AiGeneration)
                      .WithMany()
                      .HasForeignKey(cm => cm.AiGenerationId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(cm => cm.Content)
                      .WithMany()
                      .HasForeignKey(cm => cm.ContentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // VideoGenerationJob entity configuration
            modelBuilder.Entity<VideoGenerationJob>(entity =>
            {
                entity.HasKey(j => j.Id);
                entity.Property(j => j.Status).HasConversion<int>().HasDefaultValue(AiStatusEnum.Pending);
                entity.HasIndex(j => j.WorkspaceId);
                entity.HasIndex(j => j.UserId);
                entity.HasIndex(j => j.Status);
                entity.HasIndex(j => j.IsFallback);
                entity.HasOne(j => j.Workspace)
                      .WithMany()
                      .HasForeignKey(j => j.WorkspaceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(j => j.User)
                      .WithMany()
                      .HasForeignKey(j => j.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
