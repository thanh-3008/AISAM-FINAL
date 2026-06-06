import { useId } from "react";

export type ContentType = "IMAGE" | "TEXT" | "VIDEO";
export type ContentStatus = "Published" | "Scheduled" | "Draft" | "Awaiting Approval";

export const PLATFORM_CONFIG: Record<string, { color: string; icon: string; label: string }> = {
  facebook: { color: "#1877F2", icon: "facebook", label: "Facebook" },
  instagram: { color: "#E4405F", icon: "instagram", label: "Instagram" },
  tiktok: { color: "#111111", icon: "tiktok", label: "TikTok" },
};

export const ALL_PLATFORMS = Object.keys(PLATFORM_CONFIG);

export const BRANDS = ["Lumina Tech", "Summit Outdoor", "Heritage Motors", "GreenLeaf Organics", "Pulse Finance"];

export const PRODUCTS: Record<string, string[]> = {
  "Lumina Tech": ["Smart Bulb", "LED Strip", "Desk Lamp"],
  "Summit Outdoor": ["Tent", "Backpack", "Jacket"],
  "Heritage Motors": ["Engine Kit", "Tire Set"],
  "GreenLeaf Organics": ["Organic Tea", "Vitamin Pack"],
  "Pulse Finance": ["Budget App", "Portfolio Tracker"],
};

export const BRAND_COLORS: Record<string, string> = {
  "Lumina Tech": "#6366f1",
  "Summit Outdoor": "#059669",
  "Heritage Motors": "#dc2626",
  "GreenLeaf Organics": "#16a34a",
  "Pulse Finance": "#2563eb",
};

export const CONTENT_TYPES: { label: string; value: ContentType; icon: string; color: string }[] = [
  { label: "Image", value: "IMAGE", icon: "image", color: "from-blue-500/20 to-blue-600/10 text-blue-500" },
  { label: "Text", value: "TEXT", icon: "article", color: "from-purple-500/20 to-purple-600/10 text-purple-500" },
  { label: "Video", value: "VIDEO", icon: "play_circle", color: "from-rose-500/20 to-rose-600/10 text-rose-500" },
];

export const STATUS_OPTIONS: { label: string; value: ContentStatus }[] = [
  { label: "Published", value: "Published" },
  { label: "Scheduled", value: "Scheduled" },
  { label: "Draft", value: "Draft" },
  { label: "Awaiting Approval", value: "Awaiting Approval" },
];

export const STATUS_STYLES: Record<ContentStatus, string> = {
  "Published": "bg-emerald-50 text-emerald-600",
  "Scheduled": "bg-blue-50 text-blue-600",
  "Draft": "bg-surface-container-high text-on-surface-variant",
  "Awaiting Approval": "bg-amber-50 text-amber-600",
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
        <svg viewBox="0 0 40 40" className={cls} fill="none">
          <rect width="40" height="40" rx="10" fill={`url(#${gradId})`}/>
          <defs>
            <linearGradient id={gradId} x1="0" y1="0" x2="40" y2="40">
              <stop stopColor="#F58529"/><stop offset="0.25" stopColor="#DD2A7B"/><stop offset="0.5" stopColor="#8134AF"/><stop offset="0.75" stopColor="#515BD4"/><stop offset="1" stopColor="#1877F2"/>
            </linearGradient>
          </defs>
          <rect x="9" y="9" width="22" height="22" rx="6" stroke="white" strokeWidth="2"/>
          <circle cx="20" cy="20" r="6.5" stroke="white" strokeWidth="2"/>
          <circle cx="26.5" cy="13.5" r="1.5" fill="white"/>
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
