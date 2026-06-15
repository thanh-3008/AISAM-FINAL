"use client";

import { useState, useEffect, useCallback } from "react";
import Header from "@/components/layout/Header";
import StatsCards from "@/components/posts/StatsCards";
import Filters from "@/components/posts/Filters";
import PostTable from "@/components/posts/PostTable";
import PostDetailModal from "@/components/posts/PostDetailModal";
import { fetchPosts, type PostItem, type PostStatus } from "@/services/postService";
import { fetchPostQuota } from "@/services/workspaceService";

const PAGE_SIZE = 10;

type SortKey = "publishedAt" | "contentTitle" | "brandName" | "status";
type SortDir = "asc" | "desc";

const DEFAULT_FILTERS = {
  search: "",
  brand: "",
  status: "",
};

export default function PostsPage() {
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
  const [refreshKey, setRefreshKey] = useState(0);
  const [detailPost, setDetailPost] = useState<PostItem | null>(null);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const brandFilter = filters.brand || undefined;
        const statusFilter = filters.status ? (filters.status as PostStatus) : undefined;

        const res = await fetchPosts({
          page,
          pageSize: PAGE_SIZE,
          brandId: brandFilter,
          status: statusFilter,
        });
        if (!cancelled) {
          let data = res.data;
          if (filters.search) {
            const q = filters.search.toLowerCase();
            data = data.filter((p) =>
              (p.contentTitle || "").toLowerCase().includes(q) ||
              (p.brandName || "").toLowerCase().includes(q) ||
              (p.caption || "").toLowerCase().includes(q)
            );
          }
          setPosts(data);
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

  useEffect(() => {
    fetchPostQuota().then(q => { if (q) setPostQuota(q); });
  }, []);

  const handleFilterChange = useCallback((partial: Partial<typeof filters>) => {
    setFilters(prev => ({ ...prev, ...partial }));
    setPage(1);
  }, []);

  const handleClearFilters = useCallback(() => {
    setFilters(DEFAULT_FILTERS);
    setPage(1);
  }, []);

  const handleSort = useCallback((k: SortKey) => {
    if (sortKey === k) {
      setSortDir(d => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortKey(k);
      setSortDir("asc");
    }
  }, [sortKey]);

  const handleSelect = useCallback((id: string, selected: boolean) => {
    setSelectedIds(prev =>
      selected ? [...prev, id] : prev.filter(x => x !== id)
    );
  }, []);

  const handleSelectAll = useCallback((selected: boolean) => {
    setSelectedIds(selected ? posts.map(p => p.id) : []);
  }, [posts]);

  const handleView = useCallback((post: PostItem) => {
    setDetailPost(post);
  }, []);

  const quotaPercent = postQuota && postQuota.total > 0
    ? Math.round((postQuota.used / postQuota.total) * 100)
    : 0;

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Posts" },
      ]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div className="flex items-center gap-4">
              <span className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-secondary/10 text-primary flex items-center justify-center shadow-sm">
                <span className="material-symbols-outlined text-[22px]">post_add</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Posts</h1>
                <p className="text-label-sm text-outline">
                  {totalCount} post{totalCount !== 1 ? "s" : ""} published
                </p>
              </div>
            </div>
          </div>

          {/* Stats & Quota */}
          <StatsCards
            publishedCount={posts.filter(p => p.status === "Published").length}
            totalCount={totalCount}
            quotaUsed={postQuota?.used ?? null}
            quotaTotal={postQuota?.total ?? null}
          />

          {/* Filters */}
          <Filters
            posts={posts}
            filters={filters}
            onFilterChange={handleFilterChange}
            onClearFilters={handleClearFilters}
          />

          {/* Table */}
          <PostTable
            posts={posts}
            loading={loading}
            selectedIds={selectedIds}
            onSelectAll={handleSelectAll}
            onSelect={handleSelect}
            onView={handleView}
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

        {/* Post Detail Modal */}
        {detailPost && (
          <PostDetailModal post={detailPost} onClose={() => setDetailPost(null)} />
        )}
      </main>
    </>
  );
}