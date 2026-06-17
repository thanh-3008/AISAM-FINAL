import { apiClient } from "@/lib/apiClient";

export type WorkspaceMemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";

export interface InviteMemberRequest {
  email: string;
  role: WorkspaceMemberRole;
  quotaMode?: "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";
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

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  data?: T;
}

const ROLE_TO_ENUM: Record<WorkspaceMemberRole, number> = { Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 };

export async function inviteMember(data: InviteMemberRequest): Promise<WorkspaceInvitation | null> {
  try {
    const res: GenericResponse<WorkspaceInvitation> = await apiClient("/workspace-invitations", {
      method: "POST",
      data: { ...data, role: ROLE_TO_ENUM[data.role] ?? 4 },
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function acceptInvitation(token: string): Promise<{ success: boolean; workspaceId?: string; message?: string }> {
  try {
    const res: GenericResponse<{ workspaceId?: string }> = await apiClient("/workspace-invitations/accept", {
      method: "POST",
      data: { token },
    });
    if (res?.success) {
      return { success: true, workspaceId: res.data?.workspaceId };
    }
    return { success: false, message: res?.message || "Failed to accept invitation" };
  } catch {
    return { success: false, message: "Invitation not found or already processed" };
  }
}

export async function cancelInvitation(_invitationId: string): Promise<boolean> {
  // No BE endpoint — returns false
  return false;
}

export async function getWorkspaceInvitations(): Promise<WorkspaceInvitation[]> {
  // No BE endpoint — returns empty
  return [];
}
