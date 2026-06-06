"use client";

import { useState, useRef, useEffect } from "react";
import { PostItem } from "@/services/postService";
import { formatDate, formatTime, daysUntil, getStatusStyle } from "@/lib/postUtils";
import { PLATFORM_CONFIG } from "@/lib/contentConstants";

interface PostRowProps {
  post: PostItem;
  isSelected: boolean;
  onSelect: (id: string, selected: boolean) => void;
  onEdit: (post: PostItem) => void;
  onRetry: (id: string) => void;
  onDelete: (id: string) => void;
  onAnalytics: (post: PostItem) => void;
  deleting: string | null;
}

function PlatformBadge({ platform }: { platform?: string }) {
  const cfg = PLATFORM_CONFIG[platform || ""];
  if (!cfg) return <span className="text-[10px] text-outline">—</span>;
  
  if (platform === "facebook") return (
    <div className="flex items-center gap-2">
      <div className="w-6 h-6 rounded flex items-center justify-center text-white text-[11px] font-bold" style={{ backgroundColor: cfg.color }}>f</div>
      <span className="text-label-md text-on-surface">{cfg.label}</span>
    </div>
  );
  
  if (platform === "linkedin") return (
    <div className="flex items-center gap-2">
      <div className="w-6 h-6 rounded flex items-center justify-center text-white text-[11px] font-bold" style={{ backgroundColor: "#0A66C2" }}>in</div>
      <span className="text-label-md text-on-surface">{cfg.label}</span>
    </div>
  );
  
  return (
    <div className="flex items-center gap-2">
      <div className="w-6 h-6 rounded flex items-center justify-center text-white" style={{ background: "linear-gradient(135deg, #F58529, #DD2A7B, #8134AF)" }}>
        <span className="material-symbols-outlined text-[12px]">photo_camera</span>
      </div>
      <span className="text-label-md text-on-surface">{cfg.label}</span>
    </div>
  );
}

