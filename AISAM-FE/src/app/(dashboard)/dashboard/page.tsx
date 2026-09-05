"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useAccessContext } from "@/contexts/AccessContext";
import Header from "@/components/layout/Header";
import { useWorkspaces, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { fetchCreditWallet, fetchPostQuota, fetchWorkspaceDashboard, type WorkspaceDashboard } from "@/services/workspaceService";
import { fetchUpcomingSchedules, onScheduleChange, ScheduleItem } from "@/services/scheduleService";
import { fetchCampaigns, type Campaign } from "@/services/campaignService";
import { fetchPost, type PostItem } from "@/services/postService";
import PostDetailModal from "@/components/posts/PostDetailModal";
import { PLATFORM_CONFIG, PlatformIcon } from "@/lib/contentConstants";
import { apiFetch } from "@/lib/apiClient";
import { fetchChannelBreakdown, fetchTopPosts, type ChannelBreakdownItem, type TopPostItem } from "@/services/analyticsService";
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";
import { HolidayBanner } from "@/components/holiday/HolidayBanner";

function CountUp({ value, suffix = "", duration = 1500 }: { value: string; suffix?: string; duration?: number }) {
  const num = parseFloat(value.replace(/[^0-9.]/g, ""));
  const isDecimal = value.includes(".");
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting) {
        const start = performance.now();
        const step = (now: number) => {
          const pct = Math.min((now - start) / duration, 1);
          const eased = 1 - Math.pow(1 - pct, 3);
          setCount(eased * num);
          if (pct < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
        observer.disconnect();
      }
    }, { threshold: 0.3 });
    observer.observe(el);
    return () => observer.disconnect();
  }, [num, duration]);

  return <span ref={ref}>{isDecimal ? count.toFixed(1) : Math.round(count)}{suffix}</span>;
}

function AnimatedBar({ value, color, delay = 600 }: { value: number; color: string; delay?: number }) {
  const ref = useRef<HTMLDivElement>(null);
  const displayPct = Math.max(value, value > 0 ? 2 : 0);
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const timer = setTimeout(() => { el.style.width = `${displayPct}%`; }, delay);
    return () => clearTimeout(timer);
  }, [displayPct, delay]);
  return (
    <div className="w-full h-2 bg-surface-container/60 rounded-full overflow-hidden">
      <div ref={ref} className={`h-full ${color} rounded-full transition-all duration-1000 ease-out`} style={{ width: "0%" }} />
    </div>
  );
}

function DrawSVG({ children }: { children: React.ReactNode }) {
  return <g className="draw-svg">{children}</g>;
}

function formatScheduleDate(dateStr: string) {
  const d = new Date(dateStr);
  const now = new Date();
  const tomorrow = new Date(now);
  tomorrow.setDate(tomorrow.getDate() + 1);
  const timeStr = d.toLocaleTimeString("en-US", { hour: "numeric", minute: "2-digit" });
  if (d.toDateString() === now.toDateString()) return `Today, ${timeStr}`;
  if (d.toDateString() === tomorrow.toDateString()) return `Tomorrow, ${timeStr}`;
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" }) + `, ${timeStr}`;
}

const PLATFORM_DISPLAY: Record<string, { color: string; bg: string }> = {
  facebook: { color: "text-blue-600", bg: "bg-blue-100" },
  instagram: { color: "text-pink-600", bg: "bg-pink-100" },
  tiktok: { color: "text-white", bg: "bg-neutral-900" },
};

const RANK_GRADIENTS = [
  "from-amber-400 to-orange-500",
  "from-slate-400 to-slate-500",
  "from-amber-700 to-amber-800",
];

