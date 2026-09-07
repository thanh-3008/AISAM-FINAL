"use client";

import { useState, useEffect, useRef } from "react";
import { fetchTeams, type Team } from "@/services/teamService";
import { getStoredActiveTeam, storeActiveTeam, type ActiveTeam } from "@/stores/team-store";
import { TEAM_COLORS, getInitials } from "./teamUtils";

export interface OwnerTeamSwitcherProps {
  workspaceId?: string;
  isOwner?: boolean;
  role?: string;
  teams?: Team[];
  onTeamSwitched?: (team: ActiveTeam) => void;
  className?: string;
  compact?: boolean;
}

function getRoleBadge(role?: string, isOwner?: boolean) {
  if (isOwner || role === "Owner") {
    return {
      label: "Owner",
      className: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20",
    };
  }
  if (role === "Manager") {
    return {
      label: "Manager",
      className: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20",
    };
  }
  if (role === "ContentCreator") {
    return {
      label: "Creator",
      className: "bg-purple-500/10 text-purple-600 dark:text-purple-400 border-purple-500/20",
    };
  }
  if (role === "Viewer") {
    return {
      label: "Viewer",
      className: "bg-slate-500/10 text-slate-600 dark:text-slate-400 border-slate-500/20",
    };
  }
  if (role) {
    return {
      label: role,
      className: "bg-primary/10 text-primary border-primary/20",
    };
  }
  return null;
}

