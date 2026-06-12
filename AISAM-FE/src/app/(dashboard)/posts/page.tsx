"use client";

import { useState, useEffect, useCallback } from "react";
import Header from "@/components/layout/Header";
import StatsCards from "@/components/posts/StatsCards";
import Filters from "@/components/posts/Filters";
import PostTable from "@/components/posts/PostTable";
import PostDetailModal from "@/components/posts/PostDetailModal";
import { fetchPosts, createPost, updatePost, deletePost, retryPost, type PostItem, type PostStatus, type PostPlatform } from "@/services/postService";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchPostQuota } from "@/services/workspaceService";

const PAGE_SIZE = 10;

type SortKey = "publishedAt" | "contentTitle" | "brandName" | "status" | "likes" | "comments" | "shares";
type SortDir = "asc" | "desc";

const DEFAULT_FILTERS = {
  search: "",
  brand: "",
  platform: "",
  status: "",
  type: "",
  dateFrom: "",
  dateTo: "",
  minLikes: 0,
  minComments: 0,
  minShares: 0,
};

export default function PostsPage() {
  const { activeWorkspace } = useWorkspaces();
  const [posts, setPosts] = useState<PostItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [postQuota, setPostQuota] = useState<{ used: number; total: number } | null>(null);
  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [sortKey, setSortKey] = useState<SortKey>("publishedAt");
  const [sortDir, setSortDir] = useState<SortDir>("desc");
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Modal states
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createPlatform, setCreatePlatform] = useState<PostPlatform>("facebook");
  const [createBrand, setCreateBrand] = useState("");
  const [createCaption, setCreateCaption] = useState("");
  const [createSchedule, setCreateSchedule] = useState("now");
  const [createDate, setCreateDate] = useState("");
  const [createTime, setCreateTime] = useState("");
  const [toast, setToast] = useState<{ msg: string; type: "success" | "error" } | null>(null);
  const [editPost, setEditPost] = useState<PostItem | null>(null);
  const [editDate, setEditDate] = useState("");
  const [editTime, setEditTime] = useState("");
  const [deleting, setDeleting] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);
  const [detailPost, setDetailPost] = useState<PostItem | null>(null);

  // Fetch posts with pagination and filters
  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const res = await fetchPosts({
          page,
          pageSize: PAGE_SIZE,
          status: (filters.status as PostStatus) || undefined,
          platform: (filters.platform as PostPlatform) || undefined,
          search: filters.search || undefined,
          dateFrom: filters.dateFrom || undefined,
          dateTo: filters.dateTo || undefined,
          minLikes: filters.minLikes || undefined,
          minComments: filters.minComments || undefined,
          minShares: filters.minShares || undefined,
        });
        if (!cancelled) {
          setPosts(res.data);
          setTotalCount(res.totalCount);
          setTotalPages(res.totalPages);
        }
      } catch {
        if (!cancelled) {
          setPosts([]);
          setTotalCount(0);
          setTotalPages(0);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [page, filters, refreshKey]);

  // Fetch all posts for stats
  const [allPostsForStats, setAllPostsForStats] = useState<PostItem[]>([]);
  useEffect(() => {
    const loadAll = async () => {
      const res = await fetchPosts({ pageSize: 1000 });
      setAllPostsForStats(res.data);
    };
    loadAll();
    fetchPostQuota().then(q => { if (q) setPostQuota(q); });
  }, [activeWorkspace?.id]);

  // Filter handlers
  const handleFilterChange = useCallback((partial: Partial<typeof filters>) => {
    setFilters(prev => ({ ...prev, ...partial }));
    setPage(1);
  }, []);

  const handleClearFilters = useCallback(() => {
    setFilters(DEFAULT_FILTERS);
    setPage(1);
  }, []);

  // Sort handler
  const handleSort = useCallback((k: SortKey) => {
    if (sortKey === k) {
      setSortDir(d => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortKey(k);
      setSortDir("asc");
    }
  }, [sortKey]);

  // Selection handlers
  const handleSelect = useCallback((id: string, selected: boolean) => {
    setSelectedIds(prev =>
      selected ? [...prev, id] : prev.filter(x => x !== id)
    );
  }, []);

  const handleSelectAll = useCallback((selected: boolean) => {
    setSelectedIds(selected ? posts.map(p => p.id) : []);
  }, [posts]);

  // Toast
  const showToast = useCallback((msg: string, type: "success" | "error" = "success") => {
    setToast({ msg, type });
    setTimeout(() => setToast(null), 3000);
  }, []);

  // Create
  const handleCreate = async () => {
    if (!createCaption.trim()) { showToast("Please enter a caption", "error"); return; }
    if (createSchedule === "later" && (!createDate || !createTime)) { showToast("Please select date and time", "error"); return; }
    if (postQuota && postQuota.used >= postQuota.total) {
      showToast("Post quota exhausted. Please upgrade your plan to publish more posts.", "error");
      return;
    }
    const publishedAt = createSchedule === "now" ? new Date().toISOString() : new Date(`${createDate}T${createTime}`).toISOString();
    const status = createSchedule === "now" ? "Published" : "Scheduled";
    const target = createBrand || "Lumina Tech";
    await createPost({
      contentTitle: createCaption.slice(0, 50),
      brandName: target,
      platform: createPlatform,
      type: "IMAGE",
      caption: createCaption,
      publishedAt,
      status,
    });
    setShowCreateModal(false);
    setCreateCaption("");
    setCreateDate("");
    setCreateTime("");
    setCreateBrand("");
    setCreateSchedule("now");
    showToast(`Post ${status === "Published" ? "published" : "scheduled"} successfully`);
    setRefreshKey(k => k + 1);
  };

  // Edit
  const handleEdit = useCallback((post: PostItem) => {
    setEditPost(post);
    setEditDate(new Date(post.publishedAt).toISOString().slice(0, 10));
    setEditTime(new Date(post.publishedAt).toISOString().slice(11, 16));
  }, []);

  const handleSaveEdit = async () => {
    if (!editPost || !editDate || !editTime) return;
    const publishedAt = new Date(`${editDate}T${editTime}`).toISOString();
    await updatePost(editPost.id, { publishedAt });
    setEditPost(null);
    showToast("Schedule updated");
    setRefreshKey(k => k + 1);
  };

  // Retry
  const handleRetry = useCallback(async (id: string) => {
    const updated = await retryPost(id);
    if (!updated) { showToast("Failed to retry post", "error"); return; }
    showToast("Post retried and published");
    setRefreshKey(k => k + 1);
  }, [showToast]);

  // Delete
  const handleDelete = useCallback(async (id: string) => {
    setDeleting(id);
    await deletePost(id);
    setDeleting(null);
    setSelectedIds(prev => prev.filter(x => x !== id));
    showToast("Post deleted");
    setRefreshKey(k => k + 1);
  }, [showToast]);

  // Bulk operations
  const handleBulkRetry = useCallback(async (ids: string[]) => {
    for (const id of ids) {
      await retryPost(id);
    }
    setSelectedIds([]);
    showToast(`${ids.length} post(s) retried`);
    setRefreshKey(k => k + 1);
  }, [showToast]);

  const handleBulkDelete = useCallback(async (ids: string[]) => {
    setDeleting("bulk");
    for (const id of ids) {
      await deletePost(id);
    }
    setDeleting(null);
    setSelectedIds([]);
    showToast(`${ids.length} post(s) deleted`);
    setRefreshKey(k => k + 1);
  }, [showToast]);

  const handleAnalytics = useCallback((post: PostItem) => {
    setDetailPost(post);
  }, []);

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Posts" },
      ]} />
      <style>{`
        @keyframes float { 0%,100% { transform: translateY(0); } 50% { transform: translateY(-4px); } }
        .animate-float { animation: float 3s ease-in-out infinite; }
        @keyframes slide-up { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
        .animate-slide-up { animation: slide-up 0.4s ease-out forwards; }
      `}</style>
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* ── Header ── */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div className="flex items-center gap-4">
              <span className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary/10 to-secondary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">post_add</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Social Posts</h1>
                <p className="text-[11px] text-outline">{totalCount} total · {allPostsForStats.filter(p => p.status === "Published").length} live</p>
                {postQuota && (
                  <p className="text-[11px] text-outline/60 mt-0.5">
                    Post Quota: {postQuota.used} / {postQuota.total} used
                  </p>
                )}
              </div>
            </div>
            <div className="flex items-center gap-3">
              <button onClick={() => setShowCreateModal(true)}
                className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-label-sm font-bold flex items-center gap-1.5 shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95">
                <span className="material-symbols-outlined text-[16px]">add_circle</span>
                Create New Post
              </button>
            </div>
          </div>

          {/* ── Stats ── */}
          <StatsCards posts={allPostsForStats} />

          {/* ── Filters ── */}
          <Filters
            posts={allPostsForStats}
            filters={filters}
            onFilterChange={handleFilterChange}
            onClearFilters={handleClearFilters}
          />

          {/* ── Table ── */}
          <PostTable
            posts={posts}
            loading={loading}
            selectedIds={selectedIds}
            onSelectAll={handleSelectAll}
            onSelect={handleSelect}
            onEdit={handleEdit}
            onRetry={handleRetry}
            onDelete={handleDelete}
            onAnalytics={handleAnalytics}
            onBulkDelete={handleBulkDelete}
            onBulkRetry={handleBulkRetry}
            deleting={deleting}
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={PAGE_SIZE}
            onPageChange={setPage}
            sortKey={sortKey}
            sortDir={sortDir}
            onSort={handleSort}
          />
        </div>

        {/* ── Create Post Modal ── */}
        {showCreateModal && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => setShowCreateModal(false)} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => setShowCreateModal(false)}>
              <div className="w-full max-w-4xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
                <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
                  <h2 className="text-headline-sm font-bold text-on-surface">Create New Post</h2>
                  <button onClick={() => setShowCreateModal(false)} className="p-2 hover:bg-surface-container rounded-full transition-colors">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                </div>
                <div className="p-6 grid grid-cols-1 lg:grid-cols-2 gap-8">
                  {/* Left: Form */}
                  <div className="space-y-6">
                    <div className="space-y-2">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block">Select Platform</label>
                      <div className="flex gap-3">
                        {[
                          { key: "facebook", label: "Facebook", bg: "#1877F2", icon: <span className="text-white text-[11px] font-bold">f</span> },
                          { key: "instagram", label: "Instagram", bg: "linear-gradient(135deg, #F58529, #DD2A7B, #8134AF)", icon: <span className="material-symbols-outlined text-white text-[16px]">photo_camera</span> },
                          { key: "tiktok", label: "TikTok", bg: "#111111", icon: <span className="material-symbols-outlined text-white text-[16px]">music_note</span> },
                        ].map((p) => (
                          <button key={p.key} onClick={() => setCreatePlatform(p.key as PostPlatform)}
                            className={`flex-1 p-3 rounded-xl border-2 flex flex-col items-center gap-2 transition-all ${
                              createPlatform === p.key ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-primary/50"
                            }`}>
                            <div className="w-8 h-8 rounded flex items-center justify-center" style={{ background: p.bg }}>{p.icon}</div>
                            <span className="text-label-xs font-semibold text-on-surface">{p.label}</span>
                          </button>
                        ))}
                      </div>
                    </div>
                    <div className="space-y-2">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block">Brand</label>
                      <select value={createBrand} onChange={(e) => setCreateBrand(e.target.value)}
                        className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10">
                        <option value="">Select brand...</option>
                        {["Lumina Tech", "Summit Outdoor", "Heritage Motors", "GreenLeaf Organics", "Pulse Finance"].map((b) => <option key={b} value={b}>{b}</option>)}
                      </select>
                    </div>
                    <div className="space-y-2">
                      <div className="flex items-center justify-between">
                        <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Post Caption</label>
                        <button className="flex items-center gap-1 text-primary text-label-sm font-semibold hover:bg-primary/5 px-2 py-1 rounded-lg transition-colors">
                          <span className="material-symbols-outlined text-[14px]">auto_awesome</span>
                          AI Refine
                        </button>
                      </div>
                      <textarea value={createCaption} onChange={(e) => setCreateCaption(e.target.value)}
                        placeholder="What's on your mind?"
                        className="w-full h-32 p-4 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 outline-none transition-all placeholder:text-outline/40 resize-none" />
                    </div>
                    <div className="space-y-2">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Media</label>
                      <div className="border-2 border-dashed border-outline-variant/30 rounded-xl p-8 flex flex-col items-center justify-center gap-2 bg-surface-container-lowest hover:bg-surface-container-low transition-colors cursor-pointer">
                        <span className="material-symbols-outlined text-3xl text-outline/40">cloud_upload</span>
                        <p className="text-label-sm text-on-surface font-semibold">Click to upload or drag and drop</p>
                        <p className="text-label-xs text-outline">PNG, JPG or MP4 (max. 50MB)</p>
                      </div>
                    </div>
                    <div className="space-y-4">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Scheduling</label>
                      <div className="flex gap-4">
                        <label className={`flex-1 flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                          createSchedule === "now" ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-outline-variant/40"
                        }`}>
                          <input type="radio" name="schedule" checked={createSchedule === "now"} onChange={() => setCreateSchedule("now")} className="text-primary focus:ring-primary" />
                          <span className="text-label-sm font-semibold text-on-surface">Post Now</span>
                        </label>
                        <label className={`flex-1 flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                          createSchedule === "later" ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-outline-variant/40"
                        }`}>
                          <input type="radio" name="schedule" checked={createSchedule === "later"} onChange={() => setCreateSchedule("later")} className="text-primary focus:ring-primary" />
                          <span className="text-label-sm font-semibold text-on-surface">Schedule for Later</span>
                        </label>
                      </div>
                      {createSchedule === "later" && (
                        <div className="flex gap-4">
                          <input type="date" value={createDate} onChange={(e) => setCreateDate(e.target.value)}
                            className="flex-1 p-3 bg-surface-container-low border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10" />
                          <input type="time" value={createTime} onChange={(e) => setCreateTime(e.target.value)}
                            className="flex-1 p-3 bg-surface-container-low border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10" />
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Right: Mobile Preview */}
                  <div className="bg-surface-container-low rounded-2xl p-6 flex flex-col gap-4">
                    <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Mobile Preview</label>
                    <div className="mx-auto w-[280px] h-[540px] bg-black rounded-[3rem] border-[6px] border-inverse-surface relative overflow-hidden shadow-xl">
                      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-24 h-5 bg-inverse-surface rounded-b-2xl z-10" />
                      <div className="bg-white h-full w-full mt-5 overflow-y-auto">
                        <div className="p-3 flex items-center gap-2 border-b border-outline-variant/20">
                          <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center">
                            <span className="material-symbols-outlined text-[12px]">auto_awesome</span>
                          </div>
                          <div className="flex-1">
                            <div className="h-2.5 w-20 bg-surface-container-high rounded-full" />
                            <div className="h-2 w-12 bg-surface-container rounded-full mt-1" />
                          </div>
                          <span className="material-symbols-outlined text-outline/40 text-[16px]">more_vert</span>
                        </div>
                        <div className="aspect-square bg-gradient-to-br from-surface-container to-surface-container-high flex items-center justify-center">
                          <span className="material-symbols-outlined text-3xl text-outline/20">image</span>
                        </div>
                        <div className="p-3 space-y-2">
                          <div className="flex gap-3">
                            <span className="material-symbols-outlined text-[18px] text-on-surface">favorite</span>
                            <span className="material-symbols-outlined text-[18px] text-on-surface">chat_bubble</span>
                            <span className="material-symbols-outlined text-[18px] text-on-surface">send</span>
                          </div>
                          <p className="text-label-2xs text-on-surface-variant line-clamp-3">{createCaption || "Your caption will appear here..."}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
                  <button onClick={() => setShowCreateModal(false)}
                    className="px-6 py-3 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">Save as Draft</button>
                  <button onClick={handleCreate} className="px-8 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95">
                    {createSchedule === "now" ? "Post Now" : "Schedule Post"}
                  </button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* ── Edit Schedule Modal ── */}
        {editPost && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => setEditPost(null)} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => setEditPost(null)}>
              <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
                <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between">
                  <h2 className="text-headline-sm font-bold text-on-surface">Edit Schedule</h2>
                  <button onClick={() => setEditPost(null)} className="p-2 hover:bg-surface-container rounded-full transition-colors">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                </div>
                <div className="p-6 space-y-4">
                  <p className="text-body-sm text-on-surface font-semibold">{editPost.contentTitle}</p>
                  <div className="flex gap-4">
                    <div className="flex-1 space-y-1">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Date</label>
                      <input type="date" value={editDate} onChange={(e) => setEditDate(e.target.value)}
                        className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10" />
                    </div>
                    <div className="flex-1 space-y-1">
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest">Time</label>
                      <input type="time" value={editTime} onChange={(e) => setEditTime(e.target.value)}
                        className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10" />
                    </div>
                  </div>
                </div>
                <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3">
                  <button onClick={() => setEditPost(null)} className="px-6 py-3 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">Cancel</button>
                  <button onClick={handleSaveEdit} className="px-8 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95">Save</button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* ── Post Detail Modal ── */}
        {detailPost && (
          <PostDetailModal post={detailPost} onClose={() => setDetailPost(null)} />
        )}

        {/* ── Toast ── */}
        {toast && (
          <div className={`fixed bottom-6 right-6 z-[100] flex items-center gap-3 px-5 py-3 rounded-xl shadow-2xl ${
            toast.type === "success" ? "bg-emerald-600 text-white" : "bg-danger-red text-white"
          }`}>
            <span className="material-symbols-outlined text-[18px]">{toast.type === "success" ? "check_circle" : "error"}</span>
            <p className="text-label-sm font-bold">{toast.msg}</p>
            <button onClick={() => setToast(null)} className="ml-2 p-0.5 hover:bg-white/20 rounded-full transition-colors">
              <span className="material-symbols-outlined text-[14px]">close</span>
            </button>
          </div>
        )}
      </main>
    </>
  );
}
