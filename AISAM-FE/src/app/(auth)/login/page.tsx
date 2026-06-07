"use client";

import Link from "next/link";
import { useState, useEffect, useCallback } from "react";
import { useRouter } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { setToken, setRefreshToken, setStoredUser } from "@/lib/auth";
import { invalidateProfileCache } from "@/hooks/useProfiles";
import AuthShell from "@/components/auth/AuthShell";

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
            cancel_on_tap_outside?: boolean;
          }) => void;
          prompt: (momentListener?: (moment: { type: string }) => void) => void;
        };
      };
    };
  }
}

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();

  // Google Sign-In
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  const handleGoogleResponse = useCallback(async (credential: string) => {
    if (!credential) return;
    setIsLoading(true);
    setError(null);
    try {
      const result = await apiClient("/auth/google", { data: { idToken: credential } });
      if (result.success && result.data?.accessToken) {
        invalidateProfileCache();
        setToken(result.data.accessToken);
        if (result.data.refreshToken) setRefreshToken(result.data.refreshToken);
        if (result.data.user) setStoredUser(result.data.user);
        setIsSuccess(true);
        router.push("/overview");
      } else {
        setError("Google đăng nhập thất bại.");
      }
    } catch (err: any) {
      setError(err.message || "Google đăng nhập thất bại.");
    } finally {
      setIsLoading(false);
    }
  }, [router]);

  useEffect(() => {
    if (!clientId) return;
    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    document.body.appendChild(script);
    script.onload = () => {
      window.google?.accounts.id.initialize({
        client_id: clientId,
        callback: (res) => { if (res?.credential) handleGoogleResponse(res.credential); },
        cancel_on_tap_outside: false,
      });
    };
    return () => { if (script.parentNode) document.body.removeChild(script); };
  }, [clientId, handleGoogleResponse]);

  const handleGoogleLogin = () => {
    window.google?.accounts.id.prompt();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const result = await apiClient("/auth/login", {
        data: { email, password },
      });

      if (result.success && result.data?.accessToken) {
        invalidateProfileCache();
        setToken(result.data.accessToken);

        if (result.data.refreshToken) {
          setRefreshToken(result.data.refreshToken);
        }

        if (result.data.user) {
          setStoredUser(result.data.user);
        }

        // fetch full user info
        try {
          const meResult = await apiClient("/auth/me");
          if (meResult.success && meResult.data) {
            setStoredUser({
              id: meResult.data.id || meResult.data.userId || "",
              fullName: meResult.data.fullName || meResult.data.full_name || "",
              email: meResult.data.email || "",
            });
            if (meResult.data.refreshToken) {
              setRefreshToken(meResult.data.refreshToken);
            }
          }
        } catch {
          // /auth/me is optional — continue regardless
        }

        setIsSuccess(true);
        router.push("/overview");
      } else {
        setError("Đăng nhập thất bại, vui lòng thử lại.");
      }
    } catch (err: any) {
      setError(err.message || "Email hoặc mật khẩu không chính xác.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthShell
      badge={{ icon: "auto_awesome", text: "Enterprise Precision" }}
      headline="AI-Powered Precision in every campaign."
      description="Harness the power of surgical-grade neural networks to optimize your social media advertising with unparalleled accuracy and real-time intelligence."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuB9aMAbRqJJ9ilmlGLShVEUiz0ld2SmU1DXq3l-WUQAFgZgmGlYOnJryDpkUAeYfYzripf7sZ4FUQtez1hlchpmrBr_E5LQ5NbmWm4Zlmo8avlH3KkpoHBoqaonTnK6pAhFFmZe9iq8t6MUub0elaFwMFm7zV1G7E7bamnPcgA4o-eAWWmsT1_enXw1GBfszBdgwaPeAZ2nVzN47TZ9nulCNEnZPeY4eHdLDdKHJ0nLnsibjNDYDE5ctee_DCUmd0QXJ8Qd2HxG-AA"
      showSocialProof={true}
    >
      {/* Heading */}
      <div className="mb-stack-lg text-center lg:text-left">
        <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
          Welcome back
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Sign in to your AISAM command center.
        </p>
      </div>

      {/* Error State */}
      {error && (
        <div className="mb-stack-md p-stack-md bg-error-container/50 border border-error/20 rounded-lg flex items-start gap-3">
          <span
            className="material-symbols-outlined text-error shrink-0"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            error
          </span>
          <p className="font-body-sm text-body-sm text-on-error-container">
            {error}
          </p>
        </div>
      )}

      {/* Google Button */}
      <button
        type="button"
        onClick={handleGoogleLogin}
        disabled={isLoading || isSuccess}
        className="w-full h-12 flex items-center justify-center gap-3 bg-surface-container-low border border-outline-variant rounded-lg hover:bg-surface-container-high transition-colors duration-200 active:scale-[0.98] mb-stack-lg disabled:opacity-50"
      >
        <svg className="w-5 h-5" viewBox="0 0 24 24">
          <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
          <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
          <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
          <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
        </svg>
        <span className="font-label-md text-label-md text-on-surface">
          Continue with Google
        </span>
      </button>

      {/* Divider */}
      <div className="flex items-center gap-4 mb-stack-lg">
        <div className="h-px bg-outline-variant flex-1" />
        <span className="text-label-sm text-outline font-semibold">
          Or email
        </span>
        <div className="h-px bg-outline-variant flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-stack-md">
        {/* Email */}
        <div>
          <label
            htmlFor="email"
            className="block font-label-md text-label-md text-on-surface-variant mb-1"
          >
            Email Address
          </label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="name@company.com"
            required
            className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
          />
        </div>

        {/* Password */}
        <div>
          <div className="flex justify-between items-center mb-1">
            <label
              htmlFor="password"
              className="block font-label-md text-label-md text-on-surface-variant"
            >
              Password
            </label>
            <Link
              href="/forgot-password"
              className="font-label-md text-label-md text-primary hover:underline transition-all"
            >
              Forgot password?
            </Link>
          </div>
          <div className="relative">
            <input
              id="password"
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12"
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
            >
              <span className="material-symbols-outlined text-[20px]">
                {showPassword ? "visibility_off" : "visibility"}
              </span>
            </button>
          </div>
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading || isSuccess}
          className={`w-full h-12 font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:cursor-not-allowed ${
            isSuccess
              ? "bg-success-green text-white"
              : "bg-primary-container text-on-primary-container"
          }`}
        >
          {isLoading ? (
            <>
              <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
              <span>Authenticating...</span>
            </>
          ) : isSuccess ? (
            <>
              <span className="material-symbols-outlined text-[20px]">
                check_circle
              </span>
              <span>Welcome back</span>
            </>
          ) : (
            <span>Sign In</span>
          )}
        </button>
      </form>

      {/* Sign Up Link */}
      <div className="mt-stack-lg text-center">
        <p className="font-body-sm text-body-sm text-on-surface-variant">
          Don&apos;t have an account?{" "}
          <Link
            href="/register"
            className="text-primary font-bold hover:underline"
          >
            Sign up
          </Link>
        </p>
      </div>

      {/* Security Note */}
      <div className="mt-12 flex items-center justify-center gap-2 text-outline">
        <span className="material-symbols-outlined text-[16px]">lock</span>
        <span className="font-label-sm text-label-sm">
          Secure, encrypted login
        </span>
      </div>
    </AuthShell>
  );
}
