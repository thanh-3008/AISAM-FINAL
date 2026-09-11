import { getToken, refreshAccessToken, removeToken, removeRefreshToken, ensureValidToken } from "./auth";
import { getStoredActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { getStoredActiveProfile } from "@/stores/profile-store";

export const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5027/api";

let isRedirectingToLogin = false;
let isLoggingOut = false;

export function setLoggingOut(value: boolean) {
  isLoggingOut = value;
}

export function resetRedirectState() {
  isRedirectingToLogin = false;
  isLoggingOut = false;
}

function redirectToLoginAndHalt(): Promise<never> {
  if (typeof window !== "undefined") {
    document.cookie = "aisam_role=; path=/; max-age=0";
    if (!isRedirectingToLogin && window.location.pathname !== "/login") {
      isRedirectingToLogin = true;
      window.location.href = "/login";
    }
  }
  return new Promise(() => {});
}

type ApiOptions = RequestInit & {
  data?: any;
};

const PUBLIC_AUTH_ENDPOINTS = [
  "/auth/login",
  "/auth/register",
  "/auth/forgot-password",
  "/auth/reset-password",
  "/auth/change-password-with-token",
  "/auth/verify-email",
  "/auth/resend-verification",
];

function isPublicAuthEndpoint(endpoint: string): boolean {
  return PUBLIC_AUTH_ENDPOINTS.some((publicEndpoint) => endpoint.startsWith(publicEndpoint));
}

function responsePath(response: Response): string {
  try {
    return new URL(response.url).pathname.replace(/^\/api/, "");
  } catch {
    return "";
  }
}

function isValidGuid(str: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);
}

async function buildHeaders(customHeaders?: Record<string, string>, includeAuth = true) {
  const token = includeAuth ? getToken() : null;
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
  "System error": "Đã có lỗi hệ thống. Vui lòng thử lại sau.",
};

