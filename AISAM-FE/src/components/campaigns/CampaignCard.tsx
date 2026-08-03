"use client";

import { type Campaign } from "@/services/campaignService";
import {
  OBJECTIVE_CONFIG,
  STATUS_CONFIG,
  formatCurrency,
  formatDateShort,
  getBudgetProgress,
  getCtr,
  getDaysRemaining,
} from "./campaignUtils";
import { PlatformIcon } from "@/lib/contentConstants";

interface CampaignCardProps {
  campaign: Campaign;
  index: number;
  isSelected: boolean;
  isLoading: boolean;
  onSelect: (id: string, selected: boolean) => void;
  onViewDetail: (campaign: Campaign) => void;
  onEdit: (campaign: Campaign) => void;
  onToggleStatus: (campaign: Campaign) => void;
  onRestart: (campaign: Campaign) => void;
  onDelete: (campaign: Campaign) => void;
  onDeploy?: (campaign: Campaign) => void;
  onActivate?: (campaign: Campaign) => void;
  onCleanup?: (campaign: Campaign) => void;
}

export default function CampaignCard({
  campaign,
  index,
  isSelected,
  isLoading,
  onSelect,
  onViewDetail,
  onEdit,
  onToggleStatus,
  onRestart,
  onDelete,
  onDeploy,
  onActivate,
  onCleanup,
}: CampaignCardProps) {
  const objectiveConfig = OBJECTIVE_CONFIG[campaign.objective];
  const statusConfig = STATUS_CONFIG[campaign.status];
  const budgetProgress = getBudgetProgress(campaign);
  const daysRemaining = getDaysRemaining(campaign.endDate);
  const ctr = getCtr(campaign);

  const isJustDeployed = campaign.deploymentStatus === 2 && campaign.facebookCampaignId && campaign.status === "PENDING_REVIEW";
  const canActivate = campaign.deploymentStatus === 2 && campaign.facebookCampaignId && campaign.status === "PAUSED";
  const isFailed = campaign.status === "REJECTED" || campaign.deploymentStatus === 3;

  return (
    <div
      className={`group bg-surface-container-lowest rounded-2xl border overflow-hidden card-hover animate-fade-up transition-all ${
        isSelected ? "border-primary ring-2 ring-primary/20" : "border-outline-variant/20"
      }`}
      style={{ animationDelay: `${0.1 + index * 0.05}s` }}
    >
      {/* Header with checkbox and status */}
      <div className="px-5 pt-4 pb-3 flex items-start justify-between">
        <div className="flex items-start gap-3 flex-1 min-w-0">
          <input
            type="checkbox"
            checked={isSelected}
            onChange={(e) => onSelect(campaign.id, e.target.checked)}
            className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20 cursor-pointer mt-0.5"
          />
          <div className="flex-1 min-w-0">
            <h3 className="text-body-sm font-bold text-on-surface truncate">{campaign.name}</h3>
            <p className="text-[11px] text-outline mt-0.5">{campaign.brandName}</p>
            <div className="flex items-center gap-1.5 mt-1 flex-wrap">
              {campaign.productName && (
                <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-surface-container-high rounded text-[9px] font-medium text-outline">
                  <span className="material-symbols-outlined text-[10px]">inventory_2</span>
                  {campaign.productName}
                </span>
              )}
              {campaign.contentTitle && (
                <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-surface-container-high rounded text-[9px] font-medium text-outline">
                  <span className="material-symbols-outlined text-[10px]">description</span>
                  {campaign.contentTitle.length > 20 ? campaign.contentTitle.slice(0, 20) + "…" : campaign.contentTitle}
                </span>
              )}
            </div>
          </div>
        </div>
        <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-2xs font-bold border ${statusConfig.bg} ${statusConfig.color}`}>
          <span className={`w-1.5 h-1.5 rounded-full ${statusConfig.dot} ${campaign.status === "ACTIVE" ? "animate-pulse" : ""}`} />
          {statusConfig.label}
        </span>
      </div>

      {/* Platform badge */}
      <div className="px-5 pb-2">
        <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded text-label-2xs font-medium ${
          campaign.platform === "instagram" ? "bg-pink-50 text-pink-600" : "bg-blue-50 text-blue-600"
        }`}>
          <PlatformIcon platform={campaign.platform || "facebook"} className="w-[14px] h-[14px]" />
          {campaign.platform === "instagram" ? "Instagram" : "Facebook"}
        </span>
      </div>

      {/* Objective and Budget */}
      <div className="px-5 pb-3 flex items-center gap-3">
        <div className={`flex items-center gap-1.5 px-2 py-1 rounded-lg ${objectiveConfig.bg}`}>
          <span className={`material-symbols-outlined text-[14px] ${objectiveConfig.color}`}>{objectiveConfig.icon}</span>
          <span className={`text-label-xs font-semibold ${objectiveConfig.color}`}>{objectiveConfig.label}</span>
        </div>
        {campaign.budget && (
          <span className="text-[11px] text-outline font-medium">{formatCurrency(campaign.budget, campaign.adAccountCurrency || undefined)} budget</span>
        )}
      </div>

      {/* Date range */}
      <div className="px-5 pb-3 flex items-center gap-2 text-label-xs text-outline">
        <span className="material-symbols-outlined text-[12px]">calendar_today</span>
        <span>{formatDateShort(campaign.startDate)} – {formatDateShort(campaign.endDate)}</span>
        {daysRemaining !== null && daysRemaining > 0 && campaign.status === "ACTIVE" && (
          <span className="text-primary font-medium ml-auto">{daysRemaining}d left</span>
        )}
      </div>

      {/* Performance metrics */}
      <div className="px-5 pb-3 grid grid-cols-4 gap-2">
        <div className="text-center p-2 bg-surface-container-low rounded-lg">
          <p className="text-[11px] font-bold text-on-surface">{campaign.impressions >= 1000 ? `${(campaign.impressions / 1000).toFixed(1)}K` : campaign.impressions}</p>
          <p className="text-label-3xs text-outline uppercase">Impr.</p>
        </div>
        <div className="text-center p-2 bg-surface-container-low rounded-lg">
          <p className="text-[11px] font-bold text-on-surface">{ctr}</p>
          <p className="text-label-3xs text-outline uppercase">CTR</p>
        </div>
        <div className="text-center p-2 bg-surface-container-low rounded-lg">
          <p className="text-[11px] font-bold text-on-surface">{formatCurrency(campaign.spend, campaign.adAccountCurrency || undefined)}</p>
          <p className="text-label-3xs text-outline uppercase">Spend</p>
        </div>
        <div className="text-center p-2 bg-surface-container-low rounded-lg">
          <p className="text-[11px] font-bold text-on-surface">{campaign.conversions}</p>
          <p className="text-label-3xs text-outline uppercase">Conv.</p>
        </div>
      </div>

      {/* Budget progress */}
      {campaign.budget && (
        <div className="px-5 pb-3">
          <div className="flex items-center justify-between mb-1">
            <span className="text-label-2xs text-outline font-medium">Budget Used</span>
            <span className={`text-label-2xs font-bold ${budgetProgress >= 90 ? "text-danger-red" : budgetProgress >= 70 ? "text-amber-600" : "text-on-surface"}`}>
              {budgetProgress}%
            </span>
          </div>
          <div className="h-1.5 bg-surface-container-high rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all duration-500 ${
                budgetProgress >= 90 ? "bg-danger-red" : budgetProgress >= 70 ? "bg-amber-500" : "bg-emerald-500"
              }`}
              style={{ width: `${budgetProgress}%` }}
            />
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex items-center gap-2 px-5 py-3 border-t border-outline-variant/10 bg-surface-container-low/30">
        <button
          onClick={() => onViewDetail(campaign)}
          className="flex-1 px-3 py-2 bg-primary/10 hover:bg-primary/20 rounded-lg text-[11px] font-semibold text-primary transition-all flex items-center justify-center gap-1.5"
        >
          <span className="material-symbols-outlined text-[14px]">visibility</span>
          View Details
        </button>
        {campaign.status !== "COMPLETED" && campaign.status !== "REJECTED" && (
          <button
            onClick={() => onEdit(campaign)}
            disabled={isLoading}
            className="px-3 py-2 border border-outline-variant/30 hover:bg-surface-container-high rounded-lg text-[11px] font-semibold text-outline hover:text-on-surface transition-all disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[14px]">edit</span>
          </button>
        )}
        {campaign.status === "DRAFT" && !campaign.facebookCampaignId && onDeploy ? (
          <button
            onClick={() => onDeploy(campaign)}
            disabled={isLoading}
            className="px-3 py-2 bg-indigo-50 hover:bg-indigo-100 rounded-lg text-[11px] font-semibold text-indigo-600 transition-all flex items-center gap-1 disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-indigo-300 border-t-indigo-600 rounded-full animate-spin block" />
            ) : (
              <>
                <span className="material-symbols-outlined text-[14px]">ads_click</span>
                Send to Meta
              </>
            )}
          </button>
        ) : isJustDeployed ? (
          <button
            disabled
            className="px-3 py-2 bg-purple-50 rounded-lg text-[11px] font-semibold text-purple-600 transition-all flex items-center gap-1 cursor-not-allowed"
          >
            <span className="w-3.5 h-3.5 border-2 border-purple-300 border-t-purple-600 rounded-full animate-spin block" />
            Review...
          </button>
        ) : canActivate && onActivate ? (
          <button
            onClick={() => onActivate(campaign)}
            disabled={isLoading}
            className="px-3 py-2 bg-emerald-50 hover:bg-emerald-100 rounded-lg text-[11px] font-semibold text-emerald-600 transition-all flex items-center gap-1 disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-emerald-300 border-t-emerald-600 rounded-full animate-spin block" />
            ) : (
              <>
                <span className="material-symbols-outlined text-[14px]">play_arrow</span>
                Start
              </>
            )}
          </button>
        ) : campaign.status === "PAUSED" && campaign.facebookCampaignId ? (
          <button
            onClick={() => onToggleStatus(campaign)}
            disabled={isLoading}
            className="px-3 py-2 bg-emerald-50 hover:bg-emerald-100 rounded-lg text-[11px] font-semibold text-emerald-600 transition-all flex items-center gap-1 disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-emerald-300 border-t-emerald-600 rounded-full animate-spin block" />
            ) : (
              <>
                <span className="material-symbols-outlined text-[14px]">play_arrow</span>
                Start
              </>
            )}
          </button>
        ) : campaign.status === "ACTIVE" ? (
          <button
            onClick={() => onToggleStatus(campaign)}
            disabled={isLoading}
            className="px-3 py-2 border border-outline-variant/30 hover:bg-surface-container-high rounded-lg text-[11px] font-semibold text-outline hover:text-on-surface transition-all disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-outline/30 border-t-outline rounded-full animate-spin block" />
            ) : (
              <span className="material-symbols-outlined text-[14px]">pause</span>
            )}
          </button>
        ) : null}
        {isFailed && onCleanup && (
          <button
            onClick={() => onCleanup(campaign)}
            disabled={isLoading}
            className="px-3 py-2 bg-red-50 hover:bg-red-100 rounded-lg text-[11px] font-semibold text-red-600 transition-all flex items-center gap-1 disabled:opacity-50"
            title={campaign.deploymentMessage || "Cleanup failed deployment"}
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-red-300 border-t-red-600 rounded-full animate-spin block" />
            ) : (
              <>
                <span className="material-symbols-outlined text-[14px]">cleaning_services</span>
                Cleanup
              </>
            )}
          </button>
        )}
        {campaign.status === "COMPLETED" && (
          <button
            onClick={() => onRestart(campaign)}
            disabled={isLoading}
            className="px-3 py-2 bg-blue-50 hover:bg-blue-100 rounded-lg text-[11px] font-semibold text-blue-600 transition-all flex items-center gap-1 disabled:opacity-50"
          >
            {isLoading ? (
              <span className="w-3.5 h-3.5 border-2 border-blue-300 border-t-blue-600 rounded-full animate-spin block" />
            ) : (
              <>
                <span className="material-symbols-outlined text-[14px]">replay</span>
                Restart
              </>
            )}
          </button>
        )}
        <button
          onClick={() => onDelete(campaign)}
          disabled={isLoading}
          className="px-3 py-2 border border-outline-variant/30 hover:border-danger-red/30 hover:bg-danger-red/5 rounded-lg text-[11px] font-semibold text-outline hover:text-danger-red transition-all disabled:opacity-50"
        >
          <span className="material-symbols-outlined text-[14px]">delete</span>
        </button>
      </div>
    </div>
  );
}
