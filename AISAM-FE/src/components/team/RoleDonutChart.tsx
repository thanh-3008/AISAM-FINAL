"use client";

import { type TeamMember } from "@/services/teamService";

interface RoleDonutChartProps {
  members: TeamMember[];
}

export default function RoleDonutChart({ members }: RoleDonutChartProps) {
  const roleCounts = {
    Owner: members.filter((m) => m.role === "Owner").length,
    Admin: members.filter((m) => m.role === "Admin").length,
    Editor: members.filter((m) => m.role === "Editor").length,
    Member: members.filter((m) => m.role === "Member").length,
    Viewer: members.filter((m) => m.role === "Viewer").length,
  };

  const total = members.length;
  if (total === 0) return null;

  const roles = [
    { name: "Owner", count: roleCounts.Owner, color: "#004ccd" },
    { name: "Admin", count: roleCounts.Admin, color: "#731be5" },
    { name: "Editor", count: roleCounts.Editor, color: "#9e3100" },
    { name: "Member", count: roleCounts.Member, color: "#198038" },
    { name: "Viewer", count: roleCounts.Viewer, color: "#737687" },
  ].filter((r) => r.count > 0);

  let cumulativePercent = 0;
  const gradientStops = roles
    .map((role) => {
      const percent = (role.count / total) * 100;
      const start = cumulativePercent;
      cumulativePercent += percent;
      return `${role.color} ${start}% ${cumulativePercent}%`;
    })
    .join(", ");

  return (
    <div className="flex items-center gap-6">
      <div className="relative">
        <div
          className="w-32 h-32 rounded-full"
          style={{
            background: `conic-gradient(${gradientStops})`,
          }}
        >
          <div className="absolute inset-4 bg-surface-container-lowest rounded-full flex items-center justify-center">
            <div className="text-center">
              <div className="text-headline-md font-bold text-on-surface">{total}</div>
              <div className="text-label-xs text-outline">Members</div>
            </div>
          </div>
        </div>
      </div>
      <div className="flex-1 space-y-2">
        {roles.map((role) => (
          <div key={role.name} className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full" style={{ backgroundColor: role.color }} />
            <span className="text-label-sm text-on-surface flex-1">{role.name}</span>
            <span className="text-label-sm font-semibold text-on-surface">{role.count}</span>
            <span className="text-label-xs text-outline">({Math.round((role.count / total) * 100)}%)</span>
          </div>
        ))}
      </div>
    </div>
  );
}
