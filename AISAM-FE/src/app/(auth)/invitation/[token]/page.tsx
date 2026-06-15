"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { motion } from "motion/react";
import { getInvitationByToken, acceptInvitation, type InvitationDetail } from "@/services/workspaceInvitationService";
import { getToken } from "@/lib/auth";

type Status = "loading" | "invalid" | "expired" | "ready" | "accepting" | "success" | "error";

const roleConfig: Record<string, { label: string; icon: string; color: string; bg: string; desc: string }> = {
  Owner: { label: "Owner", icon: "star", color: "text-amber-600", bg: "bg-amber-50", desc: "Full access to all features" },
  Manager: { label: "Manager", icon: "manage_accounts", color: "text-blue-600", bg: "bg-blue-50", desc: "Manage content and campaigns" },
  ContentCreator: { label: "Content Creator", icon: "edit_note", color: "text-emerald-600", bg: "bg-emerald-50", desc: "Create and publish content" },
  Viewer: { label: "Viewer", icon: "visibility", color: "text-outline", bg: "bg-surface-container", desc: "View dashboard and analytics" },
};

const quotaModeConfig: Record<string, { label: string; icon: string; desc: string }> = {
  SharedPool: { label: "Shared Pool", icon: "pool", desc: "Use workspace's shared credit pool" },
  LifetimeAssigned: { label: "Lifetime Limit", icon: "lock_clock", desc: "Fixed credit limit, never resets" },
  MonthlyAssigned: { label: "Monthly Limit", icon: "calendar_month", desc: "Credit limit resets monthly" },
};

export default function AcceptInvitationPage() {
  const { token } = useParams<{ token: string }>();
  const router = useRouter();
  const [status, setStatus] = useState<Status>("loading");
  const [invitation, setInvitation] = useState<InvitationDetail | null>(null);
  const [errorMessage, setErrorMessage] = useState<string>("");

  useEffect(() => {
    if (!token) {
      const id = setTimeout(() => {
        setStatus("invalid");
        setErrorMessage("Invalid invitation link");
      }, 0);
      return () => clearTimeout(id);
    }

    const loadInvitation = async () => {
      const data = await getInvitationByToken(token);
      if (!data) {
        setStatus("invalid");
        setErrorMessage("This invitation link is invalid or has expired");
        return;
      }

      if (data.status === "Expired") {
        setStatus("expired");
        return;
      }

      if (data.status === "Accepted") {
        setInvitation(data);
        setStatus("success");
        return;
      }

      if (data.status === "Cancelled") {
        setStatus("invalid");
        setErrorMessage("This invitation has been cancelled");
        return;
      }

      setInvitation(data);
      setStatus("ready");
    };

    loadInvitation();
  }, [token]);

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
      if (result.workspaceId) {
        setTimeout(() => {
          router.push(`/profiles/${result.workspaceId}?section=overview`);
        }, 2000);
      }
    } else {
      setStatus("error");
      setErrorMessage(result.message || "Failed to accept invitation");
    }
  };

  const role = invitation ? roleConfig[invitation.role] : null;
  const quotaMode = invitation?.quotaMode ? quotaModeConfig[invitation.quotaMode] : null;

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
                {status === "loading" ? "hourglass_empty" :
                 status === "invalid" || status === "expired" || status === "error" ? "error_outline" :
                 status === "success" ? "check_circle" :
                 "group_add"}
              </span>
            </div>
            <h1 className="text-xl font-bold text-white">
              {status === "loading" ? "Loading..." :
               status === "invalid" ? "Invalid Invitation" :
               status === "expired" ? "Invitation Expired" :
               status === "success" ? "Welcome!" :
               status === "error" ? "Error" :
               "Workspace Invitation"}
            </h1>
          </div>

          {/* Content */}
          <div className="p-6">
            {/* Loading */}
            {status === "loading" && (
              <div className="text-center py-8">
                <div className="w-12 h-12 border-3 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-4" />
                <p className="text-body-sm text-on-surface-variant">Loading invitation details...</p>
              </div>
            )}

            {/* Invalid / Expired / Error */}
            {(status === "invalid" || status === "expired" || status === "error") && (
              <div className="text-center py-4">
                <div className="w-16 h-16 rounded-2xl bg-red-50 flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-red-500 text-3xl">
                    {status === "expired" ? "schedule" : "error_outline"}
                  </span>
                </div>
                <p className="text-body-md text-on-surface font-medium mb-2">
                  {status === "expired" ? "This invitation has expired" : errorMessage}
                </p>
                <p className="text-body-sm text-on-surface-variant mb-6">
                  {status === "expired"
                    ? "Please contact the workspace owner for a new invitation."
                    : "Please check the link or contact the workspace owner."}
                </p>
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
            {status === "ready" && invitation && role && (
              <div className="space-y-5">
                {/* Workspace Info */}
                <div className="text-center">
                  <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center mx-auto mb-3 ring-1 ring-primary/20">
                    <span className="material-symbols-outlined text-primary text-2xl">workspaces</span>
                  </div>
                  <h2 className="text-body-lg font-bold text-on-surface">{invitation.workspaceName}</h2>
                  <p className="text-body-sm text-on-surface-variant mt-1">
                    {invitation.workspaceType === 2 ? "Business Workspace" : "Personal Workspace"}
                  </p>
                </div>

                {/* Invited by */}
                <div className="bg-surface-container/50 rounded-xl p-4 flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-primary font-bold text-body-sm">
                    {invitation.invitedByName.charAt(0).toUpperCase()}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-body-sm text-on-surface font-medium truncate">{invitation.invitedByName}</p>
                    <p className="text-label-xs text-on-surface-variant truncate">invited you to join</p>
                  </div>
                </div>

                {/* Role */}
                <div className={`${role.bg} rounded-xl p-4 border border-outline-variant/10`}>
                  <div className="flex items-center gap-3 mb-2">
                    <div className="w-9 h-9 rounded-lg bg-white/80 flex items-center justify-center">
                      <span className={`material-symbols-outlined ${role.color} text-[20px]`}>{role.icon}</span>
                    </div>
                    <div>
                      <p className="text-body-sm font-semibold text-on-surface">Role: {role.label}</p>
                      <p className="text-label-xs text-on-surface-variant">{role.desc}</p>
                    </div>
                  </div>
                </div>

                {/* Quota Mode (if applicable) */}
                {quotaMode && invitation.creditLimit && (
                  <div className="bg-surface-container/30 rounded-xl p-4 border border-outline-variant/10">
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 rounded-lg bg-primary/5 flex items-center justify-center">
                        <span className="material-symbols-outlined text-primary text-[20px]">{quotaMode.icon}</span>
                      </div>
                      <div className="flex-1">
                        <p className="text-body-sm font-semibold text-on-surface">{quotaMode.label}</p>
                        <p className="text-label-xs text-on-surface-variant">{quotaMode.desc}</p>
                      </div>
                      <span className="text-body-sm font-bold text-primary">{invitation.creditLimit.toLocaleString()} credits</span>
                    </div>
                  </div>
                )}

                {/* Email */}
                <div className="text-center">
                  <p className="text-label-sm text-on-surface-variant">
                    Joining as <span className="font-semibold text-on-surface">{invitation.email}</span>
                  </p>
                </div>

                {/* Actions */}
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
                <p className="text-body-md text-on-surface font-medium mb-2">
                  {invitation ? `Welcome to ${invitation.workspaceName}!` : "Invitation accepted!"}
                </p>
                <p className="text-body-sm text-on-surface-variant mb-6">
                  You are now a member of the workspace. Redirecting...
                </p>
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
