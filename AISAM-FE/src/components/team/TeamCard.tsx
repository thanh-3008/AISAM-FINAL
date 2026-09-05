"use client";

import { type Team } from "@/services/teamService";
import { TEAM_COLORS, getInitials } from "./teamUtils";

interface TeamCardProps {
  team: Team;
  index: number;
  isSelected: boolean;
  isLoading: boolean;
  onSelect: (id: string, selected: boolean) => void;
  onViewDetail: (team: Team) => void;
  onEdit: (team: Team) => void;
  onDelete: (team: Team) => void;
}

export default function TeamCard({ team, index, isSelected, isLoading, onSelect, onViewDetail, onEdit, onDelete }: TeamCardProps) {
  const colors = TEAM_COLORS[index % TEAM_COLORS.length];

  return (
    <div
      onClick={() => onViewDetail(team)}
      className={`bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border card-hover cursor-pointer group relative overflow-hidden transition-all ${
        isSelected ? "border-primary ring-2 ring-primary/20" : "border-outline-variant/30 hover:shadow-md"
      }`}
    >
      <div className={`absolute top-0 left-0 w-full h-1 bg-gradient-to-r ${colors.bg}`} />

      <div className="p-5">
        <div className="flex items-start justify-between mb-4 mt-1">
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              checked={isSelected}
              onChange={(e) => { e.stopPropagation(); onSelect(team.id, e.target.checked); }}
              onClick={(e) => e.stopPropagation()}
              className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/20 cursor-pointer"
            />
            <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${colors.bg} flex items-center justify-center text-on-primary font-bold shadow-sm`}>
              {getInitials(team.name)}
            </div>
          </div>
          <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
            <button
              onClick={(e) => { e.stopPropagation(); onViewDetail(team); }}
              className="p-1.5 rounded-lg hover:bg-surface-container text-outline hover:text-primary transition-all"
              title="View detail"
            >
              <span className="material-symbols-outlined text-[16px]">visibility</span>
            </button>
            <button
              onClick={(e) => { e.stopPropagation(); onEdit(team); }}
              className="p-1.5 rounded-lg hover:bg-surface-container text-outline hover:text-primary transition-all"
            >
              <span className="material-symbols-outlined text-[16px]">edit</span>
            </button>
            <button
              onClick={(e) => { e.stopPropagation(); onDelete(team); }}
              disabled={isLoading}
              className="p-1.5 rounded-lg hover:bg-surface-container text-outline hover:text-danger-red transition-all disabled:opacity-50"
            >
              <span className="material-symbols-outlined text-[16px]">delete</span>
            </button>
          </div>
        </div>

        <div>
          <h3 className="text-headline-sm text-on-surface font-semibold mb-1">{team.name}</h3>
          <p className="text-body-sm text-on-surface-variant mb-4 truncate">{team.description}</p>

          {team.activity != null && <div className="mb-4">
            <div className="flex items-center justify-between mb-1">
              <span className="text-label-2xs text-outline">Activity</span>
              <span className="text-label-2xs text-outline font-semibold">{team.activity}%</span>
            </div>
            <div className="w-full h-1.5 bg-surface-container-high rounded-full overflow-hidden">
              <div
                className={`h-full rounded-full bg-gradient-to-r ${colors.bg} transition-all duration-700`}
                style={{ width: `${team.activity}%` }}
              />
            </div>
          </div>}
        </div>

        <div className="flex items-center justify-between pt-4 border-t border-outline-variant/20">
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
            <span className="text-label-xs text-outline">{team.memberIds.length} members</span>
          </div>
          <span className={`text-label-xs ${colors.badge} px-2 py-0.5 rounded font-semibold`}>{team.brandCount} Brands</span>
        </div>
      </div>
    </div>
  );
}
