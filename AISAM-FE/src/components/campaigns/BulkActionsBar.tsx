"use client";

interface BulkActionsBarProps {
  selectedCount: number;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  onBulkDuplicate?: () => void;
  isLoading: boolean;
}

export default function BulkActionsBar({
  selectedCount,
  onClearSelection,
  onBulkDelete,
  onBulkDuplicate,
  isLoading,
}: BulkActionsBarProps) {
  if (selectedCount === 0) return null;

  return (
    <div className="bg-primary/5 backdrop-blur-sm rounded-2xl border border-primary/20 p-4 shadow-sm animate-in slide-in-from-top-2 duration-200">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
            <span className="material-symbols-outlined text-[20px]">checklist</span>
          </div>
          <div>
            <p className="text-label-sm font-bold text-on-surface">
              {selectedCount} campaign{selectedCount > 1 ? "s" : ""} selected
            </p>
            <p className="text-label-xs text-outline">Choose an action to perform</p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {onBulkDuplicate && (
            <button
              onClick={onBulkDuplicate}
              disabled={isLoading}
              className="flex items-center gap-1.5 px-4 py-2 bg-violet-50 hover:bg-violet-100 text-violet-600 rounded-xl text-[11px] font-semibold transition-all disabled:opacity-50"
            >
              <span className="material-symbols-outlined text-[14px]">content_copy</span>
              Duplicate Selected
            </button>
          )}
          <button
            onClick={onBulkDelete}
            disabled={isLoading}
            className="flex items-center gap-1.5 px-4 py-2 bg-danger-red/10 hover:bg-danger-red/20 text-danger-red rounded-xl text-[11px] font-semibold transition-all disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin" />
            ) : (
              <span className="material-symbols-outlined text-[14px]">delete</span>
            )}
            Delete Selected
          </button>
          <button
            onClick={onClearSelection}
            className="px-4 py-2 border border-outline-variant/30 hover:bg-surface-container rounded-xl text-[11px] font-semibold text-outline hover:text-on-surface transition-all"
          >
            Clear
          </button>
        </div>
      </div>
    </div>
  );
}
