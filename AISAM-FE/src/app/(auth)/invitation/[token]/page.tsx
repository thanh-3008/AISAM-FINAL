"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { motion } from "motion/react";
import { acceptInvitation, validateInvitation } from "@/services/workspaceInvitationService";
import { getToken } from "@/lib/auth";

type Status = "ready" | "accepting" | "success" | "error";

export default function AcceptInvitationPage() {
  const { token } = useParams<{ token: string }>();
  const router = useRouter();
  const [status, setStatus] = useState<Status>("ready");
  const [errorMessage, setErrorMessage] = useState<string>("");
  const [successWorkspace, setSuccessWorkspace] = useState<string>("");

  const [isMounted, setIsMounted] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    let cancelled = false;
    const checkToken = async () => {
      const res = await validateInvitation(token);
      if (cancelled) return;
      
      if (!res.valid) {
        setStatus("error");
        setErrorMessage(res.message || "Invitation not found.");
        setIsMounted(true);
        return;
      }
      
      const loggedIn = !!getToken();
      setIsLoggedIn(loggedIn);
      setIsMounted(true);
      if (!loggedIn) {
        router.push(`/login?redirect=/invitation/${token}`);
      }
    };
    checkToken();
    return () => { cancelled = true; };
  }, [router, token]);

  if (!isMounted) {
    return null;
  }
  
  if (status !== "error" && !isLoggedIn) {
    return null;
  }

  const handleAccept = async () => {
    if (!token) return;

    const isLoggedIn = !!getToken();
    if (!isLoggedIn) {
      router.push(`/login?redirect=/invitation/${token}`);
      return;
    }

    setStatus("accepting");
    const result = await acceptInvitation(token);

    if (result.success) {
      setStatus("success");
      setSuccessWorkspace(result.workspaceId || "");
      if (result.workspaceId) {
        setTimeout(() => {
          router.push(`/overview`);
        }, 2000);
      }
    } else {
      if (result.message?.includes("Phiên đăng nhập hết hạn") || result.message?.includes("Authentication is required")) {
        router.push(`/login?redirect=/invitation/${token}`);
        return;
      }
      setStatus("error");
      setErrorMessage(result.message || "Failed to accept invitation");
    }
  };

  return (
    <div className="min-h-[100dvh] bg-surface flex items-center justify-center p-4">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4, ease: [0.16, 1, 0.3, 1] }}
        className="w-full max-w-md"
      >
        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-xl overflow-hidden">
          {/* Header */}
          <div className="bg-gradient-to-r from-primary to-primary-container p-6 text-center">
            <div className="w-16 h-16 rounded-2xl bg-white/20 backdrop-blur-sm flex items-center justify-center mx-auto mb-4">
              <span className="material-symbols-outlined text-white text-3xl">
                {status === "error" ? "error_outline" :
                 status === "success" ? "check_circle" :
                 "group_add"}
              </span>
            </div>
            <h1 className="text-xl font-bold text-white">
              {status === "success" ? "Welcome!" :
               status === "error" ? "Error" :
               "Workspace Invitation"}
            </h1>
          </div>

          {/* Content */}
          <div className="p-6">
            {/* Error */}
            {status === "error" && (
              <div className="text-center py-4">
                <div className="w-16 h-16 rounded-2xl bg-red-50 flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-red-500 text-3xl">error_outline</span>
                </div>
                <p className="text-body-md text-on-surface font-medium mb-2">{errorMessage}</p>
                <p className="text-body-sm text-on-surface-variant mb-6">Please check the link or contact the workspace owner.</p>
                <Link
                  href="/login"
                  className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all"
                >
                  <span className="material-symbols-outlined text-[18px]">login</span>
                  Go to Login
                </Link>
              </div>
            )}

            {/* Ready to accept */}
            {status === "ready" && (
              <div className="space-y-5">
                <div className="text-center">
                  <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center mx-auto mb-3 ring-1 ring-primary/20">
                    <span className="material-symbols-outlined text-primary text-2xl">workspaces</span>
                  </div>
                  <h2 className="text-body-lg font-bold text-on-surface">Workspace Invitation</h2>
                  <p className="text-body-sm text-on-surface-variant mt-1">You have been invited to join a workspace</p>
                </div>
                <div className="flex gap-3 pt-2">
                  <Link
                    href="/dashboard"
                    className="flex-1 px-4 py-3 border border-outline-variant/30 text-on-surface rounded-xl text-body-sm font-semibold hover:bg-surface-container transition-colors text-center"
                  >
                    Decline
                  </Link>
                  <button
                    onClick={handleAccept}
                    className="flex-1 px-4 py-3 bg-gradient-to-r from-primary to-primary-container text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm shadow-primary/20 flex items-center justify-center gap-2"
                  >
                    <span className="material-symbols-outlined text-[18px]">check</span>
                    Accept & Join
                  </button>
                </div>
              </div>
            )}

            {/* Accepting */}
            {status === "accepting" && (
              <div className="text-center py-8">
                <div className="w-12 h-12 border-3 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-4" />
                <p className="text-body-md text-on-surface font-medium mb-1">Joining workspace...</p>
                <p className="text-body-sm text-on-surface-variant">Please wait while we set up your access</p>
              </div>
            )}

            {/* Success */}
            {status === "success" && (
              <div className="text-center py-4">
                <div className="w-16 h-16 rounded-2xl bg-emerald-50 flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-emerald-500 text-3xl">check_circle</span>
                </div>
                <p className="text-body-md text-on-surface font-medium mb-2">Invitation accepted!</p>
                <p className="text-body-sm text-on-surface-variant mb-6">You are now a member of the workspace. Redirecting...</p>
                <Link
                  href="/dashboard"
                  className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all"
                >
                  <span className="material-symbols-outlined text-[18px]">dashboard</span>
                  Go to Dashboard
                </Link>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <p className="text-center text-label-xs text-on-surface-variant mt-4">
          Powered by <span className="font-semibold text-primary">AISAM</span>
        </p>
      </motion.div>
    </div>
  );
}
