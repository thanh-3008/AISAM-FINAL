"use client";

import { Suspense, useState, useEffect, useRef } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import Header from "@/components/layout/Header";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useAccessContext } from "@/contexts/AccessContext";
import {
  fetchAnalytics,
  fetchTopPosts,
  type AnalyticsData,
  type DateRange,
  type TopPostItem,
} from "@/services/analyticsService";
import { fetchBrands } from "@/services/brandService";
import AnalyticsKpiCards from "@/components/analytics/AnalyticsKpiCards";
import AnalyticsFilterBar from "@/components/analytics/AnalyticsFilterBar";
import AnalyticsChart from "@/components/analytics/AnalyticsChart";
import AnalyticsPerformanceTable from "@/components/analytics/AnalyticsPerformanceTable";
import AnalyticsTopPosts from "@/components/analytics/AnalyticsTopPosts";
import AnalyticsAiInsights from "@/components/analytics/AnalyticsAiInsights";
import AnalyticsEfficiencyCard from "@/components/analytics/AnalyticsEfficiencyCard";
import PersonalAnalyticsView from "@/components/analytics/PersonalAnalyticsView";

function AnalyticsContent() {
  const featureGate = useFeatureGate();
  const access = useAccessContext();
  const searchParams = useSearchParams();
  const tabParam = searchParams.get("tab");

  const canViewCampaign = access ? access.canViewAnalytics : true;
  const canViewPersonal = access ? access.canViewOwnAnalytics : true;

  const [activeTab, setActiveTab] = useState<"campaign" | "personal">(() => {
    if (tabParam === "personal") return "personal";
    if (access && !access.canViewAnalytics && access.canViewOwnAnalytics) return "personal";
    return "campaign";
  });

  useEffect(() => {
    if (tabParam === "personal") {
      setActiveTab("personal");
    } else if (tabParam === "campaign" && canViewCampaign) {
      setActiveTab("campaign");
    } else if (access && !access.canViewAnalytics && access.canViewOwnAnalytics) {
      setActiveTab("personal");
    }
  }, [tabParam, access?.canViewAnalytics, access?.canViewOwnAnalytics, canViewCampaign]);

  const { activeWorkspace } = useWorkspaces();
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [topPosts, setTopPosts] = useState<TopPostItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [dateRange, setDateRange] = useState<DateRange>("30d");
  const [campaignFilter, setCampaignFilter] = useState("all");
  const [brandFilter, setBrandFilter] = useState("all");
  const [platformFilter, setPlatformFilter] = useState("all");
  const [brandOptions, setBrandOptions] = useState<{ label: string; value: string }[]>([{ label: "All Brands", value: "all" }]);
  const analyticsRequestIdRef = useRef(0);

  useEffect(() => {
    if (activeTab !== "campaign") return;
    fetchBrands().then((brands) => {
      setBrandOptions([
        { label: "All Brands", value: "all" },
        ...brands.map((b) => ({ label: b.name, value: b.id })),
      ]);
    });
  }, [activeWorkspace?.id, activeTab]);

  useEffect(() => {
    if (activeTab !== "campaign") return;
    let cancelled = false;
    const requestId = ++analyticsRequestIdRef.current;
    const load = async () => {
      setLoading(true);
      try {
        const res = await fetchAnalytics({
          dateRange,
          campaignFilter,
          brandId: brandFilter,
          platform: platformFilter,
        });
        if (!cancelled && analyticsRequestIdRef.current === requestId) setData(res);
      } catch (err) {
        if (!cancelled && analyticsRequestIdRef.current === requestId) {
          console.error("Failed to load analytics:", err);
          setData(null);
        }
      }

      try {
        const posts = await fetchTopPosts(dateRange, "engagement", platformFilter !== "all" ? platformFilter : undefined);
        if (!cancelled && analyticsRequestIdRef.current === requestId) setTopPosts(posts);
      } catch (err) {
        if (!cancelled && analyticsRequestIdRef.current === requestId) console.error("Failed to load top posts:", err);
      } finally {
        if (!cancelled && analyticsRequestIdRef.current === requestId) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [dateRange, campaignFilter, brandFilter, platformFilter, activeWorkspace?.id, activeTab]);

  const handleRefresh = () => {
    const requestId = ++analyticsRequestIdRef.current;
    setData(null);
    setTopPosts([]);
    setLoading(true);
    const load = async () => {
      try {
        const res = await fetchAnalytics({ dateRange, campaignFilter, brandId: brandFilter, platform: platformFilter });
        if (analyticsRequestIdRef.current === requestId) setData(res);
        const posts = await fetchTopPosts(dateRange, "engagement", platformFilter !== "all" ? platformFilter : undefined);
        if (analyticsRequestIdRef.current === requestId) setTopPosts(posts);
      } catch (err) {
        if (analyticsRequestIdRef.current === requestId) console.error("Failed to refresh analytics:", err);
      } finally {
        if (analyticsRequestIdRef.current === requestId) setLoading(false);
      }
    };
    load();
  };

  const handleExport = async () => {
    if (!data) return;
    const headers = [
      "Date,Spend,CPC,Impressions,Engagement,Clicks,CTR,Published Posts",
    ];
    const rows = data.chartData.map(
      (d) =>
        `${d.date},${d.spend},${d.cpc},${d.impressions},${d.engagement},${d.clicks},${d.ctr},${d.publishedPosts}`
    );
    const csvContent = [...headers, ...rows].join("\n");
    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `analytics-report-${new Date().toISOString().split("T")[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-8px); } }
        @keyframes gradient-shift {
          0% { background-position: 0% 50%; }
          50% { background-position: 100% 50%; }
          100% { background-position: 0% 50%; }
        }
        .animate-fade-up { animation: fade-up 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
        .animate-gradient { 
          background-size: 200% 200%;
          animation: gradient-shift 3s ease infinite;
        }
      `}</style>

      <Header
        breadcrumbs={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Analysis" },
          ...(activeTab === "personal" ? [{ label: "Lịch sử cá nhân" }] : [{ label: "Chiến dịch" }]),
        ]}
      />

      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto overflow-x-hidden bg-linear-to-br from-surface-gray via-surface to-surface-gray">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* Premium Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 animate-fade-up">
            <div className="flex items-center gap-4">
              <div className="relative w-12 h-12 shrink-0">
                <div className="absolute inset-0 rounded-xl bg-linear-to-br from-primary via-secondary to-primary animate-gradient shadow-lg shadow-primary/30" />
                <div className="absolute inset-0 rounded-xl bg-linear-to-br from-white/20 to-transparent" />
                <div className="relative w-full h-full flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-xl" style={{ fontVariationSettings: '"FILL" 1' }}>
                    {activeTab === "personal" ? "history_edu" : "bar_chart"}
                  </span>
                </div>
                <div className="absolute -top-0.5 -right-0.5 w-3 h-3 bg-success-green rounded-full border-2 border-surface shadow-lg animate-pulse" />
              </div>
              <div>
                <h1 className="text-headline-sm bg-linear-to-r from-on-surface via-on-surface to-on-surface-variant bg-clip-text text-transparent">
                  {activeTab === "personal" ? "Lịch sử cá nhân & Sáng tạo" : "Báo cáo & Phân tích chiến dịch"}
                </h1>
                <p className="text-body-sm text-outline mt-1 flex items-center gap-2">
                  <span className="material-symbols-outlined text-body-sm text-success-green">check_circle</span>
                  {activeTab === "personal"
                    ? "Theo dõi số liệu cá nhân và toàn bộ lịch sử nội dung đã tạo"
                    : "Real-time campaign performance & AI insights"}
                </p>
              </div>
            </div>

            {activeTab === "campaign" && (
              <button
                onClick={handleExport}
                className="group relative bg-linear-to-r from-primary to-primary-container text-on-primary px-5 py-2.5 rounded-xl text-label-sm font-bold shadow-lg shadow-primary/30 hover:shadow-xl hover:shadow-primary/40 transition-all duration-300 hover:scale-105 overflow-hidden"
              >
                <span className="absolute inset-0 bg-linear-to-r from-white/0 via-white/20 to-white/0 -translate-x-full group-hover:translate-x-full transition-transform duration-1000" />
                <span className="relative flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-label-md">download</span>
                  Export Report
                </span>
              </button>
            )}
          </div>

          {/* Navigation Tabs (Chiến dịch vs Lịch sử cá nhân) */}
          {(canViewCampaign || canViewPersonal) && (
            <div className="flex items-center gap-2 p-1.5 bg-surface-container-lowest/80 backdrop-blur-md rounded-2xl border border-outline-variant/40 w-fit shadow-sm">
              {canViewCampaign && (
                <button
                  type="button"
                  onClick={() => setActiveTab("campaign")}
                  className={`flex items-center gap-2 px-4 py-2 rounded-xl text-body-sm font-semibold transition-all duration-200 ${
                    activeTab === "campaign"
                      ? "bg-primary text-on-primary shadow-md shadow-primary/20"
                      : "text-outline hover:text-on-surface hover:bg-surface-container"
                  }`}
                >
                  <span className="material-symbols-outlined text-[18px]">bar_chart</span>
                  <span>Tổng quan chiến dịch</span>
                </button>
              )}

              {canViewPersonal && (
                <button
                  type="button"
                  onClick={() => setActiveTab("personal")}
                  className={`flex items-center gap-2 px-4 py-2 rounded-xl text-body-sm font-semibold transition-all duration-200 ${
                    activeTab === "personal"
                      ? "bg-primary text-on-primary shadow-md shadow-primary/20"
                      : "text-outline hover:text-on-surface hover:bg-surface-container"
                  }`}
                >
                  <span className="material-symbols-outlined text-[18px]">history_edu</span>
                  <span>Lịch sử cá nhân</span>
                </button>
              )}
            </div>
          )}

          {/* Tab Content */}
          {activeTab === "personal" ? (
            <PersonalAnalyticsView />
          ) : (
            <>
              {loading || !data ? (
                <div className="space-y-6">
                  <div className="text-center py-4">
                    <span className="inline-flex items-center gap-2 text-primary font-medium bg-primary/10 px-4 py-2 rounded-full animate-pulse">
                      <span className="material-symbols-outlined text-xl">sync</span>
                      Analyzing data & waiting for response from Meta...
                    </span>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    {Array.from({ length: 4 }).map((_, i) => (
                      <div key={i} className="bg-surface-container-lowest p-6 rounded-2xl border border-outline-variant animate-pulse shadow-lg">
                        <div className="h-4 w-24 bg-surface-container rounded mb-4" />
                        <div className="h-8 w-32 bg-surface-container rounded mb-2" />
                        <div className="h-4 w-20 bg-surface-container rounded" />
                      </div>
                    ))}
                  </div>
                  <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-8 h-[500px] animate-pulse shadow-xl">
                    <div className="h-6 w-48 bg-surface-container rounded mb-4" />
                    <div className="h-96 bg-surface-container rounded" />
                  </div>
                </div>
              ) : (
                <>
                  <AnalyticsFilterBar
                    dateRange={dateRange}
                    onDateRangeChange={setDateRange}
                    campaignFilter={campaignFilter}
                    onCampaignFilterChange={setCampaignFilter}
                    brandFilter={brandFilter}
                    onBrandFilterChange={setBrandFilter}
                    platformFilter={platformFilter}
                    onPlatformFilterChange={setPlatformFilter}
                    brandOptions={brandOptions}
                    onRefresh={handleRefresh}
                  />

                  <AnalyticsKpiCards kpi={data.kpi} />

                  <div className="grid grid-cols-12 gap-6">
                    <div className="col-span-12 lg:col-span-8 space-y-6">
                      <AnalyticsChart data={data.scheduledPublishing} />
                      <AnalyticsPerformanceTable campaigns={data.campaignPerformance} onViewFullReport={handleExport} />
                      <AnalyticsTopPosts posts={topPosts} />
                    </div>

                    <aside className="col-span-12 lg:col-span-4 space-y-6">
                      {featureGate.canAccess("advancedAnalytics") ? (
                        <>
                          <AnalyticsAiInsights
                            insights={data.aiInsights}
                            dateRange={dateRange}
                            brandId={brandFilter}
                            platform={platformFilter}
                          />
                          <AnalyticsEfficiencyCard metrics={data.efficiency} />
                        </>
                      ) : (
                        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-8 text-center">
                          <div className="w-14 h-14 mx-auto mb-4 bg-outline/10 rounded-2xl flex items-center justify-center">
                            <span className="material-symbols-outlined text-outline text-[28px]">auto_awesome</span>
                          </div>
                          <h3 className="text-body-md text-on-surface font-bold mb-2">AI Insights</h3>
                          <p className="text-body-sm text-on-surface-variant mb-5">Advanced analytics with AI-powered insights and efficiency metrics are available on <strong>Personal Pro</strong> and above.</p>
                          <Link href="/pricing" className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all">
                            Upgrade Plan
                            <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
                          </Link>
                        </div>
                      )}
                    </aside>
                  </div>
                </>
              )}
            </>
          )}
        </div>
      </main>
    </>
  );
}

export default function AnalyticsPage() {
  return (
    <Suspense fallback={<div className="p-8 text-body-sm text-outline">Đang tải Analytics...</div>}>
      <AnalyticsContent />
    </Suspense>
  );
}
