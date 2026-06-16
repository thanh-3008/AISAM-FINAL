"use client";

import { type Team, type TeamMember } from "@/services/teamService";
import { TEAM_COLORS, getInitials, formatDate, ROLE_CONFIG, STATUS_CONFIG } from "./teamUtils";

interface TeamDetailModalProps {
  team: Team | null;
  members: TeamMember[];
  onClose: () => void;
}

export default function TeamDetailModal({ team, members, onClose }: TeamDetailModalProps) {
  if (!team) return null;

  const teamMembers = members.filter((m) => team.memberIds.includes(m.id));
  const colorIdx = TEAM_COLORS.findIndex((c) => c.badge.includes(team.id.charAt(0))) || 0;
  const colors = TEAM_COLORS[colorIdx % TEAM_COLORS.length];

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-3xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className={`w-12 h-12 rounded-xl bg-gradient-to-br ${colors.bg} flex items-center justify-center text-on-primary font-bold shadow-sm`}>
                {getInitials(team.name)}
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">{team.name}</h2>
                <p className="text-label-xs text-outline">{team.description}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 space-y-6">
            <div className="flex items-center gap-3 flex-wrap">
              <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-label-2xs font-bold ${colors.badge}`}>
                <span className="material-symbols-outlined text-[14px]">work</span>
                {team.brandCount} Brands
              </span>
              <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-label-2xs font-bold bg-surface-container-high text-on-surface">
                <span className="material-symbols-outlined text-[14px]">group</span>
                {teamMembers.length} Members
              </span>
              <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-label-2xs font-bold bg-emerald-50 text-success-green">
                <span className="material-symbols-outlined text-[14px]">trending_up</span>
                {team.activity}% Activity
              </span>
            </div>

            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Activity Level</h3>
              <div className="bg-surface-container-low rounded-xl p-4">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-label-xs text-outline font-medium">Team performance</span>
                  <span className={`text-label-xs font-bold ${team.activity >= 80 ? "text-success-green" : team.activity >= 50 ? "text-warning-amber" : "text-danger-red"}`}>
                    {team.activity}%
                  </span>
                </div>
                <div className="h-3 bg-surface-container-high rounded-full overflow-hidden mb-2">
                  <div
                    className={`h-full rounded-full bg-gradient-to-r ${colors.bg} transition-all duration-500`}
                    style={{ width: `${team.activity}%` }}
                  />
                </div>
              </div>
            </div>

            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Team Details</h3>
              <div className="bg-surface-container-low rounded-xl divide-y divide-outline-variant/10">
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-label-xs text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">schedule</span>
                    Created
                  </span>
                  <span className="text-label-xs text-on-surface font-medium">{formatDate(team.createdAt)}</span>
                </div>
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-label-xs text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">update</span>
                    Last Updated
                  </span>
                  <span className="text-label-xs text-on-surface font-medium">{formatDate(team.updatedAt)}</span>
                </div>
              </div>
            </div>

            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">
                Members ({teamMembers.length})
              </h3>
              <div className="space-y-2">
                {teamMembers.map((member) => {
                  const roleConfig = ROLE_CONFIG[member.role];
                  const statusConfig = STATUS_CONFIG[member.status];
                  return (
                    <div key={member.id} className="bg-surface-container-low rounded-xl p-4 flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        {member.avatar ? (
                          <img src={member.avatar} alt={member.name} className="w-9 h-9 rounded-full object-cover" />
                        ) : (
                          <div className="w-9 h-9 rounded-full bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center text-label-sm font-bold text-primary">
                            {getInitials(member.name)}
                          </div>
                        )}
                        <div>
                          <p className="text-body-sm text-on-surface font-semibold">{member.name}</p>
                          <p className="text-label-xs text-outline">{member.email}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <span className={`px-2 py-0.5 rounded-full text-label-2xs font-bold uppercase tracking-wider ${roleConfig.bg} ${roleConfig.color}`}>
                          {member.role}
                        </span>
                        <div className="flex items-center gap-1.5">
                          <span className={`w-2 h-2 rounded-full ${statusConfig.dot}`} />
                          <span className="text-label-xs text-outline">{member.status}</span>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end sticky bottom-0 bg-surface-container-lowest">
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
