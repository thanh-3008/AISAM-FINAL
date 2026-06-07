"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";
import { apiFetch } from "@/lib/apiClient";
import { useProfiles } from "@/hooks/useProfiles";
import CreateBrandModal from "@/components/brands/CreateBrandModal";
import EditBrandModal from "@/components/brands/EditBrandModal";

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
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  contentsCount: number;
}

const BRAND_COLORS = [
  { gradient: "from-blue-600 to-blue-400", light: "bg-blue-50", text: "text-blue-600", ring: "ring-blue-200" },
  { gradient: "from-emerald-600 to-emerald-400", light: "bg-emerald-50", text: "text-emerald-600", ring: "ring-emerald-200" },
  { gradient: "from-violet-600 to-violet-400", light: "bg-violet-50", text: "text-violet-600", ring: "ring-violet-200" },
  { gradient: "from-rose-600 to-rose-400", light: "bg-rose-50", text: "text-rose-600", ring: "ring-rose-200" },
  { gradient: "from-amber-600 to-amber-400", light: "bg-amber-50", text: "text-amber-600", ring: "ring-amber-200" },
  { gradient: "from-cyan-600 to-cyan-400", light: "bg-cyan-50", text: "text-cyan-600", ring: "ring-cyan-200" },
];

const MOCK_BRANDS: Brand[] = [
  { id: "mock-1", userId: "", name: "Lumina Tech", description: "Next-gen lighting solutions for smart homes and offices.", logoUrl: "", slogan: "Innovate Your Light", usp: "Smart lighting that adapts to your lifestyle", targetAudience: "Tech-savvy homeowners", profileId: null, productsCount: 3, contentsCount: 34, createdAt: "2025-01-15T00:00:00Z", updatedAt: "2025-06-04T00:00:00Z" },
  { id: "mock-2", userId: "", name: "Summit Outdoor", description: "Premium outdoor gear for adventure enthusiasts.", logoUrl: "", slogan: "Conquer Every Peak", usp: null, targetAudience: null, profileId: null, productsCount: 2, contentsCount: 0, createdAt: "2025-03-20T00:00:00Z", updatedAt: "2025-05-28T00:00:00Z" },
  { id: "mock-3", userId: "", name: "Heritage Motors", description: "Luxury automotive restoration and customization.", logoUrl: "", slogan: "Timeless Craftsmanship", usp: null, targetAudience: null, profileId: null, productsCount: 3, contentsCount: 8, createdAt: "2024-11-01T00:00:00Z", updatedAt: "2025-04-10T00:00:00Z" },
  { id: "mock-4", userId: "", name: "GreenLeaf Organics", description: "Organic farm-to-table produce and sustainable goods.", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 3, contentsCount: 15, createdAt: "2025-02-10T00:00:00Z", updatedAt: "2025-06-01T00:00:00Z" },
  { id: "mock-5", userId: "", name: "Pulse Finance", description: "Real-time financial analytics and portfolio management.", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 2, contentsCount: 21, createdAt: "2025-04-05T00:00:00Z", updatedAt: "2025-05-30T00:00:00Z" },
  { id: "mock-6", userId: "", name: "Apex Fitness", description: "", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 1, contentsCount: 0, createdAt: "2025-05-01T00:00:00Z", updatedAt: "2025-05-25T00:00:00Z" },
];

function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

