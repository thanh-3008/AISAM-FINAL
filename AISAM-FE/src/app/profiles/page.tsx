"use client";

import { useState, useEffect, useMemo } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useProfiles, getProfileTypeLabel } from "@/hooks/useProfiles";
import { Profile } from "@/hooks/useProfiles";
import CreateProfileModal from "@/components/profiles/CreateProfileModal";

function getInitials(name: string) {
  return name.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

const statusConfig: Record<number, { label: string; class: string }> = {
  0: { label: "Pending", class: "bg-amber-50 text-amber-600" },
  1: { label: "Active", class: "bg-success-green/10 text-success-green" },
  2: { label: "Suspended", class: "bg-danger-red/10 text-danger-red" },
  3: { label: "Cancelled", class: "bg-outline-variant/30 text-on-surface-variant" },
};

export default function ProfilesListPage() {
  const router = useRouter();
  const { profiles, loading, error, activeProfile, selectProfile, refetch } = useProfiles();
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState<number | "all">("all");
  const [showCreateModal, setShowCreateModal] = useState(false);

  const filtered = useMemo(() => {
    let list = profiles;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(p =>
        p.name.toLowerCase().includes(q) ||
        (p.companyName && p.companyName.toLowerCase().includes(q)) ||
        getProfileTypeLabel(p.profileType).toLowerCase().includes(q)
      );
    }
    if (filterStatus !== "all") {
      list = list.filter(p => p.status === filterStatus);
    }
    return list;
  }, [search, filterStatus, profiles]);

  const stats = useMemo(() => ({
    total: profiles.length,
    active: profiles.filter(p => p.status === 1).length,
    free: profiles.filter(p => p.profileType === 0).length,
    basic: profiles.filter(p => p.profileType === 1).length,
    pro: profiles.filter(p => p.profileType === 2).length,
  }), [profiles]);

  const handleSelect = (profile: Profile) => {
    selectProfile(profile);
    router.push("/dashboard");
  };

  return (
    <div className="min-h-screen bg-surface flex">
      <div className="flex-1 flex flex-col">
        <main className="flex-1 overflow-auto">
          <div className="max-w-7xl mx-auto p-8 animate-in fade-in duration-500">

            {/* Header */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
              <div>
                <h1 className="text-headline-lg font-bold text-on-surface">Profiles</h1>
                <p className="text-body-sm text-on-surface-variant mt-1">
                  Manage your business profiles across AISAM
                </p>
              </div>
              <button
                onClick={() => setShowCreateModal(true)}
                className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:opacity-90 transition-all shadow-sm shrink-0"
              >
                <span className="material-symbols-outlined text-[18px]">add</span>
                Create Profile
              </button>
            </div>

            {/* Stats row */}
            {!loading && !error && profiles.length > 0 && (
              <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-6">
                {[
                  { label: "Total", value: stats.total, color: "text-primary", bg: "bg-primary/5", bar: "bg-primary", max: stats.total },
                  { label: "Active", value: stats.active, color: "text-success-green", bg: "bg-success-green/5", bar: "bg-success-green", max: stats.total },
                  { label: "Free", value: stats.free, color: "text-outline", bg: "bg-surface-container/50", bar: "bg-outline", max: stats.total },
                  { label: "Basic", value: stats.basic, color: "text-secondary", bg: "bg-secondary/5", bar: "bg-secondary", max: stats.total },
                  { label: "Pro", value: stats.pro, color: "text-amber-600", bg: "bg-amber-50", bar: "bg-amber-500", max: stats.total },
                ].map((s, i) => (
                  <div key={s.label} className={`${s.bg} rounded-xl px-4 py-3 overflow-hidden relative animate-in fade-in slide-in-from-bottom-2 duration-400`} style={{ animationDelay: `${i * 60}ms`, animationFillMode: "both" }}>
                    <div className="flex items-center justify-between relative z-10">
                      <span className="text-label-sm text-on-surface-variant">{s.label}</span>
                      <span className={`text-headline-sm font-bold ${s.color} tabular-nums`}>{s.value}</span>
                    </div>
                    {s.max > 0 && (
                      <div className="mt-2 h-1 bg-surface-container-low rounded-full overflow-hidden relative z-10">
                        <div className={`h-full rounded-full transition-all duration-700 ease-out ${s.bar}`} style={{ width: `${(s.value / s.max) * 100}%` }} />
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* Search + Filters */}
            <div className="flex flex-col sm:flex-row gap-3 mb-6">
              <div className="relative flex-1">
                <span className="absolute inset-y-0 left-0 pl-4 flex items-center text-outline pointer-events-none">
                  <span className="material-symbols-outlined text-[18px]">search</span>
                </span>
                <input
                  className="w-full bg-surface-container-lowest rounded-xl border border-outline-variant/40 pl-10 pr-10 py-2.5 text-body-md text-on-surface placeholder:text-outline/50 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all"
                  placeholder="Search by name, company, or plan..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                />
                {search && (
                  <button
                    onClick={() => setSearch("")}
                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline hover:text-on-surface"
                  >
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                )}
              </div>
              <div className="flex gap-1.5 p-1 bg-surface-container/80 rounded-xl self-start">
                {[
                  { key: "all" as const, label: "All" },
                  { key: 1 as const, label: "Active" },
                  { key: 0 as const, label: "Pending" },
                ].map(f => (
                  <button
                    key={String(f.key)}
                    onClick={() => setFilterStatus(f.key)}
                    className={`px-3.5 py-1.5 rounded-xl text-label-sm font-medium transition-all active:scale-[0.97] ${
                      filterStatus === f.key
                        ? "bg-surface-container-lowest text-on-surface shadow-sm"
                        : "text-on-surface-variant hover:text-on-surface"
                    }`}
                  >
                    {f.label}
                  </button>
                ))}
              </div>
            </div>

            {/* Content */}
            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                {[1, 2, 3, 4, 5, 6].map(i => (
                  <div key={i} className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl p-5 animate-pulse space-y-4">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 rounded-xl bg-surface-container" />
                      <div className="flex-1 space-y-2">
                        <div className="h-4 bg-surface-container rounded w-3/4" />
                        <div className="h-3 bg-surface-container rounded w-1/2" />
                      </div>
                    </div>
                    <div className="h-3 bg-surface-container rounded w-full" />
                    <div className="h-3 bg-surface-container rounded w-2/3" />
                    <div className="flex gap-2">
                      <div className="h-9 bg-surface-container rounded-xl flex-1" />
                      <div className="h-9 bg-surface-container rounded-xl w-20" />
                    </div>
                  </div>
                ))}
              </div>
            ) : error ? (
              <div className="text-center py-20">
                <div className="w-16 h-16 mx-auto mb-5 rounded-2xl bg-error-container/30 flex items-center justify-center">
                  <span className="material-symbols-outlined text-danger-red text-3xl">error_outline</span>
                </div>
                <p className="text-body-md text-danger-red font-semibold mb-1">Failed to load profiles</p>
                <p className="text-body-sm text-outline mb-5">{error}</p>
                <button onClick={refetch} className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm">
                  Retry
                </button>
              </div>
            ) : filtered.length === 0 ? (
              <div className="text-center py-24">
                <div className="w-20 h-20 mx-auto mb-6 rounded-2xl bg-surface-container flex items-center justify-center ring-1 ring-outline-variant/20">
                  {search || filterStatus !== "all" ? (
                    <span className="material-symbols-outlined text-outline/50 text-4xl">search_off</span>
                  ) : (
                    <span className="material-symbols-outlined text-outline/50 text-4xl">group_off</span>
                  )}
                </div>
                <h3 className="text-headline-sm text-on-surface font-semibold mb-2">
                  {search || filterStatus !== "all" ? "No matching profiles" : "No profiles yet"}
                </h3>
                <p className="text-body-sm text-outline mb-6 max-w-md mx-auto">
                  {search || filterStatus !== "all"
                    ? "Try different search terms or filters."
                    : "Create your first business profile to start managing your content across AISAM."
                  }
                </p>
                {!search && filterStatus === "all" && (
                  <button onClick={() => setShowCreateModal(true)} className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:opacity-90 transition-all shadow-sm">
                    <span className="material-symbols-outlined text-[18px]">add</span>
                    Create Profile
                  </button>
                )}
              </div>
            ) : (
              <>
                <div className="flex items-center justify-between mb-4">
                  <p className="text-label-sm text-outline">{filtered.length} profile{filtered.length !== 1 ? "s" : ""}</p>
                </div>
                <div key={`${filterStatus}-${search}`} className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 animate-in fade-in duration-300">
                  {filtered.map((p, idx) => {
                    const isActive = activeProfile?.id === p.id;
                    const statusInfo = statusConfig[p.status] || statusConfig[0];
                    return (
                      <div
                        key={p.id}
                        className={`group bg-surface-container-lowest border rounded-2xl p-5 shadow-sm hover:shadow-md transition-all duration-300 flex flex-col animate-in fade-in slide-in-from-bottom-2 duration-400 ${
                          isActive
                            ? "border-primary/30 ring-1 ring-primary/15"
                            : "border-outline-variant/20 hover:border-outline-variant/60 hover:-translate-y-0.5"
                        }`}
                        style={{ animationDelay: `${idx * 60}ms`, animationFillMode: "both" }}
                      >
                        <div className="flex items-start gap-3.5 mb-3.5">
<div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 text-body-md font-bold transition-all duration-300 ${
                            isActive
                              ? "bg-primary text-on-primary shadow-sm shadow-primary/20"
                              : "bg-gradient-to-br from-primary/10 to-primary/5 text-primary group-hover:scale-105"
                          }`}> 
                            {getInitials(p.name)}
                          </div>
                          <div className="flex-1 min-w-0 pt-0.5">
                            <div className="flex items-center gap-2">
                              <h3 className="text-body-md font-semibold text-on-surface truncate">{p.name}</h3>
                              {isActive && (
                                <span className="shrink-0 w-2 h-2 rounded-full bg-primary animate-pulse" title="Active profile" />
                              )}
                            </div>
                            <p className="text-label-sm text-on-surface-variant font-medium">{getProfileTypeLabel(p.profileType)}</p>
                          </div>
                          <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-sm font-medium shrink-0 ${statusInfo.class}`}>
                            <span className={`w-1.5 h-1.5 rounded-full ${p.status === 1 ? "bg-success-green animate-pulse" : "bg-current"}`} />
                            {statusInfo.label}
                          </span>
                        </div>

                        <div className="space-y-1.5 mb-4 flex-1">
                          {p.companyName && (
                            <p className="text-label-sm text-outline flex items-center gap-1.5">
                              <span className="material-symbols-outlined text-[14px]">business</span>
                              {p.companyName}
                            </p>
                          )}
                          {p.bio && (
                            <p className="text-label-sm text-outline line-clamp-2 leading-relaxed">{p.bio}</p>
                          )}
                          <p className="text-label-sm text-outline flex items-center gap-1.5">
                            <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                            Created {new Date(p.createdAt).toLocaleDateString()}
                          </p>
                        </div>

                        <div className="flex gap-2 pt-1 border-t border-outline-variant/10">
                          <button
                            onClick={() => handleSelect(p)}
                            className={`flex-1 px-3.5 py-2 rounded-xl text-label-sm font-semibold transition-all duration-200 active:scale-[0.97] ${
                              isActive
                                ? "bg-primary text-on-primary shadow-sm hover:opacity-90"
                                : "bg-primary/10 text-primary hover:bg-primary/15"
                            }`}
                          >
                            {isActive ? "Dashboard" : "Select"}
                          </button>
                          <Link
                            href={`/profiles/${p.id}`}
                            className="px-3.5 py-2 rounded-xl text-label-sm font-medium border border-outline-variant/50 text-on-surface hover:bg-surface-container hover:border-outline-variant transition-colors duration-200 inline-flex items-center gap-1"
                          >
                            <span className="material-symbols-outlined text-[16px]">settings</span>
                          </Link>
                          {p.isOwner && (
                            <div className="px-2 py-2 rounded-xl text-label-sm text-primary/60 bg-primary/5 flex items-center" title="Owner">
                              <span className="material-symbols-outlined text-[16px]">star</span>
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        </main>
      </div>
      <CreateProfileModal open={showCreateModal} onClose={() => setShowCreateModal(false)} />
    </div>
  );
}
