import { apiClient } from "@/lib/apiClient";

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
  name: "Workspace Members",
  description: "All workspace members",
  brandCount: 0,
  memberIds: [],
  activity: 100,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  try {
    const res: GenericResponse<BEWorkspaceMemberDto[]> = await apiClient("/workspace-members");
    const members = res?.data || [];
    DEFAULT_TEAM.memberIds = members.map((m) => m.id);
    DEFAULT_TEAM.brandCount = 0;
    return { data: [DEFAULT_TEAM], total: 1 };
  } catch {
    return { data: [DEFAULT_TEAM], total: 1 };
  }
}

export async function fetchMembers(): Promise<{ data: TeamMember[]; total: number }> {
  const res: GenericResponse<BEWorkspaceMemberDto[]> = await apiClient("/workspace-members");
  if (res?.data) {
    const members = res.data.map(mapMember);
    return { data: members, total: members.length };
  }
  return { data: [], total: 0 };
}

/** @deprecated Local-only: does not persist to BE. Use workspace-members API. */
export async function createTeam(data: CreateTeamData): Promise<Team> {
  console.warn("[DEPRECATED] createTeam is local-only and is not persisted to BE.");
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

/** @deprecated Local-only: does not persist to BE. Use workspace-members API. */
export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  console.warn("[DEPRECATED] updateTeam is local-only and is not persisted to BE.");
  if (id === DEFAULT_TEAM.id) {
    return {
      ...DEFAULT_TEAM,
      name: data.name || DEFAULT_TEAM.name,
      description: data.description || DEFAULT_TEAM.description,
    };
  }
  return null;
}

/** @deprecated Local-only: does not persist to BE. Use workspace-members API. */
export async function deleteTeam(id: string): Promise<boolean> {
  console.warn("[DEPRECATED] deleteTeam is local-only and is not persisted to BE.");
  return id !== DEFAULT_TEAM.id;
}

/** @deprecated Local-only: does not persist to BE. Use workspace-members API. */
export async function getTeamById(id: string): Promise<Team | null> {
  console.warn("[DEPRECATED] getTeamById is local-only and is not persisted to BE.");
  return id === DEFAULT_TEAM.id ? DEFAULT_TEAM : null;
}

export async function inviteMember(data: InviteMemberData): Promise<TeamMember> {
  const roleValue = ({ Owner: 1, Manager: 2, ContentCreator: 3, Viewer: 4 } as const)[data.role];
  await apiClient("/workspace-invitations", {
    method: "POST",
    data: { email: data.email, role: roleValue },
  });

  const member: TeamMember = {
    id: `pending_${Date.now()}`,
    name: data.email.split("@")[0],
    email: data.email,
    avatar: null,
    role: data.role,
    status: "Pending",
    teamIds: [],
    lastActive: new Date().toISOString(),
    createdAt: new Date().toISOString(),
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

export async function removeMember(id: string): Promise<boolean> {
  const res: GenericResponse<object> = await apiClient(`/workspace-members/${id}`, {
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
