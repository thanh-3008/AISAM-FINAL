import { env } from "@/lib/config/env";
import type { ApiError, GenericResponse } from "@/types/api";
import { useAuthStore } from "@/stores/auth-store";
import { useProfileStore } from "@/stores/profile-store";

type RequestOptions = RequestInit & {
  auth?: boolean;
  skipRefresh?: boolean;
};

let refreshPromise: Promise<string | null> | null = null;

function toApiError<T>(payload: GenericResponse<T> | null, fallbackStatus: number): ApiError {
  return {
    message: payload?.error?.errorMessage ?? payload?.message ?? "Request failed",
    statusCode: payload?.statusCode ?? fallbackStatus,
    errorCode: payload?.error?.errorCode,
    validationErrors: payload?.error?.validationErrors
  };
}

async function tryRefreshToken() {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    const { refreshToken, clearSession, setSession } = useAuthStore.getState();
    if (!refreshToken) {
      clearSession();
      return null;
    }

    const response = await fetch(`${env.apiBaseUrl}/api/Auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ refreshToken })
    });

    const payload = (await response.json()) as GenericResponse<{
      accessToken: string;
      refreshToken: string;
      expiresAt: string;
      tokenType: string;
      user: {
        id: string;
        email: string;
        fullName?: string | null;
        role: string;
        isEmailVerified: boolean;
        createdAt: string;
        lastLoginAt?: string | null;
      };
    }>;

    if (!response.ok || !payload.success || !payload.data) {
      clearSession();
      return null;
    }

    setSession(payload.data);
    return payload.data.accessToken;
  })();

  const result = await refreshPromise;
  refreshPromise = null;
  return result;
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { auth = false, skipRefresh = false, headers, ...rest } = options;
  const authState = useAuthStore.getState();
  const mergedHeaders = new Headers(headers);

  if (!(rest.body instanceof FormData) && !mergedHeaders.has("Content-Type")) {
    mergedHeaders.set("Content-Type", "application/json");
  }

  if (auth && authState.accessToken) {
    mergedHeaders.set("Authorization", `Bearer ${authState.accessToken}`);
  }

  if (auth) {
    const activeProfileId = useProfileStore.getState().activeProfile?.id;
    if (activeProfileId) {
      mergedHeaders.set("X-Profile-Id", activeProfileId);
    }
  }

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    ...rest,
    headers: mergedHeaders
  });

  const payload = (await response.json()) as GenericResponse<T>;
  if (response.ok && payload.success) {
    return payload.data as T;
  }

  if (response.status === 401 && auth && !skipRefresh) {
    const nextToken = await tryRefreshToken();
    if (nextToken) {
      return apiRequest<T>(path, {
        ...options,
        skipRefresh: true
      });
    }

    if (typeof window !== "undefined") {
      window.location.assign("/login");
    }
  }

  throw toApiError(payload, response.status);
}
