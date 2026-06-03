"use client";

import { useState, useCallback } from "react";
import { useConversations } from "@/features/ai/hooks/use-conversations";
import { useDeleteConversation } from "@/features/ai/hooks/use-delete-conversation";
import { useBatchDelete } from "@/features/ai/hooks/use-batch-delete";
import { DeleteConversationDialog } from "@/features/ai/components/delete-conversation-dialog";
import { BatchDeleteToolbar } from "@/features/ai/components/batch-delete-toolbar";
import { Button } from "@/components/ui/button";
import type { ConversationResponseDto } from "@/features/ai/types/conversation";

const AD_TYPE_LABELS: Record<number, string> = {
  0: "TextOnly",
  1: "ImageText",
  2: "VideoText",
};

function formatRelativeTime(dateStr: string): string {
  const now = Date.now();
  const then = new Date(dateStr).getTime();
  const diffMs = now - then;
  const diffSec = Math.floor(diffMs / 1000);
  if (diffSec < 60) return "Vài giây trước";
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin} phút trước`;
  const diffHour = Math.floor(diffMin / 60);
  if (diffHour < 24) return `${diffHour} giờ trước`;
  const diffDay = Math.floor(diffHour / 24);
  if (diffDay < 7) return `${diffDay} ngày trước`;
  return new Date(dateStr).toLocaleDateString("vi-VN");
}

export default function ConversationHistoryPage() {
  const { data, isLoading, isError, refetch } = useConversations({
    pageSize: 50,
  });
  const singleDelete = useDeleteConversation();
  const batchDelete = useBatchDelete();

  const [selectMode, setSelectMode] = useState(false);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [failedIds, setFailedIds] = useState<Set<string>>(new Set());
  const [failedErrors, setFailedErrors] = useState<Map<string, string>>(new Map());

  const conversations = data?.items ?? [];

  const toggleSelection = useCallback((id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const handleSelectAll = useCallback(
    (checked: boolean) => {
      if (checked) {
        setSelectedIds(new Set(conversations.map((c) => c.id)));
      } else {
        setSelectedIds(new Set());
      }
    },
    [conversations],
  );

  const handleBatchDeleteConfirm = useCallback(
    async (ids: string[]) => {
      const result = await batchDelete.executeBatchDelete(ids);
      setFailedIds(new Set(result.failed.map((f) => f.id)));
      setFailedErrors(
        new Map(result.failed.map((f) => [f.id, f.error])),
      );
      if (result.succeeded.length > 0) {
        setSelectedIds(new Set());
        setSelectMode(false);
      }
    },
    [batchDelete],
  );

  const handleManageToggle = useCallback(() => {
    if (selectMode) {
      setSelectMode(false);
      setSelectedIds(new Set());
    } else {
      setSelectMode(true);
    }
  }, [selectMode]);

  const handleDeleteClick = useCallback(
    (conv: ConversationResponseDto) => {
      singleDelete.requestDelete(conv.id, conv.title ?? "Untitled");
    },
    [singleDelete],
  );

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-8 w-48 animate-pulse rounded bg-muted" />
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-32 animate-pulse rounded-2xl bg-muted" />
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <p className="text-muted-foreground">Không thể tải danh sách cuộc trò chuyện.</p>
        <Button variant="outline" onClick={() => refetch()}>
          Retry
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Conversation History</h1>
        <Button
          variant={selectMode ? "ghost" : "outline"}
          size="sm"
          onClick={handleManageToggle}
        >
          {selectMode ? "Cancel" : "Manage"}
        </Button>
      </div>

      {selectMode && (
        <BatchDeleteToolbar
          selectedCount={selectedIds.size}
          totalCount={conversations.length}
          isDeleting={batchDelete.isDeleting}
          onDeleteSelected={() => {
            const selected = conversations.filter((c) => selectedIds.has(c.id));
            batchDelete.requestBatchDelete(
              selected.map((c) => c.id),
              selected.map((c) => c.title ?? "Untitled"),
            );
          }}
          onCancel={() => {
            setSelectMode(false);
            setSelectedIds(new Set());
          }}
          onSelectAll={handleSelectAll}
        />
      )}

      {conversations.length === 0 ? (
        <div className="flex flex-col items-center gap-4 py-20">
          <p className="text-muted-foreground">Chưa có cuộc trò chuyện nào.</p>
          <Button variant="primary" asChild>
            <a href="/ai/chat">Bắt đầu chat ngay</a>
          </Button>
        </div>
      ) : (
        <div className="space-y-3">
          {conversations.map((conv) => {
            const isSelected = selectedIds.has(conv.id);
            const hasFailed = failedIds.has(conv.id);
            const failedError = failedErrors.get(conv.id);

            return (
              <div
                key={conv.id}
                className={`relative rounded-2xl border bg-card p-4 shadow-panel transition-colors ${
                  hasFailed
                    ? "border-red-400"
                    : isSelected
                      ? "border-primary"
                      : "border-border"
                } ${selectMode ? "cursor-pointer" : ""}`}
                onClick={selectMode ? () => toggleSelection(conv.id) : undefined}
              >
                {selectMode && (
                  <div className="absolute left-4 top-1/2 -translate-y-1/2">
                    <input
                      type="checkbox"
                      className="h-4 w-4 rounded border-gray-300"
                      checked={isSelected}
                      onChange={() => toggleSelection(conv.id)}
                    />
                  </div>
                )}

                <div className={selectMode ? "ml-8" : ""}>
                  <div className="mb-1 flex items-start justify-between">
                    <div className="min-w-0 flex-1">
                      <h3 className="truncate text-base font-medium">
                        {conv.title || "Untitled"}
                      </h3>
                      <div className="mt-1 flex flex-wrap gap-2 text-xs text-muted-foreground">
                        {conv.brandName && (
                          <span className="rounded-md bg-muted px-2 py-0.5">
                            {conv.brandName}
                          </span>
                        )}
                        {conv.productName && (
                          <span className="rounded-md bg-muted px-2 py-0.5">
                            {conv.productName}
                          </span>
                        )}
                        <span className="rounded-md bg-muted px-2 py-0.5">
                          {AD_TYPE_LABELS[conv.adType] ?? conv.adType}
                        </span>
                      </div>
                    </div>
                    <span className="ml-2 shrink-0 text-xs text-muted-foreground">
                      {conv.lastMessageAt ? formatRelativeTime(conv.lastMessageAt) : ""}
                    </span>
                  </div>

                  {conv.lastMessage && (
                    <p className="mb-3 line-clamp-2 text-sm text-muted-foreground">
                      {conv.lastMessage}
                    </p>
                  )}

                  <div className="flex items-center justify-between">
                    <span className="text-xs text-muted-foreground">
                      {conv.messageCount} messages
                    </span>
                    {!selectMode && (
                      <div className="flex gap-2">
                        <Button variant="primary" size="sm" asChild>
                          <a href={`/ai/chat?id=${conv.id}`}>Continue</a>
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDeleteClick(conv);
                          }}
                          disabled={singleDelete.isDeleting}
                        >
                          🗑️
                        </Button>
                      </div>
                    )}
                  </div>

                  {hasFailed && failedError && (
                    <p className="mt-2 text-xs text-red-500" title={failedError}>
                      Failed: {failedError}
                    </p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {singleDelete.confirm && (
        <DeleteConversationDialog
          mode="single"
          id={singleDelete.confirm.id}
          title={singleDelete.confirm.title}
          isDeleting={singleDelete.isDeleting}
          onConfirm={singleDelete.executeDelete}
          onCancel={singleDelete.cancelDelete}
        />
      )}

      {batchDelete.confirm && (
        <DeleteConversationDialog
          mode="batch"
          ids={batchDelete.confirm.ids}
          titles={batchDelete.confirm.titles}
          isDeleting={batchDelete.isDeleting}
          onConfirm={handleBatchDeleteConfirm}
          onCancel={batchDelete.cancelBatchDelete}
        />
      )}
    </div>
  );
}
