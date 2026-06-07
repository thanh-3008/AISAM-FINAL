"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { motion, useReducedMotion } from "motion/react";
import { getUserIdFromToken } from "@/lib/auth";
import { useProfiles, addProfileToCache, getProfileTypeLabel } from "@/hooks/useProfiles";
import { apiFetch } from "@/lib/apiClient";
import type { Profile } from "@/hooks/useProfiles";

interface PendingProfile {
  id: string;
  name: string;
  profileType: number;
  companyName: string | null;
  bio: string;
  brandCount: number;
  campaignCount: number;
  pendingCreate: true;
  icon: string;
  badge: string;
  badgeClass: string;
}

type DisplayProfile = Profile | PendingProfile;

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
  const { profiles, loading, activeProfile, selectProfile } = useProfiles();
  const [creating, setCreating] = useState(false);
  const [toast, setToast] = useState<{ name: string } | null>(null);
  const reduceMotion = useReducedMotion();

  const createAndSelectProfile = async (name: string, profileType: number, companyName?: string) => {
    setCreating(true);
    const userId = getUserIdFromToken();
    if (!userId) return;

    try {
      const formBody = new FormData();
      formBody.append("name", name);
      formBody.append("profileType", profileType.toString());
      if (companyName) formBody.append("companyName", companyName);

      const result = await apiFetch(`/profiles/user/${userId}`, {
        method: "POST",
        body: formBody,
      });

      if (result?.success && result.data) {
        addProfileToCache(result.data);
        selectProfile(result.data);
      }
    } catch {
      // silent
    } finally {
      setCreating(false);
    }
  };

  const handleSelect = async (profile: DisplayProfile) => {
    if ('pendingCreate' in profile && profile.pendingCreate) {
      await createAndSelectProfile(profile.name, profile.profileType, profile.companyName ?? undefined);
    } else {
      selectProfile(profile as Profile);
    }
    setToast({ name: profile.name });
    setTimeout(() => {
      router.push("/dashboard");
    }, 2000);
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

  const hasProfiles = profiles.length > 0;
  const displayProfiles: DisplayProfile[] = hasProfiles ? profiles : [
    {
      id: "pending-personal",
      name: "Personal Workspace",
      profileType: 0,
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
      profileType: 1,
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
            {hasProfiles ? "Choose your workspace" : "Get started"}
          </h1>
          <p className="text-body-md text-on-surface-variant max-w-md mx-auto leading-relaxed">
            {hasProfiles
              ? "Select a profile to access your brands, campaigns, and analytics."
              : "Create your first workspace to start managing social ads with AI."}
          </p>
        </MotionDiv>

        {/* Profile Grid */}
        <MotionDiv
          variants={reduceMotion ? undefined : container}
          initial={reduceMotion ? undefined : "hidden"}
          animate="show"
          className={`grid ${hasProfiles ? "grid-cols-1 md:grid-cols-2 lg:grid-cols-3" : "grid-cols-1 sm:grid-cols-2"} gap-4 md:gap-5 w-full mb-10`}
        >
          {displayProfiles.map((profile) => {
            const isPending = 'pendingCreate' in profile && profile.pendingCreate;
            const isActive = !isPending && activeProfile?.id === profile.id;
            return (
              <MotionDiv
                key={profile.id}
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
                        ? profile.profileType === 0
                          ? "bg-gradient-to-br from-secondary/10 to-secondary/5"
                          : "bg-gradient-to-br from-primary/10 to-primary/5"
                        : "bg-gradient-to-br from-primary/10 to-primary/5"
                    }`}>
                      <span className={`material-symbols-outlined text-[20px] ${
                        isPending
                          ? profile.profileType === 0 ? "text-secondary" : "text-primary"
                          : "text-primary"
                      }`}>
                        {isPending ? (profile as PendingProfile).icon : "account_circle"}
                      </span>
                    </div>
                    <span className={`px-2 py-0.5 text-label-2xs rounded-full font-semibold border ${('badgeClass' in profile && (profile as PendingProfile).badgeClass) || "bg-surface-container text-outline border-outline-variant/20"}`}>
                      {isPending ? (profile as PendingProfile).badge : getProfileTypeLabel(profile.profileType)}
                    </span>
                  </div>

                  {/* Name */}
                  <h3 className="text-body-lg font-bold text-on-surface mb-1.5 group-hover:text-primary transition-colors">
                    {profile.name}
                  </h3>

                  {/* Description */}
                  <p className="text-body-sm text-on-surface-variant mb-4 leading-relaxed line-clamp-2">
                    {isPending ? (profile as PendingProfile).bio : (profile.bio || "No description yet.")}
                  </p>

                  {/* Stats */}
                  <div className="flex items-center gap-3 text-on-surface-variant">
                    {isPending ? (
                      <>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">workspaces</span>
                          <span className="text-label-xs font-medium">{(profile as PendingProfile).brandCount} Brands</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">monitoring</span>
                          <span className="text-label-xs font-medium">{(profile as PendingProfile).campaignCount} Campaigns</span>
                        </div>
                      </>
                    ) : (
                      <>
                        <div className="flex items-center gap-1">
                          <span className="material-symbols-outlined text-[14px] text-outline">person</span>
                          <span className="text-label-xs font-medium">{(profile as Profile).isOwner ? "Owner" : (profile as Profile).memberRole || "Member"}</span>
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
                  onClick={() => handleSelect(profile)}
                  disabled={creating}
                >
                  {isActive ? "Continue" : isPending ? "Create & Select" : "Select"}
                </motion.button>

                {/* Subtle gradient decoration */}
                {isPending && (
                  <div className={`absolute -bottom-12 -right-12 w-32 h-32 rounded-full blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 ${
                    profile.profileType === 0 
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
          {hasProfiles ? (
            <p className="text-body-sm text-on-surface-variant">
              Need help managing permissions?{" "}
              <a className="text-primary font-semibold hover:underline underline-offset-2" href="#">Contact support</a>
            </p>
          ) : (
            <p className="text-body-sm text-on-surface-variant">
              Already have profiles?{" "}
              <button 
                className="text-primary font-semibold hover:underline underline-offset-2 bg-transparent border-none p-0 cursor-pointer" 
                onClick={() => router.push("/profiles")}
              >
                View all profiles
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
