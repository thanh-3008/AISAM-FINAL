import { apiClient } from "@/lib/apiClient";
import { MOCK_CONTENT, type ContentType } from "@/lib/mockContent";
import { PLATFORM_CONFIG } from "@/lib/contentConstants";

export type ScheduleStatus = "Pending" | "Processing" | "Completed" | "Failed";

export interface ScheduleItem {
  id: string;
  contentId: string;
  integrationId: string;
  scheduledAt: string;
  executedAt: string | null;
  status: ScheduleStatus;
  attemptCount: number;
  lastError: string | null;
  // Enriched fields (from join)
  title?: string;
  brandName?: string;
  type?: ContentType;
  platform?: string;
}

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

/* ─── Mock Data ─── */

const STORAGE_KEY = "aisam_schedules";

const INITIAL_MOCK_SCHEDULES: ScheduleItem[] = [
  { id: "s1", contentId: "c1", integrationId: "fb-1", scheduledAt: new Date(Date.now() + 86400000).toISOString(), executedAt: null, status: "Pending", attemptCount: 0, lastError: null, title: "Smart Bulb Product Showcase", brandName: "Lumina Tech", type: "VIDEO", platform: "facebook" },
  { id: "s2", contentId: "c3", integrationId: "ig-1", scheduledAt: new Date(Date.now() + 172800000).toISOString(), executedAt: null, status: "Pending", attemptCount: 0, lastError: null, title: "Midnight Blue Desk Lamp Ad", brandName: "Lumina Tech", type: "IMAGE", platform: "instagram" },
  { id: "s3", contentId: "c5", integrationId: "li-1", scheduledAt: new Date(Date.now() - 86400000).toISOString(), executedAt: new Date(Date.now() - 82800000).toISOString(), status: "Completed", attemptCount: 1, lastError: null, title: "TrailBlazer Backpack Review", brandName: "Summit Outdoor", type: "TEXT", platform: "linkedin" },
  { id: "s4", contentId: "c8", integrationId: "ig-1", scheduledAt: new Date(Date.now() + 259200000).toISOString(), executedAt: null, status: "Pending", attemptCount: 0, lastError: null, title: "All-Terrain Tire Review", brandName: "Heritage Motors", type: "IMAGE", platform: "instagram" },
  { id: "s5", contentId: "c11", integrationId: "li-1", scheduledAt: new Date(Date.now() + 345600000).toISOString(), executedAt: null, status: "Pending", attemptCount: 0, lastError: null, title: "Budget App Feature Overview", brandName: "Pulse Finance", type: "VIDEO", platform: "linkedin" },
  { id: "s6", contentId: "c2", integrationId: "fb-1", scheduledAt: new Date(Date.now() - 172800000).toISOString(), executedAt: new Date(Date.now() - 166800000).toISOString(), status: "Completed", attemptCount: 1, lastError: null, title: "LED Strip Installation Guide", brandName: "Lumina Tech", type: "TEXT", platform: "facebook" },
  { id: "s7", contentId: "c9", integrationId: "ig-1", scheduledAt: new Date(Date.now() + 432000000).toISOString(), executedAt: null, status: "Pending", attemptCount: 0, lastError: null, title: "Organic Tea - From Farm to Cup", brandName: "GreenLeaf Organics", type: "TEXT", platform: "instagram" },
  { id: "s8", contentId: "c7", integrationId: "fb-1", scheduledAt: new Date(Date.now() + 518400000).toISOString(), executedAt: null, status: "Pending", attemptCount: 1, lastError: "Rate limit exceeded", title: "Heritage V8 Engine Rebuild", brandName: "Heritage Motors", type: "VIDEO", platform: "facebook" },
];

function loadSchedules(): ScheduleItem[] {
  if (typeof window === "undefined") return [...INITIAL_MOCK_SCHEDULES];
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as ScheduleItem[];
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    }
  } catch { /* fallback */ }
  const initial = [...INITIAL_MOCK_SCHEDULES];
  localStorage.setItem(STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

function saveSchedules(schedules: ScheduleItem[]): void {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(schedules));
  } catch { /* ignore quota errors */ }
}

const MOCK_SCHEDULES: ScheduleItem[] = loadSchedules();

/* ─── Loader helpers ─── */

