export type GenericResponse<T> = {
  success: boolean;
  message?: string | null;
  statusCode: number;
  data?: T | null;
  error?: {
    errorCode?: string | null;
    errorMessage?: string | null;
    stackTrace?: string | null;
    validationErrors?: Record<string, string[]>;
  } | null;
  timestamp: string;
};

export type ApiError = {
  message: string;
  statusCode: number;
  errorCode?: string | null;
  validationErrors?: Record<string, string[]>;
};
