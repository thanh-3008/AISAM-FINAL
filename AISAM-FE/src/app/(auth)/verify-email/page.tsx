"use client";

import { useEffect, useState, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import AuthShell from "@/components/auth/AuthShell";
import Link from "next/link";
import { motion } from "framer-motion";
import { useToast } from "@/contexts/ToastContext";

function VerifyEmailContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const { showToast } = useToast();

  useEffect(() => {
    let isMounted = true;
    if (!token) {
      setStatus("error");
      showToast({
        type: "error",
        title: "Verification Failed",
        message: "Invalid or expired verification token.",
      });
      return;
    }

    const verify = async () => {
      try {
        const result = await apiClient(`/auth/verify-email?token=${encodeURIComponent(token)}`, {
          method: "GET",
        });
        
        if (isMounted) {
          if (result.success) {
            setStatus("success");
            showToast({
              type: "success",
              title: "Email Verified",
              message: "Email verified successfully. You can now login.",
            });
          } else {
            setStatus("error");
            showToast({
              type: "error",
              title: "Verification Failed",
              message: "Invalid or expired verification token.",
            });
          }
        }
      } catch (e) {
        if (isMounted) {
          setStatus("error");
          showToast({
            type: "error",
            title: "Verification Failed",
            message: "Invalid or expired verification token.",
          });
        }
      }
    };

    verify();
    return () => {
      isMounted = false;
    };
  }, [token]);

  return (
    <div className="w-full max-w-md mx-auto p-8 rounded-2xl bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30 shadow-xl relative z-10 text-center">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-on-surface tracking-tight mb-3">Email Verification</h1>
      </div>

      {status === "loading" && (
        <div className="flex flex-col items-center gap-4 py-8">
          <div className="relative">
            <div className="w-10 h-10 border-[3px] border-primary/20 rounded-full" />
            <div className="absolute inset-0 w-10 h-10 border-[3px] border-primary border-t-transparent rounded-full animate-spin" />
          </div>
          <p className="text-body-sm text-on-surface font-medium">Verifying your email...</p>
        </div>
      )}

      {status === "success" && (
        <motion.div initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
          <div className="w-16 h-16 mx-auto rounded-full bg-success-green/10 flex items-center justify-center mb-6">
            <span className="material-symbols-outlined text-success-green text-3xl">check_circle</span>
          </div>
          <p className="text-body-md text-on-surface-variant mb-6">
            Email verified successfully. You can now login.
          </p>
          <Link
            href="/login"
            className="w-full py-3.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all flex items-center justify-center shadow-md shadow-primary/20"
          >
            Go to Login
          </Link>
        </motion.div>
      )}

      {status === "error" && (
        <motion.div initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
          <div className="w-16 h-16 mx-auto rounded-full bg-red-50 flex items-center justify-center mb-6">
            <span className="material-symbols-outlined text-red-500 text-3xl">error</span>
          </div>
          <p className="text-body-md text-on-surface-variant mb-6">
            Invalid or expired verification token.
          </p>
          <div className="flex flex-col gap-3">
            <Link
              href="/resend-verification"
              className="w-full py-3.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all flex items-center justify-center shadow-md shadow-primary/20"
            >
              Resend Verification Email
            </Link>
            <Link
              href="/login"
              className="w-full py-3.5 border border-outline-variant/40 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-all flex items-center justify-center"
            >
              Back to Login
            </Link>
          </div>
        </motion.div>
      )}
    </div>
  );
}

export default function VerifyEmailPage() {
  return (
    <AuthShell
      badge={{ icon: "verified", text: "Email Verification" }}
      headline="Confirm your"
      headlineHighlight="email address"
      description="Verify your email to secure your AISAM account and start your journey."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
    >
      <Suspense fallback={<div className="text-center p-8">Loading...</div>}>
        <VerifyEmailContent />
      </Suspense>
    </AuthShell>
  );
}
