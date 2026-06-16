"use client";

import React from "react";
import { type Team, type TeamMember } from "@/services/teamService";
import { getInitials, formatDate, calcTimeAgo, ROLE_CONFIG, STATUS_CONFIG } from "./teamUtils";

interface MemberDetailModalProps {
  member: TeamMember | null;
  teams: Team[];
  onClose: () => void;
  onEdit: (member: TeamMember) => void;
  onDelete: (member: TeamMember) => void;
}

export default function MemberDetailModal({ member, teams, onClose, onEdit, onDelete }: MemberDetailModalProps) {
  const [now, setNow] = React.useState(() => Date.now());

  React.useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60000);
    return () => clearInterval(interval);
  }, []);

  if (!member) return null;

  const roleConfig = ROLE_CONFIG[member.role];
  const statusConfig = STATUS_CONFIG[member.status];
  const memberTeams = teams.filter((t) => member.teamIds.includes(t.id));

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
            <button
              onClick={() => { onDelete(member); onClose(); }}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-danger-red hover:bg-danger-red/5 transition-all flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[16px]">delete</span>
              Remove
            </button>
            <button
              onClick={() => { onEdit(member); onClose(); }}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[16px]">edit</span>
              Edit Role
            </button>
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
