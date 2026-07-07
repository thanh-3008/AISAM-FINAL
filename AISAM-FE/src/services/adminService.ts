import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

export interface AdminDashboardSummary {
  totalUsers: number;
  totalWorkspaces: number;
  totalContent: number;
  totalRevenue: number;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  role: number;
  roleName: string;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface AdminWorkspace {
  id: string;
  name: string;
  workspaceType: number;
  status: number;
  createdAt: string;
}

export interface AdminPayment {
  id: string;
  userId: string;
  amount: number;
  currency: string;
  status: number;
  paymentType: number;
  createdAt: string;
}

export interface AdminContent {
  id: string;
  title: string;
  workspaceId: string;
  status: number;
  isAiGenerated: boolean;
  createdAt: string;
}

export async function fetchAdminDashboardSummary(): Promise<AdminDashboardSummary | null> {
  try {
    const res: GenericResponse<AdminDashboardSummary> = await apiClient("/admin/dashboard/summary");
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminUsers(page = 1, pageSize = 20): Promise<{ items: AdminUser[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/users?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function fetchAdminUserDetail(id: string): Promise<AdminUser | null> {
  try {
    const res: GenericResponse<AdminUser> = await apiClient(`/admin/users/${id}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function setUserStatus(id: string, isActive: boolean): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/users/${id}/status`, { data: { isActive }, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function deleteUser(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/users/${id}`, { method: "DELETE" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchAdminWorkspaces(page = 1, pageSize = 20): Promise<{ items: AdminWorkspace[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/workspaces?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function fetchAdminPayments(page = 1, pageSize = 20): Promise<{ items: AdminPayment[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/payments?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function fetchAdminContent(page = 1, pageSize = 20): Promise<{ items: AdminContent[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/content?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export interface AdminAuditLog {
  id: string;
  actorId: string;
  actionType: string;
  targetTable: string;
  targetId: string;
  notes?: string;
  actorEmail?: string;
  hasDiff?: boolean;
  createdAt: string;
}

export async function setWorkspaceStatus(id: string, status: number): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/workspaces/${id}/status`, { data: { status }, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function deleteWorkspace(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/workspaces/${id}`, { method: "DELETE" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function setContentStatus(id: string, status: number): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/content/${id}/status`, { data: { status }, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function deleteContent(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/content/${id}`, { method: "DELETE" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchAdminAnalyticsCharts(): Promise<{ userRegistrations: any[]; revenue: any[] } | null> {
  try {
    const res: GenericResponse<{ userRegistrations: any[]; revenue: any[] }> = await apiClient("/admin/dashboard/charts");
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminAuditLogs(page = 1, pageSize = 20): Promise<{ items: AdminAuditLog[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/audit-logs?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function fetchAdminSettings(): Promise<any[] | null> {
  try {
    const res: GenericResponse<any[]> = await apiClient("/admin/settings");
    return res?.data ?? null;
  } catch { return null; }
}

export async function saveAdminSettings(settings: Record<string, string>): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient("/admin/settings", { data: settings, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export interface AdminAnalyticsOverview {
  totals: {
    impressions: number;
    clicks: number;
    ctr: number;
    spend: number;
    conversions: number;
    engagement: number;
    estimatedRevenue: number;
    publishedPosts: number;
    activeCampaigns: number;
  };
  topWorkspaces: {
    workspaceId: string;
    workspaceName: string;
    publishedPosts: number;
    activeCampaigns: number;
    impressions: number;
    clicks: number;
    spend: number;
    engagement: number;
    ctr: number;
    estimatedRevenue: number;
    roas: number;
  }[];
  topCampaigns: {
    campaignName: string;
    brandName: string;
    status: string;
    impressions: number;
    clicks: number;
    spend: number;
    conversions: number;
    ctr: number;
    cpa: number;
    roas: number;
  }[];
  systemStats: {
    totalUsers: number;
    totalWorkspaces: number;
    totalContent: number;
    totalRevenue: number;
  };
  period: { from: string; to: string };
}

export interface WorkspaceComparisonItem {
  workspaceId: string;
  workspaceName: string;
  publishedPosts: number;
  activeCampaigns: number;
  impressions: number;
  clicks: number;
  spend: number;
  engagement: number;
  ctr: number;
  estimatedRevenue: number;
  roas: number;
}

export async function fetchAdminAnalyticsOverview(from?: string, to?: string): Promise<AdminAnalyticsOverview | null> {
  try {
    const params = new URLSearchParams();
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    const qs = params.toString();
    const res: GenericResponse<AdminAnalyticsOverview> = await apiClient(`/admin/analytics/overview${qs ? "?" + qs : ""}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminWorkspaceComparison(from?: string, to?: string, top = 20): Promise<WorkspaceComparisonItem[] | null> {
  try {
    const params = new URLSearchParams();
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    params.set("top", String(top));
    const res: GenericResponse<WorkspaceComparisonItem[]> = await apiClient(`/admin/analytics/workspace-comparison?${params.toString()}`);
    return res?.data ?? null;
  } catch { return null; }
}

export function getAdminExportUrl(from?: string, to?: string): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";
  const params = new URLSearchParams();
  if (from) params.set("from", from);
  if (to) params.set("to", to);
  return `${baseUrl}/admin/analytics/export${params.toString() ? "?" + params.toString() : ""}`;
}

export interface ServiceHealthItem {
  name: string;
  status: string;
  lastHeartbeat: string;
  successCount: number;
  failureCount: number;
  lastError?: string;
  lastErrorTime?: string;
  isStale: boolean;
}

export interface AdminServiceHealth {
  services: ServiceHealthItem[];
  overallStatus: string;
}

export async function fetchServiceHealth(): Promise<AdminServiceHealth | null> {
  try {
    const res: GenericResponse<AdminServiceHealth> = await apiClient("/admin/service-health");
    return res?.data ?? null;
  } catch { return null; }
}

export interface AdminSubscription {
  id: string;
  plan: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdAt: string;
  workspaceName: string;
  workspaceId: string;
}

export async function fetchAdminSubscriptions(page = 1, pageSize = 20): Promise<{ items: AdminSubscription[]; total: number } | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/payments/subscriptions?page=${page}&pageSize=${pageSize}`);
    const paged = res?.data;
    return paged ? { items: paged.items ?? [], total: paged.total ?? 0 } : null;
  } catch { return null; }
}

export async function updateSubscription(id: string, data: { plan?: number; endDate?: string; isActive?: boolean }): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/payments/subscriptions/${id}`, { data, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}
