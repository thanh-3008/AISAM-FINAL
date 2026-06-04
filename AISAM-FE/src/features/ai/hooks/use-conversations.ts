"use client";

import { useQuery } from "@tanstack/react-query";
import { conversationApi, type ConversationListParams } from "@/features/ai/api/conversation-api";

const CONVERSATIONS_KEY = "conversations";

export function useConversations(params: ConversationListParams = {}) {
  return useQuery({
    queryKey: [CONVERSATIONS_KEY, params],
    queryFn: () => conversationApi.list(params),
  });
}

export function useConversationDetail(id: string) {
  return useQuery({
    queryKey: [CONVERSATIONS_KEY, id],
    queryFn: () => conversationApi.detail(id),
    enabled: Boolean(id),
  });
}