async function handleResponse(response: Response, config: RequestInit) {
  let result: any = null;
  let text = "";
  try {
    text = await response.text();
    result = text ? JSON.parse(text) : null;
  } catch {
    // If JSON parsing fails, result remains null, but we still have text
  }

  assertWorkspace(config);
  if (!response.ok) {
    let errorMessage = "Đã có lỗi xảy ra";

    if (result) {
      if (typeof result.error === "string") {
        errorMessage = result.error;
      } else if (result.error) {
        const validationErrors = result.error.validationErrors;
        if (validationErrors && typeof validationErrors === "object" && validationErrors !== null) {
          const values = Object.values(validationErrors).flat().filter(Boolean);
          if (values.length > 0) {
            errorMessage = values.join(", ");
          }
        }
        if (errorMessage === "Đã có lỗi xảy ra" && typeof result.error.message === "string") {
          errorMessage = result.error.message;
        }
        if (errorMessage === "Đã có lỗi xảy ra" && typeof result.error.errorMessage === "string") {
          errorMessage = result.error.errorMessage;
        }
      }
      if (errorMessage === "Đã có lỗi xảy ra" && typeof result.message === "string") {
        errorMessage = result.message;
      }
      if (errorMessage === "Đã có lỗi xảy ra" && result.errors && typeof result.errors === "object" && result.errors !== null) {
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

    // Authentication state is determined by the HTTP status, not by an error
    // message. A 403 response must never clear a valid session, even if an
    // upstream service happens to use authentication-related wording.
    if (response.status === 401 && !isPublicAuthEndpoint(responsePath(response))) {
      const isLoginRequest = response.url.includes("/auth/login");
      if (!isLoginRequest) {
        removeToken();
        removeRefreshToken();
        clearActiveWorkspace();
        if (typeof window !== "undefined") {
          if (window.location.pathname !== "/login") {
            return redirectToLoginAndHalt();
          }
          throw new Error("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
        }
      }
      // If server-side or already on login page (and is a login request), let it fall through and throw
    }

    const trimmed = errorMessage.trim();
    if (response.status === 403 && typeof window !== "undefined") {
      window.dispatchEvent(new CustomEvent("aisam-access-denied", { detail: { status: response.status, path: responsePath(response) } }));
    }
    const mappedError = ERROR_MAP[trimmed]
      ?? Object.entries(ERROR_MAP).find(([k]) => k.toLowerCase() === trimmed.toLowerCase())?.[1];
    const friendly = response.status === 403 ? mappedError ?? "Bạn không còn quyền thực hiện thao tác này. Hãy kiểm tra workspace hoặc liên hệ Owner."
      : response.status === 404 ? "Tài nguyên không tồn tại hoặc bạn không còn quyền truy cập."
      : response.status === 409 ? "Quyền đã được người khác thay đổi. Tải lại trước khi lưu."
      : mappedError ?? (trimmed || `Request failed (${response.status})`);
    const error = new Error(friendly) as Error & {
      status?: number;
      category?: string;
    };
    error.status = response.status;
    error.category = "HTTP_ERROR";
    throw error;
  }

  if (result && typeof result === "object") {
    Object.defineProperty(result, "__httpStatus", {
      value: response.status,
      enumerable: false,
    });
  }
  return result;
}

async function retryWithRefresh(endpoint: string, config: RequestInit): Promise<unknown> {
  const newToken = await refreshAccessToken();
  if (!newToken) {
    removeToken();
    removeRefreshToken();
    clearActiveWorkspace();
    if (typeof window !== "undefined") {
      if (window.location.pathname !== "/login") {
        return redirectToLoginAndHalt();
      }
    }
    throw new Error("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
  }
  assertWorkspace(config);
  const newHeaders: Record<string, string> = {
    ...(config.headers as Record<string, string> || {}),
    Authorization: `Bearer ${newToken}`,
  };
  const retryResponse = await fetch(`${API_URL}${endpoint}`, { ...config, headers: newHeaders });
  assertWorkspace(config);
  return handleResponse(retryResponse, config);
}

export async function apiClient(endpoint: string, options: ApiOptions = {}) {
  if (isLoggingOut) {
    return new Promise(() => {});
  }
  const isPublic = isPublicAuthEndpoint(endpoint);
  if (!isPublic) {
    if (typeof window !== "undefined" && !getToken()) {
      if (window.location.pathname !== "/login") {
        return redirectToLoginAndHalt();
      }
    }
    await ensureValidToken();
  }
  const { data, headers: customHeaders, ...customConfig } = options;
  const { headers, token } = await buildHeaders(customHeaders as Record<string, string> | undefined, !isPublic);

  const hasJsonBody = data !== undefined && data !== null && !(data instanceof FormData);
  const isMutation = hasJsonBody || customConfig.method === "POST" || customConfig.method === "PUT" || customConfig.method === "DELETE";

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
  assertWorkspace(config);

  if (response.status === 401 && token && !isPublic && !endpoint.includes("/auth/login") && !endpoint.includes("/auth/refresh")) {
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response, config);
}

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  if (isLoggingOut) {
    return new Promise(() => {});
  }
  if (typeof window !== "undefined" && !getToken() && !isPublicAuthEndpoint(endpoint)) {
    if (window.location.pathname !== "/login") {
      return redirectToLoginAndHalt();
    }
  }
  await ensureValidToken();
  const { headers, token } = await buildHeaders(options.headers as Record<string, string> | undefined);

  const config: RequestInit = { ...options, headers, cache: "no-store" };

  const response = await fetch(`${API_URL}${endpoint}`, config);
  assertWorkspace(config);

  if (response.status === 401 && token && !endpoint.includes("/auth/login") && !endpoint.includes("/auth/refresh")) {
    return retryWithRefresh(endpoint, config);
  }

  return handleResponse(response, config);
}

function assertWorkspace(config: RequestInit) {
  const sent = new Headers(config.headers).get("X-Workspace-Id");
  if (sent !== (getStoredActiveWorkspace()?.id ?? null)) {
    throw new DOMException("Workspace đã thay đổi; bỏ qua phản hồi cũ.", "AbortError");
  }
}
