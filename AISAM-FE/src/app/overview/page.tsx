"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { motion, useReducedMotion } from "motion/react";
import { getUserIdFromToken, getUserFromToken, getStoredUser } from "@/lib/auth";
import { useWorkspaces, addWorkspaceToCache, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { storeActiveProfile, clearActiveProfile } from "@/stores/profile-store";
import { apiClient, apiFetch } from "@/lib/apiClient";
import type { WorkspaceData } from "@/hooks/useWorkspaces";

interface PendingWorkspace {
  id: string;
  name: string;
  workspaceType: number;
  companyName: string | null;
  bio: string;
  brandCount: number;
  campaignCount: number;
  pendingCreate: true;
  icon: string;
  badge: string;
  badgeClass: string;
}

type DisplayWorkspace = WorkspaceData | PendingWorkspace;

const container = {
  hidden: { opacity: 0 },
  show: {
    opacity: 1,
    transition: {
      staggerChildren: 0.08,
      delayChildren: 0.1,
    },
  },
};

const item = {
  hidden: { opacity: 0, y: 20 },
  show: { opacity: 1, y: 0, transition: { duration: 0.5, ease: [0.16, 1, 0.3, 1] as const } },
};

export default function OverviewPage() {
  const router = useRouter();
  const { workspaces, loading, activeWorkspace, selectWorkspace } = useWorkspaces();
  const [creating, setCreating] = useState(false);
  const [toast, setToast] = useState<{ name: string } | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const reduceMotion = useReducedMotion();

  const createAndSelectWorkspace = async (name: string, workspaceType: number, companyName?: string) => {
    setCreating(true);
    setCreateError(null);
    const userId = getUserIdFromToken();
    if (!userId) return;

    try {
      // 1. Tạo Workspace thật trong DB
      const wsResult = await apiClient("/workspaces", {
        method: "POST",
        data: { name, workspaceType },
      });

      if (!wsResult?.success || !wsResult.data) {
        setCreateError(wsResult?.message || "Tạo workspace thất bại.");
        return;
      }

      // 2. Tạo Profile (để có X-Profile-Id)
      const formBody = new FormData();
      formBody.append("name", name);
      formBody.append("profileType", workspaceType.toString());
      if (companyName) formBody.append("companyName", companyName);

      const pfResult = await apiFetch(`/profiles/user/${userId}`, {
        method: "POST",
        body: formBody,
      });

      const wsId = wsResult.data.id;

      const wsData: WorkspaceData = {
        id: wsId,
        userId,
        name: wsResult.data.name || name,
        workspaceType: wsResult.data.workspaceType ?? workspaceType,
        plan: workspaceType === 2 ? "Business" : "Personal",
        status: wsResult.data.status ?? 1,
        createdAt: wsResult.data.createdAt || new Date().toISOString(),
        updatedAt: wsResult.data.updatedAt || new Date().toISOString(),
        isOwner: true,
        memberRole: "Owner",
      };
      addWorkspaceToCache(wsData);
      selectWorkspace(wsData);
      
      if (pfResult?.success && pfResult.data?.id) {
        storeActiveProfile({
          id: pfResult.data.id,
          name: pfResult.data.name || wsData.name,
          profileType: workspaceType,
        });
      } else {
        clearActiveProfile();
      }
    } catch (e: any) {
      setCreateError(e?.message || "Lỗi kết nối khi tạo workspace.");
      return;
    } finally {
      setCreating(false);
    }
  };

  const handleSelect = async (workspace: DisplayWorkspace) => {
    if ('pendingCreate' in workspace && workspace.pendingCreate) {
      if (workspace.workspaceType === 1) {
        const storedUser = getStoredUser();
        const tokenUser = getUserFromToken();
        const displayName = storedUser?.fullName || tokenUser?.name || "";
        const email = storedUser?.email || tokenUser?.email || "user";
        const wsName = displayName ? `${displayName}'s Workspace` : email.split("@")[0] + "'s Workspace";
        await createAndSelectWorkspace(wsName, workspace.workspaceType);
        setToast({ name: wsName });
        setTimeout(() => router.push("/dashboard"), 2000);
      } else {
        router.push("/pricing?create=business");
      }
    } else {
      const w = workspace as WorkspaceData;
      selectWorkspace(w);
      const userId = getUserIdFromToken();
      if (userId) {
        try {
          const existing = await apiClient(`/profiles/user/${userId}`);
          if (existing?.success && existing?.data?.length) {
            const p = existing.data[0];
            storeActiveProfile({ id: p.id, name: p.name, profileType: p.profileType });
          } else {
            const fd = new FormData();
            fd.append("name", w.name);
            fd.append("profileType", w.workspaceType.toString());
            const pfResult = await apiFetch(`/profiles/user/${userId}`, { method: "POST", body: fd });
            if (pfResult?.success && pfResult.data?.id) {
              storeActiveProfile({ id: pfResult.data.id, name: pfResult.data.name || w.name, profileType: w.workspaceType });
            }
          }
        } catch { clearActiveProfile(); }
      } else {
        clearActiveProfile();
      }
      setToast({ name: workspace.name });
      setTimeout(() => router.push("/dashboard"), 2000);
    }
  };

  if (loading || creating) {
    return (
      <main className="min-h-[100dvh] bg-surface flex items-center justify-center">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.3 }}
          className="flex flex-col items-center gap-4"
        >
          <div className="relative">
            <div className="w-10 h-10 border-[3px] border-primary/20 rounded-full" />
            <div className="absolute inset-0 w-10 h-10 border-[3px] border-primary border-t-transparent rounded-full animate-spin" />
          </div>
          <div className="text-center">
            <p className="text-body-sm text-on-surface font-medium">
              {creating ? "Creating workspace" : "Loading profiles"}
            </p>
            <p className="text-label-xs text-outline mt-1">
              {creating ? "Setting up your environment..." : "Please wait..."}
            </p>
          </div>
        </motion.div>
      </main>
    );
  }

  const hasWorkspaces = workspaces.length > 0;
  const displayWorkspaces: DisplayWorkspace[] = hasWorkspaces ? workspaces : [
    {
      id: "pending-personal",
      name: "Personal Workspace",
      workspaceType: 1,
      companyName: null,
      bio: "Individual creator accounts and small brand projects.",
      brandCount: 0,
      campaignCount: 0,
      pendingCreate: true,
      icon: "person",
      badge: "Free",
      badgeClass: "bg-success-green/10 text-success-green border-success-green/20",
    },
    {
      id: "pending-business",
      name: "Business Workspace",
      workspaceType: 2,
      companyName: null,
      bio: "Team collaboration with advanced analytics and multi-brand support.",
      brandCount: 0,
      campaignCount: 0,
      pendingCreate: true,
      icon: "business",
      badge: "Pro",
      badgeClass: "bg-primary/10 text-primary border-primary/20",
    },
  ];

  const MotionDiv = motion.div;

  return (
    <main className="min-h-[100dvh] bg-surface flex flex-col relative overflow-hidden">
      {/* Background decoration */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-primary/[0.03] rounded-full blur-[120px] -translate-y-1/2 translate-x-1/4" />
        <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-secondary/[0.03] rounded-full blur-[100px] translate-y-1/3 -translate-x-1/4" />
      </div>

      <div className="flex-1 px-6 md:px-8 lg:px-12 max-w-5xl mx-auto w-full flex flex-col justify-center items-center py-12 md:py-16 relative z-10">
        {/* Header - Centered */}
        <MotionDiv
          initial={reduceMotion ? false : { opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] as const }}
          className="mb-10 md:mb-12 text-center w-full"
        >
          <div className="flex items-center justify-center gap-2.5 mb-6">
            <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
              <span className="material-symbols-outlined text-on-primary text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }}>psychology</span>
            </div>
            <span className="text-headline-sm font-bold text-on-surface tracking-tight">AISAM</span>
          </div>
          
          <h1 className="text-3xl md:text-4xl font-bold text-on-surface tracking-tight leading-tight mb-3">
            {hasWorkspaces ? "Choose your workspace" : "Get started"}
          </h1>
          <p className="text-body-md text-on-surface-variant max-w-md mx-auto leading-relaxed">
            {hasWorkspaces
              ? "Select a workspace to access your brands, campaigns, and analytics."
              : "Create your first workspace to start managing social ads with AI."}
          </p>

          {/* Go to Dashboard Button - Only show when has workspaces */}
          {hasWorkspaces && (
            <motion.button
              initial={reduceMotion ? false : { opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2, duration: 0.5 }}
              onClick={() => router.push("/dashboard")}
              className="mt-6 inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
            >
              <span className="material-symbols-outlined text-[18px]">dashboard</span>
              Go to Dashboard
            </motion.button>
          )}
        </MotionDiv>

        {/* Error */}
        {createError && (
          <motion.div initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}
            className="flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-body-sm text-red-800 mb-6 w-full">
            <span className="material-symbols-outlined text-red-500 text-[20px]">error</span>
            <span className="flex-1">{createError}</span>
            <button onClick={() => setCreateError(null)} className="text-red-400 hover:text-red-600">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </motion.div>
        )}

        {/* Profile Grid */}
        <MotionDiv
          variants={reduceMotion ? undefined : container}
          initial={reduceMotion ? undefined : "hidden"}
          animate="show"
          className={`grid ${hasWorkspaces ? "grid-cols-1 md:grid-cols-2 lg:grid-cols-3" : "grid-cols-1 sm:grid-cols-2"} gap-4 md:gap-5 w-full mb-10`}
        >
          {displayWorkspaces.map((workspace) => {
            const isPending = 'pendingCreate' in workspace && workspace.pendingCreate;
            const isActive = !isPending && activeWorkspace?.id === workspace.id;
            return (
              <MotionDiv
                key={workspace.id}
                variants={reduceMotion ? undefined : item}
                whileHover={reduceMotion ? undefined : { y: -4, transition: { duration: 0.2 } }}
                className={`group relative bg-surface-container-lowest/80 backdrop-blur-sm border ${
                  isActive 
                    ? "border-primary ring-2 ring-primary/20 shadow-lg shadow-primary/10" 
                    : "border-outline-variant/30 hover:border-primary/30 hover:shadow-lg hover:shadow-black/5"
                } p-5 rounded-2xl flex flex-col justify-between overflow-hidden transition-colors min-h-[240px]`}
              >
                {/* Active indicator */}
                {isActive && (
                  <div className="absolute top-3 right-3">
                    <div className="w-5 h-5 bg-primary rounded-full flex items-center justify-center">
                      <span className="material-symbols-outlined text-on-primary text-[12px]" style={{ fontVariationSettings: "'FILL' 1" }}>check</span>
                    </div>
                  </div>
                )}

                <div className="relative z-10">
                  {/* Icon & Badge */}
                  <div className="flex justify-between items-start mb-4">
                    <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                      isPending
                        ? (workspace as PendingWorkspace).workspaceType === 1
                          ? "bg-gradient-to-br from-secondary/10 to-secondary/5"
                          : "bg-gradient-to-br from-primary/10 to-primary/5"
                        : "bg-gradient-to-br from-primary/10 to-primary/5"
                    }`}>
                      <span className={`material-symbols-outlined text-[20px] ${
                        isPending
                          ? (workspace as PendingWorkspace).workspaceType === 1 ? "text-secondary" : "text-primary"
                          : "text-primary"
                      }`}>
                        {isPending ? (workspace as PendingWorkspace).icon : "account_circle"}
                      </span>
                    </div>
                    <span className={`px-2 py-0.5 text-label-2xs rounded-full font-semibold border ${('badgeClass' in workspace && (workspace as PendingWorkspace).badgeClass) || "bg-surface-container text-outline border-outline-variant/20"}`}>
                      {isPending ? (workspace as PendingWorkspace).badge : getWorkspaceTypeLabel((workspace as WorkspaceData).workspaceType)}
                    </span>
                  </div>

                  {/* Name */}
                  <h3 className="text-body-lg font-bold text-on-surface mb-1.5 group-hover:text-primary transition-colors">
                    {workspace.name}
                  </h3>

                  {/* Description */}
                  <p className="text-body-sm text-on-surface-variant mb-4 leading-relaxed line-clamp-2">
                    {isPending ? (workspace as PendingWorkspace).bio : ((workspace as WorkspaceData).bio || "No description yet.")}
                  </p>

                  {/* Stats */}
                  <div className="flex items-center gap-3 text-on-surface-variant">
                    {isPending ? (
                      <>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">workspaces</span>
                          <span className="text-label-xs font-medium">{(workspace as PendingWorkspace).brandCount} Brands</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">monitoring</span>
                          <span className="text-label-xs font-medium">{(workspace as PendingWorkspace).campaignCount} Campaigns</span>
                        </div>
                      </>
                    ) : (
                      <>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">person</span>
                          <span className="text-label-xs font-medium">{(workspace as WorkspaceData).isOwner ? "Owner" : (workspace as WorkspaceData).memberRole || "Member"}</span>
                        </div>
                      </>
                    )}
                  </div>
                </div>

                {/* Action Button */}
                <motion.button
                  whileTap={reduceMotion ? {} : { scale: 0.98 }}
                  className={`mt-4 w-full py-2.5 rounded-xl font-semibold text-label-sm transition-all ${
                    isActive
                      ? "bg-primary text-on-primary shadow-md shadow-primary/20"
                      : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-primary hover:text-on-primary hover:border-primary hover:shadow-md hover:shadow-primary/20"
                  } disabled:opacity-50 disabled:cursor-not-allowed`}
                  onClick={() => handleSelect(workspace)}
                  disabled={creating}
                >
                  {isActive ? "Continue" : isPending ? ((workspace as PendingWorkspace).workspaceType === 1 ? "Continue" : "Create & Select") : "Select"}
                </motion.button>

                {/* Subtle gradient decoration */}
                {isPending && (
                  <div className={`absolute -bottom-12 -right-12 w-32 h-32 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 ${
                    (workspace as PendingWorkspace).workspaceType === 1 
                      ? "bg-gradient-to-bl from-secondary/10 to-transparent" 
                      : "bg-gradient-to-bl from-primary/10 to-transparent"
                  }`} />
                )}
              </MotionDiv>
            );
          })}
        </MotionDiv>

        {/* Secondary Actions */}
        <MotionDiv
          initial={reduceMotion ? false : { opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.4, duration: 0.5 }}
          className="flex flex-col items-center gap-2 text-center"
        >
          {hasWorkspaces ? (
            <p className="text-body-sm text-on-surface-variant">
              Need help managing permissions?{" "}
              <a className="text-primary font-semibold hover:underline underline-offset-2" href="mailto:support@aisam.ai">Contact support</a>
            </p>
          ) : (
            <p className="text-body-sm text-on-surface-variant">
              Already have workspaces?{" "}
              <button 
                className="text-primary font-semibold hover:underline underline-offset-2 bg-transparent border-none p-0 cursor-pointer" 
                onClick={() => router.push("/profiles")}
              >
                View all workspaces
              </button>
            </p>
          )}
        </MotionDiv>
      </div>

      {/* Success Toast */}
      <motion.div
        initial={reduceMotion ? false : { y: 100, opacity: 0 }}
        animate={toast ? { y: 0, opacity: 1 } : { y: 100, opacity: 0 }}
        transition={{ type: "spring", stiffness: 300, damping: 30 }}
        className={`fixed bottom-6 left-1/2 -translate-x-1/2 bg-enterprise-navy text-white px-5 py-3 rounded-xl shadow-2xl flex items-center gap-3 z-50`}
      >
        <div className="w-6 h-6 bg-success-green rounded-full flex items-center justify-center shrink-0">
          <span className="material-symbols-outlined text-[14px]" style={{ fontVariationSettings: "'FILL' 1" }}>check</span>
        </div>
        <div>
          <p className="text-body-sm font-semibold">
            Workspace <span className="text-primary-fixed-dim">{toast?.name}</span> selected
          </p>
          <p className="text-label-xs text-outline-variant">Loading your dashboard...</p>
        </div>
      </motion.div>

    </main>
  );
}
