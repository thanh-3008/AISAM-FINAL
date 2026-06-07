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
  actionUrl: string | null;
}

export interface NotificationDetail extends NotificationListItem {
  metadata: Record<string, string> | null;
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

// Mock data for fallback (static timestamps to avoid hydration mismatch)
const MOCK_NOTIFICATIONS: NotificationListItem[] = [
  {
    id: "mock-1",
    type: "CONTENT_PUBLISHED",
    title: "Winter Collection post published",
    message: "Your post has been successfully published to Facebook and Instagram",
    isRead: false,
    createdAt: "2026-06-07T23:00:00.000Z",
    actionUrl: "/posts",
  },
  {
    id: "mock-2",
    type: "AI_SUGGESTION",
    title: "AI generated new content suggestions",
    message: "5 new content ideas based on trending topics in your niche",
    isRead: false,
    createdAt: "2026-06-07T22:47:00.000Z",
    actionUrl: "/content/ai-generate",
  },
  {
    id: "mock-3",
    type: "CAMPAIGN",
    title: "Campaign 'Flash Sale' ends tomorrow",
    message: "Your campaign will end in 24 hours. Current performance: +12% ROAS",
    isRead: false,
    createdAt: "2026-06-07T22:02:00.000Z",
    actionUrl: "/campaigns",
  },
  {
    id: "mock-4",
    type: "APPROVAL",
    title: "New approval request",
    message: "Sarah Chen requested approval for 'Summer Promo' content",
    isRead: true,
    createdAt: "2026-06-07T21:02:00.000Z",
    actionUrl: "/approvals",
  },
  {
    id: "mock-5",
    type: "TEAM",
    title: "New team member joined",
    message: "Alex Rivera joined your workspace as Content Creator",
    isRead: true,
    createdAt: "2026-06-07T18:02:00.000Z",
    actionUrl: "/team",
  },
  {
    id: "mock-6",
    type: "BILLING",
    title: "Invoice ready",
    message: "Your January invoice is ready for download",
    isRead: true,
    createdAt: "2026-06-06T23:02:00.000Z",
    actionUrl: "/profiles?section=billing",
  },
  {
    id: "mock-7",
    type: "SYSTEM",
    title: "System maintenance scheduled",
    message: "Brief maintenance window on Jan 30, 2:00 AM - 3:00 AM UTC",
    isRead: true,
    createdAt: "2026-06-05T23:02:00.000Z",
    actionUrl: null,
  },
];

let mockNotifications = [...MOCK_NOTIFICATIONS];

let useMockData = true; // Use mock data by default for demo

export async function getNotifications(page = 1, pageSize = 20): Promise<PagedResult<NotificationListItem> | null> {
  if (!useMockData) {
    try {
      const res: GenericResponse<PagedResult<NotificationListItem>> = await apiClient(
        `/notifications?page=${page}&pageSize=${pageSize}`
      );
      if (res?.data?.data && res.data.data.length > 0) {
        res.data.data = res.data.data.map((n) => ({
          ...n,
          type: mapNotificationType(n.type),
          createdAt: typeof n.createdAt === "string" ? n.createdAt : new Date(n.createdAt).toISOString(),
          actionUrl: (n as NotificationListItem).actionUrl ?? null,
        }));
        return res.data;
      }
    } catch {
      // API error — fall through to mock
    }
  }
  // Fallback to mock data
  const start = (page - 1) * pageSize;
  const end = start + pageSize;
  const paginatedData = mockNotifications.slice(start, end).map((n) => ({
    ...n,
    type: mapNotificationType(n.type),
  }));
  
  return {
    data: paginatedData,
    totalCount: mockNotifications.length,
    page,
    pageSize,
    totalPages: Math.ceil(mockNotifications.length / pageSize),
    hasNextPage: end < mockNotifications.length,
    hasPreviousPage: page > 1,
  };
}

export async function getNotificationDetail(id: string): Promise<NotificationDetail | null> {
  if (!useMockData) {
    try {
      const res: GenericResponse<NotificationDetail> = await apiClient(`/notifications/${id}`);
      if (res?.data) {
        res.data.type = mapNotificationType(res.data.type);
        res.data.createdAt = typeof res.data.createdAt === "string" ? res.data.createdAt : new Date(res.data.createdAt).toISOString();
        res.data.actionUrl = (res.data as NotificationListItem).actionUrl ?? null;
        return res.data;
      }
    } catch {
      // API error — fall through to mock
    }
  }
  // Fallback to mock data
  const notification = mockNotifications.find((n) => n.id === id);
  if (!notification) return null;
  
  return {
    ...notification,
    type: mapNotificationType(notification.type),
    metadata: null,
  };
}

export async function markNotificationRead(id: string): Promise<boolean> {
  if (!useMockData) {
    try {
      const res: GenericResponse<boolean> = await apiClient(`/notifications/${id}/mark-read`, {
        method: "POST",
      });
      if (res?.success) return true;
    } catch {
      // API error — fall through to mock
    }
  }
  // Update mock data
  const notification = mockNotifications.find((n) => n.id === id);
  if (notification) {
    notification.isRead = true;
    return true;
  }
  return false;
}

export async function markAllNotificationsRead(): Promise<boolean> {
  if (!useMockData) {
    try {
      const res: GenericResponse<boolean> = await apiClient("/notifications/mark-all-read", {
        method: "POST",
      });
      if (res?.success) return true;
    } catch {
      // API error — fall through to mock
    }
  }
  // Update all mock data
  mockNotifications = mockNotifications.map((n) => ({ ...n, isRead: true }));
  return true;
}

export async function getUnreadCount(): Promise<number> {
  if (!useMockData) {
    try {
      const res: GenericResponse<UnreadCount> = await apiClient("/notifications/unread-count");
      if (res?.data && res.data.count >= 0) return res.data.count;
    } catch {
      // API error — fall through to mock
    }
  }
  // Count unread from mock data
  return mockNotifications.filter((n) => !n.isRead).length;
}

export async function deleteNotification(id: string): Promise<boolean> {
  if (!useMockData) {
    try {
      const res: GenericResponse<boolean> = await apiClient(`/notifications/${id}`, {
        method: "DELETE",
      });
      if (res?.success) return true;
    } catch {
      // API error — fall through to mock
    }
  }
  // Remove from mock data
  const idx = mockNotifications.findIndex((n) => n.id === id);
  if (idx !== -1) {
    mockNotifications.splice(idx, 1);
    return true;
  }
  return false;
}
