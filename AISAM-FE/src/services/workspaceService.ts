import { apiClient } from "@/lib/apiClient";
import type { ApiResponse } from "@/lib/apiTypes";
import type { CreditWallet, WorkspaceDashboard } from "@/lib/mockWorkspace";

type GenericResponse<T> = ApiResponse<T>;

export type { CreditWallet, WorkspaceDashboard };

export interface Workspace {
  id: string;
  name: string;
  workspaceType: number;
  plan: string;
  status: string;
  creditBalance: number;
  memberCount: number;
}

export interface WorkspaceCreateRequest {
  name: string;
  workspaceType: number;
}

export interface WorkspaceCreateResponse {
  id: string;
  name: string;
  workspaceType: number;
  status: number;
  currentUserRole: number;
  createdAt: string;
  updatedAt: string;
}

export async function fetchWorkspaces(): Promise<Workspace[]> {
  const res: GenericResponse<Workspace[]> = await apiClient("/workspaces");
  return res?.data ?? [];
}

export async function createWorkspace(data: WorkspaceCreateRequest): Promise<WorkspaceCreateResponse> {
  const res: GenericResponse<WorkspaceCreateResponse> = await apiClient("/workspaces", {
    method: "POST",
    data: {
      name: data.name.trim(),
      workspaceType: data.workspaceType,
    },
  });

  if (!res.success || !res.data) {
    throw new Error(res.message || "Failed to create workspace.");
  }

  return res.data;
}

export async function fetchWorkspaceDashboard(): Promise<WorkspaceDashboard | null> {
  try {
    const res = await apiClient<GenericResponse<{
      workspaceId: string;
      creditBalance: number;
      creditsUsed: number;
      postsRemaining: number;
      aiUsageCount: number;
      topMembers: Array<{ userId: string; name: string; creditsUsed: number; aiUsageCount: number }>;
    }>>("/workspace-dashboard/summary");
    if (res?.data) {
      const d = res.data;
      return {
        creditsRemaining: d.creditBalance,
        postsRemaining: d.postsRemaining,
        totalAiUsage: d.aiUsageCount,
        topMembers: d.topMembers.map((member) => ({
          userId: member.userId,
          name: member.name,
          usage: member.creditsUsed,
        })),
      };
    }
    return null;
  } catch {
    return null;
  }
}

export async function fetchCreditWallet(): Promise<CreditWallet | null> {
  try {
    const [dashboard, subscription] = await Promise.all([
      apiClient<GenericResponse<{ workspaceId: string; creditBalance: number; creditsUsed: number }>>("/workspace-dashboard/summary"),
      apiClient<GenericResponse<{ status: string; endDate?: string | null }>>("/payment/subscription/current").catch(() => null),
    ]);
    if (dashboard?.data) {
      return {
        id: dashboard.data.workspaceId,
        workspaceId: dashboard.data.workspaceId,
        balance: dashboard.data.creditBalance,
        maxBalance: dashboard.data.creditBalance + dashboard.data.creditsUsed,
        subscriptionEndDate: subscription?.data?.endDate ?? undefined,
        subscriptionStatus: subscription?.data?.status,
      };
    }
    return null;
  } catch {
    return null;
  }
}

export async function fetchPostQuota(_profileId?: string): Promise<{ used: number; total: number } | null> {
  void _profileId;
  try {
    const res = await apiClient<GenericResponse<{
      postUsage?: number;
      postQuotaLimit?: number;
      postRemaining?: number;
    }>>("/quota/workspace/current");
    if (res?.data) {
      const q = res.data;
      return {
        used: q.postUsage ?? 0,
        total: q.postQuotaLimit ?? 0,
      };
    }
    return null;
  } catch {
    return null;
  }
}

// Credit Usage History
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

