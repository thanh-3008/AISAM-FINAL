import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

export interface CreditWallet {
  id: string;
  workspaceId: string;
  balance: number;
  maxBalance: number;
  subscriptionEndDate?: string;
  subscriptionStatus?: string;
}

export interface WorkspaceDashboard {
  creditsRemaining: number;
  postsRemaining: number;
  totalAiUsage: number;
  topMembers: { userId: string; name: string; usage: number }[];
}

export async function fetchWorkspaces(): Promise<{ id: string; name: string; workspaceType: number }[]> {
  try {
    const res: GenericResponse<{ id: string; name: string; workspaceType: number }[]> = await apiClient("/workspaces");
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export async function fetchWorkspaceDashboard(): Promise<{
  creditsRemaining: number;
  postsRemaining: number;
  totalAiUsage: number;
  topMembers: { userId: string; name: string; usage: number }[];
} | null> {
  try {
    const res = await apiClient("/workspace-dashboard/summary");
    if (res?.data) {
      const d = res.data as {
        creditBalance?: number;
        postsRemaining?: number;
        aiUsageCount?: number;
        topMembers?: { userId: string; name: string; creditsUsed: number }[];
      };
      return {
        creditsRemaining: d.creditBalance ?? 0,
        postsRemaining: d.postsRemaining ?? 0,
        totalAiUsage: d.aiUsageCount ?? 0,
        topMembers: (d.topMembers ?? []).map(m => ({
          userId: m.userId,
          name: m.name,
          usage: m.creditsUsed,
        })),
      };
    }
    return null;
  } catch {
    return null;
  }
}

export async function fetchCreditWallet(): Promise<{
  id: string;
  workspaceId: string;
  balance: number;
  maxBalance: number;
  subscriptionEndDate?: string;
  subscriptionStatus?: string;
} | null> {
  try {
    const [subRes, dashRes] = await Promise.allSettled([
      apiClient("/payment/subscription/current"),
      apiClient("/workspace-dashboard/summary"),
    ]);

    const sub = subRes.status === "fulfilled" ? subRes.value?.data as {
      subscriptionId?: string;
      planName?: string;
      status?: string;
      startDate?: string;
      endDate?: string;
    } | undefined : undefined;

    const dash = dashRes.status === "fulfilled" ? dashRes.value?.data as {
      creditBalance?: number;
      workspaceId?: string;
    } | undefined : undefined;

    return {
      id: sub?.subscriptionId || "",
      workspaceId: dash?.workspaceId || "",
      balance: dash?.creditBalance ?? 0,
      maxBalance: 15000,
      subscriptionEndDate: sub?.endDate,
      subscriptionStatus: sub?.status,
    };
  } catch {
    return null;
  }
}

export async function fetchPostQuota(): Promise<{ used: number; total: number } | null> {
  try {
    const res = await apiClient("/quota/workspace/current");
    if (res?.data) {
      const q = res.data as { postUsage?: number; postQuotaLimit?: number };
      return { used: q.postUsage ?? 0, total: q.postQuotaLimit ?? 0 };
    }
    return null;
  } catch {
    return null;
  }
}

// Credit Usage History — BE endpoint not available
export interface CreditUsageRecord {
  id: string;
  userId: string;
  userName: string;
  action: string;
  credits: number;
  featureUsed: string;
  status: "Success" | "Failed";
  createdAt: string;
}

export interface CreditUsageHistoryResponse {
  data: CreditUsageRecord[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DeductCreditsRequest {
  feature: string;
  credits: number;
}

export async function deductCredits(_data: DeductCreditsRequest): Promise<{ balance: number } | null> {
  // No BE endpoint — returns null
  return null;
}

export async function fetchCreditUsageHistory(
  _page = 1,
  _pageSize = 10
): Promise<CreditUsageHistoryResponse | null> {
  // No BE endpoint — returns null
  return null;
}

// Workspace Members
export type WorkspaceMemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";

export interface WorkspaceMember {
  id: string;
  userId: string;
  name: string;
  email: string;
  role: WorkspaceMemberRole;
  joinedAt: string;
}

export interface WorkspaceMembersResponse {
  data: WorkspaceMember[];
  totalCount: number;
}

const BE_ROLE_MAP: Record<number, WorkspaceMemberRole> = {
  1: "Owner",
  2: "Manager",
  3: "ContentCreator",
  4: "Viewer",
};

export async function fetchWorkspaceMembers(): Promise<WorkspaceMembersResponse | null> {
  try {
    const res: GenericResponse<{
      id: string;
      userId: string;
      fullName: string;
      email: string;
      role: number;
      joinedAt: string;
    }[]> = await apiClient("/workspace-members");
    if (res?.data) {
      const members: WorkspaceMember[] = res.data.map((m) => ({
        id: m.id,
        userId: m.userId,
        name: m.fullName,
        email: m.email,
        role: BE_ROLE_MAP[m.role] ?? "Viewer",
        joinedAt: m.joinedAt,
      }));
      return { data: members, totalCount: members.length };
    }
    return null;
  } catch {
    return null;
  }
}

export async function transferOwnership(targetMemberId: string): Promise<{ success: boolean; message?: string }> {
  try {
    const res = await apiClient("/workspace-members/ownership-transfer", {
      method: "POST",
      data: { targetMemberId },
    });
    return { success: res?.success === true, message: res?.message };
  } catch (err: any) {
    return { success: false, message: err?.message || "Network error" };
  }
}
