import { apiRequest } from "@/lib/api/fetcher";
import type { ConversationDetailDto, ConversationResponseDto, PagedResult } from "@/features/ai/types/conversation";

export type ConversationListParams = {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
};

export const conversationApi = {
  list: (params: ConversationListParams = {}) => {
    const searchParams = new URLSearchParams();
    if (params.page) searchParams.set("page", String(params.page));
    if (params.pageSize) searchParams.set("pageSize", String(params.pageSize));
    if (params.searchTerm) searchParams.set("searchTerm", params.searchTerm);
    if (params.sortBy) searchParams.set("sortBy", params.sortBy);
    if (params.sortDescending !== undefined) searchParams.set("sortDescending", String(params.sortDescending));
    const qs = searchParams.toString();
    return apiRequest<PagedResult<ConversationResponseDto>>(`/api/conversations${qs ? `?${qs}` : ""}`, {
      method: "GET",
      auth: true,
    });
  },

  detail: (id: string) =>
    apiRequest<ConversationDetailDto>(`/api/conversations/${id}`, {
      method: "GET",
      auth: true,
    }),

  delete: (id: string) =>
    apiRequest<boolean>(`/api/conversations/${id}`, {
      method: "DELETE",
      auth: true,
    }),
};
