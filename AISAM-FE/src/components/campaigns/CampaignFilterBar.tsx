"use client";

import { type CampaignStatus, type CampaignObjective } from "@/services/campaignService";
import { OBJECTIVE_CONFIG } from "./campaignUtils";

export type SortOption = "newest" | "oldest" | "budget_high" | "budget_low" | "spend_high" | "name";

interface CampaignFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  statusFilter: CampaignStatus | "";
  onStatusFilterChange: (value: CampaignStatus | "") => void;
  objectiveFilter: CampaignObjective | "";
  onObjectiveFilterChange: (value: CampaignObjective | "") => void;
  sortBy: SortOption;
  onSortChange: (value: SortOption) => void;
  resultCount: number;
  totalCount: number;
}

export default function CampaignFilterBar({
  search,
  onSearchChange,
  statusFilter,
  onStatusFilterChange,
  objectiveFilter,
  onObjectiveFilterChange,
  sortBy,
  onSortChange,
  resultCount,
  totalCount,
}: CampaignFilterBarProps) {
  const hasFilters = search || statusFilter || objectiveFilter;

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 px-4 py-3 shadow-sm animate-fade-up flex items-center gap-3 flex-wrap" style={{ animationDelay: "0.15s" }}>
      {/* Search */}
      <div className="relative flex-1 min-w-[200px]">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[16px]">search</span>
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="Search campaigns..."
          className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg pl-9 pr-8 py-2 text-[12px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20 transition-all placeholder:text-outline/40"
        />
        {search && (
          <button onClick={() => onSearchChange("")} className="absolute right-2 top-1/2 -translate-y-1/2 p-0.5 hover:bg-surface-container-high rounded-full">
            <span className="material-symbols-outlined text-[12px] text-outline">close</span>
          </button>
        )}
      </div>

      {/* Status Filter */}
      <select
        value={statusFilter}
        onChange={(e) => onStatusFilterChange(e.target.value as CampaignStatus | "")}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="">All Status</option>
        <option value="ACTIVE">Active</option>
        <option value="PAUSED">Paused</option>
        <option value="COMPLETED">Completed</option>
        <option value="DRAFT">Draft</option>
      </select>

      {/* Objective Filter */}
      <select
        value={objectiveFilter}
        onChange={(e) => onObjectiveFilterChange(e.target.value as CampaignObjective | "")}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="">All Objectives</option>
        {Object.entries(OBJECTIVE_CONFIG).map(([key, config]) => (
          <option key={key} value={key}>{config.label}</option>
        ))}
      </select>

      {/* Sort */}
      <select
        value={sortBy}
        onChange={(e) => onSortChange(e.target.value as SortOption)}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="newest">Newest</option>
        <option value="oldest">Oldest</option>
        <option value="budget_high">Budget: High to Low</option>
        <option value="budget_low">Budget: Low to High</option>
        <option value="spend_high">Spend: High to Low</option>
        <option value="name">Name A-Z</option>
      </select>

      {/* Result Count & Clear */}
      {hasFilters && (
        <div className="flex items-center gap-2 ml-auto">
          <span className="text-[11px] text-outline">
            {resultCount}/{totalCount}
          </span>
          <button
            onClick={() => {
              onSearchChange("");
              onStatusFilterChange("");
              onObjectiveFilterChange("");
            }}
            className="text-[11px] text-primary font-medium hover:underline"
          >
            Clear
          </button>
        </div>
      )}
    </div>
  );
}
