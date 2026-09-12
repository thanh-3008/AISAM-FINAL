import { apiClient } from "@/lib/apiClient";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getWorkspaceInvitations, type WorkspaceInvitation } from "./workspaceInvitationService";

export type MemberRole = "Owner" | "WorkspaceManager" | "Manager" | "Member" | "ContentCreator" | "Viewer";
export type TeamRole = "Manager" | "ContentCreator" | "Viewer";
export type MemberStatus = "Active" | "Pending" | "Inactive";
export type QuotaMode = "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";

export interface TeamMember {
  id: string;
  userId?: string;
  name: string;
  email: string;
  avatar: string | null;
  role: MemberRole;
  status: MemberStatus;
  teamIds: string[];
  lastActive: string;
  createdAt: string;
  quotaMode: QuotaMode;
  creditLimit: number | null;
  creditUsed: number;
}

export interface MemberCreditUsageRecord {
  id: string;
  userId: string;
  userName: string;
  action: string;
  credits: number;
  featureUsed: string;
  status: "Success" | "Failed";
  createdAt: string;
}

export const QUOTA_MODE_LABELS: Record<QuotaMode, string> = {
  SharedPool: "Shared Pool",
  LifetimeAssigned: "Lifetime Assigned Limit",
  MonthlyAssigned: "Monthly Assigned Limit",
};

export const QUOTA_MODE_BE: Record<QuotaMode, number> = {
  SharedPool: 1,
  LifetimeAssigned: 2,
  MonthlyAssigned: 3,
};

export const QUOTA_MODE_FROM_BE: Record<number, QuotaMode> = {
  1: "SharedPool",
  2: "LifetimeAssigned",
  3: "MonthlyAssigned",
};

// ── Team entity types ──

export interface Team {
  id: string;
  name: string;
  description: string | null;
  status: string;
  memberCount: number;
  brandCount: number;
  createdAt: string;
  updatedAt?: string | null;
  memberIds?: string[];
  brandIds?: string[];
  activity?: number | string;
  hasManager?: boolean;
}

export interface TeamDetail {
  id: string;
  name: string;
  description: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  members: TeamDetailMember[];
  brands: TeamDetailBrand[];
}

export interface TeamDetailMember {
  userId: string;
  name: string;
  email: string;
  role: string;
  joinedAt: string;
  isActive: boolean;
}

export interface TeamDetailBrand {
  brandId: string;
  brandName: string;
  isActive: boolean;
  assignedAt: string;
}

export interface CreateTeamData {
  name: string;
  description?: string;
  members?: { userId: string; role: string }[];
  memberIds?: string[];
  brandIds?: string[];
}

export interface InviteMemberData {
  email: string;
  role: MemberRole;
  teamIds: string[];
  quotaMode?: QuotaMode;
  creditLimit?: number | null;
}

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

// BE DTOs
interface BEWorkspaceMemberDto {
  id: string;
  userId: string;
  email: string;
  fullName: string | null;
  role: number;
  quotaMode: number;
  creditLimit: number | null;
  creditUsed: number;
  creditPeriodStart: string | null;
  joinedAt: string;
}

interface BEWorkspaceInvitationDto {
  id: string;
  workspaceId: string;
  workspaceName: string;
  email: string;
  role: number;
  quotaMode: number;
  creditLimit: number | null;
  invitedByUserId: string;
  expiresAt: string;
  createdAt: string;
}

// Role mapping: BE enum values match FE string values
const ROLE_MAP: Record<number, MemberRole> = {
  1: "Owner",
  2: "WorkspaceManager",
  3: "Member",
  4: "Viewer",
};

export const ROLE_TO_BE: Record<MemberRole, number> = {
  Owner: 1,
  WorkspaceManager: 2,
  Manager: 2,
  Member: 3,
  ContentCreator: 3,
  Viewer: 4,
};

function mapRole(beRole: number): MemberRole {
  return ROLE_MAP[beRole] || "Viewer";
}

