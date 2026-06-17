import { useId } from "react";

export type ContentType = "IMAGE" | "TEXT" | "VIDEO";
export type ContentStatus = "Draft" | "Awaiting Approval" | "Approved" | "Rejected" | "Scheduled" | "Published";

export const PLATFORM_CONFIG: Record<string, { color: string; icon: string; label: string }> = {
  facebook: { color: "#1877F2", icon: "facebook", label: "Facebook" },
  instagram: { color: "#DD2A7B", icon: "instagram", label: "Instagram" },
  tiktok: { color: "#111111", icon: "tiktok", label: "TikTok" },
};

export const ALL_PLATFORMS = Object.keys(PLATFORM_CONFIG);

const BRAND_COLOR_PALETTE = [
  "#6366f1", "#059669", "#dc2626", "#2563eb", "#d97706",
  "#7c3aed", "#db2777", "#0891b2", "#65a30d", "#ca8a04",
];

export function getBrandColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = ((hash << 5) - hash) + name.charCodeAt(i);
    hash |= 0;
  }
  return BRAND_COLOR_PALETTE[Math.abs(hash) % BRAND_COLOR_PALETTE.length];
}

export const CONTENT_TYPES: { label: string; value: ContentType; icon: string; color: string }[] = [
  { label: "Image", value: "IMAGE", icon: "image", color: "from-blue-500/20 to-blue-600/10 text-blue-500" },
  { label: "Text", value: "TEXT", icon: "article", color: "from-purple-500/20 to-purple-600/10 text-purple-500" },
  { label: "Video", value: "VIDEO", icon: "play_circle", color: "from-rose-500/20 to-rose-600/10 text-rose-500" },
];

export const STATUS_OPTIONS: { label: string; value: ContentStatus }[] = [
  { label: "Draft", value: "Draft" },
  { label: "Awaiting Approval", value: "Awaiting Approval" },
  { label: "Approved", value: "Approved" },
  { label: "Rejected", value: "Rejected" },
  { label: "Scheduled", value: "Scheduled" },
  { label: "Published", value: "Published" },
];

export const STATUS_STYLES: Record<ContentStatus, string> = {
  "Draft": "bg-surface-container-high text-on-surface-variant",
  "Awaiting Approval": "bg-amber-50 text-amber-600",
  "Approved": "bg-emerald-50 text-emerald-600",
  "Rejected": "bg-danger-red/10 text-danger-red",
  "Scheduled": "bg-blue-50 text-blue-600",
  "Published": "bg-blue-50 text-blue-600",
};

export const ALL_TAGS = ["Product Launch", "Tutorial", "Seasonal", "Brand Story", "Behind the Scenes", "Testimonial", "Promotion", "Educational"];

export function getTypeStyle(type: ContentType) {
  switch (type) {
    case "IMAGE": return "from-blue-500 to-blue-400";
    case "TEXT": return "from-purple-500 to-purple-400";
    case "VIDEO": return "from-rose-500 to-rose-400";
  }
}

export function getTypeBadgeStyle(type: ContentType) {
  switch (type) {
    case "IMAGE": return "bg-blue-500/80";
    case "TEXT": return "bg-purple-500/80";
    case "VIDEO": return "bg-rose-500/80";
  }
}

export function getTypeIcon(type: ContentType) {
  switch (type) {
    case "IMAGE": return "image";
    case "TEXT": return "article";
    case "VIDEO": return "play_circle";
  }
}

export function getTypeConfig(type: ContentType) {
  return CONTENT_TYPES.find((t) => t.value === type) || CONTENT_TYPES[0];
}

export function PlatformIcon({ platform, className }: { platform: string; className?: string }) {
  const uid = useId();
  const cls = className || "w-[12px] h-[12px]";
  switch (platform) {
    case "facebook":
      return (
        <svg viewBox="0 0 40 40" className={cls} fill="none">
          <rect width="40" height="40" rx="8" fill="#1877F2"/>
          <path d="M30 20c0-5.523-4.477-10-10-10s-10 4.477-10 10c0 4.991 3.657 9.128 8.438 9.878v-6.988h-2.54V20h2.54v-2.203c0-2.507 1.493-3.891 3.777-3.891 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.242 0-1.63.771-1.63 1.562V20h2.773l-.443 2.89h-2.33v6.988C26.343 29.128 30 24.991 30 20z" fill="white"/>
        </svg>
      );
    case "instagram":
      const gradId = `ig-grad-${uid}`;
      return (
        <svg viewBox="0 0 24 24" className={cls}>
          <defs>
            <linearGradient id={gradId} x1="0" y1="1" x2="1" y2="0">
              <stop stopColor="#F58529"/><stop offset="0.5" stopColor="#DD2A7B"/><stop offset="1" stopColor="#8134AF"/>
            </linearGradient>
          </defs>
          <rect width="24" height="24" rx="6" fill={`url(#${gradId})`}/>
          <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zM12 0C8.741 0 8.333.014 7.053.072 2.695.272.273 2.69.073 7.052.014 8.333 0 8.741 0 12c0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98C8.333 23.986 8.741 24 12 24c3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98C15.668.014 15.259 0 12 0zm0 5.838a6.162 6.162 0 100 12.324 6.162 6.162 0 000-12.324zM12 16a4 4 0 110-8 4 4 0 010 8zm6.406-11.845a1.44 1.44 0 100 2.881 1.44 1.44 0 000-2.881z" fill="white"/>
        </svg>
      );
    case "tiktok":
      return (
        <svg viewBox="0 0 40 40" className={cls} fill="none">
          <rect width="40" height="40" rx="8" fill="#111111"/>
          <path d="M27.5 15.4c-1.7 0-3.3-.6-4.6-1.5l-.02-.02c-.32-.28-.62-.6-.9-.95v8.57c0 .18-.02.36-.04.54-.04.28-.1.55-.18.8-.15.46-.36.88-.63 1.25-.43.6-.96 1.07-1.53 1.44l-.01.01c-.48.32-1.02.57-1.6.72-.27.07-.55.12-.84.15-.19.02-.38.03-.57.03-1.93 0-3.65-1.1-4.43-2.78-.35-.76-.48-1.58-.39-2.42.02-.19.05-.38.09-.56.13-.6.36-1.18.7-1.7.56-.87 1.33-1.54 2.24-1.95.52-.24 1.07-.38 1.63-.42.16-.02.32-.02.48-.02v3.5c-.16 0-.32.02-.48.05-.37.06-.72.2-1.03.41-.35.23-.64.54-.84.9-.17.3-.27.64-.3.98-.06.6.12 1.2.48 1.67.32.43.77.74 1.27.9.22.07.44.1.67.1 1.2 0 2.23-.88 2.44-2.04.03-.15.05-.3.05-.46V12h3.76c-.02.1-.04.2-.05.3-.02.23-.02.47-.02.7 0 1.4.56 2.66 1.48 3.58.42.43.92.78 1.48 1.02.4.17.83.28 1.28.32v3.48z" fill="white"/>
        </svg>
      );
    default:
      return <span className={`material-symbols-outlined ${className || "text-[12px]"}`}>public</span>;
  }
}
