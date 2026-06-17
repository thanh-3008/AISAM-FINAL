"use client";

import Link from "next/link";
import { useState } from "react";
import { apiClient } from "@/lib/apiClient";
import AuthShell from "@/components/auth/AuthShell";

function getErrorMessage(err: unknown, fallback: string) {
  return err instanceof Error ? err.message : fallback;
}

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await apiClient("/auth/forgot-password", {
        data: { email },
      });
      setIsSuccess(true);
    } catch (err: unknown) {
      setError(getErrorMessage(err, "An error occurred, please try again."));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthShell
      badge={{ icon: "lock_reset", text: "Account Recovery" }}
      headline="Forgot your"
      headlineHighlight="password?"
      description="No worries. Enter your email address and we'll send you a link to reset your password."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
      stats={[
        { label: "Secure Reset", value: "256-bit" },
        { label: "Link Expires", value: "24 HRS" },
      ]}
    >
      {isSuccess ? (
        /* Success State */
        <div className="text-center">
          <div className="w-16 h-16 bg-success-green/10 rounded-2xl flex items-center justify-center mx-auto mb-6">
            <span className="material-symbols-outlined text-success-green text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>mark_email_read</span>
          </div>
          <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Check your inbox</h2>
          <p className="font-body-md text-body-md text-on-surface-variant mb-8">
            We&apos;ve sent a password reset link to <span className="font-semibold text-on-surface">{email}</span>. Please check your email.
          </p>
          <Link
            href="/login"
            className="inline-flex items-center gap-2 bg-primary-container text-on-primary-container font-label-md text-label-md py-3 px-6 rounded-lg hover:shadow-lg hover:opacity-90 transition-all"
          >
            <span className="material-symbols-outlined text-[18px]">arrow_back</span>
            Back to Sign In
          </Link>
          <p className="mt-6 font-body-sm text-body-sm text-on-surface-variant">
            Didn&apos;t receive the email?{" "}
            <button onClick={() => setIsSuccess(false)} className="text-primary font-bold hover:underline">
              Try again
            </button>
          </p>
        </div>
      ) : (
        /* Form */
        <>
          {/* Heading */}
          <div className="mb-stack-lg text-center lg:text-left">
            <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
              Reset password
            </h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Enter your account email and we&apos;ll send a reset link.
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
          <form onSubmit={handleSubmit} className="space-y-stack-md">
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

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full h-12 bg-primary-container text-on-primary-container font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <>
                  <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
                  <span>Sending reset link...</span>
                </>
              ) : (
                <>
                  <span className="material-symbols-outlined text-[18px]">send</span>
                  <span>Send Reset Link</span>
                </>
              )}
            </button>
          </form>

          {/* Sign In Link */}
          <div className="mt-stack-lg text-center">
            <p className="font-body-sm text-body-sm text-on-surface-variant">
              Remember your password?{" "}
              <Link
                href="/login"
                className="text-primary font-bold hover:underline"
              >
                Sign In
              </Link>
            </p>
          </div>

          {/* Security Note */}
          <div className="mt-12 flex items-center justify-center gap-2 text-outline">
            <span className="material-symbols-outlined text-[16px] text-success-green">lock</span>
            <span className="font-label-sm text-label-sm">
              Secure, encrypted link
            </span>
          </div>
        </>
      )}
    </AuthShell>
  );
}
