"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { useRbac, type TeamRoleV2, type WorkspaceRoleV2 } from "@/contexts/RbacContext";
import type { Team, TeamDetail } from "@/services/teamService";
import { getRoleLabel, WORKSPACE_ROLE_LABELS, TEAM_ROLE_LABELS } from "@/lib/roleLabels";

type Member = {
  id: string;
  userId: string;
  fullName?: string;
  email?: string;
  workspaceRole: WorkspaceRoleV2;
};

type PendingInvitation = {
  id: string;
  email: string;
  workspaceRole?: WorkspaceRoleV2;
  invitedByName?: string;
  createdAt: string;
  expiresAt: string;
};

type MemberRoleFilter = "All" | WorkspaceRoleV2;
type TeamStatusFilter = "All" | "Active" | "Inactive";
type ManagementView = "overview" | "members" | "invitations" | "teams";

const roleName = (role: string) => getRoleLabel(role);

const workspaceRoleStyle = (role: string) => ({
  Owner: "bg-blue-50 text-blue-700 ring-blue-100",
  WorkspaceManager: "bg-violet-50 text-violet-700 ring-violet-100",
  Member: "bg-slate-100 text-slate-700 ring-slate-200",
}[role] ?? "bg-slate-100 text-slate-700 ring-slate-200");

const teamRoleStyle = (role: string) => ({
  Manager: "bg-indigo-50 text-indigo-700",
  ContentCreator: "bg-emerald-50 text-emerald-700",
  Viewer: "bg-amber-50 text-amber-700",
}[role] ?? "bg-slate-100 text-slate-700");

const inputClass = "h-11 rounded-xl border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-50 disabled:cursor-not-allowed disabled:bg-slate-50";
const primaryButton = "inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-blue-600 px-5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50";
const secondaryButton = "inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white px-4 text-sm font-semibold text-slate-700 transition hover:border-blue-200 hover:bg-blue-50 hover:text-blue-700 disabled:cursor-not-allowed disabled:opacity-50";

function initials(name?: string, email?: string) {
  const source = name?.trim() || email?.split("@")[0] || "MB";
  return source.split(/\s+/).slice(0, 2).map(part => part[0]?.toUpperCase()).join("");
}

function normalizedStatus(status?: string) {
  return status?.toLowerCase() === "active" ? "Active" : "Inactive";
}

