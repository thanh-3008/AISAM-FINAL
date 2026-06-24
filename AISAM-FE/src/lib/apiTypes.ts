export interface ApiResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: {
    errorCode?: string;
    errorMessage?: string;
    validationErrors?: Record<string, string[]>;
  };
  timestamp?: string;
}

export interface PagedResult<T> {
  data?: T[];
  items?: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public errorCode?: string,
    public validationErrors?: Record<string, string[]>
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export function getPagedItems<T>(paged: PagedResult<T>): T[] {
  return paged.items ?? paged.data ?? [];
}
