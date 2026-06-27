# 📋 SPECIFICATION ANSWERS - NHÓM 3 (19 YÊU CẦU CHI TIẾT)

## Superseding Approved Decisions - Workspace Model

`CHANGE_REQUEST_WORKSPACE_SUBSCRIPTION_CREDIT_ANALYSIS.md` thay the cac de xuat cu lien quan den Profile subscription, Team Leader governance va AI quota.

Lifecycle/expiry policy moi nhat duoc chot tai `docs/product/workspace-subscription-expiry-policy.md`:

- Personal paid expiry -> Personal Free; retained Credits chi dung cho Free features.
- Business khong co Free tier; pending payment/expired Business khong duoc tieu Credits.
- Business creation khong cap Credits; verified payment/renewal grant phai idempotent.

- Ownership/subscription/credits chuyen sang Workspace.
- Role dung Owner, Manager, Content Creator, Viewer.
- Moi Workspace co dung mot Owner va mot Credit Wallet.
- Business Plus cap 15.000 Credits/toi da 10 members; Business Pro cap 50.000 Credits/toi da 50 members.
- Cac code sample ProfileId, daily quota hoac Team Leader ben duoi la phan tich cu, khong con la target implementation.

Dựa vào phân tích source code cũ **PRN232_Backend**, dưới đây là trả lời chi tiết cho tất cả 19 yêu cầu:

---

## 🔵 **NHÓM CỮ (8 ITEMS)**

### 1️⃣ **Team Permission Model** 🔴 High

**❓ Câu hỏi:** Ai được quyền duyệt/đăng/quản lý team? Có 1 Leader per team không?

**📊 Hiện Tại (Source Code):**
- ✅ **Có Team model** - `Team { Id, Name, ProfileId, Members, Brands, Permissions }`
- ✅ **Có TeamMember model** - `TeamMember { UserId, TeamId, Role, Permissions, IsActive }`
- ✅ **Có permission-based RBAC** - Permissions stored as JSONB array
- ✅ **Có role field** - Supports flexible roles (Leader, Member, Manager)
- ❌ **Nhưng:** Chưa enforce **1 Leader per team** bằng database constraint hay service validation

**🔧 Vấn Đề Hiện Tại:**
```
- Không có constraint bắt buộc team phải có 1 leader
- Có thể tạo team mà không có leader hoặc có nhiều leader
- Permission check là JSONB, có thể bị sai lệch giữa code logic và database
```

**✅ Phương Án Khả Thi (Dễ Làm):**

**Ngắn hạn (1 tuần):**
1. Thêm enum `TeamMemberRole` (Leader, Member, Manager)
   ```csharp
   public enum TeamMemberRole 
   { 
       Leader = 1,    // Duy nhất trong team
       Manager = 2,   // Phó leader
       Member = 3     // Thành viên
   }
   ```

2. Thêm service validation trong `TeamService`
   ```csharp
   public async Task AddTeamMember(Guid teamId, Guid userId, TeamMemberRole role)
   {
       if (role == TeamMemberRole.Leader)
       {
           var existingLeader = await _db.TeamMembers
               .FirstOrDefaultAsync(m => m.TeamId == teamId && m.Role == TeamMemberRole.Leader);
           
           if (existingLeader != null && existingLeader.UserId != userId)
               throw new InvalidOperationException("Team already has a leader");
       }
   }
   ```

3. Thêm business logic:
   - Khi tạo team → tự động thêm creator làm Leader
   - Khi leader inactive → assign cho manager hoặc throw error
   - Prevent leader removal nếu không có thay thế

4. Thêm API validation endpoint
   ```
   POST /api/teams/{teamId}/validate-governance
   → Check: 1 leader, >= 1 member, all permissions valid
   ```

**Trung hạn (2-3 tuần):**
- Thêm migration SQL để validate data hiện tại
- Audit log mỗi lần thay đổi leader
- Update UI để mandate role selection

**⏱️ Estimated Effort:** 
- Backend: 2-3 days (validation + logic + migration)
- Frontend: 1 day (UI enforcement)
- Testing: 1 day

---

### 2️⃣ **Subscription Plans** 🔴 High

**❓ Câu hỏi:** Plans hiện dùng enum, có thể config động? Cần CRUD hay không?

**📊 Hiện Tại (Source Code):**
- ✅ **Dùng enum** - `SubscriptionPlan { Free, Plus, Premium, PlusTrial }`
- ✅ **Có quota tracking** - `QuotaAIContentPerDay`, `QuotaAIImagesPerDay`, `BrandLimit`, `TeamLimit`
- ✅ **Có Payment model** - Liên kết với subscription, PayOS webhook
- ❌ **Nhưng:** Không có entity SubscriptionPlan để CRUD, không có versioning

**📋 Current Plans Structure:**
```
Free       → $0/month, 5 brands, 5 teams, 5 AI content/day, 3 images/day
Plus       → $5/month, 10 brands, 10 teams, 20 AI content/day, 10 images/day  
Premium    → $15/month, unlimited brands, unlimited teams, unlimited quota
PlusTrial  → 14 days free Premium trial
```

**🔧 Vấn Đề Hiện Tại:**
```
- Thay đổi plan yêu cầu cập nhật enum + migration + redeploy
- Không có audit log khi plan thay đổi
- Không có proration (nếu downgrade giữa tháng)
- Không có versioning (tracking lịch sử plan changes)
```

**✅ Phương Án Khả Thi:**

**Nếu KHÔNG cần CRUD động (Recommended Short-term):**
- Giữ enum hiện tại, thêm configuration file
- Tạo `PlanConfiguration.json` với quota mapping
  ```json
  {
    "plans": [
      {"name": "Free", "monthlyPrice": 0, "quota": {...}},
      {"name": "Plus", "monthlyPrice": 5, "quota": {...}}
    ]
  }
  ```
- Load configuration vào cache khi app start
- Admin update file → app restart (simple & safe)

**Nếu CẦN CRUD động (Recommended Mid-term):**

1. **Tạo entity `SubscriptionPlan`**
   ```csharp
   public class SubscriptionPlan
   {
       public Guid Id { get; set; }
       public string Code { get; set; }  // "Free", "Plus", "Premium"
       public string Name { get; set; }
       public decimal MonthlyPrice { get; set; }
       public int QuotaBrandCount { get; set; }
       public int QuotaTeamCount { get; set; }
       public int QuotaAIContentPerDay { get; set; }
       public int QuotaAIImagesPerDay { get; set; }
       public int Priority { get; set; }  // For sorting/display
       public DateTime EffectiveFrom { get; set; }
       public DateTime? EffectiveTo { get; set; }  // For versioning
       public bool IsActive { get; set; }
       public DateTime CreatedAt { get; set; }
   }
   ```

2. **Tạo `PlanHistory` entity** (audit trail)
   ```csharp
   public class SubscriptionPlanHistory
   {
       public Guid Id { get; set; }
       public Guid PlanId { get; set; }
       public Guid ProfileId { get; set; }
       public string FromPlan { get; set; }
       public string ToPlan { get; set; }
       public DateTime ChangedAt { get; set; }
       public string ChangedBy { get; set; }
   }
   ```

3. **Admin CRUD endpoints**
   ```
   POST   /api/admin/plans              → Create new plan
   GET    /api/admin/plans              → List all (active + historical)
   GET    /api/admin/plans/{id}         → Get detail
   PUT    /api/admin/plans/{id}         → Update (soft archive old version)
   DELETE /api/admin/plans/{id}         → Soft delete
   ```