function mapMember(dto: BEWorkspaceMemberDto): TeamMember {
  return {
    id: dto.id,
    userId: dto.userId,
    name: dto.fullName || dto.email.split("@")[0],
    email: dto.email,
    avatar: null,
    role: mapRole(dto.role),
    status: "Active",
    teamIds: [],
    lastActive: dto.joinedAt,
    createdAt: dto.joinedAt,
    quotaMode: QUOTA_MODE_FROM_BE[dto.quotaMode] || "SharedPool",
    creditLimit: dto.creditLimit ?? null,
    creditUsed: dto.creditUsed ?? 0,
  };
}

// ── Team CRUD (real API) ──

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  try {
    const res: GenericResponse<{ items: Team[]; totalCount: number }> =
      await apiClient("/teams/manage");
    if (res?.success && res.data) {
      return { data: res.data.items, total: res.data.totalCount };
    }
  } catch {
    // fall through
  }
  return { data: [], total: 0 };
}

export async function getTeamById(id: string): Promise<TeamDetail | null> {
  try {
    const res: GenericResponse<TeamDetail> = await apiClient(`/teams/${id}`);
    if (res?.success && res.data) {
      return res.data;
    }
  } catch {
    // fall through
  }
  return null;
}

export async function createTeam(data: CreateTeamData): Promise<TeamDetail> {
  const res: GenericResponse<TeamDetail> = await apiClient("/teams", {
    method: "POST",
    data: {
      name: data.name,
      description: data.description || null,
      members: data.members || [],
    },
  });
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to create team");
  }
  return res.data;
}

export async function updateTeam(
  id: string,
  data: { name: string; description?: string }
): Promise<TeamDetail> {
  const res: GenericResponse<TeamDetail> = await apiClient(`/teams/${id}`, {
    method: "PUT",
    data: { name: data.name, description: data.description || null },
  });
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update team");
  }
  return res.data;
}

export async function deleteTeam(id: string): Promise<boolean> {
  const res: GenericResponse<unknown> = await apiClient(`/teams/${id}`, {
    method: "DELETE",
  });
  return res?.success === true;
}

// ── Team Members (real API) ──

export async function addTeamMember(
  teamId: string,
  userId: string,
  role: string
): Promise<TeamDetailMember> {
  const res: GenericResponse<TeamDetailMember> = await apiClient(
    `/teams/${teamId}/members`,
    { method: "POST", data: { userId, role } }
  );
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to add team member");
  }
  return res.data;
}

export async function removeTeamMember(
  teamId: string,
  userId: string
): Promise<boolean> {
  const res: GenericResponse<unknown> = await apiClient(
    `/teams/${teamId}/members/${userId}`,
    { method: "DELETE" }
  );
  return res?.success === true;
}

export async function updateTeamMemberRole(
  teamId: string,
  userId: string,
  role: string
): Promise<TeamDetailMember> {
  const res: GenericResponse<TeamDetailMember> = await apiClient(
    `/teams/${teamId}/members/${userId}`,
    { method: "PUT", data: { role } }
  );
  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update member role");
  }
  return res.data;
}

// ── Workspace Members (existing, unchanged) ──

export async function fetchMembers(): Promise<{ data: TeamMember[]; total: number }> {
  console.debug("[teamService.fetchMembers] activeWorkspace.id at call time:", getStoredActiveWorkspace()?.id);
  const [membersRes, invitations] = await Promise.all([
    apiClient("/workspace-members").catch(() => null),
    getWorkspaceInvitations(),
  ]);

  const activeMembers: TeamMember[] = [];
  if (membersRes?.data) {
    activeMembers.push(...membersRes.data.map(mapMember));
  }

  // Only show pending if the email is NOT already an active member (already accepted)
  const activeEmails = new Set(activeMembers.map((m) => m.email));
  const pendingMembers: TeamMember[] = invitations
    .filter((inv) => !activeEmails.has(inv.email))
    .map((inv: WorkspaceInvitation) => ({
      id: inv.id,
      name: inv.email.split("@")[0],
      email: inv.email,
      avatar: null,
      role: inv.role as MemberRole,
      status: "Pending" as MemberStatus,
      teamIds: [],
      lastActive: inv.createdAt,
      createdAt: inv.createdAt,
      quotaMode: (inv.quotaMode != null ? QUOTA_MODE_FROM_BE[inv.quotaMode] : "SharedPool") as QuotaMode,
      creditLimit: inv.creditLimit ?? null,
      creditUsed: 0,
    }));

  const seen = new Set<string>();
  const allMembers = [...pendingMembers, ...activeMembers].filter((m) => {
    if (seen.has(m.id)) return false;
    seen.add(m.id);
    return true;
  });
  return { data: allMembers, total: allMembers.length };
}

