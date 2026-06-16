import { apiClient } from "@/lib/apiClient";
import type { ContentType } from "@/lib/mockContent";

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
  title?: string;
  brandName?: string;
  type?: ContentType;
  platform?: string;
}

interface GenericResponse<T> {
  success: boolean;
  data?: T;
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

type ApiDataOptions<TData> = RequestInit & { data?: TData };

export const SCHEDULE_CHANGE_EVENT = "aisam:schedule-changed";

export function dispatchScheduleChange() {
  if (typeof window !== "undefined") window.dispatchEvent(new CustomEvent(SCHEDULE_CHANGE_EVENT));
}

export function onScheduleChange(callback: () => void) {
  if (typeof window === "undefined") return () => {};
  window.addEventListener(SCHEDULE_CHANGE_EVENT, callback);
  return () => window.removeEventListener(SCHEDULE_CHANGE_EVENT, callback);
}

export async function fetchSchedules(params?: { page?: number; pageSize?: number }): Promise<PagedResult<ScheduleItem>> {
  const query = new URLSearchParams();
  if (params?.page) query.set("page", String(params.page));
  if (params?.pageSize) query.set("pageSize", String(params.pageSize));
  const res: GenericResponse<PagedResult<ScheduleItem>> = await apiClient(`/content-schedules?${query.toString()}`);
  return res.data ?? {
    data: [],
    totalCount: 0,
    page: params?.page ?? 1,
    pageSize: params?.pageSize ?? 50,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  };
}

export async function fetchUpcomingSchedules(limit = 10): Promise<ScheduleItem[]> {
  const res: GenericResponse<ScheduleItem[]> = await apiClient(`/content-schedules/upcoming?limit=${limit}`);
  return res.data ?? [];
}

export async function createSchedule(data: {
  contentId: string;
  integrationId: string;
  scheduledAt: string;
}): Promise<ScheduleItem | null> {
  const res: GenericResponse<ScheduleItem> = await apiClient(
    "/content-schedules",
    { data, method: "POST" } satisfies ApiDataOptions<typeof data>,
  );
  if (res.success) dispatchScheduleChange();
  return res.data ?? null;
}

export async function updateSchedule(id: string, data: {
  integrationId?: string;
  scheduledAt?: string;
}): Promise<boolean> {
  const res: GenericResponse<ScheduleItem> = await apiClient(
    `/content-schedules/${id}`,
    { data, method: "PUT" } satisfies ApiDataOptions<typeof data>,
  );
  if (res.success) dispatchScheduleChange();
  return Boolean(res.success);
}

export async function deleteSchedule(id: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/content-schedules/${id}`, { method: "DELETE" });
  if (res.success) dispatchScheduleChange();
  return Boolean(res.success);
}

export async function fetchScheduleById(id: string): Promise<ScheduleItem | null> {
  const res: GenericResponse<ScheduleItem> = await apiClient(`/content-schedules/${id}`);
  return res.data ?? null;
}
