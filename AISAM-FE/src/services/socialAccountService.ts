import { apiClient } from "@/lib/apiClient";

export type SocialPlatform = "facebook" | "instagram" | "tiktok";
export type AccountStatus = "connected" | "expired" | "error";

export interface SocialTarget {
  id: string;
  providerTargetId: string;
  name: string;
  type: string;
  category: string | null;
  profilePictureUrl: string | null;
  isActive: boolean;
}

export interface SocialAccount {
  id: string;
  profileId: string;
  provider: SocialPlatform;
  providerUserId: string;
  accountName: string;
  accountHandle: string;
  isActive: boolean;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
  targets: SocialTarget[];
  followers: number;
  following: number;
  postsCount: number;
}

export interface SocialIntegration {
  id: string;
  socialAccountId: string;
  targetId: string;
  provider: SocialPlatform;
  accountName: string;
  targetName: string;
  isActive: boolean;
}

export interface AvailableTarget {
  providerTargetId: string;
  name: string;
  type: string;
  category: string | null;
  profilePictureUrl: string | null;
  isActive: boolean;
}

export interface AuthUrlResponse {
  authUrl: string;
  state: string;
}

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

interface SocialIntegrationDto {
  id: string;
  socialAccountId: string;
  targetId: string;
  provider: string;
  accountName: string;
  targetName: string;
  isActive: boolean;
}

const STORAGE_KEY = "aisam_social_accounts_v2";

const INITIAL_MOCK_ACCOUNTS: SocialAccount[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "facebook",
    providerUserId: "fb_user_1001",
    accountName: "Lumina Tech Official",
    accountHandle: "@luminattech",
    isActive: true,
    expiresAt: new Date(Date.now() + 60 * 86400000).toISOString(),
    createdAt: new Date(Date.now() - 90 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 3600000).toISOString(),
    targets: [
      { id: "t1", providerTargetId: "fb_page_1001", name: "Lumina Tech Official", type: "page", category: "Technology", profilePictureUrl: null, isActive: true },
      { id: "t2", providerTargetId: "fb_page_1002", name: "Lumina Tech Community", type: "group", category: "Technology", profilePictureUrl: null, isActive: true },
    ],
    followers: 24500,
    following: 150,
    postsCount: 342,
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "facebook",
    providerUserId: "fb_user_1002",
    accountName: "Summit Outdoor Gear",
    accountHandle: "@summitoutdoor",
    isActive: true,
    expiresAt: new Date(Date.now() + 15 * 86400000).toISOString(),
    createdAt: new Date(Date.now() - 45 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 14400000).toISOString(),
    targets: [
      { id: "t3", providerTargetId: "fb_page_2001", name: "Summit Outdoor Gear", type: "page", category: "Sports", profilePictureUrl: null, isActive: true },
    ],
    followers: 8900,
    following: 320,
    postsCount: 210,
  },
  {
    id: "33333333-3333-3333-3333-333333333333",
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "facebook",
    providerUserId: "fb_user_1003",
    accountName: "Heritage Motors",
    accountHandle: "@heritagemotors",
    isActive: false,
    expiresAt: new Date(Date.now() - 2 * 86400000).toISOString(),
    createdAt: new Date(Date.now() - 200 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 3 * 86400000).toISOString(),
    targets: [
      { id: "t4", providerTargetId: "fb_page_3001", name: "Heritage Motors", type: "page", category: "Automotive", profilePictureUrl: null, isActive: false },
    ],
    followers: 15600,
    following: 95,
    postsCount: 428,
  },
  {
    id: "44444444-4444-4444-4444-444444444444",
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "instagram",
    providerUserId: "ig_user_2001",
    accountName: "Lumina Tech",
    accountHandle: "@luminattech",
    isActive: true,
    expiresAt: new Date(Date.now() + 30 * 86400000).toISOString(),
    createdAt: new Date(Date.now() - 60 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 7200000).toISOString(),
    targets: [
      { id: "t5", providerTargetId: "ig_biz_2001", name: "Lumina Tech", type: "business", category: "Technology", profilePictureUrl: null, isActive: true },
    ],
    followers: 18200,
    following: 420,
    postsCount: 189,
  },
  {
    id: "55555555-5555-5555-5555-555555555555",
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "tiktok",
    providerUserId: "tt_user_3001",
    accountName: "Lumina Tech",
    accountHandle: "@luminattech",
    isActive: true,
    expiresAt: new Date(Date.now() + 90 * 86400000).toISOString(),
    createdAt: new Date(Date.now() - 30 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 1800000).toISOString(),
    targets: [
      { id: "t6", providerTargetId: "tt_biz_3001", name: "Lumina Tech", type: "business", category: "Technology", profilePictureUrl: null, isActive: true },
    ],
    followers: 45800,
    following: 88,
    postsCount: 67,
  },
];

function loadAccounts(): SocialAccount[] {
  if (typeof window === "undefined") return [...INITIAL_MOCK_ACCOUNTS];
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as SocialAccount[];
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    }
  } catch { /* fallback */ }
  const initial = [...INITIAL_MOCK_ACCOUNTS];
  localStorage.setItem(STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

function saveAccounts(accounts: SocialAccount[]): void {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(accounts));
  } catch { /* ignore */ }
}

const MOCK_ACCOUNTS: SocialAccount[] = loadAccounts();

