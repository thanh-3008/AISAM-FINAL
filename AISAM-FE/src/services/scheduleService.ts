import { apiClient } from "@/lib/apiClient";

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
  title: string | null;
  brandName: string | null;
  type: string | null;
  platform: string | null;
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

/* ─── API Functions ─── */

export async function fetchSchedules(params?: {
  page?: number;
  pageSize?: number;
}): Promise<{ data: PagedResult<ScheduleItem>; error?: string }> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    const res: GenericResponse<PagedResult<ScheduleItem>> = await apiClient(`/content-schedules?${query.toString()}`);
    if (res?.data) return { data: res.data };
    return { data: { data: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false }, error: res?.error?.errorMessage || res?.message || "Failed to fetch schedules" };
  } catch (err: any) {
    return { data: { data: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false }, error: err.message || "Failed to fetch schedules" };
  }
}

export async function fetchUpcomingSchedules(limit = 10): Promise<ScheduleItem[]> {
  try {
    const res: GenericResponse<ScheduleItem[]> = await apiClient(`/content-schedules/upcoming?limit=${limit}`);
    if (res?.data) return res.data;
  } catch { /* ignore */ }
  return [];
}

export async function createSchedule(data: {
  contentId: string;
  integrationId: string;
  scheduledAt: string;
}): Promise<{ data: ScheduleItem | null; error?: string }> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient("/content-schedules", {
      data,
      method: "POST",
    });
    if (res?.data) {
      dispatchScheduleChange();
      return { data: res.data };
    }
    if (res?.error?.errorMessage || res?.message) {
      return { data: null, error: res?.error?.errorMessage || res?.message || "Unknown error" };
    }
  } catch (err: any) { 
    return { data: null, error: err.message || "Failed to create schedule" };
  }
  return { data: null, error: "Failed to create schedule" };
}

export async function updateSchedule(id: string, data: {
  integrationId?: string;
  scheduledAt?: string;
}): Promise<{ success: boolean; error?: string }> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`, {
      data,
      method: "PUT",
    });
    if (res?.success) {
      dispatchScheduleChange();
      return { success: true };
    }
    return { success: false, error: res?.error?.errorMessage || res?.message || "Failed to update schedule" };
  } catch (err: any) {
    return { success: false, error: err.message || "Failed to update schedule" };
  }
}

export async function deleteSchedule(id: string): Promise<{ success: boolean; error?: string }> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/content-schedules/${id}`, { method: "DELETE" });
    if (res?.success) {
      dispatchScheduleChange();
      return { success: true };
    }
    return { success: false, error: res?.error?.errorMessage || res?.message || "Failed to delete schedule" };
  } catch (err: any) {
    return { success: false, error: err.message || "Failed to delete schedule" };
  }
}

export async function fetchScheduleById(id: string): Promise<ScheduleItem | null> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`);
    if (res?.data) return res.data;
  } catch { /* ignore */ }
  return null;
}

/* ─── Bulk Schedule ─── */

export interface BulkItemResult {
  contentId: string;
  success: boolean;
  error?: string;
}

interface BulkCreateResult {
  totalRequested: number;
  successCount: number;
  failedCount: number;
  results: BulkItemResult[];
}

export async function bulkCreateSchedules(data: {
  items: { contentId: string; integrationId: string; scheduledAt: string }[];
}): Promise<{ success: boolean; message?: string; results?: BulkItemResult[] }> {
  try {
    const res: GenericResponse<BulkCreateResult> = await apiClient("/content-schedules/bulk", {
      data,
      method: "POST",
    });
    if (res?.data) {
      dispatchScheduleChange();
      const r = res.data;
      return {
        success: true,
        message: `${r.successCount}/${r.totalRequested} schedules created.`,
        results: r.results,
      };
    }
    return { success: false, message: res?.message || res?.error?.errorMessage || "Failed to bulk schedule." };
  } catch (err: any) {
    return { success: false, message: err.message || "Failed to bulk schedule." };
  }
}