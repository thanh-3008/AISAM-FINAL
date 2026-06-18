import { getToken, getUserIdFromToken, refreshAccessToken, removeToken, removeRefreshToken, ensureValidToken } from "./auth";
import { getStoredActiveWorkspace, clearActiveWorkspace, storeActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile, clearActiveProfile, storeActiveProfile } from "@/stores/profile-store";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

type ApiOptions = RequestInit & {
  data?: any;
};

let profileFetchPromise: Promise<void> | null = null;
let profileFetchDone = false;

function isValidGuid(str: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);
}

async function ensureProfile() {
  if (profileFetchDone) return;
  if (profileFetchPromise) return profileFetchPromise;
  const token = getToken();
  const workspace = getStoredActiveWorkspace();
  if (!token || !workspace || !isValidGuid(workspace.id)) return;
  const profile = getStoredActiveProfile();
  if (profile && isValidGuid(profile.id)) { profileFetchDone = true; return; }
  const userId = getUserIdFromToken();
  if (!userId) return;
  profileFetchPromise = (async () => {
    try {
      const res = await fetch(`${API_URL}/profiles/user/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) return;
      const data = await res.json();
      if (data?.success && Array.isArray(data.data) && data.data.length > 0) {
        const p = data.data[0];
        storeActiveProfile({ id: p.id, name: p.name, profileType: p.profileType });
      }
    } catch { /* ignore */ } finally {
      profileFetchDone = true;
      profileFetchPromise = null;
    }
  })();
  return profileFetchPromise;
}

async function buildHeaders(customHeaders?: Record<string, string>) {
  const token = getToken();
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
  if (token && workspace && !profile) {
    await ensureProfile();
    profile = getStoredActiveProfile();
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
  "Missing or invalid X-Profile-Id header.": "Chưa chọn Profile cho tính năng này.",
  "You are not a member of this workspace.": "Bạn không phải thành viên của workspace này.",
  "Profile does not belong to active workspace.": "Profile không thuộc workspace đang chọn.",
  "The active subscription plan does not include this feature.": "Gói thuê bao hiện tại không hỗ trợ tính năng này.",
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
    }
    if (errorMessage === "Missing or invalid X-Profile-Id header.") {
      clearActiveProfile();
    }
    return { success: false, message: ERROR_MAP[errorMessage] || errorMessage, statusCode: response.status, data: null };
  }
  return result;
}

async function retryWithRefresh(endpoint: string, config: RequestInit): Promise<unknown> {
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

  const isJsonBody = typeof options.body === "string" && (options.body.startsWith("{") || options.body.startsWith("["));
  const config: RequestInit = {
    ...options,
    headers: {
      ...(isJsonBody ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
  };

  const response = await fetch(`${API_URL}${endpoint}`, config);

  if (response.status === 401 && token) {
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response);
}
