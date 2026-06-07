"use client";

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
}

export default function CampaignDetailModal({ campaign, onClose }: CampaignDetailModalProps) {
  if (!campaign) return null;

  const objectiveConfig = OBJECTIVE_CONFIG[campaign.objective];
  const statusConfig = STATUS_CONFIG[campaign.status];
  const budgetProgress = getBudgetProgress(campaign);
  const daysRemaining = getDaysRemaining(campaign.endDate);
  const ctr = getCtr(campaign);

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
                  <p className="text-headline-sm font-bold text-on-surface">{formatCurrency(campaign.spend)}</p>
                  {campaign.budget && (
                    <p className="text-label-xs text-outline mt-1">of {formatCurrency(campaign.budget)}</p>
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

            {/* Budget Progress */}
            {campaign.budget && (
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
                    <span>{formatCurrency(campaign.spend)} spent</span>
                    <span>{formatCurrency(campaign.budget - campaign.spend)} remaining</span>
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
                      <span className="material-symbols-outlined text-[14px]">facebook</span>
                      Facebook Campaign ID
                    </span>
                    <span className="text-[11px] text-on-surface font-medium font-mono">{campaign.facebookCampaignId}</span>
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
                      <div className="grid grid-cols-4 gap-2 text-label-xs">
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
                          <p className="font-bold text-on-surface">{formatCurrency(adSet.spend)}</p>
                        </div>
                        {adSet.dailyBudget && (
                          <div>
                            <span className="text-outline">Daily Budget</span>
                            <p className="font-bold text-on-surface">{formatCurrency(adSet.dailyBudget)}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end sticky bottom-0 bg-surface-container-lowest">
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
