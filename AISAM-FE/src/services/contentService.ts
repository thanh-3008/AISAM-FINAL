import { apiClient, apiFetch } from "@/lib/apiClient";
import type { ContentType, ContentStatus } from "@/lib/contentConstants";

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

export type AdType = 0 | 1 | 2;
export type ContentApiStatus = 0 | 1 | 2 | 3 | 4;
export type { ContentType, ContentStatus };

export interface ContentApiItem {
  id: string;
  profileId: string;
  brandId: string;
  brandName: string | null;
  productId: string | null;
  productName: string | null;
  adType: AdType;
  title: string | null;
  textContent: string;
  imageUrl: string | null;
  videoUrl: string | null;
  styleDescription: string | null;
  contextDescription: string | null;
  representativeCharacter: string | null;
  status: ContentApiStatus;
  createdAt: string;
  updatedAt: string;
}

export interface ContentItem {
  id: string;
  title: string;
  brandId: string;
  brandName: string;
  productName: string;
  type: ContentType;
  status: ContentStatus;
  thumbnail: string;
  createdAt: string;
  platforms: string[];
  tags: string[];
  hashtags: string[];
}

export interface ContentDetail {
  id: string;
  title: string;
  brandId: string;
  brandName: string;
  productName: string;
  type: ContentType;
  status: ContentStatus;
  thumbnail: string;
  createdAt: string;
  platforms: string[];
  updatedAt: string;
  textContent?: string;
  imageUrl?: string;
  videoUrl?: string;
  styleDescription?: string;
  contextDescription?: string;
  representativeCharacter?: string;
  description?: string;
  caption?: string;
  ctaLink?: string;
  scheduledAt?: string;
  internalNotes?: string;
  hashtags?: string[];
  tags?: string[];
  duration?: string;
  fileSize?: string;
  dimensions?: string;
}

export interface CreateContentPayload {
  brandId: string;
  productId?: string | null;
  adType: AdType;
  title?: string | null;
  textContent: string;
  imageUrl?: string | null;
  videoUrl?: string | null;
  styleDescription?: string | null;
  contextDescription?: string | null;
  representativeCharacter?: string | null;
}

export interface UpdateContentPayload {
  productId?: string | null;
  adType?: AdType;
  title?: string | null;
  textContent?: string | null;
  imageUrl?: string | null;
  videoUrl?: string | null;
  styleDescription?: string | null;
  contextDescription?: string | null;
  representativeCharacter?: string | null;
  status?: ContentApiStatus;
}

export const ADTYPE_TO_CONTENTTYPE: Record<AdType, ContentType> = { 0: "TEXT", 1: "IMAGE", 2: "VIDEO" };
export const CONTENTTYPE_TO_ADTYPE: Record<ContentType, AdType> = { TEXT: 0, IMAGE: 1, VIDEO: 2 };

const API_STATUS_TO_STATUS: Record<ContentApiStatus, ContentStatus> = {
  0: "Draft",
  1: "Awaiting Approval",
  2: "Approved",
  3: "Rejected",
  4: "Published",
};

function apiItemToContentItem(api: ContentApiItem): ContentItem {
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: api.imageUrl || api.videoUrl || "",
    createdAt: api.createdAt,
    platforms: [],
    tags: [],
    hashtags: [],
  };
}

function apiItemToContentDetail(api: ContentApiItem): ContentDetail {
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: api.imageUrl || api.videoUrl || "",
    createdAt: api.createdAt,
    platforms: [],
    updatedAt: api.updatedAt,
    textContent: api.textContent,
    imageUrl: api.imageUrl || undefined,
    videoUrl: api.videoUrl || undefined,
    tags: [],
    hashtags: [],
  };
}

export async function fetchContents(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  brandId?: string;
  adType?: number;
  status?: number;
}): Promise<{ items: ContentItem[]; total: number; page: number; pageSize: number } | null> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    if (params?.searchTerm) query.set("searchTerm", params.searchTerm);
    if (params?.sortBy) query.set("sortBy", params.sortBy);
    if (params?.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));
    if (params?.brandId) query.set("brandId", params.brandId);
    if (params?.adType !== undefined) query.set("adType", String(params.adType));
    if (params?.status !== undefined) query.set("status", String(params.status));

    const res: GenericResponse<PagedResult<ContentApiItem>> = await apiClient(`/content?${query.toString()}`);
    const data = res?.data;
    if (data?.data) {
      return {
        items: data.data.map(apiItemToContentItem),
        total: data.totalCount,
        page: data.page,
        pageSize: data.pageSize,
      };
    }
    return null;
  } catch {
    return null;
  }
}

export async function fetchContentById(id: string): Promise<ContentDetail | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`);
    if (res?.success && res.data) {
      return apiItemToContentDetail(res.data);
    }
    return null;
  } catch {
    return null;
  }
}

export async function createContent(data: CreateContentPayload): Promise<ContentItem | null> {
  const res: GenericResponse<ContentApiItem> = await apiClient("/content", { data });
  if (res?.success && res.data) {
    return apiItemToContentItem(res.data);
  }
  return null;
}

export async function updateContent(id: string, data: UpdateContentPayload): Promise<boolean> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`, { data, method: "PUT" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function approveContent(id: string): Promise<boolean> {
  return updateContent(id, { status: 2 });
}

export async function rejectContent(id: string): Promise<boolean> {
  return updateContent(id, { status: 3 });
}

export async function deleteContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<null> = await apiFetch(`/content/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function restoreContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<null> = await apiClient(`/content/${id}/restore`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function generateAIDraft(prompt: string, brandId: string, adType: AdType, productId?: string, title?: string): Promise<string | null> {
  try {
    const res: GenericResponse<{ aiGenerationId: string; contentId: string; generatedText: string; status: number }> = await apiClient("/ai/generate-draft", {
      data: { prompt, brandId, adType, productId, title },
    });
    if (res?.success && res.data?.generatedText) return res.data.generatedText;
    return null;
  } catch {
    return null;
  }
}

export async function chatWithAI(
  message: string,
  adType: AdType,
  brandId?: string,
  productId?: string,
  conversationId?: string,
  _history?: { role: string; text: string }[]
): Promise<string | null> {
  try {
    const res: GenericResponse<{ response: string; conversationId: string }> = await apiClient("/ai/chat", {
      data: { message, adType, brandId, productId, conversationId },
    });
    if (res?.success && res.data?.response) return res.data.response;
    return null;
  } catch {
    return null;
  }
}

/* ─── Brand helpers for name resolution ─── */

const brandNameCache = new Map<string, string>();

export async function resolveBrandName(brandId: string): Promise<string> {
  if (brandNameCache.has(brandId)) return brandNameCache.get(brandId)!;
  try {
    const res: GenericResponse<{ id: string; name: string }> = await apiClient(`/brands/${brandId}`);
    if (res?.success && res.data?.name) {
      brandNameCache.set(brandId, res.data.name);
      return res.data.name;
    }
  } catch { /* fallback */ }
  return brandId;
}

export async function resolveProductName(productId: string): Promise<string> {
  try {
    const res: GenericResponse<{ id: string; name: string }> = await apiClient(`/products/${productId}`);
    if (res?.success && res.data?.name) return res.data.name;
  } catch { /* fallback */ }
  return productId;
}