export default function PostRow({
  post,
  isSelected,
  onSelect,
  onEdit,
  onRetry,
  onDelete,
  onAnalytics,
  deleting
}: PostRowProps) {
  const isPublished = post.status === "Published";
  const isScheduled = post.status === "Scheduled";
  const isFailed = post.status === "Failed";
  const days = isScheduled ? daysUntil(post.publishedAt) : null;
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    if (menuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [menuOpen]);
  
  return (
    <tr className="hover:bg-surface-container-low/50 transition-colors group" style={{ animation: `slide-up 0.3s ease-out forwards` }}>
      {/* Select Checkbox */}
      <td className="px-6 py-5">
        <input
          type="checkbox"
          checked={isSelected}
          onChange={(e) => onSelect(post.id, e.target.checked)}
          className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20"
        />
      </td>
      
      {/* Post Preview */}
      <td className="px-6 py-5">
        <div className="flex items-center gap-4">
          <div className={`w-14 h-14 rounded-lg overflow-hidden shrink-0 bg-gradient-to-br from-primary/5 to-secondary/5 flex items-center justify-center border ${isFailed ? "grayscale opacity-60 border-danger-red/20" : "border-outline-variant/20"}`}>
            <span className={`material-symbols-outlined text-[24px] ${isFailed ? "text-danger-red/30" : "text-outline/30"}`}>
              {post.type === "IMAGE" ? "image" : post.type === "VIDEO" ? "movie" : post.type === "TEXT" ? "article" : "collections"}
            </span>
          </div>
          <div className="min-w-0 max-w-[260px]">
            <p className="text-body-sm font-semibold text-on-surface">{post.contentTitle || "Untitled"}</p>
            <p className="text-[11px] text-outline line-clamp-1 mt-0.5">{post.caption || ""}</p>
            <div className="flex items-center gap-2.5 mt-1">
              {post.likes !== undefined && (
                <span className="flex items-center gap-0.5 text-[9px] text-outline/60">
                  <span className="material-symbols-outlined text-[10px]">favorite</span>
                  {post.likes >= 1000 ? `${(post.likes / 1000).toFixed(1)}k` : post.likes}
                </span>
              )}
              {post.comments !== undefined && (
                <span className="flex items-center gap-0.5 text-[9px] text-outline/60">
                  <span className="material-symbols-outlined text-[10px]">chat_bubble</span>
                  {post.comments}
                </span>
              )}
              {post.shares !== undefined && (
                <span className="flex items-center gap-0.5 text-[9px] text-outline/60">
                  <span className="material-symbols-outlined text-[10px]">share</span>
                  {post.shares}
                </span>
              )}
            </div>
          </div>
        </div>
      </td>
      
      {/* Platform & Brand */}
      <td className="px-6 py-5">
        <div className="flex flex-col gap-1">
          <PlatformBadge platform={post.platform} />
          <span className="text-[10px] text-outline">{post.brandName}</span>
          {post.type && (
            <span className="text-[9px] text-outline/60 uppercase font-semibold tracking-wide">
              {post.type}
            </span>
          )}
        </div>
      </td>
      
      {/* Status */}
      <td className="px-6 py-5">
        <span className={`px-3 py-1 rounded-full text-label-sm flex items-center gap-1.5 w-fit border ${getStatusStyle(post.status)}`}>
          <span className={`w-1.5 h-1.5 rounded-full ${
            isPublished ? "bg-emerald-500" :
            isScheduled ? "bg-blue-500 animate-pulse" :
            "bg-danger-red"
          }`} />
          {post.status}
        </span>
        {post.errorMessage && (
          <p className="text-[10px] text-danger-red font-bold mt-1 truncate max-w-[200px]" title={post.errorMessage}>
            {post.errorMessage}
          </p>
        )}
      </td>
      
      {/* Date */}
      <td className="px-6 py-5">
        <p className="text-label-md text-on-surface">{formatDate(post.publishedAt)}</p>
        {isScheduled && days && (
          <p className="text-[10px] text-primary font-bold uppercase tracking-tight">{days}</p>
        )}
        {isPublished && (
          <p className="text-[10px] text-outline">{formatTime(post.publishedAt)}</p>
        )}
        <p className="text-[9px] text-outline/60 mt-1">Created: {formatDate(post.createdAt)}</p>
      </td>
      
      {/* Actions */}
      <td className="px-6 py-5">
        <div className="flex items-center justify-end gap-2">
          {isPublished ? (
            <>
              <button onClick={() => onAnalytics(post)} className="p-1.5 text-primary hover:bg-primary/10 rounded-lg transition-colors" title="View Analytics">
                <span className="material-symbols-outlined text-[16px]">analytics</span>
              </button>
              <div className="relative" ref={menuRef}>
                <button onClick={() => setMenuOpen(!menuOpen)} className="p-1.5 text-outline hover:bg-surface-container-high rounded-lg transition-colors">
                  <span className="material-symbols-outlined text-[16px]">more_vert</span>
                </button>
                {menuOpen && (
                  <div className="absolute right-0 top-full mt-1 w-44 bg-surface-container-lowest rounded-xl border border-outline-variant/30 shadow-xl z-10 py-1 animate-in fade-in zoom-in-95 origin-top-right duration-150">
                    <button onClick={() => { onAnalytics(post); setMenuOpen(false); }} className="w-full text-left px-4 py-2.5 text-[11px] text-on-surface hover:bg-surface-container-low flex items-center gap-2 transition-colors">
                      <span className="material-symbols-outlined text-[14px] text-outline">analytics</span> View Analytics
                    </button>
                    <button onClick={() => { onDelete(post.id); setMenuOpen(false); }} className="w-full text-left px-4 py-2.5 text-[11px] text-danger-red hover:bg-danger-red/5 flex items-center gap-2 transition-colors">
                      <span className="material-symbols-outlined text-[14px]">delete</span> Delete
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : isScheduled ? (
            <>
              <button 
                onClick={() => onEdit(post)} 
                className="px-3 py-1.5 border border-outline-variant/40 rounded-lg text-label-sm text-outline hover:text-on-surface hover:bg-surface-container transition-all"
                title="Edit schedule"
              >
                Edit
              </button>
              <div className="relative" ref={menuRef}>
                <button onClick={() => setMenuOpen(!menuOpen)} className="p-1.5 text-outline hover:bg-surface-container-high rounded-lg transition-colors">
                  <span className="material-symbols-outlined text-[16px]">more_vert</span>
                </button>
                {menuOpen && (
                  <div className="absolute right-0 top-full mt-1 w-44 bg-surface-container-lowest rounded-xl border border-outline-variant/30 shadow-xl z-10 py-1 animate-in fade-in zoom-in-95 origin-top-right duration-150">
                    <button onClick={() => { onDelete(post.id); setMenuOpen(false); }} className="w-full text-left px-4 py-2.5 text-[11px] text-danger-red hover:bg-danger-red/5 flex items-center gap-2 transition-colors">
                      <span className="material-symbols-outlined text-[14px]">delete</span> Delete
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : (
            <>
              <button 
                onClick={() => onRetry(post.id)} 
                className="px-3 py-1.5 bg-primary/10 text-primary rounded-lg text-label-sm font-semibold hover:bg-primary/20 transition-all flex items-center gap-1.5"
                disabled={deleting === post.id}
              >
                <span className="material-symbols-outlined text-[14px]">refresh</span>
                Retry
              </button>
              <button 
                onClick={() => onDelete(post.id)} 
                className="p-1.5 text-danger-red hover:bg-danger-red/10 rounded-lg transition-colors"
                disabled={deleting === post.id}
                title="Delete"
              >
                <span className="material-symbols-outlined text-[16px]">delete</span>
              </button>
            </>
          )}
        </div>
      </td>
    </tr>
  );
}