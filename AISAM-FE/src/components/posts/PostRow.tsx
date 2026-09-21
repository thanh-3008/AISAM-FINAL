"use client";

import { PostItem } from "@/services/postService";
import { formatDate, getStatusStyle } from "@/lib/postUtils";
import { PLATFORM_CONFIG, PlatformIcon, CONTENT_TYPES } from "@/lib/contentConstants";
import RichTextPreview from "@/components/content/RichTextPreview";

interface PostRowProps {
  post: PostItem;
  isSelected: boolean;
  onSelect: (id: string, selected: boolean) => void;
  onView: (post: PostItem) => void;
}

function PlatformBadge({ platform }: { platform?: string | null }) {
  const cfg = PLATFORM_CONFIG[platform || ""];
  if (!cfg) return <span className="text-label-xs text-outline">—</span>;

  return (
    <div className="flex items-center gap-2">
      <PlatformIcon platform={platform || "facebook"} className="w-5 h-5" />
      <span className="text-label-sm text-on-surface font-medium">{cfg.label}</span>
    </div>
  );
}

function TypeBadge({ type }: { type?: string | null }) {
  if (!type) return null;
  const config = CONTENT_TYPES.find(t => t.value === type);
  return (
    <span className={`text-label-2xs uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-surface-container-high text-outline`}>
      {config?.label || type}
    </span>
  );
}

function AttributionLine({ icon, label, value }: { icon: string; label: string; value?: string | null }) {
  return <div className="flex min-w-0 items-center gap-1.5" title={`${label}: ${value || "Unavailable"}`}>
    <span className="material-symbols-outlined shrink-0 text-[15px] text-outline/70">{icon}</span>
    <span className="shrink-0 text-[11px] text-outline">{label}</span>
    <span className={`max-w-40 truncate text-label-xs font-semibold ${value ? "text-on-surface" : "text-outline/60"}`}>{value || "Unavailable"}</span>
  </div>;
}

function ContentIcon({ type, imageUrl, imageUrls, videoUrl, thumbnailUrl }: { type?: string | null; imageUrl?: string | null; imageUrls?: string[] | null; videoUrl?: string | null; thumbnailUrl?: string | null }) {
  const icon = type === "VIDEO" ? "movie" : type === "IMAGE" ? "image" : "article";
  const displayImage = imageUrls?.[0] || thumbnailUrl || (imageUrl && !imageUrl.startsWith("[") ? imageUrl : null);
  const count = imageUrls?.length || 0;

  if (videoUrl || displayImage) {
    return (
      <div className="w-12 h-10 rounded-lg overflow-hidden shrink-0 bg-gradient-to-br from-primary/5 to-secondary/5 flex items-center justify-center border border-outline-variant/20 relative">
        {videoUrl ? (
          <>
            <video src={videoUrl} className="absolute inset-0 w-full h-full object-cover bg-black" muted preload="metadata" />
            <div className="absolute inset-0 bg-black/25" />
            <span className="material-symbols-outlined text-[16px] text-white relative z-10">play_circle</span>
          </>
        ) : (
          <img src={displayImage!} alt="Post preview" className="absolute inset-0 w-full h-full object-cover" />
        )}
        {count > 1 && (
          <span className="absolute bottom-0 right-0 px-1 rounded-tl bg-black/75 text-[9px] font-bold text-white z-10">
            {count}
          </span>
        )}
      </div>
    );
  }

  return (
    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center shrink-0">
      <span className="material-symbols-outlined text-[20px] text-primary">{icon}</span>
    </div>
  );
}

export default function PostRow({
  post,
  isSelected,
  onSelect,
  onView,
}: PostRowProps) {
  return (
    <tr
      className="hover:bg-surface-container-low/50 transition-colors group cursor-pointer border-b border-outline-variant/10 last:border-b-0"
      onClick={() => onView(post)}
    >
      <td className="px-6 py-4" onClick={(e) => e.stopPropagation()}>
        <input
          type="checkbox"
          checked={isSelected}
          onChange={(e) => onSelect(post.id, e.target.checked)}
          className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20"
        />
      </td>

      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <ContentIcon type={post.type} imageUrl={post.imageUrl} imageUrls={post.imageUrls} videoUrl={post.videoUrl} thumbnailUrl={post.thumbnailUrl} />
          <div className="min-w-0 max-w-[280px]">
            <p className="text-body-sm font-semibold text-on-surface truncate">{post.contentTitle || "Untitled"}</p>
            {post.caption && (
              <RichTextPreview content={post.caption} richTextJson={post.richTextJson} className="text-label-xs text-outline line-clamp-1 mt-0.5" />
            )}
          </div>
        </div>
      </td>

      <td className="px-6 py-4">
        <div className="flex flex-col gap-1.5">
          <PlatformBadge platform={post.platform} />
          <div className="flex items-center gap-2">
            <span className="text-label-xs text-outline">{post.brandName}</span>
            <TypeBadge type={post.type} />
          </div>
        </div>
      </td>

      <td className="px-6 py-4">
        <div className="flex min-w-[190px] flex-col gap-1.5">
          <AttributionLine icon="person" label="Created by" value={post.creatorName} />
          <AttributionLine icon="verified_user" label="Reviewed by" value={post.reviewerName} />
          <AttributionLine icon="groups" label="Team" value={post.teamName} />
        </div>
      </td>

      <td className="px-6 py-4">
        <span className={`px-3 py-1 rounded-full text-label-sm flex items-center gap-1.5 w-fit border ${getStatusStyle(post.status)}`}>
          <span className={`w-1.5 h-1.5 rounded-full ${
            post.status === "Published" ? "bg-emerald-500" :
            post.status === "Draft" ? "bg-outline/40" :
            post.status === "PendingApproval" ? "bg-amber-500" :
            post.status === "Approved" ? "bg-sky-500" :
            post.status === "Rejected" ? "bg-danger-red" :
            "bg-outline/40"
          }`} />
          {post.status}
        </span>
      </td>

      <td className="px-6 py-4">
        <p className="text-label-sm text-on-surface">{formatDate(post.publishedAt)}</p>
      </td>
    </tr>
  );
}
