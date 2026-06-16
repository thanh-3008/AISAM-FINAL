"use client";

import { useMemo } from "react";
import { PostItem } from "@/services/postService";
import { sortPosts } from "@/lib/postUtils";
import PostRow from "./PostRow";

interface PostTableProps {
  posts: PostItem[];
  loading: boolean;
  selectedIds: string[];
  onSelectAll: (selected: boolean) => void;
  onSelect: (id: string, selected: boolean) => void;
  onView: (post: PostItem) => void;
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  sortKey: "publishedAt" | "contentTitle" | "brandName" | "status";
  sortDir: "asc" | "desc";
  onSort: (key: "publishedAt" | "contentTitle" | "brandName" | "status") => void;
}

type SortKey = PostTableProps["sortKey"];

function renderSortIcon(activeKey: SortKey, direction: "asc" | "desc", key: SortKey) {
  if (activeKey !== key) {
    return <span className="material-symbols-outlined text-[12px] text-outline/20 ml-0.5">unfold_more</span>;
  }

  return (
    <span className="material-symbols-outlined text-[12px] text-primary ml-0.5">
      {direction === "asc" ? "expand_less" : "expand_more"}
    </span>
  );
}

export default function PostTable({
  posts,
  loading,
  selectedIds,
  onSelectAll,
  onSelect,
  onView,
  page,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  sortKey,
  sortDir,
  onSort
}: PostTableProps) {
  const allSelected = posts.length > 0 && selectedIds.length === posts.length;
  const someSelected = selectedIds.length > 0 && selectedIds.length < posts.length;

  const sortedPosts = useMemo(() => sortPosts(posts, sortKey, sortDir), [posts, sortKey, sortDir]);

  const renderPaginationNumbers = () => {
    const pages = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      let start = Math.max(1, page - 2);
      const end = Math.min(totalPages, start + maxVisible - 1);

      if (end - start + 1 < maxVisible) {
        start = Math.max(1, end - maxVisible + 1);
      }

      if (start > 1) {
        pages.push(1);
        if (start > 2) pages.push("...");
      }

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (end < totalPages) {
        if (end < totalPages - 1) pages.push("...");
        pages.push(totalPages);
      }
    }

    return pages;
  };

  return (
    <div className="space-y-4">
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 overflow-hidden shadow-sm">
        {loading ? (
          <div className="flex items-center justify-center py-32">
            <span className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
          </div>
        ) : sortedPosts.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <span className="material-symbols-outlined text-4xl text-outline/20 mb-3">post_add</span>
            <p className="text-body-sm text-outline font-medium">No posts found</p>
            <p className="text-[11px] text-outline/60 mt-1">Try adjusting your filters</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-surface-container-low border-b border-outline-variant/30">
                    <th className="px-6 py-4">
                      <input
                        type="checkbox"
                        checked={allSelected}
                        ref={input => {
                          if (input) {
                            input.indeterminate = someSelected;
                          }
                        }}
                        onChange={() => onSelectAll(!allSelected)}
                        className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20"
                      />
                    </th>
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold cursor-pointer select-none hover:text-on-surface"
                      onClick={() => onSort("contentTitle")}>
                      Post
                      {renderSortIcon(sortKey, sortDir, "contentTitle")}
                    </th>
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold">
                      Platform & Brand
                    </th>
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold cursor-pointer select-none hover:text-on-surface"
                      onClick={() => onSort("status")}>
                      Status
                      {renderSortIcon(sortKey, sortDir, "status")}
                    </th>
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold cursor-pointer select-none hover:text-on-surface"
                      onClick={() => onSort("publishedAt")}>
                      Date
                      {renderSortIcon(sortKey, sortDir, "publishedAt")}
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-outline-variant/20">
                  {sortedPosts.map((post) => (
                    <PostRow
                      key={post.id}
                      post={post}
                      isSelected={selectedIds.includes(post.id)}
                      onSelect={onSelect}
                      onView={onView}
                    />
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="bg-surface-container-low px-6 py-4 border-t border-outline-variant/20 flex items-center justify-between">
              <p className="text-body-sm text-outline">
                Showing <span className="font-bold text-on-surface">{Math.min((page - 1) * pageSize + 1, totalCount)} – {Math.min(page * pageSize, totalCount)}</span> of {totalCount} posts
              </p>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => onPageChange(page - 1)}
                  disabled={page === 1}
                  className="p-2 border border-outline-variant/30 rounded-lg hover:bg-surface-container-lowest transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
                >
                  <span className="material-symbols-outlined text-[16px]">chevron_left</span>
                </button>

                <div className="flex items-center gap-1">
                  {renderPaginationNumbers().map((pageNum, idx) => (
                    pageNum === "..." ? (
                      <span key={`ellipsis-${idx}`} className="w-8 h-8 flex items-center justify-center text-outline/40">...</span>
                    ) : (
                      <button
                        key={pageNum}
                        onClick={() => onPageChange(pageNum as number)}
                        className={`w-8 h-8 rounded-lg text-label-sm font-semibold transition-all ${
                          page === pageNum ? "bg-primary text-on-primary" : "text-on-surface-variant hover:bg-surface-container-lowest"
                        }`}
                      >
                        {pageNum}
                      </button>
                    )
                  ))}
                </div>

                <button
                  onClick={() => onPageChange(page + 1)}
                  disabled={page === totalPages}
                  className="p-2 border border-outline-variant/30 rounded-lg hover:bg-surface-container-lowest transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
                >
                  <span className="material-symbols-outlined text-[16px]">chevron_right</span>
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}