import { apiClient } from "@/lib/apiClient";
import { type GenericResponse, type PagedResult, normalizeListResponse, unwrapApiData } from "@/lib/apiResponse";

export type AdCampaignStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "DRAFT";
export type AdCampaignObjective = "AWARENESS" | "TRAFFIC" | "ENGAGEMENT" | "LEADS" | "SALES" | "APP_PROMOTION";

export interface AdSetDto {
  id: string;
  name: string;
  facebookAdSetId?: string | null;
  dailyBudget?: number | null;
  status: string;
  impressions?: number;
  clicks?: number;
  spend?: number;
}

export interface AdCampaignDto {
  id: string;
  workspaceId: string;
  profileId: string;
  brandId: string;
  brandName?: string | null;
  adAccountId: string;
  facebookCampaignId?: string | null;
  name: string;
  objective: string;
  budget?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  isActive: boolean;
  isDeleted: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
  adSets: AdSetDto[];
  impressions?: number;
  clicks?: number;
  spend?: number;
  conversions?: number;
}

export interface CreateAdCampaignRequest {
  brandId: string;
  adAccountId: string;
  name: string;
  objective?: string;
  budget?: number | null;
  startDate?: string | null;
  endDate?: string | null;
}

export interface UpdateAdCampaignRequest {
  brandId?: string;
  adAccountId?: string;
  name?: string;
  objective?: string;
  budget?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  status?: string;
}

export interface AdCampaignQuery {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  brandId?: string;
  isActive?: boolean;
}

export interface AdCampaignPage {
  items: AdCampaignDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const queryDefaultPageSize = 50;

function normalizeCampaignPage(
  response: GenericResponse<PagedResult<AdCampaignDto> | AdCampaignDto[]>
): AdCampaignPage {
  const data = unwrapApiData(response);
  if (Array.isArray(data)) {
    return { items: data, totalCount: data.length, page: 1, pageSize: data.length || queryDefaultPageSize };
  }

  if (data && typeof data === "object" && Array.isArray((data as PagedResult<AdCampaignDto>).data)) {
    const paged = data as PagedResult<AdCampaignDto>;
    return {
      items: paged.data,
      totalCount: paged.totalCount ?? paged.totalItems ?? paged.total ?? paged.data.length,
      page: paged.page ?? 1,
      pageSize: paged.pageSize ?? paged.data.length,
    };
  }

  return { items: [], totalCount: 0, page: 1, pageSize: queryDefaultPageSize };
}

function buildQueryString(query: AdCampaignQuery = {}) {
  const params = new URLSearchParams();
  params.set("page", String(query.page ?? 1));
  params.set("pageSize", String(query.pageSize ?? queryDefaultPageSize));
  if (query.searchTerm) params.set("searchTerm", query.searchTerm);
  if (query.sortBy) params.set("sortBy", query.sortBy);
  if (query.sortDescending !== undefined) params.set("sortDescending", String(query.sortDescending));
  if (query.brandId) params.set("brandId", query.brandId);
  if (query.isActive !== undefined) params.set("isActive", String(query.isActive));
  return params.toString();
}

export async function fetchAdCampaignPage(query: AdCampaignQuery = {}): Promise<AdCampaignPage> {
  const res: GenericResponse<PagedResult<AdCampaignDto> | AdCampaignDto[]> = await apiClient(`/ad-campaigns?${buildQueryString(query)}`);
  return normalizeCampaignPage(res);
}

export async function fetchAdCampaigns(query: AdCampaignQuery = {}): Promise<AdCampaignDto[]> {
  const res: GenericResponse<PagedResult<AdCampaignDto> | AdCampaignDto[]> = await apiClient(`/ad-campaigns?${buildQueryString(query)}`);
  return normalizeListResponse(res);
}

export async function getAdCampaign(id: string): Promise<AdCampaignDto | null> {
  const res: GenericResponse<AdCampaignDto> = await apiClient(`/ad-campaigns/${id}`);
  return unwrapApiData(res);
}

export async function createAdCampaign(payload: CreateAdCampaignRequest): Promise<AdCampaignDto | null> {
  const res: GenericResponse<AdCampaignDto> = await apiClient("/ad-campaigns", {
    method: "POST",
    data: payload,
  });
  return unwrapApiData(res);
}

export async function updateAdCampaign(id: string, payload: UpdateAdCampaignRequest): Promise<AdCampaignDto | null> {
  const res: GenericResponse<AdCampaignDto> = await apiClient(`/ad-campaigns/${id}`, {
    method: "PUT",
    data: payload,
  });
  return unwrapApiData(res);
}

export async function deleteAdCampaign(id: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/ad-campaigns/${id}`, { method: "DELETE" });
  return res?.success === true;
}

export async function syncAdCampaign(id: string): Promise<AdCampaignDto | null> {
  const res: GenericResponse<AdCampaignDto> = await apiClient(`/ad-campaigns/${id}/sync`, { method: "POST" });
  return unwrapApiData(res);
}
