import { PostItem } from "@/services/postService";

interface StatsCardsProps {
  posts: PostItem[];
}

export default function StatsCards({ posts }: StatsCardsProps) {
  const publishedCount = posts.filter((p) => p.status === "Published").length;
  const failedCount = posts.filter((p) => p.status === "Failed").length;
  const scheduledCount = posts.filter((p) => p.status === "Scheduled").length;
  const draftCount = posts.filter((p) => p.status === "Draft").length;
  
  // Calculate total engagement
  const totalLikes = posts.reduce((sum, p) => sum + (p.likes || 0), 0);
  const totalComments = posts.reduce((sum, p) => sum + (p.comments || 0), 0);
  const totalShares = posts.reduce((sum, p) => sum + (p.shares || 0), 0);
  
  // Find next scheduled post
  const nextScheduled = posts
    .filter((p) => p.status === "Scheduled")
    .sort((a, b) => new Date(a.publishedAt).getTime() - new Date(b.publishedAt).getTime())[0];
  
  // Format next scheduled time
  const formatNextSchedule = () => {
    if (!nextScheduled) return "No scheduled posts";
    const date = new Date(nextScheduled.publishedAt);
    const now = new Date();
    const diffMs = date.getTime() - now.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    
    if (diffHours > 24) {
      const diffDays = Math.floor(diffHours / 24);
      return `In ${diffDays} days`;
    } else if (diffHours > 0) {
      return `In ${diffHours}h ${diffMinutes}m`;
    } else if (diffMinutes > 0) {
      return `In ${diffMinutes}m`;
    } else {
      return "Now";
    }
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-4 gap-gutter">
      {/* Published Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm hover:shadow-md transition-shadow">
        <div className="w-14 h-14 rounded-2xl bg-emerald-50 flex items-center justify-center text-emerald-600">
          <span className="material-symbols-outlined text-[28px]">task_alt</span>
        </div>
        <div className="flex-1">
          <p className="text-label-sm text-outline uppercase font-semibold">Published</p>
          <div className="flex items-baseline gap-2">
            <h3 className="text-headline-md text-on-surface">{publishedCount}</h3>
            <span className="text-[11px] text-outline">posts</span>
          </div>
          <div className="flex items-center justify-between mt-2">
            <p className="text-[11px] text-emerald-600 flex items-center gap-1 font-bold">
              <span className="material-symbols-outlined text-[12px]">trending_up</span>
              +12%
            </p>
            <p className="text-label-xs text-outline">{totalLikes.toLocaleString()} likes</p>
          </div>
        </div>
      </div>

      {/* Scheduled Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm hover:shadow-md transition-shadow">
        <div className="w-14 h-14 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600">
          <span className="material-symbols-outlined text-[28px]">schedule</span>
        </div>
        <div className="flex-1">
          <p className="text-label-sm text-outline uppercase font-semibold">Scheduled</p>
          <div className="flex items-baseline gap-2">
            <h3 className="text-headline-md text-on-surface">{scheduledCount}</h3>
            <span className="text-[11px] text-outline">posts</span>
          </div>
          <div className="mt-2">
            <p className="text-[11px] text-outline">
              <span className="material-symbols-outlined text-[12px] align-middle mr-1">notifications</span>
              Next: {formatNextSchedule()}
            </p>
          </div>
        </div>
      </div>

      {/* Failed/Draft Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm hover:shadow-md transition-shadow">
        <div className="w-14 h-14 rounded-2xl bg-danger-red/10 flex items-center justify-center text-danger-red">
          <span className="material-symbols-outlined text-[28px]">warning</span>
        </div>
        <div className="flex-1">
          <p className="text-label-sm text-outline uppercase font-semibold">Failed / Draft</p>
          <div className="flex items-baseline gap-2">
            <h3 className="text-headline-md text-on-surface">{failedCount + draftCount}</h3>
            <span className="text-[11px] text-outline">
              ({failedCount} failed, {draftCount} draft)
            </span>
          </div>
          <p className="text-[11px] text-danger-red font-bold mt-2 flex items-center gap-1">
            <span className="material-symbols-outlined text-[12px]">priority_high</span>
            {failedCount > 0 ? "Requires attention" : "All clear"}
          </p>
        </div>
      </div>

      {/* Engagement Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm hover:shadow-md transition-shadow">
        <div className="w-14 h-14 rounded-2xl bg-purple-50 flex items-center justify-center text-purple-600">
          <span className="material-symbols-outlined text-[28px]">trending_up</span>
        </div>
        <div className="flex-1">
          <p className="text-label-sm text-outline uppercase font-semibold">Engagement</p>
          <div className="grid grid-cols-3 gap-1 mt-1">
            <div className="text-center">
              <p className="text-label-md text-on-surface font-bold">{totalLikes.toLocaleString()}</p>
              <p className="text-label-xs text-outline">Likes</p>
            </div>
            <div className="text-center">
              <p className="text-label-md text-on-surface font-bold">{totalComments.toLocaleString()}</p>
              <p className="text-label-xs text-outline">Comments</p>
            </div>
            <div className="text-center">
              <p className="text-label-md text-on-surface font-bold">{totalShares.toLocaleString()}</p>
              <p className="text-label-xs text-outline">Shares</p>
            </div>
          </div>
          <p className="text-label-xs text-outline mt-2 text-center">
            Total across {posts.length} posts
          </p>
        </div>
      </div>
    </div>
  );
}