4. **Migration logic** (when user subscription changes)
   ```csharp
   public async Task ChangePlan(Guid profileId, string newPlanCode)
   {
       var currentPlan = await _db.Subscriptions
           .OrderByDescending(s => s.CreatedAt)
           .FirstOrDefaultAsync(s => s.ProfileId == profileId);
       
       var newPlan = await _db.SubscriptionPlans
           .FirstOrDefaultAsync(p => p.Code == newPlanCode && p.IsActive);
       
       // Calculate proration (if downgrade mid-month)
       var days_used = (DateTime.Now - currentPlan.StartDate).Days;
       var days_total = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
       var credit = currentPlan.Plan.MonthlyPrice * (days_total - days_used) / days_total;
       
       // Create new subscription
       var newSubscription = new Subscription
       {
           ProfileId = profileId,
           PlanId = newPlan.Id,
           StartDate = DateTime.Now,
           Status = "active",
           Credits = credit  // Apply credit
       };
       
       // Log history
       await _db.SubscriptionPlanHistories.AddAsync(new SubscriptionPlanHistory
       {
           PlanId = newPlan.Id,
           ProfileId = profileId,
           FromPlan = currentPlan.Plan.Code,
           ToPlan = newPlan.Code,
           ChangedAt = DateTime.Now,
           ChangedBy = userId
       });
   }
   ```

**⏱️ Estimated Effort:**
- **Config file approach:** 1-2 days
- **Full CRUD approach:** 1 week (entity + migration + admin UI + versioning)

**✅ Recommendation:**
→ **Short-term (Tháng 1):** Dùng config file + cache
→ **Mid-term (Tháng 2-3):** Implement full CRUD entity

---

### 3️⃣ **Instagram Implementation** 🔴 High

**❓ Câu hỏi:** Enum có Instagram nhưng provider chưa ready. Phát triển không?

**📊 Hiện Tại (Source Code):**
- ✅ **Có enum** - `SocialPlatformEnum { Facebook, Google, Instagram, TikTok, Twitter }`
- ✅ **Có Instagram model** - `SocialIntegration` store Instagram account/token
- ❌ **Nhưng:** Không có `InstagramProvider` implementation
- ❌ **Không có:** Publishing endpoint cho Instagram
- ❌ **Không có:** Instagram Business Account discovery flow

**🔧 Vấn Đề:**
```
- Enum tồn tại nhưng provider chưa implement
- Gây confusion cho developer (tưởng feature đã ready)
- Instagram API khác Facebook (needs separate implementation)
```

**✅ Phương Án Khả Thi:**

**Decision Point:** Phụ thuộc vào business priority
- **Nếu KHÔNG phát triển ngay:** Remove từ enum, để PLANNED status
- **Nếu CẦN phát triển:** Follow phương án dưới

**📅 Instagram Implementation Roadmap (Mid-term - 4-6 tuần):**

**Phase 1: Research & Setup (Week 1-2)**
1. Review Instagram Graph API v20.0+
   - Instagram Business Account discovery
   - Media publishing (carousel, single image, video, story)
   - Insights & analytics endpoints
   - Required permissions (`instagram_basic`, `instagram_content_publishing`, `pages_read_insights`)

2. Create `InstagramProvider` interface
   ```csharp
   public interface IInstagramProvider : ISocialProvider
   {
       Task<InstagramAccount> GetBusinessAccountAsync(string accessToken);
       Task<bool> PublishPhotoAsync(PublishRequest request);
       Task<bool> PublishCarouselAsync(PublishRequest request);
       Task<bool> PublishVideoAsync(PublishRequest request);
       Task<InstagramMetrics> GetInsightsAsync(string igMediaId, string accessToken);
       Task RevokeTokenAsync(string accessToken);
   }
   ```

**Phase 2: Core Implementation (Week 2-3)**
1. Implement `InstagramProvider` class
   ```csharp
   public class InstagramProvider : IInstagramProvider
   {
       private readonly HttpClient _httpClient;
       private readonly ILogger<InstagramProvider> _logger;
       private readonly IEncryptionService _encryption;
       
       public async Task<bool> PublishPhotoAsync(PublishRequest request)
       {
           var igMediaId = request.SocialAccount.ExternalId;
           var accessToken = _encryption.Decrypt(request.SocialAccount.AccessToken);
           
           var payload = new
           {
               image_url = request.ImageUrl,
               caption = request.Caption,
               user_tags = request.UserTags  // @mention support
           };
           
           var response = await _httpClient.PostAsJsonAsync(
               $"https://graph.instagram.com/v20.0/{igMediaId}/media",
               payload,
               new { access_token = accessToken }
           );
           
           if (!response.IsSuccessStatusCode)
           {
               _logger.LogError($"Instagram publish failed: {await response.Content.ReadAsStringAsync()}");
               return false;
           }
           
           var result = await response.Content.ReadAsAsync<dynamic>();
           // Save result.id as facebook_post_id for future reference
           return true;
       }
   }
   ```

2. Add `InstagramIntegration` model
   ```csharp
   public class InstagramIntegration
   {
       public Guid Id { get; set; }
       public Guid BrandId { get; set; }
       public string IgBusinessAccountId { get; set; }
       public string IgUserId { get; set; }
       public string AccessToken { get; set; }  // encrypted
       public string RefreshToken { get; set; }  // encrypted
       public DateTime TokenExpiresAt { get; set; }
       public bool IsConnected { get; set; }
       public string[] Permissions { get; set; }  // ["instagram_basic", ...]
       public DateTime ConnectedAt { get; set; }
   }
   ```

**Phase 3: OAuth Flow (Week 3)**
1. Add Instagram OAuth endpoint
   ```csharp
   [HttpGet("oauth/instagram")]
   public async Task<IActionResult> InstagramOAuth()
   {
       var scopes = "instagram_business_basic,instagram_business_content_publishing,pages_read_insights";
       var redirectUri = $"{_config["AppUrl"]}/api/social/instagram/callback";
       
       var authUrl = $"https://www.facebook.com/v20.0/dialog/oauth" +
           $"?client_id={_config["Facebook:AppId"]}" +
           $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
           $"&scope={Uri.EscapeDataString(scopes)}" +
           $"&response_type=code" +
           $"&display=popup" +
           $"&auth_type=rerequest";
       
       return Redirect(authUrl);
   }
   ```

2. Handle callback
   ```csharp
   [HttpGet("instagram/callback")]
   public async Task<IActionResult> InstagramCallback(string code, string state)
   {
       // Exchange code for tokens (same as Facebook flow)
       // Get Instagram Business Account ID
       // Store in InstagramIntegration table
       // Encrypt access_token + refresh_token
   }
   ```

**Phase 4: Publishing Integration (Week 4)**
1. Update `PublishService` to handle Instagram
   ```csharp
   public async Task<PublishResult> PublishAsync(Guid contentId, Guid integrationId)
   {
       var content = await _db.Contents.FindAsync(contentId);
       var integration = await _db.SocialIntegrations.FindAsync(integrationId);
       
       var provider = integration.PlatformEnum switch
       {
           SocialPlatformEnum.Facebook => _facebookProvider,
           SocialPlatformEnum.Instagram => _instagramProvider,
           _ => throw new NotSupportedException()
       };
       
       var result = await provider.PublishAsync(new PublishRequest
       {
           Title = content.Title,
           Caption = content.TextContent,
           ImageUrl = content.ImageUrl,
           VideoUrl = content.VideoUrl,
           // ... other fields
       });
       
       return result;
   }
   ```

2. Add Instagram content validation
   ```csharp
   public class InstagramPublishValidator : AbstractValidator<PublishRequest>
   {
       public InstagramPublishValidator()
       {
           // Instagram caption max 2200 chars
           RuleFor(x => x.Caption).MaximumLength(2200);
           
           // Require image or video
           RuleFor(x => x)
               .Must(x => !string.IsNullOrEmpty(x.ImageUrl) || !string.IsNullOrEmpty(x.VideoUrl))
               .WithMessage("Instagram post requires image or video");
           
           // Video max 60 seconds for IG reels
           RuleFor(x => x.VideoDuration)
               .LessThanOrEqualTo(60)
               .When(x => !string.IsNullOrEmpty(x.VideoUrl))
               .WithMessage("Instagram Reels max 60 seconds");
       }
   }
   ```

