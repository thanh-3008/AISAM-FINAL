"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { useRouter } from "next/navigation";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import Header from "@/components/layout/Header";
import PostNowModal from "@/components/content/PostNowModal";
import { fetchContents, approveContent, rejectContent, deleteContent } from "@/services/contentService";
import { fetchSchedules } from "@/services/scheduleService";
import { fetchWorkspaceMembers, type WorkspaceMember } from "@/services/workspaceService";
import {
  PLATFORM_CONFIG, PlatformIcon, getTypeStyle, getTypeConfig,
  getBrandColor,
  ALL_PLATFORMS,
} from "@/lib/contentConstants";
import type { ContentItem } from "@/services/contentService";
import type { ScheduleItem } from "@/services/scheduleService";
import { getApprovalBrands, matchesApprovalBrand } from "@/lib/approvalBrands";

type TabKey = "all" | "pending" | "approved" | "published" | "failed" | "rejected";
type ApprovalStatus = ContentItem["status"] | "Publish Failed" | "Failed" | "Post Failed" | "PublishFailed";
type ApprovalListItem = Omit<ContentItem, "status"> & {
  status: ApprovalStatus;
  approvalSource?: "content" | "schedule";
  sourceContentId?: string;
  scheduleId?: string;
  scheduledAt?: string | null;
  failedAt?: string | null;
  failureReason?: string | null;
  attemptCount?: number;
};

const TEAM_COLORS = [
  "bg-blue-500", "bg-emerald-500", "bg-amber-500", "bg-purple-500", "bg-pink-500", "bg-cyan-500"
];

const DOT_COLORS: Record<TabKey, string> = {
  all: "",
  pending: "bg-warning-amber",
  approved: "bg-emerald-500",
  published: "bg-primary",
  failed: "bg-danger-red",
  rejected: "bg-danger-red",
};

type SortKey = "title" | "brandName" | "createdAt" | "type";
type SortDir = "asc" | "desc";

function renderSortIcon(activeKey: SortKey, direction: SortDir, key: SortKey) {
  if (activeKey !== key) {
    return <span className="material-symbols-outlined text-[12px] text-outline/20 ml-0.5">unfold_more</span>;
  }

  return (
    <span className="material-symbols-outlined text-[12px] text-primary ml-0.5">
      {direction === "asc" ? "expand_less" : "expand_more"}
    </span>
  );
}

function getPriority(item: ApprovalListItem): { label: string; color: string } {
  const tags = item.tags || [];
  if (tags.some((t) => t === "Product Launch" || t === "Promotion"))
    return { label: "Urgent", color: "text-danger-red bg-danger-red/10" };
  if (tags.some((t) => t === "Seasonal"))
    return { label: "Medium", color: "text-warning-amber bg-warning-amber/10" };
  return { label: "Standard", color: "text-outline bg-surface-container-high" };
}

function getInitials(name: string) {
  return name.split(" ").map((n) => n[0]).join("").slice(0, 2);
}

function sortItems(list: ApprovalListItem[], key: SortKey, dir: SortDir): ApprovalListItem[] {
  return [...list].sort((a, b) => {
    let cmp = 0;
    if (key === "title") cmp = a.title.localeCompare(b.title);
    else if (key === "brandName") cmp = a.brandName.localeCompare(b.brandName);
    else if (key === "createdAt") cmp = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    else if (key === "type") cmp = a.type.localeCompare(b.type);
    return dir === "asc" ? cmp : -cmp;
  });
}

function isPendingStatus(status: string) {
  return status === "Awaiting Approval" || status === "PendingApproval" || status === "Pending";
}

function isApprovedStatus(status: string) {
  return status === "Approved";
}

function isRejectedStatus(status: string) {
  return status === "Rejected";
}

function isPublishedStatus(status: string) {
  return status === "Published";
}

function isPublishFailedStatus(status: string) {
  return ["PublishFailed", "Publish Failed", "Failed", "PostFailed", "Post Failed"].includes(status);
}

function getStatusMeta(status: string) {
  if (isPendingStatus(status)) {
    return {
      label: "Pending",
      icon: "hourglass_top",
      className: "bg-warning-amber/10 text-warning-amber ring-1 ring-warning-amber/20",
      dotClassName: "bg-warning-amber animate-pulse",
    };
  }

  if (isApprovedStatus(status)) {
    return {
      label: "Approved",
      icon: "verified",
      className: "bg-emerald-50 text-emerald-600 ring-1 ring-emerald-500/20",
      dotClassName: "bg-emerald-500",
    };
  }

  if (isRejectedStatus(status)) {
    return {
      label: "Rejected",
      icon: "block",
      className: "bg-danger-red/10 text-danger-red ring-1 ring-danger-red/20",
      dotClassName: "bg-danger-red",
    };
  }

  if (isPublishedStatus(status)) {
    return {
      label: "Published",
      icon: "publish",
      className: "bg-primary/10 text-primary ring-1 ring-primary/20",
      dotClassName: "bg-primary",
    };
  }

  if (isPublishFailedStatus(status)) {
    return {
      label: "Publish Failed",
      icon: "error",
      className: "bg-danger-red/10 text-danger-red ring-1 ring-danger-red/20",
      dotClassName: "bg-danger-red",
    };
  }

  return {
    label: status || "Draft",
    icon: "edit_note",
    className: "bg-surface-container-high text-on-surface-variant ring-1 ring-outline-variant/20",
    dotClassName: "bg-outline",
  };
}

function isLikelyVideoUrl(url?: string | null): boolean {
  if (!url) return false;
  const normalized = url.split("?")[0].toLowerCase();
  return normalized.includes("/video/") || /\.(mp4|mov|m4v|webm|avi|mkv)$/i.test(normalized);
}

function getPreviewImageUrl(item: ApprovalListItem) {
  const imageUrl = parseScheduleMediaUrl(item.imageUrl);
  if (imageUrl) return imageUrl;
  const thumbnail = parseScheduleMediaUrl(item.thumbnail);
  return thumbnail && !isLikelyVideoUrl(thumbnail) ? thumbnail : "";
}

function getPreviewVideoUrl(item: ApprovalListItem) {
  const videoUrl = parseScheduleMediaUrl(item.videoUrl);
  if (videoUrl) return videoUrl;
  const thumbnail = parseScheduleMediaUrl(item.thumbnail);
  return thumbnail && isLikelyVideoUrl(thumbnail) ? thumbnail : "";
}

