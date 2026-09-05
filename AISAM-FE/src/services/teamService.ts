import { apiClient } from "@/lib/apiClient";
import { getWorkspaceInvitations, type WorkspaceInvitation } from "./workspaceInvitationService";

export type MemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type MemberStatus = "Active" | "Pending" | "Inactive";
export type QuotaMode = "SharedPool" | "LifetimeAssigned" | "MonthlyAssigned";

export interface TeamMember {
  canViewCredit: boolean;
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

export interface Team {
  id: string;
  name: string;
  description: string;
  brandCount: number;
  brandIds: string[];
  memberIds: string[];
  activity: number | null;
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
    canViewCredit: dto.quotaMode != null && dto.creditUsed != null,
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

type TeamApiItem = Omit<Team, "activity">;

async function workspaceMemberIds() {
  const result: GenericResponse<BEWorkspaceMemberDto[]> = await apiClient("/workspace-members");
  if (!result.success || !result.data) throw new Error("Cannot resolve workspace members");
  return result.data;
}

async function teamPayload(data: CreateTeamData): Promise<CreateTeamData> {
  const members = await workspaceMemberIds();
  return { ...data, memberIds: data.memberIds.map(id => {
    const member = members.find(m => m.id === id || m.userId === id);
    if (!member) throw new Error("Selected member is no longer in this workspace");
    return member.userId;
  }) };
}

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  const [result, members] = await Promise.all([apiClient("/teams") as Promise<GenericResponse<TeamApiItem[]>>, workspaceMemberIds()]);
  if (!result.success || !result.data) throw new Error("Cannot load teams");
  const data = result.data.map(team => ({ ...team, description: team.description ?? "", activity: null,
    memberIds: team.memberIds.map(userId => members.find(m => m.userId === userId)?.id).filter((id): id is string => !!id) }));
  return { data, total: data.length };
}

export async function fetchMembers(): Promise<{ data: TeamMember[]; total: number }> {
  const access = await apiClient("/access/context");
  const [membersRes, invitations] = await Promise.all([
    apiClient("/workspace-members").catch(() => null),
    access.data?.canManageTeams ? getWorkspaceInvitations() : Promise.resolve([]),
  ]);

  const activeMembers: TeamMember[] = [];
  if (membersRes?.data) {
    activeMembers.push(...membersRes.data.map(mapMember));
  }
  const teams = (await fetchTeams()).data;
  for (const member of activeMembers) member.teamIds = teams.filter(t => t.memberIds.includes(member.id)).map(t => t.id);

  // Only show pending if the email is NOT already an active member (already accepted)
  const activeEmails = new Set(activeMembers.map((m) => m.email));
  const pendingMembers: TeamMember[] = invitations
    .filter((inv) => !activeEmails.has(inv.email))
    .map((inv: WorkspaceInvitation) => ({
      canViewCredit: access.data?.canManageTeams === true,
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

export async function createTeam(data: CreateTeamData): Promise<Team> {
  const result = await apiClient("/teams", { method: "POST", data: await teamPayload(data) });
  if (!result.success || !result.data?.id) throw new Error("Cannot create team");
  const team = await getTeamById(result.data.id);
  if (!team) throw new Error("Created team cannot be loaded");
  return team;
}

export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  const existing = await getTeamById(id);
  if (!existing) return null;
  const result = await apiClient(`/teams/${id}`, { method: "PUT", data: await teamPayload({ ...existing, ...data }) });
  if (!result.success) throw new Error("Cannot update team");
  return getTeamById(id);
}

export async function deleteTeam(id: string): Promise<boolean> {
  const result = await apiClient(`/teams/${id}`, { method: "DELETE" });
  return result.success === true;
}

export async function getTeamById(id: string): Promise<Team | null> {
  return (await fetchTeams()).data.find(team => team.id === id) ?? null;
}

export async function getTeamBrandAccess(teamId: string, brandId: string) {
  return apiClient(`/teams/${teamId}/brands/${brandId}/access`);
}

export async function setTeamBrandAccess(teamId: string, brandId: string, mode: "ALL" | "SPECIFIC", channelIds: string[]) {
  return apiClient(`/teams/${teamId}/brands/${brandId}/access`, { method: "PUT", data: { mode: mode === "ALL" ? 0 : 1, channelIds } });
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
    canViewCredit: inv?.quotaMode != null,
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
