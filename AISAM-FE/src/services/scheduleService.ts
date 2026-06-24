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
}): Promise<PagedResult<ScheduleItem>> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    const res: GenericResponse<PagedResult<ScheduleItem>> = await apiClient(`/content-schedules?${query.toString()}`);
    if (res?.data) return res.data;
  } catch { /* ignore */ }
  return { data: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
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
}): Promise<boolean> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`, {
      data,
      method: "PUT",
    });
    if (res?.success) {
      dispatchScheduleChange();
      return true;
    }
  } catch { /* ignore */ }
  return false;
}

export async function deleteSchedule(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/content-schedules/${id}`, { method: "DELETE" });
    if (res?.success) {
      dispatchScheduleChange();
      return true;
    }
  } catch { /* ignore */ }
  return false;
}

export async function fetchScheduleById(id: string): Promise<ScheduleItem | null> {
  try {
    const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`);
    if (res?.data) return res.data;
  } catch { /* ignore */ }
  return null;
}