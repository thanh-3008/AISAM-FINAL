import { apiClient } from "@/lib/apiClient";

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

export interface QuotaSummary {
  planName: string;
  promptRemaining: number;
  promptQuotaLimit: number;
  postRemaining: number;
  postQuotaLimit: number;
}

export interface CreditUsageRecord {
  id: string;
  userId: string;
  userName: string;
  action: string;
  credits: number;
  featureUsed: string;
  status: "Success" | "Failed" | "Pending";
  createdAt: string;
}

// [REFACTOR] Centralize quota endpoints in the service layer, matching FINAL0ld's services-through-apiClient pattern.
export async function fetchCurrentWorkspaceQuota(): Promise<QuotaSummary | null> {
  try {
    const res: GenericResponse<QuotaSummary> = await apiClient("/quota/workspace/current");
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function fetchWorkspaceUsageHistory(
  page = 1,
  pageSize = 10
): Promise<PagedResult<CreditUsageRecord> | null> {
  try {
    const res: GenericResponse<PagedResult<CreditUsageRecord>> = await apiClient(
      `/quota/workspace/usage-history?page=${page}&pageSize=${pageSize}`
    );
    return res?.data ?? null;
  } catch {
    return null;
  }
}
