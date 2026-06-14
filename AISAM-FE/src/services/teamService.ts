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

const STORAGE_KEY = "aisam_teams_v1";
const MEMBERS_STORAGE_KEY = "aisam_team_members_v1";

const INITIAL_MOCK_TEAMS: Team[] = [
  {
    id: "mk",
    name: "Marketing Team",
    description: "Global marketing initiatives and strategy.",
    brandCount: 4,
    memberIds: ["sc", "jd", "m1", "m2", "m3", "m4", "m5", "m6", "m7", "m8"],
    activity: 85,
    createdAt: new Date(Date.now() - 90 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 2 * 86400000).toISOString(),
  },
  {
    id: "cc",
    name: "Content Creators",
    description: "AI-assisted content production & design.",
    brandCount: 2,
    memberIds: ["ml", "m9", "m10", "m11", "m12", "m13", "m14", "m15", "m16", "m17", "m18", "m19", "m20", "m21"],
    activity: 72,
    createdAt: new Date(Date.now() - 60 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 1 * 86400000).toISOString(),
  },
  {
    id: "bm",
    name: "Brand Managers",
    description: "High-level brand compliance and assets.",
    brandCount: 6,
    memberIds: ["m22", "m23", "m24", "m25"],
    activity: 91,
    createdAt: new Date(Date.now() - 45 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 5 * 86400000).toISOString(),
  },
];

const INITIAL_MOCK_MEMBERS: TeamMember[] = [
  {
    id: "sc",
    name: "Sarah Connor",
    email: "sarah@aisam.intelligence",
    avatar: null,
    role: "Owner",
    status: "Active",
    teamIds: ["mk"],
    lastActive: new Date(Date.now() - 2 * 60000).toISOString(),
    createdAt: new Date(Date.now() - 120 * 86400000).toISOString(),
  },
  {
    id: "jd",
    name: "James Doe",
    email: "j.doe@marketing.com",
    avatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuB6rV9C4ceJcTyc6u5Hhuaj7cCZPxONm8g2E3-UmRFDq90BzwX0tIT-5q5yrOq4SMFf4d0AkiSs90HlpvRBxvqDNfzQKrufRG2hiq5UCI6OVioHJN4vcIoWt00GTWaW9nr85AWKo3Wky5xxaOSewEY5SH6UsXzgquMIjU5qwJnli22AD2THtrxb_B7kmiDifKlR66xUSsvI7x-OU8g0-DPpQDsKv64Sj6Erqzhg2TNkN0qmNW3GXtkrNYSuTdSvKp_fevI2qvc2YYM",
    role: "Manager",
    status: "Active",
    teamIds: ["cc"],
    lastActive: new Date(Date.now() - 5 * 60000).toISOString(),
    createdAt: new Date(Date.now() - 90 * 86400000).toISOString(),
  },
  {
    id: "ml",
    name: "Maya Lin",
    email: "maya@design.co",
    avatar: null,
    role: "ContentCreator",
    status: "Pending",
    teamIds: ["bm"],
    lastActive: new Date(Date.now() - 3600000).toISOString(),
    createdAt: new Date(Date.now() - 7 * 86400000).toISOString(),
  },
  ...Array.from({ length: 21 }, (_, i) => ({
    id: `m${i + 1}`,
    name: `Member ${i + 1}`,
    email: `member${i + 1}@aisam.com`,
    avatar: null,
    role: (["Viewer", "ContentCreator", "Manager", "Viewer"] as MemberRole[])[i % 4],
    status: (i % 5 === 0 ? "Pending" : i % 7 === 0 ? "Inactive" : "Active") as MemberStatus,
    teamIds: [INITIAL_MOCK_TEAMS[i % 3].id],
    lastActive: new Date(Date.now() - (i + 1) * 3600000).toISOString(),
    createdAt: new Date(Date.now() - (30 + i) * 86400000).toISOString(),
  })),
];

function loadTeams(): Team[] {
  if (typeof window === "undefined") return [...INITIAL_MOCK_TEAMS];
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as Team[];
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    }
  } catch { /* fallback */ }
  const initial = [...INITIAL_MOCK_TEAMS];
  localStorage.setItem(STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

function saveTeams(teams: Team[]): void {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(teams));
  } catch { /* ignore */ }
}

const OLD_ROLE_MAP: Record<string, MemberRole> = {
  Admin: "Manager",
  Editor: "ContentCreator",
  Member: "Viewer",
};

