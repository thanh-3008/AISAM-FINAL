"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { readAssignments, type AssignmentSnapshot } from "@/services/permissionService";
import { fetchSocialIntegrations, type SocialIntegration, type SocialPlatform } from "@/services/socialAccountService";
import { useRbac } from "@/contexts/RbacContext";

type TeamSummary = { id: string; name: string; description?: string | null; memberCount?: number };
type TeamFilter = "all" | "assigned" | "unassigned";

const inputClass = "h-11 rounded-xl border border-slate-200 bg-white px-3 text-sm text-slate-800 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-50";
const platformMeta: Record<SocialPlatform, { label: string; icon: string; color: string }> = {
  facebook: { label: "Facebook", icon: "facebook", color: "bg-blue-50 text-blue-700" },
  instagram: { label: "Instagram", icon: "photo_camera", color: "bg-pink-50 text-pink-700" },
  tiktok: { label: "TikTok", icon: "music_note", color: "bg-slate-900 text-white" },
};

export default function RbacBrandAccess({ brandId }: { brandId: string }) {
  const rbac = useRbac();
  const [snapshot, setSnapshot] = useState<AssignmentSnapshot | null>(null);
  const [teams, setTeams] = useState<TeamSummary[]>([]);
  const [channels, setChannels] = useState<SocialIntegration[]>([]);
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
    setSnapshot(null); setTeams([]); setChannels([]); setError(""); setLoading(true);
    if (!admin) { setLoading(false); return () => { active = false; }; }
    Promise.all([readAssignments(brandId), apiClient("/teams/manage"), fetchSocialIntegrations(brandId, true)])
      .then(([assignmentSnapshot, teamResponse, integrations]) => {
        if (!active) return;
        setSnapshot(assignmentSnapshot);
        setTeams(Array.isArray(teamResponse?.data?.items) ? teamResponse.data.items : []);
        setChannels(integrations);
      })
      .catch(errorValue => { if (active) setError(errorValue instanceof Error ? errorValue.message : "Không tải được dữ liệu phân quyền."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [brandId, admin, reload]);

  const assignedTeamIds = useMemo(() => new Set(snapshot?.teams.filter(item => item.isActive).map(item => item.teamId) ?? []), [snapshot]);
  const filteredTeams = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("vi");
    return teams.filter(team => {
      const assigned = assignedTeamIds.has(team.id);
      return (filter === "all" || (filter === "assigned" ? assigned : !assigned)) &&
        (!query || `${team.name} ${team.description ?? ""}`.toLocaleLowerCase("vi").includes(query));
    });
  }, [assignedTeamIds, filter, search, teams]);

  const change = async (teamId: string, enabled: boolean, channelId?: string) => {
    if (!snapshot || busy) return;
    setBusy(true); setError(""); setNotice("");
    try {
      const result = await apiClient(`/brands/${brandId}/${channelId ? `channels/${channelId}/` : ""}teams/${teamId}`, enabled
        ? { method: "PUT", data: { expectedRevision: snapshot.revision } }
        : { method: "DELETE", headers: { "If-Match": snapshot.revision } });
      setSnapshot(result.data);
      setNotice(channelId ? "Đã cập nhật quyền kênh." : enabled ? "Đã cấp Brand cho Team." : "Đã thu hồi Brand khỏi Team.");
      window.dispatchEvent(new Event("aisam-permissions-changed"));
    } catch (errorValue) { setError(errorValue instanceof Error ? errorValue.message : "Không lưu được quyền truy cập."); }
    finally { setBusy(false); }
  };

  if (!admin) return <section className="rounded-3xl border border-slate-200 bg-white p-8 text-center shadow-sm"><div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-amber-50 text-amber-600"><span className="material-symbols-outlined text-3xl">lock</span></div><h2 className="mt-4 text-xl font-bold text-slate-950">Quyền được quản lý theo Team</h2><p className="mx-auto mt-2 max-w-xl text-sm leading-6 text-slate-500">Owner hoặc Quản lý workspace phụ trách gán Brand và kênh. Quyền sử dụng của bạn được xác định bởi vai trò trong Team.</p></section>;

  const assignedCount = assignedTeamIds.size;
  const grantedChannelCount = snapshot?.channels.filter(channel => channel.scopeEnabledV2).length ?? 0;
  const activeChannelCount = channels.filter(channel => channel.isActive).length;

  return <section className="space-y-6">
    <div className="grid gap-4 sm:grid-cols-3">
      {[
        { label: "Team được cấp", value: assignedCount, total: teams.length, icon: "groups", color: "bg-blue-50 text-blue-700" },
        { label: "Quyền kênh đang bật", value: grantedChannelCount, icon: "verified_user", color: "bg-emerald-50 text-emerald-700" },
        { label: "Kênh đã kết nối", value: activeChannelCount, total: channels.length, icon: "hub", color: "bg-violet-50 text-violet-700" },
      ].map(item => <article key={item.label} className="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${item.color}`}><span className="material-symbols-outlined">{item.icon}</span></div><div><p className="text-2xl font-bold text-slate-950">{loading ? "—" : item.value}{item.total !== undefined && <span className="text-base font-medium text-slate-400">/{item.total}</span>}</p><p className="text-sm text-slate-500">{item.label}</p></div></article>)}
    </div>

    {error && <div role="alert" className="flex items-center gap-3 rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700"><span className="material-symbols-outlined">error</span><span className="flex-1">{error}</span><button type="button" className="font-semibold underline" onClick={() => setReload(value => value + 1)}>Tải lại quyền</button></div>}
    {notice && <div role="status" className="flex items-center gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm text-emerald-800"><span className="material-symbols-outlined">check_circle</span>{notice}</div>}

    <div className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm md:p-6">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between"><div><h2 className="text-xl font-bold text-slate-950">Phạm vi truy cập theo Team</h2><p className="mt-1 text-sm text-slate-500">Bật Brand cho Team trước, sau đó chọn các kênh mạng xã hội Team được sử dụng.</p></div><div className="flex flex-wrap gap-2 text-xs text-slate-500"><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">1.</b> Cấp Brand</span><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">2.</b> Chọn kênh</span><span className="rounded-full bg-slate-100 px-3 py-1.5"><b className="text-slate-700">3.</b> Vai trò Team quyết định thao tác</span></div></div>
      <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_220px]"><label className="relative"><span className="sr-only">Tìm Team</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Tìm Team" className={`${inputClass} w-full pl-10`} value={search} onChange={event => setSearch(event.target.value)} placeholder="Tìm theo tên hoặc mô tả Team..." /></label><select aria-label="Lọc quyền Brand" className={`${inputClass} w-full`} value={filter} onChange={event => setFilter(event.target.value as TeamFilter)}><option value="all">Tất cả Team</option><option value="assigned">Đã được cấp</option><option value="unassigned">Chưa được cấp</option></select></div>
    </div>

    {loading && <div className="grid gap-4"><div className="h-44 animate-pulse rounded-3xl bg-slate-100" /><div className="h-44 animate-pulse rounded-3xl bg-slate-100" /></div>}
    {!loading && filteredTeams.map(team => {
      const assignment = snapshot?.teams.find(item => item.teamId === team.id && item.isActive);
      const enabledChannels = assignment ? snapshot?.channels.filter(channel => channel.teamBrandId === assignment.id && channel.scopeEnabledV2).length ?? 0 : 0;
      return <article key={team.id} className={`overflow-hidden rounded-3xl border bg-white shadow-sm transition ${assignment ? "border-blue-200 ring-1 ring-blue-50" : "border-slate-200"}`}>
        <div className={`flex flex-col gap-5 p-6 md:flex-row md:items-center md:justify-between ${assignment ? "bg-gradient-to-r from-blue-50/80 to-white" : "bg-white"}`}>
          <div className="flex min-w-0 items-center gap-4"><div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl ${assignment ? "bg-blue-600 text-white shadow-lg shadow-blue-100" : "bg-slate-100 text-slate-500"}`}><span className="material-symbols-outlined">group_work</span></div><div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><h3 className="truncate text-lg font-bold text-slate-950">{team.name}</h3><span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${assignment ? "bg-blue-100 text-blue-700" : "bg-slate-100 text-slate-500"}`}>{assignment ? "Đã cấp quyền" : "Chưa cấp quyền"}</span></div><p className="mt-1 line-clamp-1 text-sm text-slate-500">{team.description || `${team.memberCount ?? 0} thành viên · ${enabledChannels} kênh được phép`}</p></div></div>
          <label className="flex cursor-pointer items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3 shadow-sm"><span className="text-sm font-semibold text-slate-700">Cho phép dùng Brand</span><span className="relative inline-flex h-6 w-11 items-center"><input aria-label={`Cấp Brand cho ${team.name}`} type="checkbox" className="peer sr-only" checked={!!assignment} disabled={busy || !snapshot} onChange={event => void change(team.id, event.target.checked)} /><span className="absolute inset-0 rounded-full bg-slate-200 transition peer-checked:bg-blue-600 peer-disabled:opacity-50" /><span className="absolute left-1 h-4 w-4 rounded-full bg-white shadow transition peer-checked:translate-x-5" /></span></label>
        </div>
        {assignment && <div className="border-t border-slate-100 p-6"><div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"><div><h4 className="font-semibold text-slate-900">Kênh Team được sử dụng</h4><p className="mt-1 text-xs text-slate-500">Tắt kênh sẽ thu hồi phạm vi truy cập của Team trên kênh đó.</p></div><span className="w-fit rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">{enabledChannels}/{channels.length} kênh đang bật</span></div>
          <div className="grid gap-3 lg:grid-cols-2">{channels.map(channel => {
            const meta = platformMeta[channel.provider] ?? { label: channel.provider || "Kênh", icon: "public", color: "bg-slate-100 text-slate-700" };
            const granted = snapshot?.channels.some(item => item.teamBrandId === assignment.id && item.integrationId === channel.id && item.scopeEnabledV2) ?? false;
            return <label key={channel.id} className={`flex items-center gap-4 rounded-2xl border p-4 transition ${granted ? "border-emerald-200 bg-emerald-50/40" : "border-slate-200 bg-white hover:border-blue-200"} ${!channel.isActive ? "cursor-not-allowed opacity-60" : "cursor-pointer"}`}><div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${meta.color}`}><span className="material-symbols-outlined text-xl">{meta.icon}</span></div><div className="min-w-0 flex-1"><p className="truncate text-sm font-semibold text-slate-900">{channel.targetName || channel.accountName}</p><p className="text-xs text-slate-500">{meta.label}{!channel.isActive && " · Ngừng hoạt động"}</p></div><input aria-label={channel.accountName} type="checkbox" className="h-5 w-5 rounded border-slate-300 text-blue-600 focus:ring-blue-500" disabled={busy || !channel.isActive} checked={granted} onChange={event => void change(team.id, event.target.checked, channel.id)} /></label>;
          })}</div>
          {channels.length === 0 && <div className="rounded-2xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center"><div className="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-white text-slate-400 shadow-sm"><span className="material-symbols-outlined">link_off</span></div><h5 className="mt-3 font-semibold text-slate-800">Brand chưa có kênh kết nối</h5><p className="mt-1 text-sm text-slate-500">Kết nối tài khoản mạng xã hội trước khi cấp quyền kênh cho Team.</p><Link href="/social" className="mt-4 inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white"><span className="material-symbols-outlined text-lg">add_link</span>Kết nối kênh</Link></div>}
        </div>}
      </article>;
    })}
    {!loading && filteredTeams.length === 0 && <div className="rounded-3xl border border-dashed border-slate-200 bg-white p-12 text-center"><span className="material-symbols-outlined text-5xl text-slate-300">group_off</span><h3 className="mt-3 font-semibold text-slate-800">Không tìm thấy Team phù hợp</h3><p className="mt-1 text-sm text-slate-500">Thử thay đổi từ khóa hoặc bộ lọc quyền Brand.</p></div>}
  </section>;
}
