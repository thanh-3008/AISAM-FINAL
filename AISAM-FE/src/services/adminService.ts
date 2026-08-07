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
  isActive: boolean;
  createdAt: string;
}

export interface AdminWorkspace {
  id: string;
  name: string;
  workspaceType: number;
  status: number;
  aiCreditBalance?: number;
  aiCreditReserved?: number;
  createdAt: string;
}

export interface AdminPayment {
  id: string;
  userId: string;
  userEmail?: string;
  amount: number;
  currency: string;
  status: number;
  paymentType: number;
  transactionId?: string;
  refundedAt?: string;
  refundReason?: string;
  createdAt: string;
}

export interface AdminContent {
  id: string;
  title: string | null;
  textContent: string;
  status: number;
  adType: number;
  isAiGenerated: boolean;
  platformRejectionReason?: string;
  rejectedPlatform?: string;
  workspaceId?: string;
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

export async function fetchAdminUserDetail(id: string): Promise<any | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/users/${id}`);
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

export async function impersonateUser(id: string): Promise<any | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/users/${id}/impersonate`, { method: "POST" });
    return res?.data ?? null;
  } catch { return null; }
}

export async function setUserRole(id: string, role: number): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/users/${id}/role`, { data: { role }, method: "PATCH" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchAdminWorkspaces(page = 1, pageSize = 20, search?: string, type?: number): Promise<{ items: AdminWorkspace[]; total: number } | null> {
  try {
    let url = `/admin/workspaces?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (type !== undefined && type !== -1) url += `&type=${type}`;
    const res: GenericResponse<any> = await apiClient(url);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function fetchAdminPayments(page = 1, pageSize = 20, status?: number): Promise<{ items: AdminPayment[]; total: number } | null> {
  try {
    let url = `/admin/payments?page=${page}&pageSize=${pageSize}`;
    if (status !== undefined) url += `&status=${status}`;
    const res: GenericResponse<any> = await apiClient(url);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function refundAdminPayment(id: string, reason: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/admin/payments/${id}/refund`, {
      method: "PATCH",
      data: { reason }
    });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchAdminContent(page = 1, pageSize = 20, search?: string, status?: number): Promise<{ items: AdminContent[]; total: number } | null> {
  try {
    let url = `/admin/content?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    if (status !== undefined) url += `&status=${status}`;
    const res: GenericResponse<any> = await apiClient(url);
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
  actorName?: string;
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

export async function fetchAdminAiCreditBreakdown(): Promise<{ workspaceId: string; workspaceName: string; totalGenerations: number }[] | null> {
  try {
    const res: GenericResponse<any> = await apiClient("/admin/dashboard/ai-credit-breakdown");
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminAnalyticsCharts(): Promise<{ userRegistrations: any[]; revenue: any[] } | null> {
  try {
    const res: GenericResponse<{ userRegistrations: any[]; revenue: any[] }> = await apiClient("/admin/dashboard/charts");
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminActiveUsers(): Promise<{ dau: number; mau: number; date: string; month: string } | null> {
  try {
    const res: GenericResponse<any> = await apiClient("/admin/dashboard/active-users");
    return res?.data ?? null;
  } catch { return null; }
}

export interface AdminTopWorkspace {
  workspaceId: string;
  workspaceName: string;
  saaSRevenue: number;
  adSpend: number;
  adRevenue: number;
  roas: number;
  engagement: number;
}

export async function fetchAdminTopWorkspaces(limit = 10, period = "month"): Promise<AdminTopWorkspace[] | null> {
  try {
    const res: GenericResponse<AdminTopWorkspace[]> = await apiClient(`/admin/dashboard/top-workspaces?limit=${limit}&period=${period}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function fetchAdminAuditLogs(
  page = 1, pageSize = 20, 
  actionType?: string, targetTable?: string, 
  searchTerm?: string, fromDate?: string, toDate?: string,
  actorId?: string
): Promise<{ items: AdminAuditLog[]; total: number } | null> {
  try {
    let url = `/admin/audit-logs?page=${page}&pageSize=${pageSize}`;
    if (actionType) url += `&actionType=${encodeURIComponent(actionType)}`;
    if (targetTable) url += `&targetTable=${encodeURIComponent(targetTable)}`;
    if (searchTerm) url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
    if (fromDate) url += `&fromDate=${encodeURIComponent(fromDate)}`;
    if (toDate) url += `&toDate=${encodeURIComponent(toDate)}`;
    if (actorId) url += `&actorId=${encodeURIComponent(actorId)}`;
    
    const res: GenericResponse<any> = await apiClient(url);
    const paged = res?.data;
    return paged ? { items: paged.data ?? [], total: paged.totalCount ?? 0 } : null;
  } catch { return null; }
}

export async function exportAdminAuditLogsCsv(
  actionType?: string, targetTable?: string, 
  searchTerm?: string, fromDate?: string, toDate?: string
): Promise<Blob | null> {
  try {
    let url = `/admin/audit-logs/export?`;
    const params = new URLSearchParams();
    if (actionType) params.append("actionType", actionType);
    if (targetTable) params.append("targetTable", targetTable);
    if (searchTerm) params.append("searchTerm", searchTerm);
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);
    
    url += params.toString();

    // Since apiClient returns JSON by default, we need to handle Blob response manually
    // Assuming apiClient can't handle Blob directly, let's fetch it using the same auth
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    if (!token) return null;
    
    const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5247/api";
    const response = await fetch(`${API_URL}${url}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) return null;
    return await response.blob();
  } catch {
    return null;
  }
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

export async function fetchAdminTopCampaigns(from?: string, to?: string, top = 20): Promise<AdminAnalyticsOverview["topCampaigns"] | null> {
  try {
    const params = new URLSearchParams();
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    params.set("top", String(top));
    const res: GenericResponse<AdminAnalyticsOverview["topCampaigns"]> = await apiClient(`/admin/analytics/top-campaigns?${params.toString()}`);
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

export interface AiCreditSummary {
  totalAiGenerations: number;
  weeklyAiGenerations: number;
  dailyAiData: { name: string; generations: number }[];
  estimatedCreditSpent: number;
  estimatedRevenue: number;
}

export interface SystemHealthCheck {
  name: string;
  status: string;
  detail: string;
}

export interface SystemHealth {
  overallStatus: string;
  checks: SystemHealthCheck[];
  checkedAt: string;
}

export async function fetchAiCreditSummary(): Promise<AiCreditSummary | null> {
  try {
    const res: GenericResponse<AiCreditSummary> = await apiClient("/admin/credit-oversight/summary");
    return res?.data ?? null;
  } catch { return null; }
}

export async function adjustAdminCredits(workspaceId: string, amount: number, reason: string): Promise<boolean> {
  try {
    const res: GenericResponse<any> = await apiClient("/admin/credit-oversight/adjust", {
      data: { workspaceId, amount, reason },
      method: "POST"
    });
    return res?.success ?? false;
  } catch { return false; }
}

export async function fetchSystemHealth(): Promise<SystemHealth | null> {
  try {
    const res: GenericResponse<SystemHealth> = await apiClient("/admin/system-health");
    return res?.data ?? null;
  } catch { return null; }
}

export async function broadcastNotification(title: string, message: string, excludeAdmins: boolean): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient("/admin/notifications/broadcast", { data: { title, message, excludeAdmins }, method: "POST" });
    return res?.success ?? false;
  } catch { return false; }
}

export async function seedDemoUsers(count = 5): Promise<any | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/tools/seed-demo-users?count=${count}`, { method: "POST" });
    return res?.data ?? null;
  } catch { return null; }
}

export async function seedDemoContent(count = 10): Promise<any | null> {
  try {
    const res: GenericResponse<any> = await apiClient(`/admin/tools/seed-demo-content?count=${count}`, { method: "POST" });
    return res?.data ?? null;
  } catch { return null; }
}

export interface SubscriptionPlanDto {
  id: string;
  name: string;
  price: number;
  credits: number;
  postsPerMonth: number;
  members: number;
  features: string[];
  isActive: boolean;
}

export async function fetchAdminPlans(): Promise<SubscriptionPlanDto[] | null> {
  try {
    const res: GenericResponse<{ plans: SubscriptionPlanDto[] }> = await apiClient("/admin/plans");
    return res?.data?.plans ?? null;
  } catch { return null; }
}

export async function saveAdminPlans(plans: SubscriptionPlanDto[]): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient("/admin/plans", { data: { plans }, method: "PUT" });
    return res?.success ?? false;
  } catch { return false; }
}

export interface CreditPackDto {
  id: string;
  name: string;
  price: number;
  credits: number;
  isActive: boolean;
}

export async function fetchAdminCreditPacks(): Promise<CreditPackDto[] | null> {
  try {
    const res: GenericResponse<{ creditPacks: CreditPackDto[] }> = await apiClient("/admin/plans/credit-packs");
    return res?.data?.creditPacks ?? null;
  } catch { return null; }
}

export async function saveAdminCreditPacks(creditPacks: CreditPackDto[]): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient("/admin/plans/credit-packs", { data: { creditPacks }, method: "PUT" });
    return res?.success ?? false;
  } catch { return false; }
}
