"use client";

import { type AdCampaignObjective, type AdCampaignStatus } from "@/services/adCampaignService";
import { CAMPAIGN_OBJECTIVES, CAMPAIGN_STATUSES, objectiveLabels } from "./campaignDisplay";

export type CampaignSortOption = "newest" | "oldest" | "updated" | "name_asc" | "name_desc";

interface CampaignFiltersProps {
  search: string;
  status: AdCampaignStatus | "";
  objective: AdCampaignObjective | "";
  sort: CampaignSortOption;
  resultCount: number;
  totalCount: number;
  onSearchChange: (value: string) => void;
  onStatusChange: (value: AdCampaignStatus | "") => void;
  onObjectiveChange: (value: AdCampaignObjective | "") => void;
  onSortChange: (value: CampaignSortOption) => void;
  onClear: () => void;
}

export default function CampaignFilters({
  search,
  status,
  objective,
  sort,
  resultCount,
  totalCount,
  onSearchChange,
  onStatusChange,
  onObjectiveChange,
  onSortChange,
  onClear,
}: CampaignFiltersProps) {
  const hasFilters = search || status || objective;

  return (
    <div className="p-4 border-b border-outline-variant/10 flex flex-col lg:flex-row lg:items-center justify-between gap-3">
      <div>
        <h2 className="text-title-md font-bold text-on-surface">Campaign list</h2>
        <p className="text-label-sm text-outline">{resultCount} shown of {totalCount} workspace campaign records</p>
      </div>
      <div className="flex flex-col sm:flex-row gap-2">
        <div className="relative">
          <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[16px] text-outline">search</span>
          <input
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder="Search campaigns"
            className="h-10 w-full sm:w-64 rounded-xl border border-outline-variant/30 bg-surface-container-lowest pl-9 pr-8 text-body-sm outline-none focus:border-primary"
          />
          {search && (
            <button onClick={() => onSearchChange("")} className="absolute right-2 top-1/2 -translate-y-1/2 w-6 h-6 rounded-lg hover:bg-surface-container material-symbols-outlined text-[14px]">
              close
            </button>
          )}
        </div>
        <select value={status} onChange={(event) => onStatusChange(event.target.value as AdCampaignStatus | "")} className="h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary">
          <option value="">All status</option>
          {CAMPAIGN_STATUSES.map((item) => <option key={item} value={item}>{item}</option>)}
        </select>
        <select value={objective} onChange={(event) => onObjectiveChange(event.target.value as AdCampaignObjective | "")} className="h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary">
          <option value="">All objectives</option>
          {CAMPAIGN_OBJECTIVES.map((item) => <option key={item} value={item}>{objectiveLabels[item]}</option>)}
        </select>
        <select value={sort} onChange={(event) => onSortChange(event.target.value as CampaignSortOption)} className="h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary">
          <option value="newest">Newest</option>
          <option value="oldest">Oldest</option>
          <option value="updated">Recently updated</option>
          <option value="name_asc">Name A-Z</option>
          <option value="name_desc">Name Z-A</option>
        </select>
        {hasFilters && (
          <button onClick={onClear} className="h-10 px-4 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container">
            Clear
          </button>
        )}
      </div>
    </div>
  );
}
