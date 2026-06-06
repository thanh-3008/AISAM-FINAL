import { apiClient, apiFetch } from "@/lib/apiClient";
import { MOCK_CONTENT, MOCK_DETAILS, type ContentItem, type ContentDetail, type ContentType, type ContentStatus } from "@/lib/mockContent";

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

let mockCounter = 0;
const MOCK_PLATFORMS = ["facebook"];
const MOCK_TAGS: string[] = [];
const MOCK_HASHTAGS: string[] = [];

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
    if (data?.data?.length) {
      return {
        items: data.data.map((api) => apiItemToContentItem(api)),
        total: data.totalCount,
        page: data.page,
        pageSize: data.pageSize,
      };
    }
  } catch {
    // fallback to mock
  }
  return fallbackFetchContents(params);
}

function fallbackFetchContents(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  brandId?: string;
  adType?: number;
}): { items: ContentItem[]; total: number; page: number; pageSize: number } {
  let list = [...MOCK_CONTENT];
  if (params?.searchTerm) {
    const q = params.searchTerm.toLowerCase();
    list = list.filter((c) => c.title.toLowerCase().includes(q) || c.brandName.toLowerCase().includes(q));
  }
  const page = params?.page || 1;
  const pageSize = params?.pageSize || 20;
  const start = (page - 1) * pageSize;
  return { items: list.slice(start, start + pageSize), total: list.length, page, pageSize };
}

export async function fetchContentById(id: string): Promise<ContentDetail | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(`/content/${id}`);
    if (res?.success && res.data) {
      return apiItemToContentDetail(res.data);
    }
  } catch {
    // fallback
  }
  return MOCK_DETAILS[id] || null;
}

export async function createContent(data: CreateContentPayload): Promise<ContentItem | null> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient("/content", { data });
    if (res?.success && res.data) {
      return apiItemToContentItem(res.data);
    }
  } catch {
    // fallback
  }
  return fallbackCreateContent(data);
}

function fallbackCreateContent(data: CreateContentPayload): ContentItem {
  mockCounter++;
  const id = `mock-${Date.now()}-${mockCounter}`;
  const now = new Date().toISOString();
  const item: ContentItem = {
    id,
    title: data.title || "",
    brandName: "",
    productName: "",
    type: ADTYPE_TO_CONTENTTYPE[data.adType] || "TEXT",
    status: "Draft",
    thumbnail: data.imageUrl || "",
    createdAt: now,
    platforms: MOCK_PLATFORMS,
    tags: MOCK_TAGS,
    hashtags: MOCK_HASHTAGS,
  };
  MOCK_CONTENT.unshift(item);
  MOCK_DETAILS[id] = {
    ...item,
    updatedAt: now,
    textContent: data.textContent || undefined,
  };
  return item;
}

export async function updateContent(id: string, data: UpdateContentPayload): Promise<boolean> {
  try {
    const res: GenericResponse<ContentApiItem> = await apiClient(
      `/content/${id}`,
      { data, method: "PUT" } satisfies RequestInit & { data?: UpdateContentPayload },
    );
    if (res?.success) return true;
  } catch {
    // fallback
  }
  return fallbackUpdateContent(id, data);
}

function fallbackUpdateContent(id: string, data: UpdateContentPayload): boolean {
  const idx = MOCK_CONTENT.findIndex((c) => c.id === id);
  if (idx >= 0) {
    if (data.title !== undefined) MOCK_CONTENT[idx].title = data.title ?? "";
    if (data.adType !== undefined) MOCK_CONTENT[idx].type = ADTYPE_TO_CONTENTTYPE[data.adType] || "TEXT";
    if (data.imageUrl !== undefined) MOCK_CONTENT[idx].thumbnail = data.imageUrl ?? "";
    if (data.status !== undefined) MOCK_CONTENT[idx].status = API_STATUS_TO_STATUS[data.status] || MOCK_CONTENT[idx].status;
  }
  if (MOCK_DETAILS[id]) {
    if (data.textContent !== undefined) MOCK_DETAILS[id].textContent = data.textContent ?? undefined;
    if (data.status !== undefined) MOCK_DETAILS[id].status = API_STATUS_TO_STATUS[data.status] || MOCK_DETAILS[id].status;
    MOCK_DETAILS[id].updatedAt = new Date().toISOString();
  }
  return true;
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
  try {
    const res: GenericResponse<null> = await apiFetch(`/content/${id}`, { method: "DELETE" });
    if (res?.success) return true;
  } catch {
    // fallback
  }
  return fallbackDeleteContent(id);
}

function fallbackDeleteContent(id: string): boolean {
  const idx = MOCK_CONTENT.findIndex((c) => c.id === id);
  if (idx >= 0) MOCK_CONTENT.splice(idx, 1);
  delete MOCK_DETAILS[id];
  return true;
}

export async function restoreContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<null> = await apiClient(`/content/${id}/restore`);
    if (res?.success) return true;
  } catch {
    // fallback
  }
  return true;
}

/* ─── AI Draft ─── */

export async function generateAIDraft(prompt: string, brandId?: string, productId?: string): Promise<string | null> {
  try {
    const res: GenericResponse<{ draft: string }> = await apiClient("/ai/generate-draft", {
      data: { prompt: `${prompt}`, brandId, productId },
    });
    if (res?.success && res.data?.draft) return res.data.draft;
  } catch {
    // fallback
  }
  return null;
}

export async function chatWithAI(message: string, history?: { role: string; text: string }[]): Promise<string | null> {
  try {
    const res: GenericResponse<{ reply: string }> = await apiClient("/ai/chat", {
      data: { message, history: history || [] },
    });
    if (res?.success && res.data?.reply) return res.data.reply;
  } catch {
    // fallback
  }
  return null;
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
