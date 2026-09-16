"use client";
import { useEffect, useState } from "react";
import { useRbac } from "@/contexts/RbacContext";
import { apiClient } from "@/lib/apiClient";

export default function TeamScopeSelect({ brandId, value, onChange, disabled = false }: {
  brandId: string; value: string; onChange: (value: string) => void; disabled?: boolean;
}) {
  const rbac = useRbac();
  const [teams, setTeams] = useState<{ id: string; name: string }[]>([]);
  const [error, setError] = useState(false);
  useEffect(() => {
    if (!rbac) return;
    let active = true;
    apiClient("/teams/manage").then(result => { if (active) setTeams(result.data.items); })
      .catch(() => { if (active) setError(true); });
    return () => { active = false; };
  }, [rbac]);
  if (!rbac) return null;
  const allowed = new Set(rbac.scopes.filter(s => s.brandId === brandId &&
    (rbac.workspaceRole !== "Member" || s.role === "Manager" || s.role === "ContentCreator")).map(s => s.teamId));
  const options = teams.filter(t => allowed.has(t.id));
  return <div className="space-y-2">
    <label htmlFor="content-team" className="block text-sm font-semibold">Team phụ trách *</label>
    <select id="content-team" disabled={disabled} value={value} onChange={e => onChange(e.target.value)} className="w-full rounded-xl border border-slate-200 bg-white px-4 py-3">
      <option value="">Chọn Team</option>
      {options.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
    </select>
    <p className="text-sm text-slate-500">Nội dung thuộc Team đã chọn và không thể chuyển Team sau khi tạo.</p>
    {error ? <p role="alert">Không tải được danh sách Team. Vui lòng tải lại trang.</p> : !options.length && <p role="status">Chưa có Team được phép tạo nội dung cho Brand này.</p>}
  </div>;
}
