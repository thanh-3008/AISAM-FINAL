import { apiClient } from "@/lib/apiClient";
import { getWorkspaceInvitations, type WorkspaceInvitation } from "./workspaceInvitationService";

export type MemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type MemberStatus = "Active" | "Pending" | "Inactive";
export type QuotaMode = "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";

export interface TeamMember {
  id: string;
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

export interface Team {
  id: string;
  name: string;
  description: string;
  brandCount: number;
  brandIds: string[];
  memberIds: string[];
  activity: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTeamData {
  name: string;
  description: string;
  brandIds: string[];
  memberIds: string[];
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
  2: "Manager",
  3: "ContentCreator",
  4: "Viewer",
};

function mapRole(beRole: number): MemberRole {
  return ROLE_MAP[beRole] || "Viewer";
}

function mapMember(dto: BEWorkspaceMemberDto): TeamMember {
  return {
    id: dto.id,
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

// Default team representing the whole workspace
const DEFAULT_TEAM: Team = {
  id: "workspace-team",
  name: "Workspace Team",
  description: "All workspace members",
  brandCount: 0,
  brandIds: [],
  memberIds: [],
  activity: 100,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  try {
    const res: GenericResponse<BEWorkspaceMemberDto[]> = await apiClient("/workspace-members").catch(() => null);
    const memberIds = (res?.data || []).map((m: BEWorkspaceMemberDto) => m.id);
    DEFAULT_TEAM.memberIds = memberIds;
    return { data: [DEFAULT_TEAM], total: 1 };
  } catch {
    return { data: [DEFAULT_TEAM], total: 1 };
  }
}

export async function fetchMembers(): Promise<{ data: TeamMember[]; total: number }> {
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
      quotaMode: "SharedPool" as QuotaMode,
      creditLimit: null,
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

export async function createTeam(data: CreateTeamData): Promise<Team> {
  const team: Team = {
    id: `team_${Date.now()}`,
    name: data.name,
    description: data.description,
    brandCount: data.brandIds.length,
    brandIds: data.brandIds,
    memberIds: data.memberIds,
    activity: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
  return team;
}

export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  if (id === DEFAULT_TEAM.id) {
    DEFAULT_TEAM.brandIds = data.brandIds ?? DEFAULT_TEAM.brandIds;
    DEFAULT_TEAM.brandCount = DEFAULT_TEAM.brandIds.length;
    return {
      ...DEFAULT_TEAM,
      name: data.name || DEFAULT_TEAM.name,
      description: data.description || DEFAULT_TEAM.description,
    };
  }
  return null;
}

export async function deleteTeam(id: string): Promise<boolean> {
  return id !== DEFAULT_TEAM.id;
}

export async function getTeamById(id: string): Promise<Team | null> {
  return id === DEFAULT_TEAM.id ? DEFAULT_TEAM : null;
}

export async function inviteMember(data: InviteMemberData): Promise<TeamMember> {
  const roleValue = ({ Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 } as const)[data.role];
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
  const roleValue = ({ Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 } as const)[role];
  const res: GenericResponse<BEWorkspaceMemberDto> = await apiClient(`/workspace-members/${id}/role`, {
    method: "PUT",
    data: { role: roleValue },
  });
  if (res?.data) {
    return mapMember(res.data);
  }
  throw new Error(res?.error?.errorMessage || "Failed to update member role");
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
      data: { mode: QUOTA_MODE_BE[quotaMode], limit: creditLimit },
    });
    return { success: res?.success === true, message: res?.message };
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : "Network error";
    return { success: false, message };
  }
}

export async function getMemberById(id: string): Promise<TeamMember | null> {
  try {
    const { data: members } = await fetchMembers();
    return members.find((m) => m.id === id) || null;
  } catch {
    return null;
  }
}
