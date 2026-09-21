"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { inclusiveToExclusiveUtc } from "@/lib/dateRanges";

type Option = { id: string; name: string };
type Tab = "creation" | "review" | "publishing" | "outcomes";
type ScopeView = "members" | "teams";
interface Row {
  memberId: string; name: string; teamRoles?: string[];
  contentsCreated: number; creatorPublishedPosts: number; publisherPublishedPosts: number;
  reviewedSubmissions: number; approvalRate?: number | null; turnaroundHours?: number | null;
  completedSchedules: number; pendingSchedules: number; failedSchedules: number;
  onTimeRate?: number | null; failedPublishRate?: number | null;
  postsWithInsights: number; engagement?: number | null; impressions?: number | null;
  reach?: number | null; engagementRate?: number | null; insightsUpdatedAt?: string | null;
}
interface Result {
  items: Row[]; total: number; from: string; to: string; updatedAt: string;
  unattributedContents?: number | null; brands: Option[]; teams: Option[]; members: Option[];
  metricDefinitions: Record<string, string>; canViewAllTeams?: boolean; teamSelectionRequired?: boolean;
  teamSummaries?: TeamSummary[];
}
interface TeamSummary {
  teamId: string; teamName: string; memberCount: number; contentsCreated: number;
  publishedFromTeamContent: number; publishedByTeamMembers: number; reviewsCompleted: number;
  approvalRate?: number | null; averageReviewHours?: number | null; completedSchedules: number;
  pendingSchedules: number; failedSchedules: number; onTimeRate?: number | null;
  failedPublishRate?: number | null; postsWithInsights: number; engagement?: number | null;
  impressions?: number | null; reach?: number | null; engagementRate?: number | null;
}

const today = () => new Date().toISOString().slice(0, 10);
const thirtyDaysAgo = () => new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10);
const metric = (value: number | null | undefined, suffix = "") =>
  typeof value !== "number" || !Number.isFinite(value) ? "Insufficient data" : `${value.toLocaleString("vi-VN")}${suffix}`;
const roleLabel: Record<string, string> = { Manager: "Team Manager", ContentCreator: "Content Creator", Viewer: "Viewer" };

function RoleBadges({ roles = [] }: { roles?: string[] }) {
  return roles.length ? <div className="flex flex-wrap gap-1.5">{roles.map(role =>
    <span key={role} className="rounded-full border border-primary/15 bg-primary/5 px-2 py-1 text-[11px] font-semibold text-primary">
      {roleLabel[role] || role}
    </span>)}</div> : <span className="text-xs text-outline">Workspace scope</span>;
}

function DataTable({ headers, rows }: { headers: string[]; rows: React.ReactNode }) {
  return <div className="overflow-x-auto rounded-2xl border border-outline-variant/40 bg-surface shadow-sm">
    <table className="w-full min-w-[880px] border-collapse text-sm">
      <thead className="bg-surface-container-low"><tr>{headers.map(header =>
        <th key={header} className="border-b border-outline-variant/40 px-5 py-4 text-left text-xs font-semibold text-on-surface-variant">{header}</th>)}</tr></thead>
      <tbody>{rows}</tbody>
    </table>
  </div>;
}

