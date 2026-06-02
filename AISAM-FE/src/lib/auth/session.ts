import { useAuthStore } from "@/stores/auth-store";

export function clearSessionAndRedirect() {
  useAuthStore.getState().clearSession();
  if (typeof window !== "undefined") {
    window.location.assign("/login");
  }
}
