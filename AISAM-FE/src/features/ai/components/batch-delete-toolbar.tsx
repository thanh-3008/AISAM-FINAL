"use client";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils/cn";

type Props = {
  selectedCount: number;
  totalCount: number;
  isDeleting: boolean;
  onDeleteSelected: () => void;
  onCancel: () => void;
  onSelectAll: (checked: boolean) => void;
};

export function BatchDeleteToolbar({
  selectedCount,
  totalCount,
  isDeleting,
  onDeleteSelected,
  onCancel,
  onSelectAll,
}: Props) {
  const allSelected = selectedCount === totalCount && totalCount > 0;
  const someSelected = selectedCount > 0 && selectedCount < totalCount;

  return (
    <div className="flex items-center justify-between rounded-lg border bg-muted/50 px-4 py-2">
      <div className="flex items-center gap-3">
        <label className="flex cursor-pointer items-center gap-2 text-sm">
          <input
            type="checkbox"
            className="h-4 w-4 rounded border-gray-300"
            checked={allSelected}
            ref={(el) => {
              if (el) el.indeterminate = someSelected;
            }}
            onChange={(e) => onSelectAll(e.target.checked)}
          />
          <span className="text-muted-foreground">
            {selectedCount > 0
              ? `Selected ${selectedCount}`
              : "Select conversations"}
          </span>
        </label>
      </div>
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={onCancel}
          disabled={isDeleting}
        >
          Cancel
        </Button>
        <Button
          variant="danger"
          size="sm"
          onClick={onDeleteSelected}
          disabled={selectedCount === 0 || isDeleting}
          className={cn(
            selectedCount === 0 && "opacity-50",
            isDeleting && "pointer-events-none opacity-50",
          )}
        >
          {isDeleting ? (
            <span className="flex items-center gap-2">
              <span className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
              Deleting...
            </span>
          ) : (
            `Delete Selected (${selectedCount})`
          )}
        </Button>
      </div>
    </div>
  );
}