**Phase 5: Analytics (Week 4-5)**
1. Add Instagram insights pulling
   ```csharp
   public async Task<InstagramMetrics> GetInsightsAsync(Guid postId)
   {
       var post = await _db.Posts.FindAsync(postId);
       var igMediaId = post.ExternalIds["instagram"];
       
       var response = await _httpClient.GetAsync(
           $"https://graph.instagram.com/v20.0/{igMediaId}" +
           $"?fields=like_count,comments_count,impressions,engagement" +
           $"&access_token={accessToken}"
       );
       
       var data = await response.Content.ReadAsAsync<dynamic>();
       return new InstagramMetrics
       {
           Likes = data.like_count,
           Comments = data.comments_count,
           Impressions = data.impressions,
           Engagement = data.engagement
       };
   }
   ```

**⏱️ Estimated Effort:**
- **Research:** 2-3 days
- **Core implementation:** 5-7 days
- **Testing:** 2-3 days
- **Total:** **2-3 weeks**

**✅ Quick Decision Matrix:**
```
Priority:   🔴 High (if business needs Vietnamese market expansion)
Effort:     2-3 weeks
Complexity: Medium (similar to Facebook, but different API)
Recommendation: Plan for next sprint, NOT this sprint
```

---

### 4️⃣ **Background Job Reliability** 🔴 High

**❓ Câu hỏi:** Lịch đăng bài có retry policy, monitoring? Reliable không?

**📊 Hiện Tại (Source Code):**
- ✅ **Có ScheduledPostingBackgroundService** - Chạy every 5 minutes
- ✅ **Có ContentCalendar model** - Stores schedule + recurring info
- ✅ **Có retry logic (implicit)** - Retry on exception
- ❌ **Nhưng:** 
  - Không có max retry count
  - Không có Dead Letter Queue (DLQ)
  - Không có failed job monitoring
  - Không có exponential backoff
  - Service crash = posts bị miss

**🔧 Vấn Đề Hiện Tại:**
```
Current flow:
- BackgroundService runs every 5 min
- Loop through ContentCalendar.IsScheduled == true
- Try publish, if error → implicit retry (no count limit)
- If still error → mark as failed (but no DLQ)
- Service crash → no state persistence → posts miss window

Risks:
- API rate limit từ Facebook → infinite retry → API ban
- Network timeout → service hang → missed schedules
- No alerting → admin không biết job failed
- 5 min check = không real-time, có thể miss time window
```

**✅ Phương Án Khả Thi:**

**Ngắn hạn (1-2 tuần) - Immediate Fixes:**

1. **Thêm retry policy với backoff**
   ```csharp
   public class ScheduledPostingBackgroundService : BackgroundService
   {
       private readonly ILogger<ScheduledPostingBackgroundService> _logger;
       private readonly IPublishService _publishService;
       
       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
       {
           while (!stoppingToken.IsCancellationRequested)
           {
               try
               {
                   var pendingSchedules = await _db.ContentCalendars
                       .Where(c => c.IsScheduled && c.ScheduledTime <= DateTime.Now)
                       .ToListAsync();
                   
                   foreach (var schedule in pendingSchedules)
                   {
                       // Max 3 retries with exponential backoff
                       var maxRetries = 3;
                       var retryCount = schedule.RetryCount ?? 0;
                       
                       if (retryCount >= maxRetries)
                       {
                           // Move to DLQ if failed after max retries
                           schedule.Status = "failed_dlq";
                           _logger.LogError($"Schedule {schedule.Id} moved to DLQ after {maxRetries} retries");
                           
                           // Create alert
                           await _notificationService.CreateAdminAlert(
                               $"Schedule failed: {schedule.ContentId}",
                               AlertLevel.Error
                           );
                           
                           continue;
                       }
                       
                       try
                       {
                           await _publishService.PublishAsync(schedule.ContentId, schedule.IntegrationId);
                           schedule.Status = "published";
                           schedule.PublishedAt = DateTime.Now;
                           _logger.LogInformation($"Schedule {schedule.Id} published successfully");
                       }
                       catch (ApiRateLimitException ex)
                       {
                           // Rate limit → exponential backoff, max 1 hour
                           retryCount++;
                           var backoffMinutes = Math.Min(60, Math.Pow(2, retryCount) * 5);
                           schedule.NextRetryAt = DateTime.Now.AddMinutes(backoffMinutes);
                           schedule.RetryCount = retryCount;
                           schedule.Status = "pending_retry";
                           _logger.LogWarning($"Rate limit, retry in {backoffMinutes}m");
                       }
                       catch (Exception ex)
                       {
                           // Other errors → standard retry
                           retryCount++;
                           schedule.NextRetryAt = DateTime.Now.AddMinutes(Math.Pow(2, retryCount));
                           schedule.RetryCount = retryCount;
                           schedule.Status = "pending_retry";
                           _logger.LogError($"Publish error, retry #{retryCount}: {ex.Message}");
                       }
                       
                       await _db.SaveChangesAsync();
                   }
                   
                   // Check interval: 5 minutes
                   await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
               }
               catch (Exception ex)
               {
                   _logger.LogCritical($"BackgroundService crashed: {ex}");
                   // Continue trying instead of crash
                   await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
               }
           }
       }
   }
   ```

2. **Thêm DLQ (Dead Letter Queue)**
   ```csharp
   public class FailedSchedule
   {
       public Guid Id { get; set; }
       public Guid ContentCalendarId { get; set; }
       public string ErrorMessage { get; set; }
       public int RetryCount { get; set; }
       public Exception LastException { get; set; }
       public DateTime FailedAt { get; set; }
   }
   
   // Add to DbContext
   public DbSet<FailedSchedule> FailedSchedules { get; set; }
   ```

3. **Add monitoring endpoint**
   ```csharp
   [HttpGet("api/admin/scheduled-jobs/status")]
   public async Task<IActionResult> GetJobStatus()
   {
       var stats = new
       {
           Pending = await _db.ContentCalendars.CountAsync(c => c.Status == "pending"),
           Scheduled = await _db.ContentCalendars.CountAsync(c => c.Status == "scheduled"),
           Published = await _db.ContentCalendars.CountAsync(c => c.Status == "published"),
           Failed = await _db.FailedSchedules.CountAsync(),
           LastRun = _backgroundService.LastExecutionTime,
           IsHealthy = (DateTime.Now - _backgroundService.LastExecutionTime).TotalMinutes < 10
       };
       
       return Ok(stats);
   }
   ```

**Trung hạn (2-3 tuần) - Robustness:**

4. **Switch to proper job queue** (Quartz.NET hoặc Hangfire)
   ```csharp
   // Instead of BackgroundService, use Quartz
   public class PublishScheduledPostsJob : IJob
   {
       public async Task Execute(IJobExecutionContext context)
       {
           // Same logic as BackgroundService
           // But with built-in retry, persistence, monitoring
       }
   }
   
   // Register in Startup
   services.AddQuartz(q =>
   {
       var jobKey = new JobKey("PublishScheduledPosts");
       q.AddJob<PublishScheduledPostsJob>(jobKey)
           .AddTrigger(t => t
               .ForJob(jobKey)
               .WithSimpleSchedule(s => s
                   .WithIntervalInMinutes(5)
                   .RepeatForever()
               )
           );
   });
   ```

