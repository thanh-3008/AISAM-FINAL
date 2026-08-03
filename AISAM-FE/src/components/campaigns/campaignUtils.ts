import { type Campaign, type CampaignStatus, type CampaignObjective } from "@/services/campaignService";

export const OBJECTIVE_CONFIG: Record<CampaignObjective, { label: string; icon: string; color: string; bg: string }> = {
  AWARENESS: { label: "Awareness", icon: "visibility", color: "text-blue-600", bg: "bg-blue-50" },
  TRAFFIC: { label: "Traffic", icon: "link", color: "text-cyan-600", bg: "bg-cyan-50" },
  ENGAGEMENT: { label: "Engagement", icon: "favorite", color: "text-pink-600", bg: "bg-pink-50" },
  LEADS: { label: "Leads", icon: "person_add", color: "text-purple-600", bg: "bg-purple-50" },
  SALES: { label: "Sales", icon: "shopping_cart", color: "text-emerald-600", bg: "bg-emerald-50" },
  APP_PROMOTION: { label: "App Promotion", icon: "smartphone", color: "text-orange-600", bg: "bg-orange-50" },
};

export const STATUS_CONFIG: Record<CampaignStatus, { label: string; color: string; bg: string; dot: string }> = {
  ACTIVE: { label: "Active", color: "text-emerald-600", bg: "bg-emerald-50 border-emerald-200/40", dot: "bg-emerald-500" },
  PAUSED: { label: "Paused", color: "text-amber-600", bg: "bg-amber-50 border-amber-200/40", dot: "bg-amber-500" },
  COMPLETED: { label: "Completed", color: "text-blue-600", bg: "bg-blue-50 border-blue-200/40", dot: "bg-blue-500" },
  DRAFT: { label: "Draft", color: "text-outline", bg: "bg-surface-container-high border-outline-variant/20", dot: "bg-outline" },
  PENDING_REVIEW: { label: "Pending Review", color: "text-purple-600", bg: "bg-purple-50 border-purple-200/40", dot: "bg-purple-500" },
  REJECTED: { label: "Rejected", color: "text-danger-red", bg: "bg-red-50 border-red-200/40", dot: "bg-danger-red" },
};

export interface BrandOption {
  id: string;
  name: string;
}

let brandCache: BrandOption[] | null = null;

export function setCachedBrands(brands: BrandOption[]) {
  brandCache = brands;
}

export function getCachedBrands(): BrandOption[] {
  return brandCache ?? [];
}

export function formatCurrency(amount: number, currency = "VND"): string {
  const num = new Intl.NumberFormat("vi-VN", { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(amount);
  return currency === "USD" ? `$${num}` : `${num}đ`;
}

export function formatNumber(n: number): string {
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M`;
  if (n >= 1000) return `${(n / 1000).toFixed(1)}K`;
  return n.toString();
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

export function formatDateShort(iso: string | null): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short" });
}

export function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "Just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export function getDaysRemaining(endDate: string | null): number | null {
  if (!endDate) return null;
  return Math.ceil((new Date(endDate).getTime() - Date.now()) / 86400000);
}

export function getBudgetProgress(campaign: Campaign): number {
  if (!campaign.budget || campaign.budget === 0) return 0;
  return Math.min(100, Math.round((campaign.spend / campaign.budget) * 100));
}

export function getCtr(campaign: Campaign): string {
  if (campaign.impressions === 0) return "0%";
  return `${((campaign.clicks / campaign.impressions) * 100).toFixed(2)}%`;
}

export function getCpa(campaign: Campaign): string {
  if (campaign.conversions === 0) return "—";
  return formatCurrency(campaign.spend / campaign.conversions, campaign.adAccountCurrency || undefined);
}
