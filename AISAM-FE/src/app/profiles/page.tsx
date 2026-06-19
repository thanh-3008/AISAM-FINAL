"use client";

import { useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { motion, useReducedMotion } from "motion/react";
import { useWorkspaces, getWorkspaceTypeLabel, WorkspaceData } from "@/hooks/useWorkspaces";
import CreateProfileModal from "@/components/profiles/CreateProfileModal";

function getInitials(name: string) {
  return name.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

const statusConfig: Record<number, { label: string; class: string; dot: string }> = {
  0: { label: "Pending", class: "bg-amber-50 text-amber-700 border-amber-200/50", dot: "bg-amber-500" },
  1: { label: "Active", class: "bg-emerald-50 text-emerald-700 border-emerald-200/50", dot: "bg-emerald-500" },
  2: { label: "Suspended", class: "bg-red-50 text-red-700 border-red-200/50", dot: "bg-red-500" },
  3: { label: "Cancelled", class: "bg-surface-container-high text-on-surface-variant border-outline-variant/20", dot: "bg-outline" },
};

const container = {
  hidden: { opacity: 0 },
  show: { opacity: 1, transition: { staggerChildren: 0.05 } },
};

const item = {
  hidden: { opacity: 0, y: 12 },
  show: { opacity: 1, y: 0, transition: { duration: 0.4, ease: [0.16, 1, 0.3, 1] as const } },
};

export default function ProfilesListPage() {
  const router = useRouter();
  const { workspaces, loading, error, activeWorkspace, selectWorkspace, refetch } = useWorkspaces();
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState<number | "all">("all");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const reduceMotion = useReducedMotion();

  const filtered = useMemo(() => {
    let list = workspaces;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(w =>
        w.name.toLowerCase().includes(q) ||
        (w.companyName && w.companyName.toLowerCase().includes(q)) ||
        getWorkspaceTypeLabel(w.workspaceType).toLowerCase().includes(q)
      );
    }
    if (filterStatus !== "all") {
      list = list.filter(w => w.status === filterStatus);
    }
    return list;
  }, [search, filterStatus, workspaces]);

  const stats = useMemo(() => ({
    total: workspaces.length,
    active: workspaces.filter(w => w.status === 1).length,
    pending: workspaces.filter(w => w.status === 0).length,
    personal: workspaces.filter(w => w.workspaceType === 1).length,
    business: workspaces.filter(w => w.workspaceType === 2).length,
  }), [workspaces]);

  const handleSelect = (workspace: WorkspaceData) => {
    selectWorkspace(workspace);
    router.push("/dashboard");
  };

  return (
    <div className="min-h-[100dvh] bg-surface flex">
      <div className="flex-1 flex flex-col">
        <main className="flex-1 overflow-auto">
          <div className="max-w-6xl mx-auto p-6 md:p-8">
            {/* Header */}
            <motion.div
              initial={reduceMotion ? undefined : { opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.4 }}
              className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8"
            >
              <div>
                <h1 className="text-3xl font-bold text-on-surface tracking-tight">Workspaces</h1>
                <p className="text-body-sm text-on-surface-variant mt-1.5">
                  Manage your workspaces across AISAM
                </p>
              </div>
              <motion.button
                whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                onClick={() => setShowCreateModal(true)}
                className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 shrink-0"
              >
                <span className="material-symbols-outlined text-[18px]">add</span>
                Create Workspace
              </motion.button>
            </motion.div>

            {/* Stats row */}
            {!loading && !error && workspaces.length > 0 && (
              <motion.div
                variants={reduceMotion ? undefined : container}
                initial={reduceMotion ? undefined : "hidden"}
                animate="show"
                className="grid grid-cols-2 sm:grid-cols-5 gap-3 mb-6"
              >
                {[
                  { label: "Total", value: stats.total, color: "text-primary", bg: "bg-primary/5", bar: "bg-primary" },
                  { label: "Active", value: stats.active, color: "text-emerald-600", bg: "bg-emerald-50", bar: "bg-emerald-500" },
                  { label: "Pending", value: stats.pending, color: "text-amber-600", bg: "bg-amber-50", bar: "bg-amber-500" },
                  { label: "Personal", value: stats.personal, color: "text-secondary", bg: "bg-secondary/5", bar: "bg-secondary" },
                  { label: "Business", value: stats.business, color: "text-blue-600", bg: "bg-blue-50", bar: "bg-blue-500" },
                ].map((s) => (
                  <motion.div
                    key={s.label}
                    variants={reduceMotion ? undefined : item}
                    className={`${s.bg} rounded-xl px-4 py-3 overflow-hidden relative border border-outline-variant/10`}
                  >
                    <div className="flex items-center justify-between relative z-10">
                      <span className="text-label-sm text-on-surface-variant">{s.label}</span>
                      <span className={`text-body-lg font-bold ${s.color} tabular-nums`}>{s.value}</span>
                    </div>
                    {stats.total > 0 && (
                      <div className="mt-2 h-1 bg-surface-container-low rounded-full overflow-hidden relative z-10">
                        <motion.div
                          initial={reduceMotion ? undefined : { width: 0 }}
                          animate={{ width: `${(s.value / stats.total) * 100}%` }}
                          transition={{ duration: 0.8, delay: 0.2, ease: "easeOut" }}
                          className={`h-full rounded-full ${s.bar}`}
                        />
                      </div>
                    )}
                  </motion.div>
                ))}
              </motion.div>
            )}

            {/* Search + Filters */}
            <motion.div
              initial={reduceMotion ? undefined : { opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.4, delay: 0.1 }}
              className="flex flex-col sm:flex-row gap-3 mb-6"
            >
              <div className="relative flex-1">
                <span className="absolute inset-y-0 left-0 pl-4 flex items-center text-outline pointer-events-none">
                  <span className="material-symbols-outlined text-[18px]">search</span>
                </span>
                <input
                  className="w-full bg-surface-container-lowest rounded-xl border border-outline-variant/30 pl-10 pr-10 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                  placeholder="Search by name, company, or plan..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                />
                {search && (
                  <button
                    onClick={() => setSearch("")}
                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline hover:text-on-surface transition-colors"
                  >
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                )}
              </div>
              <div className="flex gap-1 p-1 bg-surface-container/60 rounded-xl self-start border border-outline-variant/10">
                {[
                  { key: "all" as const, label: "All" },
                  { key: 1 as const, label: "Active" },
                  { key: 0 as const, label: "Pending" },
                ].map(f => (
                  <motion.button
                    key={String(f.key)}
                    whileTap={reduceMotion ? undefined : { scale: 0.95 }}
                    onClick={() => setFilterStatus(f.key)}
                    className={`px-4 py-1.5 rounded-lg text-label-sm font-medium transition-all ${
                      filterStatus === f.key
                        ? "bg-surface-container-lowest text-on-surface shadow-sm"
                        : "text-on-surface-variant hover:text-on-surface"
                    }`}
                  >
                    {f.label}
                  </motion.button>
                ))}
              </div>
            </motion.div>

            {/* Content */}
            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
                {[1, 2, 3, 4, 5, 6].map(i => (
                  <div key={i} className="bg-surface-container-lowest border border-outline-variant/15 rounded-2xl p-5 animate-pulse space-y-4">
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
              <motion.div
                initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="text-center py-20"
              >
                <div className="w-16 h-16 mx-auto mb-5 rounded-2xl bg-red-50 flex items-center justify-center">
                  <span className="material-symbols-outlined text-red-500 text-3xl">error_outline</span>
                </div>
                <p className="text-body-md text-red-600 font-semibold mb-1">Failed to load workspaces</p>
                <p className="text-body-sm text-outline mb-5">{error}</p>
                <motion.button
                  whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                  onClick={refetch}
                  className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                >
                  Retry
                </motion.button>
              </motion.div>
            ) : filtered.length === 0 ? (
              <motion.div
                initial={reduceMotion ? undefined : { opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
                className="text-center py-24"
              >
                <div className="w-20 h-20 mx-auto mb-6 rounded-2xl bg-surface-container flex items-center justify-center">
                  {search || filterStatus !== "all" ? (
                    <span className="material-symbols-outlined text-outline/40 text-4xl">search_off</span>
                  ) : (
                    <span className="material-symbols-outlined text-outline/40 text-4xl">group_off</span>
                  )}
                </div>
                <h3 className="text-body-lg text-on-surface font-semibold mb-2">
                  {search || filterStatus !== "all" ? "No matching workspaces" : "No workspaces yet"}
                </h3>
                <p className="text-body-sm text-outline mb-6 max-w-md mx-auto">
                  {search || filterStatus !== "all"
                    ? "Try different search terms or filters."
                    : "Create your first workspace to start managing your content across AISAM."
                  }
                </p>
                {!search && filterStatus === "all" && (
                  <motion.button
                    whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                    onClick={() => setShowCreateModal(true)}
                    className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                  >
                    <span className="material-symbols-outlined text-[18px]">add</span>
                    Create Workspace
                  </motion.button>
                )}
              </motion.div>
            ) : (
              <>
                <div className="flex items-center justify-between mb-4">
                  <p className="text-label-sm text-outline">{filtered.length} workspace{filtered.length !== 1 ? "s" : ""}</p>
                </div>
                <motion.div
                  variants={reduceMotion ? undefined : container}
                  initial={reduceMotion ? undefined : "hidden"}
                  animate="show"
                  key={`${filterStatus}-${search}`}
                  className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4"
                >
                  {filtered.map((w) => {
                    const isActive = activeWorkspace?.id === w.id;
                    const statusInfo = statusConfig[w.status] || statusConfig[0];
                    return (
                      <motion.div
                        key={w.id}
                        variants={reduceMotion ? undefined : item}
                        whileHover={reduceMotion ? undefined : { y: -2 }}
                        className={`group bg-surface-container-lowest border rounded-2xl p-5 flex flex-col transition-shadow ${
                          isActive
                            ? "border-primary/30 shadow-md shadow-primary/5"
                            : "border-outline-variant/15 hover:border-outline-variant/30 hover:shadow-md hover:shadow-black/5"
                        }`}
                      >
                        <div className="flex items-start gap-3.5 mb-4">
                          <div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 text-body-md font-bold transition-all ${
                            isActive
                              ? "bg-primary text-on-primary shadow-sm shadow-primary/20"
                              : "bg-gradient-to-br from-primary/10 to-primary/5 text-primary"
                          }`}>
                            {getInitials(w.name)}
                          </div>
                          <div className="flex-1 min-w-0 pt-0.5">
                            <div className="flex items-center gap-2">
                              <h3 className="text-body-md font-semibold text-on-surface truncate">{w.name}</h3>
                              {isActive && (
                                <span className="shrink-0 w-2 h-2 rounded-full bg-primary animate-pulse" title="Active workspace" />
                              )}
                            </div>
                            <p className="text-label-sm text-on-surface-variant font-medium">{getWorkspaceTypeLabel(w.workspaceType)}</p>
                          </div>
                          <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium shrink-0 border ${statusInfo.class}`}>
                            <span className={`w-1.5 h-1.5 rounded-full ${statusInfo.dot} ${w.status === 1 ? "animate-pulse" : ""}`} />
                            {statusInfo.label}
                          </span>
                        </div>

                        <div className="space-y-1.5 mb-4 flex-1">
                          {w.companyName && (
                            <p className="text-label-sm text-outline flex items-center gap-1.5">
                              <span className="material-symbols-outlined text-[14px]">business</span>
                              {w.companyName}
                            </p>
                          )}
                          {w.bio && (
                            <p className="text-label-sm text-outline line-clamp-2 leading-relaxed">{w.bio}</p>
                          )}
                          <p className="text-label-sm text-outline flex items-center gap-1.5">
                            <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                            Created {new Date(w.createdAt).toLocaleDateString()}
                          </p>
                        </div>

                        <div className="flex gap-2 pt-3 border-t border-outline-variant/10">
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={() => handleSelect(w)}
                            className={`flex-1 px-3.5 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                              isActive
                                ? "bg-primary text-on-primary shadow-sm shadow-primary/20 hover:bg-primary/90"
                                : "bg-primary/10 text-primary hover:bg-primary/15"
                            }`}
                          >
                            {isActive ? "Dashboard" : "Select"}
                          </motion.button>
                          <Link
                            href={`/profiles/${w.id}`}
                            className="px-3.5 py-2 rounded-xl text-label-sm font-medium border border-outline-variant/30 text-on-surface hover:bg-surface-container hover:border-outline-variant/50 transition-colors inline-flex items-center gap-1"
                          >
                            <span className="material-symbols-outlined text-[16px]">settings</span>
                          </Link>
                          {w.isOwner && (
                            <div className="px-2 py-2 rounded-xl text-amber-600 bg-amber-50 flex items-center border border-amber-200/30" title="Owner">
                              <span className="material-symbols-outlined text-[16px]">star</span>
                            </div>
                          )}
                        </div>
                      </motion.div>
                    );
                  })}
                </motion.div>
              </>
            )}
          </div>
        </main>
      </div>
      <CreateProfileModal open={showCreateModal} onClose={() => setShowCreateModal(false)} />
    </div>
  );
}
