import Header from "@/components/layout/Header";
import Link from "next/link";

// --- Stat Card ---
function StatCard({
  icon,
  iconBg,
  iconColor,
  label,
  value,
  delta,
  deltaPositive,
}: {
  icon: string;
  iconBg: string;
  iconColor: string;
  label: string;
  value: string;
  delta: string;
  deltaPositive: boolean;
}) {
  return (
    <div className="bg-surface-container-lowest border border-outline-variant/40 rounded-2xl p-6 shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between mb-4">
        <div className={`w-11 h-11 ${iconBg} rounded-xl flex items-center justify-center`}>
          <span className={`material-symbols-outlined ${iconColor} text-[22px]`}>{icon}</span>
        </div>
        <span className={`flex items-center gap-1 text-label-md px-2 py-1 rounded-full ${
          deltaPositive
            ? "bg-success-green/10 text-success-green"
            : "bg-danger-red/10 text-danger-red"
        }`}>
          <span className="material-symbols-outlined text-[14px]">
            {deltaPositive ? "trending_up" : "trending_down"}
          </span>
          {delta}
        </span>
      </div>
      <p className="text-label-md text-on-surface-variant mb-1">{label}</p>
      <p className="text-headline-lg text-on-surface">{value}</p>
    </div>
  );
}

// --- Quick Action ---
function QuickAction({ icon, label, href, isAI }: { icon: string; label: string; href: string; isAI?: boolean }) {
  return (
    <Link
      href={href}
      className={`flex flex-col items-center gap-2 p-4 rounded-2xl border transition-all hover:shadow-md group ${
        isAI
          ? "border-secondary/30 bg-secondary/5 hover:bg-secondary/10"
          : "border-outline-variant/40 bg-surface-container-lowest hover:bg-surface-container-low"
      }`}
    >
      <div className={`w-12 h-12 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform ${
        isAI ? "bg-secondary/10" : "bg-surface-container"
      }`}>
        <span className={`material-symbols-outlined text-[24px] ${isAI ? "text-secondary" : "text-on-surface-variant"}`}>
          {icon}
        </span>
      </div>
      <span className="text-label-md text-on-surface text-center">{label}</span>
      {isAI && (
        <span className="px-1.5 py-0.5 bg-secondary/10 text-secondary rounded-full text-label-sm">AI</span>
      )}
    </Link>
  );
}

// --- Recent Post Row ---
function PostRow({ platform, title, status, time }: { platform: string; title: string; status: "published" | "scheduled" | "draft"; time: string }) {
  const statusMap = {
    published: { label: "Đã đăng", bg: "bg-success-green/10", text: "text-success-green" },
    scheduled: { label: "Đã lên lịch", bg: "bg-primary/10", text: "text-primary" },
    draft: { label: "Bản nháp", bg: "bg-outline-variant/30", text: "text-on-surface-variant" },
  };
  const s = statusMap[status];

  return (
    <div className="flex items-center gap-4 py-3 border-b border-outline-variant/20 last:border-0">
      <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center shrink-0">
        <span className="material-symbols-outlined text-primary text-[18px]">{platform}</span>
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-body-sm font-medium text-on-surface truncate">{title}</p>
        <p className="text-label-sm text-on-surface-variant">{time}</p>
      </div>
      <span className={`shrink-0 px-2.5 py-1 rounded-full text-label-sm font-medium ${s.bg} ${s.text}`}>
        {s.label}
      </span>
    </div>
  );
}

