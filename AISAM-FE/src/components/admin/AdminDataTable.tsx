"use client";

import { useState } from "react";
import AdminEmptyState from "./AdminEmptyState";

interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => React.ReactNode;
  sortable?: boolean;
  sortKey?: string;
}

interface Props<T> {
  columns: Column<T>[];
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onSort?: (sortBy: string, descending: boolean) => void;
  emptyMessage?: string;
  isLoading?: boolean;
}

export default function AdminDataTable<T extends { id: string }>({
  columns, data, totalCount, page, pageSize, totalPages,
  onPageChange, onSort, emptyMessage = "No data found.", isLoading,
}: Props<T>) {
  const [sortBy, setSortBy] = useState<string | null>(null);
  const [sortDesc, setSortDesc] = useState(false);

  const handleSort = (col: Column<T>) => {
    if (!col.sortable || !col.sortKey) return;
    let nextDesc = false;
    if (sortBy === col.sortKey) {
      nextDesc = !sortDesc;
    }
    setSortBy(col.sortKey);
    setSortDesc(nextDesc);
    onSort?.(col.sortKey, nextDesc);
  };

  if (isLoading) {
    return (
      <div className="p-6 space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-12 bg-surface-container animate-pulse rounded-xl" />
        ))}
      </div>
    );
  }

  if (!data.length) {
    return <AdminEmptyState message={emptyMessage} icon="search_off" />;
  }

  const start = (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, totalCount);

  const pageNumbers: (number | "...")[] = [];
  if (totalPages <= 7) {
    for (let i = 1; i <= totalPages; i++) pageNumbers.push(i);
  } else {
    pageNumbers.push(1);
    if (page > 3) pageNumbers.push("...");
    const startPage = Math.max(2, page - 1);
    const endPage = Math.min(totalPages - 1, page + 1);
    for (let i = startPage; i <= endPage; i++) pageNumbers.push(i);
    if (page < totalPages - 2) pageNumbers.push("...");
    pageNumbers.push(totalPages);
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="bg-surface-container-low border-b border-outline-variant/20">
              {columns.map((col) => (
                <th
                  key={col.key}
                  onClick={() => handleSort(col)}
                  className={`px-6 py-4 text-label-xs text-on-surface-variant uppercase tracking-wider font-semibold ${
                    col.sortable ? "cursor-pointer hover:text-on-surface select-none" : ""
                  }`}
                >
                  <span className="inline-flex items-center gap-1">
                    {col.header}
                    {col.sortable && sortBy === col.sortKey && (
                      <span className="material-symbols-outlined text-[14px]">
                        {sortDesc ? "arrow_downward" : "arrow_upward"}
                      </span>
                    )}
                  </span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/10">
            {data.map((item) => (
              <tr key={item.id} className="hover:bg-surface-container/40 transition-colors">
                {columns.map((col) => (
                  <td key={col.key} className="px-6 py-4 text-body-sm text-on-surface">{col.render(item)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between px-6 py-3 border-t border-outline-variant/10">
          <p className="text-label-sm text-on-surface-variant">
            Showing {start}-{end} of {totalCount}
          </p>
          <div className="flex items-center gap-1">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="p-2 rounded-lg text-on-surface-variant hover:bg-surface-container disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-[18px]">chevron_left</span>
            </button>
            {pageNumbers.map((p, i) =>
              p === "..." ? (
                <span key={`dots-${i}`} className="w-8 text-center text-on-surface-variant text-body-sm">...</span>
              ) : (
                <button
                  key={p}
                  onClick={() => onPageChange(p)}
                  className={`w-8 h-8 rounded-lg text-body-sm font-medium transition-colors ${
                    p === page
                      ? "bg-primary text-on-primary"
                      : "text-on-surface-variant hover:bg-surface-container"
                  }`}
                >
                  {p}
                </button>
              )
            )}
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="p-2 rounded-lg text-on-surface-variant hover:bg-surface-container disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-[18px]">chevron_right</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
