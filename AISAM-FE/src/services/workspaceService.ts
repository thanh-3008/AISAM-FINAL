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
  creditsUsed: number;
  publishedPostCount: number;
  postQuotaLimit: number;
  postsRemaining: number;
  aiUsageCount: number;
  activeMemberCount: number;
  subscriptionEndDate?: string;
  subscriptionStatus?: string;
}

export interface WorkspaceDashboard {
  creditBalance: number;
  creditsUsed: number;
  publishedPostCount: number;
  postQuotaLimit: number;
  postsRemaining: number;
  aiUsageCount: number;
  activeMemberCount: number;
  topMembers: { userId: string; name: string; usage: number }[];
}

export interface WorkspaceApiItem {
  id: string;
  userId?: string;
  name: string;
  workspaceType: number;
  companyName?: string | null;
  bio?: string | null;
  avatarUrl?: string | null;
  status?: number;
  currentUserRole?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface WorkspacePayload {
  name: string;
  workspaceType?: number;
  companyName?: string | null;
  bio?: string | null;
  avatarUrl?: string | null;
}

export async function fetchWorkspaces(): Promise<WorkspaceApiItem[]> {
  try {
    const res: GenericResponse<WorkspaceApiItem[]> = await apiClient("/workspaces");
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export async function createWorkspace(data: WorkspacePayload): Promise<WorkspaceApiItem | null> {
  try {
    const res: GenericResponse<WorkspaceApiItem> = await apiClient("/workspaces", {
      method: "POST",
      data,
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function getWorkspaceById(id: string): Promise<WorkspaceApiItem | null> {
  try {
    const res: GenericResponse<WorkspaceApiItem> = await apiClient(`/workspaces/${id}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function updateWorkspace(id: string, data: Partial<WorkspacePayload>): Promise<WorkspaceApiItem | null> {
  try {
    const res: GenericResponse<WorkspaceApiItem> = await apiClient(`/workspaces/${id}`, {
      method: "PUT",
      data,
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function deleteWorkspace(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient(`/workspaces/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function fetchWorkspaceDashboard(): Promise<WorkspaceDashboard | null> {
  try {
    const res: GenericResponse<{
      creditBalance?: number;
      postsRemaining?: number;
      aiUsageCount?: number;
      topMembers?: { userId: string; name: string; creditsUsed: number }[];
    }> = await apiClient("/workspace-dashboard/summary");
    if (res?.data) {
      const d = res.data as {
        creditBalance?: number;
        creditsUsed?: number;
        publishedPostCount?: number;
        postQuotaLimit?: number;
        postsRemaining?: number;
        aiUsageCount?: number;
        activeMemberCount?: number;
        topMembers?: { userId: string; name: string; creditsUsed: number }[];
      };
      return {
        creditBalance: d.creditBalance ?? 0,
        creditsUsed: d.creditsUsed ?? 0,
        publishedPostCount: d.publishedPostCount ?? 0,
        postQuotaLimit: d.postQuotaLimit ?? 0,
        postsRemaining: d.postsRemaining ?? 0,
        aiUsageCount: d.aiUsageCount ?? 0,
        activeMemberCount: d.activeMemberCount ?? 0,
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
  creditsUsed: number;
  publishedPostCount: number;
  postQuotaLimit: number;
  postsRemaining: number;
  aiUsageCount: number;
  activeMemberCount: number;
  subscriptionEndDate?: string;
  subscriptionStatus?: string;
} | null> {
  try {
    const [subRes, dashRes, walletRes] = await Promise.allSettled([
      apiClient<GenericResponse<{
        subscriptionId?: string;
        planName?: string;
        status?: string;
        startDate?: string;
        endDate?: string;
      }>>("/payment/subscription/current"),
      apiClient<GenericResponse<{
        creditBalance?: number;
        workspaceId?: string;
        creditsUsed?: number;
        publishedPostCount?: number;
        postQuotaLimit?: number;
        postsRemaining?: number;
        aiUsageCount?: number;
        activeMemberCount?: number;
      }>>("/workspace-dashboard/summary"),
      apiClient<GenericResponse<{
        balance?: number;
        workspaceId?: string;
      }>>("/credit-usage/wallet"),
    ]);

    const sub = subRes.status === "fulfilled" ? subRes.value?.data : undefined;

    const dash = dashRes.status === "fulfilled" ? dashRes.value?.data : undefined;
    const wallet = walletRes.status === "fulfilled" ? walletRes.value?.data : undefined;

    const balance = dash?.creditBalance ?? wallet?.balance ?? 0;

    return {
      id: sub?.subscriptionId || "",
      workspaceId: dash?.workspaceId || wallet?.workspaceId || "",
      balance,
      maxBalance: (dash?.creditBalance ?? wallet?.balance ?? 0) + (dash?.creditsUsed ?? 0),
      creditsUsed: dash?.creditsUsed ?? 0,
      publishedPostCount: dash?.publishedPostCount ?? 0,
      postQuotaLimit: dash?.postQuotaLimit ?? 0,
      postsRemaining: dash?.postsRemaining ?? 0,
      aiUsageCount: dash?.aiUsageCount ?? 0,
      activeMemberCount: dash?.activeMemberCount ?? 0,
      subscriptionEndDate: sub?.endDate,
      subscriptionStatus: sub?.status,
    };
  } catch (err) {
    console.error("[fetchCreditWallet] error:", err);
    return null;
  }
}

export async function fetchPostQuota(): Promise<{ used: number; total: number } | null> {
  try {
    const res: GenericResponse<{ postUsage?: number; postQuotaLimit?: number }> = await apiClient("/quota/workspace/current");
    if (res?.data) {
      const q = res.data;
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

/** @deprecated BE endpoint not available. Returns null. */
export async function deductCredits(_data: DeductCreditsRequest): Promise<{ balance: number } | null> {
  void _data;
  console.warn("[DEPRECATED] deductCredits: BE endpoint not available. Credits are now deducted server-side.");
  return null;
}

export async function fetchCreditUsageHistory(
  page = 1,
  pageSize = 10
): Promise<CreditUsageHistoryResponse | null> {
  try {
    const res: GenericResponse<CreditUsageHistoryResponse & { data: CreditUsageRecord[] }> = await apiClient(`/credit-usage?page=${page}&pageSize=${pageSize}`);
    if (res?.success && res.data) {
      return {
        data: res.data.data || [],
        totalCount: res.data.totalCount || 0,
        page: res.data.page || page,
        pageSize: res.data.pageSize || pageSize,
        totalPages: res.data.totalPages || 0,
      };
    }
    return null;
  } catch {
    return null;
  }
}

// Workspace Members
export type WorkspaceMemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type WorkspaceMemberQuotaMode = "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";

export interface WorkspaceMember {
  id: string;
  userId: string;
  name: string;
  email: string;
  role: WorkspaceMemberRole;
  quotaMode?: WorkspaceMemberQuotaMode;
  creditLimit?: number | null;
  creditUsed?: number;
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

const BE_QUOTA_MODE_MAP: Record<number, WorkspaceMemberQuotaMode> = {
  1: "SharedPool",
  2: "LifetimeAssigned",
  3: "MonthlyAssigned",
};

const QUOTA_MODE_TO_BE: Record<WorkspaceMemberQuotaMode, number> = {
  SharedPool: 1,
  LifetimeAssigned: 2,
  MonthlyAssigned: 3,
};

export async function fetchWorkspaceMembers(): Promise<WorkspaceMembersResponse | null> {
  try {
    const res: GenericResponse<{
      id: string;
      userId: string;
      fullName: string;
      email: string;
      role: number;
      quotaMode?: number;
      creditLimit?: number | null;
      creditUsed?: number;
      joinedAt: string;
    }[]> = await apiClient("/workspace-members");
    if (res?.data) {
      const members: WorkspaceMember[] = res.data.map((m) => ({
        id: m.id,
        userId: m.userId,
        name: m.fullName,
        email: m.email,
        role: BE_ROLE_MAP[m.role] ?? "Viewer",
        quotaMode: m.quotaMode ? BE_QUOTA_MODE_MAP[m.quotaMode] : undefined,
        creditLimit: m.creditLimit ?? null,
        creditUsed: m.creditUsed ?? 0,
        joinedAt: m.joinedAt,
      }));
      return { data: members, totalCount: members.length };
    }
    return null;
  } catch {
    return null;
  }
}

export async function updateWorkspaceMemberQuota(
  memberId: string,
  data: { quotaMode: WorkspaceMemberQuotaMode; creditLimit?: number | null }
): Promise<WorkspaceMember | null> {
  try {
    const res: GenericResponse<{
      id: string;
      userId: string;
      fullName: string;
      email: string;
      role: number;
      quotaMode?: number;
      creditLimit?: number | null;
      creditUsed?: number;
      joinedAt: string;
    }> = await apiClient(`/workspace-members/${memberId}/quota`, {
      method: "PUT",
      data: {
        quotaMode: QUOTA_MODE_TO_BE[data.quotaMode],
        creditLimit: data.creditLimit ?? null,
      },
    });
    if (!res?.data) return null;
    return {
      id: res.data.id,
      userId: res.data.userId,
      name: res.data.fullName,
      email: res.data.email,
      role: BE_ROLE_MAP[res.data.role] ?? "Viewer",
      quotaMode: res.data.quotaMode ? BE_QUOTA_MODE_MAP[res.data.quotaMode] : data.quotaMode,
      creditLimit: res.data.creditLimit ?? data.creditLimit ?? null,
      creditUsed: res.data.creditUsed ?? 0,
      joinedAt: res.data.joinedAt,
    };
  } catch {
    return null;
  }
}

export async function transferWorkspaceOwnership(targetMemberId: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient("/workspace-members/ownership-transfer", {
      method: "POST",
      data: { targetMemberId },
    });
    return res?.success === true;
  } catch {
    return false;
  }
}
