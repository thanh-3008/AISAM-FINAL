import { type AdCampaignDto, type AdCampaignObjective, type AdCampaignStatus } from "@/services/adCampaignService";

export const CAMPAIGN_OBJECTIVES: AdCampaignObjective[] = [
  "AWARENESS",
  "TRAFFIC",
  "ENGAGEMENT",
  "LEADS",
  "SALES",
  "APP_PROMOTION",
];

export const CAMPAIGN_STATUSES: AdCampaignStatus[] = ["ACTIVE", "PAUSED", "COMPLETED", "DRAFT"];

export const objectiveLabels: Record<string, string> = {
  AWARENESS: "Awareness",
  TRAFFIC: "Traffic",
  ENGAGEMENT: "Engagement",
  LEADS: "Leads",
  SALES: "Sales",
  APP_PROMOTION: "App Promotion",
};

export const objectiveIcons: Record<string, string> = {
  AWARENESS: "visibility",
  TRAFFIC: "link",
  ENGAGEMENT: "favorite",
  LEADS: "person_add",
  SALES: "shopping_cart",
  APP_PROMOTION: "smartphone",
};

export function statusClass(status: string) {
  switch (status) {
    case "ACTIVE":
      return "bg-emerald-50 text-emerald-700 border-emerald-200";
    case "PAUSED":
      return "bg-amber-50 text-amber-700 border-amber-200";
    case "COMPLETED":
      return "bg-blue-50 text-blue-700 border-blue-200";
    default:
      return "bg-surface-container text-on-surface-variant border-outline-variant/20";
  }
}

export function formatMoney(value?: number | null) {
  if (!value) return "-";
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(value);
}

export function formatDate(value?: string | null) {
  if (!value) return "-";
  return new Date(value).toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" });
}

export function toDateInputValue(value?: string | null) {
  if (!value) return "";
  return value.split("T")[0];
}

export function campaignSpend(campaign: AdCampaignDto) {
  return campaign.spend ?? 0;
}

export function campaignImpressions(campaign: AdCampaignDto) {
  return campaign.impressions ?? 0;
}

export function campaignClicks(campaign: AdCampaignDto) {
  return campaign.clicks ?? 0;
}

export function campaignConversions(campaign: AdCampaignDto) {
  return campaign.conversions ?? 0;
}
