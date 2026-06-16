import { apiClient } from "@/lib/apiClient";
import type { ApiResponse } from "@/lib/apiTypes";

export type WorkspaceMemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type WorkspaceQuotaMode = "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";

const roleToApi: Record<WorkspaceMemberRole, number> = {
  Owner: 1,
  Manager: 2,
  ContentCreator: 3,
  Viewer: 4,
};

const roleFromApi: Record<number, WorkspaceMemberRole> = {
  1: "Owner",
  2: "Manager",
  3: "ContentCreator",
  4: "Viewer",
};

const quotaToApi: Record<WorkspaceQuotaMode, number> = {
  SharedPool: 1,
  LifetimeAssigned: 2,
  MonthlyAssigned: 3,
};

const quotaFromApi: Record<number, WorkspaceQuotaMode> = {
  1: "SharedPool",
  2: "LifetimeAssigned",
  3: "MonthlyAssigned",
};

export interface InviteMemberRequest {
  email: string;
  role: WorkspaceMemberRole;
  quotaMode?: WorkspaceQuotaMode;
  creditLimit?: number;
}

export interface WorkspaceInvitation {
  id: string;
  workspaceId: string;
  workspaceName: string;
  email: string;
  role: WorkspaceMemberRole;
  status: "Pending" | "Accepted" | "Expired" | "Cancelled";
  invitedBy: string;
  invitedByName: string;
  createdAt: string;
  expiresAt: string;
}

export interface InvitationDetail extends WorkspaceInvitation {
  workspaceType: number;
  invitedByEmail: string;
  quotaMode?: WorkspaceQuotaMode;
  creditLimit?: number;
}

interface InvitationApiResponse {
  id: string;
  workspaceId: string;
  workspaceName: string;
  email: string;
  role: number;
  quotaMode: number;
  creditLimit?: number | null;
  invitedByUserId: string;
  expiresAt: string;
  createdAt: string;
}

function mapInvitation(data: InvitationApiResponse): WorkspaceInvitation {
  return {
    id: data.id,
    workspaceId: data.workspaceId,
    workspaceName: data.workspaceName,
    email: data.email,
    role: roleFromApi[data.role] ?? "Viewer",
    status: "Pending",
    invitedBy: data.invitedByUserId,
    invitedByName: "",
    createdAt: data.createdAt,
    expiresAt: data.expiresAt,
  };
}

export async function inviteMember(data: InviteMemberRequest): Promise<WorkspaceInvitation | null> {
  const res = await apiClient<ApiResponse<InvitationApiResponse>>("/workspace-invitations", {
    method: "POST",
    data: {
      email: data.email,
      role: roleToApi[data.role],
      quotaMode: quotaToApi[data.quotaMode ?? "SharedPool"],
      creditLimit: data.creditLimit,
    },
  });
  return res.data ? mapInvitation(res.data) : null;
}

export async function getInvitationByToken(_token: string): Promise<InvitationDetail | null> {
  void _token;
  return null;
}

export async function acceptInvitation(token: string): Promise<{ success: boolean; workspaceId?: string; message?: string }> {
  try {
    const res = await apiClient<ApiResponse<{
      workspaceId: string;
      workspaceName: string;
      role: number;
      quotaMode: number;
      creditLimit?: number | null;
    }>>("/workspace-invitations/accept", {
      method: "POST",
      data: { token },
    });
    return res.success
      ? { success: true, workspaceId: res.data?.workspaceId }
      : { success: false, message: res.message || "Failed to accept invitation" };
  } catch (error) {
    return { success: false, message: error instanceof Error ? error.message : "Failed to accept invitation" };
  }
}

export async function cancelInvitation(_invitationId: string): Promise<boolean> {
  void _invitationId;
  return false;
}

export async function getWorkspaceInvitations(): Promise<WorkspaceInvitation[]> {
  return [];
}

export { roleToApi, roleFromApi, quotaToApi, quotaFromApi };
