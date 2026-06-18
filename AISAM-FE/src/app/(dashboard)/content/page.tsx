"use client";

import { useState, useEffect, useMemo, useRef, useCallback } from "react";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";
import { PLATFORM_CONFIG, ALL_PLATFORMS, CONTENT_TYPES, STATUS_OPTIONS, STATUS_STYLES, ALL_TAGS, getTypeConfig, getTypeStyle, getTypeBadgeStyle, getTypeIcon, PlatformIcon } from "@/lib/contentConstants";
import { fetchContents, createContent, updateContent, deleteContent, type ContentItem, type ContentType, type ContentStatus, type CreateContentPayload, type UpdateContentPayload } from "@/services/contentService";
import { fetchBrands } from "@/services/brandService";

type ViewMode = "grid" | "list";
type SortKey = "newest" | "oldest" | "title-asc" | "title-desc" | "brand-asc" | "product-asc" | "status";

interface Toast {
  id: number;
  message: string;
  icon: string;
}

const SORT_OPTIONS: { label: string; value: SortKey }[] = [
  { label: "Newest First", value: "newest" },
  { label: "Oldest First", value: "oldest" },
  { label: "Title A–Z", value: "title-asc" },
  { label: "Title Z–A", value: "title-desc" },
  { label: "Brand A–Z", value: "brand-asc" },
  { label: "Product A–Z", value: "product-asc" },
  { label: "By Status", value: "status" },
];


const PAGE_SIZE = 9;

const AI_QUICK_ASSISTANT_ACTIONS = [
  { icon: "auto_awesome", label: "Generate Caption", desc: "AI writes engaging captions" },
  { icon: "refresh", label: "Rewrite Content", desc: "Rephrase for better reach" },
  { icon: "translate", label: "Translate", desc: "Expand to new languages" },
  { icon: "trending_up", label: "Optimize", desc: "Boost engagement scores" },
];

let toastId = 0;

