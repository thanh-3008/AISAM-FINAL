import { apiRequest } from "@/lib/api/fetcher";
import type { SessionDto, TokenResponse } from "@/features/auth/types/auth";

export type RegisterInput = {
  fullName?: string;
  email: string;
  password: string;
  confirmPassword: string;
};

export type LoginInput = {
  email: string;
  password: string;
};

export type ForgotPasswordInput = {
  email: string;
};

export type ResetPasswordInput = {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
};

export type ChangePasswordInput = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

export const authApi = {
  register: (payload: RegisterInput) =>
    apiRequest<TokenResponse>("/api/Auth/register", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  login: (payload: LoginInput) =>
    apiRequest<TokenResponse>("/api/Auth/login", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  googleLogin: (idToken: string) =>
    apiRequest<TokenResponse>("/api/Auth/google", {
      method: "POST",
      body: JSON.stringify({ idToken })
    }),
  me: () =>
    apiRequest<{ id: string; email: string; fullName?: string | null; role: string }>("/api/Auth/me", {
      method: "GET",
      auth: true
    }),
  logout: (refreshToken?: string | null) =>
    apiRequest<null>("/api/Auth/logout", {
      method: "POST",
      auth: true,
      body: JSON.stringify({ refreshToken })
    }),
  logoutAll: () =>
    apiRequest<null>("/api/Auth/logout-all", {
      method: "POST",
      auth: true
    }),
  sessions: () =>
    apiRequest<SessionDto[]>("/api/Auth/sessions", {
      method: "GET",
      auth: true
    }),
  forgotPassword: (payload: ForgotPasswordInput) =>
    apiRequest<null>("/api/Auth/forgot-password", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  resetPassword: (payload: ResetPasswordInput) =>
    apiRequest<null>("/api/Auth/reset-password", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  changePasswordWithToken: (payload: ResetPasswordInput) =>
    apiRequest<null>("/api/Auth/change-password-with-token", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  verifyEmail: (token: string) =>
    apiRequest<null>(`/api/Auth/verify-email?token=${encodeURIComponent(token)}`, {
      method: "GET"
    }),
  resendVerification: (payload: ForgotPasswordInput) =>
    apiRequest<null>("/api/Auth/verify-email/resend", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  changePassword: (payload: ChangePasswordInput) =>
    apiRequest<null>("/api/Auth/change-password", {
      method: "POST",
      auth: true,
      body: JSON.stringify(payload)
    })
};
