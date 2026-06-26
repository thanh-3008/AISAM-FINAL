import { apiClient } from "@/lib/apiClient";

export interface AdminDashboardData {
  totalUsers: number;
  activeUsers: number;
  totalWorkspaces: number;
  activeSubscriptions: number;
  totalRevenue: number;
  recentUsers: { id: string; email: string; fullName?: string; role: string; createdAt: string }[];
  recentPayments: { id: string; amount: number; currency: string; status: string; userEmail: string; createdAt: string }[];
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

export interface AdminUserListItem {
  id: string;
  email: string;
  fullName?: string;
  role: string;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
  workspaceCount: number;
}

export interface AdminUserDetail {
  id: string;
  email: string;
  fullName?: string;
  role: string;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
  profiles: { id: string; name: string; companyName?: string; status: string; createdAt: string }[];
  workspaces: { id: string; name: string; type: string; status: string; role: string; createdAt: string }[];
  payments: { id: string; amount: number; currency: string; status: string; createdAt: string }[];
}

export async function fetchAdminDashboard(): Promise<AdminDashboardData> {
  const res = await apiClient("/admin/dashboard");
  return res.data;
}

export async function fetchAdminUsers(params: {
  page?: number; pageSize?: number; searchTerm?: string; sortBy?: string; sortDescending?: boolean; role?: string;
}): Promise<PagedResult<AdminUserListItem>> {
  const query = new URLSearchParams();
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));
  if (params.searchTerm) query.set("searchTerm", params.searchTerm);
  if (params.sortBy) query.set("sortBy", params.sortBy);
  if (params.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));
  if (params.role) query.set("role", params.role);
  const res = await apiClient(`/admin/users?${query.toString()}`);
  return res.data;
}

export async function fetchAdminUserDetail(userId: string): Promise<AdminUserDetail> {
  const res = await apiClient(`/admin/users/${userId}`);
  return res.data;
}
