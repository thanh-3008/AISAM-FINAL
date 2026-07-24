import { getToken, refreshAccessToken, removeToken, removeRefreshToken, ensureValidToken } from "./auth";
import { getStoredActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile } from "@/stores/profile-store";

export const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

type ApiOptions = RequestInit & {
  data?: any;
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
  const profile = getStoredActiveProfile();
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(workspace ? { "X-Workspace-Id": workspace.id } : {}),
    ...(profile && isValidGuid(profile.id) ? { "X-Profile-Id": profile.id } : {}),
    ...(customHeaders || {}),
  };
  return { headers, token };
}

const ERROR_MAP: Record<string, string> = {
  "Missing or invalid X-Workspace-Id header.": "Chưa chọn Workspace. Vào Overview để chọn workspace.",
  "Missing or invalid X-Profile-Id header.": "Chưa chọn Profile cho tính năng này.",
  "You are not a member of this workspace.": "Bạn không phải thành viên của workspace này.",
  "Profile does not belong to active workspace.": "Profile không thuộc workspace đang chọn.",
};

async function handleResponse(response: Response) {
  let result: any = null;
  let text = "";
  try {
    text = await response.text();
    result = text ? JSON.parse(text) : null;
  } catch {
    // If JSON parsing fails, result remains null, but we still have text
  }

  if (!response.ok) {
    let errorMessage = "Đã có lỗi xảy ra";

    if (result) {
      const validationErrors = result.error?.validationErrors;
      if (validationErrors && typeof validationErrors === "object") {
        const values = Object.values(validationErrors).flat().filter(Boolean);
        if (values.length > 0) {
          errorMessage = values.join(", ");
        }
      }
      if (errorMessage === "Đã có lỗi xảy ra" && typeof result.message === "string") {
        errorMessage = result.message;
      }
      if (errorMessage === "Đã có lỗi xảy ra" && result.errors && typeof result.errors === "object") {
        const values = Object.values(result.errors).flat().filter(Boolean);
        if (values.length > 0) {
          errorMessage = values.join(", ");
        }
      }
      if (errorMessage === "Đã có lỗi xảy ra" && result.title && typeof result.title === "string") {
        errorMessage = result.title;
      }
      if (errorMessage === "Đã có lỗi xảy ra" && result.detail && typeof result.detail === "string") {
        errorMessage = result.detail;
      }
    }

    if (!errorMessage || errorMessage === "Đã có lỗi xảy ra") {
      if (text) {
        errorMessage = text;
      } else if (response.statusText) {
        errorMessage = `${response.status} ${response.statusText}`;
      } else {
        errorMessage = `Request failed (${response.status})`;
      }
    }

    if (response.status === 401 || errorMessage === "Authentication is required.") {
      const isOnLoginPage = typeof window !== "undefined" && window.location.pathname === "/login";
      if (!isOnLoginPage) {
        removeToken();
        removeRefreshToken();
        clearActiveWorkspace();
        if (typeof window !== "undefined") {
          document.cookie = "aisam_role=; path=/; max-age=0";
          window.location.href = "/login";
        }
      }
      return;
    }

    const mappedError = ERROR_MAP[errorMessage];
    throw new Error(mappedError ?? String(errorMessage));
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
    ...(profile && isValidGuid(profile.id) ? { "X-Profile-Id": profile.id } : {}),
  };
  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  return handleResponse(retryResponse);
}

export async function apiClient(endpoint: string, options: ApiOptions = {}) {
  await ensureValidToken();
  const { data, headers: customHeaders, ...customConfig } = options;
  const { headers, token } = await buildHeaders(customHeaders as Record<string, string> | undefined);

  const hasJsonBody = data !== undefined && data !== null && !(data instanceof FormData);
  const config: RequestInit = {
    method: hasJsonBody ? "POST" : "GET",
    body: hasJsonBody ? JSON.stringify(data) : undefined,
    headers: {
      ...(hasJsonBody ? { "Content-Type": "application/json" } : {}),
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
