"use client";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils/cn";

type SingleDeleteProps = {
  mode: "single";
  id: string;
  title: string;
  isDeleting: boolean;
  onConfirm: (id: string) => void;
  onCancel: () => void;
};

type BatchDeleteProps = {
  mode: "batch";
  ids: string[];
  titles: string[];
  isDeleting: boolean;
  onConfirm: (ids: string[]) => void;
  onCancel: () => void;
};

type Props = SingleDeleteProps | BatchDeleteProps;

export function DeleteConversationDialog(props: Props) {
  const { isDeleting, onCancel } = props;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="w-full max-w-md rounded-2xl bg-card p-6 shadow-panel">
        <div className="mb-4 flex items-center gap-2">
          <span className="text-xl">&#x26A0;&#xFE0F;</span>
          <h2 className="text-lg font-semibold">
            {props.mode === "batch"
              ? `Xoá ${props.ids.length} cuộc trò chuyện?`
              : "Xoá cuộc trò chuyện?"}
          </h2>
        </div>

        <div className="mb-6 space-y-3">
          {props.mode === "batch" ? (
            <>
              <p className="text-sm text-muted-foreground">
                Bạn có chắc chắn muốn xoá {props.ids.length} cuộc trò chuyện sau?
              </p>
              <ul className="max-h-40 space-y-1 overflow-y-auto text-sm">
                {props.titles.slice(0, 5).map((t, i) => (
                  <li key={i} className="truncate rounded-md bg-muted px-2 py-1">
                    {t || "Untitled"}
                  </li>
                ))}
                {props.titles.length > 5 && (
                  <li className="px-2 py-1 text-gray-500">
                    ...và {props.titles.length - 5} cuộc khác
                  </li>
                )}
              </ul>
            </>
          ) : (
            <p className="text-sm text-muted-foreground">
              Bạn có chắc chắn muốn xoá &ldquo;{props.title}&rdquo;?
            </p>
          )}
          <p className="text-sm font-medium text-red-500">
            Hành động này không thể hoàn tác.
          </p>
        </div>

        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={onCancel} disabled={isDeleting}>
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={() => {
              if (props.mode === "batch") {
                props.onConfirm(props.ids);
              } else {
                props.onConfirm(props.id);
              }
            }}
            disabled={isDeleting}
            className={cn(isDeleting && "pointer-events-none opacity-50")}
          >
            {isDeleting ? (
              <span className="flex items-center gap-2">
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                Deleting...
              </span>
            ) : props.mode === "batch" ? (
              "Delete All"
            ) : (
              "Delete"
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
