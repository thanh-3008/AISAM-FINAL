import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
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
}

export interface NotificationDetail extends NotificationListItem {
  profileId: string;
}

export interface UnreadCount {
  count: number;
}

// Map BE notification type names to FE display types
const BE_TYPE_MAP: Record<string, string> = {
  ApprovalNeeded: "APPROVAL",
  PostScheduled: "CONTENT_PUBLISHED",
  PerformanceAlert: "CAMPAIGN",
  AiSuggestion: "AI_SUGGESTION",
  SystemUpdate: "SYSTEM",
};

function mapNotificationType(beType: string): string {
  return BE_TYPE_MAP[beType] ?? beType;
}

export async function getNotifications(page = 1, pageSize = 20): Promise<PagedResult<NotificationListItem> | null> {
  try {
    const res: GenericResponse<PagedResult<NotificationListItem>> = await apiClient(
      `/notifications?page=${page}&pageSize=${pageSize}`
    );
    if (res?.data) {
      res.data.data = (res.data.data || []).map((n) => ({
        ...n,
        type: mapNotificationType(n.type),
        createdAt: typeof n.createdAt === "string" ? n.createdAt : new Date(n.createdAt).toISOString(),
      }));
      return res.data;
    }
    return { data: [], totalCount: 0, page, pageSize, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
  } catch {
    return null;
  }
}

export async function getNotificationDetail(id: string): Promise<NotificationDetail | null> {
  try {
    const res: GenericResponse<NotificationDetail> = await apiClient(`/notifications/${id}`);
    if (res?.data) {
      res.data.type = mapNotificationType(res.data.type);
      res.data.createdAt = typeof res.data.createdAt === "string" ? res.data.createdAt : new Date(res.data.createdAt).toISOString();
      return res.data;
    }
    return null;
  } catch {
    return null;
  }
}

export async function markNotificationRead(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/notifications/${id}/mark-read`, {
      method: "POST",
    });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function markAllNotificationsRead(): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient("/notifications/mark-all-read", {
      method: "POST",
    });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function getUnreadCount(): Promise<number> {
  try {
    const res: GenericResponse<UnreadCount> = await apiClient("/notifications/unread-count");
    if (res?.data && res.data.count >= 0) return res.data.count;
    return 0;
  } catch {
    return 0;
  }
}

export async function deleteNotification(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/notifications/${id}`, {
      method: "DELETE",
    });
    return res?.success === true;
  } catch {
    return false;
  }
}
