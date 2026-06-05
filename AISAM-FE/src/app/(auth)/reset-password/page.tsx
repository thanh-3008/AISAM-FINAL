"use client";

import Link from "next/link";
import { useState, useEffect, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/apiClient";

function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get("token") || "";
  const emailFromUrl = searchParams.get("email") || "";

  const [form, setForm] = useState({ email: emailFromUrl, newPassword: "", confirmPassword: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (form.newPassword !== form.confirmPassword) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }

    setIsLoading(true);
    try {
      await apiClient("/auth/reset-password", {
        data: {
          email: form.email,
          token,
          newPassword: form.newPassword,
          confirmPassword: form.confirmPassword,
        },
      });
      setIsSuccess(true);
      setTimeout(() => router.push("/login"), 3000);
    } catch (err: any) {
      setError(err.message || "Token không hợp lệ hoặc đã hết hạn.");
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
            <span className="material-symbols-outlined text-[14px]">shield_lock</span>
            SECURE RESET
          </div>
          <h1 className="font-display-lg text-display-lg text-surface-bright mb-stack-md leading-tight">
            Create a <span className="text-primary-fixed-dim">new password</span> for your account.
          </h1>
          <p className="font-body-lg text-body-lg text-outline-variant max-w-xl">
            Choose a strong password that&apos;s at least 8 characters long with a mix of letters and numbers.
          </p>
        </div>

        {/* Stats */}
        <div className="relative z-10 flex gap-gutter">
          <div className="flex-1 p-stack-md rounded-xl bg-white/5 backdrop-blur-xl border border-white/10">
            <span className="font-label-md text-label-md text-primary-fixed-dim block mb-1">Encryption</span>
            <span className="font-headline-md text-headline-md text-surface-bright">AES-256</span>
          </div>
          <div className="flex-1 p-stack-md rounded-xl bg-white/5 backdrop-blur-xl border border-white/10">
            <span className="font-label-md text-label-md text-primary-fixed-dim block mb-1">Protected by</span>
            <span className="font-headline-md text-headline-md text-surface-bright">JWT</span>
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

          {!token ? (
            /* No Token */
            <div className="text-center">
              <div className="w-16 h-16 bg-error-container/50 rounded-2xl flex items-center justify-center mx-auto mb-6">
                <span className="material-symbols-outlined text-error text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>link_off</span>
              </div>
              <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Invalid Reset Link</h2>
              <p className="font-body-md text-body-md text-on-surface-variant mb-8">
                This link is invalid or has expired. Please request a new password reset.
              </p>
              <Link href="/forgot-password" className="inline-flex items-center gap-2 bg-primary-container text-on-primary font-body-md font-bold py-3 px-6 rounded-lg hover:bg-primary transition-all">
                Request New Link
              </Link>
            </div>
          ) : isSuccess ? (
            /* Success */
            <div className="text-center">
              <div className="w-16 h-16 bg-success-green/10 rounded-2xl flex items-center justify-center mx-auto mb-6">
                <span className="material-symbols-outlined text-success-green text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
              </div>
              <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">Password Reset!</h2>
              <p className="font-body-md text-body-md text-on-surface-variant mb-2">
                Your password has been successfully reset.
              </p>
              <p className="font-body-sm text-body-sm text-on-surface-variant">Redirecting to Sign In...</p>
              <Link href="/login" className="mt-6 inline-flex items-center gap-2 text-primary font-semibold hover:underline">
                <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
                Go to Sign In
              </Link>
            </div>
          ) : (
            /* Form */
            <>
              <div className="mb-stack-lg">
                <h2 className="font-headline-lg text-headline-lg text-on-surface mb-1">New Password</h2>
                <p className="font-body-md text-body-md text-outline">Create a strong, new password for your account.</p>
              </div>

              {error && (
                <div className="mb-stack-md p-stack-md bg-error-container/50 border border-error/20 rounded-lg flex items-start gap-3">
                  <span className="material-symbols-outlined text-error shrink-0" style={{ fontVariationSettings: "'FILL' 1" }}>error</span>
                  <p className="font-body-sm text-body-sm text-on-error-container">{error}</p>
                </div>
              )}

              <form onSubmit={handleSubmit} className="space-y-stack-md">
                {/* Email (hidden if from URL, visible if not) */}
                <div>
                  <label htmlFor="email" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
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
                    readOnly={!!emailFromUrl}
                    className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface ${emailFromUrl ? "opacity-60 cursor-not-allowed" : ""}`}
                  />
                </div>

                {/* New Password */}
                <div>
                  <label htmlFor="newPassword" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
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
                      required
                      minLength={8}
                      className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12"
                    />
                    <button type="button" onClick={() => setShowPassword(!showPassword)} className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors p-1">
                      <span className="material-symbols-outlined text-[20px]">{showPassword ? "visibility_off" : "visibility"}</span>
                    </button>
                  </div>
                </div>

                {/* Confirm Password */}
                <div>
                  <label htmlFor="confirmPassword" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
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
                      required
                  className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${
                    form.confirmPassword && form.newPassword !== form.confirmPassword ? "border-error" : "border-outline-variant"
                  }`}
                    />
                    <button type="button" onClick={() => setShowConfirm(!showConfirm)} className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors p-1">
                      <span className="material-symbols-outlined text-[20px]">{showConfirm ? "visibility_off" : "visibility"}</span>
                    </button>
                  </div>
                  {form.confirmPassword && form.newPassword !== form.confirmPassword && (
                    <p className="mt-1 ml-1 font-label-sm text-label-sm text-error">Mật khẩu không khớp</p>
                  )}
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
                        Resetting...
                      </>
                    ) : (
                      <>
                        <span className="material-symbols-outlined text-[18px]">lock_reset</span>
                        Reset Password
                      </>
                    )}
                  </button>
                </div>
              </form>

              <div className="mt-stack-lg text-center">
                <Link href="/login" className="font-body-sm text-body-sm text-primary font-semibold hover:underline">
                  ← Back to Sign In
                </Link>
              </div>

              <div className="mt-12 pt-stack-lg border-t border-outline-variant/30 flex items-center justify-center gap-2">
                <span className="material-symbols-outlined text-[18px] text-success-green">lock</span>
                <span className="text-label-sm text-outline">Secure, encrypted reset</span>
              </div>
            </>
          )}
        </div>
      </section>
    </main>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center"><span className="text-on-surface-variant">Loading...</span></div>}>
      <ResetPasswordForm />
    </Suspense>
  );
}