export async function inviteMember(data: InviteMemberData): Promise<TeamMember> {
  const roleValue = ROLE_TO_BE[data.role] ?? 3;
  const payload: Record<string, unknown> = { email: data.email, role: roleValue, teamIds: data.teamIds };
  if (data.quotaMode) {
    payload.quotaMode = QUOTA_MODE_BE[data.quotaMode];
  }
  if (data.creditLimit != null && data.creditLimit > 0) {
    payload.creditLimit = data.creditLimit;
  }
  const res: GenericResponse<BEWorkspaceInvitationDto> = await apiClient("/workspace-invitations", {
    method: "POST",
    data: payload,
  });

  const inv = res?.data;
  const member: TeamMember = {
    id: inv?.id || `pending_${Date.now()}`,
    name: data.email.split("@")[0],
    email: data.email,
    avatar: null,
    role: data.role,
    status: "Pending",
    teamIds: data.teamIds,
    lastActive: inv?.createdAt || new Date().toISOString(),
    createdAt: inv?.createdAt || new Date().toISOString(),
    quotaMode: data.quotaMode || "SharedPool",
    creditLimit: data.creditLimit ?? null,
    creditUsed: 0,
  };

  return member;
}

export async function updateMemberRole(id: string, role: MemberRole): Promise<TeamMember | null> {
  const roleValue = ROLE_TO_BE[role] ?? 3;
  const res: GenericResponse<BEWorkspaceMemberDto> = await apiClient(`/workspace-members/${id}/role`, {
    method: "PUT",
    data: { role: roleValue },
  });
  if (res?.data) {
    return mapMember(res.data);
  }
  throw new Error(res?.error?.errorMessage || "Failed to update member role");
}

export async function transferWorkspaceOwnership(targetMemberId: string): Promise<TeamMember> {
  const res: GenericResponse<BEWorkspaceMemberDto> = await apiClient("/workspace-members/ownership-transfer", {
    method: "POST",
    data: { targetMemberId },
  });

  if (res?.data) {
    return mapMember(res.data);
  }

  throw new Error(res?.error?.errorMessage || res?.message || "Failed to transfer ownership");
}

export async function removeMember(id: string, status?: MemberStatus): Promise<boolean> {
  const endpoint = status === "Pending"
    ? `/workspace-invitations/${id}`
    : `/workspace-members/${id}`;
  const res: GenericResponse<object> = await apiClient(endpoint, {
    method: "DELETE",
  });
  return res?.success === true || res?.statusCode === 200;
}

export async function updateMemberQuota(
  memberId: string,
  quotaMode: QuotaMode,
  creditLimit: number | null
): Promise<{ success: boolean; message?: string }> {
  try {
    const res = await apiClient(`/workspace-members/${memberId}/quota`, {
      method: "PUT",
      data: { quotaMode: QUOTA_MODE_BE[quotaMode], creditLimit: creditLimit },
    });
    return { success: res?.success === true, message: res?.message };
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : "Network error";
    return { success: false, message };
  }
}

export async function fetchMemberCreditUsage(
  memberId: string,
  page = 1,
  pageSize = 6
): Promise<{ data: MemberCreditUsageRecord[]; totalCount: number }> {
  try {
    const res: GenericResponse<{ data: MemberCreditUsageRecord[]; totalCount: number }> = await apiClient(
      `/credit-usage?memberId=${encodeURIComponent(memberId)}&page=${page}&pageSize=${pageSize}`
    );

    if (res?.success && res.data) {
      return {
        data: res.data.data || [],
        totalCount: res.data.totalCount || 0,
      };
    }
  } catch {
    // Keep member detail usable if history cannot be loaded.
  }

  return { data: [], totalCount: 0 };
}

export async function getMemberById(id: string): Promise<TeamMember | null> {
  try {
    const { data: members } = await fetchMembers();
    return members.find((m) => m.id === id) || null;
  } catch {
    return null;
  }
}