function enrichFromMock(s: ScheduleItem): ScheduleItem {
  if (s.title) return s;
  const content = MOCK_CONTENT.find((c) => c.id === s.contentId);
  if (!content) return s;
  const platformEntry = Object.entries(PLATFORM_CONFIG).find(([, v]) => v.label.toLowerCase() === s.platform?.toLowerCase());
  return {
    ...s,
    title: content.title,
    brandName: content.brandName,
    type: content.type,
    platform: platformEntry?.[0] || s.platform || "facebook",
  };
}

type ApiDataOptions<TData> = RequestInit & { data?: TData };

function getMockPage(page: number, pageSize: number): PagedResult<ScheduleItem> {
  const enriched = MOCK_SCHEDULES.map(enrichFromMock);
  const totalCount = enriched.length;
  const data = enriched.slice((page - 1) * pageSize, page * pageSize);
  return {
    data,
    totalCount,
    page,
    pageSize,
    totalPages: Math.ceil(totalCount / pageSize),
    hasNextPage: page * pageSize < totalCount,
    hasPreviousPage: page > 1,
  };
}

/* ─── API Functions ─── */

export async function fetchSchedules(params?: {
  page?: number;
  pageSize?: number;
}): Promise<PagedResult<ScheduleItem>> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    const res: GenericResponse<PagedResult<ScheduleItem>> = await apiClient(`/content-schedules?${query.toString()}`);
    if (res?.data?.data) return res.data;
  } catch { /* fallback */ }
  return getMockPage(params?.page || 1, params?.pageSize || 50);
}

export async function fetchUpcomingSchedules(limit = 10): Promise<ScheduleItem[]> {
  try {
    const res: GenericResponse<ScheduleItem[]> = await apiClient(`/content-schedules/upcoming?limit=${limit}`);
    if (res?.data) return res.data.map(enrichFromMock);
  } catch { /* fallback */ }
  return MOCK_SCHEDULES
    .filter((s) => s.status === "Pending" || s.status === "Processing")
    .slice(0, limit)
    .map(enrichFromMock);
}

export async function createSchedule(data: {
  contentId: string;
  integrationId: string;
  scheduledAt: string;
}): Promise<ScheduleItem | null> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(
      "/content-schedules",
      { data, method: "POST" } satisfies ApiDataOptions<typeof data>,
    );
    if (res?.data) return enrichFromMock(res.data);
  } catch { /* fallback */ }
  const mock: ScheduleItem = {
    id: `s${Date.now()}`,
    contentId: data.contentId,
    integrationId: data.integrationId,
    scheduledAt: data.scheduledAt,
    executedAt: null,
    status: "Pending",
    attemptCount: 0,
    lastError: null,
  };
  const enriched = enrichFromMock(mock);
  MOCK_SCHEDULES.unshift(enriched);
  saveSchedules(MOCK_SCHEDULES);
  return enriched;
}

export async function updateSchedule(id: string, data: {
  integrationId?: string;
  scheduledAt?: string;
}): Promise<boolean> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(
      `/content-schedules/${id}`,
      { data, method: "PUT" } satisfies ApiDataOptions<typeof data>,
    );
    if (res?.success) return true;
  } catch { /* fallback */ }
  const idx = MOCK_SCHEDULES.findIndex((s) => s.id === id);
  if (idx >= 0) {
    if (data.scheduledAt) MOCK_SCHEDULES[idx].scheduledAt = data.scheduledAt;
    if (data.integrationId) MOCK_SCHEDULES[idx].integrationId = data.integrationId;
    saveSchedules(MOCK_SCHEDULES);
  }
  return idx >= 0;
}

export async function deleteSchedule(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/content-schedules/${id}`, { method: "DELETE" });
    if (res?.success) return true;
  } catch { /* fallback */ }
  const idx = MOCK_SCHEDULES.findIndex((s) => s.id === id);
  if (idx >= 0) {
    MOCK_SCHEDULES.splice(idx, 1);
    saveSchedules(MOCK_SCHEDULES);
  }
  return idx >= 0;
}

export async function fetchScheduleById(id: string): Promise<ScheduleItem | null> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`);
    if (res?.data) return enrichFromMock(res.data);
  } catch { /* fallback */ }
  const found = MOCK_SCHEDULES.find((s) => s.id === id);
  return found ? enrichFromMock(found) : null;
}
