"use client";

import { PostItem } from "@/services/postService";
import { formatDate, formatTime, daysUntil, getStatusStyle } from "@/lib/postUtils";
import { PLATFORM_CONFIG, PlatformIcon } from "@/lib/contentConstants";

interface PostDetailModalProps {
  post: PostItem;
  onClose: () => void;
}

function PlatformBadge({ platform }: { platform?: string }) {
  const cfg = PLATFORM_CONFIG[platform || ""];
  if (!cfg) return <span className="text-label-xs text-outline">—</span>;
  
  return (
    <div className="flex items-center gap-2">
      <PlatformIcon platform={platform || "facebook"} className="w-8 h-8" />
      <span className="text-body-sm text-on-surface font-semibold">{cfg.label}</span>
    </div>
  );
}

export default function PostDetailModal({ post, onClose }: PostDetailModalProps) {
  const isPublished = post.status === "Published";
  const isScheduled = post.status === "Scheduled";
  const isFailed = post.status === "Failed";
  const days = isScheduled ? daysUntil(post.publishedAt) : null;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-3xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <h2 className="text-headline-sm font-bold text-on-surface">Post Details</h2>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 grid grid-cols-1 lg:grid-cols-2 gap-8">
            {/* Left: Post Info */}
            <div className="space-y-6">
              {/* Status Badge */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Status</label>
                <span className={`px-4 py-2 rounded-full text-label-md flex items-center gap-2 w-fit border ${getStatusStyle(post.status)}`}>
                  <span className={`w-2 h-2 rounded-full ${
                    isPublished ? "bg-emerald-500" :
                    isScheduled ? "bg-blue-500 animate-pulse" :
                    "bg-danger-red"
                  }`} />
                  {post.status}
                </span>
                {isScheduled && days && (
                  <p className="text-[11px] text-primary font-bold uppercase tracking-tight mt-2">{days}</p>
                )}
                {isFailed && post.errorMessage && (
                  <div className="mt-3 p-3 bg-danger-red/10 border border-danger-red/20 rounded-lg">
                    <p className="text-[11px] text-danger-red font-bold flex items-center gap-1.5">
                      <span className="material-symbols-outlined text-[14px]">error</span>
                      Error: {post.errorMessage}
                    </p>
                  </div>
                )}
              </div>

              {/* Platform & Brand */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Platform & Brand</label>
                <div className="space-y-3">
                  <PlatformBadge platform={post.platform} />
                  <div className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-outline">business</span>
                    <span className="text-body-sm text-on-surface">{post.brandName}</span>
                  </div>
                  {post.type && (
                    <div className="flex items-center gap-2">
                      <span className="material-symbols-outlined text-[16px] text-outline">
                        {post.type === "IMAGE" ? "image" : post.type === "VIDEO" ? "movie" : post.type === "TEXT" ? "article" : "collections"}
                      </span>
                      <span className="text-body-sm text-on-surface uppercase font-semibold tracking-wide">{post.type}</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Content */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Content</label>
                <div className="space-y-3">
                  <div>
                    <p className="text-label-xs text-outline mb-1">Title</p>
                    <p className="text-body-sm text-on-surface font-semibold">{post.contentTitle || "Untitled"}</p>
                  </div>
                  {post.caption && (
                    <div>
                      <p className="text-label-xs text-outline mb-1">Caption</p>
                      <p className="text-body-sm text-on-surface leading-relaxed bg-surface-container-low p-3 rounded-lg">{post.caption}</p>
                    </div>
                  )}
                </div>
              </div>

              {/* Dates */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Schedule</label>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-[11px] text-outline">Published At</span>
                    <span className="text-[11px] text-on-surface font-semibold">
                      {formatDate(post.publishedAt)} at {formatTime(post.publishedAt)}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-[11px] text-outline">Created</span>
                    <span className="text-[11px] text-on-surface">{formatDate(post.createdAt)}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-[11px] text-outline">Last Updated</span>
                    <span className="text-[11px] text-on-surface">{formatDate(post.updatedAt)}</span>
                  </div>
                </div>
              </div>

              {/* IDs */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Identifiers</label>
                <div className="space-y-1.5 text-label-xs">
                  <div className="flex items-center justify-between">
                    <span className="text-outline">Post ID</span>
                    <span className="text-on-surface font-mono">{post.id}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-outline">Content ID</span>
                    <span className="text-on-surface font-mono">{post.contentId}</span>
                  </div>
                  {post.externalPostId && (
                    <div className="flex items-center justify-between">
                      <span className="text-outline">External ID</span>
                      <span className="text-on-surface font-mono text-label-2xs">{post.externalPostId}</span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Right: Preview & Stats */}
            <div className="space-y-6">
              {/* Engagement Stats (only for published) */}
              {isPublished && (
                <div>
                  <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-3">Engagement</label>
                  <div className="grid grid-cols-3 gap-3">
                    <div className="bg-gradient-to-br from-rose-50 to-rose-100 rounded-xl p-4 text-center border border-rose-200/30">
                      <span className="material-symbols-outlined text-rose-500 text-[24px]">favorite</span>
                      <p className="text-headline-sm text-on-surface font-bold mt-2">{(post.likes || 0).toLocaleString()}</p>
                      <p className="text-label-xs text-outline uppercase font-semibold">Likes</p>
                    </div>
                    <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl p-4 text-center border border-blue-200/30">
                      <span className="material-symbols-outlined text-blue-500 text-[24px]">chat_bubble</span>
                      <p className="text-headline-sm text-on-surface font-bold mt-2">{(post.comments || 0).toLocaleString()}</p>
                      <p className="text-label-xs text-outline uppercase font-semibold">Comments</p>
                    </div>
                    <div className="bg-gradient-to-br from-emerald-50 to-emerald-100 rounded-xl p-4 text-center border border-emerald-200/30">
                      <span className="material-symbols-outlined text-emerald-500 text-[24px]">share</span>
                      <p className="text-headline-sm text-on-surface font-bold mt-2">{(post.shares || 0).toLocaleString()}</p>
                      <p className="text-label-xs text-outline uppercase font-semibold">Shares</p>
                    </div>
                  </div>
                  <div className="mt-3 p-3 bg-surface-container-low rounded-lg">
                    <div className="flex items-center justify-between text-[11px]">
                      <span className="text-outline">Total Engagement</span>
                      <span className="text-on-surface font-bold">
                        {((post.likes || 0) + (post.comments || 0) + (post.shares || 0)).toLocaleString()}
                      </span>
                    </div>
                  </div>
                </div>
              )}

              {/* Mobile Preview */}
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-3">Mobile Preview</label>
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
                      <p className="text-label-2xs text-on-surface-variant line-clamp-3">{post.caption || "No caption"}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            <button onClick={onClose} className="px-6 py-3 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">
              Close
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
