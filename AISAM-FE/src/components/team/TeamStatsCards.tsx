"use client";

import { type Team, type TeamMember } from "@/services/teamService";

interface TeamStatsCardsProps {
  teams: Team[];
  members: TeamMember[];
}

export default function TeamStatsCards({ teams, members }: TeamStatsCardsProps) {
  const activeMembers = members.filter((m) => m.status === "Active").length;
  const pendingInvites = members.filter((m) => m.status === "Pending").length;

  const stats = [
    { label: "Total Members", value: members.length, icon: "group", color: "text-primary", bg: "bg-primary/10", trend: `${activeMembers} active` },
    { label: "Active Teams", value: teams.length, icon: "schema", color: "text-secondary", bg: "bg-secondary/10", trend: "All active" },
    { label: "Pending Invites", value: pendingInvites, icon: "mail", color: "text-tertiary", bg: "bg-tertiary/10", trend: "Awaiting response" },
  ];

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-5 shadow-sm animate-fade-up" style={{ animationDelay: "0.1s" }}>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {stats.map((s) => (
          <div key={s.label} className="flex items-center gap-3">
            <div className={`w-10 h-10 rounded-xl ${s.bg} flex items-center justify-center ${s.color} shrink-0`}>
              <span className="material-symbols-outlined text-[20px]">{s.icon}</span>
            </div>
            <div>
              <p className="text-label-xs text-outline uppercase font-medium">{s.label}</p>
              <p className="text-headline-sm font-bold text-on-surface leading-tight">{s.value}</p>
              <p className="text-label-2xs text-outline mt-0.5">{s.trend}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
