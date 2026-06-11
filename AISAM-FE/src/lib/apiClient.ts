import { getToken, refreshAccessToken } from "./auth";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

type ApiOptions = RequestInit & {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  data?: any;
};

async function buildHeaders(customHeaders?: Record<string, string>) {
  let token = getToken();
  const workspace = getStoredActiveWorkspace();
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
    ...(customHeaders || {}),
  };
  return { headers, token };
}

async function handleResponse(response: Response) {
  const result = await response.json().catch(() => null);
  if (!response.ok) {
    const errorMessage = result?.message || response.statusText || "Đã có lỗi xảy ra";
    throw new Error(errorMessage);
  }
  return result;
}

async function retryWithRefresh(endpoint: string, config: RequestInit): Promise<any> {
  const newToken = await refreshAccessToken();
  if (!newToken) throw new Error("Session expired");
  const workspace = getStoredActiveWorkspace();
  const newHeaders: Record<string, string> = {
    ...(config.headers as Record<string, string> || {}),
    Authorization: `Bearer ${newToken}`,
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
  };
  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  return handleResponse(retryResponse);
}

export async function apiClient(endpoint: string, options: ApiOptions = {}) {
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
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response);
}

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  const { headers, token } = await buildHeaders(options.headers as Record<string, string> | undefined);

  const config: RequestInit = { ...options, headers };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response);
}
