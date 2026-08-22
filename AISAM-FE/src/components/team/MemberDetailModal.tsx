"use client";

import React from "react";
import { fetchMemberCreditUsage, QUOTA_MODE_LABELS, type MemberCreditUsageRecord, type Team, type TeamMember } from "@/services/teamService";
import { getInitials, formatDate, calcTimeAgo, ROLE_CONFIG, STATUS_CONFIG } from "./teamUtils";

interface MemberDetailModalProps {
  member: TeamMember | null;
  teams: Team[];
  onClose: () => void;
  onEdit: (member: TeamMember) => void;
  onDelete: (member: TeamMember) => void;
  onTransferOwnership?: (member: TeamMember) => void;
  isOwner?: boolean;
  isTransferringOwnership?: boolean;
}

export default function MemberDetailModal({
  member,
  teams,
  onClose,
  onEdit,
  onDelete,
  onTransferOwnership,
  isOwner = false,
  isTransferringOwnership = false,
}: MemberDetailModalProps) {
  const [now, setNow] = React.useState(() => Date.now());
  const [usage, setUsage] = React.useState<MemberCreditUsageRecord[]>([]);
  const [usageTotal, setUsageTotal] = React.useState(0);
  const [usageLoading, setUsageLoading] = React.useState(false);

  React.useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60000);
    return () => clearInterval(interval);
  }, []);

  React.useEffect(() => {
    let cancelled = false;
    if (!member || member.status !== "Active") {
      setUsage([]);
      setUsageTotal(0);
      return;
    }

    setUsageLoading(true);
    fetchMemberCreditUsage(member.id)
      .then((result) => {
        if (cancelled) return;
        setUsage(result.data);
        setUsageTotal(result.totalCount);
      })
      .finally(() => {
        if (!cancelled) setUsageLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [member?.id, member?.status]);

  if (!member) return null;

  const roleConfig = ROLE_CONFIG[member.role];
  const statusConfig = STATUS_CONFIG[member.status];
  const memberTeams = teams.filter((t) => member.teamIds.includes(t.id));
  const hasAssignedQuota = member.quotaMode !== "SharedPool";
  const remainingCredits = member.creditLimit != null ? Math.max(0, member.creditLimit - member.creditUsed) : null;
  const usagePercent = member.creditLimit ? Math.min(100, (member.creditUsed / member.creditLimit) * 100) : 0;
  const canTransferOwnership = isOwner && member.status === "Active" && member.role === "Manager";

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              {member.avatar ? (
                <img src={member.avatar} alt={member.name} className="w-12 h-12 rounded-full object-cover ring-2 ring-white shadow-sm" />
              ) : (
                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center text-headline-sm font-bold text-primary ring-2 ring-white shadow-sm">
                  {getInitials(member.name)}
                </div>
              )}
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">{member.name}</h2>
                <p className="text-label-xs text-outline">{member.email}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 space-y-6">
            <div className="flex items-center gap-3 flex-wrap">
              <span className={`px-2.5 py-1 rounded-full text-label-2xs font-bold uppercase tracking-wider ${roleConfig.bg} ${roleConfig.color}`}>
                {member.role === "ContentCreator" ? "Content Creator" : member.role}
              </span>
              <div className="flex items-center gap-1.5">
                <span className={`w-2 h-2 rounded-full ${statusConfig.dot}`} />
                <span className="text-label-xs text-outline">{member.status}</span>
              </div>
            </div>

            {memberTeams.length > 0 && (
              <div>
                <h3 className="text-label-2xs text-outline uppercase font-bold tracking-widest mb-2">Teams</h3>
                <div className="flex flex-wrap gap-2">
                  {memberTeams.map((t) => (
                    <span key={t.id} className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-label-2xs font-bold bg-surface-container-high text-on-surface">
                      <span className="material-symbols-outlined text-[14px]">group</span>
                      {t.name}
                    </span>
                  ))}
                </div>
              </div>
            )}

            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Credit Usage</h3>
              <div className="bg-surface-container-low rounded-xl divide-y divide-outline-variant/10 overflow-hidden">
                <div className="px-4 py-3 flex items-center justify-between gap-4">
                  <span className="text-label-xs text-outline flex items-center gap-2">
                    Quota Mode
                  </span>
                  <span className="text-label-xs text-on-surface font-semibold text-right">
                    {QUOTA_MODE_LABELS[member.quotaMode]}
                  </span>
                </div>
                <div className="px-4 py-3">
                  {hasAssignedQuota ? (
                    <>
                      <div className="flex items-center justify-between gap-4">
                        <span className="text-label-xs text-outline">Used credits</span>
                        <span className="text-label-xs text-on-surface font-semibold">
                          {member.creditUsed.toLocaleString()} used / {member.creditLimit?.toLocaleString() ?? "unlimited"} limit
                        </span>
                      </div>
                      {member.creditLimit != null && (
                        <>
                          <div className="w-full h-2 bg-surface-container rounded-full overflow-hidden mt-2">
                            <div className="h-full bg-primary rounded-full transition-all" style={{ width: `${usagePercent}%` }} />
                          </div>
                          <div className="flex items-center justify-between mt-2">
                            <span className="text-label-2xs text-outline">Remaining</span>
                            <span className="text-label-2xs text-on-surface font-semibold">{remainingCredits?.toLocaleString()} credits</span>
                          </div>
                        </>
                      )}
                    </>
                  ) : (
                    <p className="text-label-xs text-outline">
                      Uses the shared workspace credit pool. This member has no personal credit limit.
                    </p>
                  )}
                </div>
              </div>
            </div>

            {member.status === "Active" && (
              <div>
                <div className="flex items-center justify-between mb-3">
                  <h3 className="text-label-sm font-bold text-on-surface">Recent Credit Activity</h3>
                  {usageTotal > usage.length && (
                    <span className="text-label-2xs text-outline">{usageTotal.toLocaleString()} total</span>
                  )}
                </div>
                <div className="bg-surface-container-low rounded-xl divide-y divide-outline-variant/10 overflow-hidden">
                  {usageLoading ? (
                    <div className="px-4 py-4 text-label-xs text-outline">Loading usage...</div>
                  ) : usage.length === 0 ? (
                    <div className="px-4 py-4 text-label-xs text-outline">No credit activity yet.</div>
                  ) : (
                    usage.map((record) => (
                      <div key={record.id} className="px-4 py-3 flex items-center justify-between gap-4">
                        <div className="min-w-0">
                          <p className="text-label-xs text-on-surface font-semibold truncate">{record.action}</p>
                          <p className="text-label-2xs text-outline">
                            {record.featureUsed} - {calcTimeAgo(now, record.createdAt)}
                          </p>
                        </div>
                        <span className={`text-label-xs font-bold ${record.status === "Success" ? "text-danger-red" : "text-outline"}`}>
                          {record.status === "Success" ? "-" : ""}{record.credits.toLocaleString()}
                        </span>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}

            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Details</h3>
              <div className="bg-surface-container-low rounded-xl divide-y divide-outline-variant/10">
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-label-xs text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">schedule</span>
                    Last Active
                  </span>
                  <span className="text-label-xs text-on-surface font-medium">{calcTimeAgo(now, member.lastActive)}</span>
                </div>
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-label-xs text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">calendar_month</span>
                    Joined
                  </span>
                  <span className="text-label-xs text-on-surface font-medium">{formatDate(member.createdAt)}</span>
                </div>
              </div>
            </div>
          </div>

          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            {isOwner && (
            <>
            {canTransferOwnership && (
              <button
                onClick={() => onTransferOwnership?.(member)}
                disabled={isTransferringOwnership}
                className="px-5 py-2.5 border border-primary/20 rounded-xl text-label-sm font-semibold text-primary hover:bg-primary/5 transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span className="material-symbols-outlined text-[16px]">workspace_premium</span>
                {isTransferringOwnership ? "Transferring..." : "Transfer Owner"}
              </button>
            )}
            <button
              onClick={() => { onDelete(member); onClose(); }}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-danger-red hover:bg-danger-red/5 transition-all flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[16px]">delete</span>
              Remove
            </button>
            <button
              onClick={() => { onEdit(member); onClose(); }}
              disabled={member.status === "Pending"}
              className={`px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold transition-all flex items-center gap-2 ${
                member.status === "Pending"
                  ? "text-outline/30 cursor-not-allowed"
                  : "text-outline hover:text-on-surface hover:bg-surface-container"
              }`}
              title={member.status === "Pending" ? "Role can be changed after acceptance" : "Edit Role"}
            >
              <span className="material-symbols-outlined text-[16px]">edit</span>
              Edit Role
            </button>
            </>
            )}
            <button
              onClick={onClose}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
