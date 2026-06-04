"use client";

import Link from "next/link";
import { useState } from "react";
import { apiClient } from "@/lib/apiClient";

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
    } catch (err: any) {
      setError(err.message || "Có lỗi xảy ra, vui lòng thử lại.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="w-full min-h-screen flex flex-col md:flex-row overflow-hidden">
      {/* Left Panel */}
      <section className="hidden md:flex md:w-1/2 lg:w-3/5 bg-enterprise-navy relative flex-col justify-between p-margin-desktop overflow-hidden">
        <div className="absolute inset-0 z-0">
          <img
            alt="AI Visualization"
            className="w-full h-full object-cover opacity-40 mix-blend-screen"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-enterprise-navy via-transparent to-primary/10" />
        </div>

        {/* Logo */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-10 h-10 bg-primary-container rounded-lg flex items-center justify-center shadow-lg" style={{ boxShadow: "0 0 40px -10px rgba(15,98,254,0.3)" }}>
            <span className="material-symbols-outlined text-white" style={{ fontVariationSettings: "'FILL' 1" }}>neurology</span>
          </div>
          <span className="font-headline-sm text-headline-sm font-bold text-surface-bright tracking-tight">AISAM</span>
        </div>

        {/* Headline */}
        <div className="relative z-10 mb-12">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/20 border border-primary/30 text-primary-fixed-dim font-label-md text-label-md mb-stack-md">
            <span className="material-symbols-outlined text-[14px]">lock_reset</span>
            ACCOUNT RECOVERY
          </div>
          <h1 className="font-display-lg text-display-lg text-surface-bright mb-stack-md leading-tight">
            Forgot your <span className="text-primary-fixed-dim">password?</span>
          </h1>
          <p className="font-body-lg text-body-lg text-outline-variant max-w-xl">
            No worries. Enter your email address and we&apos;ll send you a link to reset your password.
          </p>
        </div>

        {/* Stats */}
        <div className="relative z-10 flex gap-gutter">
          <div className="flex-1 p-stack-md rounded-xl" style={{ background: "rgba(255,255,255,0.05)", backdropFilter: "blur(12px)", border: "1px solid rgba(255,255,255,0.1)" }}>
            <span className="font-label-md text-label-md text-primary-fixed-dim block mb-1">SECURE RESET</span>
            <span className="font-headline-md text-headline-md text-surface-bright">256-bit</span>
          </div>
          <div className="flex-1 p-stack-md rounded-xl" style={{ background: "rgba(255,255,255,0.05)", backdropFilter: "blur(12px)", border: "1px solid rgba(255,255,255,0.1)" }}>
            <span className="font-label-md text-label-md text-primary-fixed-dim block mb-1">LINK EXPIRES</span>
            <span className="font-headline-md text-headline-md text-surface-bright">24 HRS</span>
          </div>
        </div>
      </section>

      {/* Right Panel */}
      <section className="w-full md:w-1/2 lg:w-2/5 bg-surface-container-lowest min-h-screen flex items-center justify-center p-margin-mobile md:p-margin-desktop">
        <div className="w-full max-w-md">
          {/* Mobile Logo */}
          <div className="md:hidden flex items-center gap-2 mb-stack-lg">
            <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
              <span className="material-symbols-outlined text-white text-[18px]" style={{ fontVariationSettings: "'FILL' 1" }}>neurology</span>
            </div>
            <span className="font-headline-sm text-headline-sm font-bold text-enterprise-navy tracking-tight">AISAM</span>
          </div>

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
                className="inline-flex items-center gap-2 bg-primary-container text-on-primary font-body-md font-bold py-3 px-6 rounded-lg hover:bg-primary transition-all"
              >
                <span className="material-symbols-outlined text-[18px]">arrow_back</span>
                Back to Sign In
              </Link>
              <p className="mt-6 font-body-sm text-body-sm text-on-surface-variant">
                Didn&apos;t receive the email?{" "}
                <button onClick={() => setIsSuccess(false)} className="text-primary font-semibold hover:underline">
                  Try again
                </button>
              </p>
            </div>
          ) : (
            /* Form */
            <>
              <div className="mb-stack-lg">
                <h2 className="font-headline-lg text-headline-lg text-on-surface mb-1">Reset password</h2>
                <p className="font-body-md text-body-md text-outline">Enter your account email and we&apos;ll send a reset link.</p>
              </div>

              {error && (
                <div className="mb-stack-md p-stack-md bg-error-container/50 border border-error/20 rounded-lg flex items-start gap-3">
                  <span className="material-symbols-outlined text-error shrink-0" style={{ fontVariationSettings: "'FILL' 1" }}>error</span>
                  <p className="font-body-sm text-body-sm text-on-error-container">{error}</p>
                </div>
              )}

              <form onSubmit={handleSubmit} className="space-y-stack-md">
                <div>
                  <label htmlFor="email" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
                    Email Address
                  </label>
                  <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="name@company.com"
                    required
                    className="w-full px-4 py-3 bg-white border border-outline-variant rounded-lg focus:ring-2 focus:ring-primary-container focus:border-primary transition-all outline-none font-body-md placeholder:text-outline-variant text-on-surface"
                  />
                </div>

                <div className="pt-2">
                  <button
                    type="submit"
                    disabled={isLoading}
                    className="w-full bg-primary-container text-on-primary font-body-md font-bold py-3 px-6 rounded-lg shadow-sm hover:bg-primary transition-all active:scale-[0.98] disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                  >
                    {isLoading ? (
                      <>
                        <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        Sending reset link...
                      </>
                    ) : (
                      <>
                        <span className="material-symbols-outlined text-[18px]">send</span>
                        Send Reset Link
                      </>
                    )}
                  </button>
                </div>
              </form>

              <div className="mt-stack-lg text-center">
                <p className="font-body-sm text-body-sm text-on-surface-variant">
                  Remember your password?{" "}
                  <Link href="/login" className="text-primary font-semibold hover:underline decoration-2 underline-offset-4">
                    Sign In
                  </Link>
                </p>
              </div>

              <div className="mt-12 pt-stack-lg border-t border-outline-variant/30 flex items-center justify-center gap-2">
                <span className="material-symbols-outlined text-[18px] text-success-green">lock</span>
                <span className="font-label-sm text-label-sm text-outline uppercase tracking-wider">Secure, encrypted link</span>
              </div>
            </>
          )}
        </div>
      </section>
    </main>
  );
}
