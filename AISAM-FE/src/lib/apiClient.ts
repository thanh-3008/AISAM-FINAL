import { getToken, refreshAccessToken, removeToken, removeRefreshToken, ensureValidToken } from "./auth";
import { getApiBaseUrl } from "./apiBaseUrl";
import { getStoredActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";

const API_URL = getApiBaseUrl();

type ApiOptions = RequestInit & {
  data?: unknown;
};

function isValidGuid(str: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);
}

async function buildHeaders(customHeaders?: Record<string, string>) {
  const token = getToken();
  let workspace = getStoredActiveWorkspace();
  if (workspace && !isValidGuid(workspace.id)) {
    clearActiveWorkspace();
    workspace = null;
  }
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
    ...(customHeaders || {}),
  };
  return { headers, token };
}

const ERROR_MAP: Record<string, string> = {
  "Missing or invalid X-Workspace-Id header.": "Chưa chọn Workspace. Vào Overview để chọn workspace.",
  "You are not a member of this workspace.": "Bạn không phải thành viên của workspace này.",
  "Workspace not found.": "Workspace không tồn tại.",
};

async function handleResponse<TResponse>(response: Response): Promise<TResponse> {
  const result = await response.json().catch(() => null);
  if (!response.ok) {
    const errorMessage = result?.message || response.statusText || "Đã có lỗi xảy ra";
    if (errorMessage === "Authentication is required.") {
      removeToken();
      removeRefreshToken();
      clearActiveWorkspace();
    }
    throw new Error(ERROR_MAP[errorMessage] || errorMessage);
  }
  return result as TResponse;
}

async function retryWithRefresh<TResponse>(endpoint: string, config: RequestInit): Promise<TResponse> {
  const newToken = await refreshAccessToken();
  if (!newToken) {
    throw new Error("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
  }
  const workspace = getStoredActiveWorkspace();
  const newHeaders: Record<string, string> = {
    ...(config.headers as Record<string, string> || {}),
    Authorization: `Bearer ${newToken}`,
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
  };
  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  return handleResponse<TResponse>(retryResponse);
}

export async function apiClient<TResponse = unknown>(endpoint: string, options: ApiOptions = {}): Promise<TResponse> {
  await ensureValidToken();
  const { data, headers: customHeaders, ...customConfig } = options;
  const { headers, token } = await buildHeaders(customHeaders as Record<string, string> | undefined);

  const config: RequestInit = {
    method: data ? "POST" : "GET",
    body: data ? JSON.stringify(data) : undefined,
    headers: {
      "Content-Type": "application/json",
      ...headers,
    },
    cache: "no-store",
    ...customConfig,
  };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh<TResponse>(endpoint, config);
  }

  return handleResponse<TResponse>(response);
}

export async function apiFetch<TResponse = unknown>(endpoint: string, options: RequestInit = {}): Promise<TResponse> {
  await ensureValidToken();
  const { headers, token } = await buildHeaders(options.headers as Record<string, string> | undefined);

  const config: RequestInit = { ...options, headers };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh<TResponse>(endpoint, config);
  }

  return handleResponse<TResponse>(response);
}
