"use client";

import { useState, useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { conversationApi } from "@/features/ai/api/conversation-api";
import { useToastStore } from "@/stores/toast-store";

export type BatchDeleteConfirmState = {
  ids: string[];
  titles: string[];
} | null;

export type BatchDeleteResult = {
  succeeded: string[];
  failed: { id: string; error: string }[];
};

export function useBatchDelete() {
  const queryClient = useQueryClient();
  const toast = useToastStore((s) => s.push);
  const [confirm, setConfirm] = useState<BatchDeleteConfirmState>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const requestBatchDelete = useCallback((ids: string[], titles: string[]) => {
    setConfirm({ ids, titles });
  }, []);

  const executeBatchDelete = useCallback(
    async (ids: string[]): Promise<BatchDeleteResult> => {
      setIsDeleting(true);
      const results = await Promise.allSettled(
        ids.map((id) => conversationApi.delete(id)),
      );

      const succeeded: string[] = [];
      const failed: { id: string; error: string }[] = [];

      results.forEach((result, index) => {
        if (result.status === "fulfilled" && result.value) {
          succeeded.push(ids[index]);
        } else {
          failed.push({
            id: ids[index],
            error:
              result.status === "fulfilled"
                ? "Delete failed"
                : result.reason?.message ?? "Network error",
          });
        }
      });

      void queryClient.invalidateQueries({ queryKey: ["conversations"] });

      if (failed.length === 0) {
        toast({
          title: `Đã xoá ${succeeded.length} cuộc trò chuyện.`,
          tone: "success",
        });
      } else if (succeeded.length > 0) {
        toast({
          title: `Đã xoá ${succeeded.length}/${ids.length} cuộc trò chuyện. ${failed.length} cuộc không thể xoá.`,
          tone: "neutral",
        });
      } else {
        toast({
          title: "Không thể xoá cuộc trò chuyện nào.",
          tone: "error",
        });
      }

      setIsDeleting(false);
      setConfirm(null);
      return { succeeded, failed };
    },
    [queryClient, toast],
  );

  const cancelBatchDelete = useCallback(() => {
    setConfirm(null);
  }, []);

  return {
    isDeleting,
    confirm,
    requestBatchDelete,
    executeBatchDelete,
    cancelBatchDelete,
    setConfirm,
  };
}
