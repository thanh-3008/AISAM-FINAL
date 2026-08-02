"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import { motion, AnimatePresence, useReducedMotion } from "motion/react";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";
import { apiFetch } from "@/lib/apiClient";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useProfiles } from "@/hooks/useProfiles";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { getStoredActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { clearActiveProfile } from "@/stores/profile-store";
import CreateBrandModal from "@/components/brands/CreateBrandModal";
import EditBrandModal from "@/components/brands/EditBrandModal";
import { useToast } from "@/contexts/ToastContext";

interface Brand {
  id: string;
  userId: string;
  name: string;
  description: string | null;
  logoUrl: string | null;
  slogan: string | null;
  usp: string | null;
  targetAudience: string | null;
  profileId: string | null;
  isDeleted?: boolean;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  contentsCount: number;
}

const BRAND_COLORS = [
  { gradient: "from-primary/85 to-primary/35", light: "bg-primary/10" },
  { gradient: "from-primary/60 to-primary/25", light: "bg-primary/7" },
  { gradient: "from-primary/75 to-primary/30", light: "bg-primary/8" },
  { gradient: "from-primary/50 to-primary/20", light: "bg-primary/6" },
  { gradient: "from-primary/80 to-primary/40", light: "bg-primary/9" },
  { gradient: "from-primary/55 to-primary/25", light: "bg-primary/7" },
];



function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

const easeOut = [0.16, 1, 0.3, 1] as const;
const easeIn = [0.4, 0, 1, 1] as const;

const cardVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number) => ({
    opacity: 1, y: 0,
    transition: { duration: 0.5, delay: i * 0.06, ease: easeOut },
  }),
  exit: { opacity: 0, y: -10, scale: 0.96, transition: { duration: 0.25, ease: easeIn } },
};

function pickColor(id: string) {
  const idx = id.split("").reduce((a, c) => a + c.charCodeAt(0), 0) % BRAND_COLORS.length;
  return BRAND_COLORS[idx];
}

