export const setToken = (token: string) => {
  if (typeof window !== "undefined") {
    localStorage.setItem("aisam_token", token);
  }
};

export const getToken = () => {
  if (typeof window !== "undefined") {
    return localStorage.getItem("aisam_token");
  }
  return null;
};

export const removeToken = () => {
  if (typeof window !== "undefined") {
    localStorage.removeItem("aisam_token");
  }
};

const REFRESH_TOKEN_KEY = "aisam_refresh_token";
const USER_KEY = "aisam_user";

export const setRefreshToken = (token: string) => {
  if (typeof window !== "undefined") {
    localStorage.setItem(REFRESH_TOKEN_KEY, token);
  }
};

export const getRefreshToken = (): string | null => {
  if (typeof window !== "undefined") {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }
  return null;
};

export const removeRefreshToken = () => {
  if (typeof window !== "undefined") {
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }
};

export interface StoredUser {
  id: string;
  fullName: string;
  email: string;
}

export const setStoredUser = (user: StoredUser) => {
  if (typeof window !== "undefined") {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }
};

export const getStoredUser = (): StoredUser | null => {
  if (typeof window !== "undefined") {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
  return null;
};

export const removeStoredUser = () => {
  if (typeof window !== "undefined") {
    localStorage.removeItem(USER_KEY);
  }
};

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

export async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;

  try {
    const res = await fetch(`${API_URL}/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });
    const result = await res.json();
    if (res.ok && result.success && result.data?.accessToken) {
      setToken(result.data.accessToken);
      if (result.data.refreshToken) {
        setRefreshToken(result.data.refreshToken);
      }
      if (result.data.user) {
        setStoredUser(result.data.user);
      }
      return result.data.accessToken;
    }
    return null;
  } catch {
    return null;
  }
}

export async function ensureValidToken(): Promise<string | null> {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    if (payload.exp) {
      const expiresAt = payload.exp * 1000;
      const fiveMin = 5 * 60 * 1000;
      if (Date.now() >= expiresAt - fiveMin) {
        const newToken = await refreshAccessToken();
        if (newToken) return newToken;
      }
    }
  } catch {
    // ignore parse errors
  }
  return token;
}

export async function logout(): Promise<void> {
  try {
    const token = getToken();
    if (token) {
      await fetch(`${API_URL}/auth/logout`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });
    }
  } catch {
    // silently fail — BE call is best-effort
  } finally {
    removeToken();
    removeRefreshToken();
    removeStoredUser();
    try {
      const { invalidateProfileCache } = await import("@/hooks/useProfiles");
      invalidateProfileCache();
    } catch {
      // ignore
    }
  }
}

const CLAIM_UID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const CLAIM_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
const CLAIM_EMAIL = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
const CLAIM_ROLE = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role";

export function getUserIdFromToken(): string | null {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload[CLAIM_UID] || payload.sub || payload.nameid || null;
  } catch {
    return null;
  }
}

export function getUserFromToken(): { name?: string; email?: string } | null {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return {
      name: payload[CLAIM_NAME] || payload.name || payload.given_name || payload.unique_name || undefined,
      email: payload[CLAIM_EMAIL] || payload.email || payload.preferred_username || undefined,
    };
  } catch {
    return null;
  }
}