5. **Add comprehensive logging + alerting**
   ```csharp
   public class JobMonitoring
   {
       // Log all job executions
       public class JobExecutionLog
       {
           public Guid Id { get; set; }
           public string JobName { get; set; }
           public DateTime StartTime { get; set; }
           public DateTime? EndTime { get; set; }
           public string Status { get; set; }  // Success, Failed, Timeout
           public int ItemsProcessed { get; set; }
           public int ItemsFailed { get; set; }
           public string ErrorLog { get; set; }
       }
       
       // Alert if job fails
       public async Task AlertOnJobFailure(JobExecutionLog log)
       {
           if (log.Status == "Failed")
           {
               await _notificationService.SendEmailAsync(
                   adminEmail,
                   $"Job {log.JobName} failed at {log.EndTime}",
                   log.ErrorLog
               );
               
               await _telemetryService.LogMetricAsync(
                   "background_job_failure",
                   new { JobName = log.JobName, Error = log.ErrorLog }
               );
           }
       }
   }
   ```

**⏱️ Estimated Effort:**
- **Immediate fixes (retry + DLQ):** 3-5 days
- **Switch to Quartz:** 3-4 days
- **Monitoring + alerting:** 2-3 days
- **Total:** **1-2 weeks**

---

### 5️⃣ **AI Video Flow** 🔴 High

**❓ Câu hỏi:** VideoUrl field có nhưng chưa sinh video AI. Phát triển khi nào?

**📊 Hiện Tại (Source Code):**
- ✅ **VideoUrl field tồn tại** - `Content.VideoUrl`
- ✅ **AdType VideoText tồn tại** - `AdType { TextOnly, ImageText, VideoText }`
- ✅ **Can publish VideoText** - Publishing logic supports video URL
- ❌ **Nhưng:** Không có pipeline sinh video AI
- ❌ **Không có:** Video provider integration
- ❌ **Không có:** Progress tracking/monitoring

**🔧 Vấn Đề:**
```
- Fields tồn tại nhưng không functional → confusion
- Video generation = most expensive AI operation
- Chưa quyết định provider nào (OpenAI Sora, Runway ML, D-ID, v.v.)
- Chưa có cost/quota model
```

**✅ Phương Án Khả Thi:**

**Decision:** Đây là **Long-term feature**, NOT short-term

**Recommendation:**
1. **Remove VideoText from current deployment** (hoặc mark as "Coming Soon")
2. **Keep in roadmap** cho Q3-Q4

**IF phát triển Q2 (6-8 tuần):**

**Phase 1: Research & Decision (Week 1)**
- Evaluate video providers:
  - `OpenAI Sora` - Best quality, HIGH cost ($20-50/min video)
  - `Runway ML` - Good quality, Medium cost ($10-20/min)
  - `D-ID` - Avatar-based, Lower cost ($5-15/video)
  - `HeyGen` - Avatar-based, Lower cost ($3-10/video)

**Phase 2: Architecture Design (Week 1-2)**
```csharp
// Video Generation Provider Interface
public interface IVideoProvider
{
    Task<VideoGenerationJob> GenerateAsync(VideoGenerationRequest request);
    Task<VideoGenerationJob> GetStatusAsync(string jobId);
    Task<string> GetResultUrlAsync(string jobId);
    Task CancelAsync(string jobId);
}

public class VideoGenerationRequest
{
    public string Prompt { get; set; }
    public int DurationSeconds { get; set; }  // 15-60 seconds
    public string Style { get; set; }  // e.g., "professional", "casual", "cinematic"
    public string Music { get; set; }  // Optional background music
    public string Voiceover { get; set; }  // Optional TTS voice
}

public class VideoGenerationJob
{
    public string Id { get; set; }
    public string Status { get; set; }  // Queued, Processing, Completed, Failed
    public int ProgressPercent { get; set; }
    public string OutputUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ErrorMessage { get; set; }
}
```

**Phase 3: Database Schema**
```csharp
public class AiVideoGeneration
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public string ProviderJobId { get; set; }  // e.g., Runway job ID
    public string Status { get; set; }  // queued, processing, completed, failed
    public string PromptUsed { get; set; }
    public int DurationSeconds { get; set; }
    public int ProgressPercent { get; set; }
    public string OutputVideoUrl { get; set; }
    public decimal CostUSD { get; set; }
    public string ErrorLog { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class AiVideoQuota
{
    public Guid ProfileId { get; set; }
    public int VideosPerMonth { get; set; }
    public int VideosUsedThisMonth { get; set; }
    public decimal MonthlyBudgetUSD { get; set; }
    public decimal SpentThisMonthUSD { get; set; }
}
```

**Phase 4: Implementation (Week 3-5)**
```csharp
public class VideoGenerationService
{
    public async Task<AiVideoGeneration> GenerateVideoAsync(Guid contentId, string prompt)
    {
        var content = await _db.Contents.FindAsync(contentId);
        var profile = await _db.Profiles.FindAsync(content.ProfileId);
        
        // Check quota
        var quota = await _db.AiVideoQuotas.FindAsync(profile.Id);
        if (quota.VideosUsedThisMonth >= quota.VideosPerMonth)
            throw new QuotaExceededException();
        
        // Check budget
        var estimatedCost = 15;  // USD, provider-dependent
        if (quota.SpentThisMonthUSD + estimatedCost > quota.MonthlyBudgetUSD)
            throw new BudgetExceededException();
        
        // Generate video
        var request = new VideoGenerationRequest
        {
            Prompt = prompt,
            DurationSeconds = 30,
            Style = content.StyleDescription ?? "professional"
        };
        
        var job = await _videoProvider.GenerateAsync(request);
        
        // Store in DB
        var aiVideo = new AiVideoGeneration
        {
            ContentId = contentId,
            ProviderJobId = job.Id,
            Status = job.Status,
            PromptUsed = prompt,
            DurationSeconds = request.DurationSeconds,
            CreatedAt = DateTime.Now
        };
        
        await _db.AiVideoGenerations.AddAsync(aiVideo);
        await _db.SaveChangesAsync();
        
        return aiVideo;
    }
    
    public async Task<AiVideoGeneration> PollVideoStatusAsync(Guid videoGenId)
    {
        var aiVideo = await _db.AiVideoGenerations.FindAsync(videoGenId);
        var job = await _videoProvider.GetStatusAsync(aiVideo.ProviderJobId);
        
        aiVideo.Status = job.Status;
        aiVideo.ProgressPercent = job.ProgressPercent;
        
        if (job.Status == "Completed")
        {
            aiVideo.OutputVideoUrl = job.OutputUrl;
            aiVideo.CompletedAt = DateTime.Now;
            
            // Update content
            var content = await _db.Contents.FindAsync(aiVideo.ContentId);
            content.VideoUrl = aiVideo.OutputVideoUrl;
            
            // Update quota
            var quota = await _db.AiVideoQuotas.FindAsync(content.ProfileId);
            quota.VideosUsedThisMonth++;
            quota.SpentThisMonthUSD += 15;  // Update from actual cost
        }
        else if (job.Status == "Failed")
        {
            aiVideo.ErrorLog = job.ErrorMessage;
        }
        
        await _db.SaveChangesAsync();
        return aiVideo;
    }
}
```

**Phase 5: Frontend Integration (Week 5-6)**
- Add video generation UI in Content creation
- Show progress bar while video is being generated
- Allow re-generation if failed
- Show cost before confirming

**Phase 6: Testing & Monitoring (Week 6-8)**
- Load testing with multiple concurrent video generations
- Cost monitoring dashboard
- Provider failover (if one provider fails, try another)

**⏱️ Estimated Effort:**
- **Full implementation:** 6-8 weeks
- **Cost:** Depends on provider + usage

**✅ Short-term Recommendation:**
→ **Keep VideoText enum but mark UI as "Coming Soon"**
→ **Plan for Q2/Q3 development**
→ **NOT this sprint**

---

### 6️⃣ **Budget Auto-Optimization** 🟡 Medium

**❓ Câu hỏi:** Có tự động điều chỉnh ngân sách quảng cáo không?

**📊 Hiện Tại (Source Code):**
- ❌ **Không có service nào** tự động điều chỉnh ad budget
- ✅ **Có AdSet model** - Stores daily_budget, lifetime_budget
- ✅ **Có PerformanceReport model** - Tracks ROI, CPC, CPM
- ❌ **Nhưng:** Không có optimization algorithm

