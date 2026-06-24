import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
}

export interface AuthSession {
  id?: string;
  sessionId?: string;
  deviceName?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt?: string;
  lastActiveAt?: string | null;
  expiresAt?: string | null;
  isCurrent?: boolean;
}

export interface ChangePasswordWithTokenPayload {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export async function getAuthSessions(): Promise<AuthSession[]> {
  try {
    const res: GenericResponse<AuthSession[]> = await apiClient("/auth/sessions");
    return res?.data ?? [];
  } catch {
    return [];
  }
}

export async function logoutAllSessions(): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient("/auth/logout-all", { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function verifyEmail(token: string): Promise<boolean> {
  try {
    const query = new URLSearchParams({ token });
    const res: GenericResponse<unknown> = await apiClient(`/auth/verify-email?${query.toString()}`);
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function resendVerifyEmail(email: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient("/auth/verify-email/resend", {
      method: "POST",
      data: { email },
    });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function changePasswordWithToken(payload: ChangePasswordWithTokenPayload): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient("/auth/change-password-with-token", {
      method: "POST",
      data: payload,
    });
    return res?.success === true;
  } catch {
    return false;
  }
}
