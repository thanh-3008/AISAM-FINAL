import { apiClient } from "@/lib/apiClient";

export type CampaignStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "DRAFT";
export type CampaignObjective = "AWARENESS" | "TRAFFIC" | "ENGAGEMENT" | "LEADS" | "SALES" | "APP_PROMOTION";

export interface Ad {
  id: string;
  adId: string | null;
  status: string | null;
  creativeId: string | null;
  callToAction: string | null;
  linkUrl: string | null;
}

export interface AdSet {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: "ACTIVE" | "PAUSED";
  impressions: number;
  clicks: number;
  spend: number;
  ads: Ad[];
}

export type DeploymentStatus = 0 | 1 | 2 | 3;

export interface Campaign {
  id: string;
  profileId: string;
  workspaceId: string;
  brandId: string;
  brandName: string;
  productId: string | null;
  productName: string | null;
  contentId: string | null;
  contentTitle: string | null;
  targeting: string | null;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  status: CampaignStatus;
  deploymentStatus: DeploymentStatus;
  deploymentStep: number;
  createdAt: string;
  updatedAt: string;
  adSets: AdSet[];
  impressions: number;
  clicks: number;
  spend: number;
  conversions: number;
}

export interface CreateCampaignData {
  name: string;
  brandId: string;
  brandName: string;
  productId?: string | null;
  contentId?: string | null;
  targeting?: string | null;
  adAccountId: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
}

interface CampaignApiItem {
  id: string;
  profileId: string;
  workspaceId: string;
  brandId: string;
  brandName: string;
  productId: string | null;
  productName: string | null;
  contentId: string | null;
  contentTitle: string | null;
  targeting: string | null;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: string | null;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  isActive: boolean;
  isDeleted: boolean;
  deploymentStatus: number;
  deploymentStep: number;
  createdAt: string;
  updatedAt: string;
  adSets: AdSetApiItem[];
  impressions: number;
  clicks: number;
  spend: number;
  conversions: number;
}

interface AdSetApiItem {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: string | null;
  impressions: number;
  clicks: number;
  spend: number;
  ads: AdApiItem[];
}

interface AdApiItem {
  id: string;
  adId: string | null;
  status: string | null;
  creativeId: string | null;
  callToAction: string | null;
  linkUrl: string | null;
}

function mapCampaign(api: CampaignApiItem): Campaign {
  let status: CampaignStatus = "DRAFT";
  if (api.isActive) {
    status = "ACTIVE";
  } else if (api.endDate && new Date(api.endDate) < new Date()) {
    status = "COMPLETED";
  } else if (!api.isActive && api.startDate) {
    status = "PAUSED";
  }

  return {
    id: api.id,
    profileId: api.profileId,
    workspaceId: api.workspaceId,
    brandId: api.brandId,
    brandName: api.brandName,
    productId: api.productId ?? null,
    productName: api.productName ?? null,
    contentId: api.contentId ?? null,
    contentTitle: api.contentTitle ?? null,
    targeting: api.targeting ?? null,
    adAccountId: api.adAccountId,
    facebookCampaignId: api.facebookCampaignId,
    name: api.name,
    objective: (api.objective as CampaignObjective) || "AWARENESS",
    budget: api.budget,
    startDate: api.startDate,
    endDate: api.endDate,
    status,
    deploymentStatus: api.deploymentStatus as DeploymentStatus,
    deploymentStep: api.deploymentStep,
    createdAt: api.createdAt,
    updatedAt: api.updatedAt,
    adSets: (api.adSets || []).map((ads) => ({
      id: ads.id,
      name: ads.name,
      facebookAdSetId: ads.facebookAdSetId,
      dailyBudget: ads.dailyBudget,
      status: (ads.status === "ACTIVE" ? "ACTIVE" : "PAUSED") as "ACTIVE" | "PAUSED",
      impressions: ads.impressions,
      clicks: ads.clicks,
      spend: ads.spend,
      ads: (ads.ads || []).map((a: AdApiItem) => ({
        id: a.id,
        adId: a.adId,
        status: a.status,
        creativeId: a.creativeId,
        callToAction: a.callToAction,
        linkUrl: a.linkUrl,
      })),
    })),
    impressions: api.impressions,
    clicks: api.clicks,
    spend: api.spend,
    conversions: api.conversions,
  };
}

export async function fetchCampaigns(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}): Promise<{ data: Campaign[]; total: number }> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    if (params?.searchTerm) query.set("searchTerm", params.searchTerm);
    if (params?.sortBy) query.set("sortBy", params.sortBy);
    if (params?.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));

    const qs = query.toString();
    const res = await apiClient(`/campaigns${qs ? `?${qs}` : ""}`);
    if (res?.success && res.data) {
      return {
        data: (res.data.data as CampaignApiItem[]).map(mapCampaign),
        total: res.data.totalCount || 0,
      };
    }
  } catch (err) {
    console.error("Failed to fetch campaigns:", err);
  }
  return { data: [], total: 0 };
}

export async function createCampaign(data: CreateCampaignData): Promise<Campaign> {
  if (!data.adAccountId) {
    throw new Error("Ad account is required");
  }
  const res = await apiClient("/campaigns", {
    data: {
      name: data.name,
      brandId: data.brandId,
      productId: data.productId ?? null,
      contentId: data.contentId ?? null,
      targeting: data.targeting ?? null,
      adAccountId: data.adAccountId,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate || null,
      endDate: data.endDate || null,
    },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to create campaign");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function updateCampaign(id: string, data: CreateCampaignData): Promise<Campaign> {
  const res = await apiClient(`/campaigns/${id}`, {
    method: "PUT",
    data: {
      name: data.name,
      brandId: data.brandId,
      productId: data.productId ?? null,
      contentId: data.contentId ?? null,
      targeting: data.targeting ?? null,
      adAccountId: data.adAccountId,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate || null,
      endDate: data.endDate || null,
    },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update campaign");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function updateCampaignStatus(id: string, status: CampaignStatus): Promise<Campaign> {
  const isActive = status === "ACTIVE";
  const res = await apiClient(`/campaigns/${id}`, {
    method: "PUT",
    data: { isActive },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update campaign status");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function applyCampaign(id: string): Promise<Campaign> {
  return updateCampaignStatus(id, "ACTIVE");
}

export async function restartCampaign(id: string): Promise<Campaign> {
  return updateCampaignStatus(id, "ACTIVE");
}

export async function deleteCampaign(id: string): Promise<boolean> {
  const res = await apiClient(`/campaigns/${id}`, { method: "DELETE" });
  return res?.success === true;
}

export async function deployCampaignToFacebook(id: string): Promise<Campaign> {
  const res = await apiClient(`/campaigns/${id}/deploy`, { method: "POST" });
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to deploy campaign to Facebook");
  }
  return mapCampaign(res.data as CampaignApiItem);
}

export async function syncCampaignInsights(id: string): Promise<Campaign> {
  const res = await apiClient(`/campaigns/${id}/sync-insights`, { method: "POST" });
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to sync campaign insights");
  }
  return mapCampaign(res.data as CampaignApiItem);
}

export async function cleanupCampaignDeployment(id: string): Promise<boolean> {
  const res = await apiClient(`/campaigns/${id}/cleanup`, { method: "POST" });
  return res?.success === true;
}

export async function getCampaignById(id: string): Promise<Campaign | null> {
  try {
    const res = await apiClient(`/campaigns/${id}`);
    if (res?.success && res.data) {
      return mapCampaign(res.data as CampaignApiItem);
    }
  } catch {
    // ignore
  }
  return null;
}