function TeamComparison({ teams, tab }: { teams: TeamSummary[]; tab: Tab }) {
  const cell = "border-b border-outline-variant/30 px-5 py-4 tabular-nums";
  if (!teams.length) return <div className="rounded-2xl border border-dashed border-outline-variant bg-surface px-6 py-16 text-center">
    <span className="material-symbols-outlined mb-3 !text-4xl text-primary/50">groups</span>
    <h2 className="font-semibold">No Team data in this scope</h2>
    <p className="mt-2 text-sm text-on-surface-variant">Try another Brand or date range.</p>
  </div>;
  if (tab === "creation") return <DataTable headers={["Team", "Active members", "Contents created", "Published from Team content", "Approval rate"]} rows={teams.map(row =>
    <tr key={row.teamId}><th className={`${cell} text-left font-semibold`}>{row.teamName}</th><td className={cell}>{row.memberCount}</td><td className={cell}>{row.contentsCreated}</td><td className={cell}>{row.publishedFromTeamContent}</td><td className={cell}>{metric(row.approvalRate, "%")}</td></tr>)} />;
  if (tab === "review") return <DataTable headers={["Team", "Active members", "Reviews completed", "Approval rate", "Average decision time"]} rows={teams.map(row =>
    <tr key={row.teamId}><th className={`${cell} text-left font-semibold`}>{row.teamName}</th><td className={cell}>{row.memberCount}</td><td className={cell}>{row.reviewsCompleted}</td><td className={cell}>{metric(row.approvalRate, "%")}</td><td className={cell}>{metric(row.averageReviewHours, " hrs")}</td></tr>)} />;
  if (tab === "publishing") return <DataTable headers={["Team", "Active members", "Published by Team members", "Pending", "Completed", "Failed", "On time", "Failure rate"]} rows={teams.map(row =>
    <tr key={row.teamId}><th className={`${cell} text-left font-semibold`}>{row.teamName}</th><td className={cell}>{row.memberCount}</td><td className={cell}>{row.publishedByTeamMembers}</td><td className={cell}>{row.pendingSchedules}</td><td className={cell}>{row.completedSchedules}</td><td className={cell}>{row.failedSchedules}</td><td className={cell}>{metric(row.onTimeRate, "%")}</td><td className={cell}>{metric(row.failedPublishRate, "%")}</td></tr>)} />;
  return <DataTable headers={["Team", "Posts with insights", "Engagement", "Impressions", "Reach", "Engagement rate"]} rows={teams.map(row =>
    <tr key={row.teamId}><th className={`${cell} text-left font-semibold`}>{row.teamName}</th><td className={cell}>{row.postsWithInsights}</td><td className={cell}>{metric(row.engagement)}</td><td className={cell}>{metric(row.impressions)}</td><td className={cell}>{metric(row.reach)}</td><td className={cell}>{metric(row.engagementRate, "%")}</td></tr>)} />;
}

