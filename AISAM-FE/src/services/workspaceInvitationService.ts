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

const ROLE_TO_ENUM: Record<WorkspaceMemberRole, number> = { Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 };

const ENUM_TO_ROLE: Record<number, WorkspaceMemberRole> = { 1: "Owner", 2: "Manager", 3: "ContentCreator", 4: "Viewer" };

export async function inviteMember(data: InviteMemberRequest): Promise<{ data?: WorkspaceInvitation; error?: string } | null> {
  try {
    const res = await apiClient("/workspace-invitations", {
      method: "POST",
      data: { ...data, role: ROLE_TO_ENUM[data.role] ?? 4 },
    });
    if (!res?.success) return { error: res?.message || "Failed to send invitation" };
    return { data: res?.data ?? undefined };
  } catch (err: any) {
    return { error: err?.message || "Network error" };
  }
}

export async function acceptInvitation(token: string): Promise<{ success: boolean; workspaceId?: string; message?: string }> {
  try {
    const res = await apiClient("/workspace-invitations/accept", {
      method: "POST",
      data: { token },
    });
    if (res?.success) {
      return { success: true, workspaceId: res.data?.workspaceId };
    }
    const serverMsg = res?.message || res?.error?.errorMessage || res?.error?.message || "";
    return { success: false, message: serverMsg || "Failed to accept invitation" };
  } catch (err: any) {
    const caughtMsg = err?.message || err || "Invitation not found or already processed";
    console.error("[acceptInvitation]", caughtMsg, err);
    return { success: false, message: typeof caughtMsg === "string" ? caughtMsg : JSON.stringify(caughtMsg) };
  }
}

export async function cancelInvitation(invitationId: string): Promise<boolean> {
  try {
    const res = await apiClient(`/workspace-invitations/${invitationId}`, {
      method: "DELETE",
    });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function getWorkspaceInvitations(): Promise<WorkspaceInvitation[]> {
  try {
    const res = await apiClient("/workspace-invitations");
    const items: any[] = res?.data || (Array.isArray(res) ? res : null) || [];
    if (items.length === 0) return [];
    return items.map((item: any) => ({
      id: item.id,
      workspaceId: item.workspaceId,
      workspaceName: item.workspaceName,
      email: item.email,
      role: ENUM_TO_ROLE[item.role] ?? "Viewer",
      status: item.status || "Pending",
      invitedBy: item.invitedByUserId,
      invitedByName: item.invitedByName || "",
      createdAt: item.createdAt,
      expiresAt: item.expiresAt,
    }));
  } catch {
    return [];
  }
}
