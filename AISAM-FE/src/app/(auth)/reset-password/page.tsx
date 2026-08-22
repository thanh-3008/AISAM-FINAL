"use client";

import Link from "next/link";
import { useState, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import AuthShell from "@/components/auth/AuthShell";

function ResetPasswordForm() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token") || "";
  const [form, setForm] = useState({ newPassword: "", confirmPassword: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
    if (errors[e.target.name]) {
      setErrors({ ...errors, [e.target.name]: "" });
    }
  };

  const validate = () => {
    const newErrors: Record<string, string> = {};
    if (!form.newPassword) {
      newErrors.newPassword = "Password is required";
    } else if (form.newPassword.length < 8) {
      newErrors.newPassword = "Password must be at least 8 characters";
    }

    if (!form.confirmPassword) {
      newErrors.confirmPassword = "Confirm password is required";
    } else if (form.newPassword !== form.confirmPassword) {
      newErrors.confirmPassword = "Confirm password does not match";
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
      await apiClient("/auth/reset-password", {
        data: {
          token,
          newPassword: form.newPassword,
          confirmPassword: form.confirmPassword,
        },
      });
      setIsSuccess(true);
    } catch (err: any) {
      setError(err.message || "Invalid or expired reset token.");
    } finally {
      setIsLoading(false);
    }
  };

  if (!token) {
    return (
      <AuthShell
        badge={{ icon: "shield_lock", text: "Secure Reset" }}
        headline="Create a"
        headlineHighlight="new password for your account."
        description="Choose a strong password that's at least 8 characters long with a mix of letters and numbers."
        backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
        stats={[
          { label: "Encryption", value: "AES-256" },
          { label: "Protected by", value: "JWT" },
        ]}
      >
        <div className="text-center">
          <div className="w-16 h-16 bg-error-container/50 rounded-2xl flex items-center justify-center mx-auto mb-6">
            <span className="material-symbols-outlined text-error text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>link_off</span>
          </div>
          <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Invalid Reset Link</h2>
          <p className="font-body-md text-body-md text-on-surface-variant mb-8">
            This link is invalid or has expired. Please request a new password reset.
          </p>
          <Link
            href="/forgot-password"
            className="inline-flex items-center gap-2 bg-primary-container text-on-primary-container font-label-md text-label-md py-3 px-6 rounded-lg hover:shadow-lg hover:opacity-90 transition-all"
          >
            Request New Link
          </Link>
        </div>
      </AuthShell>
    );
  }

  if (isSuccess) {
    return (
      <AuthShell
        badge={{ icon: "shield_lock", text: "Secure Reset" }}
        headline="Create a"
        headlineHighlight="new password for your account."
        description="Choose a strong password that's at least 8 characters long with a mix of letters and numbers."
        backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
        stats={[
          { label: "Encryption", value: "AES-256" },
          { label: "Protected by", value: "JWT" },
        ]}
      >
        <div className="text-center">
          <div className="w-16 h-16 bg-success-green/10 rounded-2xl flex items-center justify-center mx-auto mb-6">
            <span className="material-symbols-outlined text-success-green text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
          </div>
          <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Password Reset!</h2>
          <p className="font-body-md text-body-md text-on-surface-variant mb-2">
            Your password has been successfully reset.
          </p>
          <Link href="/login" className="mt-6 inline-flex items-center gap-2 text-primary font-bold hover:underline">
            <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
            Go to Sign In
          </Link>
        </div>
      </AuthShell>
    );
  }

  return (
    <AuthShell
      badge={{ icon: "shield_lock", text: "Secure Reset" }}
      headline="Create a"
      headlineHighlight="new password for your account."
      description="Choose a strong password that's at least 8 characters long with a mix of letters and numbers."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
      stats={[
        { label: "Encryption", value: "AES-256" },
        { label: "Protected by", value: "JWT" },
      ]}
    >
      {/* Heading */}
      <div className="mb-stack-lg text-center lg:text-left">
        <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
          New Password
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Create a strong, new password for your account.
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

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-stack-md" noValidate>

        {/* New Password */}
        <div>
          <label htmlFor="newPassword" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            New Password
          </label>
          <div className="relative">
            <input
              id="newPassword"
              name="newPassword"
              type={showPassword ? "text" : "password"}
              value={form.newPassword}
              onChange={handleChange}
              placeholder="Min. 8 characters"
              className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${errors.newPassword ? 'border-error focus:ring-error' : 'border-outline-variant'}`}
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
          {errors.newPassword && <p className="mt-1 font-label-sm text-label-sm text-error">{errors.newPassword}</p>}
        </div>

        {/* Confirm Password */}
        <div>
          <label htmlFor="confirmPassword" className="block font-label-md text-label-md text-on-surface-variant mb-1">
            Confirm Password
          </label>
          <div className="relative">
            <input
              id="confirmPassword"
              name="confirmPassword"
              type={showConfirm ? "text" : "password"}
              value={form.confirmPassword}
              onChange={handleChange}
              placeholder="Re-enter new password"
              className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${
                errors.confirmPassword
                  ? "border-error focus:ring-error"
                  : "border-outline-variant"
              }`}
            />
            <button
              type="button"
              onClick={() => setShowConfirm(!showConfirm)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
            >
              <span className="material-symbols-outlined text-[20px]">
                {showConfirm ? "visibility_off" : "visibility"}
              </span>
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="mt-1 font-label-sm text-label-sm text-error">{errors.confirmPassword}</p>
          )}
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading}
          className="w-full h-12 bg-primary-container text-on-primary-container font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
              <span>Resetting...</span>
            </>
          ) : (
            <>
              <span className="material-symbols-outlined text-[18px]">lock_reset</span>
              <span>Reset Password</span>
            </>
          )}
        </button>
      </form>

      {/* Back to Sign In */}
      <div className="mt-stack-lg text-center">
        <Link
          href="/login"
          className="text-primary font-bold hover:underline"
        >
          ← Back to Sign In
        </Link>
      </div>

      {/* Security Note */}
      <div className="mt-12 flex items-center justify-center gap-2 text-outline">
        <span className="material-symbols-outlined text-[16px] text-success-green">lock</span>
        <span className="font-label-sm text-label-sm">
          Secure, encrypted reset
        </span>
      </div>
    </AuthShell>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center gap-3"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin" /><span className="text-body-sm text-outline">Loading...</span></div>}>
      <ResetPasswordForm />
    </Suspense>
  );
}