export default function MemberPerformancePage() {
  const [from, setFrom] = useState(thirtyDaysAgo);
  const [to, setTo] = useState(today);
  const [brandId, setBrand] = useState("");
  const [teamId, setTeam] = useState("");
  const [memberId, setMember] = useState("");
  const [tab, setTab] = useState<Tab>("creation");
  const [scopeView, setScopeView] = useState<ScopeView>("members");
  const [page, setPage] = useState(1);
  const [refresh, setRefresh] = useState(0);
  const [result, setResult] = useState<Result | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [options, setOptions] = useState<{ brands: Option[]; teams: Option[]; members: Option[] }>({ brands: [], teams: [], members: [] });

  useEffect(() => {
    let active = true;
    setError(""); setLoading(true);
    if (!from || !to || from > to) { setError("Start date must be on or before end date (UTC)."); setLoading(false); return; }
    const query = new URLSearchParams({ from: `${from}T00:00:00Z`, to: inclusiveToExclusiveUtc(to), page: String(page), pageSize: "20" });
    if (brandId) query.set("brandId", brandId);
    if (teamId) query.set("teamId", teamId);
    if (memberId) query.set("memberId", memberId);
    if (scopeView === "teams") query.set("compareTeams", "true");
    apiClient(`/team/member-performance?${query}`).then(response => {
      if (!active) return;
      if (!response?.success || !response.data) throw new Error("Unable to load performance data.");
      const data = response.data as Result;
      setResult(data); setOptions({ brands: data.brands, teams: data.teams, members: data.members });
      if (data.canViewAllTeams !== true && scopeView === "teams") setScopeView("members");
      if (data.teamSelectionRequired && !teamId && data.teams.length) {
        setBrand(""); setMember(""); setPage(1); setTeam(data.teams[0].id);
      }
    }).catch(e => { if (active) setError(e instanceof Error ? e.message : "Unable to load performance data."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [from, to, brandId, teamId, memberId, page, refresh, scopeView]);

  const selectedTeam = options.teams.find(team => team.id === teamId)?.name;
  const comparingTeams = scopeView === "teams" && result?.canViewAllTeams === true;
  const totals = useMemo(() => ({
    content: result?.items.reduce((sum, row) => sum + row.contentsCreated, 0) || 0,
    creatorPosts: result?.items.reduce((sum, row) => sum + row.creatorPublishedPosts, 0) || 0,
    reviews: result?.items.reduce((sum, row) => sum + row.reviewedSubmissions, 0) || 0,
    schedules: result?.items.reduce((sum, row) => sum + row.pendingSchedules + row.completedSchedules + row.failedSchedules, 0) || 0,
  }), [result]);
  const tabs: { id: Tab; label: string; note: string; icon: string }[] = [
    { id: "creation", label: "Content creation", note: "Creator contribution and quality", icon: "edit_document" },
    { id: "review", label: "Review workflow", note: "Decisions made by reviewers", icon: "fact_check" },
    { id: "publishing", label: "Publishing operations", note: "Publisher and scheduler activity", icon: "schedule_send" },
    { id: "outcomes", label: "Content outcomes", note: "Social performance by creator", icon: "monitoring" },
  ];
  const cell = "border-b border-outline-variant/30 px-5 py-4 tabular-nums";

  return <main className="mx-auto w-full max-w-[1600px] space-y-6 px-4 py-6 text-on-surface sm:px-8 lg:py-8">
    <Link href="/team" className="inline-flex items-center gap-2 text-sm font-medium text-on-surface-variant hover:text-primary">
      <span className="material-symbols-outlined text-lg">arrow_back</span> Back to Team
    </Link>

    <header className="rounded-3xl border border-primary/15 bg-gradient-to-br from-primary/10 via-surface to-secondary/10 p-6 shadow-sm sm:p-8">
      <div className="flex flex-wrap items-center justify-between gap-5">
        <div className="flex items-center gap-4"><span className="material-symbols-outlined flex h-14 w-14 items-center justify-center rounded-2xl bg-primary text-white shadow-lg shadow-primary/20 !text-3xl">monitoring</span>
          <div><p className="mb-1 text-xs font-bold uppercase tracking-[0.18em] text-primary">Team insights</p>
            <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">Team performance</h1>
            <p className="mt-2 max-w-3xl text-sm text-on-surface-variant">Role-aware reporting for content creation, review and publishing responsibilities.</p></div>
        </div>
        <span className="rounded-full border border-primary/15 bg-white/80 px-4 py-2 text-xs font-semibold text-primary">{selectedTeam || "All active Teams"}</span>
      </div>
    </header>

    <section className="grid grid-cols-1 items-end gap-4 rounded-2xl border border-outline-variant/40 bg-surface p-5 shadow-sm sm:grid-cols-2 xl:grid-cols-6 [&_label]:text-xs [&_label]:font-semibold [&_label]:text-on-surface-variant">
      <label>From date<input aria-label="From date" type="date" value={from} onChange={e => { setFrom(e.target.value); setPage(1); }} className="mt-2 block w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal outline-none focus:border-primary" /></label>
      <label>To date (inclusive)<input aria-label="To date" type="date" value={to} onChange={e => { setTo(e.target.value); setPage(1); }} className="mt-2 block w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal outline-none focus:border-primary" /></label>
      <label>Team<select aria-label="Team" disabled={comparingTeams} value={teamId} onChange={e => { setTeam(e.target.value); setBrand(""); setMember(""); setPage(1); }} className="mt-2 block w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal disabled:cursor-not-allowed disabled:opacity-50">
        {result?.canViewAllTeams !== false && <option value="">All active Teams</option>}{options.teams.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label>Brand<select aria-label="Brand" value={brandId} onChange={e => { setBrand(e.target.value); setMember(""); setPage(1); }} className="mt-2 block w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal"><option value="">All allowed</option>{options.brands.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label>Member<select aria-label="Member" disabled={comparingTeams} value={memberId} onChange={e => { setMember(e.target.value); setPage(1); }} className="mt-2 block w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal disabled:cursor-not-allowed disabled:opacity-50"><option value="">All Team members</option>{options.members.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <button disabled={loading} onClick={() => setRefresh(n => n + 1)} className="rounded-xl bg-primary px-5 py-3 text-sm font-semibold text-white shadow-sm transition hover:opacity-90 disabled:opacity-50">Reload</button>
    </section>

    {loading && <div role="status" className="rounded-2xl border border-outline-variant/40 bg-surface p-12 text-center text-sm text-on-surface-variant"><span className="mx-auto mb-4 block h-7 w-7 animate-spin rounded-full border-2 border-primary/20 border-t-primary" />Loading Team performanceâ€¦</div>}
    {error && <p role="alert" className="rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700">{error}</p>}

    {result && !result.teamSelectionRequired && <>
      {result.canViewAllTeams === true && <section aria-label="Comparison scope" className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-outline-variant/40 bg-surface p-3 shadow-sm">
        <div><p className="text-sm font-semibold">Report scope</p><p className="text-xs text-on-surface-variant">View individual responsibilities or compare active Teams.</p></div>
        <div className="flex rounded-xl bg-surface-container-low p-1">
          <button aria-pressed={scopeView === "members"} onClick={() => setScopeView("members")} className={`rounded-lg px-4 py-2 text-sm font-semibold transition ${scopeView === "members" ? "bg-primary text-white shadow-sm" : "text-on-surface-variant"}`}>Compare members</button>
          <button aria-pressed={scopeView === "teams"} onClick={() => { setScopeView("teams"); setTeam(""); setMember(""); setPage(1); }} className={`rounded-lg px-4 py-2 text-sm font-semibold transition ${scopeView === "teams" ? "bg-primary text-white shadow-sm" : "text-on-surface-variant"}`}>Compare Teams</button>
        </div>
      </section>}

      {!comparingTeams && <>
        <section aria-label="Performance overview" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
          {[
            ["Members in scope", result.total, "group", "bg-blue-50 text-blue-600"],
            ["Contents created", totals.content, "edit_document", "bg-violet-50 text-violet-600"],
            ["Published from members' content", totals.creatorPosts, "send", "bg-emerald-50 text-emerald-600"],
            ["Reviews completed", totals.reviews, "fact_check", "bg-cyan-50 text-cyan-700"],
            ["Schedules handled", totals.schedules, "event_available", "bg-amber-50 text-amber-700"],
          ].map(([label, value, icon, color]) => <article key={String(label)} className="rounded-2xl border border-outline-variant/40 bg-surface p-5 shadow-sm"><div className="mb-3 flex items-center justify-between"><p className="text-sm font-medium text-on-surface-variant">{label}</p><span className={`material-symbols-outlined flex h-10 w-10 items-center justify-center rounded-xl ${color}`}>{icon}</span></div><p className="text-3xl font-bold tabular-nums">{value}</p></article>)}
        </section>
        <p className="-mt-3 text-xs text-on-surface-variant">The activity totals above reflect the members displayed on the current page; “Members in scope” reflects the full filtered result.</p>
        {result.unattributedContents != null && result.unattributedContents > 0 && <p className="rounded-xl border border-amber-200/60 bg-amber-50 px-4 py-3 text-sm text-amber-800">{result.unattributedContents} contents have no attributed creator and are excluded from individual creator metrics.</p>}
      </>}

      {comparingTeams && <section aria-label="Team comparison overview" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[
          ["Teams compared", result.teamSummaries?.length || 0, "groups", "bg-blue-50 text-blue-600"],
          ["Contents created", result.teamSummaries?.reduce((sum, row) => sum + row.contentsCreated, 0) || 0, "edit_document", "bg-violet-50 text-violet-600"],
          ["Published from Team content", result.teamSummaries?.reduce((sum, row) => sum + row.publishedFromTeamContent, 0) || 0, "send", "bg-emerald-50 text-emerald-600"],
          ["Reviews completed", result.teamSummaries?.reduce((sum, row) => sum + row.reviewsCompleted, 0) || 0, "fact_check", "bg-cyan-50 text-cyan-700"],
        ].map(([label, value, icon, color]) => <article key={String(label)} className="rounded-2xl border border-outline-variant/40 bg-surface p-5 shadow-sm"><div className="mb-3 flex items-center justify-between"><p className="text-sm font-medium text-on-surface-variant">{label}</p><span className={`material-symbols-outlined flex h-10 w-10 items-center justify-center rounded-xl ${color}`}>{icon}</span></div><p className="text-3xl font-bold tabular-nums">{value}</p></article>)}
      </section>}

      <nav aria-label="Performance areas" className="grid gap-2 rounded-2xl border border-outline-variant/40 bg-surface p-2 md:grid-cols-4">{tabs.map(item =>
        <button key={item.id} onClick={() => setTab(item.id)} aria-pressed={tab === item.id} className={`flex items-center gap-3 rounded-xl px-4 py-3 text-left transition ${tab === item.id ? "bg-primary text-white shadow-md shadow-primary/15" : "hover:bg-surface-container-low"}`}>
          <span className="material-symbols-outlined">{item.icon}</span><span><strong className="block text-sm">{item.label}</strong><span className={`block text-[11px] ${tab === item.id ? "text-white/75" : "text-outline"}`}>{item.note}</span></span>
        </button>)}</nav>

      {comparingTeams && <TeamComparison teams={result.teamSummaries || []} tab={tab} />}
      {!comparingTeams && (result.items.length === 0 ? <div className="rounded-2xl border border-dashed border-outline-variant bg-surface px-6 py-16 text-center"><span className="material-symbols-outlined mb-3 !text-4xl text-primary/50">group</span><h2 className="font-semibold">No matching Team activity</h2><p className="mt-2 text-sm text-on-surface-variant">Try another Team, member or date range.</p></div> : <>
        {tab === "creation" && <DataTable headers={["Member", "Team role", "Contents created", "Published from member's content", "Approval rate"]} rows={result.items.map(row => {
          const applicable = row.teamRoles?.includes("ContentCreator") || row.contentsCreated > 0 || row.creatorPublishedPosts > 0 || row.approvalRate != null;
          return <tr key={row.memberId}><th className={`${cell} text-left font-semibold`}>{row.name}</th><td className={cell}><RoleBadges roles={row.teamRoles} /></td><td className={cell}>{applicable ? row.contentsCreated : "Not applicable"}</td><td className={cell}>{applicable ? row.creatorPublishedPosts : "Not applicable"}</td><td className={cell}>{applicable ? metric(row.approvalRate, "%") : "Not applicable"}</td></tr>;
        })} />}
        {tab === "review" && <DataTable headers={["Member", "Team role", "Reviews completed", "Average decision time"]} rows={result.items.map(row => {
          const applicable = row.teamRoles?.includes("Manager") || row.reviewedSubmissions > 0;
          return <tr key={row.memberId}><th className={`${cell} text-left font-semibold`}>{row.name}</th><td className={cell}><RoleBadges roles={row.teamRoles} /></td><td className={cell}>{applicable ? row.reviewedSubmissions : "Not applicable"}</td><td className={cell}>{applicable ? metric(row.turnaroundHours, " hrs") : "Not applicable"}</td></tr>;
        })} />}
        {tab === "publishing" && <DataTable headers={["Member", "Team role", "Published by member", "Pending", "Completed", "Failed", "On time", "Failure rate"]} rows={result.items.map(row => {
          const activity = row.publisherPublishedPosts + row.pendingSchedules + row.completedSchedules + row.failedSchedules;
          const applicable = activity > 0 || row.teamRoles?.some(role => role === "Manager" || role === "ContentCreator");
          return <tr key={row.memberId}><th className={`${cell} text-left font-semibold`}>{row.name}</th><td className={cell}><RoleBadges roles={row.teamRoles} /></td><td className={cell}>{applicable ? row.publisherPublishedPosts : "Not applicable"}</td><td className={cell}>{applicable ? row.pendingSchedules : "Not applicable"}</td><td className={cell}>{applicable ? row.completedSchedules : "Not applicable"}</td><td className={cell}>{applicable ? row.failedSchedules : "Not applicable"}</td><td className={cell}>{applicable ? metric(row.onTimeRate, "%") : "Not applicable"}</td><td className={cell}>{applicable ? metric(row.failedPublishRate, "%") : "Not applicable"}</td></tr>;
        })} />}
        {tab === "outcomes" && <DataTable headers={["Member", "Posts with insights", "Engagement", "Impressions", "Reach", "Engagement rate", "Insights updated"]} rows={result.items.map(row =>
          <tr key={row.memberId}><th className={`${cell} text-left font-semibold`}>{row.name}</th><td className={cell}>{row.postsWithInsights}</td><td className={cell}>{metric(row.engagement)}</td><td className={cell}>{metric(row.impressions)}</td><td className={cell}>{metric(row.reach)}</td><td className={cell}>{metric(row.engagementRate, "%")}</td><td className={cell}>{row.insightsUpdatedAt ? new Date(row.insightsUpdatedAt).toLocaleString("vi-VN") : "Not synced"}</td></tr>)} />}
      </>)}

      <div className="flex items-center justify-between gap-4 text-sm"><p className="text-xs text-on-surface-variant">UTC date range · Updated {new Date(result.updatedAt).toLocaleString("vi-VN")}</p>{!comparingTeams && <div className="flex items-center gap-3"><button className="rounded-xl border border-outline-variant/50 bg-surface px-4 py-2 disabled:opacity-40" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button><span>Page {page}</span><button className="rounded-xl border border-outline-variant/50 bg-surface px-4 py-2 disabled:opacity-40" disabled={page * 20 >= result.total} onClick={() => setPage(p => p + 1)}>Next</button></div>}</div>
      <details className="rounded-2xl border border-outline-variant/40 bg-surface p-5 text-sm text-on-surface-variant [&_summary]:cursor-pointer [&_summary]:font-semibold [&_summary]:text-on-surface"><summary>Formulas and data limitations</summary><p className="py-3">Metrics follow the actor responsible for each action. Missing denominator or timestamp returns “Insufficient data”; a role outside that responsibility shows “Not applicable”.</p><dl>{Object.entries(result.metricDefinitions).map(([key, value]) => <div key={key} className="py-2"><dt className="font-semibold">{key}</dt><dd>{value}</dd></div>)}</dl></details>
    </>}
  </main>;
}
