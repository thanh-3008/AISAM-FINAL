"use client";

import { PlatformIcon } from "@/lib/contentConstants";
import { type Campaign } from "@/services/campaignService";
import {
  OBJECTIVE_CONFIG,
  STATUS_CONFIG,
  formatCurrency,
  formatDate,
  formatNumber,
  getBudgetProgress,
  getCtr,
  getDaysRemaining,
} from "./campaignUtils";

interface CampaignDetailModalProps {
  campaign: Campaign | null;
  onClose: () => void;
  onRestart?: (campaign: Campaign) => void;
  onDeploy?: (campaign: Campaign) => void;
  onActivate?: (campaign: Campaign) => void;
  onCleanup?: (campaign: Campaign) => void;
  isLoading?: boolean;
}

export default function CampaignDetailModal({ campaign, onClose, onRestart, onDeploy, onActivate, onCleanup, isLoading }: CampaignDetailModalProps) {
  if (!campaign) return null;

  const objectiveConfig = OBJECTIVE_CONFIG[campaign.objective];
  const statusConfig = STATUS_CONFIG[campaign.status];
  const budgetProgress = getBudgetProgress(campaign);
  const daysRemaining = getDaysRemaining(campaign.endDate);
  const ctr = getCtr(campaign);

  const isJustDeployed = campaign.deploymentStatus === 2 && campaign.facebookCampaignId && campaign.status === "PENDING_REVIEW";
  const isFailed = campaign.status === "REJECTED" || campaign.deploymentStatus === 3;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-3xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className={`w-12 h-12 rounded-xl ${objectiveConfig.bg} flex items-center justify-center`}>
                <span className={`material-symbols-outlined text-[24px] ${objectiveConfig.color}`}>{objectiveConfig.icon}</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">{campaign.name}</h2>
                <p className="text-[11px] text-outline">{campaign.brandName}</p>
                <div className="flex items-center gap-2 mt-1 flex-wrap">
                  {campaign.productName && (
                    <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-surface-container-high rounded text-[9px] font-medium text-outline">
                      <span className="material-symbols-outlined text-[10px]">inventory_2</span>
                      {campaign.productName}
                    </span>
                  )}
                  {campaign.contentTitle && (
                    <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-surface-container-high rounded text-[9px] font-medium text-outline">
                      <span className="material-symbols-outlined text-[10px]">description</span>
                      {campaign.contentTitle.length > 30 ? campaign.contentTitle.slice(0, 30) + "…" : campaign.contentTitle}
                    </span>
                  )}
                </div>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 space-y-6">
            {/* Status and Objective */}
            <div className="flex items-center gap-3 flex-wrap">
              <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold border ${statusConfig.bg} ${statusConfig.color}`}>
                <span className={`w-2 h-2 rounded-full ${statusConfig.dot}`} />
                {statusConfig.label}
              </span>
              <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-lg ${objectiveConfig.bg}`}>
                <span className={`material-symbols-outlined text-[16px] ${objectiveConfig.color}`}>{objectiveConfig.icon}</span>
                <span className={`text-[11px] font-semibold ${objectiveConfig.color}`}>{objectiveConfig.label}</span>
              </span>
              {daysRemaining !== null && daysRemaining > 0 && campaign.status === "ACTIVE" && (
                <span className="inline-flex items-center gap-1 px-3 py-1 rounded-lg bg-primary/10 text-primary">
                  <span className="material-symbols-outlined text-[14px]">timer</span>
                  <span className="text-[11px] font-semibold">{daysRemaining} days remaining</span>
                </span>
              )}
            </div>

            {/* Performance Overview */}
            {campaign.canViewAnalytics && (
            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Performance Overview</h3>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="bg-surface-container-low rounded-xl p-4">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="material-symbols-outlined text-[16px] text-blue-600">visibility</span>
                    <span className="text-label-xs text-outline uppercase font-medium">Impressions</span>
                  </div>
                  <p className="text-headline-sm font-bold text-on-surface">{formatNumber(campaign.impressions)}</p>
                </div>
                <div className="bg-surface-container-low rounded-xl p-4">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="material-symbols-outlined text-[16px] text-cyan-600">touch_app</span>
                    <span className="text-label-xs text-outline uppercase font-medium">Clicks</span>
                  </div>
                  <p className="text-headline-sm font-bold text-on-surface">{formatNumber(campaign.clicks)}</p>
                  <p className="text-label-xs text-outline mt-1">CTR: {ctr}</p>
                </div>
                <div className="bg-surface-container-low rounded-xl p-4">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="material-symbols-outlined text-[16px] text-violet-600">payments</span>
                    <span className="text-label-xs text-outline uppercase font-medium">Spend</span>
                  </div>
                  <p className="text-headline-sm font-bold text-on-surface">{formatCurrency(campaign.spend, campaign.adAccountCurrency || undefined)}</p>
                  {campaign.canViewAnalytics && campaign.budget && campaign.spend != null && (
                    <p className="text-label-xs text-outline mt-1">of {formatCurrency(campaign.budget, campaign.adAccountCurrency || undefined)}</p>
                  )}
                </div>
                <div className="bg-surface-container-low rounded-xl p-4">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="material-symbols-outlined text-[16px] text-emerald-600">conversion_path</span>
                    <span className="text-label-xs text-outline uppercase font-medium">Conversions</span>
                  </div>
                  <p className="text-headline-sm font-bold text-on-surface">{campaign.conversions}</p>
                </div>
              </div>
            </div>

            )}
            {/* Budget Progress */}
            {campaign.canViewAnalytics && campaign.budget && campaign.spend != null && (
              <div>
                <h3 className="text-label-sm font-bold text-on-surface mb-3">Budget Utilization</h3>
                <div className="bg-surface-container-low rounded-xl p-4">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-[11px] text-outline font-medium">Spent so far</span>
                    <span className={`text-[11px] font-bold ${budgetProgress >= 90 ? "text-danger-red" : budgetProgress >= 70 ? "text-amber-600" : "text-on-surface"}`}>
                      {budgetProgress}%
                    </span>
                  </div>
                  <div className="h-3 bg-surface-container-high rounded-full overflow-hidden mb-2">
                    <div
                      className={`h-full rounded-full transition-all duration-500 ${
                        budgetProgress >= 90 ? "bg-danger-red" : budgetProgress >= 70 ? "bg-amber-500" : "bg-emerald-500"
                      }`}
                      style={{ width: `${budgetProgress}%` }}
                    />
                  </div>
                  <div className="flex items-center justify-between text-label-xs text-outline">
                    <span>{formatCurrency(campaign.spend, campaign.adAccountCurrency || undefined)} spent</span>
                    <span>{formatCurrency(campaign.budget - campaign.spend, campaign.adAccountCurrency || undefined)} remaining</span>
                  </div>
                </div>
              </div>
            )}

            {/* Campaign Details */}
            <div>
              <h3 className="text-label-sm font-bold text-on-surface mb-3">Campaign Details</h3>
              <div className="bg-surface-container-low rounded-xl divide-y divide-outline-variant/10">
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-[11px] text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                    Start Date
                  </span>
                  <span className="text-[11px] text-on-surface font-medium">{campaign.startDate ? formatDate(campaign.startDate) : "—"}</span>
                </div>
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-[11px] text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">event</span>
                    End Date
                  </span>
                  <span className="text-[11px] text-on-surface font-medium">{campaign.endDate ? formatDate(campaign.endDate) : "No end date"}</span>
                </div>
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-[11px] text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">account_balance</span>
                    Ad Account
                  </span>
                  <span className="text-[11px] text-on-surface font-medium font-mono">{campaign.adAccountId}</span>
                </div>
                {campaign.facebookCampaignId && (
                  <div className="flex items-center justify-between px-4 py-3">
                    <span className="text-[11px] text-outline flex items-center gap-2">
                      <PlatformIcon platform="facebook" className="w-[14px] h-[14px]" />
                      Facebook Campaign ID
                    </span>
                    <span className="text-[11px] text-on-surface font-medium font-mono">{campaign.facebookCampaignId}</span>
                  </div>
                )}
                {campaign.productName && (
                  <div className="flex items-center justify-between px-4 py-3">
                    <span className="text-[11px] text-outline flex items-center gap-2">
                      <span className="material-symbols-outlined text-[14px]">inventory_2</span>
                      Product
                    </span>
                    <span className="text-[11px] text-on-surface font-medium">{campaign.productName}</span>
                  </div>
                )}
                {campaign.contentTitle && (
                  <div className="flex items-center justify-between px-4 py-3">
                    <span className="text-[11px] text-outline flex items-center gap-2">
                      <span className="material-symbols-outlined text-[14px]">description</span>
                      Content
                    </span>
                    <span className="text-[11px] text-on-surface font-medium">{campaign.contentTitle}</span>
                  </div>
                )}
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-[11px] text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">schedule</span>
                    Created
                  </span>
                  <span className="text-[11px] text-on-surface font-medium">{formatDate(campaign.createdAt)}</span>
                </div>
                <div className="flex items-center justify-between px-4 py-3">
                  <span className="text-[11px] text-outline flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px]">update</span>
                    Last Updated
                  </span>
                  <span className="text-[11px] text-on-surface font-medium">{formatDate(campaign.updatedAt)}</span>
                </div>
              </div>
            </div>

            {/* Ad Sets */}
            {campaign.adSets.length > 0 && (
              <div>
                <h3 className="text-label-sm font-bold text-on-surface mb-3">
                  Ad Sets ({campaign.adSets.length})
                </h3>
                <div className="space-y-2">
                  {campaign.adSets.map((adSet) => (
                    <div key={adSet.id} className="bg-surface-container-low rounded-xl p-4">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-[11px] font-semibold text-on-surface">{adSet.name}</span>
                        <span className={`text-label-2xs font-bold px-2 py-0.5 rounded-full ${
                          adSet.status === "ACTIVE" ? "bg-emerald-50 text-emerald-600" : "bg-amber-50 text-amber-600"
                        }`}>
                          {adSet.status}
                        </span>
                      </div>
                      {campaign.canViewAnalytics && <div className="grid grid-cols-4 gap-2 text-label-xs">
                        <div>
                          <span className="text-outline">Impressions</span>
                          <p className="font-bold text-on-surface">{formatNumber(adSet.impressions)}</p>
                        </div>
                        <div>
                          <span className="text-outline">Clicks</span>
                          <p className="font-bold text-on-surface">{formatNumber(adSet.clicks)}</p>
                        </div>
                        <div>
                          <span className="text-outline">Spend</span>
                          <p className="font-bold text-on-surface">{formatCurrency(adSet.spend, campaign.adAccountCurrency || undefined)}</p>
                        </div>
                        {adSet.dailyBudget && (
                          <div>
                            <span className="text-outline">Daily Budget</span>
                            <p className="font-bold text-on-surface">{formatCurrency(adSet.dailyBudget, campaign.adAccountCurrency || undefined)}</p>
                          </div>
                        )}
                      </div>
                      }
                      {/* Ads inside Ad Set */}
                      {adSet.ads.length > 0 && (
                        <div className="mt-3 pt-3 border-t border-outline-variant/10 space-y-2">
                          <span className="text-label-2xs font-semibold text-outline uppercase tracking-wider">
                            Ads ({adSet.ads.length})
                          </span>
                          {adSet.ads.map((ad) => (
                            <div key={ad.id} className="bg-surface-container-high rounded-lg p-3">
                              <div className="flex items-center justify-between mb-1.5">
                                <span className="text-[10px] font-medium text-on-surface">Ad</span>
                                <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded-full ${
                                  ad.status === "ACTIVE" ? "bg-emerald-50 text-emerald-600" : "bg-amber-50 text-amber-600"
                                }`}>
                                  {ad.status ?? "—"}
                                </span>
                              </div>
                              <div className="grid grid-cols-3 gap-2 text-label-2xs">
                                {ad.adId && (
                                  <div>
                                    <span className="text-outline">Facebook Ad ID</span>
                                    <p className="font-medium text-on-surface truncate font-mono">{ad.adId}</p>
                                  </div>
                                )}
                                {ad.callToAction && (
                                  <div>
                                    <span className="text-outline">CTA</span>
                                    <p className="font-medium text-on-surface">{ad.callToAction}</p>
                                  </div>
                                )}
                                {ad.linkUrl && (
                                  <div className="col-span-3">
                                    <span className="text-outline">Link URL</span>
                                    <p className="font-medium text-on-surface truncate">{ad.linkUrl}</p>
                                  </div>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            {campaign.status === "DRAFT" && onDeploy && (
              <button
                onClick={() => onDeploy(campaign)}
                disabled={isLoading}
                className="px-5 py-2.5 bg-indigo-500 hover:bg-indigo-600 text-white rounded-xl text-label-sm font-bold shadow-lg shadow-indigo-500/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2 disabled:opacity-50 disabled:hover:scale-100"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">ads_click</span>
                )}
                Send to Meta
              </button>
            )}
            {isJustDeployed && (
              <div className="flex items-center gap-2 px-4 py-2 bg-purple-50 rounded-xl text-[11px] font-semibold text-purple-600">
                <span className="w-3 h-3 border-2 border-purple-300 border-t-purple-600 rounded-full animate-spin block" />
                Waiting for Meta review. AISAM will mark it ready when checks pass.
              </div>
            )}
            {isFailed && campaign.deploymentMessage && (
              <div className="flex-1 px-3 py-2 bg-red-50 rounded-xl text-[11px] font-medium text-red-600">
                {campaign.deploymentMessage}
              </div>
            )}
            {isFailed && onCleanup && (
              <button
                onClick={() => onCleanup(campaign)}
                disabled={isLoading}
                className="px-5 py-2.5 bg-red-500 hover:bg-red-600 text-white rounded-xl text-label-sm font-bold shadow-lg shadow-red-500/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2 disabled:opacity-50 disabled:hover:scale-100"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <>
                    <span className="material-symbols-outlined text-[16px]">cleaning_services</span>
                    Cleanup Failed
                  </>
                )}
              </button>
            )}
            {campaign.status === "PAUSED" && campaign.facebookCampaignId && !isJustDeployed && onActivate && (
              <button
                onClick={() => onActivate(campaign)}
                disabled={isLoading}
                className="px-5 py-2.5 bg-emerald-500 hover:bg-emerald-600 text-white rounded-xl text-label-sm font-bold shadow-lg shadow-emerald-500/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2 disabled:opacity-50 disabled:hover:scale-100"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">play_arrow</span>
                )}
                Start Campaign
              </button>
            )}
            {campaign.status === "COMPLETED" && onRestart && (
              <button
                onClick={() => onRestart(campaign)}
                disabled={isLoading}
                className="px-5 py-2.5 bg-blue-500 hover:bg-blue-600 text-white rounded-xl text-label-sm font-bold shadow-lg shadow-blue-500/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2 disabled:opacity-50 disabled:hover:scale-100"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">replay</span>
                )}
                Restart Campaign
              </button>
            )}
            <button
              onClick={onClose}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
