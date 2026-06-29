import { apiClient } from "@/lib/apiClient";
import { fetchCampaigns } from "@/services/campaignService";

export type DateRange = "7d" | "30d" | "90d" | "custom";
export type ChartView = "daily" | "weekly";

export interface KpiData {
  totalAdSpend: number;
  totalAdSpendTrend: number;
  conversionRate: number;
  conversionRateTrend: number;
  avgCpa: number;
  avgCpaTrend: number;
  roas: number;
  roasTrend: number;
}

export interface ChartDataPoint {
  date: string;
  spend: number;
  conversions: number;
  cpc: number;
}

export interface CampaignPerformance {
  id: string;
  name: string;
  status: "active" | "paused" | "completed";
  reach: number;
  clicks: number;
  ctr: number;
  roas: number;
  spend: number;
  conversions: number;
}

export interface AiInsight {
  id: string;
  type: "recommendation" | "sentiment" | "trend";
  title: string;
  message: string;
  highlight?: string;
}

export interface EfficiencyMetric {
  label: string;
  value: number;
  color: string;
}

export interface AnalyticsData {
  kpi: KpiData;
  chartData: ChartDataPoint[];
  campaignPerformance: CampaignPerformance[];
  aiInsights: AiInsight[];
  efficiency: EfficiencyMetric[];
}

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

// BE DTOs
interface BEDashboardSummaryDto {
  draftContentCount: number;
  publishedContentCount: number;
  pendingApprovalContentCount: number;
  upcomingScheduleCount: number;
  failedScheduleCount: number;
  activeSocialIntegrationCount: number;
  publishedPostCount: number;
  unreadNotificationCount: number;
}

interface BEWorkspaceDashboardSummaryDto {
  workspaceId: string;
  creditBalance: number;
  creditsUsed: number;
  publishedPostCount: number;
  postQuotaLimit: number;
  postsRemaining: number;
  aiUsageCount: number;
  activeMemberCount: number;
  topMembers: { userId: string; name: string; email: string; creditsUsed: number; aiUsageCount: number }[];
}

function generateDailyChartData(days: number): ChartDataPoint[] {
  const data: ChartDataPoint[] = [];
  const today = new Date();
  for (let i = days - 1; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(date.getDate() - i);
    data.push({
      date: date.toISOString().split("T")[0],
      spend: Math.round(Math.random() * 500 + 200),
      conversions: Math.round(Math.random() * 30 + 10),
      cpc: Math.round((Math.random() * 0.8 + 0.8) * 100) / 100,
    });
  }
  return data;
}

export async function fetchAnalytics(campaignFilter?: string): Promise<AnalyticsData> {
  let dashboard: BEDashboardSummaryDto | null = null;
  let wsDashboard: BEWorkspaceDashboardSummaryDto | null = null;

  try {
    const res1: GenericResponse<BEDashboardSummaryDto> = await apiClient("/dashboard/summary");
    if (res1?.data) dashboard = res1.data;
  } catch { /* ignore */ }

  try {
    const res2: GenericResponse<BEWorkspaceDashboardSummaryDto> = await apiClient("/workspace-dashboard/summary");
    if (res2?.data) wsDashboard = res2.data;
  } catch { /* ignore */ }

  const totalContent = (dashboard?.draftContentCount || 0) + (dashboard?.publishedContentCount || 0);
  const conversionRate = totalContent > 0 ? ((dashboard?.publishedContentCount || 0) / totalContent) * 100 : 0;

  let campaignPerformance: CampaignPerformance[] = [];
  try {
    const res = await fetchCampaigns({ pageSize: 50 });
    let campaigns = res.data;
    if (campaignFilter && campaignFilter !== "all") {
      const statusMap: Record<string, string> = { active: "ACTIVE", paused: "PAUSED", completed: "COMPLETED" };
      const targetStatus = statusMap[campaignFilter];
      if (targetStatus) {
        campaigns = campaigns.filter(c => c.status === targetStatus);
      }
    }
    campaignPerformance = campaigns.map((c) => ({
      id: c.id,
      name: c.name,
      status: c.status === "ACTIVE" ? "active" : c.status === "PAUSED" ? "paused" : "completed",
      reach: c.impressions,
      clicks: c.clicks,
      ctr: c.impressions > 0 ? Math.round((c.clicks / c.impressions) * 10000) / 100 : 0,
      roas: c.spend > 0 ? Math.round((c.conversions / c.spend) * 10) / 10 : 0,
      spend: c.spend,
      conversions: c.conversions,
    }));
  } catch { /* ignore */ }

  return {
    kpi: {
      totalAdSpend: wsDashboard?.creditsUsed || 0,
      totalAdSpendTrend: 0,
      conversionRate: Math.round(conversionRate * 10) / 10,
      conversionRateTrend: 0,
      avgCpa: 0,
      avgCpaTrend: 0,
      roas: 0,
      roasTrend: 0,
    },
    chartData: generateDailyChartData(30),
    campaignPerformance,
    aiInsights: dashboard
      ? [
          {
            id: "insight-1",
            type: "recommendation",
            title: "Content Overview",
            message: `${dashboard.draftContentCount} drafts, ${dashboard.publishedContentCount} published, ${dashboard.pendingApprovalContentCount} pending approval.`,
            highlight: `${dashboard.publishedContentCount} published`,
          },
          {
            id: "insight-2",
            type: "trend",
            title: "Schedule Status",
            message: `${dashboard.upcomingScheduleCount} upcoming schedules, ${dashboard.failedScheduleCount} failed.`,
            highlight: `${dashboard.upcomingScheduleCount} upcoming`,
          },
        ]
      : [],
    efficiency: wsDashboard
      ? [
          { label: "Credit Usage", value: wsDashboard.postQuotaLimit > 0 ? Math.round((wsDashboard.creditsUsed / wsDashboard.postQuotaLimit) * 100) : 0, color: "bg-primary" },
          { label: "Posts Remaining", value: wsDashboard.postsRemaining > 0 ? Math.round((wsDashboard.postsRemaining / wsDashboard.postQuotaLimit) * 100) : 0, color: "bg-secondary" },
        ]
      : [],
  };
}

export async function exportReport(): Promise<Blob> {
  const csvContent = [
    "Metric,Value",
    ...(await fetchAnalytics()).aiInsights.map((i) => `${i.title},${i.message}`),
  ].join("\n");

  return new Blob([csvContent], { type: "text/csv" });
}
