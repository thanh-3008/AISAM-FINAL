import { apiClient } from "@/lib/apiClient";
import { getWorkspaceInvitations, type WorkspaceInvitation } from "./workspaceInvitationService";

export type MemberRole = "Owner" | "Manager" | "ContentCreator" | "Viewer";
export type MemberStatus = "Active" | "Pending" | "Inactive";

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
}

export interface Team {
  id: string;
  name: string;
  description: string;
  brandCount: number;
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
  };
}

// Default team representing the whole workspace
const DEFAULT_TEAM: Team = {
  id: "workspace-team",
  name: "Workspace Team",
  description: "All workspace members",
  brandCount: 0,
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
    DEFAULT_TEAM.brandCount = 0;
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
    memberIds: data.memberIds,
    activity: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
  return team;
}

export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  if (id === DEFAULT_TEAM.id) {
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
  const res: GenericResponse<BEWorkspaceInvitationDto> = await apiClient("/workspace-invitations", {
    method: "POST",
    data: { email: data.email, role: roleValue, teamIds: data.teamIds },
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

export async function getMemberById(id: string): Promise<TeamMember | null> {
  try {
    const { data: members } = await fetchMembers();
    return members.find((m) => m.id === id) || null;
  } catch {
    return null;
  }
}