**🔧 Vấn Đề:**
```
- Ad optimization là complex machine learning task
- Requires historical data + pattern analysis
- Can easily waste money if not careful
- Needs manual approval để avoid automated mishaps
```

**✅ Phương Án Khả Thi:**

**Simple Version (1 week) - Rules-based:**
```csharp
public class BudgetOptimizationService
{
    /// <summary>
    /// Simple rule-based budget optimization
    /// If ROI > target → increase budget by 10%
    /// If ROI < target → decrease budget by 10%
    /// Daily check, manual approval required
    /// </summary>
    public async Task<BudgetAdjustmentSuggestion> GetOptimizationSuggestionAsync(Guid campaignId)
    {
        var campaign = await _db.AdCampaigns.FindAsync(campaignId);
        var report = await _db.PerformanceReports
            .Where(r => r.CampaignId == campaignId)
            .OrderByDescending(r => r.DateRange.End)
            .FirstOrDefaultAsync();
        
        var targetROI = 3.0m;  // 300% ROI target
        var currentROI = report?.ROI ?? 0;
        
        var suggestion = new BudgetAdjustmentSuggestion
        {
            CampaignId = campaignId,
            CurrentDailyBudget = campaign.DailyBudget,
            SuggestedDailyBudget = campaign.DailyBudget,
            Reason = "No adjustment needed",
            Status = "pending_review",
            RequiresApproval = false
        };
        
        if (currentROI > targetROI)
        {
            // Increase by 10%
            suggestion.SuggestedDailyBudget = campaign.DailyBudget * 1.1m;
            suggestion.Reason = $"ROI {currentROI}x exceeds target {targetROI}x";
            suggestion.RequiresApproval = true;
        }
        else if (currentROI < targetROI * 0.5m)
        {
            // Decrease by 10%
            suggestion.SuggestedDailyBudget = campaign.DailyBudget * 0.9m;
            suggestion.Reason = $"ROI {currentROI}x below 50% of target";
            suggestion.RequiresApproval = true;
        }
        
        return suggestion;
    }
    
    public async Task<bool> ApplyAdjustmentAsync(Guid suggestionId, bool approved)
    {
        var suggestion = await _db.BudgetAdjustmentSuggestions.FindAsync(suggestionId);
        
        if (!approved)
        {
            suggestion.Status = "rejected";
            await _db.SaveChangesAsync();
            return false;
        }
        
        // Apply adjustment via Facebook API
        var campaign = await _db.AdCampaigns.FindAsync(suggestion.CampaignId);
        await _facebookProvider.UpdateAdSetBudgetAsync(
            campaign.FacebookAdSetId,
            suggestion.SuggestedDailyBudget
        );
        
        suggestion.Status = "approved";
        suggestion.AppliedAt = DateTime.Now;
        
        await _db.SaveChangesAsync();
        return true;
    }
}
```

**Advanced Version (4 weeks) - ML-based:**
- Integrate Azure ML / TensorFlow
- Use historical data to predict best budget allocation
- A/B test different budgets → learn optimal
- Requires audit log + rollback capability

**⏱️ Estimated Effort:**
- **Simple version:** 1 week
- **Advanced version:** 4 weeks

**✅ Recommendation:**
→ **Start with simple rules-based version (1 week)**
→ **Collect data for 2-3 months**
→ **Upgrade to ML version later**

---

### 7️⃣ **Provider Architecture** 🟢 Good (Tạm được)

**❓ Câu hỏi:** AI/Payment/Social providers có abstraction layer không?

**📊 Hiện Tại (Source Code):**
- ✅ **Có IProviderService interface** - Base interface cho providers
- ✅ **FacebookProvider class** - Implements social publishing
- ✅ **GoogleProvider class** - Implements OAuth
- ✅ **GeminiProvider class** - Implements AI text generation
- ✅ **VertexAIProvider class** - Implements image generation
- ✅ **PayOSProvider class** - Implements payment

**✅ Architecture đã tốt:**
```csharp
// Base interface
public interface IProviderService
{
    string ProviderName { get; }
    Task<bool> IsConfiguredAsync();
}

// Social provider
public interface ISocialProvider : IProviderService
{
    Task<PublishResult> PublishAsync(PublishRequest request);
    Task<PostMetrics> GetMetricsAsync(string postId);
}

// AI provider
public interface IAIProvider : IProviderService
{
    Task<string> GenerateTextAsync(GenerateTextRequest request);
    Task<string> GenerateImageAsync(GenerateImageRequest request);
}

// Dependency injection
services.AddScoped<IProviderService, FacebookProvider>();
services.AddScoped<ISocialProvider, FacebookProvider>();
services.AddScoped<IAIProvider, GeminiProvider>();
```

**✅ Recommendation:** Architecture tốt, không cần refactor ngay

**⏱️ Maintenance Items (1-2 tuần):**
1. Add error handling middleware
2. Add provider configuration validation
3. Add provider health check endpoint
4. Document provider interface contracts

---

### 8️⃣ **Test Coverage** 🟡 Medium

**❓ Câu hỏi:** Các luồng chính có test đầy đủ không?

**📊 Hiện Tại (Source Code):**
- ✅ **Có folder `UnitTests` và `IntegrationTests`**
- ❌ **Nhưng:** Tập trung vào controller tests, không đầy đủ service logic
- ❌ **Chưa test:** Payment flow, Approval workflow, Scheduled posting, AI generation

**🔧 Vấn Đề:**
```
- Core business logic không có test
- High-risk scenarios không cover:
  - Payment callback validation
  - Retry logic
  - Permission enforcement
  - Content approval flow
```

**✅ Phương Án Khả Thi (2-3 tuần):**

1. **Test Payment Flow**
   ```csharp
   [TestClass]
   public class PaymentServiceTests
   {
       [TestMethod]
       public async Task CreatePayOSCheckout_ValidProfile_ReturnsCheckoutUrl()
       {
           // Arrange
           var profile = CreateTestProfile();
           var service = new PaymentService(_mockPayOSProvider, _db);
           
           // Act
           var result = await service.CreateCheckoutAsync(profile.Id, "Plus");
           
           // Assert
           Assert.IsNotNull(result.CheckoutUrl);
           Assert.IsTrue(result.CheckoutUrl.Contains("payos.vn"));
       }
       
       [TestMethod]
       public async Task ProcessPaymentCallback_ValidSignature_ActivatesSubscription()
       {
           // Arrange
           var payment = CreateTestPayment();
           var callback = new PayOSCallback { OrderCode = payment.OrderCode, Status = "PAID" };
           
           // Act
           await service.ProcessCallbackAsync(callback);
           
           // Assert
           var updatedPayment = await _db.Payments.FindAsync(payment.Id);
           Assert.AreEqual("completed", updatedPayment.Status);
           
           var subscription = await _db.Subscriptions.FindAsync(updatedPayment.SubscriptionId);
           Assert.IsTrue(subscription.IsActive);
       }
   }
   ```

2. **Test Approval Workflow**
   ```csharp
   [TestClass]
   public class ApprovalServiceTests
   {
       [TestMethod]
       public async Task SubmitForApproval_ValidContent_CreatesApprovalRecord()
       {
           // Test content submission
       }
       
       [TestMethod]
       public async Task ApproveContent_NoPermission_ThrowsUnauthorizedAccessException()
       {
           // Test permission enforcement
       }
       
       [TestMethod]
       public async Task RejectContent_WithReason_NotifiesCreator()
       {
           // Test notification logic
       }
   }
   ```

3. **Test Publishing/Scheduling**
   ```csharp
   [TestClass]
   public class ScheduledPostingTests
   {
       [TestMethod]
       public async Task PublishScheduledPosts_PastScheduleTime_PublishesContent()
       {
           // Test scheduling logic
       }
       
       [TestMethod]
       public async Task PublishScheduledPosts_FacebookApiError_RetriesToMaxRetries()
       {
           // Test retry logic
       }
   }
   ```

