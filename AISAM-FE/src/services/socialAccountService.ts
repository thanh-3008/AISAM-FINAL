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
  profileId?: string;
  brandName?: string | null;
  targetId: string;
  provider: SocialPlatform;
  accountName: string;
  targetName: string;
  isActive: boolean;
  brandId: string;
  createdAt?: string;
  updatedAt?: string;
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

// BE DTOs
interface BESocialAccountDto {
  id: string;
  profileId: string;
  provider: string;
  providerUserId: string;
  isActive: boolean;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
  targets: BESocialTargetDto[];
}

interface BESocialTargetDto {
  id: string;
  providerTargetId: string;
  name: string;
  type: string;
  category: string | null;
  profilePictureUrl: string | null;
  isActive: boolean;
}

interface BEAvailableTargetDto {
  providerTargetId: string;
  name: string;
  type: string;
  category: string | null;
  profilePictureUrl: string | null;
  isActive: boolean;
}

interface BEAuthUrlResponse {
  authUrl: string;
  state: string;
}

interface BESocialIntegrationDto {
  id: string;
  socialAccountId: string;
  profileId?: string;
  brandId?: string;
  brandName?: string | null;
  externalId: string;
  name: string;
  platform: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

interface BECallbackRequest {
  code: string;
  state: string;
}

interface BELinkTargetsRequest {
  profileId?: string;
  provider: string;
  providerTargetIds: string[];
  brandId: string;
}

// Mappers
function mapSocialAccount(dto: BESocialAccountDto): SocialAccount {
  return {
    id: dto.id,
    profileId: dto.profileId,
    provider: dto.provider.toLowerCase() as SocialPlatform,
    providerUserId: dto.providerUserId,
    accountName: dto.providerUserId,
    accountHandle: dto.providerUserId,
    isActive: dto.isActive,
    expiresAt: dto.expiresAt || null,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
    targets: (dto.targets || []).map(mapSocialTarget),
    followers: 0,
    following: 0,
    postsCount: 0,
  };
}

function mapSocialTarget(dto: BESocialTargetDto): SocialTarget {
  return {
    id: dto.id,
    providerTargetId: dto.providerTargetId,
    name: dto.name,
    type: dto.type,
    category: dto.category || null,
    profilePictureUrl: dto.profilePictureUrl || null,
    isActive: dto.isActive,
  };
}

function mapAvailableTarget(dto: BEAvailableTargetDto): AvailableTarget {
  return {
    providerTargetId: dto.providerTargetId,
    name: dto.name,
    type: dto.type,
    category: dto.category || null,
    profilePictureUrl: dto.profilePictureUrl || null,
    isActive: dto.isActive,
  };
}

export async function fetchSocialAccounts(): Promise<{ data: SocialAccount[]; total: number }> {
  const res: GenericResponse<BESocialAccountDto[]> = await apiClient("/social/accounts/me");
  if (res?.data) {
    const accounts = res.data.map(mapSocialAccount);
    return { data: accounts, total: accounts.length };
  }
  return { data: [], total: 0 };
}

export async function getFacebookAuthUrl(): Promise<AuthUrlResponse> {
  const res: GenericResponse<BEAuthUrlResponse> = await apiClient("/social-auth/facebook");
  if (res?.data) {
    return { authUrl: res.data.authUrl, state: res.data.state };
  }
  throw new Error(res?.error?.errorMessage || "Failed to get Facebook auth URL");
}

export async function handleFacebookCallback(code: string, state: string): Promise<SocialAccount> {
  const res: GenericResponse<BESocialAccountDto> = await apiClient("/social-auth/facebook/callback", {
    method: "POST",
    data: { code, state } as BECallbackRequest,
  });
  if (res?.data) {
    return mapSocialAccount(res.data);
  }
  throw new Error(res?.error?.errorMessage || "Failed to link Facebook account");
}

export async function getAvailableTargets(accountId: string): Promise<AvailableTarget[]> {
  const res: GenericResponse<BEAvailableTargetDto[]> = await apiClient(`/social/accounts/${accountId}/available-targets`);
  if (res?.data) {
    return res.data.map(mapAvailableTarget);
  }
  return [];
}

export async function getLinkedTargets(accountId: string): Promise<SocialTarget[]> {
  const res: GenericResponse<BESocialTargetDto[]> = await apiClient(`/social/accounts/${accountId}/linked-targets`);
  if (res?.data) {
    return res.data.map(mapSocialTarget);
  }
  return [];
}

export async function linkTargets(accountId: string, targetIds: string[], brandId: string, profileId?: string): Promise<SocialAccount> {
  const res: GenericResponse<BESocialAccountDto> = await apiClient(`/social/accounts/${accountId}/link-targets`, {
    method: "POST",
    data: { profileId, provider: "facebook", providerTargetIds: targetIds, brandId } as BELinkTargetsRequest,
  });
  if (res?.data) {
    return mapSocialAccount(res.data);
  }
  throw new Error(res?.error?.errorMessage || "Failed to link targets");
}

export async function deleteSocialAccount(accountId: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/social/accounts/${accountId}`, {
    method: "DELETE",
  });
  return res?.data === true;
}

export async function deleteSocialIntegration(socialIntegrationId: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient(`/social/integrations/${socialIntegrationId}`, {
    method: "DELETE",
  });
  return res?.data === true || res?.success === true;
}

export async function fetchSocialIntegrations(brandId?: string): Promise<SocialIntegration[]> {
  try {
    if (brandId) {
      const res: GenericResponse<BESocialIntegrationDto[]> = await apiClient(`/social/integrations/brand/${brandId}`);
      if (res?.data) {
        return res.data.map((dto) => ({
          id: dto.id,
          socialAccountId: dto.socialAccountId,
          profileId: dto.profileId,
          brandName: dto.brandName ?? null,
          targetId: dto.externalId,
          provider: dto.platform.toLowerCase() as SocialPlatform,
          accountName: dto.name,
          targetName: dto.name,
          isActive: dto.isActive,
          brandId: dto.brandId ?? brandId,
          createdAt: dto.createdAt,
          updatedAt: dto.updatedAt,
        }));
      }
    }
  } catch { /* fallback */ }
  return [];
}

export function getAccountStatus(account: SocialAccount): AccountStatus {
  if (!account.isActive) return "error";
  if (!account.expiresAt) return "connected";
  if (new Date(account.expiresAt) < new Date()) return "expired";
  return "connected";
}
