import { apiClient, apiFetch } from "@/lib/apiClient";
import { resolveApiMediaUrl } from "@/lib/apiBaseUrl";
import type { ContentType, ContentStatus } from "@/lib/contentConstants";
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
  status?: Extract<ContentApiStatus, 0 | 1>;
}

export interface ContentMediaUpload {
  url: string;
  fileName: string;
  contentType: string;
  size: number;
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
  status?: ContentApiStatus | ContentStatus;
}

export interface AiGeneration {
  aiGenerationId: string;
  contentId: string;
  generatedText: string | null;
  status: number;
  errorMessage?: string | null;
  createdAt: string;
}

export interface ImproveContentPayload {
  prompt?: string | null;
}

export interface ConversationListItem {
  id: string;
  profileId?: string;
  brandId?: string | null;
  brandName?: string | null;
  productId?: string | null;
  productName?: string | null;
  adType: AdType;
  title?: string | null;
  isActive: boolean;
  lastMessage?: string | null;
  lastMessageAt?: string | null;
  messageCount: number;
}

export interface ConversationMessage {
  id?: string;
  role?: string;
  content?: string;
  message?: string;
  createdAt?: string;
}

export interface ConversationDetail extends ConversationListItem {
  messages?: ConversationMessage[] | null;
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

function getFirstMediaUrl(value: string | null | undefined): string {
  if (!value) return "";
  const trimmed = value.trim();
  if (!trimmed) return "";

  if (trimmed.startsWith("[") && trimmed.endsWith("]")) {
    try {
      const parsed = JSON.parse(trimmed);
      if (Array.isArray(parsed)) {
        return parsed.find((item) => typeof item === "string" && item.trim().length > 0)?.trim() ?? "";
      }
    } catch {
      return "";
    }
  }

  return trimmed;
}

function apiItemToContentItem(api: ContentApiItem): ContentItem {
  const imageUrl = getFirstMediaUrl(api.imageUrl);
  const videoUrl = getFirstMediaUrl(api.videoUrl);
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: resolveApiMediaUrl(imageUrl || videoUrl),
    createdAt: api.createdAt,
    platforms: [],
    tags: [],
    hashtags: [],
  };
}

function apiItemToContentDetail(api: ContentApiItem): ContentDetail {
  const imageUrl = getFirstMediaUrl(api.imageUrl);
  const videoUrl = getFirstMediaUrl(api.videoUrl);
  return {
    id: api.id,
    title: api.title || "",
    brandId: api.brandId,
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: resolveApiMediaUrl(imageUrl || videoUrl),
    createdAt: api.createdAt,
    platforms: [],
    updatedAt: api.updatedAt,
    textContent: api.textContent,
    imageUrl: resolveApiMediaUrl(imageUrl) || undefined,
    videoUrl: resolveApiMediaUrl(videoUrl) || undefined,
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

export async function uploadContentMedia(file: File): Promise<ContentMediaUpload | null> {
  const formData = new FormData();
  formData.append("file", file);
  const res: GenericResponse<ContentMediaUpload> = await apiFetch("/content/media", {
    method: "POST",
    body: formData,
  });
  if (!res?.success || !res.data?.url) return null;
  return {
    ...res.data,
    url: resolveApiMediaUrl(res.data.url),
  };
}

export async function updateContent(id: string, data: UpdateContentPayload): Promise<boolean> {
  try {
    const { status: _unsupportedStatus, ...payload } = data;
    if (Object.keys(payload).length === 0) return false;
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`, { data: payload, method: "PUT" });
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

export async function rejectContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}/reject`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function deleteContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<null> = await apiFetch(`/content/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function publishContent(contentId: string, integrationId: string): Promise<{ success: boolean; error?: string }> {
  try {
    const quota: GenericResponse<{ postRemaining?: number }> = await apiClient("/quota/workspace/current");
    if ((quota?.data?.postRemaining ?? 0) <= 0) {
      return { success: false, error: "Post quota exceeded. Please upgrade or wait for quota reset." };
    }

    const res: GenericResponse<null> = await apiClient(`/content/${contentId}/publish/${integrationId}`, { method: "POST" });
    if (res?.success) return { success: true };
    return { success: false, error: res?.error?.errorMessage || res?.message || "Failed to publish. Please try again." };
  } catch (e: any) {
    return { success: false, error: e?.message || "Failed to publish. Please try again." };
  }
}

export async function publishContentDebug(contentId: string, integrationId: string): Promise<{ success: boolean; error?: string; status?: number; body?: string }> {
  try {
    const token = getToken();
    const workspace = (await import("@/stores/workspace-store")).getStoredActiveWorkspace();
    const profile = (await import("@/stores/profile-store")).getStoredActiveProfile();
    const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";
    const headers: Record<string, string> = {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
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
    return { success: false, error: e?.message || "Network error" };
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

export async function cloneContent(id: string): Promise<ContentItem | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}/clone`, { method: "POST" });
    if (res?.success && res.data) return apiItemToContentItem(res.data);
    return null;
  } catch {
    return null;
  }
}

export async function generateAIDraft(prompt: string, brandId: string, adType: AdType, productId?: string, title?: string): Promise<AiGeneration | null> {
  try {
    const res: GenericResponse<AiGeneration> = await apiClient("/ai/generate-draft", {
      data: { prompt, brandId, adType, productId, title },
    });
    if (res?.success && res.data) return res.data;
    return null;
  } catch {
    return null;
  }
}

export async function improveContent(contentId: string, payload: ImproveContentPayload = {}): Promise<AiGeneration | null> {
  try {
    const res: GenericResponse<AiGeneration> = await apiClient(`/ai/improve/${contentId}`, {
      method: "POST",
      data: payload,
    });
    return res?.success && res.data ? res.data : null;
  } catch {
    return null;
  }
}

export async function approveAIGeneration(aiGenerationId: string): Promise<ContentItem | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/ai/approve/${aiGenerationId}`, { method: "POST" });
    if (res?.success && res.data) return apiItemToContentItem(res.data);
    return null;
  } catch {
    return null;
  }
}

export async function getAIGenerations(contentId: string): Promise<AiGeneration[]> {
  try {
    const res: GenericResponse<AiGeneration[]> = await apiClient(`/ai/generations/${contentId}`);
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export async function chatWithAI(
  message: string,
  adType: AdType,
  brandId?: string,
  productId?: string,
  conversationId?: string,
  _history?: { role: string; text: string }[]
): Promise<{ text: string; conversationId: string; shouldCreateContent: boolean } | null> {
  try {
    const res: GenericResponse<{ response: string; conversationId: string; shouldCreateContent: boolean }> = await apiClient("/ai/chat", {
      data: { message, adType, brandId, productId, conversationId },
    });
    if (res?.success && res.data?.response) {
      return {
        text: res.data.response,
        conversationId: res.data.conversationId,
        shouldCreateContent: res.data.shouldCreateContent === true,
      };
    }
    return null;
  } catch {
    return null;
  }
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

export async function getConversations(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}): Promise<PagedResult<ConversationListItem> | null> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    if (params?.searchTerm) query.set("searchTerm", params.searchTerm);
    if (params?.sortBy) query.set("sortBy", params.sortBy);
    if (params?.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));
    const suffix = query.toString() ? `?${query.toString()}` : "";
    const res: GenericResponse<PagedResult<ConversationListItem>> = await apiClient(`/conversations${suffix}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function getConversationById(id: string): Promise<ConversationDetail | null> {
  try {
    const res: GenericResponse<ConversationDetail> = await apiClient(`/conversations/${id}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function deleteConversation(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient(`/conversations/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
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