export default function BrandsPage() {
  const router = useRouter();
  const prefersReducedMotion = useReducedMotion();
  const { activeWorkspace } = useWorkspaces();
  const { activeProfile } = useProfiles();
  const { addToast } = useToast();
  const featureGate = useFeatureGate();
  const canEdit = featureGate.isOwner || featureGate.isManager;
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingBrand, setEditingBrand] = useState<Brand | null>(null);
  const [deletingBrand, setDeletingBrand] = useState<Brand | null>(null);
  const [sortBy, setSortBy] = useState<string>("createdat");
  const [sortDesc, setSortDesc] = useState(true);
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [showSortMenu, setShowSortMenu] = useState(false);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const PAGE_SIZE = 12;

  // If workspace ID is not a valid GUID → clear + redirect overview
  useEffect(() => {
    const ws = getStoredActiveWorkspace();
    if (ws && !/^[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}$/i.test(ws.id)) {
      clearActiveWorkspace();
      clearActiveProfile();
      router.push("/overview");
    }
  }, [router]);

  const fadeUp = prefersReducedMotion ? { initial: {}, animate: {} } : {
    initial: { opacity: 0, y: 20 },
    animate: { opacity: 1, y: 0 },
  };

  const fetchBrands = useCallback(async () => {
    if (!activeWorkspace) { setLoading(false); return; }
    try {
      const params = new URLSearchParams();
      params.set("page", String(page));
      params.set("pageSize", String(PAGE_SIZE));
      params.set("sortBy", sortBy);
      params.set("sortDescending", String(sortDesc));
      if (includeDeleted) params.set("includeDeleted", "true");
      const result = await apiFetch(`/brands?${params.toString()}`);
      if (result?.success && result.data) {
        setBrands((result.data.data as Brand[]) ?? []);
        setTotalCount(result.data.totalCount ?? 0);
        setTotalPages(Math.max(1, Math.ceil((result.data.totalCount ?? 0) / PAGE_SIZE)));
      }
    } catch { /* ignore */ }
    finally { setLoading(false); }
  }, [activeWorkspace, sortBy, sortDesc, includeDeleted, page]);

  useEffect(() => { fetchBrands(); }, [fetchBrands]);

  useEffect(() => { setPage(1); }, [sortBy, sortDesc, includeDeleted]);

  const filtered = useMemo(() => {
    let list = brands;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((b) => b.name.toLowerCase().includes(q) || (b.description && b.description.toLowerCase().includes(q)));
    }
    return list;
  }, [brands, search]);

  const stats = useMemo(() => ({
    total: brands.length, active: brands.filter((b) => b.productsCount > 0).length, draft: brands.filter((b) => b.productsCount === 0).length,
  }), [brands]);

  const hasFilters = !!search;
  const clearFilters = () => setSearch("");

  const handleEditSuccess = (updated: any) => {
    setBrands((prev) => prev.map((b) => (b.id === updated.id ? updated : b)));
    setEditingBrand(null);
    addToast("Brand updated successfully", "check");
  };

  const handleDeleteBrand = async () => {
    if (!deletingBrand) return;
    const brandToDelete = deletingBrand;
    setDeletingBrand(null);
    try {
      const result = await apiFetch(`/brands/${brandToDelete.id}`, { method: "DELETE" });
      if (result?.success) {
        setBrands((prev) => prev.filter((b) => b.id !== brandToDelete.id));
        addToast("Brand deleted successfully", "check");
      } else {
        addToast(result?.message || result?.error?.errorMessage || "Failed to delete brand", "error");
      }
    } catch (err: any) {
      addToast(err?.message || "Failed to delete brand", "error");
    }
  };

  const handleRestoreBrand = async (brandId: string) => {
    try {
      const result = await apiFetch(`/brands/${brandId}/restore`, { method: "POST" });
      if (result?.success) {
        fetchBrands();
        addToast("Brand restored successfully", "check");
      } else {
        addToast(result?.message || "Failed to restore brand", "error");
      }
    } catch (err: any) {
      addToast(err?.message || "Failed to restore brand", "error");
    }
  };

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-10">

        {/* ─── Hero ─── */}
        <motion.div {...fadeUp} transition={{ duration: 0.6, ease: easeOut }} className="flex items-end justify-between">
          <div className="flex items-center gap-4">
            <div className="relative w-10 h-10 shrink-0">
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-lg shadow-primary/20" />
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
              <div className="relative w-full h-full flex items-center justify-center">
                <span className="material-symbols-outlined text-on-primary text-[20px]">style</span>
              </div>
            </div>
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">Brands</h1>
              <p className="text-body-sm text-on-surface-variant mt-1 max-w-lg">
                Manage your brand portfolios and products.
              </p>
            </div>
          </div>
          {canEdit && (
            <button onClick={() => {
              if (!activeWorkspace) {
                setError("Please select a Workspace first (go to Overview).");
                return;
              }
              setShowCreateModal(true);
            }}
              className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-label-sm shadow-lg shadow-primary/20 hover:scale-105 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 transition-all shrink-0">
              <span className="material-symbols-outlined text-[18px]">add</span>
              New Brand
            </button>
          )}
        </motion.div>

        {/* ─── Error ─── */}
        {error && (
          <motion.div initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}
            className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-5 py-4 text-body-sm text-on-error-container">
            <span className="material-symbols-outlined text-error text-[20px]">error</span>
            <span className="flex-1">{error}</span>
            <button onClick={() => setError(null)} className="text-on-error-container/50 hover:text-on-error-container">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </motion.div>
        )}

        {/* ─── Stats ─── */}
        {!loading && brands.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
            {[
              { label: "Total Brands", value: stats.total, icon: "style", iconBg: "from-blue-500/20 to-blue-600/10", iconColor: "text-blue-500", gradient: "from-blue-500/5 to-transparent", accent: "#3b82f6", bar: "from-blue-400 to-blue-500" },
              { label: "With Products", value: stats.active, icon: "check_circle", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", gradient: "from-emerald-500/5 to-transparent", accent: "#10b981", bar: "from-emerald-400 to-emerald-500" },
              { label: "No Products", value: stats.draft, icon: "edit_note", iconBg: "from-amber-500/20 to-amber-600/10", iconColor: "text-amber-500", gradient: "from-amber-500/5 to-transparent", accent: "#f59e0b", bar: "from-amber-400 to-amber-500" },
            ].map((s, i) => (
              <motion.div key={s.label}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: i * 0.1, ease: easeOut }}
                className="relative bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden group card-hover"
                style={{ animationDelay: `${0.08 * i}s` }}>
                <div className={`absolute inset-0 bg-gradient-to-br ${s.gradient} pointer-events-none`} />
                <div className="absolute inset-x-0 top-0 h-0.5 scale-x-0 group-hover:scale-x-100 transition-transform duration-500 origin-left" style={{ background: `linear-gradient(90deg, transparent, ${s.accent}, transparent)` }} />
                <div className="relative p-4">
                  <div className="flex items-start justify-between mb-3">
                    <div className={`w-9 h-9 rounded-xl bg-gradient-to-br ${s.iconBg} flex items-center justify-center ${s.iconColor} group-hover:scale-110 transition-transform duration-300`}>
                      <span className="material-symbols-outlined text-[18px]">{s.icon}</span>
                    </div>
                    <span className="flex items-center gap-1 text-label-2xs px-2 py-0.5 rounded-full font-semibold bg-outline/5 text-on-surface-variant">
                      {stats.total > 0 ? Math.round((s.value / stats.total) * 100) : 0}%
                    </span>
                  </div>
                  <p className="text-label-xs text-outline mb-0.5 font-medium">{s.label}</p>
                  <div className="flex items-baseline gap-1.5">
                    <span className="text-2xl font-extrabold text-on-surface tabular-nums tracking-tight">{s.value}</span>
                    {s.label === "Total Brands" && (
                      <span className="text-label-2xs text-outline">total</span>
                    )}
                  </div>
                  <div className="mt-2.5 h-1 bg-outline/5 rounded-full overflow-hidden">
                    <motion.div
                      className={`h-full rounded-full bg-gradient-to-r ${s.bar}`}
                      initial={{ width: "0%" }}
                      animate={{ width: `${Math.min(100, (s.value / (stats.total || 1)) * 100)}%` }}
                      transition={{ duration: 1, delay: 0.5 + i * 0.12, ease: easeOut }} />
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        )}

        {/* ─── Search + Sort + Filter ─── */}
        <motion.div {...fadeUp} transition={{ duration: 0.5, delay: 0.2, ease: easeOut }}>
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex-1 relative max-w-md">
              <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center text-outline/35 pointer-events-none">
                <span className="material-symbols-outlined text-[18px]">search</span>
              </span>
              <input className="w-full bg-surface-container-lowest border border-outline-variant/10 rounded-xl py-2.5 pl-10 pr-9 text-body-sm placeholder:text-outline/30 focus:border-primary/40 focus:ring-2 focus:ring-primary/8 outline-none transition-all shadow-sm"
                placeholder="Search brands..." value={search} onChange={(e) => setSearch(e.target.value)} />
              {search && (
                <button onClick={() => setSearch("")} className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline/40 hover:text-on-surface active:scale-[0.97] focus-visible:outline-none">
                  <span className="material-symbols-outlined text-[16px]">close</span>
                </button>
              )}
            </div>

            {/* Sort */}
            <div className="relative">
              <button
                onClick={() => setShowSortMenu(!showSortMenu)}
                className="inline-flex items-center gap-1.5 px-3 py-2.5 rounded-xl border border-outline-variant/10 bg-surface-container-lowest text-body-sm text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all shadow-sm"
              >
                <span className="material-symbols-outlined text-[16px]">{sortDesc ? "arrow_downward" : "arrow_upward"}</span>
                <span className="max-w-[100px] truncate">
                  {sortBy === "name" ? "Name" : "Created Date"}
                </span>
                <span className="material-symbols-outlined text-[14px] text-outline/40">unfold_more</span>
              </button>
              {showSortMenu && (
                <>
                  <div className="fixed inset-0 z-10" onClick={() => setShowSortMenu(false)} />
                  <div className="absolute right-0 top-full mt-1 w-56 bg-surface-container-lowest rounded-xl border border-outline-variant/20 shadow-lg z-20 py-1 overflow-hidden">
                    <div className="px-3 py-2 text-label-xs text-on-surface-variant/60 font-semibold uppercase tracking-wide">Sort by</div>
                    {[
                      { key: "name", desc: false, label: "Name (A → Z)", icon: "sort_by_alpha" },
                      { key: "name", desc: true, label: "Name (Z → A)", icon: "sort_by_alpha" },
                      { key: "createdat", desc: true, label: "Created Date (Newest)", icon: "calendar_today" },
                      { key: "createdat", desc: false, label: "Created Date (Oldest)", icon: "calendar_today" },
                    ].map((opt) => {
                      const isActive = sortBy === opt.key && sortDesc === opt.desc;
                      return (
                        <button
                          key={`${opt.key}-${opt.desc}`}
                          onClick={() => {
                            setSortBy(opt.key);
                            setSortDesc(opt.desc);
                            setShowSortMenu(false);
                          }}
                          className={`w-full text-left px-3 py-2 text-body-sm hover:bg-surface-container transition-colors flex items-center gap-2 ${
                            isActive ? "text-primary font-semibold bg-primary/5" : "text-on-surface"
                          }`}
                        >
                          <span className="material-symbols-outlined text-[16px]">{opt.icon}</span>
                          <span>{opt.label}</span>
                          {isActive && <span className="material-symbols-outlined text-[14px] text-primary ml-auto">check</span>}
                        </button>
                      );
                    })}
                  </div>
                </>
              )}
            </div>

            {/* Show Deleted */}
            <button
              onClick={() => setIncludeDeleted(!includeDeleted)}
              className={`inline-flex items-center gap-1.5 px-3 py-2.5 rounded-xl border text-body-sm font-medium transition-all shadow-sm ${
                includeDeleted
                  ? "bg-amber-50 border-amber-200/50 text-amber-700"
                  : "border-outline-variant/10 bg-surface-container-lowest text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className="material-symbols-outlined text-[16px]">{includeDeleted ? "delete" : "delete_outline"}</span>
              {includeDeleted ? "Deleted Visible" : "Show Deleted"}
            </button>
          </div>
          {hasFilters && (
            <div className="flex flex-wrap items-center gap-1.5 mt-2">
              <span className="inline-flex items-center gap-0.5 px-2.5 py-0.5 rounded-full bg-primary/8 text-primary text-label-xs font-semibold">
                &ldquo;{search}&rdquo;
                <button onClick={() => setSearch("")} className="hover:text-primary/60 active:scale-[0.97] focus-visible:outline-none">
                  <span className="material-symbols-outlined text-label-xs">close</span>
                </button>
              </span>
              <button onClick={clearFilters} className="text-label-xs text-outline/40 hover:text-on-surface underline underline-offset-2 decoration-dotted ml-0.5 focus-visible:outline-none">Clear</button>
            </div>
          )}
        </motion.div>

        {/* ─── Cards ─── */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl overflow-hidden">
                <div className="h-32 bg-surface-container animate-pulse" />
                <div className="p-5 space-y-3">
                  <div className="h-5 w-2/3 bg-surface-container animate-pulse rounded" />
                  <div className="h-3 w-full bg-surface-container animate-pulse rounded" />
                  <div className="h-3 w-1/2 bg-surface-container animate-pulse rounded" />
                </div>
              </div>
            ))}
          </div>
        ) : filtered.length === 0 && brands.length > 0 ? (
          <motion.div {...fadeUp} transition={{ duration: 0.5, ease: easeOut }}
            className="flex flex-col items-center justify-center py-24 text-center gap-6">
            <div className="w-20 h-20 rounded-3xl bg-surface-container-high flex items-center justify-center">
              <span className="material-symbols-outlined text-outline/40 text-4xl">search_off</span>
            </div>
            <div className="max-w-sm">
              <h2 className="text-headline-md text-on-surface font-bold mb-2">No brands found</h2>
              <p className="text-body-md text-on-surface-variant">
                {search ? "No brands match your search. Try different keywords." : "No brands match the current filters."}
              </p>
            </div>
            {(search || includeDeleted) && (
              <button onClick={() => { setSearch(""); setIncludeDeleted(false); }} className="inline-flex items-center gap-1.5 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-label-sm shadow-lg shadow-primary/20 hover:scale-105 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 transition-all">
                <span className="material-symbols-outlined text-[16px]">close</span>
                Clear Filters
              </button>
            )}
          </motion.div>
          ) : brands.length === 0 ? (
          <motion.div {...fadeUp} transition={{ duration: 0.5, ease: easeOut }}
            className="flex flex-col items-center justify-center py-24 text-center gap-6">
            <div className="w-20 h-20 rounded-3xl bg-surface-container-high flex items-center justify-center">
              <span className="material-symbols-outlined text-outline/40 text-4xl">inventory_2</span>
            </div>
            <div className="max-w-sm">
              <h2 className="text-headline-md text-on-surface font-bold mb-2">No brands yet</h2>
              <p className="text-body-md text-on-surface-variant">Create your first brand to start managing products and campaigns</p>
            </div>
            {canEdit && (
              <button onClick={() => {
                if (!activeWorkspace) {
                  setError("Please select a Workspace first (go to Overview).");
                  return;
                }
                setShowCreateModal(true);
              }} className="inline-flex items-center gap-1.5 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-label-sm shadow-lg shadow-primary/20 hover:scale-105 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 transition-all">
                <span className="material-symbols-outlined text-[16px]">add</span>
                Create Your First Brand
              </button>
            )}
          </motion.div>
        ) : (
          <AnimatePresence mode="popLayout">
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6 auto-rows-min">
              {/* Sort: most content-rich first for visual hierarchy */}
              {filtered.map((brand, i) => {
                const c = pickColor(brand.id);
                const initials = getInitials(brand.name);

                return (
                  <motion.div
                    key={brand.id}
                    layout
                    variants={prefersReducedMotion ? undefined : cardVariants}
                    initial="hidden"
                    animate="visible"
                    exit="exit"
                    custom={i}>
                    <motion.div
                      whileHover={prefersReducedMotion ? {} : { y: -4 }}
                      className="group bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden transition-all duration-300 hover:shadow-[0_16px_48px_rgba(0,0,0,0.08)] hover:border-outline-variant/30 h-full flex flex-col">
                      <div className={`h-1 w-full bg-gradient-to-r ${c.gradient}`} />
                      <div className="p-6 flex flex-col flex-1">
                        <div className="flex items-start gap-4 min-w-0">
                          <div className={`w-14 h-14 rounded-2xl bg-gradient-to-br ${c.gradient} flex items-center justify-center text-white font-bold text-lg shadow-sm shrink-0 overflow-hidden`}>
                            {brand.logoUrl ? (
                              <img
                                src={brand.logoUrl}
                                alt={`${brand.name} logo`}
                                className="h-full w-full object-contain bg-white/20 p-1.5"
                              />
                            ) : (
                              initials
                            )}
                          </div>
                          <div className="min-w-0 flex-1">
                            <h3 className="text-headline-sm font-bold text-on-surface truncate leading-tight">{brand.name}</h3>
                            {brand.isDeleted && (
                              <div className="flex items-center gap-2 mt-1">
                                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold bg-red-50 text-red-600 border border-red-200/50">
                                  <span className="material-symbols-outlined text-[12px]">delete</span>
                                  Deleted
                                </span>
                                <button
                                  onClick={(e) => { e.stopPropagation(); handleRestoreBrand(brand.id); }}
                                  className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold bg-emerald-50 text-emerald-600 border border-emerald-200/50 hover:bg-emerald-100 transition-colors"
                                >
                                  <span className="material-symbols-outlined text-[12px]">restore</span>
                                  Restore
                                </button>
                              </div>
                            )}
                            {brand.slogan && (
                              <p className="text-label-sm text-on-surface-variant/50 italic truncate mt-0.5">&ldquo;{brand.slogan}&rdquo;</p>
                            )}
                          </div>
                          <div className="flex gap-1 shrink-0">
                            {canEdit && (
                              <>
                                <button onClick={(e) => { e.stopPropagation(); setEditingBrand(brand); }}
                                  className="w-8 h-8 rounded-xl flex items-center justify-center text-outline/40 hover:bg-surface-container hover:text-primary transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30"
                                  title="Edit">
                                  <span className="material-symbols-outlined text-[15px]">edit</span>
                                </button>
                                <button onClick={(e) => { e.stopPropagation(); setDeletingBrand(brand); }}
                                  className="w-8 h-8 rounded-xl flex items-center justify-center text-outline/40 hover:bg-surface-container hover:text-danger-red transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/30"
                                  title="Delete">
                                  <span className="material-symbols-outlined text-[15px]">delete</span>
                                </button>
                              </>
                            )}
                          </div>
                        </div>

                        {brand.description ? (
                          <p className="text-body-sm text-on-surface-variant/70 line-clamp-2 leading-relaxed mt-3 mb-4">{brand.description}</p>
                        ) : (
                          <p className="text-body-sm text-outline/30 italic mt-3 mb-4">No description</p>
                        )}

                        <div className="flex items-center justify-between mt-auto">
                          <div className="flex items-center gap-4 text-label-sm text-on-surface-variant/50">
                            <span className="flex items-center gap-1">
                              <span className="material-symbols-outlined text-[14px]">inventory_2</span>
                              {brand.productsCount} product{brand.productsCount !== 1 ? "s" : ""}
                            </span>
                            <span className="flex items-center gap-1">
                              <span className="material-symbols-outlined text-[14px]">auto_awesome</span>
                              {brand.contentsCount} content{brand.contentsCount !== 1 ? "s" : ""}
                            </span>
                          </div>
                          <button onClick={() => router.push(`/brands/${brand.id}`)}
                            className="inline-flex items-center gap-1 px-4 py-1.5 rounded-lg border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container hover:border-outline-variant/40 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 transition-all">
                            Details
                            <span className="material-symbols-outlined text-[14px]">chevron_right</span>
                          </button>
                        </div>
                      </div>
                    </motion.div>
                  </motion.div>
                );
              })}
            </div>
          </AnimatePresence>
        )}

        {/* ─── Pagination ─── */}
        {!loading && brands.length > 0 && totalPages > 1 && (
          <div className="flex items-center justify-between pt-2">
            <p className="text-label-sm text-on-surface-variant">
              Showing {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, totalCount)} of {totalCount}
            </p>
            <div className="flex items-center gap-1">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page <= 1}
                className="px-3 py-2 rounded-lg border border-outline-variant/20 text-body-sm text-on-surface-variant hover:bg-surface-container disabled:opacity-30 disabled:cursor-not-allowed transition-colors flex items-center gap-1"
              >
                <span className="material-symbols-outlined text-[16px]">chevron_left</span>
                Previous
              </button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  className={`w-9 h-9 rounded-lg text-body-sm font-medium transition-colors ${
                    p === page
                      ? "bg-primary text-on-primary shadow-sm"
                      : "text-on-surface-variant hover:bg-surface-container"
                  }`}
                >
                  {p}
                </button>
              ))}
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page >= totalPages}
                className="px-3 py-2 rounded-lg border border-outline-variant/20 text-body-sm text-on-surface-variant hover:bg-surface-container disabled:opacity-30 disabled:cursor-not-allowed transition-colors flex items-center gap-1"
              >
                Next
                <span className="material-symbols-outlined text-[16px]">chevron_right</span>
              </button>
            </div>
          </div>
        )}
      </main>

      <CreateBrandModal
        open={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSuccess={(brand) => {
            setBrands((prev) => [brand, ...prev]);
            addToast("Brand created successfully", "check");
          }}
        profileId={activeProfile?.id || ""}
      />

      {editingBrand && (
        <EditBrandModal
          open={!!editingBrand}
          onClose={() => setEditingBrand(null)}
          onSuccess={handleEditSuccess}
          brand={editingBrand}
        />
      )}

      {deletingBrand && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 8 }}
            transition={{ duration: 0.2, ease: easeOut }}
            className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Brand</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{deletingBrand.name}</span>? All associated products and campaigns will be permanently removed.
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={() => setDeletingBrand(null)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-outline">Cancel</button>
              <button onClick={handleDeleteBrand} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/50">Delete</button>
            </div>
          </motion.div>
        </motion.div>
      )}
    </>
  );
}