**⏱️ Estimated Effort:** 2-3 weeks

---

## 🔵 **NHÓM MỚI (11 ITEMS)**

### 🤖 **AI Quota Management** 🔴 High

**❓ Câu hỏi:** Tính theo số lần API / token / số bài / combo? Reset theo ngày/tuần/tháng? Hard/soft limit?

**📊 Hiện Tại (Source Code):**
- ✅ **Có quota tracking** - `QuotaAIContentPerDay`, `QuotaAIImagesPerDay`
- ✅ **Quota check trước generation** - Prevent exceed
- ✅ **Reset daily** - Quota đặt lại mỗi ngày
- ❌ **Nhưng:** 
  - Chỉ track số lượng (posts/images), không track token
  - Không có soft limit (warning)
  - Không có gradual throttling

**📋 Current Quota Model:**
```
Free:     5 posts/day, 3 images/day
Plus:     20 posts/day, 10 images/day
Premium:  Unlimited
```

**✅ Phương Án Khả Thi:**

**Ngắn hạn (1 tuần) - Current Approach Sufficient:**
- Giữ hệ thống hiện tại (posts/images per day)
- Thêm soft limit warning: "Used 80% of quota"
- Thêm quota reset scheduler chính xác lúc nửa đêm UTC

**Code:**
```csharp
public class AiQuotaService
{
    public async Task<QuotaCheckResult> CheckQuotaAsync(Guid profileId, AiServiceType serviceType)
    {
        var profile = await _db.Profiles.FindAsync(profileId);
        var subscription = await _db.Subscriptions
            .Where(s => s.ProfileId == profileId && s.IsActive)
            .FirstOrDefaultAsync();
        
        var quota = subscription.Plan switch
        {
            "Free" => new { posts = 5, images = 3 },
            "Plus" => new { posts = 20, images = 10 },
            "Premium" => new { posts = 99999, images = 99999 },
        };
        
        var today = DateTime.Today;
        var used = serviceType switch
        {
            AiServiceType.TextGeneration => await _db.AiGenerations
                .Where(g => g.ProfileId == profileId && g.CreatedAt.Date == today)
                .CountAsync(),
            AiServiceType.ImageGeneration => await _db.AiGenerations
                .Where(g => g.ProfileId == profileId && g.CreatedAt.Date == today && g.ImageUrl != null)
                .CountAsync()
        };
        
        var limit = serviceType == AiServiceType.TextGeneration ? quota.posts : quota.images;
        var remaining = limit - used;
        var usagePercent = (decimal)used / limit * 100;
        
        return new QuotaCheckResult
        {
            HasQuota = remaining > 0,
            Remaining = remaining,
            UsagePercent = usagePercent,
            Warning = usagePercent >= 80 ? $"Used {usagePercent}% of daily quota" : null,
            ResetAt = DateTime.Today.AddDays(1)  // Next midnight UTC
        };
    }
}
```

**Trung hạn (2-3 tuần) - Advanced Quota System:**

1. **Track by cost (tokens)**
   ```csharp
   public class AiQuotaUsage
   {
       public Guid Id { get; set; }
       public Guid ProfileId { get; set; }
       public string ServiceType { get; set; }  // TextGeneration, ImageGeneration
       public int TokensUsed { get; set; }
       public decimal CostUSD { get; set; }
       public DateTime CreatedAt { get; set; }
   }
   
   // Pricing model
   // Gemini text: $0.075 per 1M input tokens, $0.3 per 1M output tokens
   // Vertex Imagen: $0.04 per image (1024x1024)
   ```

2. **Soft limits + throttling**
   ```csharp
   public enum QuotaLimitType
   {
       Hard,        // Reject request if exceeded
       Soft,        // Warn but allow
       Throttle     // Slow down request (add delay)
   }
   
   public async Task<(bool allowed, string? warning)> CheckQuotaWithThrottleAsync(
       Guid profileId, 
       AiServiceType serviceType)
   {
       var usage = await GetDailyUsageAsync(profileId, serviceType);
       var limit = GetLimit(profileId, serviceType);
       
       if (usage > limit * 0.9m)  // 90%
           return (true, $"Used {usage}% of quota, consider upgrading");
       
       if (usage > limit * 0.99m)  // 99%
           return (true, "Almost at quota limit, adding delay...");
           // Add 5-10 second delay to discourage heavy usage
       
       if (usage >= limit)
           return (false, "Quota exceeded");
       
       return (true, null);
   }
   ```

3. **Monthly cap (in addition to daily)**
   ```csharp
   public class MonthlyAiQuota
   {
       public Guid ProfileId { get; set; }
       public int Year { get; set; }
       public int Month { get; set; }
       public decimal BudgetUSD { get; set; }
       public decimal SpentUSD { get; set; }
       public bool IsSoftLimit { get; set; } = true;  // Warn vs Reject
   }
   ```

**⏱️ Estimated Effort:**
- **Current system:** Sufficient for now
- **Advanced system:** 2-3 weeks

**✅ Recommendation:**
→ Giữ system hiện tại, add soft limit warning
→ Monitor usage 2-3 tháng
→ Upgrade to token-based nếu user feedback yêu cầu

---

### ✔️ **Leader Approval Workflow** 🔴 High

**❓ Câu hỏi:** Content status flow? SLA bao lâu? Xử lý Leader vắng mặt? Quy tắc chuyển quyền?

**📊 Hiện Tại (Source Code):**
- ✅ **Có Approval model** - Tracks approver, status, created/updated time
- ✅ **Có status flow** - pending → approved/rejected
- ✅ **Có multi-approver support** - Multiple approvers can review
- ❌ **Nhưng:**
  - Không có SLA (Service Level Agreement) tracking
  - Không có escalation nếu leader không response
  - Không có delegation khi leader offline
  - Không có auto-approve timeout

**📋 Current Flow:**
```
1. Creator submits content
2. Approval created (status: pending)
3. Leader reviews
4. Leader approves/rejects
5. Notification sent
```

**🔧 Vấn Đề:**
```
- Nếu leader offline → content stuck
- Không có SLA monitoring → missing deadlines
- Không biết approval pending bao lâu
```

**✅ Phương Án Khả Thi:**

**Ngắn hạn (1 tuần) - SLA + Escalation:**

```csharp
public class ApprovalSLA
{
    public Guid Id { get; set; }
    public Guid ApprovalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueAt { get; set; }  // e.g., +24 hours
    public string Status { get; set; }  // OnTrack, Warning, Overdue
    public DateTime? EscalatedAt { get; set; }
}

public class ApprovalService
{
    public async Task<Approval> CreateApprovalAsync(Guid contentId, Guid teamId)
    {
        var leader = await GetTeamLeaderAsync(teamId);
        
        var approval = new Approval
        {
            ContentId = contentId,
            ApproverId = leader.UserId,
            Status = "pending",
            CreatedAt = DateTime.Now
        };
        
        var sla = new ApprovalSLA
        {
            ApprovalId = approval.Id,
            CreatedAt = DateTime.Now,
            DueAt = DateTime.Now.AddHours(24),  // 24-hour SLA
            Status = "OnTrack"
        };
        
        await _db.Approvals.AddAsync(approval);
        await _db.ApprovalSLAs.AddAsync(sla);
        await _db.SaveChangesAsync();
        
        return approval;
    }
    
    public async Task CheckAndEscalateOverdueApprovalsAsync()
    {
        var overdue = await _db.ApprovalSLAs
            .Where(s => s.DueAt < DateTime.Now && s.Status == "OnTrack")
            .ToListAsync();
        
        foreach (var sla in overdue)
        {
            sla.Status = "Overdue";
            sla.EscalatedAt = DateTime.Now;
            
            var approval = await _db.Approvals.FindAsync(sla.ApprovalId);
            
            // Escalate to team manager or admin
            var team = await _db.Teams.FindAsync(approval.Approval.Content.Team);
            var manager = await GetTeamManagerAsync(team.Id);
            
            if (manager != null)
            {
                approval.ApproverId = manager.UserId;  // Reassign
                
                await _notificationService.SendAsync(manager.UserId,
                    $"Approval overdue, reassigned to you",
                    $"Content '{approval.Content.Title}' approval overdue"
                );
            }
            else
            {
                // No manager, send to admin
                await _notificationService.SendToAdminsAsync(
                    $"Approval overdue with no manager",
                    $"Team {team.Name} has no manager to escalate to"
                );
            }
        }
        
        await _db.SaveChangesAsync();
    }
}
```

