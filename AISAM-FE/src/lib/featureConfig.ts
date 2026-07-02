export enum PlanType {
  Free = 0,
  PersonalPlus = 1,
  PersonalPro = 2,
  BusinessPlus = 3,
  BusinessPro = 4,
}

export const PLAN_NAMES: Record<PlanType, string> = {
  [PlanType.Free]: "Free",
  [PlanType.PersonalPlus]: "Personal Plus",
  [PlanType.PersonalPro]: "Personal Pro",
  [PlanType.BusinessPlus]: "Business Plus",
  [PlanType.BusinessPro]: "Business Pro",
};

export enum WorkspaceRole {
  Owner = "Owner",
  Manager = "Manager",
  ContentCreator = "ContentCreator",
  Viewer = "Viewer",
}

export type Feature =
  | "generateText"
  | "manualPost"
  | "basicAnalytics"
  | "aiImage"
  | "contentCalendar"
  | "schedulePost"
  | "multiPlatformPublish"
  | "trendAnalysis"
  | "holidaySuggestion"
  | "aiVideo"
  | "advancedAnalytics"
  | "campaignRecommendation"
  | "teamManagement"
  | "sharedCredits"
  | "sharedWorkspace"
  | "workspaceDashboard"
  | "lifetimeAssignedLimit"
  | "monthlyAssignedLimit"
  | "creditUsageReport"
  | "topMemberAnalytics";

export type Permission =
  | "viewDashboard"
  | "viewAnalytics"
  | "manageBrand"
  | "manageProduct"
  | "manageContent"
  | "manageCampaign"
  | "generateContent"
  | "generateImage"
  | "generateVideo"
  | "createDraft"
  | "publishPost"
  | "viewTeamUsage"
  | "inviteMember"
  | "removeMember"
  | "assignQuota"
  | "manageBilling"
  | "manageSubscription"
  | "transferOwnership";

export const PLAN_HIERARCHY: Record<PlanType, number> = {
  [PlanType.Free]: 0,
  [PlanType.PersonalPlus]: 1,
  [PlanType.PersonalPro]: 2,
  [PlanType.BusinessPlus]: 3,
  [PlanType.BusinessPro]: 4,
};

export const FEATURE_MATRIX: Record<Feature, PlanType[]> = {
  generateText: [PlanType.Free, PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  manualPost: [PlanType.Free, PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  basicAnalytics: [PlanType.Free, PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  aiImage: [PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  contentCalendar: [PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  schedulePost: [PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  multiPlatformPublish: [PlanType.PersonalPlus, PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  trendAnalysis: [PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  holidaySuggestion: [PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  aiVideo: [PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  advancedAnalytics: [PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  campaignRecommendation: [PlanType.PersonalPro, PlanType.BusinessPlus, PlanType.BusinessPro],
  teamManagement: [PlanType.BusinessPlus, PlanType.BusinessPro],
  sharedCredits: [PlanType.BusinessPlus, PlanType.BusinessPro],
  sharedWorkspace: [PlanType.BusinessPlus, PlanType.BusinessPro],
  workspaceDashboard: [PlanType.BusinessPlus, PlanType.BusinessPro],
  lifetimeAssignedLimit: [PlanType.BusinessPro],
  monthlyAssignedLimit: [PlanType.BusinessPro],
  creditUsageReport: [PlanType.BusinessPro],
  topMemberAnalytics: [PlanType.BusinessPro],
};

export const PERMISSION_MATRIX: Record<Permission, WorkspaceRole[]> = {
  viewDashboard: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator, WorkspaceRole.Viewer],
  viewAnalytics: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator, WorkspaceRole.Viewer],
  manageBrand: [WorkspaceRole.Owner, WorkspaceRole.Manager],
  manageProduct: [WorkspaceRole.Owner, WorkspaceRole.Manager],
  manageContent: [WorkspaceRole.Owner, WorkspaceRole.Manager],
  manageCampaign: [WorkspaceRole.Owner, WorkspaceRole.Manager],
  generateContent: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator],
  generateImage: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator],
  generateVideo: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator],
  createDraft: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator],
  publishPost: [WorkspaceRole.Owner, WorkspaceRole.Manager, WorkspaceRole.ContentCreator],
  viewTeamUsage: [WorkspaceRole.Owner, WorkspaceRole.Manager],
  inviteMember: [WorkspaceRole.Owner],
  removeMember: [WorkspaceRole.Owner],
  assignQuota: [WorkspaceRole.Owner],
  manageBilling: [WorkspaceRole.Owner],
  manageSubscription: [WorkspaceRole.Owner],
  transferOwnership: [WorkspaceRole.Owner],
};

export const CREDIT_COST: Record<string, number> = {
  generateText: 1,
  regenerate: 1,
  refine: 1,
  generateImage: 5,
  generateVideo: 20,
  trendContent: 2,
  campaignRecommendation: 2,
};

export function getPlanType(planName: string): PlanType {
  const normalized = planName.toLowerCase().replace(/\s+/g, "");
  if (normalized.includes("businesspro")) return PlanType.BusinessPro;
  if (normalized.includes("businessplus")) return PlanType.BusinessPlus;
  if (normalized.includes("personalpro")) return PlanType.PersonalPro;
  if (normalized.includes("personalplus")) return PlanType.PersonalPlus;
  // BE may return "Premium" or "Pro" without workspace prefix
  if (normalized === "premium" || normalized === "pro") return PlanType.PersonalPro;
  if (normalized === "plus") return PlanType.PersonalPlus;
  return PlanType.Free;
}

export function getWorkspacePlanType(planName: string, workspaceType?: number | null): PlanType {
  const normalized = planName.toLowerCase().replace(/\s+/g, "");

  if (workspaceType === 2) {
    if (normalized.includes("businesspro") || normalized === "premium" || normalized === "pro") return PlanType.BusinessPro;
    if (normalized.includes("businessplus") || normalized === "plus") return PlanType.BusinessPlus;
    // Generic "business" name from workspace fetch — preserve current tier if subscription API resolves later
    if (normalized === "business") return PlanType.BusinessPlus;
  }

  if (workspaceType === 1) {
    if (normalized.includes("personalpro") || normalized === "premium" || normalized === "pro") return PlanType.PersonalPro;
    if (normalized.includes("personalplus") || normalized === "plus") return PlanType.PersonalPlus;
    // Generic "personal" name from workspace fetch — treat as at least Plus so paid features aren't locked during plan resolution
    if (normalized === "personal") return PlanType.PersonalPlus;
  }

  return getPlanType(planName);
}

export function canAccessFeature(plan: PlanType, feature: Feature): boolean {
  const allowedPlans = FEATURE_MATRIX[feature];
  if (!allowedPlans) return false;
  return allowedPlans.some(p => PLAN_HIERARCHY[p] <= PLAN_HIERARCHY[plan]);
}

export function hasPermission(role: WorkspaceRole | string, permission: Permission): boolean {
  if (!role) return false;
  const allowedRoles = PERMISSION_MATRIX[permission];
  if (!allowedRoles) return false;
  return allowedRoles.includes(role as WorkspaceRole);
}