const MOCK_AVAILABLE_TARGETS: AvailableTarget[] = [
  { providerTargetId: "fb_page_new_1", name: "Lumina Tech Support", type: "page", category: "Technology", profilePictureUrl: null, isActive: true },
  { providerTargetId: "fb_page_new_2", name: "Lumina Tech Vietnam", type: "page", category: "Technology", profilePictureUrl: null, isActive: true },
  { providerTargetId: "fb_group_new_1", name: "Smart Home Community", type: "group", category: "Technology", profilePictureUrl: null, isActive: true },
  { providerTargetId: "fb_page_new_3", name: "Summit Outdoor Vietnam", type: "page", category: "Sports", profilePictureUrl: null, isActive: true },
];

export async function fetchSocialAccounts(): Promise<{ data: SocialAccount[]; total: number }> {
  return { data: [...MOCK_ACCOUNTS], total: MOCK_ACCOUNTS.length };
}

export async function getFacebookAuthUrl(): Promise<AuthUrlResponse> {
  return {
    authUrl: `https://www.facebook.com/v18.0/dialog/oauth?client_id=mock_app_id&redirect_uri=${encodeURIComponent(window.location.origin + "/social/callback")}&scope=pages_show_list,pages_read_engagement,pages_manage_posts&state=mock_state_${Date.now()}`,
    state: `mock_state_${Date.now()}`,
  };
}

export async function handleFacebookCallback(): Promise<SocialAccount> {
  const account: SocialAccount = {
    id: crypto.randomUUID(),
    profileId: "00000000-0000-0000-0000-000000000001",
    provider: "facebook",
    providerUserId: `fb_user_${Date.now()}`,
    accountName: "New Facebook Page",
    accountHandle: "@newpage",
    isActive: true,
    expiresAt: new Date(Date.now() + 90 * 86400000).toISOString(),
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    targets: [],
    followers: 0,
    following: 0,
    postsCount: 0,
  };
  MOCK_ACCOUNTS.unshift(account);
  saveAccounts(MOCK_ACCOUNTS);
  return account;
}

export async function getAvailableTargets(accountId: string): Promise<AvailableTarget[]> {
  const account = MOCK_ACCOUNTS.find((a) => a.id === accountId);
  if (!account) return [];
  const linkedIds = (account.targets || []).map((t) => t.providerTargetId);
  return MOCK_AVAILABLE_TARGETS.filter((t) => !linkedIds.includes(t.providerTargetId));
}

export async function getLinkedTargets(accountId: string): Promise<SocialTarget[]> {
  const account = MOCK_ACCOUNTS.find((a) => a.id === accountId);
  return account?.targets || [];
}

export async function linkTargets(accountId: string, targetIds: string[]): Promise<SocialAccount> {
  const idx = MOCK_ACCOUNTS.findIndex((a) => a.id === accountId);
  if (idx < 0) throw new Error("Account not found");

  const newTargets: SocialTarget[] = targetIds.map((id, i) => {
    const available = MOCK_AVAILABLE_TARGETS.find((t) => t.providerTargetId === id);
    return {
      id: `t_${Date.now()}_${i}`,
      providerTargetId: id,
      name: available?.name || `Target ${id}`,
      type: available?.type || "page",
      category: available?.category || null,
      profilePictureUrl: available?.profilePictureUrl || null,
      isActive: true,
    };
  });

  MOCK_ACCOUNTS[idx].targets = [...(MOCK_ACCOUNTS[idx].targets || []), ...newTargets];
  MOCK_ACCOUNTS[idx].updatedAt = new Date().toISOString();
  saveAccounts(MOCK_ACCOUNTS);
  return MOCK_ACCOUNTS[idx];
}

export async function deleteSocialAccount(accountId: string): Promise<boolean> {
  const idx = MOCK_ACCOUNTS.findIndex((a) => a.id === accountId);
  if (idx >= 0) {
    MOCK_ACCOUNTS.splice(idx, 1);
    saveAccounts(MOCK_ACCOUNTS);
  }
  return idx >= 0;
}

export async function fetchSocialIntegrations(brandId?: string): Promise<SocialIntegration[]> {
  try {
    if (brandId) {
      const res: GenericResponse<SocialIntegrationDto[]> = await apiClient(`/social/integrations/brand/${brandId}`);
      if (res?.data) {
        return res.data.map((dto) => ({
          id: dto.id,
          socialAccountId: dto.socialAccountId,
          targetId: dto.targetId,
          provider: dto.provider.toLowerCase() as SocialPlatform,
          accountName: dto.accountName,
          targetName: dto.targetName,
          isActive: dto.isActive,
        }));
      }
    }
  } catch { /* fallback */ }
  
  const accounts = loadAccounts();
  const integrations: SocialIntegration[] = [];
  for (const account of accounts) {
    if (!account.isActive) continue;
    for (const target of account.targets || []) {
      if (!target.isActive) continue;
      integrations.push({
        id: `${account.id}-${target.id}`,
        socialAccountId: account.id,
        targetId: target.id,
        provider: account.provider,
        accountName: account.accountName,
        targetName: target.name,
        isActive: true,
      });
    }
  }
  return integrations;
}

export function getAccountStatus(account: SocialAccount): AccountStatus {
  if (!account.isActive) return "error";
  if (!account.expiresAt) return "connected";
  if (new Date(account.expiresAt) < new Date()) return "expired";
  return "connected";
}
