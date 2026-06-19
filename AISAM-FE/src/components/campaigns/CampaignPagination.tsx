"use client";

interface CampaignPaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  isLoading: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

export default function CampaignPagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  isLoading,
  onPageChange,
  onPageSizeChange,
}: CampaignPaginationProps) {
  const start = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const end = Math.min(totalCount, page * pageSize);

  return (
    <div className="px-4 py-3 border-t border-outline-variant/10 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
      <p className="text-label-sm text-outline">
        Showing {start}-{end} of {totalCount}
      </p>
      <div className="flex items-center gap-2">
        <select
          value={pageSize}
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
          className="h-9 rounded-lg border border-outline-variant/30 bg-surface-container-lowest px-2 text-label-sm"
        >
          {[10, 25, 50, 100].map((size) => <option key={size} value={size}>{size} / page</option>)}
        </select>
        <button
          onClick={() => onPageChange(page - 1)}
          disabled={isLoading || page <= 1}
          className="w-9 h-9 rounded-lg border border-outline-variant/30 disabled:opacity-40 material-symbols-outlined text-[18px]"
          title="Previous page"
        >
          chevron_left
        </button>
        <span className="text-label-sm font-semibold text-on-surface min-w-16 text-center">
          {page} / {totalPages}
        </span>
        <button
          onClick={() => onPageChange(page + 1)}
          disabled={isLoading || page >= totalPages}
          className="w-9 h-9 rounded-lg border border-outline-variant/30 disabled:opacity-40 material-symbols-outlined text-[18px]"
          title="Next page"
        >
          chevron_right
        </button>
      </div>
    </div>
  );
}
