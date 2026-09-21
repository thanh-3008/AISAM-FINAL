"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { readAssignments, type AssignmentSnapshot } from "@/services/permissionService";
import { fetchSocialIntegrations, type SocialIntegration, type SocialPlatform } from "@/services/socialAccountService";
import { useRbac } from "@/contexts/RbacContext";

type TeamSummary = { id: string; name: string; description?: string | null; status: string; memberCount?: number };
type TeamFilter = "all" | "assigned" | "unassigned";
type DraftTeamAccess = { active: boolean; channelIds: string[] };
type DraftAccess = Record<string, DraftTeamAccess>;

const inputClass = "h-11 rounded-xl border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-50";
const platformMeta: Record<SocialPlatform, { label: string; icon: string; color: string }> = {
  facebook: { label: "Facebook", icon: "facebook", color: "bg-blue-50 text-blue-700" },
  instagram: { label: "Instagram", icon: "photo_camera", color: "bg-pink-50 text-pink-700" },
  tiktok: { label: "TikTok", icon: "music_note", color: "bg-slate-900 text-white" },
};
const buildDraft = (snapshot: AssignmentSnapshot, teams: TeamSummary[]): DraftAccess => Object.fromEntries(teams.map(team => {
  const assignment = snapshot.teams.find(item => item.teamId === team.id && item.isActive);
  const channelIds = assignment ? snapshot.channels.filter(item => item.teamBrandId === assignment.id && item.scopeEnabledV2).map(item => item.integrationId).sort() : [];
  return [team.id, { active: !!assignment, channelIds }];
}));
const cloneDraft = (draft: DraftAccess): DraftAccess => Object.fromEntries(Object.entries(draft).map(([teamId, value]) => [teamId, { active: value.active, channelIds: [...value.channelIds] }]));
const sameTeamAccess = (left?: DraftTeamAccess, right?: DraftTeamAccess) => {
  if (!left || !right) return left === right;
  return left.active === right.active && (!left.active || [...left.channelIds].sort().join(",") === [...right.channelIds].sort().join(","));
};

