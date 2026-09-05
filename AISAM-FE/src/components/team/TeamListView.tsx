"use client";

import { type Team } from "@/services/teamService";
import { TEAM_COLORS, getInitials } from "./teamUtils";

interface TeamListViewProps {
  teams: Team[];
  selectedIds: string[];
  actionLoading: string | null;
  onSelect: (id: string, selected: boolean) => void;
  onViewDetail: (team: Team) => void;
  onEdit: (team: Team) => void;
  onDelete: (team: Team) => void;
}

export default function TeamListView({
  teams,
  selectedIds,
  actionLoading,
  onSelect,
  onViewDetail,
  onEdit,
  onDelete,
}: TeamListViewProps) {
  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 overflow-hidden shadow-sm">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-surface-container/50">
              <th className="px-6 py-3.5 text-left">
                <input
                  type="checkbox"
                  checked={teams.length > 0 && selectedIds.length === teams.length}
                  onChange={(e) => {
                    teams.forEach((t) => onSelect(t.id, e.target.checked));
                  }}
                  className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/20 cursor-pointer"
                />
              </th>
              <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Team</th>
              <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Members</th>
              <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Brands</th>
              <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Activity</th>
              <th className="px-6 py-3.5 text-right text-label-xs text-outline font-bold uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/10">
            {teams.map((team, idx) => {
              const colors = TEAM_COLORS[idx % TEAM_COLORS.length];
              return (
                <tr key={team.id} className="hover:bg-primary-fixed/10 transition-colors group cursor-pointer" onClick={() => onViewDetail(team)}>
                  <td className="px-6 py-4">
                    <input
                      type="checkbox"
                      checked={selectedIds.includes(team.id)}
                      onChange={(e) => { e.stopPropagation(); onSelect(team.id, e.target.checked); }}
                      onClick={(e) => e.stopPropagation()}
                      className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/20 cursor-pointer"
                    />
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${colors.bg} flex items-center justify-center text-on-primary font-bold shadow-sm`}>
                        {getInitials(team.name)}
                      </div>
                      <div>
                        <p className="text-body-sm text-on-surface font-semibold">{team.name}</p>
                        <p className="text-label-xs text-outline truncate max-w-xs">{team.description}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className="flex -space-x-1.5">
                        {Array.from({ length: Math.min(team.memberIds.length, 3) }).map((_, i) => (
                          <div
                            key={i}
                            className={`w-6 h-6 rounded-full border-2 border-white bg-gradient-to-br ${TEAM_COLORS[i % TEAM_COLORS.length].bg} flex items-center justify-center text-label-3xs font-bold text-on-primary`}
                          >
                            {i + 1}
                          </div>
                        ))}
                      </div>
                      <span className="text-label-xs text-outline">{team.memberIds.length}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`text-label-xs ${colors.badge} px-2 py-0.5 rounded font-semibold`}>{team.brandCount}</span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2 min-w-[120px]">
                      <div className="flex-1 h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                        <div
                          className={`h-full rounded-full bg-gradient-to-r ${colors.bg}`}
                          style={{ width: `${team.activity == null ? "—" : `${team.activity}%`}` }}
                        />
                      </div>
                      <span className="text-label-xs text-outline font-semibold w-8">{team.activity == null ? "—" : `${team.activity}%`}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button
                        onClick={(e) => { e.stopPropagation(); onViewDetail(team); }}
                        className="p-1.5 rounded-lg text-outline hover:text-primary hover:bg-primary/10 transition-all"
                        title="View detail"
                      >
                        <span className="material-symbols-outlined text-[16px]">visibility</span>
                      </button>
                      <button
                        onClick={(e) => { e.stopPropagation(); onEdit(team); }}
                        className="p-1.5 rounded-lg text-outline hover:text-primary hover:bg-primary/10 transition-all"
                      >
                        <span className="material-symbols-outlined text-[16px]">edit</span>
                      </button>
                      <button
                        onClick={(e) => { e.stopPropagation(); onDelete(team); }}
                        disabled={actionLoading === team.id}
                        className="p-1.5 rounded-lg text-outline hover:text-danger-red hover:bg-danger-red/10 transition-all disabled:opacity-50"
                      >
                        <span className="material-symbols-outlined text-[16px]">delete</span>
                      </button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
