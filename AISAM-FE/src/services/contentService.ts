import { apiClient } from "@/lib/apiClient";
import type { ContentItem, ContentDetail, ContentType, ContentStatus } from "@/lib/mockContent";

/* ─── Generic API response types ─── */

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

/* ─── Types matching BE ContentResponseDto ─── */

export type AdType = 0 | 1 | 2; // 0=TextOnly, 1=ImageText, 2=VideoText
export type ContentApiStatus = 0 | 1 | 2 | 3 | 4; // 0=Draft, 1=PendingApproval, 2=Approved, 3=Rejected, 4=Published

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

/* ─── Mappers ─── */

export const ADTYPE_TO_CONTENTTYPE: Record<AdType, ContentType> = { 0: "TEXT", 1: "IMAGE", 2: "VIDEO" };
export const CONTENTTYPE_TO_ADTYPE: Record<ContentType, AdType> = { TEXT: 0, IMAGE: 1, VIDEO: 2 };

const API_STATUS_TO_STATUS: Record<ContentApiStatus, ContentStatus> = {
  0: "Draft",
  1: "Awaiting Approval",
  2: "Awaiting Approval",
  3: "Draft",
  4: "Published",
};
function apiItemToContentItem(api: ContentApiItem, platforms: string[] = ["facebook"], tags: string[] = [], hashtags: string[] = []): ContentItem {
  return {
    id: api.id,
    title: api.title || "",
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: api.imageUrl || "",
    createdAt: api.createdAt,
    platforms,
    tags,
    hashtags,
  };
}

function apiItemToContentDetail(api: ContentApiItem, extra?: Partial<ContentDetail>): ContentDetail {
  return {
    id: api.id,
    title: api.title || "",
    brandName: api.brandName || "",
    productName: api.productName || "",
    type: ADTYPE_TO_CONTENTTYPE[api.adType] || "TEXT",
    status: API_STATUS_TO_STATUS[api.status] || "Draft",
    thumbnail: api.imageUrl || "",
    createdAt: api.createdAt,
    platforms: [],
    updatedAt: api.updatedAt,
    textContent: api.textContent,
    imageUrl: api.imageUrl || undefined,
    videoUrl: api.videoUrl || undefined,
    ...extra,
  };
}

/* ─── Service Functions ─── */

export async function fetchContents(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  brandId?: string;
  adType?: number;
  status?: number;
}): Promise<{ items: ContentItem[]; total: number; page: number; pageSize: number }> {
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
  const data = res.data;
  return {
    items: data?.data?.map((item) => apiItemToContentItem(item)) ?? [],
    total: data?.totalCount ?? 0,
    page: data?.page ?? params?.page ?? 1,
    pageSize: data?.pageSize ?? params?.pageSize ?? 20,
  };
}

export async function fetchContentById(id: string): Promise<ContentDetail | null> {
  const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`);
  return res.success && res.data ? apiItemToContentDetail(res.data) : null;
}

export async function createContent(data: CreateContentPayload): Promise<ContentItem | null> {
  const res: GenericResponse<ContentApiItem> = await apiClient("/content", { data });
  return res.success && res.data ? apiItemToContentItem(res.data) : null;
}

export async function updateContent(id: string, data: UpdateContentPayload): Promise<boolean> {
  const res: GenericResponse<ContentApiItem> = await apiClient(
    `/content/${id}`,
    { data, method: "PUT" } satisfies RequestInit & { data?: UpdateContentPayload },
  );
  return Boolean(res.success);
}

export async function approveContent(id: string): Promise<boolean> {
  return updateContent(id, { status: 2 }); // 2 = Approved
}

export async function rejectContent(id: string): Promise<boolean> {
  return updateContent(id, { status: 3 }); // 3 = Rejected
}

export async function requestApproval(id: string): Promise<boolean> {
  return updateContent(id, { status: 1 }); // 1 = PendingApproval
}

export async function deleteContent(id: string): Promise<boolean> {
  const res: GenericResponse<null> = await apiClient(`/content/${id}`, { method: "DELETE" });
  return Boolean(res.success);
}

export async function restoreContent(id: string): Promise<boolean> {
  const res: GenericResponse<null> = await apiClient(`/content/${id}/restore`, { method: "POST" });
  return Boolean(res.success);
}

export async function cloneContent(id: string): Promise<ContentItem | null> {
  const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}/clone`, {
    method: "POST",
  });
  return res.success && res.data ? apiItemToContentItem(res.data) : null;
}

