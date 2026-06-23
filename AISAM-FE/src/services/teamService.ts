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

// Local Storage Keys
const LOCAL_TEAMS_KEY = "aisam_local_teams";

function getLocalTeams(): Team[] {
  if (typeof window === "undefined") return [];
  try {
    const data = localStorage.getItem(LOCAL_TEAMS_KEY);
    return data ? JSON.parse(data) : [];
  } catch {
    return [];
  }
}

function saveLocalTeams(teams: Team[]) {
  if (typeof window !== "undefined") {
    localStorage.setItem(LOCAL_TEAMS_KEY, JSON.stringify(teams));
  }
}

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  try {
    const localTeams = getLocalTeams();
    if (localTeams.length === 0) {
      // Default initial team
      const defaultTeam: Team = {
        id: "workspace-team",
        name: "Workspace Members",
        description: "All workspace members",
        brandCount: 0,
        memberIds: [],
        activity: 100,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      
      try {
        const res: GenericResponse<BEWorkspaceMemberDto[]> = await apiClient("/workspace-members");
        if (res?.data) {
          defaultTeam.memberIds = res.data.map(m => m.id);
        }
      } catch (e) {
        console.warn("Failed to fetch workspace members for default team", e);
      }
      
      saveLocalTeams([defaultTeam]);
      return { data: [defaultTeam], total: 1 };
    }
    
    return { data: localTeams, total: localTeams.length };
  } catch {
    return { data: [], total: 0 };
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
  
  const teams = getLocalTeams();
  teams.push(team);
  saveLocalTeams(teams);
  
  return team;
}

export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  const teams = getLocalTeams();
  const index = teams.findIndex(t => t.id === id);
  if (index === -1) return null;
  
  const updatedTeam = {
    ...teams[index],
    ...data,
    updatedAt: new Date().toISOString()
  };
  
  teams[index] = updatedTeam;
  saveLocalTeams(teams);
  return updatedTeam;
}

export async function deleteTeam(id: string): Promise<boolean> {
  const teams = getLocalTeams();
  const index = teams.findIndex(t => t.id === id);
  if (index === -1) return false;
  
  teams.splice(index, 1);
  saveLocalTeams(teams);
  return true;
}

export async function getTeamById(id: string): Promise<Team | null> {
  const teams = getLocalTeams();
  return teams.find(t => t.id === id) || null;
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
    teamIds: data.teamIds || [],
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