export default function RbacBrandAccess({ brandId }: { brandId: string }) {
  const rbac = useRbac();
  const [snapshot, setSnapshot] = useState<AssignmentSnapshot | null>(null);
  const [teams, setTeams] = useState<TeamSummary[]>([]);
  const [channels, setChannels] = useState<SocialIntegration[]>([]);
  const [draft, setDraft] = useState<DraftAccess>({});
  const [savedDraft, setSavedDraft] = useState<DraftAccess>({});
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [reload, setReload] = useState(0);
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState<TeamFilter>("all");
  const admin = rbac?.actions.includes("brand.manage");

  useEffect(() => {
    let active = true;
    setSnapshot(null); setTeams([]); setChannels([]); setDraft({}); setSavedDraft({}); setError(""); setLoading(true);
    if (!admin) { setLoading(false); return () => { active = false; }; }
    Promise.all([readAssignments(brandId), apiClient("/teams/manage"), fetchSocialIntegrations(brandId, true)])
      .then(([assignmentSnapshot, teamResponse, integrations]) => {
        if (!active) return;
        const teamItems: TeamSummary[] = Array.isArray(teamResponse?.data?.items)
          ? teamResponse.data.items.filter((team: TeamSummary) => team.status?.toLocaleLowerCase() === "active")
          : [];
        const nextDraft = buildDraft(assignmentSnapshot, teamItems);
        setSnapshot(assignmentSnapshot);
        setTeams(teamItems);
        setChannels(integrations);
        setDraft(nextDraft); setSavedDraft(cloneDraft(nextDraft));
      })
      .catch(errorValue => { if (active) setError(errorValue instanceof Error ? errorValue.message : "Failed to load permission data."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [brandId, admin, reload]);

  const dirtyTeamIds = useMemo(() => teams.filter(team => !sameTeamAccess(draft[team.id], savedDraft[team.id])).map(team => team.id), [draft, savedDraft, teams]);
  const assignedTeamIds = useMemo(() => new Set(teams.filter(team => draft[team.id]?.active).map(team => team.id)), [draft, teams]);
  const filteredTeams = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("vi");
    return teams.filter(team => {
      const assigned = assignedTeamIds.has(team.id);
      return (filter === "all" || (filter === "assigned" ? assigned : !assigned)) &&
        (!query || `${team.name} ${team.description ?? ""}`.toLocaleLowerCase("vi").includes(query));
    });
  }, [assignedTeamIds, filter, search, teams]);

  useEffect(() => {
    const warn = (event: BeforeUnloadEvent) => { if (dirtyTeamIds.length) { event.preventDefault(); event.returnValue = ""; } };
    window.addEventListener("beforeunload", warn);
    return () => window.removeEventListener("beforeunload", warn);
  }, [dirtyTeamIds.length]);

  const changeBrand = (teamId: string, active: boolean) => setDraft(current => ({ ...current,
    [teamId]: { active, channelIds: active ? current[teamId]?.channelIds ?? [] : [] }
  }));
  const changeChannel = (teamId: string, channelId: string, enabled: boolean) => setDraft(current => {
    const selected = new Set(current[teamId]?.channelIds ?? []);
    if (enabled) selected.add(channelId); else selected.delete(channelId);
    return { ...current, [teamId]: { active: true, channelIds: [...selected].sort() } };
  });
  const save = async () => {
    if (!snapshot || busy || dirtyTeamIds.length === 0) return;
    setBusy(true); setError(""); setNotice("");
    try {
      const result = await apiClient(`/brands/${brandId}/access`, { method: "PUT", data: {
        expectedRevision: snapshot.revision,
        teams: dirtyTeamIds.map(teamId => ({ teamId, active: draft[teamId].active, channelIds: draft[teamId].active ? draft[teamId].channelIds : [] }))
      } });
      setSnapshot(result.data);
      const nextDraft = buildDraft(result.data, teams);
      setDraft(nextDraft); setSavedDraft(cloneDraft(nextDraft));
      setNotice(`${dirtyTeamIds.length} Team permission change${dirtyTeamIds.length === 1 ? "" : "s"} saved.`);
      window.dispatchEvent(new Event("aisam-permissions-changed"));
    } catch (errorValue) { setError(errorValue instanceof Error ? errorValue.message : "Failed to save access permissions."); }
    finally { setBusy(false); }
  };

  if (!admin) return <section className="rounded-3xl border border-slate-200 bg-white p-8 text-center shadow-sm"><div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-amber-50 text-amber-600"><span className="material-symbols-outlined text-3xl">lock</span></div><h2 className="mt-4 text-xl font-bold text-slate-950">Permissions are managed by Team</h2><p className="mx-auto mt-2 max-w-xl text-sm leading-6 text-slate-500">Owner or Workspace Manager is responsible for assigning Brands and channels. Your access is determined by your Team role.</p></section>;

  const assignedCount = assignedTeamIds.size;
  const grantedChannelCount = Object.values(draft).reduce((total, team) => total + (team.active ? team.channelIds.length : 0), 0);
  const activeChannelCount = channels.filter(channel => channel.isActive).length;

  return <section className="space-y-6">
    <div className="grid gap-4 sm:grid-cols-3">
      {[
        { label: "Assigned Teams", value: assignedCount, total: teams.length, icon: "groups", color: "bg-blue-50 text-blue-700" },
        { label: "Active Channel Permissions", value: grantedChannelCount, icon: "verified_user", color: "bg-emerald-50 text-emerald-700" },
        { label: "Connected Channels", value: activeChannelCount, total: channels.length, icon: "hub", color: "bg-violet-50 text-violet-700" },
      ].map(item => <article key={item.label} className="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${item.color}`}><span className="material-symbols-outlined">{item.icon}</span></div><div><p className="text-2xl font-bold text-slate-950">{loading ? "—" : item.value}{item.total !== undefined && <span className="text-base font-medium text-slate-400">/{item.total}</span>}</p><p className="text-sm text-slate-500">{item.label}</p></div></article>)}
    </div>

    {error && <div role="alert" className="flex items-center gap-3 rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700"><span className="material-symbols-outlined">error</span><span className="flex-1">{error}</span><button type="button" className="font-semibold underline" onClick={() => setReload(value => value + 1)}>Reload permissions</button></div>}
    {notice && <div role="status" className="flex items-center gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm text-emerald-800"><span className="material-symbols-outlined">check_circle</span>{notice}</div>}
    {dirtyTeamIds.length > 0 && <div className="sticky top-3 z-20 flex flex-col gap-3 rounded-2xl border border-amber-200 bg-amber-50/95 px-5 py-4 shadow-lg backdrop-blur sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-center gap-3 text-sm text-amber-900"><span className="material-symbols-outlined">edit_note</span><span><b>{dirtyTeamIds.length}</b> Team permission change{dirtyTeamIds.length === 1 ? "" : "s"} not saved.</span></div>
      <div className="flex gap-2"><button type="button" disabled={busy} onClick={() => { setDraft(cloneDraft(savedDraft)); setError(""); setNotice(""); }} className="rounded-xl border border-amber-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700 disabled:opacity-50">Discard</button><button type="button" disabled={busy} onClick={() => void save()} className="rounded-xl bg-blue-600 px-5 py-2 text-sm font-semibold text-white shadow-sm disabled:opacity-50">{busy ? "Saving…" : "Save changes"}</button></div>
    </div>}

    <div className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm md:p-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between"><div><h2 className="text-xl font-bold text-slate-950">Team Access Scope</h2><p className="mt-1 text-sm text-slate-500">Enable Brand for Team first, then select social media channels the Team can use.</p></div><div className="flex flex-wrap gap-2 text-xs text-slate-500"><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">1.</b> Grant Brand</span><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">2.</b> Select Channels</span><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">3.</b> Team Role Determines Actions</span></div></div>
      <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_220px]"><label className="relative"><span className="sr-only">Search Teams</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Search Teams" className={`${inputClass} w-full pl-10`} value={search} onChange={event => setSearch(event.target.value)} placeholder="Search by Team name or description..." /></label><select aria-label="Filter Brand access" className={`${inputClass} w-full`} value={filter} onChange={event => setFilter(event.target.value as TeamFilter)}><option value="all">All Teams</option><option value="assigned">Assigned</option><option value="unassigned">Unassigned</option></select></div>
    </div>

    {loading && <div className="grid gap-4"><div className="h-44 animate-pulse rounded-3xl bg-slate-100" /><div className="h-44 animate-pulse rounded-3xl bg-slate-100" /></div>}
    {!loading && filteredTeams.map(team => {
      const teamDraft = draft[team.id] ?? { active: false, channelIds: [] };
      const enabledChannels = teamDraft.active ? teamDraft.channelIds.length : 0;
      return <article key={team.id} className={`overflow-hidden rounded-3xl border bg-white shadow-sm transition ${teamDraft.active ? "border-blue-200 ring-1 ring-blue-50" : "border-slate-200"}`}>
        <div className={`flex flex-col gap-5 p-6 md:flex-row md:items-center md:justify-between ${teamDraft.active ? "bg-gradient-to-r from-blue-50/80 to-white" : "bg-white"}`}>
          <div className="flex min-w-0 items-center gap-4"><div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${teamDraft.active ? "bg-blue-600 text-white shadow-lg shadow-blue-100" : "bg-slate-100 text-slate-500"}`}><span className="material-symbols-outlined">group_work</span></div><div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><h3 className="truncate text-lg font-bold text-slate-950">{team.name}</h3><span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${teamDraft.active ? "bg-blue-100 text-blue-700" : "bg-slate-100 text-slate-500"}`}>{teamDraft.active ? "Granted" : "Not granted"}</span></div><p className="mt-1 line-clamp-1 text-sm text-slate-500">{team.description || `${team.memberCount ?? 0} members · ${enabledChannels} channels allowed`}</p></div></div>
          <label className="flex cursor-pointer items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3 shadow-sm"><span className="text-sm font-semibold text-slate-700">Allow Brand Access</span><span className="relative inline-flex h-6 w-11 items-center"><input aria-label={`Grant Brand to ${team.name}`} type="checkbox" className="peer sr-only" checked={teamDraft.active} disabled={busy || !snapshot} onChange={event => changeBrand(team.id, event.target.checked)} /><span className="absolute inset-0 rounded-full bg-slate-200 transition peer-checked:bg-blue-600 peer-disabled:opacity-50" /><span className="absolute left-1 h-4 w-4 rounded-full bg-white shadow transition peer-checked:translate-x-5" /></span></label>
        </div>
        {teamDraft.active && <div className="border-t border-slate-100 p-6"><div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"><div><h4 className="font-semibold text-slate-900">Channels Team Can Use</h4><p className="mt-1 text-xs text-slate-500">Changes take effect after you save.</p></div><span className="w-fit rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">{enabledChannels}/{channels.length} channels active</span></div>
          <div className="grid gap-3 lg:grid-cols-2">{channels.map(channel => {
            const meta = platformMeta[channel.provider] ?? { label: channel.provider || "Channel", icon: "public", color: "bg-slate-100 text-slate-700" };
            const granted = teamDraft.channelIds.includes(channel.id);
            return <label key={channel.id} className={`flex items-center gap-4 rounded-2xl border p-4 transition ${granted ? "border-emerald-200 bg-emerald-50/40" : "border-slate-200 bg-white hover:border-blue-200"} ${!channel.isActive ? "cursor-not-allowed opacity-60" : "cursor-pointer"}`}><div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${meta.color}`}><span className="material-symbols-outlined text-xl">{meta.icon}</span></div><div className="min-w-0 flex-1"><p className="truncate text-sm font-semibold text-slate-900">{channel.targetName || channel.accountName}</p><p className="text-xs text-slate-500">{meta.label}{!channel.isActive && " · Inactive"}</p></div><input aria-label={channel.accountName} type="checkbox" className="h-5 w-5 rounded border-slate-300 text-blue-600 focus:ring-blue-500" disabled={busy || !channel.isActive} checked={granted} onChange={event => changeChannel(team.id, channel.id, event.target.checked)} /></label>;
          })}</div>
          {channels.length === 0 && <div className="rounded-2xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center"><div className="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-white text-slate-400 shadow-sm"><span className="material-symbols-outlined">link_off</span></div><h5 className="mt-3 font-semibold text-slate-800">No connected channels for this Brand</h5><p className="mt-1 text-sm text-slate-500">Connect social accounts before granting channel access to Teams.</p><Link href="/social" className="mt-4 inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white"><span className="material-symbols-outlined text-lg">add_link</span>Connect Channels</Link></div>}
        </div>}
      </article>;
    })}
    {!loading && filteredTeams.length === 0 && <div className="rounded-3xl border border-dashed border-slate-200 bg-white p-12 text-center"><span className="material-symbols-outlined text-5xl text-slate-300">group_off</span><h3 className="mt-3 font-semibold text-slate-800">No matching Teams found</h3><p className="mt-1 text-sm text-slate-500">Try adjusting your search terms or Brand access filter.</p></div>}
  </section>;
}