function loadMembers(): TeamMember[] {
  if (typeof window === "undefined") return [...INITIAL_MOCK_MEMBERS];
  try {
    const stored = localStorage.getItem(MEMBERS_STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as TeamMember[];
      if (Array.isArray(parsed) && parsed.length > 0) {
        let migrated = false;
        const mapped = parsed.map((m) => {
          const newRole = OLD_ROLE_MAP[m.role];
          if (newRole) {
            migrated = true;
            return { ...m, role: newRole };
          }
          return m;
        });
        if (migrated) {
          localStorage.setItem(MEMBERS_STORAGE_KEY, JSON.stringify(mapped));
        }
        return mapped;
      }
    }
  } catch { /* fallback */ }
  const initial = [...INITIAL_MOCK_MEMBERS];
  localStorage.setItem(MEMBERS_STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

function saveMembers(members: TeamMember[]): void {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(MEMBERS_STORAGE_KEY, JSON.stringify(members));
  } catch { /* ignore */ }
}

let MOCK_TEAMS: Team[] = loadTeams();
let MOCK_MEMBERS: TeamMember[] = loadMembers();

export async function fetchTeams(): Promise<{ data: Team[]; total: number }> {
  return { data: [...MOCK_TEAMS], total: MOCK_TEAMS.length };
}

export async function fetchMembers(): Promise<{ data: TeamMember[]; total: number }> {
  return { data: [...MOCK_MEMBERS], total: MOCK_MEMBERS.length };
}

export async function createTeam(data: CreateTeamData): Promise<Team> {
  const team: Team = {
    id: `t_${Date.now()}`,
    name: data.name,
    description: data.description,
    brandCount: data.brandIds.length,
    memberIds: data.memberIds,
    activity: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
  MOCK_TEAMS.unshift(team);
  saveTeams(MOCK_TEAMS);
  return team;
}

export async function updateTeam(id: string, data: Partial<CreateTeamData>): Promise<Team | null> {
  const idx = MOCK_TEAMS.findIndex((t) => t.id === id);
  if (idx < 0) return null;
  if (data.name !== undefined) MOCK_TEAMS[idx].name = data.name;
  if (data.description !== undefined) MOCK_TEAMS[idx].description = data.description;
  if (data.brandIds !== undefined) MOCK_TEAMS[idx].brandCount = data.brandIds.length;
  if (data.memberIds !== undefined) MOCK_TEAMS[idx].memberIds = data.memberIds;
  MOCK_TEAMS[idx].updatedAt = new Date().toISOString();
  saveTeams(MOCK_TEAMS);
  return MOCK_TEAMS[idx];
}

export async function deleteTeam(id: string): Promise<boolean> {
  const idx = MOCK_TEAMS.findIndex((t) => t.id === id);
  if (idx >= 0) {
    MOCK_TEAMS.splice(idx, 1);
    saveTeams(MOCK_TEAMS);
    MOCK_MEMBERS = MOCK_MEMBERS.map((m) => ({
      ...m,
      teamIds: m.teamIds.filter((tid) => tid !== id),
    }));
    saveMembers(MOCK_MEMBERS);
  }
  return idx >= 0;
}

export async function getTeamById(id: string): Promise<Team | null> {
  return MOCK_TEAMS.find((t) => t.id === id) || null;
}

export async function inviteMember(data: InviteMemberData): Promise<TeamMember> {
  const member: TeamMember = {
    id: `mem_${Date.now()}`,
    name: data.email.split("@")[0],
    email: data.email,
    avatar: null,
    role: data.role,
    status: "Pending",
    teamIds: data.teamIds,
    lastActive: new Date().toISOString(),
    createdAt: new Date().toISOString(),
  };
  MOCK_MEMBERS.unshift(member);
  saveMembers(MOCK_MEMBERS);
  return member;
}

export async function updateMemberRole(id: string, role: MemberRole): Promise<TeamMember | null> {
  const idx = MOCK_MEMBERS.findIndex((m) => m.id === id);
  if (idx < 0) return null;
  MOCK_MEMBERS[idx].role = role;
  saveMembers(MOCK_MEMBERS);
  return MOCK_MEMBERS[idx];
}

export async function removeMember(id: string): Promise<boolean> {
  const idx = MOCK_MEMBERS.findIndex((m) => m.id === id);
  if (idx >= 0) {
    MOCK_MEMBERS.splice(idx, 1);
    saveMembers(MOCK_MEMBERS);
  }
  return idx >= 0;
}

export async function getMemberById(id: string): Promise<TeamMember | null> {
  return MOCK_MEMBERS.find((m) => m.id === id) || null;
}
