"use client";

import Link from "next/link";
import { useState, useEffect, useCallback, useRef } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { getToken, getUserRoleFromToken, setToken, setRefreshToken, setStoredUser } from "@/lib/auth";
import { invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import AuthShell from "@/components/auth/AuthShell";
import { initializeGoogleIdentity, renderGoogleIdentityButton } from "@/lib/googleIdentity";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [isCheckingSession, setIsCheckingSession] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isGoogleReady, setIsGoogleReady] = useState(false);
  const googleButtonRef = useRef<HTMLDivElement | null>(null);
  const router = useRouter();
  const searchParams = useSearchParams();

  // Google Sign-In
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  const getRedirectUrl = useCallback(() => {
    return searchParams.get("redirect") || "/overview";
  }, [searchParams]);

  useEffect(() => {
    let cancelled = false;

    const redirectIfAlreadySignedIn = async () => {
      const token = getToken();
      if (!token) {
        setIsCheckingSession(false);
        return;
      }

      try {
        const meResult = await apiClient("/auth/me");
        if (cancelled) return;

        if (meResult?.success && meResult.data) {
          setStoredUser({
            id: meResult.data.id || meResult.data.userId || "",
            fullName: meResult.data.fullName || meResult.data.full_name || "",
            email: meResult.data.email || "",
          });
        }

        setIsSuccess(true);
        router.replace(getUserRoleFromToken() === "Admin" ? "/admin/dashboard" : getRedirectUrl());
      } catch {
        if (!cancelled) {
          setIsCheckingSession(false);
        }
      }
    };

    redirectIfAlreadySignedIn();

    return () => {
      cancelled = true;
    };
  }, [getRedirectUrl, router]);

  const handleGoogleResponse = useCallback(async (credential: string) => {
    if (!credential) return;
    setIsLoading(true);
    setError(null);
    try {
      const result = await apiClient("/auth/google", { data: { idToken: credential } });
      if (result.success && result.data?.accessToken) {
        invalidateWorkspaceCache();
        setToken(result.data.accessToken);
        if (result.data.refreshToken) setRefreshToken(result.data.refreshToken);
        if (result.data.user) {
          setStoredUser(result.data.user);

          const roleMap: Record<number, string> = { 0: "User", 1: "Vendor", 2: "Admin" };
          const roleStr =
            typeof result.data.user.role === "number"
              ? roleMap[result.data.user.role] || "User"
              : String(result.data.user.role || "User");

          if (typeof document !== "undefined") {
            document.cookie = `aisam_role=${roleStr}; path=/; max-age=86400`;
          }

          if (roleStr === "Admin") {
            setIsSuccess(true);
            router.push("/admin/dashboard");
            return;
          }
        }
        setIsSuccess(true);
        router.push(getRedirectUrl());
      } else {
        setError("Google sign-in failed.");
      }
    } catch (err: any) {
      setError(err.message || "Google sign-in failed.");
    } finally {
      setIsLoading(false);
    }
  }, [router, getRedirectUrl]);

  useEffect(() => {
    let isMounted = true;

    initializeGoogleIdentity(clientId, handleGoogleResponse)
      .then(() => {
        if (!isMounted) return;
        setIsGoogleReady(true);
        if (googleButtonRef.current) {
          renderGoogleIdentityButton(googleButtonRef.current, {
            theme: "outline",
            size: "large",
            text: "continue_with",
            shape: "rectangular",
            width: 420,
          });
        }
      })
      .catch((err: Error) => {
        if (isMounted && clientId) setError(err.message);
      });

    return () => {
      isMounted = false;
    };
  }, [clientId, handleGoogleResponse]);

  const validate = () => {
    const newErrors: Record<string, string> = {};
    const trimmedEmail = email.trim();
    if (!trimmedEmail) {
      newErrors.email = "Email is required";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      newErrors.email = "Please enter a valid email address";
    }
    
    if (!password) {
      newErrors.password = "Password is required";
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;
    setIsLoading(true);

    try {
      const result = await apiClient("/auth/login", {
        data: { email: email.trim(), password },
      });

      if (result.success && result.data?.accessToken) {
        invalidateWorkspaceCache();
        setToken(result.data.accessToken);

        if (result.data.refreshToken) {
          setRefreshToken(result.data.refreshToken);
        }

        if (result.data.user) {
          setStoredUser(result.data.user);

          const roleMap: Record<number, string> = { 0: "User", 1: "Vendor", 2: "Admin" };
          const roleStr =
            typeof result.data.user.role === "number"
              ? roleMap[result.data.user.role] || "User"
              : String(result.data.user.role || "User");

          if (typeof document !== "undefined") {
            document.cookie = `aisam_role=${roleStr}; path=/; max-age=86400`;
          }

          if (roleStr === "Admin") {
            setIsSuccess(true);
            router.push("/admin/dashboard");
            return;
          }
        }

        // fetch full user info for non-admin users
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
        router.push(getRedirectUrl());
      } else {
        setError("Login failed, please try again.");
      }
    } catch (err: any) {
      setError("Tài khoản hoặc mật khẩu không đúng, vui lòng thử lại");
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
      <div className="mb-stack-lg">
        <div
          ref={googleButtonRef}
          className="flex min-h-12 w-full items-center justify-center"
          aria-busy={!isGoogleReady || isLoading || isSuccess || isCheckingSession}
        />
        {!clientId && (
          <p className="mt-2 text-center font-body-sm text-body-sm text-error">
            Google sign-in is not configured.
          </p>
        )}
      </div>

      {/* Divider */}
      <div className="flex items-center gap-4 mb-stack-lg">
        <div className="h-px bg-outline-variant flex-1" />
        <span className="text-label-sm text-outline font-semibold">
          Or email
        </span>
        <div className="h-px bg-outline-variant flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-stack-md" noValidate>
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
            onChange={(e) => {
              setEmail(e.target.value);
              if (errors.email) setErrors({ ...errors, email: "" });
            }}
            placeholder="name@company.com"
            className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface ${errors.email ? 'border-error focus:ring-error' : 'border-outline-variant'}`}
          />
          {errors.email && <p className="mt-1 font-label-sm text-label-sm text-error">{errors.email}</p>}
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
              onChange={(e) => {
                setPassword(e.target.value);
                if (errors.password) setErrors({ ...errors, password: "" });
              }}
              placeholder="••••••••"
              className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${errors.password ? 'border-error focus:ring-error' : 'border-outline-variant'}`}
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
          {errors.password && <p className="mt-1 font-label-sm text-label-sm text-error">{errors.password}</p>}
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading || isSuccess || isCheckingSession}
          className={`w-full h-12 font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:cursor-not-allowed ${
            isSuccess
              ? "bg-success-green text-white"
              : "bg-primary-container text-on-primary-container"
          }`}
        >
          {isCheckingSession ? (
            <>
              <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
              <span>Checking session...</span>
            </>
          ) : isLoading ? (
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