export default function OwnerTeamSwitcher({
  workspaceId,
  isOwner = false,
  role,
  teams: initialTeams,
  onTeamSwitched,
  className = "",
  compact = false,
}: OwnerTeamSwitcherProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [teams, setTeams] = useState<Team[]>(initialTeams || []);
  const [loading, setLoading] = useState(false);
  const [activeTeam, setActiveTeam] = useState<ActiveTeam | null>(() => getStoredActiveTeam(workspaceId));
  const containerRef = useRef<HTMLDivElement>(null);

  // Sync active team on storage/custom event
  useEffect(() => {
    const handleSync = () => {
      setActiveTeam(getStoredActiveTeam(workspaceId));
    };

    handleSync();
    window.addEventListener("aisam_active_team_changed", handleSync);
    window.addEventListener("storage", handleSync);
    return () => {
      window.removeEventListener("aisam_active_team_changed", handleSync);
      window.removeEventListener("storage", handleSync);
    };
  }, [workspaceId]);

  // Load teams if not provided as prop
  useEffect(() => {
    if (initialTeams !== undefined) {
      setTeams(initialTeams);
      return;
    }

    if (!workspaceId) return;

    let cancelled = false;
    setLoading(true);
    fetchTeams()
      .then((res) => {
        if (cancelled) return;
        setTeams(res.data);
        const current = getStoredActiveTeam(workspaceId);
        if ((!current || !res.data.some((t) => t.id === current.id)) && res.data.length > 0) {
          const auto: ActiveTeam = { id: res.data[0].id, name: res.data[0].name, workspaceId };
          storeActiveTeam(auto);
          setActiveTeam(auto);
        }
      })
      .catch(() => {
        if (!cancelled) setTeams([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [workspaceId, initialTeams]);

  // Click outside to close
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSelectTeam = (team: Team) => {
    const newActive: ActiveTeam = {
      id: team.id,
      name: team.name,
      workspaceId,
    };
    storeActiveTeam(newActive);
    setActiveTeam(newActive);
    setIsOpen(false);
    onTeamSwitched?.(newActive);
  };

  if (teams.length <= 0) {
    return null;
  }

  const currentTeam = teams.find((t) => t.id === activeTeam?.id) || teams[0];
  const roleBadge = getRoleBadge(role, isOwner);

  return (
    <div className={`relative inline-block text-left ${className}`} ref={containerRef}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={`flex items-center gap-2 rounded-xl transition-all duration-200 border cursor-pointer ${
          compact
            ? "px-2.5 py-1 text-label-xs bg-surface-container/60 hover:bg-surface-container border-outline-variant/30 text-on-surface"
            : "px-3 py-1.5 text-label-sm bg-surface-container-low hover:bg-surface-container border-outline-variant/30 text-on-surface shadow-sm hover:shadow"
        } ${isOpen ? "ring-2 ring-primary/20 border-primary/40 bg-surface-container" : ""}`}
        title="Chuyển đổi Team đang làm việc"
      >
        <div className="w-5 h-5 rounded-md bg-linear-to-br from-primary to-primary-container flex items-center justify-center text-[11px] text-on-primary font-bold shadow-xs">
          <span className="material-symbols-outlined text-[13px]">swap_horiz</span>
        </div>
        <div className="flex items-center gap-1.5 min-w-0">
          <span className="text-outline text-label-2xs uppercase tracking-wider font-semibold shrink-0">
            Team:
          </span>
          <span className="font-semibold text-on-surface truncate max-w-32.5 sm:max-w-45">
            {currentTeam?.name || "Chọn Team"}
          </span>
        </div>
        {roleBadge && (
          <span className={`px-1.5 py-0.2 rounded text-[10px] font-bold border shrink-0 ${roleBadge.className}`}>
            {roleBadge.label}
          </span>
        )}
        <span
          className={`material-symbols-outlined text-outline text-[16px] transition-transform duration-200 ${
            isOpen ? "rotate-180" : ""
          }`}
        >
          expand_more
        </span>
      </button>

      {isOpen && (
        <div className="absolute right-0 sm:left-0 sm:right-auto mt-2 w-72 bg-surface-container-lowest/95 backdrop-blur-xl border border-outline-variant/30 rounded-2xl shadow-2xl z-50 overflow-hidden animate-in fade-in slide-in-from-top-2 duration-150">
          {/* Header */}
          <div className="px-4 py-3 border-b border-outline-variant/20 bg-surface-container-low/50 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-primary text-[18px]">schema</span>
              <span className="text-label-sm font-bold text-on-surface">Chuyển đổi Team</span>
            </div>
            <span className="text-label-2xs px-2 py-0.5 rounded-full bg-primary/10 text-primary font-bold">
              {teams.length} {teams.length === 1 ? "team" : "teams"}
            </span>
          </div>

          {/* List */}
          <div className="p-1.5 max-h-64 overflow-y-auto space-y-1">
            {teams.map((team, idx) => {
              const isActive = team.id === (activeTeam?.id || currentTeam?.id);
              const color = TEAM_COLORS[idx % TEAM_COLORS.length];

              return (
                <button
                  key={team.id}
                  type="button"
                  onClick={() => handleSelectTeam(team)}
                  className={`w-full flex items-center justify-between p-2.5 rounded-xl transition-all text-left cursor-pointer ${
                    isActive
                      ? "bg-primary/10 border border-primary/30 shadow-xs"
                      : "hover:bg-surface-container border border-transparent"
                  }`}
                >
                  <div className="flex items-center gap-2.5 min-w-0">
                    <div
                      className={`w-8 h-8 rounded-lg bg-linear-to-br ${color.bg} flex items-center justify-center text-on-primary text-label-xs font-bold shrink-0 shadow-xs`}
                    >
                      {getInitials(team.name)}
                    </div>
                    <div className="min-w-0">
                      <p
                        className={`text-label-sm font-semibold truncate ${
                          isActive ? "text-primary font-bold" : "text-on-surface"
                        }`}
                      >
                        {team.name}
                      </p>
                      <p className="text-label-2xs text-outline flex items-center gap-2">
                        <span>{team.memberIds?.length || 0} thành viên</span>
                        <span>•</span>
                        <span>{team.brandCount || 0} brands</span>
                      </p>
                    </div>
                  </div>

                  {isActive ? (
                    <div className="flex items-center gap-1 shrink-0 px-2 py-0.5 rounded-full bg-primary text-on-primary text-[11px] font-bold shadow-xs">
                      <span className="material-symbols-outlined text-[13px]">check</span>
                      <span>Đang chọn</span>
                    </div>
                  ) : (
                    <span className="material-symbols-outlined text-outline/40 hover:text-primary text-[18px] shrink-0">
                      arrow_forward
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Footer */}
          <div className="px-4 py-2 border-t border-outline-variant/15 bg-surface-container-low/30 text-center">
            <p className="text-label-3xs text-outline">
              Chuyển đổi nhanh giữa các Team bạn tham gia
            </p>
          </div>
        </div>
      )}
    </div>
  );
}

export { OwnerTeamSwitcher as TeamSwitcher };
