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

/* ─── Schedule Change Event ─── */

export const SCHEDULE_CHANGE_EVENT = "aisam:schedule-changed";

export function dispatchScheduleChange() {
  if (typeof window !== "undefined") {
    window.dispatchEvent(new CustomEvent(SCHEDULE_CHANGE_EVENT));
  }
}

export function onScheduleChange(callback: () => void) {
  if (typeof window === "undefined") return () => {};
  window.addEventListener(SCHEDULE_CHANGE_EVENT, callback);
  return () => window.removeEventListener(SCHEDULE_CHANGE_EVENT, callback);
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
  const schedules = loadSchedules();
  const enriched = schedules.map(enrichFromMock);
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
    if (res?.data?.data) {
      const enriched = res.data.data.map(enrichFromMock);
      if (enriched.length > 0) {
        saveSchedules(enriched);
        MOCK_SCHEDULES.length = 0;
        MOCK_SCHEDULES.push(...enriched);
      }
      return { ...res.data, data: enriched };
    }
  } catch { /* fallback */ }
  return getMockPage(params?.page || 1, params?.pageSize || 50);
}

export async function fetchUpcomingSchedules(limit = 10): Promise<ScheduleItem[]> {
  try {
    const res: GenericResponse<ScheduleItem[]> = await apiClient(`/content-schedules/upcoming?limit=${limit}`);
    if (res?.data) {
      const enriched = res.data.map(enrichFromMock);
      if (enriched.length > 0) {
        const existing = loadSchedules();
        const newIds = new Set(enriched.map(s => s.id));
        const merged = [...enriched, ...existing.filter(s => !newIds.has(s.id))];
        saveSchedules(merged);
        MOCK_SCHEDULES.length = 0;
        MOCK_SCHEDULES.push(...merged);
      }
      return enriched;
    }
  } catch { /* fallback */ }
  const schedules = loadSchedules();
  return schedules
    .filter((s) => s.status === "Pending" || s.status === "Processing")
    .slice(0, limit)
    .map(enrichFromMock);
}

function isValidGuid(str: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);
}

function generateMockGuid(prefix: string): string {
  const hash = prefix.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  const hex = hash.toString(16).padStart(8, '0');
  return `${hex.slice(0, 8)}-${hex.slice(0, 4)}-4${hex.slice(1, 4)}-a${hex.slice(1, 4)}-${hex.padEnd(12, '0').slice(0, 12)}`;
}

export async function createSchedule(data: {
  contentId: string;
  integrationId: string;
  scheduledAt: string;
}): Promise<ScheduleItem | null> {
  const contentId = isValidGuid(data.contentId) ? data.contentId : generateMockGuid(data.contentId);
  const integrationId = isValidGuid(data.integrationId) ? data.integrationId : generateMockGuid(data.integrationId);
  
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(
      "/content-schedules",
      { data: { contentId, integrationId, scheduledAt: data.scheduledAt }, method: "POST" } satisfies ApiDataOptions<{ contentId: string; integrationId: string; scheduledAt: string }>,
    );
    if (res?.data) {
      const enriched = enrichFromMock(res.data);
      const schedules = loadSchedules();
      schedules.unshift(enriched);
      saveSchedules(schedules);
      MOCK_SCHEDULES.length = 0;
      MOCK_SCHEDULES.push(...schedules);
      dispatchScheduleChange();
      return enriched;
    }
  } catch { /* fallback */ }
  const platform = data.integrationId.split("-")[0] || "facebook";
  const mock: ScheduleItem = {
    id: `s${Date.now()}`,
    contentId,
    integrationId,
    scheduledAt: data.scheduledAt,
    executedAt: null,
    status: "Pending",
    attemptCount: 0,
    lastError: null,
    platform,
  };
  const enriched = enrichFromMock(mock);
  const schedules = loadSchedules();
  schedules.unshift(enriched);
  saveSchedules(schedules);
  MOCK_SCHEDULES.length = 0;
  MOCK_SCHEDULES.push(...schedules);
  dispatchScheduleChange();
  return enriched;
}

export async function updateSchedule(id: string, data: {
  integrationId?: string;
  scheduledAt?: string;
}): Promise<boolean> {
  const updateData: { integrationId?: string; scheduledAt?: string } = {};
  if (data.scheduledAt) updateData.scheduledAt = data.scheduledAt;
  if (data.integrationId) {
    updateData.integrationId = isValidGuid(data.integrationId) ? data.integrationId : generateMockGuid(data.integrationId);
  }
  
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(
      `/content-schedules/${id}`,
      { data: updateData, method: "PUT" } satisfies ApiDataOptions<typeof updateData>,
    );
    if (res?.success) {
      const schedules = loadSchedules();
      const idx = schedules.findIndex((s) => s.id === id);
      if (idx >= 0) {
        if (updateData.scheduledAt) schedules[idx].scheduledAt = updateData.scheduledAt;
        if (updateData.integrationId) {
          schedules[idx].integrationId = updateData.integrationId;
          schedules[idx].platform = updateData.integrationId.split("-")[0] || schedules[idx].platform;
        }
        saveSchedules(schedules);
        MOCK_SCHEDULES.length = 0;
        MOCK_SCHEDULES.push(...schedules);
      }
      dispatchScheduleChange();
      return true;
    }
  } catch { /* fallback */ }
  const schedules = loadSchedules();
  const idx = schedules.findIndex((s) => s.id === id);
  if (idx >= 0) {
    if (updateData.scheduledAt) schedules[idx].scheduledAt = updateData.scheduledAt;
    if (updateData.integrationId) {
      schedules[idx].integrationId = updateData.integrationId;
      schedules[idx].platform = updateData.integrationId.split("-")[0] || schedules[idx].platform;
    }
    saveSchedules(schedules);
    MOCK_SCHEDULES.length = 0;
    MOCK_SCHEDULES.push(...schedules);
    dispatchScheduleChange();
  }
  return idx >= 0;
}

export async function deleteSchedule(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/content-schedules/${id}`, { method: "DELETE" });
    if (res?.success) {
      const schedules = loadSchedules();
      const idx = schedules.findIndex((s) => s.id === id);
      if (idx >= 0) {
        schedules.splice(idx, 1);
        saveSchedules(schedules);
        MOCK_SCHEDULES.length = 0;
        MOCK_SCHEDULES.push(...schedules);
      }
      dispatchScheduleChange();
      return true;
    }
  } catch { /* fallback */ }
  const schedules = loadSchedules();
  const idx = schedules.findIndex((s) => s.id === id);
  if (idx >= 0) {
    schedules.splice(idx, 1);
    saveSchedules(schedules);
    MOCK_SCHEDULES.length = 0;
    MOCK_SCHEDULES.push(...schedules);
    dispatchScheduleChange();
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
