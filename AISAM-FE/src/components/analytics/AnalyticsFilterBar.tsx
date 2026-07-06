"use client";

import { type DateRange } from "@/services/analyticsService";
import { DATE_RANGE_OPTIONS, CAMPAIGN_OPTIONS } from "./analyticsUtils";

interface BrandOption {
  label: string;
  value: string;
}

interface PlatformOption {
  label: string;
  value: string;
  color?: string;
}

interface AnalyticsFilterBarProps {
  dateRange: DateRange;
  onDateRangeChange: (value: DateRange) => void;
  campaignFilter: string;
  onCampaignFilterChange: (value: string) => void;
  brandFilter: string;
  onBrandFilterChange: (value: string) => void;
  platformFilter: string;
  onPlatformFilterChange: (value: string) => void;
  brandOptions?: BrandOption[];
  platformOptions?: PlatformOption[];
  onRefresh: () => void;
}

const DEFAULT_PLATFORM_OPTIONS: PlatformOption[] = [
  { value: "all", label: "All Platforms" },
  { value: "facebook", label: "Facebook", color: "#1877F2" },
  { value: "instagram", label: "Instagram", color: "#DD2A7B" },
  { value: "tiktok", label: "TikTok", color: "#111111" },
];

export default function AnalyticsFilterBar({
  dateRange,
  onDateRangeChange,
  campaignFilter,
  onCampaignFilterChange,
  brandFilter,
  onBrandFilterChange,
  platformFilter,
  onPlatformFilterChange,
  brandOptions,
  platformOptions,
  onRefresh,
}: AnalyticsFilterBarProps) {
  const brands = brandOptions && brandOptions.length > 0
    ? brandOptions
    : [{ label: "All Brands", value: "all" }];
  const platforms = platformOptions && platformOptions.length > 0
    ? platformOptions
    : DEFAULT_PLATFORM_OPTIONS;
  const selectedPlatform = platforms.find((p) => p.value === platformFilter);

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-xl rounded-2xl border border-outline-variant/30 px-6 py-4 shadow-lg animate-fade-up" style={{ animationDelay: "0.15s" }}>
      <div className="flex items-center gap-4 flex-wrap">
        {/* Filter label */}
        <div className="flex items-center gap-2 pr-4 border-r border-outline-variant/30">
          <span className="material-symbols-outlined text-primary text-body-sm">filter_list</span>
          <span className="text-label-sm font-bold text-on-surface">Filters</span>
        </div>

        {/* Date Range */}
        <select
          value={dateRange}
          onChange={(e) => onDateRangeChange(e.target.value as DateRange)}
          className="bg-surface-container-low border border-outline-variant/30 rounded-xl px-4 py-2.5 text-label-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all duration-300 cursor-pointer hover:bg-surface-container-high"
        >
          {DATE_RANGE_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>

        {/* Campaign Filter */}
        <select
          value={campaignFilter}
          onChange={(e) => onCampaignFilterChange(e.target.value)}
          className="bg-surface-container-low border border-outline-variant/30 rounded-xl px-4 py-2.5 text-label-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all duration-300 cursor-pointer hover:bg-surface-container-high"
        >
          {CAMPAIGN_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>

        {/* Brand Filter */}
        <select
          value={brandFilter}
          onChange={(e) => onBrandFilterChange(e.target.value)}
          className="bg-surface-container-low border border-outline-variant/30 rounded-xl px-4 py-2.5 text-label-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all duration-300 cursor-pointer hover:bg-surface-container-high"
        >
          {brands.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>

        {/* Platform Filter with highlight */}
        <select
          value={platformFilter}
          onChange={(e) => onPlatformFilterChange(e.target.value)}
          className={`border rounded-xl px-4 py-2.5 text-label-sm outline-none focus:ring-2 focus:ring-primary/30 transition-all duration-300 cursor-pointer ${
            selectedPlatform
              ? "bg-gradient-to-r from-primary/10 to-primary/5 border-primary/30 text-primary font-semibold"
              : "bg-surface-container-low border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
          }`}
        >
          {platforms.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>

        {/* Actions */}
        <div className="ml-auto flex items-center gap-2">
          <button
            onClick={onRefresh}
            className="group p-2.5 rounded-xl bg-surface-container-low hover:bg-primary/10 border border-outline-variant/30 hover:border-primary/30 transition-all duration-300"
            title="Refresh data"
          >
            <span className="material-symbols-outlined text-outline group-hover:text-primary group-hover:rotate-180 transition-all duration-500 text-body-sm">
              refresh
            </span>
          </button>
          <button className="group p-2.5 rounded-xl bg-surface-container-low hover:bg-surface-container-high border border-outline-variant/30 transition-all duration-300">
            <span className="material-symbols-outlined text-outline group-hover:text-on-surface text-body-sm">
              more_vert
            </span>
          </button>
        </div>
      </div>

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-up {
          animation: fade-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
      `}</style>
    </div>
  );
}
