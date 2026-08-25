import { apiClient } from "@/lib/apiClient";
import { fetchCampaigns } from "@/services/campaignService";

export type DateRange = "7d" | "30d" | "90d" | "custom";
export type ChartView = "daily" | "weekly";

export interface KpiData {
  totalReach: number;
  totalReachTrend: number;
  totalInteractions: number;
  totalInteractionsTrend: number;
  avgCpe: number;
  avgCpeTrend: number;
  publishedPosts: number;
  sparklines: {
    spend: number[];
    engagement: number[];
    impressions: number[];
    clicks: number[];
    conversions: number[];
  };
}

export interface ChartDataPoint {
  date: string;
  spend: number;
  conversions: number;
  cpc: number;
  impressions: number;
  engagement: number;
  clicks: number;
  ctr: number;
  publishedPosts: number;
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

export interface AiRecommendationItem {
  priority: 'HIGH' | 'MID' | 'LOW';
  title: string;
  rationale: string;
  actionable_steps: string[];
  kpi_target: string;
}

export interface AiRecommendationsResponse {
  recommendations?: AiRecommendationItem[];
  error?: string;
  message?: string;
}

export interface AiRequestOptions {
  signal?: AbortSignal;
  correlationId?: string;
}

export interface EfficiencyMetric {
  label: string;
  value: number;
  color: string;
}

export interface ChannelBreakdownItem {
  platform: string;
  integrationId?: string;
  displayName?: string;
  impressions: number;
  reach: number;
  engagement: number;
  clicks: number;
  ctr: number;
  spend: number;
  publishedPosts: number;
  lastSyncedAt?: string | null;
}

export interface UsageBreakdownItem {
  category: string;
  count: number;
  percentage: number;
}

export interface TopPostItem {
  postId: string;
  contentId?: string;
  contentTitle?: string;
  brandName?: string;
  platform: string;
  publishedAt?: string;
  externalPostId?: string;
  impressions: number;
  reach: number;
  engagement: number;
  clicks: number;
  totalMediaViewUnique: number;
  ctr: number;
}

export interface AnalyticsData {
  kpi: KpiData;
  chartData: ChartDataPoint[];
  campaignPerformance: CampaignPerformance[];
  channelBreakdown: ChannelBreakdownItem[];
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

interface AnalyticsTotals {
  impressions: number;
  reach: number;
  engagement: number;
  clicks: number;
  conversions: number;
  ctr: number;
  spend: number;
  estimatedRevenue: number;
  publishedPosts: number;
  activeCampaigns: number;
}

interface AnalyticsSparklines {
  impressions: number[];
  engagement: number[];
  clicks: number[];
  conversions: number[];
  ctr: number[];
  spend: number[];
}

interface AnalyticsOverviewResponse {
  dateRange: { from: string; to: string };
  totals: AnalyticsTotals;
  changes: {
    impressionsPct: number;
    engagementPct: number;
    ctrPct: number;
    spendPct: number;
    clicksPct: number;
    conversionRatePct: number;
    cpaPct: number;
    roasPct: number;
  };
  sparklines: AnalyticsSparklines;
  dataFreshness: {
    lastSyncedAt: string | null;
    isPartial: boolean;
  };
}

interface AnalyticsPoint {
  date: string;
  impressions: number;
  reach: number;
  engagement: number;
  clicks: number;
  conversions: number;
  ctr: number;
  spend: number;
  estimatedRevenue: number;
  publishedPosts: number;
  activeCampaigns: number;
}

interface TimeSeriesResponse {
  granularity: string;
  points: AnalyticsPoint[];
}

interface PaginatedResponse {
  items: unknown[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

function getDateRange(range: DateRange): { from: string; to: string } {
  const to = new Date();
  to.setHours(23, 59, 59, 999);
  const from = new Date();

  switch (range) {
    case "7d":
      from.setDate(from.getDate() - 7);
      break;
    case "30d":
      from.setDate(from.getDate() - 30);
      break;
    case "90d":
      from.setDate(from.getDate() - 90);
      break;
    case "custom":
    default:
      from.setDate(from.getDate() - 30);
      break;
  }
  from.setHours(0, 0, 0, 0);

  return {
    from: from.toISOString(),
    to: to.toISOString(),
  };
}

function buildFilterQuery(
  from: string,
  to: string,
  params: { brandId?: string; platform?: string; campaignId?: string }
): string {
  let query = `from=${from}&to=${to}`;
  if (params.brandId) query += `&brandId=${params.brandId}`;
  if (params.platform) query += `&platform=${params.platform}`;
  if (params.campaignId) query += `&campaignId=${params.campaignId}`;
  return query;
}

export async function fetchChannelBreakdown(
  dateRange?: DateRange,
  brandId?: string
): Promise<ChannelBreakdownItem[]> {
  const { from, to } = getDateRange(dateRange || "30d");
  const query = buildFilterQuery(from, to, { brandId });
  try {
    const res: GenericResponse<ChannelBreakdownItem[]> = await apiClient(
      `/analytics/channel-breakdown?${query}`
    );
    return res?.data || [];
  } catch {
    return [];
  }
}

export async function fetchUsageBreakdown(): Promise<UsageBreakdownItem[]> {
  try {
    const res: GenericResponse<{ items: UsageBreakdownItem[] }> =
      await apiClient("/analytics/usage-breakdown");
    console.log("[fetchUsageBreakdown] API response:", res);
    const items = res?.data?.items || [];
    console.log("[fetchUsageBreakdown] Parsed items:", items);
    return items;
  } catch (err) {
    console.error("[fetchUsageBreakdown] Error:", err);
    return [];
  }
}

export async function fetchTopPosts(
  dateRange?: DateRange,
  metric?: string,
  platform?: string,
  pageSize?: number
): Promise<TopPostItem[]> {
  const { from, to } = getDateRange(dateRange || "30d");
  let query = `from=${from}&to=${to}&metric=${metric || "engagement"}&pageSize=${pageSize || 10}`;
  if (platform) query += `&platform=${platform}`;
  try {
    const res: GenericResponse<PaginatedResponse> = await apiClient(
      `/analytics/top-posts?${query}`
    );
    return ((res?.data?.items || []) as TopPostItem[]);
  } catch {
    return [];
  }
}

export async function fetchAiRecommendations(
  dateRange?: DateRange,
  forceRefresh?: boolean,
  requestOptions?: AiRequestOptions
): Promise<AiRecommendationsResponse> {
  const { from, to } = getDateRange(dateRange || "30d");
  const controller = new AbortController();
  const correlationId = requestOptions?.correlationId ||
    (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function"
      ? crypto.randomUUID()
      : `ask-ai-${Date.now()}-${Math.random().toString(16).slice(2)}`);
  const requestStart = new Date().toISOString();
  const startedAt = Date.now();
  let timeoutTriggered = false;
  let abortTriggered = false;
  let status: number | undefined;
  let outcome = "INTERNAL_ERROR";
  let errorCategory: string | undefined;
  const onExternalAbort = () => {
    abortTriggered = true;
    controller.abort();
  };
  requestOptions?.signal?.addEventListener("abort", onExternalAbort, { once: true });
  const timeout = setTimeout(() => {
    timeoutTriggered = true;
    controller.abort();
  }, 65000);
  try {
    const res: GenericResponse<string> = await apiClient(
      `/analytics/ai-recommendations?from=${from}&to=${to}${forceRefresh ? '&forceRefresh=true' : ''}`,
      {
        signal: controller.signal,
        headers: { "X-Correlation-ID": correlationId },
      } as RequestInit
    );
    status = (res as GenericResponse<string> & { __httpStatus?: number }).__httpStatus;
    const rawData = res?.data || "";
    if (!rawData) {
      outcome = "LLM_PARSE_FAILURE";
      errorCategory = "EMPTY_RESPONSE";
      return { error: "EMPTY", message: "No data returned." };
    }
    try {
      outcome = "SUCCESS";
      return JSON.parse(rawData) as AiRecommendationsResponse;
    } catch {
      outcome = "LLM_PARSE_FAILURE";
      errorCategory = "INVALID_JSON";
      return { error: "PARSE_ERROR", message: "Failed to parse AI output." };
    }
  } catch (error: unknown) {
    const typedError = error as Error & { status?: number; category?: string };
    status = typedError.status;
    if (typedError.name === "AbortError" || typedError.name === "CanceledError") {
      outcome = timeoutTriggered ? "FRONTEND_TIMEOUT" : "CLIENT_CANCELLED";
      errorCategory = timeoutTriggered ? "TIMEOUT" : "ABORT";
      return timeoutTriggered
        ? { error: "TIMEOUT", message: "Request timed out." }
        : { error: "CLIENT_CANCELLED", message: "Request cancelled." };
    }
    outcome = status ? "INTERNAL_ERROR" : "INTERNAL_ERROR";
    errorCategory = typedError.category || "NETWORK_ERROR";
    return { error: "NETWORK_ERROR", message: typedError.message || "Network error occurred." };
  } finally {
    clearTimeout(timeout);
    requestOptions?.signal?.removeEventListener("abort", onExternalAbort);
    const requestEnd = new Date().toISOString();
    const telemetry = {
      correlationId,
      requestStart,
      requestEnd,
      durationMs: Date.now() - startedAt,
      timeoutTriggered,
      abortTriggered,
      status,
      outcome,
      errorCategory,
    };
    if (outcome === "SUCCESS") {
      console.info("[AskAI.Telemetry]", telemetry);
    } else {
      console.warn("[AskAI.Telemetry]", telemetry);
    }
  }
}

export interface GeographicItem {
  country: string;
  percentage: number;
  count: number;
}

export interface DemographicItem {
  group: string;
  percentage: number;
  count: number;
}

export interface DeviceItem {
  device: string;
  percentage: number;
}

export interface AudienceBreakdown {
  geographic: GeographicItem[];
  demographics: DemographicItem[];
  devices: DeviceItem[];
}

export async function fetchAudienceBreakdown(): Promise<AudienceBreakdown> {
  try {
    const res: GenericResponse<AudienceBreakdown> = await apiClient("/analytics/audience");
    return res?.data || { geographic: [], demographics: [], devices: [] };
  } catch {
    return { geographic: [], demographics: [], devices: [] };
  }
}

export async function fetchAnalytics(
  options?: {
    dateRange?: DateRange;
    campaignFilter?: string;
    brandId?: string;
    platform?: string;
  }
): Promise<AnalyticsData> {
  const range = options?.dateRange || "30d";
  const { from, to } = getDateRange(range);
  const platform = options?.platform && options.platform !== "all" ? options.platform : undefined;
  const brandId = options?.brandId && options.brandId !== "all" ? options.brandId : undefined;

  const filterQuery = buildFilterQuery(from, to, { brandId, platform });

  let overview: AnalyticsOverviewResponse | null = null;
  let timeSeries: TimeSeriesResponse | null = null;
  let channelBreakdown: ChannelBreakdownItem[] = [];

  // Fetch all analytics data in parallel to reduce total DB connection hold time
  const [overviewResult, timeSeriesResult, channelResult, campaignsResult] = await Promise.allSettled([
    apiClient(`/analytics/overview?${filterQuery}`) as Promise<GenericResponse<AnalyticsOverviewResponse>>,
    apiClient(`/analytics/time-series?${filterQuery}&granularity=day`) as Promise<GenericResponse<TimeSeriesResponse>>,
    fetchChannelBreakdown(range, brandId),
    fetchCampaigns({ pageSize: 50 }),
  ]);

  if (overviewResult.status === "fulfilled") {
    const res1 = overviewResult.value as GenericResponse<AnalyticsOverviewResponse>;
    if (res1?.data) overview = res1.data;
  }
  if (timeSeriesResult.status === "fulfilled") {
    const res2 = timeSeriesResult.value as GenericResponse<TimeSeriesResponse>;
    if (res2?.data) timeSeries = res2.data;
  }
  if (channelResult.status === "fulfilled") {
    channelBreakdown = channelResult.value as ChannelBreakdownItem[];
  }

  const totals = overview?.totals;
  const changes = overview?.changes;
  const sparklines = overview?.sparklines;

  let campaignPerformance: CampaignPerformance[] = [];
  try {
    const res = campaignsResult.status === "fulfilled"
      ? (campaignsResult.value as Awaited<ReturnType<typeof fetchCampaigns>>)
      : await fetchCampaigns({ pageSize: 50 });
    let campaigns = res.data;
    const campaignFilter = options?.campaignFilter;
    if (campaignFilter && campaignFilter !== "all") {
      const statusMap: Record<string, string> = {
        active: "ACTIVE",
        paused: "PAUSED",
        completed: "COMPLETED",
      };
      const targetStatus = statusMap[campaignFilter];
      if (targetStatus) {
        campaigns = campaigns.filter((c) => c.status === targetStatus);
      }
    }
    campaignPerformance = campaigns.map((c) => ({
      id: c.id,
      name: c.name,
      status:
        c.status === "ACTIVE"
          ? "active"
          : c.status === "PAUSED"
            ? "paused"
            : "completed",
      reach: c.impressions,
      clicks: c.clicks,
      ctr:
        c.impressions > 0
          ? Math.round((c.clicks / c.impressions) * 10000) / 100
          : 0,
      roas: c.spend > 0 ? Math.round((c.conversions / c.spend) * 10) / 10 : 0,
      spend: c.spend,
      conversions: c.conversions,
    }));
  } catch {
    /* ignore */
  }

  const chartData: ChartDataPoint[] = timeSeries?.points?.length
    ? timeSeries.points.map((p) => ({
        date: p.date,
        spend: p.spend || 0,
        conversions: p.conversions || 0,
        cpc:
          p.clicks > 0
            ? Math.round((p.spend / p.clicks) * 100) / 100
            : 0,
        impressions: p.impressions || 0,
        engagement: p.engagement || 0,
        clicks: p.clicks || 0,
        ctr: p.ctr || 0,
        publishedPosts: p.publishedPosts || 0,
      }))
    : [];

  const engagementBase = totals?.reach || totals?.impressions || 0;
  const engagementRate =
    engagementBase > 0
      ? Math.round(((totals?.engagement || 0) / engagementBase) * 10000) / 100
      : 0;

  return {
    kpi: {
      totalReach: totals?.reach || totals?.impressions || 0,
      totalReachTrend: changes?.impressionsPct || 0,
      totalInteractions: totals?.engagement || 0,
      totalInteractionsTrend: changes?.engagementPct || 0,
      avgCpe:
        totals?.engagement && totals.engagement > 0
          ? Math.round((totals.spend / totals.engagement) * 100) / 100
          : 0,
      avgCpeTrend: (changes?.spendPct || 0) - (changes?.engagementPct || 0),
      publishedPosts: totals?.publishedPosts || 0,
      sparklines: {
        spend: sparklines?.spend || [],
        engagement: sparklines?.engagement || [],
        impressions: sparklines?.impressions || [],
        clicks: sparklines?.clicks || [],
        conversions: sparklines?.conversions || [],
      },
    },
    chartData,
    campaignPerformance,
    channelBreakdown,
    aiInsights: [
      {
        id: "insight-1",
        type: "recommendation" as const,
        title: "Performance Overview",
        message: `${totals?.impressions?.toLocaleString() || 0} impressions, ${totals?.clicks?.toLocaleString() || 0} clicks, CTR ${totals?.ctr || 0}%`,
        highlight: `${totals?.publishedPosts || 0} posts`,
      },
      {
        id: "insight-2",
        type: "trend" as const,
        title: "Campaign Status",
        message: `${totals?.activeCampaigns || 0} active campaigns, ${totals?.estimatedRevenue?.toLocaleString() || 0} VND estimated revenue`,
        highlight: `${totals?.activeCampaigns || 0} active`,
      },
    ],
    efficiency: [
      {
        label: "Engagement Rate",
        value: engagementRate,
        color: "bg-primary",
      },
      {
        label: "CTR",
        value: totals?.ctr ? Math.round(totals.ctr * 100) / 100 : 0,
        color: "bg-secondary",
      },
    ],
  };
}
