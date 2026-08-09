"use client";

import { useState, useEffect } from "react";
import { type TeamMember, type MemberRole, type QuotaMode, QUOTA_MODE_LABELS } from "@/services/teamService";

interface EditMemberModalProps {
  member: TeamMember | null;
  onClose: () => void;
  onUpdate: (id: string, role: MemberRole) => void;
  isLoading: boolean;
  canAssignQuota?: boolean;
  onUpdateQuota?: (id: string, mode: QuotaMode, limit: number | null) => void;
  isUpdatingQuota?: boolean;
}

export default function EditMemberModal({ member, onClose, onUpdate, isLoading, canAssignQuota = false, onUpdateQuota, isUpdatingQuota = false }: EditMemberModalProps) {
  const [role, setRole] = useState<MemberRole>("Viewer");
  const [quotaMode, setQuotaMode] = useState<QuotaMode>("SharedPool");
  const [creditLimit, setCreditLimit] = useState<string>("");
  const [quotaChanged, setQuotaChanged] = useState(false);

  useEffect(() => {
    if (member) {
      setRole(member.role);
      setQuotaMode(member.quotaMode || "SharedPool");
      setCreditLimit(member.creditLimit != null ? String(member.creditLimit) : "");
      setQuotaChanged(false);
    }
  }, [member]);

  if (!member) return null;

  const showCreditInput = canAssignQuota && quotaMode !== "SharedPool";
  const parsedCreditLimit = showCreditInput && creditLimit ? Number(creditLimit) : null;

  const handleSubmit = () => {
    onUpdate(member.id, role);
    if (quotaChanged && onUpdateQuota) {
      onUpdateQuota(member.id, quotaMode, showCreditInput && parsedCreditLimit && parsedCreditLimit > 0 ? parsedCreditLimit : null);
    }
  };

  const handleQuotaModeChange = (mode: QuotaMode) => {
    setQuotaMode(mode);
    setQuotaChanged(true);
    if (mode === "SharedPool") setCreditLimit("");
  };

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-hidden flex flex-col" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-secondary/10 text-secondary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">person_edit</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Edit Member</h2>
                <p className="text-label-xs text-outline">{member.name}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>
          <div className="p-6 space-y-5 overflow-y-auto flex-1">
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Email</label>
              <div className="p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-outline">
                {member.email}
              </div>
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Current Status</label>
              <div className="p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm flex items-center gap-2">
                <span className={`w-2 h-2 rounded-full ${member.status === "Active" ? "bg-success-green" : member.status === "Pending" ? "bg-warning-amber" : "bg-outline"}`} />
                {member.status}
              </div>
            </div>
            {member.status === "Pending" && (
              <div className="p-3 rounded-xl bg-warning-amber/10 border border-warning-amber/20 flex items-start gap-2">
                <span className="material-symbols-outlined text-warning-amber text-[18px] shrink-0">info</span>
                <p className="text-label-xs text-warning-amber">This member hasn&apos;t accepted the invitation yet. Role will apply after they join.</p>
              </div>
            )}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Role</label>
              <div className="space-y-2">
                {(["Owner", "Manager", "ContentCreator", "Viewer"] as MemberRole[]).map((r) => (
                  <button
                    key={r}
                    type="button"
                    onClick={() => setRole(r)}
                    className={`w-full flex items-center gap-3 p-3 rounded-xl border-2 transition-all text-left ${
                      role === r
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-outline-variant/40"
                    }`}
                  >
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center text-label-2xs font-bold ${
                      role === r ? "bg-primary text-on-primary" : "bg-surface-container-high text-outline"
                    }`}>
                      {r === "Owner" ? "👑" : r === "Manager" ? "⚙️" : r === "ContentCreator" ? "✏️" : "👁️"}
                    </div>
                    <div className="flex-1 min-w-0">
                      <span className="text-label-sm font-semibold text-on-surface">{r === "ContentCreator" ? "Content Creator" : r}</span>
                      <p className="text-label-xs text-outline">
                        {r === "Owner" && "Full access to everything"}
                        {r === "Manager" && "Can manage teams and members"}
                        {r === "ContentCreator" && "Can create and manage content"}
                        {r === "Viewer" && "Read-only access"}
                      </p>
                    </div>
                    {role === r && (
                      <span className="material-symbols-outlined text-primary text-[18px] shrink-0">check_circle</span>
                    )}
                  </button>
                ))}
              </div>
            </div>
            {canAssignQuota && member.status === "Active" && (
              <div>
                <div className="flex items-center gap-2 mb-2">
                  <span className="material-symbols-outlined text-[16px] text-outline">toll</span>
                  <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Credit Quota</label>
                </div>
                {member.quotaMode !== "SharedPool" && (
                  <div className="p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl mb-3">
                    <div className="flex items-center justify-between text-label-xs text-outline mb-1">
                      <span>Current usage</span>
                      <span className="font-semibold">{member.creditUsed.toLocaleString()} / {member.creditLimit?.toLocaleString() || "—"}</span>
                    </div>
                    <div className="w-full h-2 bg-surface-container rounded-full overflow-hidden">
                      <div
                        className="h-full bg-primary rounded-full transition-all"
                        style={{ width: `${member.creditLimit ? Math.min(100, (member.creditUsed / member.creditLimit) * 100) : 0}%` }}
                      />
                    </div>
                  </div>
                )}
                <select
                  value={quotaMode}
                  onChange={(e) => handleQuotaModeChange(e.target.value as QuotaMode)}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 transition-all mb-2"
                >
                  <option value="SharedPool">{QUOTA_MODE_LABELS.SharedPool}</option>
                  <option value="LifetimeAssigned">{QUOTA_MODE_LABELS.LifetimeAssigned}</option>
                  <option value="MonthlyAssigned">{QUOTA_MODE_LABELS.MonthlyAssigned}</option>
                </select>
                {showCreditInput && (
                  <input
                    type="number"
                    min="1"
                    value={creditLimit}
                    onChange={(e) => { setCreditLimit(e.target.value); setQuotaChanged(true); }}
                    placeholder={quotaMode === "MonthlyAssigned" ? "e.g. 500 credits/month" : "e.g. 10000 lifetime credits"}
                    className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 transition-all"
                  />
                )}
              </div>
            )}
          </div>
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 shrink-0">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={isLoading || isUpdatingQuota}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
            >
              {(isLoading || isUpdatingQuota) ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <span className="material-symbols-outlined text-[16px]">save</span>
              )}
              Save Changes
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
