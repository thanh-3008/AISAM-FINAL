"use client";

import { PlatformIcon } from "@/lib/contentConstants";
import { type SocialAccount } from "@/services/socialAccountService";
import {
  PLATFORM_INFO,
  STATUS_CONFIG,
  formatDate,
  timeAgo,
  getExpiresDays,
  getExpiresColor,
  getExpiresProgress,
  getInitials,
  getAccountDisplayName,
  getAccountHandle,
  getAccountStatus,
} from "./socialUtils";

interface SocialAccountCardProps {
  account: SocialAccount;
  index: number;
  isSelected: boolean;
  isLoading: boolean;
  onDelete: (account: SocialAccount) => void;
  onManageTargets: (account: SocialAccount) => void;
  onSelect: (id: string) => void;
  canManage?: boolean;
}

export default function SocialAccountCard({
  account,
  index,
  isSelected,
  isLoading,
  onDelete,
  onManageTargets,
  onSelect,
  canManage = true,
}: SocialAccountCardProps) {
  const platformInfo = PLATFORM_INFO[account.provider];
  const status = getAccountStatus(account);
  const statusConfig = STATUS_CONFIG[status];
  const expiresDays = getExpiresDays(account.expiresAt);
  const expiresColor = getExpiresColor(expiresDays);
  const expiresProgress = getExpiresProgress(expiresDays);
  const displayName = getAccountDisplayName(account);
  const handle = getAccountHandle(account);
  const initials = getInitials(displayName);

  if (!platformInfo) return null;

  return (
    <div
      className={`group relative bg-surface-container-lowest rounded-2xl border overflow-hidden card-hover animate-fade-up transition-all ${
        isSelected ? "border-primary ring-2 ring-primary/20" : "border-outline-variant/20"
      }`}
      style={{ animationDelay: `${0.1 + index * 0.05}s` }}
    >
      <div className="absolute top-4 left-4 z-10">
        <input
          type="checkbox"
          checked={isSelected}
          onChange={(e) => onSelect(account.id, e.target.checked)}
          className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20 cursor-pointer"
        />
      </div>

      <div className={`h-2 w-full bg-gradient-to-r ${platformInfo.gradient}`} />

      <div className="p-5">
        <div className="flex items-start gap-3 mb-4">
          <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${platformInfo.gradient} flex items-center justify-center text-white font-bold text-lg shadow-lg shrink-0 relative`}>
            {initials}
            <div className="absolute -bottom-1 -right-1 w-5 h-5 rounded-full bg-surface-container-lowest border-2 border-surface-container-lowest flex items-center justify-center shadow-sm">
              <PlatformIcon platform={account.provider} className="w-3 h-3" />
            </div>
          </div>

          <div className="flex-1 min-w-0 pt-1">
            <h3 className="text-body-sm font-bold text-on-surface truncate">{displayName}</h3>
            <p className="text-[11px] text-outline truncate">{handle}</p>
            <div className="flex items-center gap-2 mt-1.5">
              <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-2xs font-bold border ${statusConfig.bg} ${statusConfig.color}`}>
                <span className={`w-1.5 h-1.5 rounded-full ${statusConfig.dot} ${status === "connected" ? "" : "animate-pulse"}`} />
                {statusConfig.label}
              </span>
              {expiresDays !== null && (
                <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-2xs font-bold ${expiresColor.bg} ${expiresColor.color}`}>
                  <span className="material-symbols-outlined text-label-xs">timer</span>
                  {expiresColor.label}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Metrics grid hidden — BE does not return followers/following/postsCount */}

        {account.expiresAt && (
          <div className="mb-4">
            <div className="flex items-center justify-between mb-1.5">
              <span className="text-label-xs text-outline font-medium">Token Validity</span>
              <span className={`text-label-xs font-bold ${expiresColor.color}`}>{expiresProgress}%</span>
            </div>
            <div className="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
              <div
                className={`h-full rounded-full transition-all duration-500 ${
                  expiresProgress <= 20 ? "bg-danger-red" : expiresProgress <= 50 ? "bg-amber-500" : "bg-emerald-500"
                }`}
                style={{ width: `${expiresProgress}%` }}
              />
            </div>
          </div>
        )}

        <div className="space-y-1.5 mb-4 text-label-xs">
          <div className="flex items-center justify-between">
            <span className="text-outline flex items-center gap-1.5">
              <span className="material-symbols-outlined text-[12px]">schedule</span>
              Connected
            </span>
            <span className="text-on-surface font-medium">{formatDate(account.createdAt)}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-outline flex items-center gap-1.5">
              <span className="material-symbols-outlined text-[12px]">update</span>
              Last sync
            </span>
            <span className="text-on-surface font-medium">{timeAgo(account.updatedAt)}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-outline flex items-center gap-1.5">
              <span className="material-symbols-outlined text-[12px]">link</span>
              Targets
            </span>
            <span className="text-on-surface font-medium">{account.targets?.length || 0} linked</span>
          </div>
        </div>

        {account.targets && account.targets.length > 0 && (
          <div className="mb-4 p-3 bg-surface-container-low rounded-xl">
            <p className="text-label-xs text-outline font-semibold mb-2 flex items-center gap-1">
              <span className="material-symbols-outlined text-[12px]">target</span>
              Linked Targets
            </p>
            <div className="space-y-1 max-h-16 overflow-y-auto">
              {account.targets.slice(0, 2).map((target) => (
                <div key={target.id} className="flex items-center gap-2">
                  <span className={`w-1.5 h-1.5 rounded-full ${target.isActive ? "bg-emerald-500" : "bg-outline/40"}`} />
                  <div className="flex-1 min-w-0">
                    <span className="text-label-xs text-on-surface truncate block">{target.name || target.providerTargetId}</span>
                    {target.brandName && <span className="text-label-3xs text-outline truncate block">{target.brandName}</span>}
                  </div>
                  <span className="text-label-3xs text-outline uppercase bg-surface-container-high px-1.5 py-0.5 rounded">{target.type}</span>
                </div>
              ))}
              {account.targets.length > 2 && (
                <p className="text-label-2xs text-outline text-center pt-0.5">+{account.targets.length - 2} more</p>
              )}
            </div>
          </div>
        )}

        {canManage && (
        <div className="flex items-center gap-2 pt-3 border-t border-outline-variant/10">
          <button
            onClick={() => onManageTargets(account)}
            className="flex-1 px-3 py-2.5 bg-primary/10 hover:bg-primary/20 rounded-xl text-[11px] font-semibold text-primary transition-all flex items-center justify-center gap-1.5"
          >
            <span className="material-symbols-outlined text-[14px]">link</span>
            Manage
          </button>
          <button
            onClick={() => onDelete(account)}
            disabled={isLoading}
            className="px-4 py-2.5 border border-outline-variant/30 hover:border-danger-red/30 hover:bg-danger-red/5 rounded-xl text-[11px] font-semibold text-outline hover:text-danger-red transition-all disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin block" />
            ) : (
              <span className="material-symbols-outlined text-[14px]">delete</span>
            )}
          </button>
        </div>
        )}
      </div>
    </div>
  );
}
