"use client";
import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { useRbac, type TeamRoleV2 } from "@/contexts/RbacContext";
import type { Team, TeamDetail } from "@/services/teamService";

type Member = { id: string; userId: string; fullName?: string; email?: string; workspaceRole: string };
const field = "rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm";
const button = "rounded-xl bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-40";
const roleName = (role: string) => ({ Owner: "Chủ workspace", WorkspaceManager: "Quản lý workspace", Member: "Thành viên", Manager: "Quản lý Team", ContentCreator: "Người tạo nội dung", Viewer: "Người xem" }[role] ?? role);

export default function RbacTeamManagement() {
  const rbac = useRbac()!;
  const admin = rbac.actions.includes("team.manage");
  const [teams, setTeams] = useState<Team[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [selected, setSelected] = useState("");
  const [detail, setDetail] = useState<TeamDetail | null>(null);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState(false);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [inviteRole, setInviteRole] = useState("3");
  const [user, setUser] = useState("");
  const [teamRole, setTeamRole] = useState<TeamRoleV2>("ContentCreator");
  const canManage = admin || rbac.teams.some(t => t.teamId === selected && t.role === "Manager");
  const roles: TeamRoleV2[] = admin ? ["Manager", "ContentCreator", "Viewer"] : ["ContentCreator", "Viewer"];
  const load = useCallback(async () => {
    const [t, m] = await Promise.all([apiClient("/teams/manage"), apiClient("/workspace-members")]);
    setTeams(t.data.items); setMembers(m.data);
  }, []);
  useEffect(() => { let active = true; load().catch(e => { if (active) setError(e.message); }); return () => { active = false; }; }, [load]);
  useEffect(() => {
    let active = true; setDetail(null);
    if (selected) apiClient(`/teams/${selected}`).then(r => { if (active) setDetail(r.data); }).catch(e => { if (active) setError(e.message); });
    return () => { active = false; };
  }, [selected]);
  const run = async (action: () => Promise<unknown>, message: string, refreshDetail = true) => {
    setBusy(true); setError(""); setNotice("");
    try {
      await action(); await load();
      if (selected && refreshDetail) setDetail((await apiClient(`/teams/${selected}`)).data);
      setNotice(message);
    } catch (e) { setError(e instanceof Error ? e.message : "Không lưu được thay đổi. Tải lại dữ liệu và thử lại."); }
    finally { setBusy(false); }
  };
  return <main className="mx-auto w-full max-w-6xl space-y-6 p-6 md:p-10">
    <header className="flex flex-wrap items-center justify-between gap-4"><div><p className="text-sm text-slate-500">{roleName(rbac.workspaceRole)}</p><h1 className="text-2xl font-bold">Team và thành viên</h1><p className="mt-2 text-sm text-slate-600">Vai trò workspace quản lý tổ chức. Vai trò Team quyết định quyền làm nội dung.</p></div><Link href="/team/performance" className={field}>Hiệu suất thành viên</Link></header>
    {error && <div role="alert" className="rounded-xl bg-red-50 p-4 text-red-700">{error}<button className="ml-4 underline" onClick={() => void run(load, "Đã tải lại dữ liệu.")}>Tải lại</button></div>}
    {notice && <p role="status" className="rounded-xl bg-green-50 p-4 text-green-800">{notice}</p>}
    <section className="rounded-2xl border border-slate-200 bg-white p-6"><h2 className="mb-4 text-lg font-semibold">Thành viên workspace</h2>
      {admin && <form className="mb-5 flex flex-wrap gap-3" onSubmit={e => { e.preventDefault(); void run(() => apiClient("/workspace-invitations", { method: "POST", data: { email, workspaceRole: Number(inviteRole), quotaMode: 1 } }), "Đã gửi lời mời. Thành viên cần chấp nhận trước khi được thêm vào Team."); }}>
        <input aria-label="Email mời thành viên" type="email" required className={field} value={email} onChange={e => setEmail(e.target.value)} placeholder="Email thành viên" />
        <select aria-label="Vai trò workspace" className={field} value={inviteRole} onChange={e => setInviteRole(e.target.value)}><option value="3">Thành viên</option>{rbac.workspaceRole === "Owner" && <option value="2">Quản lý workspace</option>}</select><button disabled={busy} className={button}>Mời vào workspace</button>
      </form>}
      <ul className="divide-y divide-slate-100">{members.map(m => <li key={m.id} className="flex flex-wrap items-center justify-between gap-3 py-3"><div className="text-sm font-medium">{m.fullName || m.email || "Thành viên"}<p className="text-xs font-normal text-slate-500">{roleName(m.workspaceRole)}</p></div>
        {rbac.workspaceRole === "Owner" && m.workspaceRole !== "Owner" && <select aria-label={`Vai trò workspace của ${m.fullName || m.email}`} className={field} disabled={busy} value={m.workspaceRole === "WorkspaceManager" ? "2" : "3"} onChange={e => void run(() => apiClient(`/workspace-members/${m.id}/role`, { method: "PUT", data: { workspaceRole: Number(e.target.value) } }), "Đã cập nhật vai trò workspace.")}><option value="3">Thành viên</option><option value="2">Quản lý workspace</option></select>}
        {admin && m.workspaceRole !== "Owner" && (rbac.workspaceRole === "Owner" || m.workspaceRole === "Member") && <button className="text-sm text-red-700" disabled={busy} onClick={() => { if (window.confirm(`Gỡ ${m.fullName || m.email || "thành viên"} khỏi workspace và thu hồi quyền trong các Team?`)) void run(() => apiClient(`/workspace-members/${m.id}`, { method: "DELETE" }), "Đã gỡ thành viên khỏi workspace."); }}>Gỡ khỏi workspace</button>}
      </li>)}</ul>
    </section>
    <section className="rounded-2xl border border-slate-200 bg-white p-6"><h2 className="mb-4 text-lg font-semibold">Các Team</h2>
      {admin && <form className="mb-5 flex gap-3" onSubmit={e => { e.preventDefault(); void run(async () => { const r = await apiClient("/teams", { method: "POST", data: { name, members: [] } }); setSelected(r.data.id); setName(""); }, "Đã tạo Team. Tiếp tục thêm thành viên và gán Brand.", false); }}><input className={field} aria-label="Tên Team mới" required maxLength={200} value={name} onChange={e => setName(e.target.value)} placeholder="Tên Team mới" /><button disabled={busy} className={button}>Tạo Team</button></form>}
      <div className="flex flex-wrap gap-2">{teams.map(t => <button key={t.id} disabled={busy} onClick={() => setSelected(t.id)} className={`${field} ${selected === t.id ? "border-blue-500 text-blue-700" : ""}`}>{t.name}</button>)}</div>
      {!teams.length && <p className="py-4 text-sm text-slate-500">Chưa có Team trong phạm vi của bạn.</p>}
      {detail && <div className="mt-6 space-y-4 border-t pt-5"><h3 className="font-semibold">{detail.name}</h3>
        {admin && <form className="flex gap-3" onSubmit={e => { e.preventDefault(); const data = new FormData(e.currentTarget); void run(() => apiClient(`/teams/${selected}`, { method: "PUT", data: { name: data.get("name"), description: detail.description } }), "Đã lưu tên Team."); }}><input key={detail.id + detail.name} name="name" aria-label="Sửa tên Team" required className={field} defaultValue={detail.name} /><button className={button} disabled={busy}>Lưu tên</button></form>}
        <ul className="divide-y">{detail.members.filter(m => m.isActive).map(m => <li key={m.userId} className="flex flex-wrap items-center gap-3 py-3"><span className="mr-auto text-sm">{m.name} · {roleName(m.role)}</span>{canManage && (admin || m.role !== "Manager") && <><select aria-label={`Vai trò Team của ${m.name}`} className={field} value={m.role} disabled={busy} onChange={e => void run(() => apiClient(`/teams/${selected}/members/${m.userId}`, { method: "PUT", data: { role: e.target.value } }), "Đã cập nhật vai trò Team.")}>{roles.map(r => <option key={r} value={r}>{roleName(r)}</option>)}</select><button disabled={busy} className="text-sm text-red-700" onClick={() => void run(() => apiClient(`/teams/${selected}/members/${m.userId}`, { method: "DELETE" }), "Đã gỡ thành viên khỏi Team.")}>Gỡ khỏi Team</button></>}</li>)}</ul>
        {canManage && <form className="flex flex-wrap gap-3" onSubmit={e => { e.preventDefault(); void run(() => apiClient(`/teams/${selected}/members`, { method: "POST", data: { userId: user, role: teamRole } }), "Đã thêm thành viên vào Team."); }}><select aria-label="Thành viên cần thêm" className={field} required value={user} onChange={e => setUser(e.target.value)}><option value="">Chọn thành viên workspace</option>{members.filter(m => !detail.members.some(tm => tm.userId === m.userId && tm.isActive)).map(m => <option key={m.id} value={m.userId}>{m.fullName || m.email || "Thành viên"}</option>)}</select><select aria-label="Vai trò trong Team" className={field} value={teamRole} onChange={e => setTeamRole(e.target.value as TeamRoleV2)}>{roles.map(r => <option key={r} value={r}>{roleName(r)}</option>)}</select><button className={button} disabled={busy}>Thêm vào Team</button></form>}
        <h4 className="font-medium">Brand được cấp</h4><div className="flex flex-wrap gap-2">{detail.brands.filter(b => b.isActive).map(b => <Link className={field} href={`/brands/${b.brandId}`} key={b.brandId}>{b.brandName}</Link>)}</div>{admin && <Link href="/brands" className="inline-block text-sm text-blue-700 underline">Chọn Brand để gán Team và cấp kênh</Link>}
        {admin && <div className="border-t pt-4"><button className="text-sm text-red-700" disabled={busy} onClick={() => { if (window.confirm("Ngừng hoạt động Team này? Quyền Team sẽ bị thu hồi, lịch sử được giữ lại.")) void run(async () => { await apiClient(`/teams/${selected}`, { method: "DELETE" }); setSelected(""); setDetail(null); }, "Đã ngừng hoạt động Team.", false); }}>Ngừng hoạt động Team</button></div>}
      </div>}
    </section>
  </main>;
}