const MOCK_CREDIT_USAGE: CreditUsageRecord[] = [
  {
    id: "cu-1",
    userId: "user-1",
    userName: "You",
    action: "Generate Text",
    credits: 1,
    featureUsed: "AI Content Generator",
    status: "Success",
    createdAt: "2026-06-11T10:30:00.000Z",
  },
  {
    id: "cu-2",
    userId: "user-1",
    userName: "You",
    action: "Generate Image",
    credits: 5,
    featureUsed: "AI Image Generator",
    status: "Success",
    createdAt: "2026-06-11T09:15:00.000Z",
  },
  {
    id: "cu-3",
    userId: "user-1",
    userName: "You",
    action: "Generate Video",
    credits: 20,
    featureUsed: "AI Video Generator",
    status: "Success",
    createdAt: "2026-06-10T16:45:00.000Z",
  },
  {
    id: "cu-4",
    userId: "user-1",
    userName: "You",
    action: "Generate Text",
    credits: 1,
    featureUsed: "AI Content Generator",
    status: "Failed",
    createdAt: "2026-06-10T14:20:00.000Z",
  },
  {
    id: "cu-5",
    userId: "user-1",
    userName: "You",
    action: "Regenerate",
    credits: 1,
    featureUsed: "AI Content Refine",
    status: "Success",
    createdAt: "2026-06-10T11:00:00.000Z",
  },
  {
    id: "cu-6",
    userId: "user-1",
    userName: "You",
    action: "Generate Text",
    credits: 1,
    featureUsed: "AI Content Generator",
    status: "Success",
    createdAt: "2026-06-09T15:30:00.000Z",
  },
  {
    id: "cu-7",
    userId: "user-1",
    userName: "You",
    action: "Trend Analysis",
    credits: 2,
    featureUsed: "Trend Content",
    status: "Success",
    createdAt: "2026-06-09T10:00:00.000Z",
  },
  {
    id: "cu-8",
    userId: "user-1",
    userName: "You",
    action: "Campaign Recommendation",
    credits: 2,
    featureUsed: "AI Campaign",
    status: "Success",
    createdAt: "2026-06-08T14:00:00.000Z",
  },
];

export interface DeductCreditsRequest {
  feature: string;
  credits: number;
}

export async function deductCredits(data: DeductCreditsRequest): Promise<{ balance: number } | null> {
  void data;
  // BE charges credits inside AI endpoints and has no standalone deduct endpoint.
  return null;
}

export async function fetchCreditUsageHistory(
  page = 1,
  pageSize = 10
): Promise<CreditUsageHistoryResponse | null> {
  // BE does not expose credit usage history yet; keep controlled mock data.
  const start = (page - 1) * pageSize;
  const end = start + pageSize;
  const paginatedData = MOCK_CREDIT_USAGE.slice(start, end);

  return {
    data: paginatedData,
    totalCount: MOCK_CREDIT_USAGE.length,
    page,
    pageSize,
    totalPages: Math.ceil(MOCK_CREDIT_USAGE.length / pageSize),
  };
}

// Workspace Members
export type WorkspaceMemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type MemberStatus = "Active" | "Pending" | "Invited";

export interface WorkspaceMember {
  id: string;
  userId: string;
  name: string;
  email: string;
  role: WorkspaceMemberRole;
  status: MemberStatus;
  avatarUrl?: string;
  joinedAt: string;
  lastActiveAt?: string;
}

export interface WorkspaceMembersResponse {
  data: WorkspaceMember[];
  totalCount: number;
}

export async function fetchWorkspaceMembers(): Promise<WorkspaceMembersResponse | null> {
  const roles: Record<number, WorkspaceMemberRole> = { 1: "Owner", 2: "Manager", 3: "ContentCreator", 4: "Viewer" };
  const res: GenericResponse<Array<{
    id: string;
    userId: string;
    email: string;
    fullName?: string | null;
    role: number;
    joinedAt: string;
  }>> = await apiClient("/workspace-members");
  const members = (res.data ?? []).map((member) => ({
    id: member.id,
    userId: member.userId,
    name: member.fullName || member.email,
    email: member.email,
    role: roles[member.role] ?? "Viewer",
    status: "Active" as const,
    joinedAt: member.joinedAt,
  }));
  return { data: members, totalCount: members.length };
}

const memberRoles: Record<WorkspaceMemberRole, number> = { Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 };
const quotaModes = { SharedPool: 1, LifetimeAssigned: 2, MonthlyAssigned: 3 } as const;

export async function updateWorkspaceMemberRole(memberId: string, role: WorkspaceMemberRole): Promise<boolean> {
  const res = await apiClient<GenericResponse<unknown>>(`/workspace-members/${memberId}/role`, {
    method: "PUT",
    data: { role: memberRoles[role] },
  });
  return res.success;
}

export async function removeWorkspaceMember(memberId: string): Promise<boolean> {
  const res = await apiClient<GenericResponse<boolean>>(`/workspace-members/${memberId}`, { method: "DELETE" });
  return res.success;
}

export async function transferWorkspaceOwnership(targetMemberId: string): Promise<boolean> {
  const res = await apiClient<GenericResponse<unknown>>("/workspace-members/ownership-transfer", {
    method: "POST",
    data: { targetMemberId },
  });
  return res.success;
}

export async function updateWorkspaceMemberQuota(
  memberId: string,
  quotaMode: keyof typeof quotaModes,
  creditLimit?: number
): Promise<boolean> {
  const res = await apiClient<GenericResponse<unknown>>(`/workspace-members/${memberId}/quota`, {
    method: "PUT",
    data: { quotaMode: quotaModes[quotaMode], creditLimit: quotaMode === "SharedPool" ? null : creditLimit },
  });
  return res.success;
}
