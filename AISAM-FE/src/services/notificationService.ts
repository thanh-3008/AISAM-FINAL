import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  data?: T;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface NotificationListItem {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  actionUrl: string | null;
}

export interface NotificationDetail extends NotificationListItem {
  metadata: Record<string, string> | null;
}

export interface UnreadCount {
  count: number;
}

const BE_TYPE_MAP: Record<string, string> = {
  ApprovalNeeded: "APPROVAL",
  PostScheduled: "CONTENT_PUBLISHED",
  PerformanceAlert: "CAMPAIGN",
  AiSuggestion: "AI_SUGGESTION",
  SystemUpdate: "SYSTEM",
};

function mapNotification<T extends NotificationListItem>(notification: T): T {
  return {
    ...notification,
    type: BE_TYPE_MAP[notification.type] ?? notification.type,
    createdAt: new Date(notification.createdAt).toISOString(),
    actionUrl: notification.actionUrl ?? null,
  };
}

export async function getNotifications(page = 1, pageSize = 20): Promise<PagedResult<NotificationListItem> | null> {
  const res: GenericResponse<PagedResult<NotificationListItem>> = await apiClient(
    `/notifications?page=${page}&pageSize=${pageSize}`,
  );
  if (!res.success || !res.data) return null;
  return { ...res.data, data: res.data.data.map(mapNotification) };
}

export async function getNotificationDetail(id: string): Promise<NotificationDetail | null> {
  const res: GenericResponse<NotificationDetail> = await apiClient(`/notifications/${id}`);
  return res.success && res.data ? mapNotification(res.data) : null;
}

export async function markNotificationRead(id: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/notifications/${id}/mark-read`, { method: "POST" });
  return Boolean(res.success);
}

export async function markAllNotificationsRead(): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient("/notifications/mark-all-read", { method: "POST" });
  return Boolean(res.success);
}

export async function getUnreadCount(): Promise<number> {
  const res: GenericResponse<UnreadCount> = await apiClient("/notifications/unread-count");
  return res.data?.count ?? 0;
}

export async function deleteNotification(_id: string): Promise<boolean> {
  return false;
}
