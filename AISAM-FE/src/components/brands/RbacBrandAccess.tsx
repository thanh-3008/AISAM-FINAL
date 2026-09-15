"use client";
import { useEffect, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { readAssignments, type AssignmentSnapshot } from "@/services/permissionService";
import { fetchSocialIntegrations } from "@/services/socialAccountService";
import { useRbac } from "@/contexts/RbacContext";

export default function RbacBrandAccess({ brandId }: { brandId: string }) {
  const rbac = useRbac();
  const [snapshot, setSnapshot] = useState<AssignmentSnapshot | null>(null);
  const [teams, setTeams] = useState<{ id: string; name: string }[]>([]);
  const [channels, setChannels] = useState<{ id: string; accountName: string; isActive: boolean }[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [reload, setReload] = useState(0);
  const admin = rbac?.actions.includes("brand.manage");
  useEffect(() => {
    let active = true;
    setSnapshot(null); setTeams([]); setChannels([]); setError("");
    if (admin) Promise.all([readAssignments(brandId), apiClient("/teams/manage"), fetchSocialIntegrations(brandId, true)])
      .then(([s, t, c]) => { if (active) { setSnapshot(s); setTeams(t.data.items); setChannels(c); } })
      .catch(e => { if (active) setError(e.message); });
    return () => { active = false; };
  }, [brandId, admin, reload]);
  if (!admin) return <p className="text-sm text-slate-500">Quản trị workspace phụ trách cấp Team và kênh. Quyền thao tác của bạn theo vai trò Team.</p>;
  const change = async (teamId: string, enabled: boolean, channelId?: string) => {
    if (!snapshot) return;
    setBusy(true); setError(""); setNotice("");
    try {
      const result = await apiClient(`/brands/${brandId}/${channelId ? `channels/${channelId}/` : ""}teams/${teamId}`, enabled
        ? { method: "PUT", data: { expectedRevision: snapshot.revision } }
        : { method: "DELETE", headers: { "If-Match": snapshot.revision } });
      setSnapshot(result.data); setNotice("Đã lưu quyền truy cập.");
    } catch (e) { setError(e instanceof Error ? e.message : "Không lưu được quyền truy cập."); }
    finally { setBusy(false); }
  };
  return <section className="space-y-4"><h3 className="text-lg font-semibold">Team và kênh được cấp</h3>
    <p className="text-sm text-slate-500">Chọn Team sử dụng Brand, rồi chọn các kênh của Team đó. Vai trò trong Team quyết định thao tác được phép.</p>
    {error && <p role="alert" className="rounded-xl bg-red-50 p-3 text-red-700">{error} <button className="underline" onClick={() => setReload(n => n + 1)}>Tải lại quyền</button></p>}
    {notice && <p role="status" className="text-sm text-green-700">{notice}</p>}
    {teams.map(t => { const link = snapshot?.teams.find(x => x.teamId === t.id && x.isActive); return <div key={t.id} className="rounded-xl border border-slate-200 p-4">
      <label className="flex gap-3 font-medium"><input type="checkbox" checked={!!link} disabled={busy || !snapshot} onChange={e => void change(t.id, e.target.checked)} />{t.name}</label>
      {link && <div className="mt-3 space-y-2 pl-7">{channels.map(c => <label key={c.id} className="flex gap-3 text-sm"><input type="checkbox" disabled={busy || !c.isActive} checked={snapshot?.channels.some(g => g.teamBrandId === link.id && g.integrationId === c.id && g.scopeEnabledV2) ?? false} onChange={e => void change(t.id, e.target.checked, c.id)} />{c.accountName}{!c.isActive && " (ngừng hoạt động)"}</label>)}{!channels.length && <p className="text-sm text-slate-500">Chưa có kênh. Kết nối tài khoản mạng xã hội cho Brand trước.</p>}</div>}
    </div>; })}
  </section>;
}
