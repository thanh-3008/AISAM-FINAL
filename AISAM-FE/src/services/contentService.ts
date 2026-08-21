import { apiClient, apiFetch } from "@/lib/apiClient";
import type { ContentType, ContentStatus } from "@/lib/contentConstants";
import { getStoredActiveProfile } from "@/stores/profile-store";
import { getToken } from "@/lib/auth";

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
export type ContentApiStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7;
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
  thumbnailUrl?: string | null;
  styleDescription: string | null;
  contextDescription: string | null;
  representativeCharacter: string | null;
  status: ContentApiStatus;
  isAiGenerated: boolean;
  tags: string | null;
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
  imageUrl?: string;
  videoUrl?: string;
  textContent?: string;
  createdAt: string;
  platforms: string[];
  tags: string[];
  hashtags: string[];
  isAiGenerated: boolean;
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
  thumbnailUrl?: string | null;
  styleDescription?: string | null;
  contextDescription?: string | null;
  representativeCharacter?: string | null;
  status?: Extract<ContentApiStatus, 0 | 1>;
  isAiGenerated?: boolean;
  tags?: string[];
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
  5: "Draft",
  6: "Rejected",
  7: "Failed",
};

const STATUS_TO_API_STATUS: Record<ContentStatus, ContentApiStatus> = {
  "Draft": 0,
  "Awaiting Approval": 1,
  "Approved": 2,
  "Rejected": 3,
  "Published": 4,
  "Scheduled": 4,
  "Failed": 7,
};