export async function publishContent(id: string, integrationId: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient(`/content/${id}/publish/${integrationId}`, {
      method: "POST",
    });
    return Boolean(res?.success);
  } catch {
    return false;
  }
}

/* ─── AI Draft ─── */

export async function generateAIDraft(prompt: string, brandId?: string, productId?: string): Promise<string | null> {
  try {
    const res: GenericResponse<{ generatedText?: string; draft?: string }> = await apiClient("/ai/generate-draft", {
      data: { prompt, brandId, productId, adType: 0 },
    });
    if (res?.success && (res.data?.generatedText || res.data?.draft)) {
      return res.data.generatedText ?? res.data.draft ?? null;
    }
  } catch {
    // API generation failed; keep caller-visible null.
  }
  return null;
}

export interface ChatWithAIOptions {
  brandId?: string;
  productId?: string;
  adType?: AdType;
  conversationId?: string;
}

export async function chatWithAI(
  message: string,
  _history?: { role: string; text: string }[],
  options: ChatWithAIOptions = {}
): Promise<string | null> {
  try {
    const res: GenericResponse<{ response?: string; reply?: string; conversationId?: string }> = await apiClient("/ai/chat", {
      data: {
        brandId: options.brandId,
        productId: options.productId,
        adType: options.adType ?? 0,
        message,
        conversationId: options.conversationId,
      },
    });
    if (res?.success && (res.data?.response || res.data?.reply)) {
      return res.data.response ?? res.data.reply ?? null;
    }
  } catch {
    // API chat failed; keep caller-visible null.
  }
  return null;
}

export interface AiGeneration {
  aiGenerationId: string;
  contentId: string;
  generatedText?: string | null;
  status: number;
  errorMessage?: string | null;
  createdAt: string;
}

export async function improveContent(contentId: string, prompt: string): Promise<AiGeneration | null> {
  try {
    const res: GenericResponse<AiGeneration> = await apiClient(`/ai/improve/${contentId}`, {
      method: "POST",
      data: { prompt },
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function approveAIGeneration(aiGenerationId: string): Promise<ContentDetail | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/ai/approve/${aiGenerationId}`, {
      method: "POST",
    });
    if (res?.data) return apiItemToContentDetail(res.data);
  } catch {
    // API approval failed; keep caller-visible null.
  }
  return null;
}

export async function getAIGenerations(contentId: string): Promise<AiGeneration[]> {
  try {
    const res: GenericResponse<AiGeneration[]> = await apiClient(`/ai/generations/${contentId}`);
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export interface ConversationListItem {
  id: string;
  title?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ConversationDetail extends ConversationListItem {
  messages?: Array<{ role: string; content?: string; text?: string; createdAt?: string }>;
}

export async function getConversations(params?: { page?: number; pageSize?: number }): Promise<ConversationListItem[]> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    const suffix = query.toString() ? `?${query.toString()}` : "";
    const res: GenericResponse<PagedResult<ConversationListItem>> = await apiClient(`/conversations${suffix}`);
    return res?.data?.data ?? [];
  } catch {
    return [];
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
    return Boolean(res?.success);
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
  } catch { /* keep id when name lookup fails */ }
  return brandId;
}

export async function resolveProductName(productId: string): Promise<string> {
  try {
    const res: GenericResponse<{ id: string; name: string }> = await apiClient(`/products/${productId}`);
    if (res?.success && res.data?.name) return res.data.name;
  } catch { /* keep id when name lookup fails */ }
  return productId;
}
