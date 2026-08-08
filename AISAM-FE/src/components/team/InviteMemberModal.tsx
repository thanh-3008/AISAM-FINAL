"use client";

import { useState } from "react";
import { type InviteMemberData, type MemberRole, type QuotaMode, QUOTA_MODE_LABELS } from "@/services/teamService";

interface InviteMemberModalProps {
  open: boolean;
  onClose: () => void;
  onInvite: (data: InviteMemberData) => void;
  isLoading: boolean;
  currentMemberCount?: number;
  maxMembers?: number;
  canAssignQuota?: boolean;
}

export default function InviteMemberModal({ open, onClose, onInvite, isLoading, currentMemberCount = 0, maxMembers = Infinity, canAssignQuota = false }: InviteMemberModalProps) {
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<MemberRole>("Viewer");
  const [quotaMode, setQuotaMode] = useState<QuotaMode>("SharedPool");
  const [creditLimit, setCreditLimit] = useState<string>("");

  if (!open) return null;

  const atLimit = currentMemberCount >= maxMembers;

  const showCreditInput = canAssignQuota && quotaMode !== "SharedPool";
  const parsedCreditLimit = showCreditInput && creditLimit ? Number(creditLimit) : null;

  const handleSubmit = () => {
    if (!email.trim() || atLimit) return;
    onInvite({
      email: email.trim(),
      role,
      teamIds: [],
      quotaMode: canAssignQuota ? quotaMode : undefined,
      creditLimit: showCreditInput && parsedCreditLimit && parsedCreditLimit > 0 ? parsedCreditLimit : undefined,
    });
    setEmail("");
    setRole("Viewer");
    setQuotaMode("SharedPool");
    setCreditLimit("");
  };

  const isValid = email.trim() && email.includes("@");

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">person_add</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Invite Member</h2>
                <p className="text-label-xs text-outline">Send an invitation to join your organization</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>
          <div className="p-6 space-y-5">
            {atLimit && (
              <div className="p-3 rounded-xl bg-error/10 border border-error/20 flex items-start gap-3">
                <span className="material-symbols-outlined text-error text-[18px] shrink-0">error_outline</span>
                <p className="text-label-sm text-error">
                  Member limit reached ({currentMemberCount}/{maxMembers}). Upgrade your plan to add more members.
                </p>
              </div>
            )}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="colleague@company.com"
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 transition-all"
              />
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Role</label>
              <select
                value={role}
                onChange={(e) => setRole(e.target.value as MemberRole)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 transition-all"
              >
                <option value="Viewer">Viewer</option>
                <option value="Manager">Manager</option>
                <option value="ContentCreator">Content Creator</option>
                <option value="Owner">Owner</option>
              </select>
            </div>
            {canAssignQuota && (
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Credit Quota Mode</label>
                <select
                  value={quotaMode}
                  onChange={(e) => { setQuotaMode(e.target.value as QuotaMode); if (e.target.value === "SharedPool") setCreditLimit(""); }}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 transition-all"
                >
                  <option value="SharedPool">{QUOTA_MODE_LABELS.SharedPool}</option>
                  <option value="LifetimeAssigned">{QUOTA_MODE_LABELS.LifetimeAssigned}</option>
                  <option value="MonthlyAssigned">{QUOTA_MODE_LABELS.MonthlyAssigned}</option>
                </select>
                {showCreditInput && (
                  <div className="mt-2">
                    <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Credit Limit</label>
                    <input
                      type="number"
                      min="1"
                      value={creditLimit}
                      onChange={(e) => setCreditLimit(e.target.value)}
                      placeholder={quotaMode === "MonthlyAssigned" ? "e.g. 500 credits/month" : "e.g. 10000 lifetime credits"}
                      className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 transition-all"
                    />
                    {creditLimit && Number(creditLimit) <= 0 && (
                      <p className="text-label-2xs text-error mt-1">Credit limit must be greater than 0</p>
                    )}
                  </div>
                )}
              </div>
            )}
          </div>
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={!isValid || isLoading || atLimit}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
            >
              {isLoading ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <span className="material-symbols-outlined text-[16px]">send</span>
              )}
              Send Invite
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
