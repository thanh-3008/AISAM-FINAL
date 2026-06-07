"use client";

import { useState } from "react";
import { PLATFORM_CONFIG } from "@/lib/contentConstants";
import { PostItem, PostStatus, PostType } from "@/services/postService";

interface FiltersProps {
  posts: PostItem[];
  filters: {
    search: string;
    brand: string;
    platform: string;
    status: string;
    type: string;
    dateFrom: string;
    dateTo: string;
    minLikes: number;
    minComments: number;
    minShares: number;
  };
  onFilterChange: (filters: Partial<FiltersProps["filters"]>) => void;
  onClearFilters: () => void;
}

const POST_TYPES: Record<PostType, string> = {
  IMAGE: "Image",
  TEXT: "Text",
  VIDEO: "Video",
  CAROUSEL: "Carousel",
  STORY: "Story"
};

const STATUS_OPTIONS: Record<PostStatus, string> = {
  Published: "Published",
  Scheduled: "Scheduled",
  Failed: "Failed",
  Draft: "Draft"
};

export default function Filters({
  posts,
  filters,
  onFilterChange,
  onClearFilters
}: FiltersProps) {
  const [showAdvanced, setShowAdvanced] = useState(false);
  
  // Get unique values
  const brands = [...new Set(posts.map((p) => p.brandName).filter(Boolean))] as string[];
  const platforms = Object.keys(PLATFORM_CONFIG);
  const statuses = Object.values(STATUS_OPTIONS);
  const types = Object.values(POST_TYPES);
  
  const hasActiveFilters = 
    filters.search ||
    filters.brand ||
    filters.platform ||
    filters.status ||
    filters.type ||
    filters.dateFrom ||
    filters.dateTo ||
    filters.minLikes > 0 ||
    filters.minComments > 0 ||
    filters.minShares > 0;
  
  const handleInputChange = (key: keyof typeof filters, value: string | number) => {
    onFilterChange({ [key]: value });
  };

  const handleNumberInput = (key: keyof typeof filters, value: string) => {
    const numValue = value === "" ? 0 : parseInt(value, 10);
    onFilterChange({ [key]: isNaN(numValue) ? 0 : numValue });
  };

  return (
    <div className="space-y-4">
      {/* Main Filter Bar */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-xl border border-outline-variant/30 px-5 py-3 flex flex-wrap items-center gap-x-6 gap-y-3 shadow-sm">
        <div className="flex items-center gap-2 text-outline pr-4 border-r border-outline-variant/30">
          <span className="material-symbols-outlined text-[16px]">filter_list</span>
          <span className="text-label-sm font-semibold">Filters</span>
        </div>
        
        {/* Search */}
        <div className="relative">
          <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[14px]">search</span>
          <input
            type="text"
            value={filters.search}
            onChange={(e) => handleInputChange("search", e.target.value)}
            placeholder="Search posts..."
            className="w-48 bg-surface-container-low border border-outline-variant/20 rounded-lg pl-9 pr-4 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
          />
        </div>

        {/* Platform Filter */}
        <select
          value={filters.platform}
          onChange={(e) => handleInputChange("platform", e.target.value)}
          className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
        >
          <option value="">All Platforms</option>
          {platforms.map((key) => (
            <option key={key} value={key}>
              {PLATFORM_CONFIG[key]?.label || key}
            </option>
          ))}
        </select>

        {/* Brand Filter */}
        <select
          value={filters.brand}
          onChange={(e) => handleInputChange("brand", e.target.value)}
          className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
        >
          <option value="">All Brands</option>
          {brands.map((brand) => (
            <option key={brand} value={brand}>
              {brand}
            </option>
          ))}
        </select>

        {/* Status Filter */}
        <select
          value={filters.status}
          onChange={(e) => handleInputChange("status", e.target.value)}
          className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
        >
          <option value="">All Statuses</option>
          {statuses.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>

        {/* Date Range */}
        <div className="flex items-center gap-2 bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5">
          <span className="material-symbols-outlined text-[14px] text-outline">calendar_today</span>
          <input
            type="date"
            value={filters.dateFrom}
            onChange={(e) => handleInputChange("dateFrom", e.target.value)}
            className="bg-transparent border-none text-[11px] text-on-surface outline-none w-28"
            placeholder="From"
          />
          <span className="text-outline/40">–</span>
          <input
            type="date"
            value={filters.dateTo}
            onChange={(e) => handleInputChange("dateTo", e.target.value)}
            className="bg-transparent border-none text-[11px] text-on-surface outline-none w-28"
            placeholder="To"
          />
        </div>

        {/* Advanced Toggle */}
        <button
          onClick={() => setShowAdvanced(!showAdvanced)}
          className="flex items-center gap-1 text-label-sm text-primary font-semibold hover:text-primary/80 transition-colors ml-auto"
        >
          <span className="material-symbols-outlined text-[14px]">
            {showAdvanced ? "expand_less" : "expand_more"}
          </span>
          Advanced Filters
        </button>

        {/* Clear Filters */}
        {hasActiveFilters && (
          <button
            onClick={onClearFilters}
            className="text-label-sm text-primary font-semibold hover:underline flex items-center gap-1"
          >
            <span className="material-symbols-outlined text-[14px]">clear_all</span>
            Clear All
          </button>
        )}
      </div>

      {/* Advanced Filters */}
      {showAdvanced && (
        <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-xl border border-outline-variant/30 px-5 py-4 grid grid-cols-1 md:grid-cols-4 gap-4 shadow-sm">
          {/* Post Type */}
          <div className="space-y-1">
            <label className="text-label-xs text-outline uppercase font-semibold">Post Type</label>
            <select
              value={filters.type}
              onChange={(e) => handleInputChange("type", e.target.value)}
              className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
            >
              <option value="">All Types</option>
              {types.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>

          {/* Minimum Likes */}
          <div className="space-y-1">
            <label className="text-label-xs text-outline uppercase font-semibold">Min Likes</label>
            <div className="flex items-center gap-2">
              <input
                type="number"
                min="0"
                value={filters.minLikes || ""}
                onChange={(e) => handleNumberInput("minLikes", e.target.value)}
                className="flex-1 bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="0"
              />
              <span className="text-label-xs text-outline">+</span>
            </div>
          </div>

          {/* Minimum Comments */}
          <div className="space-y-1">
            <label className="text-label-xs text-outline uppercase font-semibold">Min Comments</label>
            <div className="flex items-center gap-2">
              <input
                type="number"
                min="0"
                value={filters.minComments || ""}
                onChange={(e) => handleNumberInput("minComments", e.target.value)}
                className="flex-1 bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="0"
              />
              <span className="text-label-xs text-outline">+</span>
            </div>
          </div>

          {/* Minimum Shares */}
          <div className="space-y-1">
            <label className="text-label-xs text-outline uppercase font-semibold">Min Shares</label>
            <div className="flex items-center gap-2">
              <input
                type="number"
                min="0"
                value={filters.minShares || ""}
                onChange={(e) => handleNumberInput("minShares", e.target.value)}
                className="flex-1 bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="0"
              />
              <span className="text-label-xs text-outline">+</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