export default function DashboardPage() {
  return (
    <>
      <Header
        breadcrumbs={[{ label: "AISAM" }, { label: "Dashboard" }]}
      />
      <div className="p-gutter space-y-gutter">
        {/* Welcome Banner */}
        <div className="relative bg-enterprise-navy rounded-2xl p-8 overflow-hidden">
          <div className="absolute inset-0 pointer-events-none">
            <div className="absolute top-0 right-0 w-72 h-72 bg-primary/20 rounded-full blur-[80px]" />
            <div className="absolute bottom-0 left-0 w-48 h-48 bg-secondary/20 rounded-full blur-[60px]" />
          </div>
          <div className="relative z-10">
            <p className="text-label-md text-primary-fixed-dim mb-2">Xin chào 👋</p>
            <h2 className="text-headline-lg text-surface-bright mb-2">
              Welcome back, User!
            </h2>
            <p className="text-body-md text-outline-variant max-w-md">
              Bạn có <span className="text-warning-amber font-medium">3 bài</span> đang chờ lên lịch và{" "}
              <span className="text-primary-fixed-dim font-medium">2 nội dung</span> chờ AI hoàn thiện.
            </p>
            <div className="flex gap-3 mt-6">
              <Link href="/ai-studio" className="flex items-center gap-2 bg-secondary text-on-secondary px-5 py-2.5 rounded-xl text-body-sm font-medium hover:bg-secondary-container transition-colors">
                <span className="material-symbols-outlined text-[18px]">auto_awesome</span>
                Mở AI Studio
              </Link>
              <Link href="/campaigns" className="flex items-center gap-2 bg-white/10 text-surface-bright px-5 py-2.5 rounded-xl text-body-sm font-medium hover:bg-white/20 transition-colors border border-white/10">
                <span className="material-symbols-outlined text-[18px]">campaign</span>
                Tạo Campaign
              </Link>
            </div>
          </div>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-gutter">
          <StatCard icon="description" iconBg="bg-primary/10" iconColor="text-primary" label="Tổng nội dung" value="48" delta="+12%" deltaPositive={true} />
          <StatCard icon="send" iconBg="bg-success-green/10" iconColor="text-success-green" label="Bài đã đăng" value="124" delta="+8%" deltaPositive={true} />
          <StatCard icon="schedule" iconBg="bg-warning-amber/10" iconColor="text-warning-amber" label="Đã lên lịch" value="7" delta="+3" deltaPositive={true} />
          <StatCard icon="auto_awesome" iconBg="bg-secondary/10" iconColor="text-secondary" label="AI Generations" value="312" delta="-5%" deltaPositive={false} />
        </div>

        {/* Main 2-col grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter">
          {/* Recent Posts */}
          <div className="lg:col-span-2 bg-surface-container-lowest border border-outline-variant/40 rounded-2xl p-6 shadow-sm">
            <div className="flex items-center justify-between mb-5">
              <h3 className="text-headline-sm text-on-surface">Bài đăng gần đây</h3>
              <Link href="/content" className="text-label-md text-primary hover:text-primary-container transition-colors flex items-center gap-1">
                Xem tất cả
                <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
              </Link>
            </div>
            <div>
              <PostRow platform="public" title="Giới thiệu sản phẩm mới Q3 2024 — chiến dịch hè" status="published" time="Hôm nay, 14:30" />
              <PostRow platform="campaign" title="Khuyến mãi Flash Sale 50% — Chỉ hôm nay" status="scheduled" time="Ngày mai, 09:00" />
              <PostRow platform="edit_note" title="Bài viết Blog: Xu hướng Marketing 2025" status="draft" time="02/06/2024" />
              <PostRow platform="public" title="Tổng kết tháng 5 — Thành tích và Số liệu" status="published" time="01/06/2024" />
              <PostRow platform="campaign" title="Launch sản phẩm mùa hè — Brand Awareness" status="scheduled" time="05/06/2024" />
            </div>
          </div>

          {/* Quick Actions + Quota */}
          <div className="space-y-gutter">
            {/* Quick Actions */}
            <div className="bg-surface-container-lowest border border-outline-variant/40 rounded-2xl p-6 shadow-sm">
              <h3 className="text-headline-sm text-on-surface mb-4">Hành động nhanh</h3>
              <div className="grid grid-cols-2 gap-3">
                <QuickAction href="/ai-studio" icon="auto_awesome" label="Tạo bằng AI" isAI />
                <QuickAction href="/content" icon="add_circle" label="Nội dung mới" />
                <QuickAction href="/scheduling" icon="schedule" label="Lên lịch đăng" />
                <QuickAction href="/brands" icon="workspaces" label="Quản lý Brand" />
              </div>
            </div>

            {/* Quota Card */}
            <div className="bg-surface-container-lowest border border-outline-variant/40 rounded-2xl p-6 shadow-sm">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-headline-sm text-on-surface">Quota sử dụng</h3>
                <span className="px-2.5 py-1 bg-primary/10 text-primary rounded-full text-label-sm">Free Plan</span>
              </div>
              <div className="space-y-4">
                {/* AI Prompts */}
                <div>
                  <div className="flex justify-between mb-1.5">
                    <span className="text-body-sm text-on-surface-variant">AI Prompts</span>
                    <span className="text-label-md text-on-surface">68 / 100</span>
                  </div>
                  <div className="h-2 bg-surface-container rounded-full overflow-hidden">
                    <div className="h-full bg-secondary rounded-full w-[68%] transition-all" />
                  </div>
                </div>
                {/* Posts */}
                <div>
                  <div className="flex justify-between mb-1.5">
                    <span className="text-body-sm text-on-surface-variant">Bài đăng</span>
                    <span className="text-label-md text-on-surface">24 / 30</span>
                  </div>
                  <div className="h-2 bg-surface-container rounded-full overflow-hidden">
                    <div className="h-full bg-warning-amber rounded-full w-[80%] transition-all" />
                  </div>
                </div>
              </div>
              <Link href="/pricing" className="mt-5 w-full flex items-center justify-center gap-2 bg-primary-container text-on-primary-container px-4 py-2.5 rounded-xl text-label-md hover:bg-primary transition-colors">
                <span className="material-symbols-outlined text-[18px]">rocket_launch</span>
                Nâng cấp gói
              </Link>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
