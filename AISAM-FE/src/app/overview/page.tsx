"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { getToken, getUserIdFromToken } from "@/lib/auth";
import { useProfiles, addProfileToCache, getProfileTypeLabel } from "@/hooks/useProfiles";
import { apiFetch } from "@/lib/apiClient";

function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

export default function OverviewPage() {
  const router = useRouter();
  const { profiles, loading, activeProfile, selectProfile } = useProfiles();
  const [creating, setCreating] = useState(false);
  const [toast, setToast] = useState<{ name: string } | null>(null);

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

  const handleSelect = async (profile: any) => {
    if (profile.pendingCreate) {
      await createAndSelectProfile(profile.name, profile.profileType, profile.companyName);
    } else {
      selectProfile(profile);
    }
    setToast({ name: profile.name });
    setTimeout(() => {
      router.push("/dashboard");
    }, 2000);
  };

  if (loading || creating) {
    return (
      <main className="min-h-screen bg-surface flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="w-10 h-10 border-2 border-primary border-b-transparent rounded-full animate-spin" />
          <p className="text-body-sm text-outline">{creating ? "Creating your workspace..." : "Loading..."}</p>
        </div>
      </main>
    );
  }

  const hasProfiles = profiles.length > 0;
  const displayProfiles = hasProfiles ? profiles : [
    {
      id: "pending-personal",
      name: "Personal Workspace",
      profileType: 0,
      companyName: null,
      bio: "Manage individual creator accounts and small brand projects.",
      brandCount: 0,
      campaignCount: 0,
      pendingCreate: true,
      icon: "person",
      badge: "Free",
      badgeClass: "bg-success-green/10 text-success-green",
    },
    {
      id: "pending-business",
      name: "Nexus Agency Group",
      profileType: 1,
      companyName: "Nexus Agency",
      bio: "Collaborative workspace for agency teams and large-scale clients.",
      brandCount: 0,
      campaignCount: 0,
      pendingCreate: true,
      icon: "corporate_fare",
      badge: "Basic",
      badgeClass: "bg-primary/10 text-primary",
    },
  ];

  return (
    <main className="min-h-screen bg-surface flex flex-col">
      <style>{`
        @keyframes fadeIn {
          from { opacity: 0; transform: translateY(12px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-in { animation: fadeIn 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards; }
        .profile-card {
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        .profile-card:hover {
          transform: translateY(-4px);
          box-shadow: 0 16px 32px rgba(0, 0, 0, 0.08);
        }
        .profile-card:hover .select-btn {
          background-color: #004ccd;
          color: white;
          box-shadow: 0 4px 12px rgba(0, 76, 205, 0.25);
        }
      `}</style>

      <div className="flex-1 px-6 md:px-8 max-w-5xl mx-auto w-full flex flex-col justify-center min-h-screen">
        {/* Logo */}
        <div className="flex justify-center mb-10 animate-fade-in">
          <div className="flex items-center gap-3">
            <div className="w-11 h-11 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
              <span className="material-symbols-outlined text-on-primary text-xl" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
            </div>
            <div>
              <h1 className="text-headline-sm font-bold bg-gradient-to-r from-primary to-primary-container bg-clip-text text-transparent leading-none">AISAM</h1>
              <p className="text-label-sm text-outline leading-none mt-0.5 tracking-wide">AI Ad Manager</p>
            </div>
          </div>
        </div>

        {/* Page Header */}
        <div className="text-center mb-10 animate-fade-in" style={{ animationDelay: "0.1s" }}>
          <h2 className="text-headline-lg-mobile lg:text-headline-lg text-on-surface mb-3">
            {hasProfiles ? "Select Your Active Profile" : "Get Started with AISAM"}
          </h2>
          <p className="text-body-lg text-on-surface-variant max-w-xl mx-auto leading-relaxed">
            {hasProfiles
              ? "Your profile choice determines the specific brands, historical data, and active campaigns visible in your workspace."
              : "Choose a workspace type that fits your needs. You can always change or create additional profiles later."}
          </p>
        </div>

        {/* Profile Grid */}
        <div className={`grid ${hasProfiles ? "grid-cols-1 md:grid-cols-2 lg:grid-cols-3" : "grid-cols-1 md:grid-cols-2"} gap-6 max-w-4xl mx-auto w-full mb-12 animate-fade-in`} style={{ animationDelay: "0.2s" }}>
          {displayProfiles.map((profile: any, index: number) => {
            const isActive = !profile.pendingCreate && activeProfile?.id === profile.id;
            return (
              <div
                key={profile.id}
                className={`profile-card group relative bg-surface-container-lowest border ${isActive ? "border-primary/40 ring-1 ring-primary/20" : "border-outline-variant/40"} p-7 rounded-2xl flex flex-col justify-between overflow-hidden`}
              >
                <div className="relative z-10">
                  <div className="flex justify-between items-start mb-6">
                    <div className={`w-14 h-14 rounded-2xl flex items-center justify-center shadow-sm ${
                      profile.pendingCreate
                        ? profile.profileType === 0
                          ? "bg-gradient-to-br from-secondary-fixed-dim to-secondary-fixed"
                          : "bg-gradient-to-br from-primary-container to-primary"
                        : "bg-gradient-to-br from-primary/10 to-primary/5"
                    }`}>
                      <span className={`material-symbols-outlined text-2xl ${
                        profile.pendingCreate
                          ? profile.profileType === 0 ? "text-on-secondary-fixed" : "text-on-primary-container"
                          : "text-primary"
                      }`}>
                        {profile.pendingCreate ? profile.icon : "account_circle"}
                      </span>
                    </div>
                    <span className={`px-3 py-1 text-label-sm rounded-full uppercase tracking-wider font-semibold ${profile.badgeClass || "bg-surface-container text-outline"}`}>
                      {profile.pendingCreate ? profile.badge : getProfileTypeLabel(profile.profileType)}
                    </span>
                  </div>

                  <h3 className="text-headline-sm mb-2 group-hover:text-primary transition-colors">
                    {profile.name}
                  </h3>

                  <p className="text-body-sm text-on-surface-variant mb-5 leading-relaxed line-clamp-2">
                    {profile.pendingCreate ? profile.bio : (profile.bio || "No description yet.")}
                  </p>

                  <div className="flex items-center gap-5 text-on-surface-variant">
                    {profile.pendingCreate ? (
                      <>
                        <div className="flex items-center gap-1.5">
                          <span className="material-symbols-outlined text-[18px]">workspaces</span>
                          <span className="text-label-md font-medium">{profile.brandCount} Brands</span>
                        </div>
                        <div className="flex items-center gap-1.5">
                          <span className="material-symbols-outlined text-[18px]">monitoring</span>
                          <span className="text-label-md font-medium">{profile.campaignCount} Campaigns</span>
                        </div>
                      </>
                    ) : (
                      <>
                        <div className="flex items-center gap-1.5">
                          <span className="material-symbols-outlined text-[18px]">person</span>
                          <span className="text-label-md font-medium">{profile.isOwner ? "Owner" : profile.memberRole || "Member"}</span>
                        </div>
                        <div className="flex items-center gap-1.5">
                          <span className="material-symbols-outlined text-[18px]">calendar_today</span>
                          <span className="text-label-md font-medium">{new Date(profile.createdAt).toLocaleDateString()}</span>
                        </div>
                      </>
                    )}
                  </div>
                </div>

                <button
                  className="select-btn mt-8 w-full py-3 rounded-xl border border-primary/30 text-primary font-semibold text-label-md transition-all active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
                  onClick={() => handleSelect(profile)}
                  disabled={creating}
                >
                  {isActive ? "Continue" : "Select Profile"}
                </button>

                {profile.pendingCreate && profile.profileType === 1 && (
                  <div className="absolute -bottom-12 -right-12 w-48 h-48 bg-gradient-to-bl from-primary/[0.06] to-transparent rounded-full blur-2xl group-hover:from-primary/[0.12] transition-colors" />
                )}
                {profile.pendingCreate && profile.profileType === 0 && (
                  <div className="absolute top-0 right-0 w-40 h-40 bg-gradient-to-bl from-secondary-fixed-dim/10 to-transparent rounded-full -mr-20 -mt-20 transition-transform group-hover:scale-150" />
                )}
              </div>
            );
          })}
        </div>

        {/* Secondary Actions */}
        <div className="flex flex-col items-center gap-3 animate-fade-in" style={{ animationDelay: "0.35s" }}>
          {hasProfiles ? (
            <p className="text-body-sm text-on-surface-variant">
              Need help managing your permissions?{" "}
              <a className="text-primary font-semibold hover:underline" href="#">Contact Support</a>
            </p>
          ) : (
            <p className="text-body-sm text-on-surface-variant">
              Already have a profile?{" "}
              <button className="text-primary font-semibold hover:underline bg-transparent border-none p-0 cursor-pointer" onClick={() => router.push("/profiles")}>
                View all profiles
              </button>
            </p>
          )}
        </div>
      </div>

      {/* Success Toast */}
      <div
        className={`fixed bottom-8 left-1/2 -translate-x-1/2 bg-enterprise-navy text-white px-8 py-4 rounded-full shadow-2xl flex items-center gap-4 z-50 transition-all duration-500 ${
          toast ? "translate-y-0 opacity-100" : "translate-y-24 opacity-0"
        }`}
      >
        <div className="w-7 h-7 bg-success-green rounded-full flex items-center justify-center shrink-0">
          <span className="material-symbols-outlined text-[18px]">check</span>
        </div>
        <p className="text-label-md">
          Profile <span className="font-bold">{toast?.name}</span> selected successfully. Loading workspace...
        </p>
      </div>
    </main>
  );
}
