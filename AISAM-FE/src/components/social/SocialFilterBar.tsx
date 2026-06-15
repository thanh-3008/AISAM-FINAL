"use client";

import { PlatformIcon } from "@/lib/contentConstants";
import { type SocialPlatform, type AccountStatus } from "@/services/socialAccountService";
import { PLATFORM_INFO } from "./socialUtils";

export type SortOption = "newest" | "expiring" | "followers" | "targets";

interface SocialFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  platformFilter: SocialPlatform | "";
  onPlatformFilterChange: (value: SocialPlatform | "") => void;
  statusFilter: AccountStatus | "";
  onStatusFilterChange: (value: AccountStatus | "") => void;
  sortBy: SortOption;
  onSortChange: (value: SortOption) => void;
  resultCount: number;
  totalCount: number;
}

export default function SocialFilterBar({
  search,
  onSearchChange,
  platformFilter,
  onPlatformFilterChange,
  statusFilter,
  onStatusFilterChange,
  sortBy,
  onSortChange,
  resultCount,
  totalCount,
}: SocialFilterBarProps) {
  const hasFilters = search || platformFilter || statusFilter;

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 px-4 py-3 shadow-sm animate-fade-up flex items-center gap-3 flex-wrap" style={{ animationDelay: "0.15s" }}>
      {/* Search */}
      <div className="relative flex-1 min-w-[200px]">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[16px]">search</span>
        <input
          type="text"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="Search..."
          className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg pl-9 pr-8 py-2 text-[12px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20 transition-all placeholder:text-outline/40"
        />
        {search && (
          <button onClick={() => onSearchChange("")} className="absolute right-2 top-1/2 -translate-y-1/2 p-0.5 hover:bg-surface-container-high rounded-full">
            <span className="material-symbols-outlined text-[12px] text-outline">close</span>
          </button>
        )}
      </div>

      {/* Platform Filter */}
      <div className="flex items-center gap-1 bg-surface-container-low rounded-lg p-0.5">
        <button
          onClick={() => onPlatformFilterChange("")}
          className={`px-2.5 py-1.5 rounded-md text-[11px] font-medium transition-all ${
            platformFilter === "" ? "bg-surface-container-lowest text-on-surface shadow-sm" : "text-outline hover:text-on-surface"
          }`}
        >
          All
        </button>
        {(Object.keys(PLATFORM_INFO) as SocialPlatform[]).map((platform) => (
          <button
            key={platform}
            onClick={() => onPlatformFilterChange(platformFilter === platform ? "" : platform)}
            className={`flex items-center gap-1 px-2.5 py-1.5 rounded-md text-[11px] font-medium transition-all ${
              platformFilter === platform ? "bg-surface-container-lowest text-on-surface shadow-sm" : "text-outline hover:text-on-surface"
            }`}
          >
            <PlatformIcon platform={platform} className="w-[11px] h-[11px]" />
            <span className="hidden sm:inline">{PLATFORM_INFO[platform].label}</span>
          </button>
        ))}
      </div>

      {/* Status Filter */}
      <select
        value={statusFilter}
        onChange={(e) => onStatusFilterChange(e.target.value as AccountStatus | "")}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="">All Status</option>
        <option value="connected">Connected</option>
        <option value="expired">Expired</option>
        <option value="error">Error</option>
      </select>

      {/* Sort */}
      <select
        value={sortBy}
        onChange={(e) => onSortChange(e.target.value as SortOption)}
        className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-2 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
      >
        <option value="newest">Newest</option>
        <option value="expiring">Expiring</option>
        <option value="followers">Most Followers</option>
        <option value="targets">Most Targets</option>
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
              onPlatformFilterChange("");
              onStatusFilterChange("");
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
