import { getToken, refreshAccessToken, removeToken, removeRefreshToken, ensureValidToken } from "./auth";
import { getStoredActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile, clearActiveProfile } from "@/stores/profile-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

type ApiOptions = RequestInit & {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  data?: any;
};

function isValidGuid(str: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);
}

async function buildHeaders(customHeaders?: Record<string, string>) {
  let token = getToken();
  let workspace = getStoredActiveWorkspace();
  let profile = getStoredActiveProfile();
  if (workspace && !isValidGuid(workspace.id)) {
    clearActiveWorkspace();
    workspace = null;
  }
  if (profile && !isValidGuid(profile.id)) {
    clearActiveProfile();
    profile = null;
  }
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
    ...(profile ? { "X-Profile-Id": profile.id } : {}),
    ...(customHeaders || {}),
  };
  return { headers, token };
}

const ERROR_MAP: Record<string, string> = {
  "Missing or invalid X-Workspace-Id header.": "Chưa chọn Workspace. Vào Overview để chọn workspace.",
  "Missing or invalid X-Profile-Id header.": "Chưa chọn Profile. Vào Overview để chọn workspace.",
  "You are not a member of this workspace.": "Bạn không phải thành viên của workspace này.",
  "Workspace not found.": "Workspace không tồn tại.",
  "Profile not found.": "Profile không tồn tại. Đang chuyển hướng...",
};

async function handleResponse(response: Response) {
  const result = await response.json().catch(() => null);
  if (!response.ok) {
    const errorMessage = result?.message || response.statusText || "Đã có lỗi xảy ra";
    if (errorMessage === "Authentication is required.") {
      removeToken();
      removeRefreshToken();
      clearActiveWorkspace();
      clearActiveProfile();
    }
    if (response.status === 404 && errorMessage === "Profile not found.") {
      clearActiveProfile();
      clearActiveWorkspace();
    }
    throw new Error(ERROR_MAP[errorMessage] || errorMessage);
  }
  return result;
}

async function retryWithRefresh(endpoint: string, config: RequestInit): Promise<any> {
  const newToken = await refreshAccessToken();
  if (!newToken) {
    throw new Error("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
  }
  const workspace = getStoredActiveWorkspace();
  const profile = getStoredActiveProfile();
  const newHeaders: Record<string, string> = {
    ...(config.headers as Record<string, string> || {}),
    Authorization: `Bearer ${newToken}`,
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
    ...(profile ? { "X-Profile-Id": profile.id } : {}),
  };
  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  return handleResponse(retryResponse);
}

export async function apiClient(endpoint: string, options: ApiOptions = {}) {
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
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response);
}

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  await ensureValidToken();
  const { headers, token } = await buildHeaders(options.headers as Record<string, string> | undefined);

  const config: RequestInit = { ...options, headers };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response);
}
