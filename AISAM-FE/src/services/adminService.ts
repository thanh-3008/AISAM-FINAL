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
    const res: GenericResponse<{ items: AdminUser[]; total: number }> = await apiClient(`/admin/users?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
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
    const res: GenericResponse<{ items: AdminWorkspace[]; total: number }> = await apiClient(`/admin/workspaces?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminPayments(page = 1, pageSize = 20): Promise<{ items: AdminPayment[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminPayment[]; total: number }> = await apiClient(`/admin/payments?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}

export async function fetchAdminContent(page = 1, pageSize = 20): Promise<{ items: AdminContent[]; total: number } | null> {
  try {
    const res: GenericResponse<{ items: AdminContent[]; total: number }> = await apiClient(`/admin/content?page=${page}&pageSize=${pageSize}`);
    return res?.data ?? null;
  } catch { return null; }
}