function parseScheduleMediaUrl(url?: string | null): string {
  const raw = (url || "").trim();
  if (!raw) return "";

  const pickUrl = (value: unknown): string => {
    if (!value) return "";
    if (typeof value === "string") return parseScheduleMediaUrl(value);
    if (Array.isArray(value)) {
      for (const entry of value) {
        const parsed = pickUrl(entry);
        if (parsed) return parsed;
      }
      return "";
    }
    if (typeof value === "object") {
      const record = value as Record<string, unknown>;
      for (const key of ["url", "secure_url", "imageUrl", "image_url", "videoUrl", "video_url", "thumbnailUrl", "src"]) {
        const parsed = pickUrl(record[key]);
        if (parsed) return parsed;
      }
    }
    return "";
  };

  try {
    const parsed = JSON.parse(raw);
    const parsedUrl = pickUrl(parsed);
    if (parsedUrl) return parsedUrl;
  } catch {
    // Some legacy rows contain plain URLs or JSON-ish strings; fall through to regex.
  }

  const match = raw.match(/https?:\/\/[^"'\]\s,}]+/i);
  return match?.[0] || raw;
}

function normalizeScheduleType(type?: string | null): ContentItem["type"] {
  const normalized = (type || "").toUpperCase();
  if (normalized === "VIDEO") return "VIDEO";
  if (normalized === "IMAGE") return "IMAGE";
  return "TEXT";
}

function normalizeSchedulePlatform(platform?: string | null) {
  const normalized = (platform || "").toLowerCase();
  return PLATFORM_CONFIG[normalized] ? normalized : "facebook";
}

function mapFailedScheduleToApprovalItem(schedule: ScheduleItem, sourceContent?: ApprovalListItem): ApprovalListItem {
  const type = normalizeScheduleType(schedule.type || sourceContent?.type);
  const thumbnailUrl = parseScheduleMediaUrl(schedule.thumbnailUrl);
  const sourceThumbnailUrl = parseScheduleMediaUrl(sourceContent?.thumbnail);
  const imageUrl = parseScheduleMediaUrl(schedule.imageUrl) || parseScheduleMediaUrl(sourceContent?.imageUrl);
  const videoUrl = parseScheduleMediaUrl(schedule.videoUrl) || parseScheduleMediaUrl(sourceContent?.videoUrl);
  const thumbnail = thumbnailUrl || videoUrl || imageUrl || sourceThumbnailUrl;
  const thumbnailAsImage = thumbnailUrl && !isLikelyVideoUrl(thumbnailUrl) ? thumbnailUrl : "";
  const sourceThumbnailAsImage = sourceThumbnailUrl && !isLikelyVideoUrl(sourceThumbnailUrl) ? sourceThumbnailUrl : "";
  const thumbnailAsVideo = thumbnailUrl && isLikelyVideoUrl(thumbnailUrl) ? thumbnailUrl : "";
  const sourceThumbnailAsVideo = sourceThumbnailUrl && isLikelyVideoUrl(sourceThumbnailUrl) ? sourceThumbnailUrl : "";

  return {
    id: `schedule-${schedule.id}`,
    sourceContentId: schedule.contentId,
    scheduleId: schedule.id,
    approvalSource: "schedule",
    title: schedule.title || sourceContent?.title || "Failed scheduled post",
    brandId: schedule.brandId || sourceContent?.brandId || "",
    brandName: schedule.brandName || sourceContent?.brandName || "Unknown brand",
    productName: schedule.productName || sourceContent?.productName || "",
    type,
    status: "Publish Failed",
    thumbnail,
    imageUrl: imageUrl || thumbnailAsImage || sourceThumbnailAsImage || undefined,
    videoUrl: videoUrl || thumbnailAsVideo || sourceThumbnailAsVideo || undefined,
    textContent: schedule.textContent || sourceContent?.textContent || "",
    createdAt: schedule.executedAt || schedule.scheduledAt,
    platforms: [normalizeSchedulePlatform(schedule.platform)],
    tags: ["Publish Failed"],
    hashtags: [],
    isAiGenerated: false,
    scheduledAt: schedule.scheduledAt,
    failedAt: schedule.executedAt,
    failureReason: schedule.lastError || "Publishing failed without a saved error message.",
    attemptCount: schedule.attemptCount,
  };
}

function isScheduleFailureItem(item: ApprovalListItem) {
  return item.approvalSource === "schedule";
}

function SocialPostPreview({ item, platform }: { item: ApprovalListItem; platform: string }) {
  const imageUrl = getPreviewImageUrl(item);
  const videoUrl = getPreviewVideoUrl(item);
  const caption = item.textContent?.trim() || item.title || "No caption provided.";
  const cfg = PLATFORM_CONFIG[platform];
  const brandInitial = (item.brandName || "A").charAt(0).toUpperCase();
  const brandColor = getBrandColor(item.brandName || "AISAM") || "#2563eb";

  if (platform === "instagram") {
    return (
      <div className="mx-auto max-w-sm rounded-2xl overflow-hidden border border-outline-variant/20 bg-white text-[#111827] shadow-sm">
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-full p-[2px] bg-gradient-to-tr from-yellow-400 via-pink-500 to-purple-600">
              <div className="w-full h-full rounded-full bg-white p-[2px]">
                <div className="w-full h-full rounded-full flex items-center justify-center text-white text-label-xs font-bold" style={{ backgroundColor: brandColor }}>
                  {brandInitial}
                </div>
              </div>
            </div>
            <div>
              <p className="text-sm font-bold leading-tight">{item.brandName || "Brand"}</p>
              <p className="text-[11px] text-gray-500">{item.productName || "Product"}</p>
            </div>
          </div>
          <span className="material-symbols-outlined text-[18px]">more_horiz</span>
        </div>
        <div className="aspect-square bg-gray-100 flex items-center justify-center overflow-hidden">
          {videoUrl ? (
            <video src={videoUrl} controls className="w-full h-full object-contain bg-black" />
          ) : imageUrl ? (
            <img src={imageUrl} alt={item.title || "Instagram preview"} className="w-full h-full object-cover" />
          ) : (
            <div className="text-gray-400 text-sm flex flex-col items-center gap-2">
              <span className="material-symbols-outlined">image</span>
              No media
            </div>
          )}
        </div>
        <div className="px-4 py-3 space-y-2">
          <div className="flex items-center gap-4">
            <span className="material-symbols-outlined text-[22px]">favorite</span>
            <span className="material-symbols-outlined text-[22px]">mode_comment</span>
            <span className="material-symbols-outlined text-[22px]">send</span>
            <span className="material-symbols-outlined text-[22px] ml-auto">bookmark</span>
          </div>
          <p className="text-sm whitespace-pre-line leading-relaxed"><span className="font-bold">{item.brandName || "brand"}</span> {caption}</p>
        </div>
      </div>
    );
  }

  if (platform === "tiktok") {
    return (
      <div className="mx-auto w-[260px] h-[460px] rounded-[2rem] overflow-hidden bg-black text-white shadow-sm relative border border-black">
        {videoUrl ? (
          <video src={videoUrl} controls className="absolute inset-0 w-full h-full object-cover" />
        ) : imageUrl ? (
          <img src={imageUrl} alt={item.title || "TikTok preview"} className="absolute inset-0 w-full h-full object-cover opacity-80" />
        ) : (
          <div className="absolute inset-0 bg-gradient-to-br from-zinc-800 to-black flex items-center justify-center text-zinc-400">
            <span className="material-symbols-outlined text-4xl">smart_display</span>
          </div>
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/10 to-black/10" />
        <div className="absolute right-3 bottom-24 flex flex-col items-center gap-4">
          <div className="w-10 h-10 rounded-full bg-white/15 backdrop-blur flex items-center justify-center">
            <span className="material-symbols-outlined text-[20px]">favorite</span>
          </div>
          <div className="w-10 h-10 rounded-full bg-white/15 backdrop-blur flex items-center justify-center">
            <span className="material-symbols-outlined text-[20px]">chat_bubble</span>
          </div>
          <div className="w-10 h-10 rounded-full bg-white/15 backdrop-blur flex items-center justify-center">
            <span className="material-symbols-outlined text-[20px]">share</span>
          </div>
        </div>
        <div className="absolute left-4 right-14 bottom-5">
          <p className="text-sm font-bold">@{(item.brandName || "brand").replace(/\s+/g, "").toLowerCase()}</p>
          <p className="mt-2 text-xs leading-relaxed whitespace-pre-line line-clamp-6">{caption}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-md rounded-2xl overflow-hidden border border-outline-variant/20 bg-white text-[#111827] shadow-sm">
      <div className="flex items-center gap-3 px-4 py-3">
        <div className="w-10 h-10 rounded-full flex items-center justify-center text-white text-label-sm font-bold" style={{ backgroundColor: cfg?.color || brandColor }}>
          {brandInitial}
        </div>
        <div>
          <p className="text-sm font-bold leading-tight">{item.brandName || "Brand"}</p>
          <p className="text-[11px] text-gray-500">Just now · Public</p>
        </div>
        <span className="material-symbols-outlined text-[18px] ml-auto text-gray-500">more_horiz</span>
      </div>
      <div className="px-4 pb-3">
        <p className="text-sm whitespace-pre-line leading-relaxed">{caption}</p>
      </div>
      {(videoUrl || imageUrl) && (
        <div className="bg-gray-100 border-y border-gray-100">
          {videoUrl ? (
            <video src={videoUrl} controls className="w-full max-h-[320px] object-contain bg-black" />
          ) : (
            <img src={imageUrl} alt={item.title || "Facebook preview"} className="w-full max-h-[320px] object-contain" />
          )}
        </div>
      )}
      <div className="grid grid-cols-3 px-4 py-2 text-gray-500 text-sm border-t border-gray-100">
        <span className="flex items-center justify-center gap-1"><span className="material-symbols-outlined text-[18px]">thumb_up</span>Like</span>
        <span className="flex items-center justify-center gap-1"><span className="material-symbols-outlined text-[18px]">mode_comment</span>Comment</span>
        <span className="flex items-center justify-center gap-1"><span className="material-symbols-outlined text-[18px]">share</span>Share</span>
      </div>
    </div>
  );
}

export default function ApprovalsPage() {
  const router = useRouter();
  const { activeWorkspace } = useWorkspaces();
  const featureGate = useFeatureGate();
  const canReview = featureGate.isOwner || featureGate.isManager || featureGate.isContentCreator;
  const [items, setItems] = useState<ApprovalListItem[]>([]);
  const [teamMembers, setTeamMembers] = useState<WorkspaceMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionId, setActionId] = useState<string | null>(null);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" | "undo"; id?: string; undo?: () => void } | null>(null);
  const [tab, setTab] = useState<TabKey>("all");
  const [brandFilter, setBrandFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [search, setSearch] = useState("");
  const [sortKey, setSortKey] = useState<SortKey>("createdAt");
  const [sortDir, setSortDir] = useState<SortDir>("desc");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;
  const [confirmItem, setConfirmItem] = useState<ApprovalListItem | null>(null);
  const [drawerItem, setDrawerItem] = useState<ApprovalListItem | null>(null);
  const [drawerPreviewPlatform, setDrawerPreviewPlatform] = useState("facebook");
  const [postNowItem, setPostNowItem] = useState<ApprovalListItem | null>(null);
  const [revisionDrawer, setRevisionDrawer] = useState<ApprovalListItem | null>(null);
  const [noteMode, setNoteMode] = useState<"revision" | "reject">("revision");
  const [batchRejectIds, setBatchRejectIds] = useState<string[]>([]);
  const [revisionNote, setRevisionNote] = useState("");
  const revisionsRef = useRef<HTMLTextAreaElement>(null);

  const load = useCallback(async (reset = true) => {
    if (reset) { setLoading(true); }
    const [contentResult, scheduleResult] = await Promise.all([
      fetchContents({ pageSize: 100 }),
      fetchSchedules({ pageSize: 100 }),
    ]);
    const contentItems: ApprovalListItem[] = (contentResult?.items ?? [])
      .filter((item) => item.status !== "Draft")
      .map((item) => ({
        ...item,
        approvalSource: "content",
      }));
    const contentById = new Map(contentItems.map((item) => [item.id, item]));
    const failedScheduleItems = (scheduleResult.data.data ?? [])
      .filter((schedule) => schedule.status === "Failed")
      .map((schedule) => mapFailedScheduleToApprovalItem(schedule, contentById.get(schedule.contentId)));
    setItems([...contentItems, ...failedScheduleItems]);
    setLoading(false);
  }, [activeWorkspace?.id]);

  useEffect(() => {
    void Promise.resolve().then(() => load());
  }, [load]);

  useEffect(() => {
    if (activeWorkspace?.workspaceType === 2) {
      fetchWorkspaceMembers().then(res => {
        if (res?.data) {
          setTeamMembers(res.data);
        }
      });
    }
  }, [activeWorkspace]);

  const showToast = (message: string, type: "success" | "error" | "undo" = "success", undo?: () => void) => {
    setToast({ message, type, undo });
    setTimeout(() => setToast(null), type === "undo" ? 6000 : 3000);
  };

  const openReviewDrawer = (item: ApprovalListItem) => {
    const firstPlatform = item.platforms.find((platform) => PLATFORM_CONFIG[platform]) || "facebook";
    setDrawerPreviewPlatform(firstPlatform);
    setDrawerItem(item);
  };

  const applyItemStatus = (id: string, status: ContentItem["status"]) => {
    setItems((prev) => prev.map((item) => (item.id === id ? { ...item, status } : item)));
  };

  const handleApprove = async (id: string) => {
    setActionId(id);
    const item = items.find((i) => i.id === id);
    const success = await approveContent(id);
    if (success) {
      applyItemStatus(id, "Approved");
      setSelected((prev) => { const s = new Set(prev); s.delete(id); return s; });
      if (drawerItem?.id === id) setDrawerItem(null);
      if (revisionDrawer?.id === id) { setRevisionDrawer(null); setRevisionNote(""); }
      showToast(`"${item?.title || "Asset"}" approved`, "success");
    } else {
      showToast("Failed to approve content", "error");
    }
    setActionId(null);
  };

  const openRejectModal = (item: ApprovalListItem) => {
    setConfirmItem(null);
    setNoteMode("reject");
    setRevisionDrawer(item);
    setRevisionNote("");
    setTimeout(() => revisionsRef.current?.focus(), 100);
  };

  const handleDeleteRejected = async (item: ApprovalListItem) => {
    if (!isRejectedStatus(item.status)) return;
    const ok = window.confirm(`Delete rejected content "${item.title || "Asset"}"?`);
    if (!ok) return;

    setActionId(item.id);
    const deleted = await deleteContent(item.id);
    setActionId(null);

    if (!deleted) {
      showToast("Could not delete rejected content", "error");
      return;
    }

    setItems((prev) => prev.filter((content) => content.id !== item.id));
    setSelected((prev) => { const s = new Set(prev); s.delete(item.id); return s; });
    if (drawerItem?.id === item.id) setDrawerItem(null);
    showToast(`"${item.title || "Asset"}" deleted`, "success");
  };

  const batchApprove = async () => {
    const selectedIds = Array.from(selected);
    let successCount = 0;
    for (const id of selectedIds) {
      setActionId(id);
      const success = await approveContent(id);
      if (success) {
        setItems((prev) => prev.map((item) => (item.id === id ? { ...item, status: "Approved" } : item)));
        successCount++;
      }
      setActionId(null);
    }
    showToast(`${successCount} assets approved`, "success");
    setSelected(new Set());
  };

  const batchReject = async () => {
    const selectedIds = Array.from(selected);
    const firstItem = items.find((item) => item.id === selectedIds[0]);
    if (!firstItem) return;
    setBatchRejectIds(selectedIds);
    openRejectModal(firstItem);
  };

  const handleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else { setSortKey(key); setSortDir("asc"); }
  };

  const handleRequestChanges = (item: ApprovalListItem) => {
    setNoteMode("revision");
    setRevisionDrawer(item);
    setRevisionNote("");
    setTimeout(() => revisionsRef.current?.focus(), 100);
  };

  const submitRevision = async () => {
    if (!revisionDrawer || revisionNote.trim().length < 5) return;
    const ids = batchRejectIds.length > 0 ? batchRejectIds : [revisionDrawer.id];
    setActionId(revisionDrawer.id);
    try {
      let successCount = 0;
      for (const id of ids) {
        if (await rejectContent(id, revisionNote.trim())) {
          applyItemStatus(id, "Rejected");
          successCount++;
        }
      }
      if (successCount === ids.length) {
        showToast(ids.length > 1 ? `${successCount} assets rejected` : noteMode === "reject" ? "Content rejected" : "Revision requested", "success");
        setSelected(new Set());
      } else {
        showToast(`Rejected ${successCount} of ${ids.length} assets`, "error");
      }
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Failed to submit rejection note", "error");
    }
    setActionId(null);
    setRevisionDrawer(null);
    setRevisionNote("");
    setBatchRejectIds([]);
  };

  const toggleSelect = (id: string) => {
    const item = items.find((entry) => entry.id === id);
    if (!item || isScheduleFailureItem(item)) return;
    setSelected((prev) => {
      const s = new Set(prev);
      if (s.has(id)) s.delete(id); else s.add(id);
      return s;
    });
  };

  const toggleSelectAll = () => {
    const selectableIds = paged.filter((item) => !isScheduleFailureItem(item)).map((item) => item.id);
    if (selectableIds.length === 0) return;
    const allSelected = selectableIds.every((id) => selected.has(id));
    setSelected((prev) => {
      const next = new Set(prev);
      if (allSelected) selectableIds.forEach((id) => next.delete(id));
      else selectableIds.forEach((id) => next.add(id));
      return next;
    });
  };

  const statusFilter: Record<TabKey, (i: ApprovalListItem) => boolean> = {
    all: () => true,
    pending: (i) => isPendingStatus(i.status),
    approved: (i) => isApprovedStatus(i.status),
    published: (i) => isPublishedStatus(i.status),
    failed: (i) => isPublishFailedStatus(i.status),
    rejected: (i) => isRejectedStatus(i.status),
  };

  const filtered = sortItems(
    items
      .filter(statusFilter[tab])
      .filter((i) => matchesApprovalBrand(i, brandFilter))
      .filter((i) => !priorityFilter || getPriority(i).label === priorityFilter)
      .filter((i) => !search || i.title.toLowerCase().includes(search.toLowerCase()) || i.brandName.toLowerCase().includes(search.toLowerCase())),
    sortKey, sortDir,
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const safeCurrentPage = Math.max(1, Math.min(currentPage, totalPages || 1));
  const paged = filtered.slice((safeCurrentPage - 1) * pageSize, safeCurrentPage * pageSize);
  
  const selectableFiltered = paged.filter((item) => !isScheduleFailureItem(item));
  const allSelectableSelected = selectableFiltered.length > 0 && selectableFiltered.every((item) => selected.has(item.id));

  const tabCounts: Record<TabKey, number> = {
    all: items.length,
    pending: items.filter((i) => isPendingStatus(i.status)).length,
    approved: items.filter((i) => isApprovedStatus(i.status)).length,
    published: items.filter((i) => isPublishedStatus(i.status)).length,
    failed: items.filter((i) => isPublishFailedStatus(i.status)).length,
    rejected: items.filter((i) => isRejectedStatus(i.status)).length,
  };

  const brands = getApprovalBrands(items);

  const SkeletonRow = () => (
    <tr className="animate-pulse">
      {[...Array(6)].map((_, i) => (
        <td key={i} className="px-6 py-4"><div className="h-4 bg-surface-container-high rounded w-3/4" /></td>
      ))}
    </tr>
  );

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Approvals" },
      ]} />

      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">
          {/* ── Page Header ── */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div>
              <div className="flex items-center gap-3 mb-1">
                <span className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center">
                  <span className="material-symbols-outlined text-[18px]">approval</span>
                </span>
                <h1 className="text-headline-sm text-on-surface font-bold">Content Approvals</h1>
                <span className="text-label-xs text-outline bg-surface-container-high px-2 py-0.5 rounded-full font-semibold">{filtered.length} items</span>
              </div>
              <p className="text-body-md text-on-surface-variant ml-11">Review and manage AI-generated marketing assets across your portfolio.</p>
            </div>
            <div className="flex items-center gap-4">
              {activeWorkspace?.workspaceType === 2 && (
                <div className="flex items-center -space-x-2">
                  {teamMembers.map((m, i) => (
                    <span key={i}
                      className={`w-8 h-8 rounded-full ${TEAM_COLORS[i % TEAM_COLORS.length]} text-white text-label-xs font-bold flex items-center justify-center ring-2 ring-surface-container-lowest`}
                      title={m.name}>{getInitials(m.name)}</span>
                  ))}
                  <span onClick={() => router.push("/team")} className="w-8 h-8 rounded-full bg-surface-container text-on-surface-variant text-[14px] font-medium flex items-center justify-center ring-2 ring-surface-container-lowest cursor-pointer hover:bg-surface-container-high transition-all">
                    <span className="material-symbols-outlined text-[14px]">add</span>
                  </span>
                </div>
              )}
              <div className="flex items-center gap-2">
                <button onClick={() => {
                  const csv = ["Title,Brand,Type,Status,Platforms,Created"];
                  filtered.forEach((i) => csv.push(`"${i.title}","${i.brandName}","${i.type}","${i.status}","${i.platforms.join(";")}","${i.createdAt}"`));
                  const blob = new Blob([csv.join("\n")], { type: "text/csv" });
                  const url = URL.createObjectURL(blob);
                  const a = document.createElement("a"); a.href = url; a.download = "approvals.csv"; a.click();
                  URL.revokeObjectURL(url);
                  showToast("Exported to CSV", "success");
                }}
                  className="px-3 py-2 rounded-xl border border-outline-variant/20 text-label-sm text-on-surface-variant hover:bg-surface-container transition-all flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-[14px]">file_download</span>
                  Export
                </button>
                <button onClick={() => load()} disabled={loading}
                  className="px-3 py-2 rounded-xl border border-outline-variant/20 text-label-sm text-on-surface-variant hover:bg-surface-container transition-all flex items-center gap-1.5">
                  <span className={`material-symbols-outlined text-[14px] ${loading ? "animate-spin" : ""}`}>refresh</span>
                  Refresh
                </button>
              </div>
            </div>
          </div>

          {/* ── Status Tabs ── */}
          <div className="border-b border-outline-variant flex gap-6">
            {([
              { key: "all", label: "All" },
              { key: "pending", label: "Pending" },
              { key: "approved", label: "Approved" },
              { key: "published", label: "Published" },
              { key: "failed", label: "Failed" },
              { key: "rejected", label: "Rejected" },
            ] as { key: TabKey; label: string }[]).map((t) => (
              <button key={t.key} onClick={() => { setTab(t.key); setSelected(new Set()); setCurrentPage(1); }}
                className={`pb-3 text-label-sm font-semibold transition-all border-b-2 ${
                  tab === t.key
                    ? "border-primary text-primary"
                    : "border-transparent text-outline hover:text-on-surface"
                }`}>
                <span className="flex items-center gap-2">
                  {DOT_COLORS[t.key] && <span className={`w-2 h-2 rounded-full ${DOT_COLORS[t.key]}`} />}
                  {t.label}
                  <span className="text-label-xs text-outline/60">({tabCounts[t.key]})</span>
                </span>
              </button>
            ))}
          </div>

          {/* ── Search + Filters ── */}
          <div className="flex flex-wrap items-center gap-3">
            <div className="relative flex-1 min-w-[200px] max-w-sm">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline/40 text-[16px]">search</span>
              <input value={search} onChange={(e) => { setSearch(e.target.value); setCurrentPage(1); }}
                placeholder="Search by title or brand..."
                className="w-full bg-surface-container-lowest border border-outline-variant/20 rounded-lg pl-9 pr-9 py-2 text-body-sm text-on-surface placeholder:text-outline/40 focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all" />
              {search && (
                <button onClick={() => { setSearch(""); setCurrentPage(1); }} className="absolute right-3 top-1/2 -translate-y-1/2 text-outline/40 hover:text-outline">
                  <span className="material-symbols-outlined text-[14px]">close</span>
                </button>
              )}
            </div>
            <div className="relative">
              <select value={brandFilter} onChange={(e) => { setBrandFilter(e.target.value); setCurrentPage(1); }}
                className="appearance-none bg-surface-container-lowest border border-outline-variant/20 rounded-lg pl-4 pr-10 py-2 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all min-w-[140px]">
                <option value="">Brand: All</option>
                {brands.map((brand) => (
                  <option key={brand.brandId} value={brand.brandId}>Brand: {brand.brandName}</option>
                ))}
              </select>
              <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-outline pointer-events-none text-[14px]">expand_more</span>
            </div>
            <div className="relative">
              <select value={priorityFilter} onChange={(e) => { setPriorityFilter(e.target.value); setCurrentPage(1); }}
                className="appearance-none bg-surface-container-lowest border border-outline-variant/20 rounded-lg pl-4 pr-10 py-2 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all min-w-[140px]">
                <option value="">Priority: All</option>
                <option value="Urgent">Urgent</option>
                <option value="Medium">Medium</option>
                <option value="Standard">Standard</option>
              </select>
              <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-outline pointer-events-none text-[14px]">expand_more</span>
            </div>
          </div>

          {/* ── Batch Actions Bar ── */}
          {selected.size > 0 && (
            <div className="flex items-center gap-3 px-4 py-3 bg-primary/5 border border-primary/20 rounded-xl animate-in fade-in slide-in-from-top-2 duration-200">
              <span className="text-label-sm font-semibold text-primary">{selected.size} selected</span>
              <div className="flex items-center gap-2 ml-auto">
                <button onClick={batchApprove}
                  className="px-4 py-1.5 bg-emerald-500 text-white text-label-xs font-bold rounded-lg hover:bg-emerald-600 transition-all flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-[12px]">check_circle</span>
                  Approve All
                </button>
                <button onClick={batchReject}
                  className="px-4 py-1.5 bg-danger-red/10 text-danger-red text-label-xs font-bold rounded-lg hover:bg-danger-red/20 transition-all flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-[12px]">block</span>
                  Reject All
                </button>
                <button onClick={() => setSelected(new Set())}
                  className="px-3 py-1.5 text-label-xs font-semibold text-outline hover:text-on-surface transition-all">Clear</button>
              </div>
            </div>
          )}

          {/* ── Content ── */}
          {loading ? (
            <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden shadow-sm">
              <table className="w-full text-left border-collapse">
                <thead className="bg-surface-container-low">
                  <tr>{["Content", "Brand", "Requester", "Platform", "Urgency", "Actions"].map((h) => (
                    <th key={h} className="px-6 py-4"><div className="h-3 bg-surface-container-high rounded w-16" /></th>
                  ))}</tr>
                </thead>
                <tbody className="divide-y divide-outline-variant/20">
                  {[...Array(5)].map((_, i) => <SkeletonRow key={i} />)}
                </tbody>
              </table>
            </div>
          ) : filtered.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-24 text-center">
              <div className="w-24 h-24 bg-gradient-to-br from-surface-container to-surface-container-high rounded-full flex items-center justify-center mb-5 shadow-inner">
                <span className="material-symbols-outlined text-5xl text-outline/30">done_all</span>
              </div>
              <h3 className="text-headline-sm text-on-surface font-semibold">
                {search || brandFilter || priorityFilter ? "No matching results" : `No ${tab === "all" ? "assets" : tab} items`}
              </h3>
              <p className="text-body-sm text-outline mt-1 mb-6">
                {search || brandFilter || priorityFilter ? "Try adjusting your search or filters." : "Your queue is empty. High five your team!"}
              </p>
              {(search || brandFilter || priorityFilter) && (
                <button onClick={() => { setSearch(""); setBrandFilter(""); setPriorityFilter(""); setCurrentPage(1); }}
                  className="text-label-sm text-primary font-semibold hover:underline">Clear all filters</button>
              )}
              {tab !== "all" && !search && !brandFilter && (
                <button onClick={() => { setTab("all"); setCurrentPage(1); }}
                  className="text-label-sm text-primary font-semibold hover:underline">View all assets</button>
              )}
            </div>
          ) : (
            <>
              <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden shadow-sm">
                <table className="w-full text-left border-collapse">
                  <thead className="bg-surface-container-low">
                    <tr>
                      <th className="px-4 py-4 w-10">
                        <div className="flex items-center justify-center">
                          <input type="checkbox" checked={allSelectableSelected}
                            disabled={selectableFiltered.length === 0}
                            onChange={toggleSelectAll}
                            className="w-3.5 h-3.5 rounded border-outline-variant text-primary focus:ring-primary/30 cursor-pointer disabled:cursor-not-allowed disabled:opacity-40" />
                        </div>
                      </th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface transition-colors"
                        onClick={() => handleSort("title")}>
                        <span className="flex items-center gap-0.5">Content{renderSortIcon(sortKey, sortDir, "title")}</span>
                      </th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface transition-colors"
                        onClick={() => handleSort("brandName")}>
                        <span className="flex items-center gap-0.5">Brand{renderSortIcon(sortKey, sortDir, "brandName")}</span>
                      </th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Requester</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Platform</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface transition-colors"
                        onClick={() => handleSort("createdAt")}>
                        <span className="flex items-center gap-0.5">Date{renderSortIcon(sortKey, sortDir, "createdAt")}</span>
                      </th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Urgency</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Status</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/20">
                    {paged.map((item) => {
                      const typeCfg = getTypeConfig(item.type);
                      const priority = getPriority(item);
                      const brandColor = getBrandColor(item.brandName);
                      const isSelected = selected.has(item.id);
                      const isSelectable = !isScheduleFailureItem(item);
                      const statusMeta = getStatusMeta(item.status);
                      const canReview = isPendingStatus(item.status) && (featureGate.isOwner || featureGate.isManager || featureGate.isContentCreator);
                      const canDelete = isRejectedStatus(item.status);
                      const rowImageUrl = getPreviewImageUrl(item);
                      const rowVideoUrl = getPreviewVideoUrl(item);
                      return (
                        <tr key={item.id}
                          className={`transition-colors cursor-pointer group ${
                            isSelected ? "bg-primary/5" : "hover:bg-surface-container-low/60"
                          }`}
                          onClick={() => openReviewDrawer(item)}>
                          <td className="px-4 py-4" onClick={(e) => e.stopPropagation()}>
                            <div className="flex items-center justify-center">
                              <input type="checkbox" checked={isSelected} disabled={!isSelectable} onChange={() => toggleSelect(item.id)}
                                className="w-3.5 h-3.5 rounded border-outline-variant text-primary focus:ring-primary/30 cursor-pointer disabled:cursor-not-allowed disabled:opacity-40" />
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-4">
                              <div className={`w-14 h-11 rounded-lg bg-gradient-to-br ${getTypeStyle(item.type)} flex items-center justify-center text-white shrink-0 relative overflow-hidden`}>
                                {rowVideoUrl || rowImageUrl ? (
                                  rowVideoUrl ? (
                                    <>
                                      <video src={rowVideoUrl} className="absolute inset-0 w-full h-full object-cover bg-black" muted preload="metadata" />
                                      <div className="absolute inset-0 bg-black/25" />
                                      <span className="material-symbols-outlined text-[18px] relative z-10">play_circle</span>
                                    </>
                                  ) : (
                                    <img src={rowImageUrl} alt={item.title || "Approval asset"} className="absolute inset-0 w-full h-full object-cover" />
                                  )
                                ) : (
                                  <>
                                    <span className="material-symbols-outlined text-[18px] relative z-10">{typeCfg.icon}</span>
                                    <div className="absolute inset-0 bg-white/10" />
                                  </>
                                )}
                              </div>
                              <div>
                                <p className="text-body-sm font-semibold text-on-surface leading-tight">{item.title}</p>
                                <p className="text-[11px] text-outline mt-0.5">{item.type}</p>
                                {item.failureReason && (
                                  <p className="mt-1 max-w-xs text-[11px] leading-snug text-danger-red line-clamp-2">{item.failureReason}</p>
                                )}
                              </div>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-2">
                              <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: brandColor }} />
                              <div>
                                <p className="text-body-sm font-semibold text-on-surface">{item.brandName || "Unknown brand"}</p>
                                <p className="text-[11px] text-outline">{item.productName}</p>
                              </div>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-2">
                              {item.isAiGenerated ? (
                                <>
                                  <div className="w-7 h-7 rounded-full bg-gradient-to-br from-primary/80 to-secondary/80 flex items-center justify-center text-white text-label-3xs font-bold shrink-0">
                                    <span className="material-symbols-outlined text-[12px]">auto_awesome</span>
                                  </div>
                                  <span className="text-[11px] font-semibold text-on-surface bg-primary/5 px-1.5 py-0.5 rounded">AI</span>
                                </>
                              ) : (
                                <>
                                  <div className="w-7 h-7 rounded-full bg-surface-container-high flex items-center justify-center text-on-surface-variant text-label-3xs font-bold shrink-0">
                                    <span className="material-symbols-outlined text-[12px]">person</span>
                                  </div>
                                  <span className="text-[11px] font-semibold text-on-surface">Manual</span>
                                </>
                              )}
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-1">
                              {item.platforms.slice(0, 2).map((p) => {
                                const cfg = PLATFORM_CONFIG[p];
                                return cfg ? (
                                  <span key={p} className="flex items-center gap-1 px-1.5 py-0.5 rounded text-label-xs font-semibold"
                                    style={{ backgroundColor: cfg.color + "15", color: cfg.color }}>
                                    <PlatformIcon platform={cfg.icon} className="w-[12px] h-[12px]" />
                                  </span>
                                ) : null;
                              })}
                              {item.platforms.length > 2 && (
                                <span className="text-label-xs text-outline font-medium px-1">+{item.platforms.length - 2}</span>
                              )}
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <p className="text-[11px] text-outline font-medium">{new Date(item.createdAt).toLocaleDateString("en-GB", { day: "numeric", month: "short" })}</p>
                          </td>
                          <td className="px-6 py-4">
                            <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-bold ${priority.color}`}>
                              {priority.label === "Urgent" && <span className="material-symbols-outlined text-label-xs">priority_high</span>}
                              {priority.label}
                            </span>
                          </td>
                          <td className="px-6 py-4">
                            <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-bold ${statusMeta.className}`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${statusMeta.dotClassName}`} />
                              {statusMeta.label}
                            </span>
                          </td>
                          <td className="px-6 py-4 text-right" onClick={(e) => e.stopPropagation()}>
                            <div className="flex items-center justify-end gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                              {canReview && (
                                <>
                                  <button onClick={() => handleApprove(item.id)} disabled={actionId === item.id}
                                    className="p-2 text-emerald-500 hover:bg-emerald-50 rounded-lg transition-all disabled:opacity-40 relative group/btn" title="Approve (A)">
                                    {actionId === item.id ? (
                                      <span className="w-3.5 h-3.5 border-2 border-emerald-500/30 border-t-emerald-500 rounded-full animate-spin block" />
                                    ) : (
                                      <span className="material-symbols-outlined text-[17px]">check_circle</span>
                                    )}
                                    <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Approve</span>
                                  </button>
                                  <button onClick={() => handleRequestChanges(item)} disabled={actionId === item.id}
                                    className="p-2 text-secondary hover:bg-secondary/10 rounded-lg transition-all disabled:opacity-40 relative group/btn" title="Request Changes">
                                    <span className="material-symbols-outlined text-[17px]">rate_review</span>
                                    <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Request Changes</span>
                                  </button>
                                </>
                              )}
                              <button onClick={() => openReviewDrawer(item)}
                                className="p-2 text-on-surface-variant hover:bg-surface-container rounded-lg transition-all relative group/btn" title="Review">
                                <span className="material-symbols-outlined text-[17px]">visibility</span>
                                <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Review</span>
                              </button>
                              {isApprovedStatus(item.status) && (
                                <>
                                  <button onClick={() => { setPostNowItem(item); }}
                                    className="p-2 text-primary hover:bg-primary/10 rounded-lg transition-all relative group/btn" title="Post Now">
                                    <span className="material-symbols-outlined text-[17px]">send</span>
                                    <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Post Now</span>
                                  </button>
                                  <button onClick={() => { router.push(`/calendar?contentId=${item.id}`); }}
                                    className="p-2 text-on-surface-variant hover:bg-surface-container rounded-lg transition-all relative group/btn" title="Schedule">
                                    <span className="material-symbols-outlined text-[17px]">calendar_month</span>
                                    <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Schedule</span>
                                  </button>
                                </>
                              )}
                              {(canReview || canDelete) && <div className="w-px h-5 bg-outline-variant/30 mx-0.5" />}
                              {canReview && (
                                <button onClick={() => setConfirmItem(item)} disabled={actionId === item.id}
                                  className="p-2 text-danger-red hover:bg-danger-red/10 rounded-lg transition-all disabled:opacity-40 relative group/btn" title="Reject (R)">
                                  <span className="material-symbols-outlined text-[17px]">block</span>
                                  <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Reject</span>
                                </button>
                              )}
                              {canDelete && (
                                <button onClick={() => handleDeleteRejected(item)} disabled={actionId === item.id}
                                  className="p-2 text-danger-red hover:bg-danger-red/10 rounded-lg transition-all disabled:opacity-40 relative group/btn" title="Delete rejected content">
                                  {actionId === item.id ? (
                                    <span className="w-3.5 h-3.5 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin block" />
                                  ) : (
                                    <span className="material-symbols-outlined text-[17px]">delete</span>
                                  )}
                                  <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Delete</span>
                                </button>
                              )}
                              <button onClick={(e) => e.stopPropagation()}
                                className="p-2 text-outline/40 hover:text-outline hover:bg-surface-container rounded-lg transition-all relative group/btn cursor-not-allowed" title="Leader Only">
                                <span className="material-symbols-outlined text-[15px]">lock</span>
                                <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-inverse-surface text-inverse-on-surface text-label-2xs px-2 py-1 rounded-md opacity-0 group-hover/btn:opacity-100 transition-opacity whitespace-nowrap">Leader only</span>
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              {/* Pagination Controls */}
              {totalPages > 1 && (
                <div className="flex items-center justify-between mt-4">
                  <p className="text-body-sm text-outline">
                    Showing {(safeCurrentPage - 1) * pageSize + 1} to {Math.min(safeCurrentPage * pageSize, filtered.length)} of {filtered.length} entries
                  </p>
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                      disabled={safeCurrentPage === 1}
                      className="p-2 rounded-lg border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container disabled:opacity-40 disabled:hover:bg-transparent transition-all flex items-center"
                    >
                      <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                    </button>
                    
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => {
                      if (
                        page === 1 ||
                        page === totalPages ||
                        (page >= safeCurrentPage - 1 && page <= safeCurrentPage + 1)
                      ) {
                        return (
                          <button
                            key={page}
                            onClick={() => setCurrentPage(page)}
                            className={`w-9 h-9 rounded-lg text-label-sm font-semibold transition-all ${
                              safeCurrentPage === page
                                ? "bg-primary text-white"
                                : "text-on-surface-variant hover:bg-surface-container"
                            }`}
                          >
                            {page}
                          </button>
                        );
                      }
                      if (
                        page === safeCurrentPage - 2 ||
                        page === safeCurrentPage + 2
                      ) {
                        return (
                          <span key={page} className="w-9 h-9 flex items-center justify-center text-outline">
                            ...
                          </span>
                        );
                      }
                      return null;
                    })}

                    <button
                      onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                      disabled={safeCurrentPage === totalPages}
                      className="p-2 rounded-lg border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container disabled:opacity-40 disabled:hover:bg-transparent transition-all flex items-center"
                    >
                      <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                    </button>
                  </div>
                </div>
              )}

            </>
          )}
        </div>

        {/* ── Confirm Reject Modal ── */}
        {confirmItem && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-[60]" onClick={() => setConfirmItem(null)} />
            <div className="fixed inset-0 z-[60] flex items-center justify-center p-4" onClick={() => setConfirmItem(null)}>
              <div className="w-full max-w-sm bg-surface-container-lowest rounded-2xl shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
                <div className="w-12 h-12 rounded-full bg-danger-red/10 text-danger-red flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-[24px]">block</span>
                </div>
                <h3 className="text-headline-sm font-bold text-on-surface text-center mb-2">Reject Asset?</h3>
                <p className="text-body-sm text-outline text-center mb-6">This will reject &quot;{confirmItem.title}&quot; and move it to the rejected queue.</p>
                <div className="flex items-center gap-3">
                  <button onClick={() => setConfirmItem(null)}
                    className="flex-1 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container transition-all">Cancel</button>
                  <button onClick={() => openRejectModal(confirmItem)} disabled={actionId === confirmItem.id}
                    className="flex-1 py-2.5 rounded-xl bg-danger-red text-white text-label-sm font-bold hover:bg-danger-red/90 transition-all disabled:opacity-50 flex items-center justify-center gap-2">
                    {actionId === confirmItem.id ? (
                      <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    ) : (
                      <span className="material-symbols-outlined text-[16px]">block</span>
                    )}
                    Reject
                  </button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* ── Details Modal ── */}
        {drawerItem && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => setDrawerItem(null)} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => setDrawerItem(null)}>
            <div className="w-full max-w-2xl max-h-[90vh] bg-surface-container-lowest rounded-2xl shadow-2xl flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
              <div className="px-6 py-4 border-b border-outline-variant/20 flex items-center justify-between shrink-0 bg-surface-container-low/30">
                <div className="flex items-center gap-3">
                  <div className={`w-9 h-9 rounded-xl bg-gradient-to-br ${getTypeStyle(drawerItem.type)} flex items-center justify-center text-white shadow-sm`}>
                    <span className="material-symbols-outlined text-[18px]">{getTypeConfig(drawerItem.type).icon}</span>
                  </div>
                  <div>
                    <h3 className="text-label-sm font-bold text-on-surface">Asset Review</h3>
                    <p className="text-label-xs text-outline">{drawerItem.type} · ID: {drawerItem.id.toUpperCase()}</p>
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <button onClick={() => setDrawerItem(null)} className="p-2 hover:bg-surface-container rounded-lg transition-all">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                </div>
              </div>

              <div className="flex-1 overflow-y-auto">
                <div className="relative w-full aspect-[2/1] bg-gradient-to-br from-surface-container to-surface-container-high flex items-center justify-center overflow-hidden">
                  {getPreviewVideoUrl(drawerItem) ? (
                    <video
                      src={getPreviewVideoUrl(drawerItem)}
                      className="absolute inset-0 w-full h-full object-contain bg-black"
                      controls
                      preload="metadata"
                    />
                  ) : getPreviewImageUrl(drawerItem) ? (
                    <img
                      src={getPreviewImageUrl(drawerItem)}
                      alt={drawerItem.title || "Approval asset preview"}
                      className="absolute inset-0 w-full h-full object-contain bg-surface-container-low"
                    />
                  ) : (
                    <>
                      <div className={`absolute inset-0 bg-gradient-to-br ${getTypeStyle(drawerItem.type)} opacity-15`} />
                      <div className="absolute inset-0 bg-gradient-to-t from-surface-container-lowest/80 via-transparent to-transparent" />
                      <div className="relative z-10 flex flex-col items-center gap-3">
                        <div className={`w-20 h-20 rounded-2xl bg-gradient-to-br ${getTypeStyle(drawerItem.type)} flex items-center justify-center text-white shadow-lg`}>
                          <span className="material-symbols-outlined text-4xl">{getTypeConfig(drawerItem.type).icon}</span>
                        </div>
                        <span className="text-label-sm font-semibold text-on-surface-variant bg-surface-container-lowest/80 backdrop-blur-sm px-4 py-1.5 rounded-full">
                          {drawerItem.type} Asset Preview
                        </span>
                      </div>
                    </>
                  )}
                  <div className="absolute top-3 right-3 flex gap-1.5">
                    <span className="text-label-xs font-bold px-2 py-1 rounded-md bg-surface-container-lowest/70 backdrop-blur-sm text-on-surface-variant">
                      {getTypeConfig(drawerItem.type).label}
                    </span>
                    <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-label-xs font-bold ${getPriority(drawerItem).color}`}>
                      {getPriority(drawerItem).label}
                    </span>
                  </div>
                </div>

                <div className="p-6 space-y-7">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <h2 className="text-headline-sm font-bold text-on-surface leading-snug">{drawerItem.title}</h2>
                      <div className="flex items-center gap-3 mt-1.5">
                        <div className="flex items-center gap-1.5">
                          <span className="w-2 h-2 rounded-full" style={{ backgroundColor: getBrandColor(drawerItem.brandName) || "#6366f1" }} />
                          <span className="text-body-sm text-on-surface-variant">{drawerItem.brandName}</span>
                        </div>
                        <span className="text-outline/30">·</span>
                        <span className="text-body-sm text-on-surface-variant">{drawerItem.productName}</span>
                      </div>
                    </div>
                    <span className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-[11px] font-bold shrink-0 ${getStatusMeta(drawerItem.status).className}`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${getStatusMeta(drawerItem.status).dotClassName}`} />
                      {getStatusMeta(drawerItem.status).label}
                    </span>
                  </div>

                  <section>
                    <div className="grid grid-cols-2 gap-3">
                      {[
                        { icon: "article", label: "Headline", value: drawerItem.title, full: true },
                        { icon: "calendar_today", label: "Created", value: new Date(drawerItem.createdAt).toLocaleDateString("en-GB", { day: "numeric", month: "long", year: "numeric" }) },
                        { icon: "person", label: "Requester", value: drawerItem.isAiGenerated ? "AI Generated" : "Manual", badge: true },
                        { icon: "business", label: "Brand", value: `${drawerItem.brandName} · ${drawerItem.productName}`, color: getBrandColor(drawerItem.brandName) || "#6366f1" },
                        { icon: "flag", label: "Priority", value: getPriority(drawerItem).label, chip: getPriority(drawerItem).color },
                      ].map((f, i) => (
                        <div key={i} className={`${f.full ? "col-span-2" : ""} p-4 rounded-xl bg-surface-container-low border border-outline-variant/10`}>
                          <label className="flex items-center gap-1 text-label-2xs text-outline uppercase font-bold tracking-widest mb-2">
                            <span className="material-symbols-outlined text-[11px]">{f.icon}</span>
                            {f.label}
                          </label>
                          {f.badge ? (
                            <div className="flex items-center gap-2">
                              <div className="w-6 h-6 rounded-full bg-gradient-to-br from-primary/80 to-secondary/80 flex items-center justify-center text-white shrink-0">
                                <span className="material-symbols-outlined text-label-xs">auto_awesome</span>
                              </div>
                              <p className="text-body-sm font-semibold text-on-surface">{f.value}</p>
                            </div>
                          ) : f.chip ? (
                            <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-xs font-bold ${f.chip}`}>{f.value}</span>
                          ) : f.color ? (
                            <div className="flex items-center gap-2">
                              <span className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: f.color }} />
                              <p className="text-body-sm font-semibold text-on-surface">{f.value}</p>
                            </div>
                          ) : (
                            <p className="text-body-sm font-semibold text-on-surface">{f.value}</p>
                          )}
                        </div>
                      ))}
                    </div>
                  </section>

                  {isPublishFailedStatus(drawerItem.status) && (
                    <section>
                      <div className="p-4 rounded-xl bg-danger-red/10 border border-danger-red/20">
                        <label className="flex items-center gap-1 text-label-2xs text-danger-red uppercase font-bold tracking-widest mb-2">
                          <span className="material-symbols-outlined text-[13px]">error</span>
                          Failure reason
                        </label>
                        <p className="text-body-sm text-danger-red whitespace-pre-line leading-relaxed">
                          {drawerItem.failureReason || "Publishing failed without a saved error message."}
                        </p>
                        <div className="mt-3 grid grid-cols-1 sm:grid-cols-3 gap-2 text-label-xs text-on-surface-variant">
                          {drawerItem.scheduledAt && (
                            <span>Scheduled: {new Date(drawerItem.scheduledAt).toLocaleString("en-GB")}</span>
                          )}
                          {drawerItem.failedAt && (
                            <span>Failed: {new Date(drawerItem.failedAt).toLocaleString("en-GB")}</span>
                          )}
                          {typeof drawerItem.attemptCount === "number" && (
                            <span>Attempts: {drawerItem.attemptCount}</span>
                          )}
                        </div>
                      </div>
                    </section>
                  )}

                  {drawerItem.tags && drawerItem.tags.length > 0 && (
                    <section>
                      <div className="p-4 rounded-xl bg-surface-container-low border border-outline-variant/10">
                        <label className="flex items-center gap-1 text-label-2xs text-outline uppercase font-bold tracking-widest mb-2.5">
                          <span className="material-symbols-outlined text-[11px]">label</span>
                          Campaign Tags
                        </label>
                        <div className="flex flex-wrap gap-1.5">
                          {drawerItem.tags.map((tag) => (
                            <span key={tag}
                              className="px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant text-label-xs font-medium border border-outline-variant/10">{tag}</span>
                          ))}
                        </div>
                      </div>
                    </section>
                  )}

                  <section>
                    <div className="p-4 rounded-xl bg-surface-container-low border border-outline-variant/10">
                      <label className="flex items-center gap-1 text-label-2xs text-outline uppercase font-bold tracking-widest mb-2.5">
                        <span className="material-symbols-outlined text-[13px]">notes</span>
                        Full Content
                      </label>
                      <div className="space-y-3">
                        <div>
                          <p className="text-label-2xs text-outline uppercase font-bold tracking-widest mb-1">Title</p>
                          <p className="text-body-sm font-semibold text-on-surface">{drawerItem.title || "Untitled"}</p>
                        </div>
                        <div>
                          <p className="text-label-2xs text-outline uppercase font-bold tracking-widest mb-1">Caption</p>
                          <p className="text-body-sm text-on-surface-variant whitespace-pre-line leading-relaxed">
                            {drawerItem.textContent?.trim() || "No caption provided."}
                          </p>
                        </div>
                      </div>
                    </div>
                  </section>

                  <section>
                    <div className="p-4 rounded-xl bg-surface-container-low border border-outline-variant/10">
                      <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
                        <label className="flex items-center gap-1 text-label-2xs text-outline uppercase font-bold tracking-widest">
                          <span className="material-symbols-outlined text-[13px]">preview</span>
                          Platform Preview
                        </label>
                        <div className="flex items-center gap-1 p-1 rounded-xl bg-surface-container-high">
                          {ALL_PLATFORMS.map((platform) => {
                            const cfg = PLATFORM_CONFIG[platform];
                            const active = drawerPreviewPlatform === platform;
                            return (
                              <button
                                key={platform}
                                type="button"
                                onClick={() => setDrawerPreviewPlatform(platform)}
                                className={`px-3 py-1.5 rounded-lg text-label-xs font-bold flex items-center gap-1.5 transition-all ${
                                  active ? "bg-surface-container-lowest shadow-sm text-on-surface" : "text-outline hover:text-on-surface"
                                }`}
                              >
                                <PlatformIcon platform={cfg.icon} className="w-[14px] h-[14px]" />
                                {cfg.label}
                              </button>
                            );
                          })}
                        </div>
                      </div>
                      <SocialPostPreview item={drawerItem} platform={drawerPreviewPlatform} />
                    </div>
                  </section>

                  <hr className="border-outline-variant/20" />

                  <section>
                    <h4 className="text-label-xs text-outline uppercase font-bold tracking-widest mb-4 flex items-center gap-2">
                      <span className="material-symbols-outlined text-[14px]">share</span>
                      Distribution
                    </h4>
                    <div className="grid grid-cols-2 gap-2">
                      {drawerItem.platforms.map((p) => {
                        const cfg = PLATFORM_CONFIG[p];
                        return cfg ? (
                          <div key={p} className="flex items-center gap-3 px-4 py-3 rounded-xl border"
                            style={{ backgroundColor: cfg.color + "06", color: cfg.color, borderColor: cfg.color + "15" }}>
                            <PlatformIcon platform={cfg.icon} className="w-[18px] h-[18px]" />
                            <div>
                              <p className="text-[11px] font-bold">{cfg.label}</p>
                              <p className="text-label-2xs opacity-60">{isPublishFailedStatus(drawerItem.status) ? "Publish failed" : "Post scheduled"}</p>
                            </div>
                          </div>
                        ) : null;
                      })}
                    </div>
                  </section>

                  <hr className="border-outline-variant/20" />

                  <section>
                    <h4 className="text-label-xs text-outline uppercase font-bold tracking-widest mb-5 flex items-center gap-2">
                      <span className="material-symbols-outlined text-[14px]">timeline</span>
                      Audit Trail
                    </h4>
                    <div className="space-y-0">
                      {[
                        { icon: drawerItem.isAiGenerated ? "auto_awesome" : "edit_note", color: drawerItem.isAiGenerated ? "bg-primary/10 text-primary" : "bg-surface-container-high text-on-surface-variant", label: drawerItem.isAiGenerated ? "AI generated content" : "Manual content", time: new Date(drawerItem.createdAt).toLocaleString("en-GB"), desc: drawerItem.isAiGenerated ? "AI-generated marketing asset created" : "Content created manually" },
                        { icon: "assignment", color: "bg-sky-500/10 text-sky-500", label: "Content review assigned", time: new Date(drawerItem.createdAt).toLocaleString("en-GB"), desc: `Assigned to ${teamMembers.length ? teamMembers.map((t) => t.name).join(", ") : "Workspace Team"}` },
                        isPublishFailedStatus(drawerItem.status)
                          ? {
                              icon: "error",
                              color: "bg-danger-red/10 text-danger-red",
                              label: "Publishing failed",
                              time: drawerItem.failedAt ? new Date(drawerItem.failedAt).toLocaleString("en-GB") : "Failed",
                              desc: drawerItem.failureReason || "Publishing failed",
                            }
                          : { icon: "flag", color: "bg-warning-amber/15 text-warning-amber", label: "Submitted for approval", time: "Pending", desc: "Awaiting review decision" },
                      ].map((step, i) => (
                        <div key={i} className="flex gap-4 relative pb-6 last:pb-0">
                          {i < 2 && <div className="absolute left-3 top-6 bottom-0 w-px bg-outline-variant/30" />}
                          <div className={`w-6 h-6 rounded-full ${step.color} flex items-center justify-center z-10 shrink-0 ring-2 ring-surface-container-lowest`}>
                            <span className="material-symbols-outlined text-[11px]">{step.icon}</span>
                          </div>
                          <div className="pt-0.5 flex-1 min-w-0">
                            <p className="text-body-sm font-semibold text-on-surface">{step.label}</p>
                            <div className="flex items-center gap-2 mt-0.5">
                              <span className="text-label-xs text-outline">{step.time}</span>
                              {step.desc && (
                                <>
                                  <span className="text-outline/20">·</span>
                                  <span className="text-label-xs text-outline">{step.desc}</span>
                                </>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  </section>
                </div>
              </div>

              <div className="px-6 py-4 border-t border-outline-variant/20 bg-surface-container-low/80 backdrop-blur-sm flex items-center gap-3 shrink-0">
                {isPendingStatus(drawerItem.status) && (featureGate.isOwner || featureGate.isManager || featureGate.isContentCreator) && (
                  <>
                    <button onClick={() => handleApprove(drawerItem.id)} disabled={actionId === drawerItem.id}
                      className="flex-1 bg-emerald-500 text-white py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-emerald-600 active:scale-[0.98] transition-all disabled:opacity-50 shadow-sm">
                      {actionId === drawerItem.id ? (
                        <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                      ) : (
                        <span className="material-symbols-outlined text-[17px]">verified</span>
                      )}
                      Approve
                    </button>
                    <button onClick={() => { handleRequestChanges(drawerItem); setDrawerItem(null); }}
                      className="flex-1 bg-secondary/5 text-secondary py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-secondary/10 active:scale-[0.98] transition-all border border-secondary/20">
                      <span className="material-symbols-outlined text-[17px]">rate_review</span>
                      Revise
                    </button>
                    <button onClick={() => { setConfirmItem(drawerItem); setDrawerItem(null); }} disabled={actionId === drawerItem.id}
                      className="px-4 py-3 bg-danger-red/5 text-danger-red rounded-xl text-label-sm font-bold hover:bg-danger-red/10 active:scale-[0.98] transition-all disabled:opacity-50 border border-danger-red/20">
                      <span className="material-symbols-outlined text-[17px]">block</span>
                    </button>
                  </>
                )}
                {isRejectedStatus(drawerItem.status) && (
                  <button onClick={() => handleDeleteRejected(drawerItem)} disabled={actionId === drawerItem.id}
                    className="flex-1 bg-danger-red/5 text-danger-red py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-danger-red/10 active:scale-[0.98] transition-all disabled:opacity-50 border border-danger-red/20">
                    {actionId === drawerItem.id ? (
                      <span className="w-4 h-4 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin" />
                    ) : (
                      <span className="material-symbols-outlined text-[17px]">delete</span>
                    )}
                    Delete Rejected Content
                  </button>
                )}
                {isApprovedStatus(drawerItem.status) && (
                  <>
                    <button onClick={() => { setPostNowItem(drawerItem); setDrawerItem(null); }}
                      className="flex-1 bg-primary text-on-primary py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:shadow-lg active:scale-[0.98] transition-all shadow-sm">
                      <span className="material-symbols-outlined text-[17px]">send</span>
                      Post Now
                    </button>
                    <button onClick={() => { router.push(`/calendar?contentId=${drawerItem.id}`); setDrawerItem(null); }}
                      className="flex-1 border border-outline-variant/20 text-on-surface-variant py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-surface-container active:scale-[0.98] transition-all">
                      <span className="material-symbols-outlined text-[17px]">calendar_month</span>
                      Schedule
                    </button>
                  </>
                  )}
                {isPublishFailedStatus(drawerItem.status) && drawerItem.sourceContentId && (
                  <button onClick={() => { router.push(`/calendar?contentId=${drawerItem.sourceContentId}`); setDrawerItem(null); }}
                    className="flex-1 bg-primary text-on-primary py-3 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:shadow-lg active:scale-[0.98] transition-all shadow-sm">
                    <span className="material-symbols-outlined text-[17px]">calendar_month</span>
                    Open in Calendar
                  </button>
                )}
              </div>
            </div>
            </div>
          </>
        )}

        {/* ── Revision Request Modal ── */}
        {revisionDrawer && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => { setRevisionDrawer(null); setRevisionNote(""); }} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => { setRevisionDrawer(null); setRevisionNote(""); }}>
            <div className="w-full max-w-lg max-h-[85vh] bg-surface-container-lowest rounded-2xl shadow-2xl flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
              <div className="px-6 py-4 border-b border-outline-variant/20 flex items-center justify-between shrink-0 bg-surface-container-low/30">
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-xl bg-warning-amber/10 text-warning-amber flex items-center justify-center">
                    <span className="material-symbols-outlined text-[18px]">rate_review</span>
                  </div>
                  <div>
                    <h3 className="text-label-sm font-bold text-on-surface">{noteMode === "reject" ? "Reject Asset" : "Request Changes"}</h3>
                    <p className="text-label-xs text-outline">{noteMode === "reject" ? "Add feedback for the content writer" : "Send feedback for AI revision"}</p>
                  </div>
                </div>
                <button onClick={() => { setRevisionDrawer(null); setRevisionNote(""); }} className="p-2 hover:bg-surface-container rounded-lg transition-all">
                  <span className="material-symbols-outlined text-[18px]">close</span>
                </button>
              </div>

              <div className="flex-1 overflow-y-auto p-6 space-y-4">
                <div className="flex items-center gap-4 p-4 rounded-xl bg-surface-container-low border border-outline-variant/20">
                  <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${getTypeStyle(revisionDrawer.type)} flex items-center justify-center text-white shrink-0 shadow-sm`}>
                    <span className="material-symbols-outlined text-[22px]">{getTypeConfig(revisionDrawer.type).icon}</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-body-sm font-bold text-on-surface truncate">{revisionDrawer.title}</p>
                    <div className="flex items-center gap-2 mt-0.5">
                      <span className="text-label-xs font-semibold px-1.5 py-0.5 rounded" style={{ backgroundColor: (getBrandColor(revisionDrawer.brandName) || "#6366f1") + "15", color: getBrandColor(revisionDrawer.brandName) || "#6366f1" }}>
                        {revisionDrawer.brandName}
                      </span>
                      <span className="text-outline/20">·</span>
                      <span className="text-label-xs text-outline">{revisionDrawer.type}</span>
                      <span className="text-outline/20">·</span>
                      <span className="text-label-xs text-outline">{revisionDrawer.productName}</span>
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div className="p-3 rounded-xl bg-surface-container-low border border-outline-variant/10">
                    <label className="text-label-3xs text-outline uppercase font-bold tracking-widest block mb-1">Platforms</label>
                    <div className="flex flex-wrap gap-1">
                      {revisionDrawer.platforms.map((p) => {
                        const cfg = PLATFORM_CONFIG[p];
                        return cfg ? (
                          <span key={p} className="flex items-center gap-0.5 px-1.5 py-0.5 rounded text-label-2xs font-semibold"
                            style={{ backgroundColor: cfg.color + "12", color: cfg.color }}>
                            <PlatformIcon platform={cfg.icon} className="w-[9px] h-[9px]" />
                            {cfg.label}
                          </span>
                        ) : null;
                      })}
                    </div>
                  </div>
                  <div className="p-3 rounded-xl bg-surface-container-low border border-outline-variant/10">
                    <label className="text-label-3xs text-outline uppercase font-bold tracking-widest block mb-1">Priority</label>
                    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-2xs font-bold ${getPriority(revisionDrawer).color}`}>
                      {getPriority(revisionDrawer).label}
                    </span>
                  </div>
                </div>

                <div className="flex items-start gap-3 p-3 rounded-xl bg-gradient-to-r from-warning-amber/5 to-transparent border border-warning-amber/20">
                  <span className="material-symbols-outlined text-warning-amber text-[16px] shrink-0 mt-0.5">info</span>
                  <p className="text-label-xs text-outline leading-relaxed">Your feedback will be sent to AISAM for regeneration. The current version will be rejected and replaced with a new AI-generated version addressing your notes.</p>
                </div>

                <div>
                  <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Quick suggestions</label>
                  <div className="flex flex-wrap gap-1.5">
                    {["Adjust copy", "Change image", "Fix CTA", "Branding issue", "Tone of voice", "Formatting", "Other"].map((s) => (
                      <button key={s} onClick={() => {
                        const current = revisionNote;
                        const prefix = current ? (current.endsWith(".") || current.endsWith("\n") ? " " : ". ") : "";
                        setRevisionNote(current + prefix + s + ".");
                        revisionsRef.current?.focus();
                      }}
                        className="px-2.5 py-1.5 rounded-lg border border-outline-variant/20 text-label-2xs font-semibold text-on-surface-variant hover:bg-surface-container hover:border-outline-variant/40 transition-all active:scale-95">
                        {s}
                      </button>
                    ))}
                  </div>
                </div>

                <div>
                  <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">
                    {noteMode === "reject" ? "Rejection Notes" : "Revision Notes"} <span className="text-danger-red">*</span>
                  </label>
                  <div className="relative">
                    <textarea ref={revisionsRef} value={revisionNote} onChange={(e) => setRevisionNote(e.target.value.slice(0, 500))}
                      placeholder="Describe what needs to be changed — be specific about copy, visuals, tone, or CTA..."
                      className="w-full h-36 bg-surface-container-low border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface placeholder:text-outline/40 focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all resize-none" />
                    <div className="absolute bottom-3 right-3 flex items-center gap-1.5">
                      <div className="h-1 w-20 rounded-full overflow-hidden bg-outline-variant/30">
                        <div className={`h-full rounded-full transition-all duration-300 ${
                          revisionNote.length > 450 ? "bg-danger-red" :
                          revisionNote.length > 400 ? "bg-warning-amber" :
                          "bg-primary/40"
                        }`} style={{ width: `${Math.min(100, (revisionNote.length / 500) * 100)}%` }} />
                      </div>
                      <span className={`text-label-2xs font-semibold ${
                        revisionNote.length >= 500 ? "text-danger-red" :
                        revisionNote.length > 400 ? "text-warning-amber" :
                        "text-outline"
                      }`}>{revisionNote.length}/500</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="px-6 py-4 border-t border-outline-variant/20 bg-surface-container-low/80 backdrop-blur-sm flex items-center gap-3 shrink-0">
                <button onClick={() => { setRevisionDrawer(null); setRevisionNote(""); }}
                  className="px-5 py-2.5 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container rounded-xl transition-all">Cancel</button>
                <button onClick={submitRevision} disabled={revisionNote.trim().length < 5 || actionId === revisionDrawer.id}
                  className="flex-1 bg-gradient-to-r from-warning-amber to-amber-500 text-white py-2.5 rounded-xl text-label-sm font-bold flex items-center justify-center gap-2 hover:from-warning-amber/90 hover:to-amber-500/90 active:scale-[0.98] transition-all disabled:opacity-50 shadow-sm">
                  {actionId === revisionDrawer.id ? (
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : (
                    <span className="material-symbols-outlined text-[16px]">send</span>
                  )}
                  {noteMode === "reject" ? "Reject with Feedback" : "Submit Revision Request"}
                </button>
              </div>
            </div>
            </div>
          </>
        )}

        {/* ── Post Now Modal ── */}
        {postNowItem && (
          <PostNowModal
            contentId={postNowItem.id}
            brandId={postNowItem.brandId}
            onClose={() => setPostNowItem(null)}
            onSuccess={() => {
              setPostNowItem(null);
              showToast(`"${postNowItem.title}" published successfully!`, "success");
              load();
            }}
          />
        )}

        {/* ── Toast ── */}
        {toast && (
          <div className="fixed bottom-8 left-1/2 -translate-x-1/2 z-[100] flex items-center gap-4 bg-inverse-surface text-inverse-on-surface px-6 py-4 rounded-xl shadow-2xl animate-in fade-in slide-in-from-bottom-2 duration-300">
            <div className={`w-9 h-9 rounded-full ${toast.type === "success" ? "bg-emerald-500" : toast.type === "undo" ? "bg-primary" : "bg-danger-red"} flex items-center justify-center shrink-0 shadow-lg`}>
              <span className="material-symbols-outlined text-white text-[16px]">
                {toast.type === "success" ? "check" : toast.type === "undo" ? "undo" : "close"}
              </span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-label-sm font-bold">{toast.message}</p>
            </div>
            {toast.type === "undo" && (
              <button onClick={() => { setToast(null); }}
                className="text-label-sm font-bold text-primary hover:text-primary/80 transition-all">Undo</button>
            )}
            <button onClick={() => setToast(null)} className="p-1 hover:bg-white/10 rounded-full transition-all">
              <span className="material-symbols-outlined text-[14px]">close</span>
            </button>
          </div>
        )}
      </main>
    </>
  );
}
