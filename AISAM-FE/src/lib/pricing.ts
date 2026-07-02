import { PlanType, PLAN_NAMES, type Feature, FEATURE_MATRIX } from "@/lib/featureConfig";

export interface PlanPricing {
  planType: PlanType;
  name: string;
  price: number;
  priceFormatted: string;
  period: string;
  credits: number;
  postQuota: string;
  popular?: boolean;
  features: string[];
  cta: string;
  category: "personal" | "business";
}

export interface CreditPackPricing {
  id: string;
  name: string;
  credits: number;
  price: number;
  priceFormatted: string;
  popular?: boolean;
  icon: string;
}

export const PLAN_PRICING: PlanPricing[] = [
  {
    planType: PlanType.Free,
    name: PLAN_NAMES[PlanType.Free],
    price: 0,
    priceFormatted: "$0",
    period: "/month",
    credits: 50,
    postQuota: "20 posts/week",
    category: "personal",
    features: [
      "Generate Text",
      "Manual Post",
      "Basic Analytics",
      "50 AI Credits (reset every 7 days)",
      "20 Posts/week",
    ],
    cta: "Current Plan",
  },
  {
    planType: PlanType.PersonalPlus,
    name: PLAN_NAMES[PlanType.PersonalPlus],
    price: 2000,
    priceFormatted: "2,000₫",
    period: "/month",
    credits: 500,
    postQuota: "300 posts/month",
    popular: true,
    category: "personal",
    features: [
      "All Free features",
      "AI Image Generation",
      "Content Calendar",
      "Schedule Post",
      "Multi Platform Publish",
      "500 Credits",
      "300 Posts/month",
    ],
    cta: "Upgrade",
  },
  {
    planType: PlanType.PersonalPro,
    name: PLAN_NAMES[PlanType.PersonalPro],
    price: 3000,
    priceFormatted: "3,000₫",
    period: "/month",
    credits: 2000,
    postQuota: "1,000 posts/month",
    category: "personal",
    features: [
      "All Personal Plus features",
      "Trend Analysis",
      "Holiday Suggestion",
      "AI Video Generation",
      "Advanced Analytics",
      "Campaign Recommendation",
      "2,000 Credits",
      "1,000 Posts/month",
    ],
    cta: "Upgrade",
  },
  {
    planType: PlanType.BusinessPlus,
    name: PLAN_NAMES[PlanType.BusinessPlus],
    price: 4000,
    priceFormatted: "4,000₫",
    period: "/month",
    credits: 15000,
    postQuota: "5,000 posts/month",
    category: "business",
    features: [
      "All Personal Pro features",
      "Team Management",
      "Shared Credits Pool",
      "Shared Workspace",
      "Workspace Dashboard",
      "Up to 10 Team Members",
      "15,000 Credits",
      "5,000 Posts/month",
    ],
    cta: "Upgrade",
  },
  {
    planType: PlanType.BusinessPro,
    name: PLAN_NAMES[PlanType.BusinessPro],
    price: 5000,
    priceFormatted: "5,000₫",
    period: "/month",
    credits: 50000,
    postQuota: "20,000 posts/month",
    category: "business",
    features: [
      "All Business Plus features",
      "Lifetime Assigned Limit",
      "Monthly Assigned Limit",
      "Credit Usage Report",
      "Top Member Analytics",
      "Up to 50 Team Members",
      "50,000 Credits",
      "20,000 Posts/month",
    ],
    cta: "Upgrade",
  },
];

export const CREDIT_PACK_PRICING: CreditPackPricing[] = [
  { id: "starter", name: "Starter", credits: 100, price: 2000, priceFormatted: "2,000₫", icon: "bolt" },
  { id: "standard", name: "Standard", credits: 500, price: 3000, priceFormatted: "3,000₫", icon: "electric_bolt", popular: true },
  { id: "growth", name: "Growth", credits: 1500, price: 4000, priceFormatted: "4,000₫", icon: "local_fire_department" },
  { id: "business", name: "Business", credits: 5000, price: 5000, priceFormatted: "5,000₫", icon: "whatshot" },
];

export function getPlanByType(planType: PlanType): PlanPricing | undefined {
  return PLAN_PRICING.find(p => p.planType === planType);
}

export function getPlanFeatures(planType: PlanType): Feature[] {
  return (Object.keys(FEATURE_MATRIX) as Feature[]).filter(f =>
    FEATURE_MATRIX[f].includes(planType)
  );
}
