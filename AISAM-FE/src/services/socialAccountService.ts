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
  data?: T;
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

interface SocialAccountDto {
  id: string;
  profileId: string;
  provider: string;
  providerUserId: string;
  isActive: boolean;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
  targets: SocialTarget[];
}

function mapSocialAccount(dto: SocialAccountDto): SocialAccount {
  const provider = dto.provider.toLowerCase() as SocialPlatform;
  return {
    ...dto,
    provider,
    accountName: dto.providerUserId || `${provider} account`,
    accountHandle: dto.providerUserId ? `@${dto.providerUserId}` : "",
    targets: dto.targets ?? [],
    followers: 0,
    following: 0,
    postsCount: 0,
  };
}

export async function fetchSocialAccounts(): Promise<{ data: SocialAccount[]; total: number }> {
  const res: GenericResponse<SocialAccountDto[]> = await apiClient("/social/accounts/me");
  const data = res.data?.map(mapSocialAccount) ?? [];
  return { data, total: data.length };
}

export async function getFacebookAuthUrl(): Promise<AuthUrlResponse> {
  const res: GenericResponse<AuthUrlResponse> = await apiClient("/social-auth/facebook");
  if (!res.data?.authUrl) throw new Error("Facebook authorization URL was not returned.");
  return res.data;
}

export async function handleFacebookCallback(data?: { code?: string; state?: string }): Promise<SocialAccount> {
  if (!data?.code || !data.state) throw new Error("Missing Facebook callback code or state.");
  const res: GenericResponse<SocialAccountDto> = await apiClient("/social-auth/facebook/callback", {
    method: "POST",
    data,
  });
  if (!res.data) throw new Error("Facebook account connection failed.");
  return mapSocialAccount(res.data);
}

export async function getAvailableTargets(accountId: string): Promise<AvailableTarget[]> {
  const res: GenericResponse<AvailableTarget[]> = await apiClient(`/social/accounts/${accountId}/available-targets`);
  return res.data ?? [];
}

export async function getLinkedTargets(accountId: string): Promise<SocialTarget[]> {
  const res: GenericResponse<SocialTarget[]> = await apiClient(`/social/accounts/${accountId}/linked-targets`);
  return res.data ?? [];
}

export async function linkTargets(
  accountId: string,
  request: { targetIds: string[]; brandId: string; provider?: SocialPlatform },
): Promise<SocialAccount> {
  const res: GenericResponse<SocialAccountDto> = await apiClient(`/social/accounts/${accountId}/link-targets`, {
    method: "POST",
    data: {
      provider: request.provider ?? "facebook",
      providerTargetIds: request.targetIds,
      brandId: request.brandId,
    },
  });
  if (!res.data) throw new Error("Failed to link social targets.");
  return mapSocialAccount(res.data);
}

export async function deleteSocialAccount(accountId: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/social/accounts/${accountId}`, { method: "DELETE" });
  return Boolean(res.success);
}

export async function deleteIntegration(integrationId: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/social/integrations/${integrationId}`, { method: "DELETE" });
  return Boolean(res.success);
}

export async function fetchSocialIntegrations(brandId?: string): Promise<SocialIntegration[]> {
  if (!brandId) return [];
  const res: GenericResponse<SocialIntegrationDto[]> = await apiClient(`/social/integrations/brand/${brandId}`);
  return (res.data ?? []).map((dto) => ({
    ...dto,
    provider: dto.provider.toLowerCase() as SocialPlatform,
  }));
}

export function getAccountStatus(account: SocialAccount): AccountStatus {
  if (!account.isActive) return "error";
  if (!account.expiresAt) return "connected";
  if (new Date(account.expiresAt) < new Date()) return "expired";
  return "connected";
}
