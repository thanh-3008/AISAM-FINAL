"use client";
import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { changeAssignment, readAssignments, type AssignmentSnapshot, Kind, Permission } from "@/services/permissionService";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";
import { useResourcePermissions } from "@/hooks/useResourcePermissions";

export default function BrandAccessPage() {
  const { id } = useParams<{ id: string }>();
  const allowed = useResourcePermissions([{ kind: Kind.Brand, resourceId: id, permission: Permission.BrandManage }]);
  const canManage = allowed(0);
  const [snapshot, setSnapshot] = useState<AssignmentSnapshot | null>(null);
  const [teams, setTeams] = useState<{ id: string; name: string }[]>([]);
  const [channels, setChannels] = useState<SocialIntegration[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const load = useCallback(async () => {
    setBusy(true); setError(""); setSnapshot(null);
    try {
      const [access, integrations] = await Promise.all([readAssignments(id), fetchSocialIntegrations(id)]);
      const allTeams: { id: string; name: string }[] = [];
      for (let page = 1; ; page++) {
        const result = await apiClient(`/teams?page=${page}&pageSize=100`);
        allTeams.push(...result.data.items);
        if (!result.data.items.length || allTeams.length >= result.data.totalCount) break;
      }
      setTeams(allTeams); setChannels(integrations); setSnapshot(access);
    } catch (e) { setError(e instanceof Error ? e.message : "Không tải được quyền."); }
    finally { setBusy(false); }
  }, [id]);
  useEffect(() => { if (canManage) void load(); }, [canManage, load]);
  async function save(teamId: string, active: boolean, channel?: { id: string; canView: boolean; canPublish: boolean; canManage: boolean }) {
    if (!snapshot || busy) return;
    setBusy(true); setError("");
    try { setSnapshot(await changeAssignment(id, teamId, snapshot.revision, active, channel)); }
    catch (e) {
      setError(e instanceof Error ? e.message : "Không lưu được quyền.");
      // Do not replay a stale grant. Require an explicit reload on conflict.
      if ((e as { status?: number }).status === 409) setSnapshot(null);
    } finally { setBusy(false); }
  }
  return <main className="p-6 space-y-5 max-w-5xl">
    <Link href={`/brands/${id}`} className="underline">Quay lại Brand</Link>
    <h1 className="text-2xl font-bold">Quyền truy cập Brand</h1>
    <p>Danh sách chỉ gồm Team và kênh trong phạm vi của bạn. Quyền kênh cần quyền xem; cấp quyền đăng không tự cấp quyền duyệt nội dung.</p>
    {!canManage && <p role="status">Quyền quản lý chưa được xác nhận hoặc bạn không được quản lý Brand này.</p>}
    {error && <p role="alert" className="text-red-600">{error}</p>}
    {canManage && <button disabled={busy} onClick={load} className="border rounded px-4 py-2">{busy ? "Đang xử lý…" : "Tải lại quyền"}</button>}
    {snapshot && teams.map(team => {
      const assignment = snapshot.teams.find(t => t.teamId === team.id);
      return <section key={team.id} className="border rounded-xl p-4 space-y-3">
        <h2 className="font-semibold">{team.name}</h2>
        <label className="flex gap-2"><input type="checkbox" disabled={busy} checked={assignment?.isActive ?? false}
          onChange={e => save(team.id, e.target.checked)} />Được truy cập Brand</label>
        {assignment?.isActive && channels.map(channel => {
          const grant = snapshot.channels.find(c => c.teamBrandId === assignment.id && c.integrationId === channel.id);
          const flags = { id: channel.id, canView: grant?.canView ?? false, canPublish: grant?.canPublish ?? false, canManage: grant?.canManage ?? false };
          return <fieldset key={channel.id} disabled={busy} className="p-3 bg-slate-50 rounded flex flex-wrap gap-4">
            <legend>{channel.targetName || channel.accountName} ({channel.provider})</legend>
            {([['canView', 'Xem'], ['canPublish', 'Đăng bài'], ['canManage', 'Quản lý']] as const).map(([key, label]) =>
              <label key={key} className="flex gap-2"><input type="checkbox" checked={flags[key]} disabled={key !== 'canView' && !flags.canView}
                onChange={e => {
                  const next = { ...flags, [key]: e.target.checked };
                  if (!next.canView) { next.canPublish = false; next.canManage = false; }
                  void save(team.id, next.canView, next);
                }} />{label}</label>)}
          </fieldset>;
        })}
      </section>;
    })}
    {snapshot && teams.length === 0 && <p>Không có Team trong phạm vi quản lý.</p>}
  </main>;
}
