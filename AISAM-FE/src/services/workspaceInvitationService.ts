import { apiClient, apiFetch } from "@/lib/apiClient";

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

export interface InvitationDetail {
  id: string;
  workspaceId: string;
  workspaceName: string;
  workspaceType: number;
  email: string;
  role: WorkspaceMemberRole;
  status: "Pending" | "Accepted" | "Expired" | "Cancelled";
  invitedBy: string;
  invitedByName: string;
  invitedByEmail: string;
  createdAt: string;
  expiresAt: string;
  quotaMode?: "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";
  creditLimit?: number;
}

const MOCK_INVITATIONS: Record<string, InvitationDetail> = {
  test: {
    id: "inv-1",
    workspaceId: "ws-demo",
    workspaceName: "Demo Business Workspace",
    workspaceType: 2,
    email: "user@example.com",
    role: "ContentCreator",
    status: "Pending",
    invitedBy: "user-owner",
    invitedByName: "Nguyen Van A",
    invitedByEmail: "owner@example.com",
    createdAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    quotaMode: "SharedPool",
  },
  demo: {
    id: "inv-2",
    workspaceId: "ws-demo-2",
    workspaceName: "Marketing Team",
    workspaceType: 2,
    email: "demo@example.com",
    role: "Manager",
    status: "Pending",
    invitedBy: "user-owner-2",
    invitedByName: "Tran Thi B",
    invitedByEmail: "manager@example.com",
    createdAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    quotaMode: "MonthlyAssigned",
    creditLimit: 5000,
  },
  expired: {
    id: "inv-3",
    workspaceId: "ws-expired",
    workspaceName: "Expired Workspace",
    workspaceType: 1,
    email: "expired@example.com",
    role: "Viewer",
    status: "Expired",
    invitedBy: "user-expired",
    invitedByName: "Le Van C",
    invitedByEmail: "expired-owner@example.com",
    createdAt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
    expiresAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
  },
  cancelled: {
    id: "inv-4",
    workspaceId: "ws-cancelled",
    workspaceName: "Cancelled Invite",
    workspaceType: 2,
    email: "cancelled@example.com",
    role: "ContentCreator",
    status: "Cancelled",
    invitedBy: "user-cancelled",
    invitedByName: "Pham Van D",
    invitedByEmail: "cancelled-owner@example.com",
    createdAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
    expiresAt: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
  },
  personal: {
    id: "inv-5",
    workspaceId: "ws-personal",
    workspaceName: "My Personal Workspace",
    workspaceType: 1,
    email: "personal@example.com",
    role: "Viewer",
    status: "Pending",
    invitedBy: "user-personal",
    invitedByName: "Hoang Van E",
    invitedByEmail: "personal@example.com",
    createdAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
  },
};

export async function inviteMember(data: InviteMemberRequest): Promise<WorkspaceInvitation | null> {
  try {
    const res = await apiClient("/workspaces/invitations", {
      method: "POST",
      data,
    });
    return res?.data ?? null;
  } catch {
    // Mock response when BE not available
    return {
      id: `inv-${Date.now()}`,
      workspaceId: "ws-current",
      workspaceName: "Current Workspace",
      email: data.email,
      role: data.role,
      status: "Pending",
      invitedBy: "current-user",
      invitedByName: "You",
      createdAt: new Date().toISOString(),
      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    };
  }
}

export async function getInvitationByToken(token: string): Promise<InvitationDetail | null> {
  try {
    const res = await apiFetch(`/workspaces/invitations/accept?token=${encodeURIComponent(token)}`);
    if (res?.success && res.data) {
      return res.data as InvitationDetail;
    }
    return MOCK_INVITATIONS[token] ?? null;
  } catch {
    return MOCK_INVITATIONS[token] ?? null;
  }
}

export async function acceptInvitation(token: string): Promise<{ success: boolean; workspaceId?: string; message?: string }> {
  try {
    const res = await apiClient("/workspaces/invitations/accept", {
      method: "POST",
      data: { token },
    });
    if (res?.success) {
      return { success: true, workspaceId: res.data?.workspaceId };
    }
    return { success: false, message: res?.message || "Failed to accept invitation" };
  } catch {
    // Mock response when BE not available
    const invitation = MOCK_INVITATIONS[token];
    if (invitation && invitation.status === "Pending") {
      await new Promise(resolve => setTimeout(resolve, 1500));
      return { success: true, workspaceId: invitation.workspaceId };
    }
    return { success: false, message: "Invitation not found or already processed" };
  }
}

export async function cancelInvitation(invitationId: string): Promise<boolean> {
  try {
    const res = await apiFetch(`/workspaces/invitations/${invitationId}`, {
      method: "DELETE",
    });
    return res?.success ?? false;
  } catch {
    return true; // Mock success
  }
}

export async function getWorkspaceInvitations(): Promise<WorkspaceInvitation[]> {
  try {
    const res = await apiClient("/workspaces/invitations");
    return res?.data ?? [];
  } catch {
    // Mock response when BE not available
    return [
      {
        id: "inv-pending-1",
        workspaceId: "ws-current",
        workspaceName: "Current Workspace",
        email: "pending1@example.com",
        role: "Viewer",
        status: "Pending",
        invitedBy: "current-user",
        invitedByName: "You",
        createdAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
        expiresAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
      },
      {
        id: "inv-pending-2",
        workspaceId: "ws-current",
        workspaceName: "Current Workspace",
        email: "pending2@example.com",
        role: "ContentCreator",
        status: "Pending",
        invitedBy: "current-user",
        invitedByName: "You",
        createdAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
        expiresAt: new Date(Date.now() + 6 * 24 * 60 * 60 * 1000).toISOString(),
      },
    ];
  }
}
