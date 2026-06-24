export interface GenericResponse<T> {
  success?: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

export interface PagedResult<T> {
  data: T[];
  page?: number;
  pageSize?: number;
  total?: number;
  totalItems?: number;
  totalCount?: number;
}

export function unwrapApiData<T>(response: GenericResponse<T> | T | null | undefined): T | null {
  if (!response) return null;
  if (typeof response === "object" && "data" in response) {
    return (response as GenericResponse<T>).data ?? null;
  }
  return response as T;
}

export function normalizeListResponse<T>(response: GenericResponse<PagedResult<T> | T[]> | PagedResult<T> | T[] | null | undefined): T[] {
  const data = unwrapApiData(response);
  if (Array.isArray(data)) return data;
  if (data && typeof data === "object" && Array.isArray((data as PagedResult<T>).data)) {
    return (data as PagedResult<T>).data;
  }
  return [];
}
