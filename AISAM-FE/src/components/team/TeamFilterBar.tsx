"use client";

import { type MemberStatus } from "@/services/teamService";
import { STATUS_CONFIG } from "./teamUtils";

export type SortOption = "newest" | "oldest" | "name" | "role" | "status";

interface TeamFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  statusFilter: MemberStatus | "";
  onStatusFilterChange: (value: MemberStatus | "") => void;
  sortBy: SortOption;
  onSortChange: (value: SortOption) => void;
  resultCount: number;
  totalCount: number;
}

export default function TeamFilterBar({
  search,
  onSearchChange,
  statusFilter,
  onStatusFilterChange,
  sortBy,
  onSortChange,
  resultCount,
  totalCount,
}: TeamFilterBarProps) {
  const hasFilters = search || statusFilter;

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 px-4 py-3 shadow-sm animate-fade-up flex items-center gap-3 flex-wrap" style={{ animationDelay: "0.15s" }}>
      <div className="relative flex-1 min-w-[200px]">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[16px]">search</span>
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="Search members..."
          className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg pl-9 pr-8 py-2 text-label-md text-on-surface outline-none focus:ring-2 focus:ring-primary/20 transition-all placeholder:text-outline/40"
        />
        {search && (
          <button onClick={() => onSearchChange("")} className="absolute right-2 top-1/2 -translate-y-1/2 p-0.5 hover:bg-surface-container-high rounded-full">
            <span className="material-symbols-outlined text-label-md text-outline">close</span>
          </button>
        )}
      </div>

      <select
        value={statusFilter}
        onChange={(e) => onStatusFilterChange(e.target.value as MemberStatus | "")}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-label-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="">All Status</option>
        {Object.entries(STATUS_CONFIG).map(([key, config]) => (
          <option key={key} value={key}>{config.label}</option>
        ))}
      </select>

      <select
        value={sortBy}
        onChange={(e) => onSortChange(e.target.value as SortOption)}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-label-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="newest">Newest</option>
        <option value="oldest">Oldest</option>
        <option value="name">Name A-Z</option>
        <option value="role">Role</option>
        <option value="status">Status</option>
      </select>

      {hasFilters && (
        <div className="flex items-center gap-2 ml-auto">
          <span className="text-label-sm text-outline">
            {resultCount}/{totalCount}
          </span>
          <button
            onClick={() => {
              onSearchChange("");
              onStatusFilterChange("");
            }}
            className="text-label-sm text-primary font-medium hover:underline"
          >
            Clear
          </button>
        </div>
      )}
    </div>
  );
}
