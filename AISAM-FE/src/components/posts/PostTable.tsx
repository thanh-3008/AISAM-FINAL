"use client";

import { useState, useMemo } from "react";
import { PostItem } from "@/services/postService";
import { sortPosts, exportToCSV } from "@/lib/postUtils";
import PostRow from "./PostRow";

interface PostTableProps {
  posts: PostItem[];
  loading: boolean;
  selectedIds: string[];
  onSelectAll: (selected: boolean) => void;
  onSelect: (id: string, selected: boolean) => void;
  onEdit: (post: PostItem) => void;
  onRetry: (id: string) => void;
  onDelete: (id: string) => void;
  onAnalytics: (post: PostItem) => void;
  onBulkDelete: (ids: string[]) => void;
  onBulkRetry: (ids: string[]) => void;
  deleting: string | null;
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  sortKey: "publishedAt" | "contentTitle" | "brandName" | "status" | "likes" | "comments" | "shares";
  sortDir: "asc" | "desc";
  onSort: (key: "publishedAt" | "contentTitle" | "brandName" | "status" | "likes" | "comments" | "shares") => void;
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
  onEdit,
  onRetry,
  onDelete,
  onAnalytics,
  onBulkDelete,
  onBulkRetry,
  deleting,
  page,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  sortKey,
  sortDir,
  onSort
}: PostTableProps) {
  const [showBulkActions, setShowBulkActions] = useState(false);
  
  const allSelected = posts.length > 0 && selectedIds.length === posts.length;
  const someSelected = selectedIds.length > 0 && selectedIds.length < posts.length;
  
  const sortedPosts = useMemo(() => sortPosts(posts, sortKey, sortDir), [posts, sortKey, sortDir]);
  
  const handleSelectAll = () => {
    onSelectAll(!allSelected);
  };
  
  const handleExport = () => {
    const csvContent = exportToCSV(posts);
    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `posts-export-${new Date().toISOString().split("T")[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };
  
  const handleBulkRetry = () => {
    if (selectedIds.length > 0 && window.confirm(`Retry ${selectedIds.length} failed post(s)?`)) {
      onBulkRetry(selectedIds);
    }
  };
  
  const handleBulkDelete = () => {
    if (selectedIds.length > 0 && window.confirm(`Delete ${selectedIds.length} post(s)? This action cannot be undone.`)) {
      onBulkDelete(selectedIds);
    }
  };
  
  // Pagination logic
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
      {/* Bulk Actions Bar */}
      {selectedIds.length > 0 && (
        <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-xl border border-outline-variant/30 px-5 py-3 flex items-center justify-between shadow-sm animate-in slide-in-from-top-2">
          <div className="flex items-center gap-3">
            <span className="text-label-sm font-semibold text-on-surface">
              {selectedIds.length} post(s) selected
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={handleBulkRetry}
                className="px-4 py-1.5 bg-primary/10 text-primary rounded-lg text-label-sm font-semibold hover:bg-primary/20 transition-all flex items-center gap-1.5"
              >
                <span className="material-symbols-outlined text-[14px]">refresh</span>
                Retry Selected
              </button>
              <button
                onClick={handleBulkDelete}
                className="px-4 py-1.5 bg-danger-red/10 text-danger-red rounded-lg text-label-sm font-semibold hover:bg-danger-red/20 transition-all flex items-center gap-1.5"
              >
                <span className="material-symbols-outlined text-[14px]">delete</span>
                Delete Selected
              </button>
              <button
                onClick={() => onSelectAll(false)}
                className="px-3 py-1.5 border border-outline-variant/40 rounded-lg text-label-sm text-outline hover:text-on-surface hover:bg-surface-container transition-all"
              >
                Clear Selection
              </button>
            </div>
          </div>
          <button
            onClick={() => setShowBulkActions(!showBulkActions)}
            className="p-1.5 text-outline hover:bg-surface-container-high rounded-lg transition-colors"
          >
            <span className="material-symbols-outlined text-[16px]">{showBulkActions ? "expand_less" : "expand_more"}</span>
          </button>
        </div>
      )}

      {/* Table */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 overflow-hidden shadow-sm">
        {loading ? (
          <div className="flex items-center justify-center py-32">
            <span className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
          </div>
        ) : sortedPosts.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <span className="material-symbols-outlined text-4xl text-outline/20 mb-3">post_add</span>
            <p className="text-body-sm text-outline font-medium">No posts found</p>
            <p className="text-[11px] text-outline/60 mt-1">Try adjusting your filters or create a new post</p>
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
                        onChange={handleSelectAll}
                        className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20"
                      />
                    </th>
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold cursor-pointer select-none hover:text-on-surface"
                      onClick={() => onSort("contentTitle")}>
                      Post Preview
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
                    <th className="px-6 py-4 text-label-sm text-outline uppercase tracking-wider font-semibold text-right">
                      Actions
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
                      onEdit={onEdit}
                      onRetry={onRetry}
                      onDelete={onDelete}
                      onAnalytics={onAnalytics}
                      deleting={deleting}
                    />
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="bg-surface-container-low px-6 py-4 border-t border-outline-variant/20 flex items-center justify-between">
              <div className="flex items-center gap-4">
                <p className="text-body-sm text-outline">
                  Showing <span className="font-bold text-on-surface">{Math.min((page - 1) * pageSize + 1, totalCount)} – {Math.min(page * pageSize, totalCount)}</span> of {totalCount} posts
                </p>
                <button
                  onClick={handleExport}
                  className="flex items-center gap-1.5 text-label-sm text-primary font-semibold hover:text-primary/80 transition-colors"
                >
                  <span className="material-symbols-outlined text-[14px]">download</span>
                  Export CSV
                </button>
              </div>
              
              <div className="flex items-center gap-2">
                {/* Rows per page selector */}
                <div className="flex items-center gap-2 mr-4">
                  <span className="text-[11px] text-outline">Rows:</span>
                  <select 
                    value={pageSize}
                    onChange={() => onPageChange(1)}
                    className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-2 py-1 text-[11px] text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                  >
                    <option value="10">10</option>
                    <option value="25">25</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                  </select>
                </div>
                
                {/* Pagination buttons */}
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
                      <span key={`ellipsis-${idx}`} className="w-8 h-8 flex items-center justify-center text-outline/40">
                        ...
                      </span>
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