**Trung hạn (2 tuần) - Delegation + Auto-Approve:**

1. **Delegation flow (leader offline)**
   ```csharp
   public class ApprovalDelegation
   {
       public Guid Id { get; set; }
       public Guid TeamId { get; set; }
       public Guid FromLeaderId { get; set; }
       public Guid ToManagerId { get; set; }
       public DateTime StartDate { get; set; }
       public DateTime EndDate { get; set; }
       public bool IsActive => DateTime.Now >= StartDate && DateTime.Now < EndDate;
   }
   
   public async Task DelegateApprovalAsync(Guid teamId, Guid toManagerId, int daysCount)
   {
       var leader = await GetTeamLeaderAsync(teamId);
       
       var delegation = new ApprovalDelegation
       {
           TeamId = teamId,
           FromLeaderId = leader.UserId,
           ToManagerId = toManagerId,
           StartDate = DateTime.Now,
           EndDate = DateTime.Now.AddDays(daysCount)
       };
       
       await _db.ApprovalDelegations.AddAsync(delegation);
       await _db.SaveChangesAsync();
       
       // Notify team
       await _notificationService.SendToTeamAsync(teamId,
           $"Approval delegated to {toManager.Name} until {delegation.EndDate}",
           NotificationLevel.Info
       );
   }
   ```

2. **Auto-approve if overdue X hours**
   ```csharp
   public async Task AutoApproveOverdueAsync(int hoursOverdue = 48)
   {
       var toAutoApprove = await _db.ApprovalSLAs
           .Where(s => s.DueAt.AddHours(hoursOverdue) < DateTime.Now && s.Status != "Resolved")
           .ToListAsync();
       
       foreach (var sla in toAutoApprove)
       {
           var approval = await _db.Approvals.FindAsync(sla.ApprovalId);
           approval.Status = "auto_approved";
           approval.ApprovedAt = DateTime.Now;
           approval.ApprovedBy = "SYSTEM";
           
           // Publish content automatically
           var content = await _db.Contents.FindAsync(approval.ContentId);
           await _publishService.PublishAsync(content.Id);
           
           sla.Status = "Resolved";
       }
       
       await _db.SaveChangesAsync();
   }
   ```

**⏱️ Estimated Effort:**
- **SLA + Escalation:** 5-7 days
- **Delegation + Auto-approve:** 7-10 days
- **Total:** **2 weeks**

---

### 📝 **Prompting Strategy** 🟡 Medium

**❓ Câu hỏi:** Template prompt chuẩn? Lưu history? Versioning? Ai được chỉnh sửa?

**📊 Hiện Tại (Source Code):**
- ✅ **Có prompt building từ Brand + Product** - Dynamic prompt construction
- ❌ **Nhưng:** Không có prompt template management
- ❌ **Không có:** Prompt versioning
- ❌ **Không có:** Prompt history/audit log

**📋 Current Prompt Building:**
```csharp
public string BuildPrompt(Brand brand, Product product, Content content)
{
    return $@"Create a post for {brand.Name}.
Description: {brand.Description}
Slogan: {brand.Slogan}
USP: {brand.USP}
Target audience: {brand.TargetAudience}

Product: {product.Name}
Product description: {product.Description}
Product price: {product.Price}

Style: {content.StyleDescription}
Context: {content.ContextDescription}
Character: {content.RepresentativeCharacter}

Generate engaging content that appeals to the target audience.";
}
```

**🔧 Vấn Đề:**
```
- Prompt hardcoded trong code → không flexible
- Không có way để optimize prompt
- Không có audit trail nếu prompt thay đổi
- Không có versioning
```

**✅ Phương Án Khả Thi:**

**Ngắn hạn (1 tuần) - Template System:**

```csharp
public class PromptTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // e.g., "Default Facebook Post", "LinkedIn Professional"
    public string TemplateText { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDefault { get; set; }
    public int Version { get; set; }
}

public class PromptTemplateVariable
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string VariableName { get; set; }  // e.g., "brand_name", "product_price"
    public string Placeholder { get; set; }  // e.g., {{brand_name}}
}

public class PromptHistory
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public Guid TemplateId { get; set; }
    public string PromptUsed { get; set; }
    public string GeneratedText { get; set; }
    public DateTime GeneratedAt { get; set; }
    public decimal Quality { get; set; }  // 1-5 rating from user feedback
}

public class PromptService
{
    public async Task<string> BuildPromptAsync(Guid contentId, Guid? templateId = null)
    {
        var content = await _db.Contents.FindAsync(contentId);
        var brand = await _db.Brands.FindAsync(content.BrandId);
        var product = await _db.Products.FindAsync(content.ProductId);
        
        var template = templateId.HasValue 
            ? await _db.PromptTemplates.FindAsync(templateId.Value)
            : await _db.PromptTemplates.FirstOrDefaultAsync(t => t.IsDefault);
        
        var prompt = template.TemplateText
            .Replace("{{brand_name}}", brand.Name)
            .Replace("{{brand_description}}", brand.Description)
            .Replace("{{brand_slogan}}", brand.Slogan)
            .Replace("{{brand_usp}}", brand.USP)
            .Replace("{{target_audience}}", brand.TargetAudience)
            .Replace("{{product_name}}", product.Name)
            .Replace("{{product_description}}", product.Description)
            .Replace("{{product_price}}", product.Price.ToString())
            .Replace("{{style_description}}", content.StyleDescription ?? "")
            .Replace("{{context}}", content.ContextDescription ?? "")
            .Replace("{{character}}", content.RepresentativeCharacter ?? "");
        
        return prompt;
    }
    
    public async Task<Guid> SavePromptHistoryAsync(Guid contentId, string prompt, string result)
    {
        var history = new PromptHistory
        {
            ContentId = contentId,
            PromptUsed = prompt,
            GeneratedText = result,
            GeneratedAt = DateTime.Now
        };
        
        await _db.PromptHistories.AddAsync(history);
        await _db.SaveChangesAsync();
        
        return history.Id;
    }
}
```

**Admin endpoints:**
```csharp
[HttpPost("api/admin/prompt-templates")]
public async Task<IActionResult> CreateTemplate(CreatePromptTemplateRequest request)
{
    // Create new template version
}

[HttpGet("api/admin/prompt-templates")]
public async Task<IActionResult> ListTemplates()
{
    // List all templates with versions
}

[HttpPut("api/admin/prompt-templates/{id}")]
public async Task<IActionResult> UpdateTemplate(Guid id, UpdatePromptTemplateRequest request)
{
    // Create new version, mark old as archived
}

[HttpGet("api/contents/{id}/prompt-history")]
public async Task<IActionResult> GetPromptHistory(Guid id)
{
    // Show all prompts used for this content
}
```

**⏱️ Estimated Effort:** 5-7 days

---

### 📚 **Content Library** 🔴 High

**❓ Câu hỏi:** Lưu tất cả revisions hay latest only? Phân quyền chi tiết? Version control? Soft/hard delete policy?

