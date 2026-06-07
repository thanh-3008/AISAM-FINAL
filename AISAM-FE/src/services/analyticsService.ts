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

const STORAGE_KEY = "aisam_analytics_v1";

const INITIAL_MOCK_DATA: AnalyticsData = {
  kpi: {
    totalAdSpend: 12450,
    totalAdSpendTrend: 8,
    conversionRate: 3.2,
    conversionRateTrend: 0.5,
    avgCpa: 4.15,
    avgCpaTrend: -12,
    roas: 4.8,
    roasTrend: 15,
  },
  chartData: [
    { date: "2024-06-01", spend: 420, conversions: 18, cpc: 1.2 },
    { date: "2024-06-02", spend: 580, conversions: 24, cpc: 1.1 },
    { date: "2024-06-03", spend: 450, conversions: 20, cpc: 1.3 },
    { date: "2024-06-04", spend: 680, conversions: 32, cpc: 1.0 },
    { date: "2024-06-05", spend: 820, conversions: 38, cpc: 0.95 },
    { date: "2024-06-06", spend: 620, conversions: 28, cpc: 1.15 },
    { date: "2024-06-07", spend: 380, conversions: 15, cpc: 1.4 },
    { date: "2024-06-08", spend: 520, conversions: 22, cpc: 1.25 },
    { date: "2024-06-09", spend: 720, conversions: 34, cpc: 1.05 },
    { date: "2024-06-10", spend: 880, conversions: 42, cpc: 0.9 },
    { date: "2024-06-11", spend: 560, conversions: 26, cpc: 1.2 },
    { date: "2024-06-12", spend: 340, conversions: 14, cpc: 1.45 },
    { date: "2024-06-13", spend: 490, conversions: 21, cpc: 1.3 },
    { date: "2024-06-14", spend: 650, conversions: 30, cpc: 1.1 },
    { date: "2024-06-15", spend: 780, conversions: 36, cpc: 1.0 },
    { date: "2024-06-16", spend: 410, conversions: 17, cpc: 1.35 },
    { date: "2024-06-17", spend: 550, conversions: 25, cpc: 1.2 },
    { date: "2024-06-18", spend: 710, conversions: 33, cpc: 1.05 },
    { date: "2024-06-19", spend: 850, conversions: 40, cpc: 0.95 },
    { date: "2024-06-20", spend: 600, conversions: 27, cpc: 1.15 },
    { date: "2024-06-21", spend: 430, conversions: 19, cpc: 1.3 },
    { date: "2024-06-22", spend: 570, conversions: 26, cpc: 1.2 },
    { date: "2024-06-23", spend: 690, conversions: 31, cpc: 1.1 },
    { date: "2024-06-24", spend: 820, conversions: 37, cpc: 1.0 },
    { date: "2024-06-25", spend: 480, conversions: 20, cpc: 1.25 },
    { date: "2024-06-26", spend: 630, conversions: 29, cpc: 1.15 },
    { date: "2024-06-27", spend: 750, conversions: 35, cpc: 1.05 },
    { date: "2024-06-28", spend: 880, conversions: 41, cpc: 0.95 },
    { date: "2024-06-29", spend: 540, conversions: 24, cpc: 1.2 },
    { date: "2024-06-30", spend: 400, conversions: 16, cpc: 1.4 },
  ],
  campaignPerformance: [
    {
      id: "cp1",
      name: "Summer Sale 2024",
      status: "active",
      reach: 45200,
      clicks: 1800,
      ctr: 3.98,
      roas: 5.2,
      spend: 2850,
      conversions: 148,
    },
    {
      id: "cp2",
      name: "Product Launch - V2",
      status: "active",
      reach: 128000,
      clicks: 4200,
      ctr: 3.28,
      roas: 4.8,
      spend: 4200,
      conversions: 202,
    },
    {
      id: "cp3",
      name: "Brand Awareness Global",
      status: "paused",
      reach: 2400000,
      clicks: 15800,
      ctr: 0.66,
      roas: 1.2,
      spend: 3800,
      conversions: 46,
    },
    {
      id: "cp4",
      name: "Retargeting - Cart Abandoners",
      status: "active",
      reach: 18500,
      clicks: 920,
      ctr: 4.97,
      roas: 6.8,
      spend: 1600,
      conversions: 109,
    },
    {
      id: "cp5",
      name: "Holiday Season Preview",
      status: "completed",
      reach: 89000,
      clicks: 2100,
      ctr: 2.36,
      roas: 3.5,
      spend: 2400,
      conversions: 84,
    },
  ],
  aiInsights: [
    {
      id: "ai1",
      type: "recommendation",
      title: "Budget Optimization",
      message: "Your 'Summer Sale' campaign is outperforming the industry average by 20%. AISAM recommends shifting 15% more budget to this creative set.",
      highlight: "20%",
    },
    {
      id: "ai2",
      type: "sentiment",
      title: "Sentiment Analysis",
      message: "Positive (Future/Planned)",
    },
    {
      id: "ai3",
      type: "trend",
      title: "Trend Alert",
      message: "Engagement rates usually dip between 2 AM - 5 AM UTC. We've scheduled automatic bid pausing for this window.",
      highlight: "2 AM - 5 AM",
    },
  ],
  efficiency: [
    { label: "Creative Asset ROI", value: 88, color: "bg-primary" },
    { label: "Audience Match Rate", value: 64, color: "bg-secondary" },
  ],
};

function loadAnalytics(): AnalyticsData {
  if (typeof window === "undefined") return { ...INITIAL_MOCK_DATA };
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as AnalyticsData;
      if (parsed && parsed.kpi) return parsed;
    }
  } catch {
    /* fallback */
  }
  const initial = { ...INITIAL_MOCK_DATA };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

const MOCK_DATA: AnalyticsData = loadAnalytics();

export async function fetchAnalytics(): Promise<AnalyticsData> {
  return { ...MOCK_DATA };
}

export async function exportReport(): Promise<Blob> {
  const csvContent = [
    "Campaign,Reach,Clicks,CTR,ROAS,Spend,Conversions",
    ...MOCK_DATA.campaignPerformance.map(
      (c) => `${c.name},${c.reach},${c.clicks},${c.ctr}%,${c.roas}x,$${c.spend},${c.conversions}`
    ),
  ].join("\n");

  return new Blob([csvContent], { type: "text/csv" });
}