export const parseApiUrl = (url?: string | null): string => {
  const raw = (url || "").trim();
  if (!raw) return "";

  const pickUrl = (value: unknown): string => {
    if (!value) return "";
    if (typeof value === "string") return parseApiUrl(value);
    if (Array.isArray(value)) {
      for (const entry of value) {
        const parsed = pickUrl(entry);
        if (parsed) return parsed;
      }
      return "";
    }
    if (typeof value === "object") {
      const record = value as Record<string, unknown>;
      for (const key of ["url", "secure_url", "imageUrl", "image_url", "videoUrl", "video_url", "thumbnailUrl", "src"]) {
        const parsed = pickUrl(record[key]);
        if (parsed) return parsed;
      }
    }
    return "";
  };

  try {
    const parsed = JSON.parse(raw);
    const parsedUrl = pickUrl(parsed);
    if (parsedUrl) return parsedUrl;
  } catch {
    // Legacy records may be plain URLs; fall through to URL extraction.
  }

  const match = raw.match(/https?:\/\/[^"'\]\s,}]+/i);
  return match?.[0] || raw;
};

export function apiItemToContentItem(api: ContentApiItem): ContentItem {
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: parseApiUrl(api.thumbnailUrl) || parseApiUrl(api.imageUrl) || parseApiUrl(api.videoUrl) || "",
    imageUrl: parseApiUrl(api.imageUrl) || undefined,
    videoUrl: parseApiUrl(api.videoUrl) || undefined,
    textContent: api.textContent || "",
    createdAt: api.createdAt,
    platforms: [],
    tags: api.tags ? JSON.parse(api.tags) : [],
    hashtags: [],
    isAiGenerated: api.isAiGenerated,
  };
}

export function apiItemToContentDetail(api: ContentApiItem): ContentDetail {
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: parseApiUrl(api.thumbnailUrl) || parseApiUrl(api.imageUrl) || parseApiUrl(api.videoUrl) || "",
    createdAt: api.createdAt,
    platforms: [],
    updatedAt: api.updatedAt,
    textContent: api.textContent,
    imageUrl: parseApiUrl(api.imageUrl) || undefined,
    videoUrl: parseApiUrl(api.videoUrl) || undefined,
    description: api.contextDescription || undefined,
    tags: api.tags ? JSON.parse(api.tags) : [],
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

export async function updateContentDetails(id: string, payload: Partial<CreateContentPayload>): Promise<ContentApiItem | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`, {
      method: "PUT",
      data: payload,
    });

    if (res?.success && res.data) {
      return res.data;
    }

    throw new Error(res?.message || "Failed to update content");
  } catch (error) {
    throw error;
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

export async function submitForApproval(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<null> = await apiClient(`/content/${id}/submit`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function approveContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}/approve`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function rejectContent(id: string, notes?: string): Promise<boolean> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}/reject`, {
      method: "POST",
      data: { notes: notes || "" }
    });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function deleteContent(id: string): Promise<boolean> {
  const result = await deleteContentWithResult(id);
  return result.success;
}

export async function deleteContentWithResult(id: string): Promise<{ success: boolean; error?: string }> {
  try {
    const res: GenericResponse<null> = await apiFetch(`/content/${id}`, { method: "DELETE" });
    if (res?.success === true) return { success: true };
    return { success: false, error: res?.error?.errorMessage || res?.message || "Failed to delete content" };
  } catch (error: any) {
    return { success: false, error: error?.message || "Failed to delete content" };
  }
}

export async function publishContent(contentId: string, integrationId: string): Promise<{ success: boolean; error?: string }> {
  try {
    const res: GenericResponse<null> = await apiClient(`/content/${contentId}/publish/${integrationId}`, { method: "POST" });
    return { success: res?.success === true };
  } catch (e: any) {
    return { success: false, error: e?.message || "Failed to publish. Please try again." };
  }
}

export async function publishContentDebug(contentId: string, integrationId: string): Promise<{ success: boolean; error?: string; status?: number; body?: string }> {
  try {
    const token = getToken();
    const workspace = (await import('@/stores/workspace-store')).getStoredActiveWorkspace();
    const profile = (await import('@/stores/profile-store')).getStoredActiveProfile();
    const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";
    const headers: Record<string, string> = {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
      ...(profile ? { "X-Profile-Id": profile.id } : {}),
      "Content-Type": "application/json",
    };
    const response = await fetch(`${API_URL}/content/${contentId}/publish/${integrationId}`, { method: "POST", headers });
    const bodyText = await response.text();
    return {
      success: response.ok,
      status: response.status,
      body: bodyText,
      error: response.ok ? undefined : `HTTP ${response.status}: ${response.statusText}`
    };
  } catch (e: any) {
    return { success: false, error: e?.message || "Network error. Please check your connection" };
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
  _history?: { role: string; text: string }[],
  options?: {
    generationMode?: "exact_product_reference" | "normal_generation";
    uploadedPrimaryImageUrl?: string | null;
    selectedProductImageUrl?: string | null;
    useOriginalProductImages?: boolean;
  }
): Promise<{ text?: string; conversationId?: string; shouldCreateContent?: boolean; createdContentId?: string; errorMessage?: string } | null> {
  try {
    const res: GenericResponse<{ response: string; conversationId: string; shouldCreateContent: boolean; createdContentId?: string }> = await apiClient("/ai/chat", {
      data: {
        message,
        adType,
        brandId,
        productId,
        conversationId,
        generationMode: options?.generationMode,
        uploadedPrimaryImageUrl: options?.uploadedPrimaryImageUrl ?? null,
        selectedProductImageUrl: options?.selectedProductImageUrl ?? null,
        useOriginalProductImages: options?.useOriginalProductImages === true,
        userPrompt: message,
      },
    });
    if (res?.success && res.data?.response) {
      return {
        text: res.data.response,
        conversationId: res.data.conversationId,
        shouldCreateContent: res.data.shouldCreateContent === true,
        createdContentId: res.data.createdContentId,
      };
    }
    return { errorMessage: res?.message || "AI service request failed." };
  } catch (error) {
    return { errorMessage: error instanceof Error ? error.message : "AI service request failed." };
  }
}

export async function uploadContentMedia(file: File): Promise<string> {
  const formData = new FormData();
  formData.append("File", file);

  const res: GenericResponse<{ url: string }> = await apiFetch("/content/media", {
    method: "POST",
    body: formData,
  });

  if (res?.success && res.data?.url) return res.data.url;
  throw new Error(res?.message || res?.error?.errorMessage || "Failed to upload media.");
}

export async function getConversationMessages(
  conversationId: string
): Promise<{ senderType: number; message: string; createdAt: string }[] | null> {
  try {
    const res: GenericResponse<{ id: string; messages: { senderType: number; message: string; createdAt: string }[] }> = await apiClient(`/conversations/${conversationId}`);
    if (res?.success && res.data?.messages) return res.data.messages;
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

export interface AiGenerationResponse {
  id: string;
  status: number; // 0: Pending, 1: Completed, 2: Failed, 3: Processing
  errorMessage?: string;
  generatedVideoUrl?: string;
  createdAt: string;
}

export async function fetchContentGenerations(contentId: string): Promise<AiGenerationResponse[]> {
  try {
    const res: GenericResponse<AiGenerationResponse[]> = await apiClient(`/ai/generations/${contentId}`);
    if (res?.success && res.data) return res.data;
    return [];
  } catch {
    return [];
  }
}
