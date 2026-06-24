"use client";

import { useState, useEffect } from "react";
import { type TeamMember } from "@/services/teamService";
import { ROLE_CONFIG, STATUS_CONFIG, getInitials, calcTimeAgo } from "./teamUtils";

interface MemberCardProps {
  member: TeamMember;
  onEdit: (member: TeamMember) => void;
  onDelete: (member: TeamMember) => void;
  onViewDetail?: (member: TeamMember) => void;
}

export default function MemberCard({ member, onEdit, onDelete, onViewDetail }: MemberCardProps) {
  const roleConfig = ROLE_CONFIG[member.role];
  const statusConfig = STATUS_CONFIG[member.status];
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div
      className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-5 card-hover group relative overflow-hidden cursor-pointer"
      onClick={() => onViewDetail?.(member)}
    >
      <div className={`absolute top-0 left-0 w-full h-1 ${member.status === "Active" ? "bg-gradient-to-r from-success-green to-emerald-400" : member.status === "Pending" ? "bg-gradient-to-r from-warning-amber to-amber-400" : "bg-gradient-to-r from-outline/30 to-outline/20"}`} />

      <div className="flex items-start justify-between mb-4 mt-1">
        <div className="relative">
          {member.avatar ? (
            <div className="relative">
              <img src={member.avatar} alt={member.name} className="w-14 h-14 rounded-full object-cover ring-2 ring-white shadow-md" />
              {member.status === "Active" && (
                <span className="absolute -bottom-0.5 -right-0.5 w-4 h-4 rounded-full bg-success-green border-2 border-white animate-pulse-dot" />
              )}
            </div>
          ) : (
            <div className="relative">
              <div className="w-14 h-14 rounded-full bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center text-headline-sm font-bold text-primary ring-2 ring-white shadow-md">
                {getInitials(member.name)}
              </div>
              {member.status === "Active" && (
                <span className="absolute -bottom-0.5 -right-0.5 w-4 h-4 rounded-full bg-success-green border-2 border-white animate-pulse-dot" />
              )}
            </div>
          )}
        </div>
        <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            onClick={(e) => { e.stopPropagation(); onEdit(member); }}
            disabled={member.status === "Pending"}
            className={`p-1.5 rounded-lg transition-all ${
              member.status === "Pending"
                ? "text-outline/30 cursor-not-allowed"
                : "hover:bg-surface-container text-outline hover:text-primary"
            }`}
            title={member.status === "Pending" ? "Role can be changed after acceptance" : "Edit member"}
          >
            <span className="material-symbols-outlined text-[16px]">edit</span>
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); onDelete(member); }}
            className="p-1.5 rounded-lg hover:bg-surface-container text-outline hover:text-danger-red transition-all"
          >
            <span className="material-symbols-outlined text-[16px]">delete</span>
          </button>
        </div>
      </div>

      <div className="space-y-3">
        <div>
          <h3 className="text-body-md text-on-surface font-semibold">{member.name}</h3>
          <p className="text-label-xs text-outline">{member.email}</p>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <span className={`px-2.5 py-1 rounded-full text-label-2xs font-bold uppercase tracking-wider ${roleConfig.bg} ${roleConfig.color}`}>
            {member.role}
          </span>
          <div className="flex items-center gap-1.5">
            <span className={`w-2 h-2 rounded-full ${statusConfig.dot}`} />
            <span className="text-label-xs text-outline">{member.status}</span>
          </div>
        </div>

        <div className="pt-3 border-t border-outline-variant/20">
          <div className="flex items-center gap-1.5 text-label-xs text-outline">
            <span className="material-symbols-outlined text-[14px]">schedule</span>
            <span>Last active: {calcTimeAgo(now, member.lastActive)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}