export default function BrandsPage() {
  const router = useRouter();
  const { activeProfile } = useProfiles();
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingBrand, setEditingBrand] = useState<Brand | null>(null);
  const [deletingBrand, setDeletingBrand] = useState<Brand | null>(null);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 80);
    return () => clearTimeout(timer);
  }, []);

  const fetchBrands = useCallback(async () => {
    if (!activeProfile) { setLoading(false); setBrands(MOCK_BRANDS); return; }
    try {
      const result = await apiFetch(`/brands?profileId=${activeProfile.id}&pageSize=100`);
      if (result?.success && result.data?.data) setBrands(result.data.data as Brand[]);
      else setBrands(MOCK_BRANDS);
    } catch { setBrands(MOCK_BRANDS); }
    finally { setLoading(false); }
  }, [activeProfile]);

  useEffect(() => { fetchBrands(); }, [fetchBrands]);

  const filtered = useMemo(() => {
    let list = brands;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((b) => b.name.toLowerCase().includes(q) || (b.description && b.description.toLowerCase().includes(q)));
    }
    return [...list].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
  }, [brands, search]);

  const stats = useMemo(() => ({
    total: brands.length, active: brands.filter((b) => b.productsCount > 0).length, draft: brands.filter((b) => b.productsCount === 0).length,
  }), [brands]);

  const hasFilters = !!search;
  const clearFilters = () => setSearch("");

  const handleEditSuccess = (updated: Brand) => {
    setBrands((prev) => {
      const next = prev.map((b) => (b.id === updated.id ? updated : b));
      MOCK_BRANDS.splice(0, MOCK_BRANDS.length, ...next);
      return next;
    });
    setEditingBrand(null);
  };

  const handleDeleteBrand = async () => {
    if (!deletingBrand) return;
    const brandToDelete = deletingBrand;
    setDeletingBrand(null);
    try {
      await apiFetch(`/brands/${brandToDelete.id}`, { method: "DELETE" });
    } catch {
      // mock fallback — remove from local state
    }
    setBrands((prev) => {
      const next = prev.filter((b) => b.id !== brandToDelete.id);
      MOCK_BRANDS.splice(0, MOCK_BRANDS.length, ...next);
      return next;
    });
  };

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-6px); } }
        @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
        @keyframes bar-rise { from { transform: scaleY(0); } to { transform: scaleY(1); } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
        .card-hover { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
        .card-hover:hover { transform: translateY(-4px); box-shadow: 0 12px 40px -12px rgba(0,0,0,0.15); }
        @supports (animation-timeline: scroll()) {
          .shimmer-bg { background: linear-gradient(90deg, transparent, rgba(255,255,255,0.03), transparent); background-size: 200% 100%; animation: shimmer 3s ease-in-out infinite; }
        }
        .shimmer-bg {
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.03), transparent);
          background-size: 200% 100%;
          animation: shimmer 3s ease-in-out infinite;
        }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-8">

        {/* ─── Hero ─── */}
        <div className={`flex items-center justify-between ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0s" }}>
          <div className="flex items-center gap-3">
            <div className="relative w-10 h-10 shrink-0">
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-md shadow-primary/20" />
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
              <div className="relative w-full h-full flex items-center justify-center">
                <span className="material-symbols-outlined text-on-primary text-[20px]">workspace_premium</span>
              </div>
            </div>
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">Brand Management</h1>
              <p className="text-body-sm text-on-surface-variant">Manage your brand portfolios and product catalogs</p>
            </div>
          </div>
          <button onClick={() => setShowCreateModal(true)}
            className="inline-flex items-center gap-1.5 px-4 py-2 bg-primary text-on-primary rounded-xl font-semibold text-label-sm hover:shadow-lg hover:shadow-primary/25 active:scale-[0.97] transition-all shrink-0">
            <span className="material-symbols-outlined text-[16px]">add</span>
            New Brand
          </button>
        </div>

        {/* ─── Stats ─── */}
        {!loading && brands.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
            {[
              { label: "Total Brands", value: stats.total, icon: "inventory_2", iconBg: "from-blue-500/20 to-blue-600/10", iconColor: "text-blue-500", bar: "bg-blue-500", accent: "#3b82f6" },
              { label: "With Products", value: stats.active, icon: "check_circle", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", bar: "bg-emerald-500", accent: "#10b981" },
              { label: "No Products", value: stats.draft, icon: "edit_note", iconBg: "from-amber-500/20 to-amber-600/10", iconColor: "text-amber-500", bar: "bg-amber-500", accent: "#f59e0b" },
            ].map((s, i) => (
              <div key={s.label}
                className={`relative bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden group ${visible ? "animate-fade-up" : ""} card-hover`}
                style={{ animationDelay: `${0.08 + 0.08 * i}s` }}>
                <div className="p-5 flex items-center gap-4">
                  <div className={`w-12 h-12 rounded-2xl bg-gradient-to-br ${s.iconBg} flex items-center justify-center ${s.iconColor} shrink-0`}>
                    <span className="material-symbols-outlined text-[24px]">{s.icon}</span>
                  </div>
                  <div>
                    <p className="text-label-2xs text-outline/50 uppercase tracking-widest font-semibold">{s.label}</p>
                    <p className="text-3xl font-extrabold text-on-surface tabular-nums tracking-tight">{s.value}</p>
                  </div>
                </div>
                <div className="h-1 bg-outline-variant/10 mx-5 mb-4 overflow-hidden rounded-full">
                  <div className={`h-full ${s.bar} rounded-full`} style={{ width: `${Math.min(100, (s.value / (stats.total || 1)) * 100)}%`, animation: `${visible ? "bar-rise 0.8s ease-out 0.5s forwards" : "none"}`, transformOrigin: "bottom", transform: "scaleY(0)" }} />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* ─── Search ─── */}
        <div className={`flex flex-col sm:flex-row gap-2 items-stretch sm:items-center ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.32s" }}>
          <div className="flex-1 relative max-w-sm">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-outline/40 pointer-events-none">
              <span className="material-symbols-outlined text-[18px]">search</span>
            </span>
            <input className="w-full bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 pl-10 pr-9 text-body-sm placeholder:text-outline/30 focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm"
              placeholder="Search brands..." value={search} onChange={(e) => setSearch(e.target.value)} />
            {search && (
              <button onClick={() => setSearch("")} className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline/40 hover:text-on-surface active:scale-[0.97]">
                <span className="material-symbols-outlined text-[16px]">close</span>
              </button>
            )}
          </div>
        </div>
        {hasFilters && (
          <div className={`flex flex-wrap items-center gap-1.5 -mt-5 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.4s" }}>
            {search && (
              <span className="inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full bg-primary/8 text-primary text-label-xs font-semibold">
                &ldquo;{search}&rdquo;
                <button onClick={() => setSearch("")} className="hover:text-primary/60 active:scale-[0.97]"><span className="material-symbols-outlined text-label-xs">close</span></button>
              </span>
            )}
            <button onClick={clearFilters} className="text-label-xs text-outline/40 hover:text-on-surface underline underline-offset-2 decoration-dotted ml-0.5">Clear</button>
          </div>
        )}

        {/* ─── Content ─── */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-gutter">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl overflow-hidden animate-pulse">
                <div className="p-6 space-y-4">
                  <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-2xl bg-surface-container" />
                    <div className="space-y-2 flex-1">
                      <div className="h-5 w-40 bg-surface-container rounded" />
                      <div className="h-3 w-24 bg-surface-container rounded" />
                    </div>
                  </div>
                  <div className="h-3 w-full bg-surface-container rounded" />
                  <div className="h-3 w-3/4 bg-surface-container rounded" />
                  <div className="flex gap-4 pt-2">
                    <div className="h-4 w-16 bg-surface-container rounded" />
                    <div className="h-4 w-16 bg-surface-container rounded" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : brands.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-24 text-center gap-6">
            <div className="w-20 h-20 rounded-3xl bg-surface-container-high flex items-center justify-center">
              <span className="material-symbols-outlined text-outline/40 text-4xl">inventory_2</span>
            </div>
            <div className="max-w-sm">
              <h2 className="text-headline-md text-on-surface font-bold mb-2">No brands yet</h2>
              <p className="text-body-md text-on-surface-variant">Create your first brand to start managing products and campaigns</p>
            </div>
            <button onClick={() => setShowCreateModal(true)} className="inline-flex items-center gap-1.5 px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-label-sm hover:shadow-lg hover:shadow-primary/25 active:scale-[0.97] transition-all">
              <span className="material-symbols-outlined text-[16px]">add</span>
              Create Your First Brand
            </button>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-gutter">
              {filtered.map((brand, i) => {
                const colorIdx = brand.id.split("").reduce((a, c) => a + c.charCodeAt(0), 0) % BRAND_COLORS.length;
                const c = BRAND_COLORS[colorIdx];
                const initials = getInitials(brand.name);
                return (
                  <div key={brand.id}
                    className={`group bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden card-hover ${visible ? "animate-fade-up" : ""}`}
                    style={{ animationDelay: `${0.16 + i * 0.06}s` }}>
                    <div className={`h-2 w-full bg-gradient-to-r ${c.gradient} opacity-60`} />
                    <div className="p-6">
                      {/* Header */}
                      <div className="flex items-center gap-4 mb-4">
                        <div className={`w-14 h-14 rounded-2xl bg-gradient-to-br ${c.gradient} flex items-center justify-center text-white font-extrabold text-xl shadow-sm shrink-0 group-hover:scale-105 transition-transform duration-300`}>
                          {brand.logoUrl ? (
                            <img src={brand.logoUrl} alt={brand.name} className="w-full h-full object-cover rounded-2xl" />
                          ) : initials}
                        </div>
                        <div className="min-w-0 flex-1">
                          <h3 className="text-headline-sm font-bold text-on-surface truncate">{brand.name}</h3>
                          <div className="flex items-center gap-2 text-label-sm text-on-surface-variant/60">
                            <span>{brand.productsCount} products</span>
                            <span className="w-1 h-1 rounded-full bg-outline/30" />
                            <span>{brand.contentsCount} contents</span>
                          </div>
                        </div>
                      </div>

                      {/* Description */}
                      {brand.description && (
                        <p className="text-body-sm text-on-surface-variant/70 line-clamp-2 mb-4 leading-relaxed">{brand.description}</p>
                      )}

                      {/* Slogan / USP */}
                      {brand.slogan && (
                        <div className="flex items-center gap-1.5 mb-3">
                          <span className="material-symbols-outlined text-[14px] text-outline/40">format_quote</span>
                          <span className="text-[11px] text-outline/60 italic">&ldquo;{brand.slogan}&rdquo;</span>
                        </div>
                      )}

                      {/* Actions */}
                      <div className="flex items-center justify-between pt-4 border-t border-outline-variant/10">
                        <button onClick={() => router.push(`/brands/${brand.id}`)}
                          className="px-3 py-1.5 bg-primary text-on-primary rounded-xl text-label-xs font-semibold hover:bg-primary/90 hover:shadow-md active:scale-[0.97] transition-all relative overflow-hidden group/btn">
                          <span className="relative z-10">Details</span>
                          <span className="absolute inset-0 bg-white/10 translate-x-[-100%] group-hover/btn:translate-x-0 transition-transform duration-300" />
                        </button>
                        <div className="flex gap-1">
                          <button onClick={() => setEditingBrand(brand)} className="w-8 h-8 flex items-center justify-center rounded-full border border-outline-variant/30 text-outline/40 hover:bg-surface-container hover:text-on-surface hover:border-primary/30 transition-all hover:scale-110 active:scale-95" title="Edit">
                            <span className="material-symbols-outlined text-[16px]">edit</span>
                          </button>
                          <button onClick={() => setDeletingBrand(brand)} className="w-8 h-8 flex items-center justify-center rounded-full border border-outline-variant/30 text-outline/40 hover:bg-error-container/10 hover:text-danger-red hover:border-danger-red/30 transition-all hover:scale-110 active:scale-95" title="Delete">
                            <span className="material-symbols-outlined text-[16px]">delete</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </>
        )}
      </main>

      <CreateBrandModal
        open={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSuccess={(brand) => setBrands((prev) => {
          const next = [brand, ...prev];
          MOCK_BRANDS.splice(0, MOCK_BRANDS.length, ...next);
          return next;
        })}
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
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
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
              <button onClick={() => setDeletingBrand(null)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
              <button onClick={handleDeleteBrand} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2">Delete</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
