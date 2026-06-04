"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import { authApi, type ChangePasswordInput, type ForgotPasswordInput, type LoginInput, type RegisterInput, type ResetPasswordInput } from "@/features/auth/api/auth-api";
import { useAuthStore } from "@/stores/auth-store";
import { clearSessionAndRedirect } from "@/lib/auth/session";

export function useRegister() {
  const setSession = useAuthStore((state) => state.setSession);
  return useMutation({
    mutationFn: (payload: RegisterInput) => authApi.register(payload),
    onSuccess: (data) => setSession(data)
  });
}

export function useLogin() {
  const setSession = useAuthStore((state) => state.setSession);
  return useMutation({
    mutationFn: (payload: LoginInput) => authApi.login(payload),
    onSuccess: (data) => setSession(data)
  });
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (payload: ForgotPasswordInput) => authApi.forgotPassword(payload)
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: (payload: ResetPasswordInput) => authApi.resetPassword(payload)
  });
}

export function useResendVerification() {
  return useMutation({
    mutationFn: (payload: ForgotPasswordInput) => authApi.resendVerification(payload)
  });
}

export function useVerifyEmail(token: string | null) {
  return useQuery({
    queryKey: ["auth", "verify-email", token],
    queryFn: () => authApi.verifyEmail(token as string),
    enabled: Boolean(token)
  });
}

export function useMe() {
  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: authApi.me
  });
}

export function useSessions() {
  return useQuery({
    queryKey: ["auth", "sessions"],
    queryFn: authApi.sessions
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: (payload: ChangePasswordInput) => authApi.changePassword(payload)
  });
}

export function useLogout() {
  const refreshToken = useAuthStore((state) => state.refreshToken);
  return useMutation({
    mutationFn: () => authApi.logout(refreshToken),
    onSettled: () => clearSessionAndRedirect()
  });
}

export function useLogoutAll() {
  return useMutation({
    mutationFn: authApi.logoutAll,
    onSettled: () => clearSessionAndRedirect()
  });
}
