import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export type CampaignStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "DRAFT";
export type CampaignObjective = "AWARENESS" | "TRAFFIC" | "ENGAGEMENT" | "LEADS" | "SALES" | "APP_PROMOTION";

export interface AdSet {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: "ACTIVE" | "PAUSED";
  impressions: number;
  clicks: number;
  spend: number;
}

export interface Campaign {
  id: string;
  workspaceId: string;
  brandId: string;
  brandName: string;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  status: CampaignStatus;
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
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
}

function mapCampaign(campaign: Campaign): Campaign {
  return {
    ...campaign,
    brandName: campaign.brandName || "",
    objective: campaign.objective as CampaignObjective,
    status: campaign.status as CampaignStatus,
    startDate: campaign.startDate ?? null,
    endDate: campaign.endDate ?? null,
    budget: campaign.budget ?? null,
    adSets: campaign.adSets ?? [],
    impressions: campaign.impressions ?? 0,
    clicks: campaign.clicks ?? 0,
    spend: campaign.spend ?? 0,
    conversions: campaign.conversions ?? 0,
  };
}

export async function fetchCampaigns(): Promise<{ data: Campaign[]; total: number }> {
  const res: GenericResponse<PagedResult<Campaign>> = await apiClient("/ad-campaigns?page=1&pageSize=100");
  const data = res.data?.data ?? [];
  return { data: data.map(mapCampaign), total: res.data?.totalCount ?? data.length };
}

export async function createCampaign(data: CreateCampaignData): Promise<Campaign> {
  const res: GenericResponse<Campaign> = await apiClient("/ad-campaigns", {
    data: {
      brandId: data.brandId,
      name: data.name,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate,
      endDate: data.endDate,
    },
  });
  if (!res.success || !res.data) throw new Error(res.message || "Could not create campaign.");
  return mapCampaign(res.data);
}

export async function restartCampaign(id: string): Promise<Campaign | null> {
  return updateCampaignStatus(id, "ACTIVE");
}

export async function applyCampaign(id: string): Promise<Campaign | null> {
  const res: GenericResponse<Campaign> = await apiClient(`/ad-campaigns/${id}/sync`, { method: "POST" });
  return res.success && res.data ? mapCampaign(res.data) : null;
}

export async function updateCampaignStatus(id: string, status: CampaignStatus): Promise<Campaign | null> {
  const res: GenericResponse<Campaign> = await apiClient(`/ad-campaigns/${id}`, {
    method: "PUT",
    data: { status },
  });
  return res.success && res.data ? mapCampaign(res.data) : null;
}

export async function updateCampaign(id: string, data: CreateCampaignData): Promise<Campaign | null> {
  const res: GenericResponse<Campaign> = await apiClient(`/ad-campaigns/${id}`, {
    method: "PUT",
    data: {
      brandId: data.brandId,
      name: data.name,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate,
      endDate: data.endDate,
    },
  });
  return res.success && res.data ? mapCampaign(res.data) : null;
}

export async function deleteCampaign(id: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/ad-campaigns/${id}`, { method: "DELETE" });
  return res.success === true;
}

export async function getCampaignById(id: string): Promise<Campaign | null> {
  const res: GenericResponse<Campaign> = await apiClient(`/ad-campaigns/${id}`);
  return res.success && res.data ? mapCampaign(res.data) : null;
}
