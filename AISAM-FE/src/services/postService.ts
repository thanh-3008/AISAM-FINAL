import { apiClient } from "@/lib/apiClient";

export type PostStatus = string;

export interface PostItem {
  id: string;
  contentId: string;
  integrationId: string;
  externalPostId: string | null;
  publishedAt: string;
  status: PostStatus;
  contentTitle: string | null;
  brandName: string | null;
  platform: string | null;
  type: string | null;
  caption: string | null;
}

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PostFilters {
  page?: number;
  pageSize?: number;
  brandId?: string;
  status?: PostStatus;
}

export async function fetchPosts(params?: PostFilters): Promise<PagedResult<PostItem>> {
  const query = new URLSearchParams();
  if (params?.page) query.set("page", String(params.page));
  if (params?.pageSize) query.set("pageSize", String(params.pageSize));
  if (params?.brandId) query.set("brandId", params.brandId);
  if (params?.status) query.set("status", params.status);

  const res: GenericResponse<PagedResult<PostItem>> = await apiClient(`/posts?${query.toString()}`);
  if (res?.data) return res.data;
  return { data: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false };
}

export async function fetchPost(id: string): Promise<PostItem | null> {
  const res: GenericResponse<PostItem> = await apiClient(`/posts/${id}`);
  if (res?.data) return res.data;
  return null;
}

export async function deletePost(id: string): Promise<boolean> {
  const res: GenericResponse<null> = await apiClient(`/posts/${id}`, { method: "DELETE" });
  return res?.success === true;
}
