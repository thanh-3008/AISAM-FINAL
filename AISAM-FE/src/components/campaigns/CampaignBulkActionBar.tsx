"use client";

interface CampaignBulkActionBarProps {
  selectedCount: number;
  isLoading: boolean;
  onClear: () => void;
  onDelete: () => void;
}

export default function CampaignBulkActionBar({ selectedCount, isLoading, onClear, onDelete }: CampaignBulkActionBarProps) {
  if (selectedCount === 0) return null;

  return (
    <div className="rounded-2xl border border-primary/20 bg-primary/5 px-4 py-3 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
      <div className="flex items-center gap-3">
        <span className="w-9 h-9 rounded-xl bg-primary/10 text-primary flex items-center justify-center material-symbols-outlined text-[18px]">
          checklist
        </span>
        <div>
          <p className="text-label-sm font-bold text-on-surface">{selectedCount} campaign{selectedCount > 1 ? "s" : ""} selected</p>
          <p className="text-label-xs text-outline">Bulk actions apply to the selected rows.</p>
        </div>
      </div>
      <div className="flex items-center gap-2">
        <button
          onClick={onDelete}
          disabled={isLoading}
          className="h-9 px-4 rounded-xl bg-danger-red/10 text-danger-red text-label-sm font-semibold hover:bg-danger-red/20 disabled:opacity-50 flex items-center gap-2"
        >
          <span className="material-symbols-outlined text-[16px]">delete</span>
          Delete
        </button>
        <button
          onClick={onClear}
          className="h-9 px-4 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container"
        >
          Clear
        </button>
      </div>
    </div>
  );
}
