"use client";

import { PostItem } from "@/services/postService";
import { formatDate, getStatusStyle } from "@/lib/postUtils";
import { PLATFORM_CONFIG, PlatformIcon } from "@/lib/contentConstants";

interface PostDetailModalProps {
  post: PostItem;
  onClose: () => void;
}

function PlatformBadge({ platform }: { platform?: string | null }) {
  const cfg = PLATFORM_CONFIG[platform || ""];
  if (!cfg) return <span className="text-label-xs text-outline">—</span>;

  return (
    <div className="flex items-center gap-2.5">
      <PlatformIcon platform={platform || "facebook"} className="w-7 h-7" />
      <span className="text-body-sm text-on-surface font-semibold">{cfg.label}</span>
    </div>
  );
}

function DetailRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-start justify-between gap-4 py-3 border-b border-outline-variant/10 last:border-b-0">
      <span className="text-label-sm text-outline font-medium shrink-0 w-28">{label}</span>
      <div className="text-body-sm text-on-surface text-right">{children}</div>
    </div>
  );
}

export default function PostDetailModal({ post, onClose }: PostDetailModalProps) {
  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl overflow-hidden" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
          <div className="px-6 py-5 border-b border-outline-variant/20 flex items-center justify-between bg-surface-container-lowest">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-[18px] text-primary">article</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Post Details</h2>
                <p className="text-label-xs text-outline">Published post record</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="px-6 py-5 space-y-2">
            {/* Title */}
            <div className="mb-4">
              <p className="text-body-sm font-bold text-on-surface text-lg">{post.contentTitle || "Untitled"}</p>
            </div>

            {/* Status + Platform row */}
            <div className="flex items-center gap-4 mb-4">
              <span className={`px-3 py-1 rounded-full text-label-sm flex items-center gap-1.5 w-fit border ${getStatusStyle(post.status)}`}>
                <span className={`w-1.5 h-1.5 rounded-full ${post.status === "Published" ? "bg-emerald-500" : "bg-outline/40"}`} />
                {post.status}
              </span>
              <PlatformBadge platform={post.platform} />
            </div>

            {/* Media preview */}
            {(post.videoUrl || post.imageUrl || post.thumbnailUrl) && (
              <div className="mb-4 rounded-xl overflow-hidden bg-surface-container-low border border-outline-variant/20 max-h-[320px] flex items-center justify-center relative">
                {post.videoUrl ? (
                  <video src={post.videoUrl} controls className="w-full max-h-[320px] object-contain bg-black rounded-xl" />
                ) : (
                  <img src={post.imageUrl || post.thumbnailUrl || ""} alt={post.contentTitle || "Post media"} className="w-full max-h-[320px] object-contain rounded-xl" />
                )}
              </div>
            )}

            {/* Details */}
            <div className="bg-surface-container-low rounded-xl px-4">
              <DetailRow label="Brand">
                <span>{post.brandName || "—"}</span>
              </DetailRow>
              <DetailRow label="Type">
                <span className="uppercase font-semibold tracking-wide">{post.type || "—"}</span>
              </DetailRow>
              <DetailRow label="Published At">
                <span>{formatDate(post.publishedAt)}</span>
              </DetailRow>
              <DetailRow label="Post ID">
                <span className="font-mono text-label-xs">{post.id}</span>
              </DetailRow>
              <DetailRow label="Content ID">
                <span className="font-mono text-label-xs">{post.contentId}</span>
              </DetailRow>
              {post.externalPostId && (
                <DetailRow label="External ID">
                  <span className="font-mono text-label-xs truncate max-w-[200px] block">{post.externalPostId}</span>
                </DetailRow>
              )}
            </div>

            {/* Caption */}
            {post.caption && (
              <div className="mt-4">
                <p className="text-label-sm text-outline font-medium mb-2">Caption</p>
                <p className="text-body-sm text-on-surface leading-relaxed bg-surface-container-low p-4 rounded-xl border border-outline-variant/10">
                  {post.caption}
                </p>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-4 border-t border-outline-variant/20 flex items-center justify-end bg-surface-container-lowest">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/30 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </>
  );
}