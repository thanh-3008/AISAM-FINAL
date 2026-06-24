"use client";

import Link from "next/link";
import { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { setToken, setRefreshToken, setStoredUser } from "@/lib/auth";
import { invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import AuthShell from "@/components/auth/AuthShell";
import { initializeGoogleIdentity, renderGoogleIdentityButton } from "@/lib/googleIdentity";

export default function RegisterPage() {
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isGoogleReady, setIsGoogleReady] = useState(false);
  const [form, setForm] = useState({ full_name: "", email: "", password: "", confirm_password: "" });
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const googleButtonRef = useRef<HTMLDivElement | null>(null);
  const router = useRouter();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleGoogleResponse = useCallback(async (credential: string) => {
    if (!credential) return;
    setError(null);
    setIsLoading(true);
    try {
      const result = await apiClient("/auth/google", { data: { idToken: credential } });
      if (result.success && result.data?.accessToken) {
        invalidateWorkspaceCache();
        setToken(result.data.accessToken);
        if (result.data.refreshToken) setRefreshToken(result.data.refreshToken);
        if (result.data.user) setStoredUser(result.data.user);
        router.push("/overview");
      } else {
        setError("Google sign-in failed.");
      }
    } catch (err: any) {
      setError(err.message || "Google sign-in failed.");
    } finally {
      setIsLoading(false);
    }
  }, [router]);

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
            text: "signup_with",
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    
    try {
      if (form.password !== form.confirm_password) {
        setError("Confirm password does not match.");
        setIsLoading(false);
        return;
      }
      const result = await apiClient("/auth/register", {
        data: {
          fullName: form.full_name,
          email: form.email,
          password: form.password,
          confirmPassword: form.confirm_password,
        },
      });

      if (result.success) {
        if (result.data?.accessToken) {
          setToken(result.data.accessToken);
        }
        if (result.data?.refreshToken) {
          setRefreshToken(result.data.refreshToken);
        }
        if (result.data?.user) {
          setStoredUser(result.data.user);
        }
        invalidateWorkspaceCache();
        router.push("/overview");
      } else {
        setError("Registration failed, please try again.");
      }
    } catch (err: any) {
      setError(err.message || "An error occurred during registration.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthShell
      badge={{ icon: "verified", text: "Enterprise Precision" }}
      headline="Surgical-grade"
      headlineHighlight="ad optimization powered by neural networks."
      description="Deploy AISAM's advanced command center to manage, optimize, and scale your social advertising with unmatched intelligence and speed."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
      stats={[
        { label: "Real-time ROAS", value: "+24.8%" },
        { label: "Neural Sync", value: "ACTIVE" },
      ]}
    >
      {/* Heading */}
      <div className="mb-stack-lg text-center lg:text-left">
        <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
          Create your account
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Get started with your AISAM command center.
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

      {/* Google Sign Up */}
      <div className="mb-stack-lg">
        <div
          ref={googleButtonRef}
          className="flex min-h-12 w-full items-center justify-center"
          aria-busy={!isGoogleReady || isLoading}
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
      <form onSubmit={handleSubmit} className="space-y-stack-md">
        {/* Full Name */}
        <div>
          <label htmlFor="full_name" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            Full Name
          </label>
          <input
            id="full_name"
            name="full_name"
            type="text"
            value={form.full_name}
            onChange={handleChange}
            placeholder="John Doe"
            required
            className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
          />
        </div>

        {/* Email */}
        <div>
          <label htmlFor="email" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            Email Address
          </label>
          <input
            id="email"
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            placeholder="name@company.com"
            required
            className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
          />
        </div>

        {/* Password */}
        <div>
          <label htmlFor="password" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            Password
          </label>
          <div className="relative">
            <input
              id="password"
              name="password"
              type={showPassword ? "text" : "password"}
              value={form.password}
              onChange={handleChange}
              placeholder="Min. 8 characters"
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

        {/* Confirm Password */}
        <div>
          <label htmlFor="confirm_password" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            Confirm Password
          </label>
          <div className="relative">
            <input
              id="confirm_password"
              name="confirm_password"
              type={showConfirmPassword ? "text" : "password"}
              value={form.confirm_password}
              onChange={handleChange}
              placeholder="Re-enter your password"
              required
              className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${
                form.confirm_password && form.password !== form.confirm_password
                  ? "border-error focus:ring-error"
                  : "border-outline-variant"
              }`}
            />
            <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
            >
              <span className="material-symbols-outlined text-[20px]">
                {showConfirmPassword ? "visibility_off" : "visibility"}
              </span>
            </button>
          </div>
          {form.confirm_password && form.password !== form.confirm_password && (
            <p className="mt-1 font-label-sm text-label-sm text-error">Passwords do not match</p>
          )}
        </div>

        {/* Submit */}
        <button
          type="submit"
          disabled={isLoading}
          className="w-full h-12 bg-primary-container text-on-primary-container font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
              <span>Creating Account...</span>
            </>
          ) : (
            <span>Create Account</span>
          )}
        </button>
      </form>

      {/* Sign In Link */}
      <div className="mt-stack-lg text-center">
        <p className="font-body-sm text-body-sm text-on-surface-variant">
          Already have an account?{" "}
          <Link
            href="/login"
            className="text-primary font-bold hover:underline"
          >
            Sign In
          </Link>
        </p>
      </div>

      {/* Trust Note */}
      <div className="mt-12 flex items-center justify-center gap-2 text-outline">
        <span className="material-symbols-outlined text-[16px] text-success-green">lock</span>
        <span className="font-label-sm text-label-sm">
          Secure, encrypted registration
        </span>
      </div>
    </AuthShell>
  );
}
