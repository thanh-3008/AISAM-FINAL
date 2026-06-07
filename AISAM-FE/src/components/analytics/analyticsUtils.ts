import { type CampaignPerformance } from "@/services/analyticsService";

export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
}

export function formatNumber(n: number): string {
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M`;
  if (n >= 1000) return `${(n / 1000).toFixed(1)}K`;
  return n.toString();
}

export function formatPercent(n: number): string {
  return `${n.toFixed(2)}%`;
}

export function getRoasColor(roas: number): { bg: string; text: string } {
  if (roas >= 4) return { bg: "bg-success-green/10", text: "text-success-green" };
  if (roas >= 2) return { bg: "bg-warning-amber/10", text: "text-on-surface" };
  return { bg: "bg-danger-red/10", text: "text-danger-red" };
}

export function getStatusColor(status: CampaignPerformance["status"]): string {
  switch (status) {
    case "active":
      return "bg-success-green";
    case "paused":
      return "bg-warning-amber";
    case "completed":
      return "bg-outline";
  }
}

export function getTrendIcon(trend: number): string {
  return trend >= 0 ? "trending_up" : "trending_down";
}

export function getTrendColor(trend: number): string {
  if (trend > 0) return "text-success-green";
  if (trend < 0) return "text-danger-red";
  return "text-outline";
}

export function getTrendLabel(trend: number, suffix = "%"): string {
  const sign = trend >= 0 ? "+" : "";
  return `${sign}${trend}${suffix} trend`;
}

export const DATE_RANGE_OPTIONS = [
  { value: "7d", label: "Last 7 Days" },
  { value: "30d", label: "Last 30 Days" },
  { value: "90d", label: "Last 90 Days" },
  { value: "custom", label: "Custom Range" },
] as const;

export const CAMPAIGN_OPTIONS = [
  { value: "all", label: "All Campaigns" },
  { value: "active", label: "Active Only" },
  { value: "paused", label: "Paused Only" },
  { value: "completed", label: "Completed Only" },
] as const;

export const BRAND_OPTIONS = [
  { value: "all", label: "All Brands" },
  { value: "meta", label: "Meta Brand" },
  { value: "lumina", label: "Lumina Tech" },
  { value: "summit", label: "Summit Outdoor" },
  { value: "heritage", label: "Heritage Motors" },
] as const;

export const PLATFORM_OPTIONS = [
  { value: "facebook", label: "Facebook", color: "#1877F2" },
  { value: "instagram", label: "Instagram", color: "#E4405F" },
  { value: "tiktok", label: "TikTok", color: "#111111" },
] as const;