export default function RbacTeamManagement() {
  const rbac = useRbac()!;
  const admin = rbac.actions.includes("team.manage");
  const workspaceHrAdmin = rbac.workspaceRole === "Owner" || rbac.workspaceRole === "WorkspaceManager";
  const canViewPerformance = workspaceHrAdmin || rbac.teams.some(team => team.role === "Manager");
  const [teams, setTeams] = useState<Team[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [pendingInvitations, setPendingInvitations] = useState<PendingInvitation[]>([]);
  const [selected, setSelected] = useState("");
  const [detail, setDetail] = useState<TeamDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState(false);
  const [view, setView] = useState<ManagementView>("overview");

  const [memberSearch, setMemberSearch] = useState("");
  const [memberRoleFilter, setMemberRoleFilter] = useState<MemberRoleFilter>("All");
  const [invitationSearch, setInvitationSearch] = useState("");
  const [teamSearch, setTeamSearch] = useState("");
  const [teamStatusFilter, setTeamStatusFilter] = useState<TeamStatusFilter>("All");

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [email, setEmail] = useState("");
  const [inviteRole, setInviteRole] = useState("3");
  const [user, setUser] = useState("");
  const [teamRole, setTeamRole] = useState<TeamRoleV2>("ContentCreator");

  const canManage = admin || rbac.teams.some(t => t.teamId === selected && t.role === "Manager");
  const assignableTeamRoles: TeamRoleV2[] = admin
    ? ["Manager", "ContentCreator", "Viewer"]
    : ["ContentCreator", "Viewer"];

  const load = useCallback(async () => {
    const [teamResponse, memberResponse, invitationResponse] = await Promise.all([
      apiClient("/teams/manage"),
      apiClient("/workspace-members"),
      workspaceHrAdmin ? apiClient("/workspace-invitations") : Promise.resolve({ data: [] }),
    ]);
    const nextTeams = Array.isArray(teamResponse?.data?.items) ? teamResponse.data.items as Team[] : [];
    const nextMembers = Array.isArray(memberResponse?.data) ? memberResponse.data as Member[] : [];
    const nextInvitations = Array.isArray(invitationResponse?.data) ? invitationResponse.data as PendingInvitation[] : [];
    setTeams(nextTeams);
    setMembers(nextMembers);
    setPendingInvitations(nextInvitations);
    setSelected(current => current && nextTeams.some(team => team.id === current)
      ? current
      : nextTeams[0]?.id ?? "");
  }, [workspaceHrAdmin]);

  useEffect(() => {
    let active = true;
    setLoading(true);
    load()
      .catch(e => { if (active) setError(e instanceof Error ? e.message : "Failed to load Team data."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [load]);

  useEffect(() => {
    let active = true;
    setDetail(null);
    if (view !== "teams" || !selected) return () => { active = false; };
    setLoadingDetail(true);
    apiClient(`/teams/${selected}`)
      .then(response => { if (active) setDetail(response.data); })
      .catch(e => { if (active) setError(e instanceof Error ? e.message : "Failed to load Team details."); })
      .finally(() => { if (active) setLoadingDetail(false); });
    return () => { active = false; };
  }, [selected, view]);

  const run = async (action: () => Promise<unknown>, message: string, refreshDetail = true) => {
    setBusy(true);
    setError("");
    setNotice("");
    try {
      await action();
      await load();
      if (selected && refreshDetail) setDetail((await apiClient(`/teams/${selected}`)).data);
      setNotice(message);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to save changes. Reload data and try again.");
    } finally {
      setBusy(false);
    }
  };

  const filteredMembers = useMemo(() => {
    const query = memberSearch.trim().toLowerCase();
    return members.filter(member => {
      const matchesQuery = !query || `${member.fullName ?? ""} ${member.email ?? ""}`.toLowerCase().includes(query);
      const matchesRole = memberRoleFilter === "All" || member.workspaceRole === memberRoleFilter;
      return matchesQuery && matchesRole;
    });
  }, [members, memberRoleFilter, memberSearch]);

  const filteredTeams = useMemo(() => {
    const query = teamSearch.trim().toLowerCase();
    return teams.filter(team => {
      const matchesQuery = !query || `${team.name} ${team.description ?? ""}`.toLowerCase().includes(query);
      const status = normalizedStatus(team.status);
      return matchesQuery && (teamStatusFilter === "All" || status === teamStatusFilter);
    });
  }, [teamSearch, teamStatusFilter, teams]);

  const filteredInvitations = useMemo(() => {
    const query = invitationSearch.trim().toLowerCase();
    return pendingInvitations.filter(invitation => !query ||
      `${invitation.email} ${invitation.invitedByName ?? ""}`.toLowerCase().includes(query));
  }, [invitationSearch, pendingInvitations]);

  const selectedTeam = teams.find(team => team.id === selected);
  const managerCount = members.filter(member => member.workspaceRole === "WorkspaceManager").length;
  const activeTeamCount = teams.filter(team => normalizedStatus(team.status) === "Active").length;
  const activeDetailMembers = (detail?.members ?? []).filter(member => member.isActive);
  const activeBrands = (detail?.brands ?? []).filter(brand => brand.isActive);
  const availableMembers = members.filter(member => !(detail?.members ?? []).some(teamMember => teamMember.userId === member.userId && teamMember.isActive));
  const navigation: Array<{ id: ManagementView; label: string; description: string; icon: string; count?: number }> = [
    { id: "overview", label: "Overview", description: "Workspace summary", icon: "dashboard" },
    { id: "members", label: "Workspace members", description: "Roles and access", icon: "group", count: members.length },
    ...(workspaceHrAdmin ? [{ id: "invitations" as const, label: "Invitations", description: "Pending invitations", icon: "outgoing_mail", count: pendingInvitations.length }] : []),
    { id: "teams", label: "Teams", description: "Members, roles and Brands", icon: "workspaces", count: teams.length },
  ];

  return (
    <main className="mx-auto w-full max-w-[1440px] space-y-7 p-5 md:p-8 xl:p-10">
      <header className="relative overflow-hidden rounded-3xl border border-blue-100 bg-gradient-to-br from-white via-blue-50/70 to-indigo-50 p-6 shadow-sm md:p-8">
        <div className="absolute -right-20 -top-24 h-64 w-64 rounded-full bg-blue-200/30 blur-3xl" />
        <div className="relative flex flex-col justify-between gap-6 lg:flex-row lg:items-center">
          <div className="flex items-start gap-4">
            <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-blue-600 text-white shadow-lg shadow-blue-200">
              <span className="material-symbols-outlined text-3xl">groups</span>
            </div>
            <div>
              <div className="mb-2 flex flex-wrap items-center gap-2">
                <span className={`rounded-full px-3 py-1 text-xs font-semibold ring-1 ${workspaceRoleStyle(rbac.workspaceRole)}`}>
                  {roleName(rbac.workspaceRole)}
                </span>
                <span className="rounded-full bg-white/80 px-3 py-1 text-xs font-medium text-slate-500 ring-1 ring-slate-200">Two-tier RBAC</span>
              </div>
              <h1 className="text-2xl font-bold tracking-tight text-slate-950 md:text-3xl">Team and members</h1>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600 md:text-base">
                Manage workspace roles, organize personnel by Team, and control Brand access in one place.
              </p>
            </div>
          </div>
          {canViewPerformance && <Link href="/team/performance" className={`${secondaryButton} relative bg-white`}>
            <span className="material-symbols-outlined text-xl">monitoring</span>
            Member Performance
          </Link>}
        </div>
      </header>

      <nav aria-label="Team management sections" className="sticky top-3 z-20 rounded-2xl border border-slate-200 bg-white/95 p-2 shadow-lg shadow-slate-200/40 backdrop-blur">
        <div className={`grid gap-2 ${workspaceHrAdmin ? "sm:grid-cols-2 xl:grid-cols-4" : "sm:grid-cols-3"}`}>
          {navigation.map(item => {
            const active = view === item.id;
            return <button key={item.id} type="button" aria-label={item.label} aria-pressed={active} onClick={() => setView(item.id)} className={`flex items-center gap-3 rounded-xl px-4 py-3 text-left transition ${active ? "bg-blue-600 text-white shadow-md shadow-blue-200" : "text-slate-600 hover:bg-slate-50 hover:text-slate-950"}`}>
              <span className={`material-symbols-outlined flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${active ? "bg-white/15" : "bg-slate-100 text-slate-500"}`}>{item.icon}</span>
              <span className="min-w-0 flex-1"><span className="block truncate text-sm font-semibold">{item.label}</span><span className={`block truncate text-xs ${active ? "text-blue-100" : "text-slate-400"}`}>{item.description}</span></span>
              {item.count !== undefined && <span className={`rounded-full px-2.5 py-1 text-xs font-bold ${active ? "bg-white/20 text-white" : "bg-slate-100 text-slate-600"}`}>{item.count}</span>}
            </button>;
          })}
        </div>
      </nav>

      {view === "overview" && <section aria-label="Team overview" className="space-y-5">
        <div><h2 className="text-xl font-bold text-slate-950">Workspace overview</h2><p className="mt-1 text-sm text-slate-500">Choose a card to open the area you want to manage.</p></div>
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {[
          { label: "Workspace members", value: members.length, icon: "group", color: "bg-blue-50 text-blue-600", target: "members" as const },
          { label: "Workspace managers", value: managerCount, icon: "admin_panel_settings", color: "bg-violet-50 text-violet-600", target: "members" as const },
          ...(workspaceHrAdmin ? [{ label: "Pending invitations", value: pendingInvitations.length, icon: "schedule_send", color: "bg-cyan-50 text-cyan-700", target: "invitations" as const }] : []),
          { label: "Active teams", value: activeTeamCount, icon: "workspaces", color: "bg-emerald-50 text-emerald-600", target: "teams" as const },
          { label: "Assigned brands", value: teams.reduce((sum, team) => sum + (team.brandCount ?? 0), 0), icon: "sell", color: "bg-amber-50 text-amber-600", target: "teams" as const },
        ].map(item => (
          <button type="button" onClick={() => setView(item.target)} key={item.label} className="group relative flex min-h-28 items-center gap-3 rounded-2xl border border-slate-200 bg-white p-4 pr-11 text-left shadow-sm transition hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-md 2xl:gap-4 2xl:p-5 2xl:pr-12">
            <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${item.color}`}>
              <span className="material-symbols-outlined">{item.icon}</span>
            </div>
            <div className="min-w-0 flex-1"><p className="text-2xl font-bold text-slate-950">{loading ? "—" : item.value}</p><p className="mt-0.5 text-sm leading-5 text-slate-500">{item.label}</p></div>
            <span className="material-symbols-outlined absolute right-4 top-4 text-slate-300 transition group-hover:translate-x-1 group-hover:text-blue-500">arrow_forward</span>
          </button>
        ))}
        </div>
      </section>}

      {error && (
        <div role="alert" className="flex flex-wrap items-center gap-3 rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700">
          <span className="material-symbols-outlined">error</span><span className="flex-1">{error}</span>
          <button type="button" className="font-semibold underline" onClick={() => void run(load, "Data reloaded.", false)}>Reload</button>
        </div>
      )}
      {notice && (
        <div role="status" className="flex items-center gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm text-emerald-800">
          <span className="material-symbols-outlined">check_circle</span>{notice}
        </div>
      )}

      {view === "members" && <section className="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-100 p-6 md:p-7">
          <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center">
            <div><h2 className="text-xl font-bold text-slate-950">Workspace members</h2><p className="mt-1 text-sm text-slate-500">Roles here manage organization level, separate from Team roles.</p></div>
            <span className="w-fit rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{filteredMembers.length}/{members.length} members</span>
          </div>

          {workspaceHrAdmin && (
            <form className="mt-6 grid gap-3 rounded-2xl border border-blue-100 bg-blue-50/60 p-4 md:grid-cols-[minmax(220px,1fr)_220px_auto]" onSubmit={event => {
              event.preventDefault();
              void run(async () => {
                await apiClient("/workspace-invitations", { method: "POST", data: { email, workspaceRole: Number(inviteRole), quotaMode: 1 } });
                setEmail("");
              }, "Invitation sent. Member must accept before being added to a Team.", false);
            }}>
              <label className="relative"><span className="sr-only">Member invitation email</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">mail</span><input aria-label="Member invitation email" type="email" required className={`${inputClass} w-full pl-10`} value={email} onChange={event => setEmail(event.target.value)} placeholder="Member email" /></label>
              <select aria-label="Workspace role" className={`${inputClass} w-full`} value={inviteRole} onChange={event => setInviteRole(event.target.value)}><option value="3">{WORKSPACE_ROLE_LABELS.Member}</option>{rbac.workspaceRole === "Owner" && <option value="2">{WORKSPACE_ROLE_LABELS.WorkspaceManager}</option>}</select>
              <button disabled={busy} className={primaryButton}><span className="material-symbols-outlined text-xl">person_add</span>Invite member</button>
            </form>
          )}

          <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_230px]">
            <label className="relative"><span className="sr-only">Search workspace members</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Search workspace members" className={`${inputClass} w-full pl-10`} value={memberSearch} onChange={event => setMemberSearch(event.target.value)} placeholder="Search by name or email..." /></label>
            <select aria-label="Filter workspace role" className={`${inputClass} w-full`} value={memberRoleFilter} onChange={event => setMemberRoleFilter(event.target.value as MemberRoleFilter)}><option value="All">All roles</option><option value="Owner">{WORKSPACE_ROLE_LABELS.Owner}</option><option value="WorkspaceManager">{WORKSPACE_ROLE_LABELS.WorkspaceManager}</option><option value="Member">{WORKSPACE_ROLE_LABELS.Member}</option></select>
          </div>
        </div>

        <div className="divide-y divide-slate-100">
          {loading && <div className="p-8 text-center text-sm text-slate-500">Loading members...</div>}
          {!loading && filteredMembers.map(member => {
            const displayName = member.fullName || member.email || "Member";
            return (
              <article key={member.id} className="flex flex-col gap-4 px-6 py-5 transition hover:bg-slate-50/70 md:flex-row md:items-center md:px-7">
                <div className="flex min-w-0 flex-1 items-center gap-4">
                  <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-100 to-indigo-100 text-sm font-bold text-blue-700 ring-4 ring-white">{initials(member.fullName, member.email)}</div>
                  <div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{displayName}</p><p className="truncate text-sm text-slate-500">{member.email}</p></div>
                </div>
                <span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ring-1 ${workspaceRoleStyle(member.workspaceRole)}`}>{roleName(member.workspaceRole)}</span>
                <div className="flex flex-wrap items-center gap-2 md:justify-end">
                  {rbac.workspaceRole === "Owner" && member.workspaceRole !== "Owner" && (
                    <select aria-label={`Workspace role of ${displayName}`} className={`${inputClass} h-10 min-w-48`} disabled={busy} value={member.workspaceRole === "WorkspaceManager" ? "2" : "3"} onChange={event => void run(() => apiClient(`/workspace-members/${member.id}/role`, { method: "PUT", data: { workspaceRole: Number(event.target.value) } }), "Workspace role updated.")}><option value="3">{WORKSPACE_ROLE_LABELS.Member}</option><option value="2">{WORKSPACE_ROLE_LABELS.WorkspaceManager}</option></select>
                  )}
                  {admin && member.workspaceRole !== "Owner" && (rbac.workspaceRole === "Owner" || member.workspaceRole === "Member") && (
                    <button type="button" title="Remove from workspace" className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-400 transition hover:bg-red-50 hover:text-red-600" disabled={busy} onClick={() => { if (window.confirm(`Remove ${displayName} from workspace and revoke permissions across all Teams?`)) void run(() => apiClient(`/workspace-members/${member.id}`, { method: "DELETE" }), "Member removed from workspace."); }}><span className="material-symbols-outlined">person_remove</span></button>
                  )}
                </div>
              </article>
            );
          })}
          {!loading && filteredMembers.length === 0 && <div className="p-10 text-center"><span className="material-symbols-outlined text-4xl text-slate-300">person_search</span><p className="mt-2 font-semibold text-slate-700">No members found</p><p className="text-sm text-slate-500">Try adjusting your search or role filter.</p></div>}
        </div>
      </section>}

      {view === "invitations" && workspaceHrAdmin && (
        <section className="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
          <div className="flex flex-col justify-between gap-4 border-b border-slate-100 p-6 md:flex-row md:items-center md:p-7">
            <div>
              <div className="flex items-center gap-3">
                <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-cyan-50 text-cyan-700"><span className="material-symbols-outlined">mark_email_unread</span></div>
                <div><h2 className="text-xl font-bold text-slate-950">Pending invitations</h2><p className="mt-1 text-sm text-slate-500">Track unaccepted invitations and cancel unneeded ones.</p></div>
              </div>
            </div>
            <span className="w-fit rounded-full bg-cyan-50 px-3 py-1 text-xs font-semibold text-cyan-700">{pendingInvitations.length} pending</span>
          </div>

          <div className="border-b border-slate-100 px-6 py-4 md:px-7">
            <label className="relative block"><span className="sr-only">Search pending invitations</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Search pending invitations" className={`${inputClass} w-full pl-10`} value={invitationSearch} onChange={event => setInvitationSearch(event.target.value)} placeholder="Search by email or inviter..." /></label>
          </div>

          <div className="divide-y divide-slate-100">
            {filteredInvitations.map(invitation => {
              const canCancel = rbac.workspaceRole === "Owner" || invitation.workspaceRole !== "WorkspaceManager";
              return (
                <article key={invitation.id} className="flex flex-col gap-4 px-6 py-5 transition hover:bg-slate-50/70 lg:flex-row lg:items-center md:px-7">
                  <div className="flex min-w-0 flex-1 items-center gap-4">
                    <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-cyan-50 text-cyan-700"><span className="material-symbols-outlined">outgoing_mail</span></div>
                    <div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{invitation.email}</p><p className="truncate text-xs text-slate-500">Invited by {invitation.invitedByName || "Workspace Manager"}</p></div>
                  </div>
                  <span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ring-1 ${workspaceRoleStyle(invitation.workspaceRole || "Member")}`}>{roleName(invitation.workspaceRole || "Member")}</span>
                  <div className="text-xs leading-5 text-slate-500 lg:min-w-52"><p>Sent: {new Date(invitation.createdAt).toLocaleString("vi-VN")}</p><p>Expires: {new Date(invitation.expiresAt).toLocaleString("vi-VN")}</p></div>
                  {canCancel && <button type="button" aria-label={`Cancel invitation ${invitation.email}`} className="inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-red-100 px-4 text-sm font-semibold text-red-600 transition hover:bg-red-50 disabled:opacity-50" disabled={busy} onClick={() => { if (window.confirm(`Cancel invitation sent to ${invitation.email}?`)) void run(() => apiClient(`/workspace-invitations/${invitation.id}`, { method: "DELETE" }), "Invitation cancelled.", false); }}><span className="material-symbols-outlined text-lg">cancel_schedule_send</span>Cancel invitation</button>}
                </article>
              );
            })}
            {!loading && filteredInvitations.length === 0 && <div className="p-10 text-center"><span className="material-symbols-outlined text-4xl text-slate-300">mark_email_read</span><p className="mt-2 font-semibold text-slate-700">No pending invitations</p><p className="text-sm text-slate-500">New invitations will appear here until accepted, cancelled, or expired.</p></div>}
          </div>
        </section>
      )}

      {view === "teams" && <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm md:p-7">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center"><div><h2 className="text-xl font-bold text-slate-950">Team list</h2><p className="mt-1 text-sm text-slate-500">Select a Team to manage members, roles, and assigned Brands.</p></div><span className="w-fit rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">{filteredTeams.length}/{teams.length} Teams</span></div>

        {admin && (
          <form className="mt-6 grid gap-3 rounded-2xl border border-slate-200 bg-slate-50 p-4 lg:grid-cols-[minmax(180px,0.8fr)_minmax(260px,1.2fr)_auto]" onSubmit={event => {
            event.preventDefault();
            void run(async () => {
              const response = await apiClient("/teams", { method: "POST", data: { name, description: description || null, members: [] } });
              setSelected(response.data.id);
              setName("");
              setDescription("");
            }, "Team created. Proceed to add members and assign Brands.", false);
          }}>
            <input className={`${inputClass} w-full`} aria-label="New Team name" required maxLength={200} value={name} onChange={event => setName(event.target.value)} placeholder="New Team name" />
            <input className={`${inputClass} w-full`} aria-label="New Team description" maxLength={500} value={description} onChange={event => setDescription(event.target.value)} placeholder="Brief description of Team responsibilities" />
            <button disabled={busy} className={primaryButton}><span className="material-symbols-outlined text-xl">add</span>Create Team</button>
          </form>
        )}

        <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_220px]">
          <label className="relative"><span className="sr-only">Search Team</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Search Team" className={`${inputClass} w-full pl-10`} value={teamSearch} onChange={event => setTeamSearch(event.target.value)} placeholder="Search by Team name or description..." /></label>
          <select aria-label="Filter Team status" className={`${inputClass} w-full`} value={teamStatusFilter} onChange={event => setTeamStatusFilter(event.target.value as TeamStatusFilter)}><option value="All">All statuses</option><option value="Active">Active</option><option value="Inactive">Inactive</option></select>
        </div>

        <div className="mt-6 grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
          <div className="space-y-3">
            {filteredTeams.map(team => {
              const active = normalizedStatus(team.status) === "Active";
              const isSelected = team.id === selected;
              return (
                <button key={team.id} type="button" aria-label={team.name} disabled={busy} onClick={() => setSelected(team.id)} className={`w-full rounded-2xl border p-4 text-left transition ${isSelected ? "border-blue-500 bg-blue-50 shadow-sm ring-2 ring-blue-100" : "border-slate-200 bg-white hover:border-blue-200 hover:bg-slate-50"}`}>
                  <div className="flex items-start gap-3"><div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ${isSelected ? "bg-blue-600 text-white" : "bg-slate-100 text-slate-600"}`}><span className="material-symbols-outlined">group_work</span></div><div className="min-w-0 flex-1"><div className="flex items-center justify-between gap-2"><h3 className="truncate font-semibold text-slate-900">{team.name}</h3><span className={`h-2.5 w-2.5 shrink-0 rounded-full ${active ? "bg-emerald-500" : "bg-slate-300"}`} title={active ? "Active" : "Inactive"} /></div><p className="mt-1 line-clamp-2 text-xs leading-5 text-slate-500">{team.description || "No description for this Team yet."}</p><div className="mt-3 flex gap-3 text-xs font-medium text-slate-500"><span className="flex items-center gap-1"><span className="material-symbols-outlined text-base">group</span>{team.memberCount ?? 0}</span><span className="flex items-center gap-1"><span className="material-symbols-outlined text-base">sell</span>{team.brandCount ?? 0}</span></div></div></div>
                </button>
              );
            })}
            {!loading && filteredTeams.length === 0 && <div className="rounded-2xl border border-dashed border-slate-200 p-8 text-center"><span className="material-symbols-outlined text-4xl text-slate-300">search_off</span><p className="mt-2 text-sm font-semibold text-slate-700">No Teams found</p><p className="mt-1 text-xs text-slate-500">Try adjusting your search or status filter.</p></div>}
          </div>

          <div className="min-h-[420px] rounded-2xl border border-slate-200 bg-slate-50/60 p-5 md:p-6">
            {!selected && <div className="flex h-full min-h-[360px] flex-col items-center justify-center text-center"><div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-white text-slate-300 shadow-sm"><span className="material-symbols-outlined text-4xl">group_work</span></div><h3 className="mt-4 font-semibold text-slate-800">Select a Team</h3><p className="mt-1 max-w-sm text-sm text-slate-500">Member details, roles, and assigned Brands for the Team will appear here.</p></div>}
            {selected && loadingDetail && <div className="flex min-h-[360px] items-center justify-center gap-3 text-sm text-slate-500"><span className="material-symbols-outlined animate-spin text-blue-600">progress_activity</span>Loading Team details...</div>}
            {detail && !loadingDetail && (
              <div className="space-y-6">
                <div className="flex flex-col justify-between gap-4 border-b border-slate-200 pb-5 md:flex-row md:items-start"><div><div className="flex items-center gap-2"><h3 className="text-xl font-bold text-slate-950">{detail.name}</h3><span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${normalizedStatus(detail.status) === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-slate-200 text-slate-600"}`}>{normalizedStatus(detail.status) === "Active" ? "Active" : "Inactive"}</span></div><p className="mt-2 text-sm text-slate-500">{detail.description || "No description for this Team yet."}</p></div><div className="flex gap-2"><span className="rounded-xl bg-white px-3 py-2 text-xs font-semibold text-slate-600 shadow-sm">{activeDetailMembers.length} members</span><span className="rounded-xl bg-white px-3 py-2 text-xs font-semibold text-slate-600 shadow-sm">{activeBrands.length} Brands</span></div></div>

                {admin && <form className="grid gap-3 md:grid-cols-[minmax(180px,0.8fr)_minmax(240px,1.2fr)_auto]" onSubmit={event => { event.preventDefault(); const data = new FormData(event.currentTarget); void run(() => apiClient(`/teams/${selected}`, { method: "PUT", data: { name: data.get("name"), description: data.get("description") } }), "Team information updated."); }}><input key={`${detail.id}-${detail.name}`} name="name" aria-label="Edit Team name" required className={`${inputClass} w-full`} defaultValue={detail.name} /><input key={`${detail.id}-${detail.description}`} name="description" aria-label="Edit Team description" className={`${inputClass} w-full`} defaultValue={detail.description ?? ""} placeholder="Team description" /><button className={secondaryButton} disabled={busy}><span className="material-symbols-outlined text-lg">save</span>Save</button></form>}

                <div><div className="mb-3 flex items-center justify-between"><h4 className="font-semibold text-slate-900">Team members</h4><span className="text-xs text-slate-500">Roles apply within this Team</span></div><div className="overflow-hidden rounded-2xl border border-slate-200 bg-white divide-y divide-slate-100">{activeDetailMembers.map(member => <article key={member.userId} className="flex flex-col gap-3 p-4 md:flex-row md:items-center"><div className="flex min-w-0 flex-1 items-center gap-3"><div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-50 text-xs font-bold text-indigo-700">{initials(member.name, member.email)}</div><div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{member.name}</p><p className="truncate text-xs text-slate-500">{member.email}</p></div></div><span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${teamRoleStyle(member.role)}`}>{roleName(member.role)}</span>{canManage && (admin || member.role !== "Manager") && <div className="flex items-center gap-2"><select aria-label={`Team role of ${member.name}`} className={`${inputClass} h-10 min-w-44`} value={member.role} disabled={busy} onChange={event => void run(() => apiClient(`/teams/${selected}/members/${member.userId}`, { method: "PUT", data: { role: event.target.value } }), "Team role updated.")}>{assignableTeamRoles.map(role => <option key={role} value={role}>{roleName(role)}</option>)}</select><button type="button" title="Remove from Team" disabled={busy} className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-400 hover:bg-red-50 hover:text-red-600" onClick={() => void run(() => apiClient(`/teams/${selected}/members/${member.userId}`, { method: "DELETE" }), "Member removed from Team.")}><span className="material-symbols-outlined">person_remove</span></button></div>}</article>)}{activeDetailMembers.length === 0 && <p className="p-6 text-center text-sm text-slate-500">Team has no members.</p>}</div></div>

                {canManage && <form className="grid gap-3 rounded-2xl border border-indigo-100 bg-indigo-50/60 p-4 md:grid-cols-[minmax(220px,1fr)_200px_auto]" onSubmit={event => { event.preventDefault(); void run(async () => { await apiClient(`/teams/${selected}/members`, { method: "POST", data: { userId: user, role: teamRole } }); setUser(""); }, "Member added to Team."); }}><select aria-label="Member to add" className={`${inputClass} w-full`} required value={user} onChange={event => setUser(event.target.value)}><option value="">Select workspace member</option>{availableMembers.map(member => <option key={member.id} value={member.userId}>{member.fullName || member.email || "Member"}</option>)}</select><select aria-label="Role in Team" className={`${inputClass} w-full`} value={teamRole} onChange={event => setTeamRole(event.target.value as TeamRoleV2)}>{assignableTeamRoles.map(role => <option key={role} value={role}>{roleName(role)}</option>)}</select><button className={primaryButton} disabled={busy || availableMembers.length === 0}><span className="material-symbols-outlined text-xl">group_add</span>Add to Team</button></form>}

                <div><h4 className="font-semibold text-slate-900">Assigned Brands</h4><div className="mt-3 flex flex-wrap gap-2">{activeBrands.map(brand => <Link className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:border-blue-200 hover:text-blue-700" href={`/brands/${brand.brandId}`} key={brand.brandId}><span className="material-symbols-outlined text-lg text-blue-500">sell</span>{brand.brandName}</Link>)}{activeBrands.length === 0 && <p className="text-sm text-slate-500">No Brands assigned yet.</p>}</div>{admin && <Link href="/brands" className="mt-3 inline-flex items-center gap-1 text-sm font-semibold text-blue-700 hover:underline"><span className="material-symbols-outlined text-lg">add_link</span>Assign Brands and grant channel access</Link>}</div>

                {admin && <div className="flex items-center justify-between gap-4 border-t border-slate-200 pt-5"><p className="text-xs leading-5 text-slate-500">Deactivating a Team revokes active permissions while preserving activity history.</p><button type="button" className="shrink-0 rounded-xl px-3 py-2 text-sm font-semibold text-red-600 transition hover:bg-red-50" disabled={busy} onClick={() => { if (window.confirm("Deactivate this Team? Team permissions will be revoked, activity history will be retained.")) void run(async () => { await apiClient(`/teams/${selected}`, { method: "DELETE" }); setSelected(""); setDetail(null); }, "Team deactivated.", false); }}>Deactivate Team</button></div>}
              </div>
            )}
          </div>
        </div>
      </section>}
    </main>
  );
}