**📊 Hiện Tại (Source Code):**
- ✅ **Soft delete** - `IsDeleted` flag
- ✅ **Restore capability** - Can restore deleted content
- ❌ **Nhưng:** Không có revision tracking
- ❌ **Không có:** Full version history
- ❌ **Không có:** Content diff/compare

**✅ Phương Án Khả Thi:**

**Ngắn hạn (1 tuần) - Add Revision Tracking:**

```csharp
public class ContentRevision
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public int RevisionNumber { get; set; }  // 1, 2, 3...
    public string Title { get; set; }
    public string TextContent { get; set; }
    public string ImageUrl { get; set; }
    public string VideoUrl { get; set; }
    public string StyleDescription { get; set; }
    public string ContextDescription { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangeReason { get; set; }  // "AI improved", "User edited", "Admin fix"
}

public class ContentService
{
    public async Task<ContentRevision> CreateRevisionAsync(
        Guid contentId, 
        Content updates,
        Guid userId,
        string reason)
    {
        var content = await _db.Contents.FindAsync(contentId);
        
        var revision = new ContentRevision
        {
            ContentId = contentId,
            RevisionNumber = await GetNextRevisionNumberAsync(contentId),
            Title = updates.Title,
            TextContent = updates.TextContent,
            ImageUrl = updates.ImageUrl,
            VideoUrl = updates.VideoUrl,
            StyleDescription = updates.StyleDescription,
            ContextDescription = updates.ContextDescription,
            ChangedBy = userId,
            ChangedAt = DateTime.Now,
            ChangeReason = reason
        };
        
        // Update content
        content.Title = updates.Title;
        content.TextContent = updates.TextContent;
        // ... other fields
        
        await _db.ContentRevisions.AddAsync(revision);
        await _db.SaveChangesAsync();
        
        return revision;
    }
    
    public async Task<IEnumerable<ContentRevision>> GetRevisionHistoryAsync(Guid contentId)
    {
        return await _db.ContentRevisions
            .Where(r => r.ContentId == contentId)
            .OrderByDescending(r => r.RevisionNumber)
            .ToListAsync();
    }
    
    public async Task RestoreRevisionAsync(Guid revisionId)
    {
        var revision = await _db.ContentRevisions.FindAsync(revisionId);
        var content = await _db.Contents.FindAsync(revision.ContentId);
        
        // Restore all fields
        content.Title = revision.Title;
        content.TextContent = revision.TextContent;
        // ... other fields
        
        // Create new revision to track restore
        await CreateRevisionAsync(content.Id, content, GetCurrentUserId(), $"Restored from revision {revision.RevisionNumber}");
    }
}
```

**trung hạn (2 tuần) - Advanced Features:**

1. **Content permissions**
   ```csharp
   public class ContentPermission
   {
       public Guid Id { get; set; }
       public Guid ContentId { get; set; }
       public Guid UserId { get; set; }
       public string Permission { get; set; }  // view, edit, delete, publish
       public DateTime GrantedAt { get; set; }
       public Guid GrantedBy { get; set; }
   }
   ```

2. **Diff/Compare revisions**
   ```csharp
   public async Task<ContentDiff> CompareRevisionsAsync(Guid revision1Id, Guid revision2Id)
   {
       var r1 = await _db.ContentRevisions.FindAsync(revision1Id);
       var r2 = await _db.ContentRevisions.FindAsync(revision2Id);
       
       return new ContentDiff
       {
           TitleDiff = DiffText(r1.Title, r2.Title),
           TextDiff = DiffText(r1.TextContent, r2.TextContent),
           ImageChanged = r1.ImageUrl != r2.ImageUrl,
           VideoChanged = r1.VideoUrl != r2.VideoUrl
       };
   }
   ```

3. **Hard delete policy**
   ```
   - Soft delete: 30 days retention
   - Hard delete: After 30 days + admin approval
   - OR immediately with admin override
   ```

**⏱️ Estimated Effort:** 1-2 weeks

---

[... Các yêu cầu còn lại: Meta OAuth, Scheduled Posts, Ads Automation, Analytics, Payment, Data Model, Security - sẽ được thêm vào phần tiếp theo ...]

---

## 📊 **SUMMARY TABLE - 19 YÊU CẦU**

| # | Yêu Cầu | Status | Effort | Priority | Timeline |
|----|---------|--------|--------|----------|----------|
| 1 | Team Permission | ⚠️ Partial | 2-3 days | 🔴 High | Week 1 |
| 2 | Subscription Plans | ⚠️ Enum only | 1-2 weeks | 🔴 High | Week 2-3 |
| 3 | Instagram | ❌ None | 2-3 weeks | 🔴 High | Sprint 2 |
| 4 | Background Job | ✅ Basic | 1-2 weeks | 🔴 High | Week 1-2 |
| 5 | AI Video | ❌ None | 6-8 weeks | 🔴 High | Q2 |
| 6 | Budget Auto | ❌ None | 1-4 weeks | 🟡 Medium | Q2 |
| 7 | Provider Arch | ✅ Good | 1 week | 🟢 Low | Maintenance |
| 8 | Test Coverage | ⚠️ Partial | 2-3 weeks | 🔴 High | Week 2-3 |
| 9 | AI Quota | ✅ Basic | Maintain | 🟢 Low | N/A |
| 10 | Leader Approval | ⚠️ Basic | 2 weeks | 🔴 High | Week 1-2 |
| 11 | Prompting | ⚠️ Basic | 1 week | 🟡 Medium | Week 1 |
| 12 | Content Library | ⚠️ Basic | 1-2 weeks | 🔴 High | Week 2 |
| 13 | Meta OAuth | ⚠️ Security issue | 1 week | 🔴 High | URGENT |
| 14 | Scheduled Posts | ✅ Basic | 1-2 weeks | 🔴 High | Week 1 |
| 15 | Ads Automation | ✅ Basic | Maintain | 🟢 Low | N/A |
| 16 | Analytics | ⚠️ Basic | 1 week | 🟡 Medium | Week 1 |
| 17 | Payment | ✅ Basic | 1 week | 🔴 High | Week 2 |
| 18 | Data Model | ✅ Good | Maintain | 🟢 Low | N/A |
| 19 | Security/RBAC | ⚠️ Partial | 1-2 weeks | 🔴 High | URGENT |

---

## 🚀 **PRIORITY MATRIX - NEXT 3 MONTHS**

### **URGENT (This Week)**
- 🔒 **Meta Token Encryption** - Security issue
- 🔒 **Team Leader Enforcement** - Governance
- 🔐 **Security/RBAC Hardening** - Audit log implementation

### **SPRINT 1 (Week 1-2)**
- ✅ Approval SLA + Escalation
- ✅ Prompt Template System
- ✅ Add retry logic to scheduled posting
- ✅ Meta token encryption
- ✅ Team leader constraint

### **SPRINT 2 (Week 3-4)**
- ✅ Content revision tracking
- ✅ Improve test coverage (payment, approval)
- ✅ Dynamic subscription plans (config file approach)
- ✅ Scheduled posts monitoring

### **SPRINT 3 (Week 5-6)**
- ✅ Ads automation rules
- ✅ Analytics caching
- ✅ Payment proration logic
- ✅ Admin dashboard improvements

### **FUTURE (Q2-Q3)**
- 🔄 Instagram integration
- 🔄 AI video generation
- 🔄 Advanced budget optimization
- 🔄 Sentiment analysis

---

## ✅ **CONCLUSION**

| Aspect | Status | Action |
|--------|--------|--------|
| **Source code quality** | ✅ Good | Maintain |
| **Architecture** | ✅ Good | Minor refactoring |
| **Security** | ⚠️ Needs improvement | Encrypt tokens, add audit log |
| **Test coverage** | ⚠️ Needs improvement | Add service tests |
| **Feature completeness** | ✅ Decent | Add missing pieces (SLA, versioning) |

**→ Tổng cộng: 6-8 tuần để implement all 19 requirements**

---

**Document này được tạo:** 2026-05-26