export default function DashboardPage() {
  const { activeWorkspace } = useWorkspaces();
  const canViewBilling = useAccessContext()?.role === "Owner";
  const workspaceName = activeWorkspace?.name || "User";
  const [visible, setVisible] = useState(false);
  const [scheduleItems, setScheduleItems] = useState<ScheduleItem[]>([]);
  const [creditBalance, setCreditBalance] = useState<number | null>(null);
  const [maxCreditBalance, setMaxCreditBalance] = useState<number | null>(null);
  const [postQuota, setPostQuota] = useState<{ used: number; total: number } | null>(null);
  const [dashboard, setDashboard] = useState<WorkspaceDashboard | null>(null);
  const [dailyUsage, setDailyUsage] = useState<{ date: string; credits: number }[]>([]);
  const [usageDays, setUsageDays] = useState(7);
  const [dashboardCampaigns, setDashboardCampaigns] = useState<Campaign[]>([]);
  const [platformBreakdown, setPlatformBreakdown] = useState<ChannelBreakdownItem[]>([]);
  const [topPostsByPlatform, setTopPostsByPlatform] = useState<Record<string, TopPostItem[]>>({});
  const [detailPost, setDetailPost] = useState<PostItem | null>(null);
  const [loadingPostId, setLoadingPostId] = useState<string | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 100);
    return () => clearTimeout(timer);
  }, []);

  const loadSchedules = () => {
    fetchUpcomingSchedules(6).then(setScheduleItems);
  };

  useEffect(() => {
    loadSchedules();
    const unsubscribe = onScheduleChange(loadSchedules);
    return unsubscribe;
  }, []);

  useEffect(() => {
    let disposed = false;
    if (canViewBilling) {
      fetchCreditWallet().then(w => { if (!disposed && w) { setCreditBalance(w.balance); setMaxCreditBalance(w.maxBalance); } });
      fetchPostQuota().then(q => { if (!disposed && q) setPostQuota(q); });
    }
    fetchWorkspaceDashboard().then(d => { if (d) setDashboard(d); });
    fetchCampaigns({ pageSize: 5 }).then((res) => {
      if (res) setDashboardCampaigns(res.data.slice(0, 5));
    });
    fetchChannelBreakdown("90d").then(d => { setPlatformBreakdown(d); }).catch(() => setPlatformBreakdown([]));
    return () => { disposed = true; };
  }, [activeWorkspace?.id, canViewBilling]);

  useEffect(() => {
    const platforms = ["facebook", "instagram", "tiktok"];
    const fetchTop = async () => {
      const results: Record<string, TopPostItem[]> = {};
      await Promise.all(platforms.map(async (p) => {
        results[p] = await fetchTopPosts("90d", "impressions", p, 3);
      }));
      setTopPostsByPlatform(results);
    };
    fetchTop();
    const interval = setInterval(fetchTop, 30000);
    return () => clearInterval(interval);
  }, [activeWorkspace?.id]);

  useEffect(() => {
    apiFetch(`/credit-usage/daily-summary?days=${usageDays}`).then(res => {
      if (res?.success && res.data) {
        setDailyUsage((res.data as { date: string; totalCredits: number }[]).map(d => ({
          date: d.date,
          credits: d.totalCredits,
        })));
      }
    });
  }, [usageDays, activeWorkspace?.id]);

  return (
    <>
      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(24px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes fade-right {
          from { opacity: 0; transform: translateX(-16px); }
          to   { opacity: 1; transform: translateX(0); }
        }
        @keyframes scale-in {
          from { opacity: 0; transform: scale(0.92); }
          to   { opacity: 1; transform: scale(1); }
        }
        @keyframes draw-line {
          from { stroke-dashoffset: 2000; }
          to   { stroke-dashoffset: 0; }
        }
        @keyframes float-y {
          0%, 100% { transform: translateY(0); }
          50%      { transform: translateY(-6px); }
        }
        @keyframes glow-pulse {
          0%, 100% { box-shadow: 0 0 8px rgba(15, 98, 254, 0.15); }
          50%      { box-shadow: 0 0 20px rgba(15, 98, 254, 0.3); }
        }
        @keyframes slide-up-row {
          from { opacity: 0; transform: translateY(12px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes bar-grow {
          from { height: 0; }
        }
        @keyframes shimmer {
          0% { background-position: -200% 0; }
          100% { background-position: 200% 0; }
        }
        @keyframes border-dance {
          0%, 100% { border-color: rgba(15, 98, 254, 0.1); }
          50% { border-color: rgba(15, 98, 254, 0.3); }
        }
        .animate-fade-up { animation: fade-up 0.6s ease-out forwards; opacity: 0; }
        .animate-fade-right { animation: fade-right 0.5s ease-out forwards; opacity: 0; }
        .animate-scale-in { animation: scale-in 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float-y 3s ease-in-out infinite; }
        .animate-glow-pulse { animation: glow-pulse 2s ease-in-out infinite; }
        .chart-line { stroke-dasharray: 2000; animation: draw-line 2.5s ease-out forwards; }
        .chart-fill { animation: fade-up 1.5s ease-out 0.6s forwards; opacity: 0; }
        .card-hover {
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        .card-hover:hover {
          transform: translateY(-4px);
          box-shadow: 0 16px 48px rgba(0, 0, 0, 0.08);
        }
        .shimmer-bg {
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.03), transparent);
          background-size: 200% 100%;
          animation: shimmer 3s ease-in-out infinite;
        }
      `}</style>

      <Header />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-8">
        {/* ===== HERO CARD ===== */}
        <div className={`relative overflow-hidden rounded-2xl bg-gradient-to-br from-primary/[0.12] via-surface to-surface-container border border-primary/20 shadow-sm ${visible ? "animate-scale-in" : ""}`} style={{ animationDelay: "0s" }}>
          <div className="absolute top-0 right-0 w-96 h-96 bg-gradient-to-bl from-primary/20 via-primary/[0.08] to-transparent rounded-full blur-3xl pointer-events-none" />
          <div className="absolute bottom-0 left-0 w-64 h-64 bg-gradient-to-tr from-secondary/[0.08] via-transparent to-transparent rounded-full blur-3xl pointer-events-none" />
          <div className="absolute inset-x-0 top-0 h-0.5 bg-gradient-to-r from-transparent via-primary/50 to-transparent" />

          <div className="relative z-10 p-8">
            <div className="flex items-start justify-between gap-8">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-4">
                  <span className="px-3 py-1 bg-primary/10 text-primary rounded-full text-label-sm font-semibold inline-flex items-center gap-1.5">
                    <span className="w-1.5 h-1.5 bg-primary rounded-full animate-pulse" />
                    Dashboard Overview
                  </span>
                  <span className="text-label-sm text-outline">{new Date().toLocaleDateString("en-US", { weekday: "long", month: "long", day: "numeric" })}</span>
                </div>

                <h2 className="text-headline-lg text-on-surface tracking-tight mb-2">
                  {(() => {
                    const h = new Date().getHours();
                    if (h < 12) return "Good morning";
                    if (h < 18) return "Good afternoon";
                    return "Good evening";
                  })()}, <span className="text-primary">{workspaceName}</span>!
                </h2>
                <p className="text-body-lg text-on-surface-variant max-w-xl mb-6">Here&apos;s your brand performance snapshot. Everything you need is right here.</p>

                <div className="flex flex-wrap items-center gap-4">
                  <div className="flex items-center gap-3 bg-surface-container rounded-xl px-4 py-2.5">
                    <div className="flex -space-x-1.5">
                      <div className="w-8 h-8 rounded-full bg-[#1877F2] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0s" }}>
                        <PlatformIcon platform="facebook" className="w-4 h-4" />
                      </div>
                      <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-[#F58529] via-[#DD2A7B] to-[#8134AF] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0.15s" }}>
                        <PlatformIcon platform="instagram" className="w-4 h-4" />
                      </div>
                      <div className="w-8 h-8 rounded-full bg-[#111111] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0.3s" }}>
                        <PlatformIcon platform="tiktok" className="w-4 h-4" />
                      </div>
                    </div>
                    <span className="text-label-sm text-on-surface-variant">3 platforms connected</span>
                  </div>
                  <div className="flex items-center gap-2.5 bg-surface-container rounded-xl px-4 py-2.5">
                    <span className="material-symbols-outlined text-success-green text-[18px]">check_circle</span>
                    <div>
                      <span className="text-label-sm text-on-surface-variant">Status</span>
                      <p className="text-label-md text-success-green font-semibold">All systems nominal</p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="hidden lg:flex flex-col items-center gap-3 shrink-0">
                <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-primary/10 to-primary/5 border border-outline-variant/30 flex items-center justify-center shadow-sm">
                  <span className="text-[28px] font-bold text-primary">
                    {workspaceName.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2)}
                  </span>
                </div>
                <div className="text-center">
                  <p className="text-body-sm text-on-surface font-semibold">{workspaceName}</p>
                  <p className="text-label-sm text-outline mb-2">{activeWorkspace ? getWorkspaceTypeLabel(activeWorkspace.workspaceType) : "Workspace"}</p>
                  <Link
                    href="/overview"
                    className="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 bg-primary/10 text-primary rounded-lg text-label-xs font-semibold hover:bg-primary/20 transition-all"
                  >
                    <span className="material-symbols-outlined text-[14px]">dashboard</span>
                    Overview
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* ===== HOLIDAY BANNER ===== */}
        {/* <HolidayBanner onSuccess={() => {
          // Could refresh content list if we had one on this page, but dashboard doesn't need to refresh just yet
          alert("Caption suggested and saved to Content list for approval!");
        }} /> */}

        {/* ===== KPI GRID ===== */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter">
          {[
            { icon: "check_circle", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", label: "Published", value: String(dashboard?.publishedPostCount ?? 0), delta: null, deltaUp: null, gradient: "from-emerald-500/5 to-transparent", accent: "#10b981" },
            { icon: "auto_awesome", iconBg: "from-blue-500/20 to-blue-600/10", iconColor: "text-blue-500", label: "AI Usage", value: String(dashboard?.aiUsageCount ?? 0), delta: null, deltaUp: null, gradient: "from-blue-500/5 to-transparent", accent: "#3b82f6" },
            { icon: "token", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", label: "AI Credits", value: String(creditBalance ?? 0), delta: null, deltaUp: null, max: maxCreditBalance ? String(maxCreditBalance) : undefined, pct: maxCreditBalance ? Math.min(100, Math.round(((creditBalance ?? 0) / maxCreditBalance) * 100)) : 0, gradient: "from-emerald-500/5 to-transparent", accent: "#10b981" },
            { icon: "send", iconBg: "from-amber-500/20 to-amber-600/10", iconColor: "text-amber-500", label: "Posts This Month", value: String(postQuota?.used ?? (dashboard ? ((dashboard.postQuotaLimit ?? 0) - (dashboard.postsRemaining ?? 0)) : 0)), delta: null, deltaUp: null, max: String(postQuota?.total ?? dashboard?.postQuotaLimit ?? 1000), pct: Math.min(100, postQuota ? Math.round((postQuota.used / postQuota.total) * 100) : dashboard ? Math.round((((dashboard.postQuotaLimit ?? 0) - (dashboard.postsRemaining ?? 0)) / (dashboard.postQuotaLimit || 1)) * 100) : 0), gradient: "from-amber-500/5 to-transparent", accent: "#f59e0b" },
          ].filter(kpi => canViewBilling || !["AI Credits", "Posts This Month"].includes(kpi.label)).map((kpi, i) => (
            <div
              key={kpi.label}
              className={`relative bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden group ${visible ? "animate-fade-up" : ""} card-hover`}
              style={{ animationDelay: `${0.08 * i}s` }}
            >
              <div className={`absolute inset-0 bg-gradient-to-br ${kpi.gradient} pointer-events-none`} />
              <div className="absolute inset-x-0 top-0 h-0.5 scale-x-0 group-hover:scale-x-100 transition-transform duration-500" style={{ background: `linear-gradient(90deg, transparent, ${kpi.accent}, transparent)` }} />
              <div className="relative p-6">
                <div className="flex items-start justify-between mb-4">
                  <div className={`w-11 h-11 rounded-xl bg-gradient-to-br ${kpi.iconBg} flex items-center justify-center ${kpi.iconColor} group-hover:scale-110 transition-transform duration-300`}>
                    <span className="material-symbols-outlined text-[22px]">{kpi.icon}</span>
                  </div>
                  {kpi.delta !== null && (
                    <span className={`flex items-center gap-1 text-label-sm px-2 py-1 rounded-full font-semibold ${kpi.deltaUp === true ? "bg-emerald-50 text-emerald-600" :
                        kpi.deltaUp === false ? "bg-red-50 text-red-500" :
                          "bg-surface-container-high text-on-surface-variant"
                      }`}>
                      {kpi.deltaUp === true && <span className="material-symbols-outlined text-[14px]">trending_up</span>}
                      {kpi.deltaUp === false && <span className="material-symbols-outlined text-[14px]">trending_down</span>}
                      {kpi.delta}
                    </span>
                  )}
                </div>
                <p className="text-label-sm text-on-surface-variant mb-1.5 font-medium">{kpi.label}</p>
                <div className="flex items-baseline gap-2">
                  <h3 className="text-kpi-lg text-on-surface">
                    <CountUp value={kpi.value} />
                  </h3>
                  {kpi.max && <span className="text-label-md text-outline">/ {kpi.max}</span>}
                </div>
                {kpi.pct !== undefined && (
                  <div className="mt-3">
                    <AnimatedBar value={kpi.pct} color="bg-gradient-to-r from-emerald-400 to-emerald-500" delay={800 + i * 100} />
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* ===== CHART + UPCOMING ===== */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter">
          {/* Daily Credit Usage Chart */}
          <div className={`lg:col-span-2 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.32s" }}>
            <div className="flex items-center justify-between mb-6">
              <div>
                <div className="flex items-center gap-2 mb-1">
                  <h4 className="text-headline-sm text-on-surface">Daily Credit Usage</h4>
                  <span className="px-2 py-0.5 bg-emerald-500/10 text-emerald-600 rounded-full text-label-xs font-semibold">
                    Total: {dailyUsage.reduce((s, d) => s + d.credits, 0).toLocaleString()}
                  </span>
                </div>
                <p className="text-body-sm text-on-surface-variant">AI credits consumed per day by your team</p>
              </div>
              <div className="flex items-center gap-1.5 bg-surface-container rounded-lg p-1">
                {[7, 30, 90].map(d => (
                  <button key={d} onClick={() => setUsageDays(d)}
                    className={`px-3 py-1.5 rounded-md text-label-sm font-semibold transition-all ${usageDays === d ? "bg-white text-on-surface shadow-sm" : "text-on-surface-variant hover:text-on-surface"}`}>
                    {d}D
                  </button>
                ))}
              </div>
            </div>
            <div className="h-72 w-full">
              {dailyUsage.length > 0 ? (
                <ResponsiveContainer width="100%" height={260}>
                  <AreaChart data={dailyUsage} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
                    <defs>
                      <linearGradient id="creditGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="#0f62fe" stopOpacity={0.3} />
                        <stop offset="100%" stopColor="#0f62fe" stopOpacity={0.02} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e0e0e0" strokeOpacity={0.12} vertical={false} />
                    <XAxis dataKey="date" tick={{ fontSize: 11, fill: "#888" }} tickLine={false} axisLine={false}
                      tickFormatter={(v: string) => {
                        const d = new Date(v);
                        return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
                      }} />
                    <YAxis tick={{ fontSize: 11, fill: "#888" }} tickLine={false} axisLine={false} />
                    <Tooltip contentStyle={{ borderRadius: "12px", border: "1px solid rgba(0,0,0,0.08)", boxShadow: "0 8px 24px rgba(0,0,0,0.08)" }}
                      labelFormatter={(label) => new Date(String(label)).toLocaleDateString("en-US", { weekday: "long", month: "long", day: "numeric" })}
                      formatter={(value: any) => [`${Number(value).toLocaleString()} credits used`, "Credits"]} />
                    <Area type="monotone" dataKey="credits" stroke="#0f62fe" strokeWidth={2.5} fill="url(#creditGrad)" dot={false} activeDot={{ r: 5, fill: "#0f62fe", stroke: "#fff", strokeWidth: 2 }}
                      animationBegin={200} animationDuration={1200} animationEasing="ease-out" />
                  </AreaChart>
                </ResponsiveContainer>
              ) : (
                <div className="h-full flex items-center justify-center text-outline text-body-sm">No credit usage data</div>
              )}
            </div>
          </div>

          {/* Upcoming */}
          <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 flex flex-col ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.4s" }}>
            <div className="flex items-center justify-between mb-5">
              <div className="flex items-center gap-2">
                <h4 className="text-headline-sm text-on-surface">Schedule</h4>
                <span className="px-1.5 py-0.5 bg-amber-50 text-amber-600 rounded-md text-label-xs font-semibold">{scheduleItems.length}</span>
              </div>
              <Link href="/calendar" className="text-label-sm text-primary font-semibold hover:text-primary-container transition-colors flex items-center gap-1 group">
                View All
                <span className="material-symbols-outlined text-[14px] group-hover:translate-x-0.5 transition-transform">arrow_forward</span>
              </Link>
            </div>
            <div className="space-y-2 flex-1">
              {scheduleItems.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-8 text-center">
                  <span className="material-symbols-outlined text-outline/40 text-[32px] mb-2">calendar_month</span>
                  <p className="text-body-sm text-outline">No upcoming schedules</p>
                </div>
              ) : scheduleItems.map((post, i) => {
                const pConfig = PLATFORM_CONFIG[post.platform || "facebook"];
                const pDisplay = PLATFORM_DISPLAY[post.platform || "facebook"] || { color: "text-outline", bg: "bg-surface-container-high" };
                return (
                  <div
                    key={post.id}
                    className="group flex items-center gap-3 p-2.5 rounded-xl hover:bg-surface-container hover:shadow-sm transition-all duration-200 cursor-pointer"
                    style={{ animation: `slide-up-row 0.4s ease-out ${0.42 + i * 0.06}s forwards`, opacity: 0 }}
                  >
                    <div className={`w-10 h-10 rounded-xl ${pDisplay.bg} flex items-center justify-center shrink-0 group-hover:scale-110 group-hover:rotate-3 transition-all duration-300`}>
                      {pConfig ? (
                        <PlatformIcon platform={post.platform || "facebook"} className="w-[18px] h-[18px]" />
                      ) : (
                        <span className={`text-label-sm font-bold ${pDisplay.color}`}>{(post.platform || "?").slice(0, 2).toUpperCase()}</span>
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-body-sm font-medium text-on-surface truncate group-hover:text-primary transition-colors">{post.title || "Untitled"}</p>
                      <p className="text-label-sm text-outline">{formatScheduleDate(post.scheduledAt)}</p>
                    </div>
                    <span className={`px-2 py-0.5 rounded text-label-2xs font-semibold tracking-wide ${post.status === "Completed" ? "bg-emerald-50 text-emerald-600" :
                        post.status === "Failed" ? "bg-red-50 text-red-500" :
                          "bg-amber-50 text-amber-600"
                      }`}>
                      {post.status}
                    </span>
                  </div>
                );
              })}
            </div>
            <div className="mt-4 pt-4 border-t border-outline-variant/20">
              <div className="p-3.5 bg-gradient-to-r from-purple-500/[0.06] to-purple-500/[0.02] rounded-xl border border-purple-500/10 hover:border-purple-500/20 transition-colors">
                <div className="flex items-center gap-2 mb-1.5">
                  <span className="material-symbols-outlined text-purple-500 text-[16px] animate-float">auto_awesome</span>
                  <span className="text-label-sm text-purple-600 font-semibold">AI Insight</span>
                </div>
                <p className="text-label-sm leading-relaxed text-on-surface-variant">
                  Your audience is <span className="text-on-surface font-semibold">24% more active</span> at 8:00 PM on Sundays. Consider rescheduling.
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* ===== RECENT CAMPAIGNS ===== */}
        <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.48s" }}>
          <div className="px-6 py-5 border-b border-outline-variant/20 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h4 className="text-headline-sm text-on-surface">Recent Campaigns</h4>
              <span className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-label-xs font-semibold">{dashboardCampaigns.length} campaigns</span>
            </div>
            <button onClick={() => {
              const csv = ["Name,Objective,Budget,Spend,Status"];
              dashboardCampaigns.forEach((c) => csv.push(`"${c.name}","${c.objective}","$${c.budget || 0}","$${c.spend}","${c.status}"`));
              const blob = new Blob([csv.join("\n")], { type: "text/csv" });
              const url = URL.createObjectURL(blob);
              const a = document.createElement("a"); a.href = url; a.download = "campaigns-export.csv"; a.click();
              URL.revokeObjectURL(url);
            }} className="flex items-center gap-1.5 text-label-sm text-primary font-semibold hover:text-primary-container transition-colors group">
              <span className="material-symbols-outlined text-[16px] group-hover:scale-110 transition-transform">download</span>
              Export CSV
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="text-label-sm text-outline border-b border-outline-variant/10">
                  <th className="px-6 py-3.5 font-semibold">Campaign Name</th>
                  <th className="px-6 py-3.5 font-semibold">Objective</th>
                  <th className="px-6 py-3.5 font-semibold">Budget</th>
                  <th className="px-6 py-3.5 font-semibold">Spent</th>
                  <th className="px-6 py-3.5 font-semibold">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-outline-variant/10">
                {dashboardCampaigns.map((row, i) => (
                  <tr
                    key={row.id}
                    className="group hover:bg-surface-container/40 transition-colors duration-150"
                    style={{ animation: `slide-up-row 0.4s ease-out ${0.5 + i * 0.08}s forwards`, opacity: 0 }}
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center group-hover:scale-110 group-hover:bg-primary/10 transition-all duration-300">
                          <span className="material-symbols-outlined text-outline group-hover:text-primary text-[16px] transition-colors">campaign</span>
                        </div>
                        <span className="text-body-sm font-medium text-on-surface group-hover:text-primary transition-colors">{row.name}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 rounded-lg text-label-xs font-bold tracking-wide inline-block hover:scale-105 transition-transform ${row.objective === "SALES" ? "bg-blue-50 text-blue-600" :
                          row.objective === "AWARENESS" ? "bg-purple-50 text-purple-600" :
                            row.objective === "TRAFFIC" ? "bg-orange-50 text-orange-600" :
                              row.objective === "LEADS" ? "bg-emerald-50 text-emerald-600" :
                                "bg-surface-container-high text-on-surface-variant"
                        }`}>{row.objective}</span>
                    </td>
                    <td className="px-6 py-4 text-body-sm text-on-surface font-medium">${row.budget?.toLocaleString() || "0"}</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <span className="text-body-sm text-on-surface font-medium">{row.spend == null ? "—" : `$${row.spend.toLocaleString()}`}</span>
                        {row.budget && row.budget > 0 && row.spend != null && (
                          <span className="text-label-sm text-outline">({Math.round(row.spend / row.budget * 100)}%)</span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-sm font-semibold ${row.status === "ACTIVE" ? "bg-emerald-50 text-emerald-600" :
                          row.status === "COMPLETED" ? "bg-blue-50 text-blue-600" :
                            "bg-surface-container-high text-on-surface-variant"
                        }`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${row.status === "ACTIVE" ? "bg-emerald-500 animate-pulse" : "bg-outline"}`} />
                        {row.status === "ACTIVE" ? "Active" : row.status === "COMPLETED" ? "Completed" : row.status === "PAUSED" ? "Paused" : "Draft"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* ===== BOTTOM GRID ===== */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
          {(["facebook", "instagram", "tiktok"] as const).map((platform, i) => {
            const posts = topPostsByPlatform[platform] || [];
            const config = PLATFORM_CONFIG[platform];
            return (
              <div
                key={platform}
                className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""} card-hover`}
                style={{ animationDelay: `${0.56 + 0.08 * i}s` }}
              >
                <div className="flex items-center gap-2.5 mb-5">
                  <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-surface-container-highest to-surface-container flex items-center justify-center">
                    <PlatformIcon platform={platform} className="w-[20px] h-[20px]" />
                  </div>
                  <h5 className="text-label-md text-on-surface font-semibold">{config.label}</h5>
                </div>
                {posts.length === 0 ? (
                  <p className="text-label-sm text-outline italic">No posts yet</p>
                ) : (
                  <div className="space-y-3">
                    {posts.map((post, j) => {
                      const isLoading = loadingPostId === post.postId;
                      return (
                      <div
                        key={post.postId}
                        className={`group relative p-3 rounded-xl border transition-all duration-300 cursor-pointer ${
                          isLoading
                            ? "bg-primary/5 border-primary/20 animate-pulse"
                            : "bg-surface-container/50 border-outline-variant/10 hover:bg-surface-container hover:border-outline-variant/30 hover:shadow-md hover:-translate-y-0.5"
                        }`}
                        onClick={async () => {
                          if (isLoading) return;
                          setLoadingPostId(post.postId);
                          const detail = await fetchPost(post.postId);
                          if (detail) setDetailPost(detail);
                          setLoadingPostId(null);
                        }}
                      >
                        <div className="flex items-center gap-2.5">
                          <div className={`w-6 h-6 rounded-md bg-gradient-to-br ${RANK_GRADIENTS[j]} flex items-center justify-center shrink-0 group-hover:scale-110 transition-transform duration-300`}>
                            <span className="text-[11px] font-bold text-white">#{j + 1}</span>
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-label-sm text-on-surface font-medium truncate group-hover:text-primary transition-colors duration-300">
                              {post.contentTitle || "Untitled"}
                            </p>
                            <div className="flex items-center gap-3 text-label-xs text-outline mt-1">
                              <span className="flex items-center gap-1 group-hover:text-blue-500 transition-colors duration-300">
                                <span className="material-symbols-outlined text-[12px]">visibility</span>
                                {post.impressions.toLocaleString()}
                              </span>
                              <span className="flex items-center gap-1 group-hover:text-pink-500 transition-colors duration-300">
                                <span className="material-symbols-outlined text-[12px]">favorite</span>
                                {post.engagement.toLocaleString()}
                              </span>
                            </div>
                          </div>
                        </div>
                      </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {/* ===== PLATFORM DISTRIBUTION ===== */}
        <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.96s" }}>
          <div className="flex items-center gap-2 mb-6">
            <h4 className="text-headline-sm text-on-surface">Platform Distribution</h4>
            <span className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-label-xs font-semibold">{platformBreakdown.length > 0 ? `${platformBreakdown.filter(p => p.publishedPosts > 0).length} active` : "No data"}</span>
          </div>
          {(() => {
            const barColor: Record<string, string> = {
              facebook: "from-blue-500 to-blue-400",
              instagram: "from-purple-500 to-purple-400",
              tiktok: "from-amber-500 to-amber-400",
              twitter: "from-cyan-500 to-cyan-400",
              google: "from-red-500 to-red-400",
              youtube: "from-red-600 to-red-500",
            };
            const defaultPlatforms = ["facebook", "instagram", "tiktok"];
            const items = platformBreakdown.length > 0
              ? defaultPlatforms.map(p => platformBreakdown.find(b => b.platform === p) || { platform: p, publishedPosts: 0, impressions: 0, reach: 0, engagement: 0, clicks: 0, ctr: 0, spend: 0 })
              : defaultPlatforms.map(p => ({ platform: p, publishedPosts: 0, impressions: 0, reach: 0, engagement: 0, clicks: 0, ctr: 0, spend: 0 }));
            const maxPosts = Math.max(...items.map(p => p.publishedPosts), 1);
            const totalPosts = items.reduce((s, p) => s + p.publishedPosts, 0);

            if (totalPosts === 0) {
              return (
                <div className="h-48 flex flex-col items-center justify-center text-center gap-3">
                  <span className="material-symbols-outlined text-outline/30 text-[40px]">bar_chart</span>
                  <p className="text-body-sm text-outline">No posts published yet</p>
                  <p className="text-label-sm text-outline/60">Data will appear once content is published to social platforms</p>
                </div>
              );
            }

            return (
          <div className="h-72 flex items-end justify-evenly gap-10 px-4 pb-2 border-b border-outline-variant/20">
            {items.map((item, i) => (
              <div key={i} className="w-14 flex flex-col items-center gap-2 group" style={{ height: "100%" }}>
                <span className="text-label-xs text-outline font-semibold opacity-0 group-hover:opacity-100 transition-opacity">{item.publishedPosts} posts</span>
                <div className="w-full flex-1 flex flex-col justify-end">
                  <div
                    className={`w-full bg-gradient-to-t ${barColor[item.platform] || "from-outline to-outline-variant"} rounded-t-lg transition-all duration-1000 group-hover:rounded-t-xl`}
                    style={{
                      height: visible ? `${Math.max(Math.round((item.publishedPosts / maxPosts) * 100), 4)}%` : "0%",
                      opacity: visible ? 1 : 0,
                    }}
                  />
                </div>
                <PlatformIcon platform={item.platform} className="w-[18px] h-[18px]" />
                <span className="text-label-sm text-outline font-medium group-hover:text-on-surface transition-colors capitalize">{item.platform}</span>
              </div>
            ))}
          </div>
            );
          })()}
        </div>

        {/* ===== FOOTER ===== */}
        <footer className="flex items-center justify-between text-outline/50">
          <div className="flex items-center gap-5">
            <span className="text-label-sm font-mono">v1.4.2-alpha</span>
            <span className="w-1 h-1 rounded-full bg-outline/20" />
            <span className="text-label-sm flex items-center gap-1.5">
              <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-pulse" />
              All systems operational
            </span>
          </div>
          <div className="flex items-center gap-1 text-label-sm font-mono">
            <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full" />
            API
          </div>
        </footer>
      </main>
      {detailPost && (
        <PostDetailModal post={detailPost} onClose={() => setDetailPost(null)} />
      )}
    </>
  );
}
