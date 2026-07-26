"use client";

import { useState } from "react";
import { apiClient } from "@/lib/apiClient";
import AuthShell from "@/components/auth/AuthShell";
import Link from "next/link";
import { motion } from "framer-motion";
import { useReducedMotion } from "framer-motion";

export default function ResendVerificationPage() {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [success, setSuccess] = useState(false);
  const reduceMotion = useReducedMotion();

  const validate = () => {
    const newErrors: Record<string, string> = {};
    const trimmedEmail = email.trim();
    if (!trimmedEmail) {
      newErrors.email = "Email is required";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      newErrors.email = "Please enter a valid email address";
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
      const result = await apiClient("/auth/verify-email/resend", {
        method: "POST",
        data: { email: email.trim() },
      });

      if (result.success) {
        setSuccess(true);
      } else {
        setError(result.message || "Failed to resend verification email.");
      }
    } catch (err: any) {
      setError(err.message || "An unexpected error occurred.");
    } finally {
      setIsLoading(false);
    }
  };

  const inputClass = "w-full bg-surface-container-low border border-outline-variant/30 text-on-surface rounded-xl px-4 py-3 text-body-md focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all placeholder:text-outline-variant/50";
  const labelClass = "block text-label-sm font-semibold text-on-surface-variant mb-1.5";

  return (
    <AuthShell
      badge={{ icon: "mark_email_unread", text: "Resend Verification" }}
      headline="Didn't get the"
      headlineHighlight="email?"
      description="Enter your email to receive a new verification link to secure your account."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
    >
      <div className="w-full max-w-md mx-auto p-8 rounded-2xl bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30 shadow-xl relative z-10">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-on-surface tracking-tight mb-3">Resend Verification</h1>
          <p className="text-body-md text-on-surface-variant leading-relaxed">
            Enter your email to receive a new verification link.
          </p>
        </div>

        {error && (
          <motion.div initial={reduceMotion ? undefined : { opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}
            className="flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-body-sm text-red-800 mb-6">
            <span className="material-symbols-outlined text-red-500 text-[20px]">error</span>
            <span className="flex-1">{error}</span>
            <button onClick={() => setError(null)} className="text-red-400 hover:text-red-600 transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </motion.div>
        )}

        {success ? (
          <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} className="text-center">
            <div className="w-16 h-16 mx-auto bg-primary/10 rounded-full flex items-center justify-center mb-6">
              <span className="material-symbols-outlined text-primary text-[32px]">mark_email_read</span>
            </div>
            <p className="text-body-md text-on-surface-variant mb-8 leading-relaxed">
              If the email exists and is not verified, a verification email has been sent.
            </p>
            <Link
              href="/login"
              className="inline-flex items-center gap-2 px-5 py-3 bg-surface-container text-on-surface border border-outline-variant/20 rounded-xl font-semibold text-body-sm hover:bg-surface-container-high transition-all shadow-sm"
            >
              <span className="material-symbols-outlined text-[18px]">arrow_back</span>
              Back to Login
            </Link>
          </motion.div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5" noValidate>
            <div>
              <label htmlFor="email" className={labelClass}>Email Address</label>
              <div className="relative">
                <input
                  id="email"
                  name="email"
                  type="email"
                  placeholder="you@example.com"
                  className={inputClass}
                  value={email}
                  onChange={(e) => {
                    setEmail(e.target.value);
                    if (errors.email) setErrors({ ...errors, email: "" });
                  }}
                  disabled={isLoading}
                />
                <span className="material-symbols-outlined absolute right-4 top-1/2 -translate-y-1/2 text-outline-variant text-[20px] pointer-events-none">mail</span>
              </div>
              {errors.email && <p className="text-red-500 text-sm mt-1">{errors.email}</p>}
            </div>

            <motion.button
              whileHover={reduceMotion ? undefined : { scale: 1.01 }}
              whileTap={reduceMotion ? undefined : { scale: 0.98 }}
              type="submit"
              disabled={isLoading}
              className="w-full py-3.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 shadow-md shadow-primary/20 mt-8"
            >
              {isLoading ? (
                <>
                  <svg className="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                  </svg>
                  Sending...
                </>
              ) : (
                <>
                  Send Verification Link
                  <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
                </>
              )}
            </motion.button>
            <div className="text-center mt-6">
              <Link href="/login" className="text-body-sm text-on-surface-variant hover:text-primary transition-colors font-medium">
                Back to Login
              </Link>
            </div>
          </form>
        )}
      </div>
    </AuthShell>
  );
}
