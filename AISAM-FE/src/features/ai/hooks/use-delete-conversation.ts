"use client";

import { useState, useCallback } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { conversationApi } from "@/features/ai/api/conversation-api";
import { useToastStore } from "@/stores/toast-store";

export type DeleteConfirmState = {
  id: string;
  title: string;
} | null;

export function useDeleteConversation() {
  const queryClient = useQueryClient();
  const toast = useToastStore((s) => s.push);
  const [confirm, setConfirm] = useState<DeleteConfirmState>(null);

  const mutation = useMutation({
    mutationFn: (id: string) => conversationApi.delete(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["conversations"] });
      toast({ title: "Đã xoá cuộc trò chuyện.", tone: "success" });
    },
    onError: () => {
      toast({ title: "Không thể xoá cuộc trò chuyện. Vui lòng thử lại.", tone: "error" });
    },
    onSettled: () => {
      setConfirm(null);
    },
  });

  const requestDelete = useCallback((id: string, title: string) => {
    setConfirm({ id, title });
  }, []);

  const executeDelete = useCallback(
    (id: string) => {
      mutation.mutate(id);
    },
    [mutation],
  );

  const cancelDelete = useCallback(() => {
    setConfirm(null);
  }, []);

  return {
    isDeleting: mutation.isPending,
    confirm,
    requestDelete,
    executeDelete,
    cancelDelete,
    setConfirm,
  };
}
