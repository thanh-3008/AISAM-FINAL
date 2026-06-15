import { getToken, refreshAccessToken } from "./auth";
import { ApiError } from "./apiTypes";
import { getStoredActiveProfile } from "@/stores/profile-store";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

type ApiOptions = RequestInit & {
  data?: unknown;
  rawBody?: boolean;
};

type LooseApiResponse = {
  success?: boolean;
  message?: string | null;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  data?: any;
};

let refreshPromise: Promise<string | null> | null = null;

async function safeRefreshToken(): Promise<string | null> {
  if (refreshPromise) return refreshPromise;

  refreshPromise = refreshAccessToken().finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

function normalizeHeaders(headers?: HeadersInit): Record<string, string> {
  if (!headers) return {};
  if (headers instanceof Headers) return Object.fromEntries(headers.entries());
  if (Array.isArray(headers)) return Object.fromEntries(headers);
  return headers;
}

function buildHeaders(customHeaders?: HeadersInit) {
  const token = getToken();
  const profile = getStoredActiveProfile();
  const workspace = getStoredActiveWorkspace();
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(profile?.id ? { "X-Profile-Id": profile.id } : {}),
    ...(workspace?.id ? { "X-Workspace-Id": workspace.id } : {}),
    ...normalizeHeaders(customHeaders),
  };
  return { headers, token };
}

async function handleResponse<T>(response: Response): Promise<T> {
  const text = await response.text();
  let result: unknown = null;

  try {
    result = text ? JSON.parse(text) : null;
  } catch {
    if (!response.ok) {
      throw new ApiError(
        `Server returned ${response.status}: ${text.slice(0, 200)}`,
        response.status
      );
    }
    return null as T;
  }

  if (!response.ok) {
    const payload = result as {
      message?: string;
      error?: {
        errorCode?: string;
        errorMessage?: string;
        validationErrors?: Record<string, string[]>;
      };
    } | null;

    throw new ApiError(
      payload?.message ||
        payload?.error?.errorMessage ||
        response.statusText ||
        "Request failed",
      response.status,
      payload?.error?.errorCode,
      payload?.error?.validationErrors
    );
  }

  return result as T;
}

async function retryWithRefresh<T>(endpoint: string, config: RequestInit): Promise<T> {
  const newToken = await safeRefreshToken();
  if (!newToken) throw new ApiError("Session expired", 401);

  const newHeaders = {
    ...normalizeHeaders(config.headers),
    Authorization: `Bearer ${newToken}`,
  };

  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  return handleResponse<T>(retryResponse);
}

export async function apiClient<T = LooseApiResponse>(endpoint: string, options: ApiOptions = {}): Promise<T> {
  const { data, headers: customHeaders, rawBody, ...customConfig } = options;
  const { headers, token } = buildHeaders(customHeaders);
  const isFormData = typeof FormData !== "undefined" && data instanceof FormData;
  const shouldStringify = data !== undefined && !rawBody && !isFormData;

  const config: RequestInit = {
    method: customConfig.method || (data !== undefined ? "POST" : "GET"),
    body: rawBody || isFormData ? (data as BodyInit) : shouldStringify ? JSON.stringify(data) : undefined,
    headers: {
      ...(shouldStringify ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
    cache: "no-store",
    ...customConfig,
  };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh<T>(endpoint, config);
  }

  return handleResponse<T>(response);
}

export async function apiFetch<T = LooseApiResponse>(endpoint: string, options: RequestInit = {}): Promise<T> {
  return apiClient<T>(endpoint, options);
}
