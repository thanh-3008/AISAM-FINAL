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

export async function fetchWorkspaces(): Promise<{ id: string; name: string; workspaceType: number }[]> {
  try {
    const res: GenericResponse<{ id: string; name: string; workspaceType: number }[]> = await apiClient("/workspaces");
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export async function fetchWorkspaceDashboard(): Promise<WorkspaceDashboard | null> {
  try {
    const res = await apiClient("/workspace-dashboard/summary");
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
      apiClient("/payment/subscription/current"),
      apiClient("/workspace-dashboard/summary"),
      apiClient("/credit-usage/wallet"),
    ]);

    const sub = subRes.status === "fulfilled" ? subRes.value?.data as {
      subscriptionId?: string;
      planName?: string;
      status?: string;
      startDate?: string;
      endDate?: string;
    } | undefined : undefined;

    const dash = dashRes.status === "fulfilled" ? dashRes.value?.data as {
      workspaceId?: string;
      creditBalance?: number;
      creditsUsed?: number;
      publishedPostCount?: number;
      postQuotaLimit?: number;
      postsRemaining?: number;
      aiUsageCount?: number;
      activeMemberCount?: number;
    } | undefined : undefined;

    const wallet = walletRes.status === "fulfilled" ? walletRes.value?.data as {
      balance?: number;
      workspaceId?: string;
    } | undefined : undefined;

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

export interface QuotaSummary {
  promptUsage: number;
  promptQuotaLimit: number;
  postUsage: number;
  postQuotaLimit: number;
  textContentCount: number;
  imageContentCount: number;
  videoContentCount: number;
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

export async function fetchContentQuota(): Promise<QuotaSummary | null> {
  try {
    const res = await apiClient("/quota/workspace/current");
    if (res?.data) {
      const q = res.data as { promptUsage?: number; promptQuotaLimit?: number; postUsage?: number; postQuotaLimit?: number; textContentCount?: number; imageContentCount?: number; videoContentCount?: number };
      return {
        promptUsage: q.promptUsage ?? 0,
        promptQuotaLimit: q.promptQuotaLimit ?? 0,
        postUsage: q.postUsage ?? 0,
        postQuotaLimit: q.postQuotaLimit ?? 0,
        textContentCount: q.textContentCount ?? 0,
        imageContentCount: q.imageContentCount ?? 0,
        videoContentCount: q.videoContentCount ?? 0,
      };
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
  // Credits are now deducted server-side via ConsumeCreditsAsync
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
