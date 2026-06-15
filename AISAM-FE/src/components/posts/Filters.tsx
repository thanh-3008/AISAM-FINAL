"use client";

import { useState, useMemo } from "react";
import { PostItem } from "@/services/postService";
import { PLATFORM_CONFIG } from "@/lib/contentConstants";

interface FiltersProps {
  posts: PostItem[];
  filters: {
    search: string;
    brand: string;
    status: string;
  };
  onFilterChange: (filters: Partial<FiltersProps["filters"]>) => void;
  onClearFilters: () => void;
}

export default function Filters({
  posts,
  filters,
  onFilterChange,
  onClearFilters
}: FiltersProps) {
  const brands = useMemo(() => [...new Set(posts.map((p) => p.brandName).filter(Boolean))] as string[], [posts]);

  const hasActiveFilters = filters.search || filters.brand || filters.status;

  const handleInputChange = (key: keyof typeof filters, value: string) => {
    onFilterChange({ [key]: value });
  };

  return (
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
        <option value="Published">Published</option>
        <option value="Draft">Draft</option>
      </select>

      {/* Clear Filters */}
      {hasActiveFilters && (
        <button
          onClick={onClearFilters}
          className="text-label-sm text-primary font-semibold hover:underline flex items-center gap-1 ml-auto"
        >
          <span className="material-symbols-outlined text-[14px]">clear_all</span>
          Clear All
        </button>
      )}
    </div>
  );
}