export default function ContentPage() {
  const router = useRouter();
  const [visible, setVisible] = useState(false);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [brandFilter, setBrandFilter] = useState("");
  const [productFilter, setProductFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState<ContentType | "">("");
  const [statusFilter, setStatusFilter] = useState<ContentStatus | "">("");
  const [platformFilter, setPlatformFilter] = useState<string[]>([]);
  const [tagFilter, setTagFilter] = useState<string>("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [sortBy, setSortBy] = useState<SortKey>("newest");
  const [viewMode, setViewMode] = useState<ViewMode>("grid");
  const [page, setPage] = useState(1);
  const [showCreateMenu, setShowCreateMenu] = useState(false);
  const [showPlatformFilter, setShowPlatformFilter] = useState(false);
  const [openMenuId, setOpenMenuId] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [toasts, setToasts] = useState<Toast[]>([]);
  const [editingItem, setEditingItem] = useState<ContentItem | null>(null);
  const [deletingItem, setDeletingItem] = useState<ContentItem | null>(null);
  const [previewItem, setPreviewItem] = useState<ContentItem | null>(null);
  const [batchStatus, setBatchStatus] = useState<ContentStatus | "">("");
  const [allContent, setAllContent] = useState<ContentItem[]>([]);
  const [brandNameList, setBrandNameList] = useState<string[]>([]);
  const createBtnRef = useRef<HTMLButtonElement>(null);
  const [createMenuStyle, setCreateMenuStyle] = useState<{ top: number; right: number } | null>(null);

  const addToast = useCallback((message: string, icon: string) => {
    const id = ++toastId;
    setToasts((prev) => [...prev, { id, message, icon }]);
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), 3000);
  }, []);

  // Load viewMode from localStorage
  useEffect(() => {
    try {
      const saved = localStorage.getItem("content-view-mode");
      if (saved === "grid" || saved === "list") setViewMode(saved);
    } catch (e) { console.error("content: localStorage read failed", e); }
  }, []);

  // Save viewMode to localStorage
  useEffect(() => {
    try { localStorage.setItem("content-view-mode", viewMode); } catch (e) { console.error("content: localStorage write failed", e); }
  }, [viewMode]);

  const loadContent = useCallback(async () => {
    setLoading(true);
    const result = await fetchContents({ pageSize: 100 });
    if (result) {
      setAllContent(result.items);
    }
    setLoading(false);
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 80);
    loadContent();
    fetchBrands().then(list => setBrandNameList(list.map(b => b.name)));
    return () => clearTimeout(timer);
  }, [loadContent]);

  const filtered = useMemo(() => {
    let list = allContent;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((c) => c.title.toLowerCase().includes(q) || c.brandName.toLowerCase().includes(q));
    }
    if (brandFilter) list = list.filter((c) => c.brandName === brandFilter);
    if (productFilter) list = list.filter((c) => c.productName === productFilter);
    if (typeFilter) list = list.filter((c) => c.type === typeFilter);
    if (statusFilter) list = list.filter((c) => c.status === statusFilter);
    if (platformFilter.length > 0) {
      list = list.filter((c) => platformFilter.some((p) => c.platforms.includes(p)));
    }
    if (tagFilter) list = list.filter((c) => (c.tags ?? []).includes(tagFilter));
    if (dateFrom) list = list.filter((c) => new Date(c.createdAt) >= new Date(dateFrom));
    if (dateTo) list = list.filter((c) => new Date(c.createdAt) <= new Date(dateTo + "T23:59:59Z"));

    switch (sortBy) {
      case "oldest": list.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()); break;
      case "title-asc": list.sort((a, b) => a.title.localeCompare(b.title)); break;
      case "title-desc": list.sort((a, b) => b.title.localeCompare(a.title)); break;
      case "brand-asc": list.sort((a, b) => a.brandName.localeCompare(b.brandName)); break;
      case "product-asc": list.sort((a, b) => a.productName.localeCompare(b.productName)); break;
      case "status": {
        const order: Record<ContentStatus, number> = { "Published": 0, "Scheduled": 1, "Approved": 2, "Awaiting Approval": 3, "Draft": 4, "Rejected": 5 };
        list.sort((a, b) => order[a.status] - order[b.status]); break;
      }
      default: list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    }
    return list;
  }, [allContent, search, brandFilter, productFilter, typeFilter, statusFilter, platformFilter, dateFrom, dateTo, sortBy]);

  const paginated = useMemo(() => filtered.slice(0, page * PAGE_SIZE), [filtered, page]);
  const totalPages = Math.ceil(filtered.length / PAGE_SIZE);
  const hasMore = page < totalPages;

  const stats = useMemo(() => ({
    total: allContent.length,
    published: allContent.filter((c) => c.status === "Published").length,
    scheduled: allContent.filter((c) => c.status === "Scheduled").length,
    draft: allContent.filter((c) => c.status === "Draft" || c.status === "Awaiting Approval").length,
  }), [allContent]);

  const availableProducts = useMemo(() => {
    if (!brandFilter) return [];
    return [...new Set(allContent.filter((c) => c.brandName === brandFilter).map((c) => c.productName))];
  }, [brandFilter, allContent]);

  const hasFilters = !!search || !!brandFilter || !!productFilter || !!typeFilter || !!statusFilter || platformFilter.length > 0 || !!tagFilter || !!dateFrom || !!dateTo || sortBy !== "newest";

  const clearFilters = () => {
    setSearch(""); setBrandFilter(""); setProductFilter(""); setTypeFilter("");
    setStatusFilter(""); setPlatformFilter([]); setTagFilter(""); setDateFrom(""); setDateTo(""); setSortBy("newest");
  };

  // Bulk select helpers
  const allVisibleSelected = paginated.length > 0 && paginated.every((c) => selectedIds.has(c.id));
  const toggleSelectAll = () => {
    if (allVisibleSelected) setSelectedIds(new Set());
    else setSelectedIds(new Set(paginated.map((c) => c.id)));
  };
  const toggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const handleBatchDelete = async () => {
    const ids = new Set(selectedIds);
    for (const id of ids) {
      await deleteContent(id);
    }
    setSelectedIds(new Set());
    loadContent();
    addToast(`Deleted ${ids.size} items`, "delete");
  };

  const handleBatchStatusChange = async () => {
    if (!batchStatus) return;
    const ids = Array.from(selectedIds);
    const statusMap: Record<string, number> = { "Draft": 0, "Awaiting Approval": 1, "Approved": 2, "Rejected": 3, "Published": 4 };
    await Promise.all(ids.map(id => updateContent(id, { status: statusMap[batchStatus] as any })));
    setSelectedIds(new Set());
    setBatchStatus("");
    loadContent();
    addToast(`Updated ${ids.length} items to ${batchStatus}`, "check_circle");
  };

  // Create action
  const handleCreateAction = (action: string) => {
    setShowCreateMenu(false);
    if (action === "Manual Creation") {
      router.push("/content/create");
      return;
    }
    if (action === "AI Generate") {
      router.push("/content/ai-generate");
      return;
    }
    addToast(`${action} — coming soon`, "construction");
  };

  // Card actions
  const handleCardAction = (action: string, item: ContentItem) => {
    setOpenMenuId(null);
    switch (action) {
      case "Preview":
        setPreviewItem(item);
        break;
      case "View Details":
        router.push(`/content/${item.id}`);
        break;
      case "Edit":
        setEditingItem(item);
        break;
      case "delete":
        setDeletingItem(item);
        break;
      case "Duplicate": {
        addToast(`"${item.title}" duplicated`, "content_copy");
        break;
      }
    }
  };

  // Edit save
  const handleEditSave = async (updated: ContentItem) => {
    await updateContent(updated.id, {
      title: updated.title,
      adType: updated.type === "TEXT" ? 0 : updated.type === "IMAGE" ? 1 : 2,
      textContent: updated.thumbnail || undefined,
      imageUrl: updated.thumbnail || undefined,
      status: updated.status === "Draft" ? 0 : updated.status === "Awaiting Approval" ? 1 : updated.status === "Approved" ? 2 : updated.status === "Rejected" ? 3 : 4,
    });
    setEditingItem(null);
    loadContent();
    addToast(`"${updated.title}" updated`, "check_circle");
  };

  // Delete confirm
  const handleDeleteConfirm = async () => {
    if (!deletingItem) return;
    await deleteContent(deletingItem.id);
    setSelectedIds((prev) => { const n = new Set(prev); n.delete(deletingItem.id); return n; });
    setDeletingItem(null);
    loadContent();
    addToast(`"${deletingItem.title}" deleted`, "delete");
  };

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-6px); } }
        @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }
        @keyframes bar-rise { from { transform: scaleY(0); } to { transform: scaleY(1); } }
        @keyframes slide-up-row { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes toast-in { from { opacity: 0; transform: translateX(100%); } to { opacity: 1; transform: translateX(0); } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
        .card-hover { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
        .card-hover:hover { transform: translateY(-4px); box-shadow: 0 12px 40px -12px rgba(0,0,0,0.15); }
        .shimmer-bg { background: linear-gradient(90deg, transparent, rgba(255,255,255,0.03), transparent); background-size: 200% 100%; animation: shimmer 3s ease-in-out infinite; }
        .dropdown-enter { animation: fade-up 0.15s ease-out forwards; }
        .toast-in { animation: toast-in 0.3s ease-out forwards; }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Content Library" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-6">

        {/* ─── Hero ─── */}
        <div className={`flex items-center justify-between ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0s" }}>
          <div className="flex items-center gap-3">
            <div className="relative w-10 h-10 shrink-0">
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-md shadow-primary/20" />
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
              <div className="relative w-full h-full flex items-center justify-center">
                <span className="material-symbols-outlined text-on-primary text-[20px]">library_books</span>
              </div>
            </div>
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">Content Library</h1>
              <p className="text-body-sm text-on-surface-variant">Create, manage, and publish your brand content</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <button onClick={() => { setLoading(true); setTimeout(() => setLoading(false), 400); }}
              className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97]"
              title="Refresh">
              <span className="material-symbols-outlined text-[18px]">refresh</span>
            </button>
            <div className="relative">
              <button onClick={() => addToast("Export feature coming soon", "file_download")}
                className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97]"
                title="Export">
                <span className="material-symbols-outlined text-[18px]">file_download</span>
              </button>
            </div>
            <div className="relative">
              <button ref={createBtnRef} onClick={() => {
                  if (!showCreateMenu && createBtnRef.current) {
                    const rect = createBtnRef.current.getBoundingClientRect();
                    setCreateMenuStyle({ top: rect.bottom + 8, right: window.innerWidth - rect.right });
                  }
                  setShowCreateMenu((p) => !p);
                }}
                className="inline-flex items-center gap-1.5 px-4 py-2 bg-primary text-on-primary rounded-xl font-semibold text-label-sm hover:shadow-lg hover:shadow-primary/25 active:scale-[0.97] transition-all shrink-0">
                <span className="material-symbols-outlined text-[16px]">add</span>
                Create New Content
                <span className={`material-symbols-outlined text-[14px] transition-transform ${showCreateMenu ? "rotate-180" : ""}`}>expand_more</span>
              </button>
            </div>
          </div>
        </div>

        {/* ─── Stats ─── */}
        {!loading && (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-gutter">
            {[
              { label: "Total Content", value: stats.total, icon: "library_books", iconBg: "from-primary/20 to-primary/10", iconColor: "text-primary", bar: "bg-primary" },
              { label: "Published", value: stats.published, icon: "check_circle", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", bar: "bg-emerald-500" },
              { label: "Scheduled", value: stats.scheduled, icon: "schedule", iconBg: "from-blue-500/20 to-blue-600/10", iconColor: "text-blue-500", bar: "bg-blue-500" },
              { label: "Draft / Review", value: stats.draft, icon: "edit_note", iconBg: "from-amber-500/20 to-amber-600/10", iconColor: "text-amber-500", bar: "bg-amber-500" },
            ].map((s, i) => (
              <div key={s.label}
                className={`relative bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden group ${visible ? "animate-fade-up" : ""} card-hover`}
                style={{ animationDelay: `${0.08 + 0.08 * i}s` }}>
                <div className="p-5 flex items-center gap-4">
                  <div className={`w-11 h-11 rounded-2xl bg-gradient-to-br ${s.iconBg} flex items-center justify-center ${s.iconColor} shrink-0`}>
                    <span className="material-symbols-outlined text-[22px]">{s.icon}</span>
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

        {/* ─── Filters ─── */}
        <div className={`flex flex-wrap items-center gap-2 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.32s" }}>
          <div className="flex-1 min-w-[180px] max-w-sm relative">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-outline/40 pointer-events-none">
              <span className="material-symbols-outlined text-[18px]">search</span>
            </span>
            <input className="w-full bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 pl-10 pr-9 text-body-sm placeholder:text-outline/30 focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm"
              placeholder="Search content..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} />
            {search && (
              <button onClick={() => setSearch("")} className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline/40 hover:text-on-surface active:scale-[0.97]">
                <span className="material-symbols-outlined text-[16px]">close</span>
              </button>
            )}
          </div>

          {/* Tags filter */}
          <select value={tagFilter} onChange={(e) => { setTagFilter(e.target.value); setPage(1); }}
            className="bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 px-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm min-w-[100px]">
            <option value="">Tags</option>
            {ALL_TAGS.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>

          {/* Date range */}
          <div className="flex items-center gap-1.5">
            <input type="date" value={dateFrom} onChange={(e) => { setDateFrom(e.target.value); setPage(1); }}
              className="bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 px-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm w-[140px]" />
            <span className="text-outline/30 text-label-xs">–</span>
            <input type="date" value={dateTo} onChange={(e) => { setDateTo(e.target.value); setPage(1); }}
              className="bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 px-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm w-[140px]" />
          </div>

          <select value={sortBy} onChange={(e) => setSortBy(e.target.value as SortKey)}
            className="bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-2.5 px-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all shadow-sm min-w-[110px]">
            {SORT_OPTIONS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>

          <div className="flex items-center gap-1 bg-surface-container-lowest border border-outline-variant/15 rounded-xl p-1 shadow-sm">
            <button onClick={() => setViewMode("grid")}
              className={`w-8 h-8 flex items-center justify-center rounded-lg transition-all ${viewMode === "grid" ? "bg-white text-primary shadow-sm" : "text-outline/50 hover:text-on-surface"}`}>
              <span className="material-symbols-outlined text-[18px]">grid_view</span>
            </button>
            <button onClick={() => setViewMode("list")}
              className={`w-8 h-8 flex items-center justify-center rounded-lg transition-all ${viewMode === "list" ? "bg-white text-primary shadow-sm" : "text-outline/50 hover:text-on-surface"}`}>
              <span className="material-symbols-outlined text-[18px]">list</span>
            </button>
          </div>

          {hasFilters && (
            <button onClick={clearFilters} className="text-label-sm text-outline/40 hover:text-on-surface underline underline-offset-2 decoration-dotted whitespace-nowrap">
              Clear all
            </button>
          )}
        </div>

        {/* ─── Active filter chips ─── */}
        {hasFilters && (
          <div className={`flex flex-wrap items-center gap-1.5 -mt-4 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.36s" }}>
            {search && <FilterChip label={`"${search}"`} onRemove={() => setSearch("")} />}
            {brandFilter && <FilterChip label={brandFilter} color="blue" onRemove={() => { setBrandFilter(""); setProductFilter(""); }} />}
            {productFilter && <FilterChip label={productFilter} color="purple" onRemove={() => setProductFilter("")} />}
            {typeFilter && <FilterChip label={getTypeConfig(typeFilter).label} color="amber" onRemove={() => setTypeFilter("")} />}
            {statusFilter && <FilterChip label={statusFilter} color="rose" onRemove={() => setStatusFilter("")} />}
            {platformFilter.map((p) => <FilterChip key={p} label={p} color="indigo" onRemove={() => { setPlatformFilter((prev) => prev.filter((x) => x !== p)); }} />)}
            {tagFilter && <FilterChip label={tagFilter} onRemove={() => setTagFilter("")} />}
            {dateFrom && <FilterChip label={`From ${dateFrom}`} onRemove={() => setDateFrom("")} />}
            {dateTo && <FilterChip label={`To ${dateTo}`} onRemove={() => setDateTo("")} />}
            {sortBy !== "newest" && <FilterChip label={SORT_OPTIONS.find((s) => s.value === sortBy)?.label || ""} color="indigo" onRemove={() => setSortBy("newest")} />}
            <button onClick={clearFilters} className="text-label-xs text-outline/40 hover:text-on-surface underline underline-offset-2 decoration-dotted ml-0.5">Clear</button>
          </div>
        )}

        {/* ─── Results count ─── */}
        {!loading && filtered.length > 0 && (
          <div className={`flex items-center justify-between ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.4s" }}>
            <p className="text-body-sm text-on-surface-variant">
              Showing <span className="font-semibold text-on-surface">{paginated.length}</span> of <span className="font-semibold text-on-surface">{filtered.length}</span> results
            </p>
          </div>
        )}

        {/* ─── Batch Actions Bar ─── */}
        {selectedIds.size > 0 && (
          <div className={`flex items-center gap-3 px-5 py-3 bg-primary/5 border border-primary/20 rounded-2xl ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.42s" }}>
            <span className="text-label-sm text-on-surface font-semibold">{selectedIds.size} selected</span>
            <div className="h-4 w-px bg-outline-variant/20" />
            <select value={batchStatus} onChange={(e) => setBatchStatus(e.target.value as ContentStatus)}
              className="bg-surface-container-lowest border border-outline-variant/15 rounded-xl py-1.5 px-2.5 text-label-sm text-on-surface focus:border-primary/40 outline-none transition-all shadow-sm">
              <option value="">Set status...</option>
              {STATUS_OPTIONS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </select>
            {batchStatus && (
              <button onClick={handleBatchStatusChange}
                className="px-3 py-1.5 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all">
                Apply
              </button>
            )}
            <div className="flex-1" />
            <button onClick={() => setSelectedIds(new Set())}
              className="text-label-sm text-outline/50 hover:text-on-surface underline underline-offset-2 decoration-dotted transition-colors">
              Deselect
            </button>
            <button onClick={handleBatchDelete}
              className="px-3 py-1.5 rounded-xl border border-danger-red/20 text-danger-red text-label-sm font-semibold hover:bg-danger-red/5 active:scale-[0.97] transition-all flex items-center gap-1.5">
              <span className="material-symbols-outlined text-[14px]">delete</span>
              Delete
            </button>
          </div>
        )}

        {/* ─── Content Grid + Sidebar ─── */}
        <div className="flex flex-col xl:flex-row gap-gutter">
          {/* Content Area */}
          <div className="flex-1 min-w-0">
            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 2xl:grid-cols-3 gap-gutter">
                {Array.from({ length: 6 }).map((_, i) => (
                  <div key={i} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/10 overflow-hidden animate-pulse">
                    <div className="aspect-[4/3] bg-surface-container" />
                    <div className="p-5 space-y-3">
                      <div className="h-4 w-3/4 bg-surface-container rounded" />
                      <div className="h-3 w-1/2 bg-surface-container rounded" />
                      <div className="flex justify-between pt-2">
                        <div className="h-3 w-20 bg-surface-container rounded" />
                        <div className="h-3 w-16 bg-surface-container rounded" />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : filtered.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-24 text-center gap-6">
                <div className="w-20 h-20 rounded-3xl bg-surface-container-high flex items-center justify-center">
                  <span className="material-symbols-outlined text-outline/40 text-4xl">library_books</span>
                </div>
                <div className="max-w-sm">
                  <h2 className="text-headline-md text-on-surface font-bold mb-2">No content found</h2>
                  <p className="text-body-md text-on-surface-variant">Try adjusting your filters or create new content</p>
                </div>
              </div>
            ) : viewMode === "grid" ? (
              <div className="grid grid-cols-1 md:grid-cols-2 2xl:grid-cols-3 gap-gutter">
                {paginated.map((item, i) => (
                  <div key={item.id} className={`relative group bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden card-hover ${visible ? "animate-fade-up" : ""}`}
                    style={{ animationDelay: `${0.16 + i * 0.06}s` }}>
                    {item.id && (
                      <div className="absolute top-3 left-3 z-10">
                        <input type="checkbox" checked={selectedIds.has(item.id)} onChange={() => toggleSelect(item.id)}
                          className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30 cursor-pointer transition-opacity"
                          style={{ opacity: selectedIds.size > 0 || selectedIds.has(item.id) ? 1 : undefined }}
                          onClick={(e) => e.stopPropagation()} />
                      </div>
                    )}
                    <ContentCard item={item} index={i} visible={visible} openMenuId={openMenuId} onToggleMenu={setOpenMenuId} onAction={handleCardAction} />
                  </div>
                ))}
              </div>
            ) : (
              <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden">
                <table className="w-full text-left">
                  <thead>
                    <tr className="text-label-sm text-outline border-b border-outline-variant/10">
                      <th className="px-3 py-3.5 w-10">
                        <input type="checkbox" checked={allVisibleSelected} onChange={toggleSelectAll}
                          className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                      </th>
                      <th className="px-5 py-3.5 font-semibold">Content</th>
                      <th className="px-5 py-3.5 font-semibold">Brand</th>
                      <th className="px-5 py-3.5 font-semibold">Type</th>
                      <th className="px-5 py-3.5 font-semibold">Status</th>
                      <th className="px-5 py-3.5 font-semibold">Tags</th>
                      <th className="px-5 py-3.5 font-semibold">Date</th>
                      <th className="px-5 py-3.5 font-semibold">Platforms</th>
                      <th className="px-5 py-3.5 w-10" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/10">
                    {paginated.map((item, i) => (
                      <tr key={item.id}
                        className={`group hover:bg-surface-container/40 transition-colors duration-150 ${selectedIds.has(item.id) ? "bg-primary/5" : ""}`}
                        style={{ animation: visible ? `slide-up-row 0.4s ease-out ${0.5 + i * 0.04}s forwards` : "none", opacity: 0 }}>
                        <td className="px-3 py-3.5">
                          <input type="checkbox" checked={selectedIds.has(item.id)} onChange={() => toggleSelect(item.id)}
                            className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                        </td>
                        <td className="px-5 py-3.5">
                          <button onClick={() => router.push(`/content/${item.id}`)} className="flex items-center gap-3 text-left">
                            <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${getTypeStyle(item.type)} flex items-center justify-center text-white shrink-0`}>
                              <span className="material-symbols-outlined text-[18px]">{getTypeConfig(item.type).icon}</span>
                            </div>
                            <span className="text-body-sm font-medium text-on-surface group-hover:text-primary transition-colors">{item.title}</span>
                          </button>
                        </td>
                        <td className="px-5 py-3.5 text-body-sm text-on-surface-variant">{item.brandName}</td>
                        <td className="px-5 py-3.5">
                          <span className={`px-2 py-0.5 rounded-md text-label-xs font-semibold ${getTypeConfig(item.type).color}`}>{item.type}</span>
                        </td>
                        <td className="px-5 py-3.5">
                          <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold ${STATUS_STYLES[item.status]}`}>
                            <span className={`w-1.5 h-1.5 rounded-full ${item.status === "Published" ? "bg-emerald-500 animate-pulse" : item.status === "Scheduled" ? "bg-blue-500" : item.status === "Awaiting Approval" ? "bg-amber-500" : "bg-outline"}`} />
                            {item.status}
                          </span>
                        </td>
                        <td className="px-5 py-3.5">
                          <div className="flex items-center gap-1 flex-wrap">
                            {(item.tags ?? []).slice(0, 2).map((t) => (
                              <span key={t} className="px-1.5 py-0.5 rounded-md bg-surface-container text-label-2xs font-semibold text-on-surface-variant">{t}</span>
                            ))}
                            {(item.tags ?? []).length > 2 && <span className="text-label-2xs text-outline font-semibold">+{(item.tags ?? []).length - 2}</span>}
                          </div>
                        </td>
                        <td className="px-5 py-3.5 text-body-sm text-outline">{new Date(item.createdAt).toLocaleDateString()}</td>
                        <td className="px-5 py-3.5">
                          <div className="flex items-center gap-1">
                            {item.platforms.slice(0, 3).map((p) => {
                              const cfg = PLATFORM_CONFIG[p];
                              return (
                                <div key={p} className="w-5 h-5 rounded-md flex items-center justify-center" style={{ backgroundColor: cfg?.color + "20", color: cfg?.color || "#666" }} title={cfg?.label || p}>
                                  <PlatformIcon platform={cfg?.icon || "default"} />
                                </div>
                              );
                            })}
                            {item.platforms.length > 3 && <span className="text-label-2xs text-outline font-semibold ml-0.5">+{item.platforms.length - 3}</span>}
                          </div>
                        </td>
                        <td className="px-5 py-3.5 relative">
                          <button onClick={(e) => { e.stopPropagation(); setOpenMenuId(openMenuId === item.id ? null : item.id); }}
                            className="w-7 h-7 flex items-center justify-center rounded-lg text-outline/30 hover:bg-surface-container hover:text-on-surface transition-all">
                            <span className="material-symbols-outlined text-[16px]">more_vert</span>
                          </button>
                          {openMenuId === item.id && (
                            <TableMenu item={item} onClose={() => setOpenMenuId(null)} onAction={handleCardAction} />
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {/* ─── Pagination ─── */}
            {!loading && filtered.length > PAGE_SIZE && (
              <div className="flex items-center justify-center gap-2 mt-6">
                <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
                  className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all disabled:opacity-30 disabled:cursor-not-allowed active:scale-[0.97]">
                  <span className="material-symbols-outlined text-[16px]">chevron_left</span>
                </button>
                {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => i + 1).map((p) => (
                  <button key={p} onClick={() => setPage(p)}
                    className={`min-w-[36px] h-9 flex items-center justify-center rounded-xl text-label-sm font-semibold transition-all active:scale-[0.97] ${
                      page === p ? "bg-primary text-on-primary shadow-sm" : "border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
                    }`}>{p}</button>
                ))}
                {totalPages > 7 && <span className="text-outline/30 text-label-sm">...</span>}
                {page < totalPages && (
                  <button onClick={() => setPage((p) => p + 1)}
                    className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97]">
                    <span className="material-symbols-outlined text-[16px]">chevron_right</span>
                  </button>
                )}
              </div>
            )}
          </div>

          {/* ─── Right Sidebar ─── */}
          <div className="w-full xl:w-80 shrink-0 space-y-gutter">
            {/* Quota Card */}
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.48s" }}>
              <div className="p-5">
                <div className="flex items-center justify-between mb-4">
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-primary">
                      <span className="material-symbols-outlined text-[18px]">token</span>
                    </div>
                    <h3 className="text-label-md text-on-surface font-semibold">Content Quota</h3>
                  </div>
                  <span className="text-label-sm text-primary font-semibold">--/--</span>
                </div>
                <div className="h-2 bg-outline-variant/10 rounded-full overflow-hidden mb-2">
                  <div className="h-full bg-gradient-to-r from-primary to-primary-container rounded-full transition-all duration-1000" style={{ width: `0%` }} />
                </div>
                <p className="text-label-xs text-on-surface-variant">-- used this month</p>
                <div className="mt-4 flex items-center justify-between text-label-xs">
                  <div className="flex items-center gap-1.5">
                    <span className="w-2 h-2 rounded-full bg-emerald-500" />
                    <span className="text-on-surface-variant">Images</span>
                    <span className="text-on-surface font-semibold ml-1">342</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <span className="w-2 h-2 rounded-full bg-purple-500" />
                    <span className="text-on-surface-variant">Text</span>
                    <span className="text-on-surface font-semibold ml-1">267</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <span className="w-2 h-2 rounded-full bg-rose-500" />
                    <span className="text-on-surface-variant">Video</span>
                    <span className="text-on-surface font-semibold ml-1">175</span>
                  </div>
                </div>
              </div>
              <div className="px-5 pb-5">
                <button className="w-full py-2 rounded-xl bg-gradient-to-r from-primary/10 to-primary/5 border border-primary/20 text-label-sm text-primary font-semibold hover:from-primary/15 hover:to-primary/10 active:scale-[0.97] transition-all flex items-center justify-center gap-1.5">
                  <span className="material-symbols-outlined text-[14px]">add</span>
                  Upgrade Plan
                </button>
              </div>
            </div>

            {/* AI Quick Assistant */}
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ai-glow ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.56s" }}>
              <div className="p-5">
                <div className="flex items-center gap-2 mb-4">
                  <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-purple-500/20 to-purple-600/10 flex items-center justify-center text-purple-500">
                    <span className="material-symbols-outlined text-[18px]">auto_awesome</span>
                  </div>
                  <div>
                    <h3 className="text-label-md text-on-surface font-semibold">AI Quick Assistant</h3>
                    <p className="text-label-xs text-on-surface-variant">Smart tools for your content</p>
                  </div>
                </div>
                <div className="space-y-2">
                  {AI_QUICK_ASSISTANT_ACTIONS.map((action) => (
                    <button key={action.label} onClick={() => addToast(`${action.label} — coming soon`, "construction")}
                      className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-surface-container transition-all group text-left active:scale-[0.98]">
                      <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center text-outline group-hover:text-purple-500 group-hover:bg-purple-50 transition-all">
                        <span className="material-symbols-outlined text-[16px]">{action.icon}</span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-label-sm text-on-surface font-medium group-hover:text-purple-600 transition-colors">{action.label}</p>
                        <p className="text-label-xs text-on-surface-variant">{action.desc}</p>
                      </div>
                      <span className="material-symbols-outlined text-[14px] text-outline/30 group-hover:text-purple-400 transition-colors">arrow_forward</span>
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {/* Recent Activity */}
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.64s" }}>
              <div className="p-5">
                <div className="flex items-center gap-2 mb-4">
                  <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500/20 to-amber-600/10 flex items-center justify-center text-amber-500">
                    <span className="material-symbols-outlined text-[18px]">history</span>
                  </div>
                  <h3 className="text-label-md text-on-surface font-semibold">Recent Activity</h3>
                </div>
                <div className="space-y-0">
                  {[].map((act: any, i: number) => (
                    <div key={i} className="flex items-start gap-3 py-3 border-b border-outline-variant/10 last:border-0">
                      <div className={`w-8 h-8 rounded-lg ${act.bg} flex items-center justify-center ${act.color} shrink-0 mt-0.5`}>
                        <span className="material-symbols-outlined text-[16px]">{act.icon}</span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-body-sm text-on-surface">{act.text}</p>
                        <p className="text-label-xs text-outline mt-0.5">{act.time}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>

      {/* ─── Create Dropdown ─── */}
      {showCreateMenu && createMenuStyle && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setShowCreateMenu(false)} />
          <div className="fixed z-20 dropdown-enter" style={{ top: createMenuStyle.top, right: createMenuStyle.right }}>
            <div className="w-64 bg-surface-container-lowest rounded-xl border border-outline-variant/20 shadow-xl overflow-hidden">
              <button onClick={() => handleCreateAction("Manual Creation")} className="w-full flex items-center gap-3 px-4 py-3 hover:bg-surface-container transition-colors text-left group">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-blue-500/20 to-blue-600/10 flex items-center justify-center text-blue-500 group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-[18px]">edit_note</span>
                </div>
                <div>
                  <p className="text-label-md text-on-surface font-semibold">Manual Creation</p>
                  <p className="text-label-xs text-on-surface-variant">Write and format your content</p>
                </div>
              </button>
              <div className="h-px bg-outline-variant/10 mx-4" />
              <button onClick={() => handleCreateAction("AI Generate")} className="w-full flex items-center gap-3 px-4 py-3 hover:bg-surface-container transition-colors text-left group">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-purple-500/20 to-purple-600/10 flex items-center justify-center text-purple-500 group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-[18px]">auto_awesome</span>
                </div>
                <div>
                  <p className="text-label-md text-on-surface font-semibold">AI Generate</p>
                  <p className="text-label-xs text-on-surface-variant">Let AI create content for you</p>
                </div>
              </button>
              <div className="h-px bg-outline-variant/10 mx-4" />
              <button onClick={() => handleCreateAction("Import Content")} className="w-full flex items-center gap-3 px-4 py-3 hover:bg-surface-container transition-colors text-left group">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-emerald-500/20 to-emerald-600/10 flex items-center justify-center text-emerald-500 group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-[18px]">post_add</span>
                </div>
                <div>
                  <p className="text-label-md text-on-surface font-semibold">Import Content</p>
                  <p className="text-label-xs text-on-surface-variant">Upload from external sources</p>
                </div>
              </button>
            </div>
          </div>
        </>
      )}

      {/* ─── Edit Modal ─── */}
      {editingItem && (
        <ContentFormModal item={editingItem} onClose={() => setEditingItem(null)} onSave={handleEditSave} />
      )}

      {/* ─── Delete Confirmation ─── */}
      {deletingItem && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Content</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{deletingItem.title}</span>? This content will be permanently removed.
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={() => setDeletingItem(null)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
              <button onClick={handleDeleteConfirm} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2">Delete</button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Preview Modal ─── */}
      {previewItem && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150" onClick={() => setPreviewItem(null)}>
          <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant shadow-lg w-full max-w-lg mx-4 animate-in fade-in zoom-in-95 duration-200 overflow-hidden" onClick={(e) => e.stopPropagation()}>
            <div className="relative aspect-video bg-gradient-to-br from-surface-container to-surface-container-high flex items-center justify-center">
              <div className={`w-20 h-20 rounded-2xl bg-gradient-to-br ${getTypeStyle(previewItem.type)} flex items-center justify-center text-white shadow-lg`}>
                <span className="material-symbols-outlined text-4xl">{getTypeConfig(previewItem.type).icon}</span>
              </div>
              <span className={`absolute top-3 right-3 px-2 py-0.5 rounded-md text-label-xs font-semibold text-white ${getTypeBadgeStyle(previewItem.type)}`}>{previewItem.type}</span>
            </div>
            <div className="p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="text-headline-sm text-on-surface font-bold">{previewItem.title}</h3>
                  <p className="text-body-sm text-on-surface-variant mt-0.5">{previewItem.brandName} &middot; {previewItem.productName}</p>
                </div>
                <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold shrink-0 ${STATUS_STYLES[previewItem.status]}`}>
                  <span className={`w-1.5 h-1.5 rounded-full ${previewItem.status === "Published" ? "bg-emerald-500 animate-pulse" : previewItem.status === "Scheduled" ? "bg-blue-500" : previewItem.status === "Awaiting Approval" ? "bg-amber-500" : "bg-outline"}`} />
                  {previewItem.status}
                </span>
              </div>
              <div className="flex items-center gap-2 mb-3">
                {previewItem.platforms.map((p) => {
                  const cfg = PLATFORM_CONFIG[p];
                  return (
                    <div key={p} className="w-7 h-7 rounded-lg flex items-center justify-center text-label-xs font-bold" style={{ backgroundColor: cfg?.color + "20", color: cfg?.color || "#666" }} title={cfg?.label || p}>
                      <PlatformIcon platform={cfg?.icon || "default"} className="w-[12px] h-[12px]" />
                    </div>
                  );
                })}
              </div>
              {(previewItem.tags ?? []).length > 0 && (
                <div className="flex items-center gap-1 mb-4 flex-wrap">
                  {previewItem.tags!.map((t) => (
                    <span key={t} className="px-2 py-0.5 rounded-md bg-surface-container text-label-xs font-semibold text-on-surface-variant">{t}</span>
                  ))}
                </div>
              )}
              <div className="flex items-center justify-between pt-3 border-t border-outline-variant/10">
                <span className="text-label-sm text-outline">Created {new Date(previewItem.createdAt).toLocaleDateString()}</span>
                <button onClick={() => { setPreviewItem(null); router.push(`/content/${previewItem.id}`); }}
                  className="px-4 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-1.5">
                  View Full Details
                  <span className="material-symbols-outlined text-[14px]">arrow_forward</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ─── Toast Notifications ─── */}
      <div className="fixed bottom-6 right-6 z-50 flex flex-col gap-2">
        {toasts.map((t) => (
          <div key={t.id} className="toast-in flex items-center gap-2.5 px-4 py-3 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl text-body-sm text-on-surface min-w-[240px]">
            <span className="material-symbols-outlined text-[18px] text-primary">{t.icon}</span>
            {t.message}
            <button onClick={() => setToasts((prev) => prev.filter((x) => x.id !== t.id))} className="ml-auto text-outline/30 hover:text-on-surface active:scale-[0.97]">
              <span className="material-symbols-outlined text-[14px]">close</span>
            </button>
          </div>
        ))}
      </div>
    </>
  );
}

/* ─── Sub-components ─── */

function FilterChip({ label, color, onRemove }: { label: string; color?: string; onRemove: () => void }) {
  const bg = color === "blue" ? "bg-blue-50 text-blue-600" : color === "purple" ? "bg-purple-50 text-purple-600" : color === "amber" ? "bg-amber-50 text-amber-600" : color === "rose" ? "bg-rose-50 text-rose-600" : color === "indigo" ? "bg-indigo-50 text-indigo-600" : "bg-primary/8 text-primary";
  return (
    <span className={`inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full text-label-xs font-semibold ${bg}`}>
      {label}
      <button onClick={onRemove} className="hover:opacity-60 active:scale-[0.97]"><span className="material-symbols-outlined text-label-xs">close</span></button>
    </span>
  );
}

function TableMenu({ item, onClose, onAction }: { item: ContentItem; onClose: () => void; onAction: (action: string, item: ContentItem) => void }) {
  return (
    <>
      <div className="fixed inset-0 z-10" onClick={onClose} />
      <div className="absolute right-0 top-full mt-1 w-44 bg-surface-container-lowest rounded-xl border border-outline-variant/20 shadow-xl z-20 overflow-hidden dropdown-enter">
        <button onClick={(e) => { e.stopPropagation(); onAction("Preview", item); }} className="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-surface-container transition-colors text-left text-label-sm text-on-surface group">
          <span className="material-symbols-outlined text-[14px] text-outline/50 group-hover:text-primary">visibility</span>
          Quick Preview
        </button>
        <button onClick={(e) => { e.stopPropagation(); onAction("View Details", item); }} className="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-surface-container transition-colors text-left text-label-sm text-on-surface group">
          <span className="material-symbols-outlined text-[14px] text-outline/50 group-hover:text-primary">open_in_new</span>
          View Details
        </button>
        <button onClick={(e) => { e.stopPropagation(); onAction("Edit", item); }} className="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-surface-container transition-colors text-left text-label-sm text-on-surface group">
          <span className="material-symbols-outlined text-[14px] text-outline/50 group-hover:text-primary">edit</span>
          Edit
        </button>
        <button onClick={(e) => { e.stopPropagation(); onAction("Duplicate", item); }} className="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-surface-container transition-colors text-left text-label-sm text-on-surface group">
          <span className="material-symbols-outlined text-[14px] text-outline/50 group-hover:text-primary">content_copy</span>
          Duplicate
        </button>
        <div className="h-px bg-outline-variant/10 mx-3" />
        <button onClick={(e) => { e.stopPropagation(); onAction("delete", item); }} className="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-surface-container transition-colors text-left text-label-sm text-danger-red group">
          <span className="material-symbols-outlined text-[14px]">delete</span>
          Delete
        </button>
      </div>
    </>
  );
}

function ContentCard({ item, index, visible, openMenuId, onToggleMenu, onAction }: {
  item: ContentItem; index: number; visible: boolean; openMenuId: string | null; onToggleMenu: (id: string | null) => void; onAction: (action: string, item: ContentItem) => void;
}) {
  const tc = getTypeConfig(item.type);
  const typeGradient = getTypeStyle(item.type);
  const typeBadgeColor = getTypeBadgeStyle(item.type);

  return (
    <>
      <div className="cursor-pointer" onClick={() => onAction("View Details", item)}>
        <div className="relative aspect-[4/3] bg-gradient-to-br from-surface-container to-surface-container-high overflow-hidden">
          {item.thumbnail ? (
            <img src={item.thumbnail} alt={item.title} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <div className={`w-16 h-16 rounded-2xl bg-gradient-to-br ${typeGradient} flex items-center justify-center text-white shadow-lg`}>
                <span className="material-symbols-outlined text-[28px]">{tc.icon}</span>
              </div>
            </div>
          )}
          <div className="absolute top-3 left-3">
            <span className={`px-2 py-0.5 rounded-md text-label-xs font-semibold text-white ${typeBadgeColor} backdrop-blur-[2px] flex items-center gap-1`}>
              <span className="material-symbols-outlined text-label-xs">{tc.icon}</span>
              {item.type}
            </span>
          </div>
          <div className="absolute top-3 right-3">
            <button onClick={(e) => { e.stopPropagation(); onToggleMenu(openMenuId === item.id ? null : item.id); }}
              className="w-7 h-7 rounded-lg bg-black/30 backdrop-blur-[2px] flex items-center justify-center text-white hover:bg-black/50 transition-colors active:scale-[0.95] opacity-0 group-hover:opacity-100">
              <span className="material-symbols-outlined text-[14px]">more_vert</span>
            </button>
            {openMenuId === item.id && (
              <TableMenu item={item} onClose={() => onToggleMenu(null)} onAction={onAction} />
            )}
          </div>
        </div>
        <div className="p-5">
          <h3 className="text-body-sm font-semibold text-on-surface group-hover:text-primary transition-colors line-clamp-1 mb-1">{item.title}</h3>
          <div className="flex items-center gap-1.5 text-[11px] text-on-surface-variant/60 mb-3">
            <span>{item.brandName}</span>
            <span className="w-1 h-1 rounded-full bg-outline/30" />
            <span>{item.productName}</span>
          </div>
          <div className="flex items-center justify-between mb-3">
            <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold ${STATUS_STYLES[item.status]}`}>
              <span className={`w-1.5 h-1.5 rounded-full ${item.status === "Published" ? "bg-emerald-500 animate-pulse" : item.status === "Scheduled" ? "bg-blue-500" : item.status === "Awaiting Approval" ? "bg-amber-500" : "bg-outline"}`} />
              {item.status}
            </span>
            <span className="text-label-xs text-outline">{new Date(item.createdAt).toLocaleDateString()}</span>
          </div>
          {(item.tags?.length ?? 0) > 0 && (
            <div className="flex items-center gap-1 mb-3 flex-wrap">
              {item.tags!.slice(0, 2).map((t) => (
                <span key={t} className="px-1.5 py-0.5 rounded-md bg-surface-container text-label-2xs font-semibold text-on-surface-variant">{t}</span>
              ))}
              {item.tags!.length > 2 && <span className="text-label-2xs text-outline font-semibold">+{item.tags!.length - 2}</span>}
            </div>
          )}
          <div className="flex items-center gap-1.5 pt-3 border-t border-outline-variant/10">
            {item.platforms.map((p) => {
              const cfg = PLATFORM_CONFIG[p];
              return (
                <div key={p} className="w-6 h-6 rounded-lg flex items-center justify-center text-label-2xs font-bold" style={{ backgroundColor: cfg?.color + "20", color: cfg?.color || "#666" }} title={cfg?.label || p}>
                    <PlatformIcon platform={cfg?.icon || "default"} className="w-[11px] h-[11px]" />
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </>
  );
}

/* ─── Content Form Modal (Create / Edit) ─── */
function ContentFormModal({ item, onClose, onSave }: { item?: ContentItem; onClose: () => void; onSave: (data: ContentItem) => void }) {
  const isEdit = !!item;
  const [brandNames, setBrandNames] = useState<string[]>([]);
  const [form, setForm] = useState({
    title: item?.title || "",
    brandName: item?.brandName || "",
    productName: item?.productName || "",
    type: item?.type || "TEXT" as ContentType,
    status: item?.status || "Draft" as ContentStatus,
    platforms: item?.platforms || [] as string[],
    tags: item?.tags || [] as string[],
    thumbnail: item?.thumbnail || "",
  });
  const [showPlatformPicker, setShowPlatformPicker] = useState(false);
  const [showTagPicker, setShowTagPicker] = useState(false);
  const thumbnailInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    fetchBrands().then(list => setBrandNames(list.map(b => b.name)));
  }, []);

  const availableProducts = useMemo(() => {
    if (!form.brandName) return [];
    return [];
  }, [form.brandName]);
  const isValid = form.title.trim().length > 0 && form.brandName && form.productName;

  const handleThumbnailUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setForm((p) => ({ ...p, thumbnail: URL.createObjectURL(file) }));
  };

  const handleSave = () => {
    if (!isValid) return;
    if (isEdit && item) {
      onSave({ ...item, ...form });
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-lg mx-4 animate-in fade-in zoom-in-95 duration-200 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center gap-3 mb-6">
          <div className={`w-10 h-10 rounded-xl ${isEdit ? "bg-primary/10" : "bg-emerald-500/10"} flex items-center justify-center`}>
            <span className={`${isEdit ? "text-primary" : "text-emerald-500"} text-[22px] material-symbols-outlined`}>
              {isEdit ? "edit" : "add_circle"}
            </span>
          </div>
          <div>
            <h3 className="text-headline-sm text-on-surface font-semibold">{isEdit ? "Edit Content" : "Create Content"}</h3>
            <p className="text-body-sm text-on-surface-variant">{isEdit ? "Update content details" : "Fill in the details to create new content"}</p>
          </div>
        </div>

        <div className="space-y-4">
          {/* Title */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Title <span className="text-danger-red">*</span></label>
            <input value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all"
              placeholder="Enter content title" />
          </div>

          {/* Brand */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Brand <span className="text-danger-red">*</span></label>
            <select value={form.brandName} onChange={(e) => setForm((p) => ({ ...p, brandName: e.target.value, productName: "" }))}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
              <option value="">Select brand</option>
              {brandNames.map((b) => <option key={b} value={b}>{b}</option>)}
            </select>
          </div>

          {/* Product */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Product <span className="text-danger-red">*</span></label>
            <input value={form.productName} onChange={(e) => setForm((p) => ({ ...p, productName: e.target.value }))}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all"
              placeholder="Enter product name" />
          </div>

          {/* Type */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Content Type</label>
            <div className="grid grid-cols-3 gap-2">
              {CONTENT_TYPES.map((t) => (
                <button key={t.value} type="button" onClick={() => setForm((p) => ({ ...p, type: t.value }))}
                  className={`flex flex-col items-center gap-1.5 p-3 rounded-xl border transition-all ${
                    form.type === t.value
                      ? "border-primary bg-primary/5 text-primary"
                      : "border-outline-variant/20 bg-surface-container text-on-surface-variant hover:border-primary/30"
                  }`}>
                  <span className="material-symbols-outlined text-[20px]">{t.icon}</span>
                  <span className="text-label-xs font-semibold">{t.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Status */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Status</label>
            <select value={form.status} onChange={(e) => setForm((p) => ({ ...p, status: e.target.value as ContentStatus }))}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
              {STATUS_OPTIONS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
            </select>
          </div>

          {/* Platforms */}
          <div className="relative">
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Platforms</label>
            <button type="button" onClick={() => setShowPlatformPicker(!showPlatformPicker)}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-left text-on-surface hover:border-primary/40 transition-all flex items-center justify-between">
              <span>{form.platforms.length === 0 ? "Select platforms" : `${form.platforms.length} selected`}</span>
              <span className="material-symbols-outlined text-[14px] text-outline">expand_more</span>
            </button>
            {showPlatformPicker && (
              <>
                <div className="fixed inset-0 z-10" onClick={() => setShowPlatformPicker(false)} />
                <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 space-y-0.5 dropdown-enter">
                  {ALL_PLATFORMS.map((p) => (
                    <label key={p} className="flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container cursor-pointer transition-colors">
                      <input type="checkbox" checked={form.platforms.includes(p)} onChange={() => {
                        setForm((prev) => ({
                          ...prev,
                          platforms: prev.platforms.includes(p) ? prev.platforms.filter((x) => x !== p) : [...prev.platforms, p],
                        }));
                      }} className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                      <span className="text-label-sm text-on-surface capitalize">{p}</span>
                    </label>
                  ))}
                </div>
              </>
            )}
            {form.platforms.length > 0 && (
              <div className="flex items-center gap-1.5 mt-2 flex-wrap">
                {form.platforms.map((p) => (
                  <span key={p} className="px-2 py-0.5 rounded-lg flex items-center gap-1 text-label-xs font-semibold" style={{ backgroundColor: (PLATFORM_CONFIG[p]?.color || "#666") + "20", color: PLATFORM_CONFIG[p]?.color || "#666" }}>
                    <PlatformIcon platform={PLATFORM_CONFIG[p]?.icon || "default"} className="w-[10px] h-[10px]" />
                    {p}
                    <button onClick={() => setForm((prev) => ({ ...prev, platforms: prev.platforms.filter((x) => x !== p) }))} className="hover:opacity-60">
                      <span className="material-symbols-outlined text-label-xs">close</span>
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          {/* Tags */}
          <div className="relative">
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Tags</label>
            <button type="button" onClick={() => setShowTagPicker(!showTagPicker)}
              className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-left text-on-surface hover:border-primary/40 transition-all flex items-center justify-between">
              <span>{form.tags.length === 0 ? "Select tags" : `${form.tags.length} selected`}</span>
              <span className="material-symbols-outlined text-[14px] text-outline">expand_more</span>
            </button>
            {showTagPicker && (
              <>
                <div className="fixed inset-0 z-10" onClick={() => setShowTagPicker(false)} />
                <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 space-y-0.5 dropdown-enter">
                  {ALL_TAGS.map((t) => (
                    <label key={t} className="flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container cursor-pointer transition-colors">
                      <input type="checkbox" checked={form.tags.includes(t)} onChange={() => {
                        setForm((prev) => ({
                          ...prev,
                          tags: prev.tags.includes(t) ? prev.tags.filter((x) => x !== t) : [...prev.tags, t],
                        }));
                      }} className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                      <span className="text-label-sm text-on-surface">{t}</span>
                    </label>
                  ))}
                </div>
              </>
            )}
            {form.tags.length > 0 && (
              <div className="flex items-center gap-1.5 mt-2 flex-wrap">
                {form.tags.map((t) => (
                  <span key={t} className="px-2 py-0.5 rounded-md bg-surface-container text-label-xs font-semibold text-on-surface-variant flex items-center gap-1">
                    {t}
                    <button onClick={() => setForm((prev) => ({ ...prev, tags: prev.tags.filter((x) => x !== t) }))} className="hover:opacity-60">
                      <span className="material-symbols-outlined text-label-xs">close</span>
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          {/* Thumbnail */}
          <div>
            <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Thumbnail</label>
            <input ref={thumbnailInputRef} type="file" accept="image/*" className="hidden" onChange={handleThumbnailUpload} />
            <div onClick={() => thumbnailInputRef.current?.click()}
              className="border-2 border-dashed border-outline-variant/30 hover:border-primary/40 hover:bg-surface-container/50 rounded-xl p-4 text-center cursor-pointer transition-all">
              {form.thumbnail ? (
                <div className="relative inline-block">
                  <div className="w-36 h-20 rounded-lg overflow-hidden">
                    <img src={form.thumbnail} alt="" className="w-full h-full object-cover" />
                  </div>
                  <button onClick={(e) => { e.stopPropagation(); setForm((p) => ({ ...p, thumbnail: "" })); }}
                    className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-black/50 text-white flex items-center justify-center hover:bg-danger-red/80 transition-all">
                    <span className="material-symbols-outlined text-label-xs">close</span>
                  </button>
                </div>
              ) : (
                <div className="flex items-center gap-2 justify-center text-label-sm text-outline/60">
                  <span className="material-symbols-outlined text-[16px]">image</span>
                  Upload thumbnail image
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-outline-variant/10">
          <button onClick={onClose} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
          <button onClick={handleSave} disabled={!isValid}
            className="px-5 py-2 rounded-xl bg-primary text-on-primary text-label-md font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed">
            <span className="material-symbols-outlined text-[16px]">{isEdit ? "check" : "add"}</span>
            {isEdit ? "Save Changes" : "Create Content"}
          </button>
        </div>
      </div>
    </div>
  );
}
