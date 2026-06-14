import { apiClient } from "@/lib/apiClient";
import { getMockCreditWallet, getMockWorkspaceDashboard, getMockPostQuota, type CreditWallet, type WorkspaceDashboard } from "@/lib/mockWorkspace";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

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

const MOCK_WS: Workspace = {
  id: "ws-1",
  name: "My Workspace",
  workspaceType: 1,
  plan: "Personal Pro",
  status: "Active",
  creditBalance: 850,
  memberCount: 1,
};

export async function fetchWorkspaces(): Promise<Workspace[]> {
  try {
    const res: GenericResponse<Workspace[]> = await apiClient("/workspaces");
    return res?.data ?? [MOCK_WS];
  } catch {
    return [MOCK_WS];
  }
}

export async function fetchWorkspaceDashboard(): Promise<WorkspaceDashboard | null> {
  try {
    const res = await apiClient("/dashboard/summary");
    if (res?.data) {
      const d = res.data as {
        draftContentCount?: number;
        publishedContentCount?: number;
        pendingApprovalContentCount?: number;
        upcomingScheduleCount?: number;
        failedScheduleCount?: number;
        activeSocialIntegrationCount?: number;
        publishedPostCount?: number;
        unreadNotificationCount?: number;
      };
      return {
        creditsRemaining: 0,
        postsRemaining: d.publishedPostCount ?? 0,
        totalAiUsage: d.draftContentCount ?? 0,
        topMembers: [],
      };
    }
    return getMockWorkspaceDashboard();
  } catch {
    return getMockWorkspaceDashboard();
  }
}

export async function fetchCreditWallet(): Promise<CreditWallet | null> {
  try {
    const res = await apiClient("/payment/subscription/current");
    if (res?.data) {
      const sub = res.data as {
        planName?: string;
        status?: string;
        startDate?: string;
        endDate?: string;
      };
      return {
        id: "wallet-1",
        workspaceId: "ws-1",
        balance: sub.status === "Active" ? 850 : 0,
        maxBalance: 15000,
        subscriptionEndDate: sub.endDate,
        subscriptionStatus: sub.status,
      };
    }
    return getMockCreditWallet();
  } catch {
    return getMockCreditWallet();
  }
}

export async function fetchPostQuota(profileId?: string): Promise<{ used: number; total: number } | null> {
  try {
    const res = await apiClient(`/quota/profile/${profileId || "00000000-0000-0000-0000-000000000000"}`);
    if (res?.data) {
      const q = res.data as {
        postUsage?: number;
        postQuotaLimit?: number;
        postRemaining?: number;
      };
      return {
        used: q.postUsage ?? 0,
        total: q.postQuotaLimit ?? 0,
      };
    }
    return getMockPostQuota();
  } catch {
    return getMockPostQuota();
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
  try {
    const res: GenericResponse<{ balance: number }> = await apiClient("/credits/deduct", {
      data,
      method: "POST",
    });
    return res?.data ?? null;
  } catch {
    // Mock: simulate deduction
    const wallet = getMockCreditWallet();
    const newBalance = Math.max(0, wallet.balance - data.credits);
    return { balance: newBalance };
  }
}

export async function fetchCreditUsageHistory(
  page = 1,
  pageSize = 10
): Promise<CreditUsageHistoryResponse | null> {
  try {
    const res: GenericResponse<CreditUsageHistoryResponse> = await apiClient(
      `/credits/history?page=${page}&pageSize=${pageSize}`
    );
    return res?.data ?? null;
  } catch {
    // Fallback to mock data
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

const MOCK_WORKSPACE_MEMBERS: WorkspaceMember[] = [
  {
    id: "member-1",
    userId: "user-1",
    name: "Nguyen Van A",
    email: "nguyenvana@example.com",
    role: "Owner",
    status: "Active",
    joinedAt: "2026-01-15T10:00:00.000Z",
    lastActiveAt: "2026-06-11T08:30:00.000Z",
  },
  {
    id: "member-2",
    userId: "user-2",
    name: "Tran Thi B",
    email: "tranthib@example.com",
    role: "Manager",
    status: "Active",
    joinedAt: "2026-02-20T14:00:00.000Z",
    lastActiveAt: "2026-06-10T16:45:00.000Z",
  },
  {
    id: "member-3",
    userId: "user-3",
    name: "Le Van C",
    email: "levanc@example.com",
    role: "ContentCreator",
    status: "Active",
    joinedAt: "2026-03-10T09:00:00.000Z",
    lastActiveAt: "2026-06-11T07:15:00.000Z",
  },
  {
    id: "member-4",
    userId: "user-4",
    name: "Pham Thi D",
    email: "phamthid@example.com",
    role: "Viewer",
    status: "Active",
    joinedAt: "2026-04-05T11:00:00.000Z",
    lastActiveAt: "2026-06-09T14:20:00.000Z",
  },
  {
    id: "member-5",
    userId: "user-5",
    name: "Hoang Van E",
    email: "hoangvane@example.com",
    role: "ContentCreator",
    status: "Pending",
    joinedAt: "2026-06-10T15:00:00.000Z",
  },
];

export async function fetchWorkspaceMembers(): Promise<WorkspaceMembersResponse | null> {
  try {
    const res: GenericResponse<WorkspaceMembersResponse> = await apiClient("/workspaces/members");
    return res?.data ?? null;
  } catch {
    // Fallback to mock data
    return {
      data: MOCK_WORKSPACE_MEMBERS,
      totalCount: MOCK_WORKSPACE_MEMBERS.length,
    };
  }
}
