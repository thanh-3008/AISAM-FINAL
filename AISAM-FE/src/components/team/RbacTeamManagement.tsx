"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { useRbac, type TeamRoleV2, type WorkspaceRoleV2 } from "@/contexts/RbacContext";
import type { Team, TeamDetail } from "@/services/teamService";

type Member = {
  id: string;
  userId: string;
  fullName?: string;
  email?: string;
  workspaceRole: WorkspaceRoleV2;
};

type MemberRoleFilter = "All" | WorkspaceRoleV2;
type TeamStatusFilter = "All" | "Active" | "Inactive";

const roleName = (role: string) => ({
  Owner: "Chủ workspace",
  WorkspaceManager: "Quản lý workspace",
  Member: "Thành viên",
  Manager: "Quản lý Team",
  ContentCreator: "Người tạo nội dung",
  Viewer: "Người xem",
}[role] ?? role);

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
  const source = name?.trim() || email?.split("@")[0] || "TV";
  return source.split(/\s+/).slice(0, 2).map(part => part[0]?.toUpperCase()).join("");
}

function normalizedStatus(status?: string) {
  return status?.toLowerCase() === "active" ? "Active" : "Inactive";
}

export default function RbacTeamManagement() {
  const rbac = useRbac()!;
  const admin = rbac.actions.includes("team.manage");
  const [teams, setTeams] = useState<Team[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [selected, setSelected] = useState("");
  const [detail, setDetail] = useState<TeamDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState(false);

  const [memberSearch, setMemberSearch] = useState("");
  const [memberRoleFilter, setMemberRoleFilter] = useState<MemberRoleFilter>("All");
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
    const [teamResponse, memberResponse] = await Promise.all([
      apiClient("/teams/manage"),
      apiClient("/workspace-members"),
    ]);
    const nextTeams = Array.isArray(teamResponse?.data?.items) ? teamResponse.data.items as Team[] : [];
    const nextMembers = Array.isArray(memberResponse?.data) ? memberResponse.data as Member[] : [];
    setTeams(nextTeams);
    setMembers(nextMembers);
    setSelected(current => current && nextTeams.some(team => team.id === current)
      ? current
      : nextTeams[0]?.id ?? "");
  }, []);

  useEffect(() => {
    let active = true;
    setLoading(true);
    load()
      .catch(e => { if (active) setError(e instanceof Error ? e.message : "Không tải được dữ liệu Team."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [load]);

  useEffect(() => {
    let active = true;
    setDetail(null);
    if (!selected) return () => { active = false; };
    setLoadingDetail(true);
    apiClient(`/teams/${selected}`)
      .then(response => { if (active) setDetail(response.data); })
      .catch(e => { if (active) setError(e instanceof Error ? e.message : "Không tải được chi tiết Team."); })
      .finally(() => { if (active) setLoadingDetail(false); });
    return () => { active = false; };
  }, [selected]);

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
      setError(e instanceof Error ? e.message : "Không lưu được thay đổi. Tải lại dữ liệu và thử lại.");
    } finally {
      setBusy(false);
    }
  };

  const filteredMembers = useMemo(() => {
    const query = memberSearch.trim().toLocaleLowerCase("vi");
    return members.filter(member => {
      const matchesQuery = !query || `${member.fullName ?? ""} ${member.email ?? ""}`.toLocaleLowerCase("vi").includes(query);
      const matchesRole = memberRoleFilter === "All" || member.workspaceRole === memberRoleFilter;
      return matchesQuery && matchesRole;
    });
  }, [members, memberRoleFilter, memberSearch]);

  const filteredTeams = useMemo(() => {
    const query = teamSearch.trim().toLocaleLowerCase("vi");
    return teams.filter(team => {
      const matchesQuery = !query || `${team.name} ${team.description ?? ""}`.toLocaleLowerCase("vi").includes(query);
      const status = normalizedStatus(team.status);
      return matchesQuery && (teamStatusFilter === "All" || status === teamStatusFilter);
    });
  }, [teamSearch, teamStatusFilter, teams]);

  const selectedTeam = teams.find(team => team.id === selected);
  const managerCount = members.filter(member => member.workspaceRole === "WorkspaceManager").length;
  const activeTeamCount = teams.filter(team => normalizedStatus(team.status) === "Active").length;
  const activeDetailMembers = (detail?.members ?? []).filter(member => member.isActive);
  const activeBrands = (detail?.brands ?? []).filter(brand => brand.isActive);
  const availableMembers = members.filter(member => !(detail?.members ?? []).some(teamMember => teamMember.userId === member.userId && teamMember.isActive));

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
                <span className="rounded-full bg-white/80 px-3 py-1 text-xs font-medium text-slate-500 ring-1 ring-slate-200">RBAC hai tầng</span>
              </div>
              <h1 className="text-2xl font-bold tracking-tight text-slate-950 md:text-3xl">Team và thành viên</h1>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600 md:text-base">
                Quản lý vai trò workspace, tổ chức nhân sự theo Team và kiểm soát phạm vi Brand tại một nơi.
              </p>
            </div>
          </div>
          <Link href="/team/performance" className={`${secondaryButton} relative bg-white`}>
            <span className="material-symbols-outlined text-xl">monitoring</span>
            Hiệu suất thành viên
          </Link>
        </div>
      </header>

      <section aria-label="Tổng quan Team" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[
          { label: "Thành viên workspace", value: members.length, icon: "group", color: "bg-blue-50 text-blue-600" },
          { label: "Quản lý workspace", value: managerCount, icon: "admin_panel_settings", color: "bg-violet-50 text-violet-600" },
          { label: "Team hoạt động", value: activeTeamCount, icon: "workspaces", color: "bg-emerald-50 text-emerald-600" },
          { label: "Brand đã gán", value: teams.reduce((sum, team) => sum + (team.brandCount ?? 0), 0), icon: "sell", color: "bg-amber-50 text-amber-600" },
        ].map(item => (
          <article key={item.label} className="flex items-center gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${item.color}`}>
              <span className="material-symbols-outlined">{item.icon}</span>
            </div>
            <div><p className="text-2xl font-bold text-slate-950">{loading ? "—" : item.value}</p><p className="text-sm text-slate-500">{item.label}</p></div>
          </article>
        ))}
      </section>

      {error && (
        <div role="alert" className="flex flex-wrap items-center gap-3 rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700">
          <span className="material-symbols-outlined">error</span><span className="flex-1">{error}</span>
          <button type="button" className="font-semibold underline" onClick={() => void run(load, "Đã tải lại dữ liệu.", false)}>Tải lại</button>
        </div>
      )}
      {notice && (
        <div role="status" className="flex items-center gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm text-emerald-800">
          <span className="material-symbols-outlined">check_circle</span>{notice}
        </div>
      )}

      <section className="overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-100 p-6 md:p-7">
          <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center">
            <div><h2 className="text-xl font-bold text-slate-950">Thành viên workspace</h2><p className="mt-1 text-sm text-slate-500">Vai trò ở đây quản lý cấp tổ chức, tách biệt với vai trò trong Team.</p></div>
            <span className="w-fit rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{filteredMembers.length}/{members.length} thành viên</span>
          </div>

          {admin && (
            <form className="mt-6 grid gap-3 rounded-2xl border border-blue-100 bg-blue-50/60 p-4 md:grid-cols-[minmax(220px,1fr)_220px_auto]" onSubmit={event => {
              event.preventDefault();
              void run(async () => {
                await apiClient("/workspace-invitations", { method: "POST", data: { email, workspaceRole: Number(inviteRole), quotaMode: 1 } });
                setEmail("");
              }, "Đã gửi lời mời. Thành viên cần chấp nhận trước khi được thêm vào Team.", false);
            }}>
              <label className="relative"><span className="sr-only">Email mời thành viên</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">mail</span><input aria-label="Email mời thành viên" type="email" required className={`${inputClass} w-full pl-10`} value={email} onChange={event => setEmail(event.target.value)} placeholder="Email thành viên" /></label>
              <select aria-label="Vai trò workspace" className={`${inputClass} w-full`} value={inviteRole} onChange={event => setInviteRole(event.target.value)}><option value="3">Thành viên</option>{rbac.workspaceRole === "Owner" && <option value="2">Quản lý workspace</option>}</select>
              <button disabled={busy} className={primaryButton}><span className="material-symbols-outlined text-xl">person_add</span>Mời thành viên</button>
            </form>
          )}

          <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_230px]">
            <label className="relative"><span className="sr-only">Tìm thành viên workspace</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Tìm thành viên workspace" className={`${inputClass} w-full pl-10`} value={memberSearch} onChange={event => setMemberSearch(event.target.value)} placeholder="Tìm theo tên hoặc email..." /></label>
            <select aria-label="Lọc vai trò workspace" className={`${inputClass} w-full`} value={memberRoleFilter} onChange={event => setMemberRoleFilter(event.target.value as MemberRoleFilter)}><option value="All">Tất cả vai trò</option><option value="Owner">Chủ workspace</option><option value="WorkspaceManager">Quản lý workspace</option><option value="Member">Thành viên</option></select>
          </div>
        </div>

        <div className="divide-y divide-slate-100">
          {loading && <div className="p-8 text-center text-sm text-slate-500">Đang tải thành viên...</div>}
          {!loading && filteredMembers.map(member => {
            const displayName = member.fullName || member.email || "Thành viên";
            return (
              <article key={member.id} className="flex flex-col gap-4 px-6 py-5 transition hover:bg-slate-50/70 md:flex-row md:items-center md:px-7">
                <div className="flex min-w-0 flex-1 items-center gap-4">
                  <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-blue-100 to-indigo-100 text-sm font-bold text-blue-700 ring-4 ring-white">{initials(member.fullName, member.email)}</div>
                  <div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{displayName}</p><p className="truncate text-sm text-slate-500">{member.email}</p></div>
                </div>
                <span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ring-1 ${workspaceRoleStyle(member.workspaceRole)}`}>{roleName(member.workspaceRole)}</span>
                <div className="flex flex-wrap items-center gap-2 md:justify-end">
                  {rbac.workspaceRole === "Owner" && member.workspaceRole !== "Owner" && (
                    <select aria-label={`Vai trò workspace của ${displayName}`} className={`${inputClass} h-10 min-w-48`} disabled={busy} value={member.workspaceRole === "WorkspaceManager" ? "2" : "3"} onChange={event => void run(() => apiClient(`/workspace-members/${member.id}/role`, { method: "PUT", data: { workspaceRole: Number(event.target.value) } }), "Đã cập nhật vai trò workspace.")}><option value="3">Thành viên</option><option value="2">Quản lý workspace</option></select>
                  )}
                  {admin && member.workspaceRole !== "Owner" && (rbac.workspaceRole === "Owner" || member.workspaceRole === "Member") && (
                    <button type="button" title="Gỡ khỏi workspace" className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-400 transition hover:bg-red-50 hover:text-red-600" disabled={busy} onClick={() => { if (window.confirm(`Gỡ ${displayName} khỏi workspace và thu hồi quyền trong các Team?`)) void run(() => apiClient(`/workspace-members/${member.id}`, { method: "DELETE" }), "Đã gỡ thành viên khỏi workspace."); }}><span className="material-symbols-outlined">person_remove</span></button>
                  )}
                </div>
              </article>
            );
          })}
          {!loading && filteredMembers.length === 0 && <div className="p-10 text-center"><span className="material-symbols-outlined text-4xl text-slate-300">person_search</span><p className="mt-2 font-semibold text-slate-700">Không tìm thấy thành viên</p><p className="text-sm text-slate-500">Thử đổi từ khóa hoặc bộ lọc vai trò.</p></div>}
        </div>
      </section>

      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm md:p-7">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center"><div><h2 className="text-xl font-bold text-slate-950">Danh sách Team</h2><p className="mt-1 text-sm text-slate-500">Chọn một Team để quản lý thành viên, vai trò và Brand được cấp.</p></div><span className="w-fit rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">{filteredTeams.length}/{teams.length} Team</span></div>

        {admin && (
          <form className="mt-6 grid gap-3 rounded-2xl border border-slate-200 bg-slate-50 p-4 lg:grid-cols-[minmax(180px,0.8fr)_minmax(260px,1.2fr)_auto]" onSubmit={event => {
            event.preventDefault();
            void run(async () => {
              const response = await apiClient("/teams", { method: "POST", data: { name, description: description || null, members: [] } });
              setSelected(response.data.id);
              setName("");
              setDescription("");
            }, "Đã tạo Team. Tiếp tục thêm thành viên và gán Brand.", false);
          }}>
            <input className={`${inputClass} w-full`} aria-label="Tên Team mới" required maxLength={200} value={name} onChange={event => setName(event.target.value)} placeholder="Tên Team mới" />
            <input className={`${inputClass} w-full`} aria-label="Mô tả Team mới" maxLength={500} value={description} onChange={event => setDescription(event.target.value)} placeholder="Mô tả ngắn về nhiệm vụ của Team" />
            <button disabled={busy} className={primaryButton}><span className="material-symbols-outlined text-xl">add</span>Tạo Team</button>
          </form>
        )}

        <div className="mt-5 grid gap-3 md:grid-cols-[minmax(240px,1fr)_220px]">
          <label className="relative"><span className="sr-only">Tìm Team</span><span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-xl text-slate-400">search</span><input aria-label="Tìm Team" className={`${inputClass} w-full pl-10`} value={teamSearch} onChange={event => setTeamSearch(event.target.value)} placeholder="Tìm theo tên hoặc mô tả Team..." /></label>
          <select aria-label="Lọc trạng thái Team" className={`${inputClass} w-full`} value={teamStatusFilter} onChange={event => setTeamStatusFilter(event.target.value as TeamStatusFilter)}><option value="All">Tất cả trạng thái</option><option value="Active">Đang hoạt động</option><option value="Inactive">Ngừng hoạt động</option></select>
        </div>

        <div className="mt-6 grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
          <div className="space-y-3">
            {filteredTeams.map(team => {
              const active = normalizedStatus(team.status) === "Active";
              const isSelected = team.id === selected;
              return (
                <button key={team.id} type="button" aria-label={team.name} disabled={busy} onClick={() => setSelected(team.id)} className={`w-full rounded-2xl border p-4 text-left transition ${isSelected ? "border-blue-500 bg-blue-50 shadow-sm ring-2 ring-blue-100" : "border-slate-200 bg-white hover:border-blue-200 hover:bg-slate-50"}`}>
                  <div className="flex items-start gap-3"><div className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ${isSelected ? "bg-blue-600 text-white" : "bg-slate-100 text-slate-600"}`}><span className="material-symbols-outlined">group_work</span></div><div className="min-w-0 flex-1"><div className="flex items-center justify-between gap-2"><h3 className="truncate font-semibold text-slate-900">{team.name}</h3><span className={`h-2.5 w-2.5 shrink-0 rounded-full ${active ? "bg-emerald-500" : "bg-slate-300"}`} title={active ? "Đang hoạt động" : "Ngừng hoạt động"} /></div><p className="mt-1 line-clamp-2 text-xs leading-5 text-slate-500">{team.description || "Chưa có mô tả cho Team này."}</p><div className="mt-3 flex gap-3 text-xs font-medium text-slate-500"><span className="flex items-center gap-1"><span className="material-symbols-outlined text-base">group</span>{team.memberCount ?? 0}</span><span className="flex items-center gap-1"><span className="material-symbols-outlined text-base">sell</span>{team.brandCount ?? 0}</span></div></div></div>
                </button>
              );
            })}
            {!loading && filteredTeams.length === 0 && <div className="rounded-2xl border border-dashed border-slate-200 p-8 text-center"><span className="material-symbols-outlined text-4xl text-slate-300">search_off</span><p className="mt-2 text-sm font-semibold text-slate-700">Không tìm thấy Team</p><p className="mt-1 text-xs text-slate-500">Thử đổi từ khóa hoặc trạng thái.</p></div>}
          </div>

          <div className="min-h-[420px] rounded-2xl border border-slate-200 bg-slate-50/60 p-5 md:p-6">
            {!selected && <div className="flex h-full min-h-[360px] flex-col items-center justify-center text-center"><div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-white text-slate-300 shadow-sm"><span className="material-symbols-outlined text-4xl">group_work</span></div><h3 className="mt-4 font-semibold text-slate-800">Chọn một Team</h3><p className="mt-1 max-w-sm text-sm text-slate-500">Chi tiết thành viên, vai trò và Brand của Team sẽ hiển thị tại đây.</p></div>}
            {selected && loadingDetail && <div className="flex min-h-[360px] items-center justify-center gap-3 text-sm text-slate-500"><span className="material-symbols-outlined animate-spin text-blue-600">progress_activity</span>Đang tải chi tiết Team...</div>}
            {detail && !loadingDetail && (
              <div className="space-y-6">
                <div className="flex flex-col justify-between gap-4 border-b border-slate-200 pb-5 md:flex-row md:items-start"><div><div className="flex items-center gap-2"><h3 className="text-xl font-bold text-slate-950">{detail.name}</h3><span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${normalizedStatus(detail.status) === "Active" ? "bg-emerald-100 text-emerald-700" : "bg-slate-200 text-slate-600"}`}>{normalizedStatus(detail.status) === "Active" ? "Đang hoạt động" : "Ngừng hoạt động"}</span></div><p className="mt-2 text-sm text-slate-500">{detail.description || "Chưa có mô tả cho Team này."}</p></div><div className="flex gap-2"><span className="rounded-xl bg-white px-3 py-2 text-xs font-semibold text-slate-600 shadow-sm">{activeDetailMembers.length} thành viên</span><span className="rounded-xl bg-white px-3 py-2 text-xs font-semibold text-slate-600 shadow-sm">{activeBrands.length} Brand</span></div></div>

                {admin && <form className="grid gap-3 md:grid-cols-[minmax(180px,0.8fr)_minmax(240px,1.2fr)_auto]" onSubmit={event => { event.preventDefault(); const data = new FormData(event.currentTarget); void run(() => apiClient(`/teams/${selected}`, { method: "PUT", data: { name: data.get("name"), description: data.get("description") } }), "Đã cập nhật thông tin Team."); }}><input key={`${detail.id}-${detail.name}`} name="name" aria-label="Sửa tên Team" required className={`${inputClass} w-full`} defaultValue={detail.name} /><input key={`${detail.id}-${detail.description}`} name="description" aria-label="Sửa mô tả Team" className={`${inputClass} w-full`} defaultValue={detail.description ?? ""} placeholder="Mô tả Team" /><button className={secondaryButton} disabled={busy}><span className="material-symbols-outlined text-lg">save</span>Lưu</button></form>}

                <div><div className="mb-3 flex items-center justify-between"><h4 className="font-semibold text-slate-900">Thành viên trong Team</h4><span className="text-xs text-slate-500">Vai trò áp dụng trong Team này</span></div><div className="overflow-hidden rounded-2xl border border-slate-200 bg-white divide-y divide-slate-100">{activeDetailMembers.map(member => <article key={member.userId} className="flex flex-col gap-3 p-4 md:flex-row md:items-center"><div className="flex min-w-0 flex-1 items-center gap-3"><div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-50 text-xs font-bold text-indigo-700">{initials(member.name, member.email)}</div><div className="min-w-0"><p className="truncate text-sm font-semibold text-slate-900">{member.name}</p><p className="truncate text-xs text-slate-500">{member.email}</p></div></div><span className={`w-fit rounded-full px-3 py-1 text-xs font-semibold ${teamRoleStyle(member.role)}`}>{roleName(member.role)}</span>{canManage && (admin || member.role !== "Manager") && <div className="flex items-center gap-2"><select aria-label={`Vai trò Team của ${member.name}`} className={`${inputClass} h-10 min-w-44`} value={member.role} disabled={busy} onChange={event => void run(() => apiClient(`/teams/${selected}/members/${member.userId}`, { method: "PUT", data: { role: event.target.value } }), "Đã cập nhật vai trò Team.")}>{assignableTeamRoles.map(role => <option key={role} value={role}>{roleName(role)}</option>)}</select><button type="button" title="Gỡ khỏi Team" disabled={busy} className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-400 hover:bg-red-50 hover:text-red-600" onClick={() => void run(() => apiClient(`/teams/${selected}/members/${member.userId}`, { method: "DELETE" }), "Đã gỡ thành viên khỏi Team.")}><span className="material-symbols-outlined">person_remove</span></button></div>}</article>)}{activeDetailMembers.length === 0 && <p className="p-6 text-center text-sm text-slate-500">Team chưa có thành viên.</p>}</div></div>

                {canManage && <form className="grid gap-3 rounded-2xl border border-indigo-100 bg-indigo-50/60 p-4 md:grid-cols-[minmax(220px,1fr)_200px_auto]" onSubmit={event => { event.preventDefault(); void run(async () => { await apiClient(`/teams/${selected}/members`, { method: "POST", data: { userId: user, role: teamRole } }); setUser(""); }, "Đã thêm thành viên vào Team."); }}><select aria-label="Thành viên cần thêm" className={`${inputClass} w-full`} required value={user} onChange={event => setUser(event.target.value)}><option value="">Chọn thành viên workspace</option>{availableMembers.map(member => <option key={member.id} value={member.userId}>{member.fullName || member.email || "Thành viên"}</option>)}</select><select aria-label="Vai trò trong Team" className={`${inputClass} w-full`} value={teamRole} onChange={event => setTeamRole(event.target.value as TeamRoleV2)}>{assignableTeamRoles.map(role => <option key={role} value={role}>{roleName(role)}</option>)}</select><button className={primaryButton} disabled={busy || availableMembers.length === 0}><span className="material-symbols-outlined text-xl">group_add</span>Thêm vào Team</button></form>}

                <div><h4 className="font-semibold text-slate-900">Brand được cấp</h4><div className="mt-3 flex flex-wrap gap-2">{activeBrands.map(brand => <Link className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:border-blue-200 hover:text-blue-700" href={`/brands/${brand.brandId}`} key={brand.brandId}><span className="material-symbols-outlined text-lg text-blue-500">sell</span>{brand.brandName}</Link>)}{activeBrands.length === 0 && <p className="text-sm text-slate-500">Chưa có Brand nào được gán.</p>}</div>{admin && <Link href="/brands" className="mt-3 inline-flex items-center gap-1 text-sm font-semibold text-blue-700 hover:underline"><span className="material-symbols-outlined text-lg">add_link</span>Gán Brand và cấp quyền kênh</Link>}</div>

                {admin && <div className="flex items-center justify-between gap-4 border-t border-slate-200 pt-5"><p className="text-xs leading-5 text-slate-500">Ngừng Team sẽ thu hồi quyền đang có nhưng vẫn giữ lịch sử hoạt động.</p><button type="button" className="shrink-0 rounded-xl px-3 py-2 text-sm font-semibold text-red-600 transition hover:bg-red-50" disabled={busy} onClick={() => { if (window.confirm("Ngừng hoạt động Team này? Quyền Team sẽ bị thu hồi, lịch sử được giữ lại.")) void run(async () => { await apiClient(`/teams/${selected}`, { method: "DELETE" }); setSelected(""); setDetail(null); }, "Đã ngừng hoạt động Team.", false); }}>Ngừng Team</button></div>}
              </div>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
