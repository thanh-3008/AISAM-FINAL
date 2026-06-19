"use client";

import { type AdCampaignDto } from "@/services/adCampaignService";
import {
  campaignClicks,
  campaignConversions,
  campaignImpressions,
  campaignSpend,
  formatDate,
  formatMoney,
  objectiveIcons,
  objectiveLabels,
  statusClass,
} from "./campaignDisplay";

interface CampaignDetailDialogProps {
  campaign: AdCampaignDto | null;
  isLoading: boolean;
  onClose: () => void;
  onApply: (campaign: AdCampaignDto) => void;
  onRestart: (campaign: AdCampaignDto) => void;
}

export default function CampaignDetailDialog({ campaign, isLoading, onClose, onApply, onRestart }: CampaignDetailDialogProps) {
  if (!campaign) return null;

  const icon = objectiveIcons[campaign.objective] ?? "campaign";

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <section className="w-full max-w-3xl max-h-[90vh] overflow-y-auto rounded-2xl bg-surface-container-lowest shadow-2xl" onClick={(event) => event.stopPropagation()}>
          <header className="sticky top-0 bg-surface-container-lowest border-b border-outline-variant/20 p-5 flex items-center justify-between gap-4">
            <div className="flex items-center gap-3 min-w-0">
              <span className="w-11 h-11 rounded-xl bg-primary/10 text-primary flex items-center justify-center material-symbols-outlined">
                {icon}
              </span>
              <div className="min-w-0">
                <h2 className="text-title-lg font-bold text-on-surface truncate">{campaign.name}</h2>
                <p className="text-label-sm text-outline truncate">{campaign.brandName || "Unknown brand"}</p>
              </div>
            </div>
            <button onClick={onClose} className="w-9 h-9 rounded-lg hover:bg-surface-container material-symbols-outlined text-[18px]">
              close
            </button>
          </header>

          <div className="p-5 space-y-5">
            <div className="flex items-center gap-2 flex-wrap">
              <span className={`inline-flex items-center px-3 py-1 rounded-full border text-label-xs font-semibold ${statusClass(campaign.status)}`}>
                {campaign.status}
              </span>
              <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-surface-container text-label-xs font-semibold text-on-surface-variant">
                <span className="material-symbols-outlined text-[14px]">{icon}</span>
                {objectiveLabels[campaign.objective] ?? campaign.objective}
              </span>
              {campaign.facebookCampaignId && (
                <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-blue-50 text-blue-700 text-label-xs font-semibold">
                  <span className="material-symbols-outlined text-[14px]">sync</span>
                  Synced locally
                </span>
              )}
            </div>

            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              <Metric label="Impressions" value={campaignImpressions(campaign).toLocaleString()} icon="visibility" />
              <Metric label="Clicks" value={campaignClicks(campaign).toLocaleString()} icon="touch_app" />
              <Metric label="Spend" value={formatMoney(campaignSpend(campaign))} icon="payments" />
              <Metric label="Conversions" value={campaignConversions(campaign).toLocaleString()} icon="conversion_path" />
            </div>

            <div className="rounded-xl bg-surface-container divide-y divide-outline-variant/10">
              <DetailRow label="Ad account" value={campaign.adAccountId} />
              <DetailRow label="Budget" value={formatMoney(campaign.budget)} />
              <DetailRow label="Start date" value={formatDate(campaign.startDate)} />
              <DetailRow label="End date" value={formatDate(campaign.endDate)} />
              <DetailRow label="Facebook campaign ID" value={campaign.facebookCampaignId || "-"} />
              <DetailRow label="Created" value={formatDate(campaign.createdAt)} />
              <DetailRow label="Updated" value={formatDate(campaign.updatedAt)} />
            </div>

            {campaign.adSets.length > 0 && (
              <div>
                <h3 className="text-title-sm font-bold text-on-surface mb-3">Ad sets</h3>
                <div className="space-y-2">
                  {campaign.adSets.map((adSet) => (
                    <div key={adSet.id} className="rounded-xl border border-outline-variant/20 p-3 flex items-center justify-between gap-3">
                      <div>
                        <p className="text-body-sm font-semibold text-on-surface">{adSet.name}</p>
                        <p className="text-label-xs text-outline">{adSet.facebookAdSetId || "Local ad set"}</p>
                      </div>
                      <span className={`px-2.5 py-1 rounded-full border text-label-xs font-semibold ${statusClass(adSet.status)}`}>
                        {adSet.status}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>

          <footer className="sticky bottom-0 bg-surface-container-lowest border-t border-outline-variant/20 p-5 flex justify-end gap-2">
            {campaign.status === "DRAFT" && (
              <button onClick={() => onApply(campaign)} disabled={isLoading} className="h-10 px-4 rounded-xl bg-emerald-600 text-white text-label-sm font-semibold disabled:opacity-50">
                Apply
              </button>
            )}
            {campaign.status === "COMPLETED" && (
              <button onClick={() => onRestart(campaign)} disabled={isLoading} className="h-10 px-4 rounded-xl bg-blue-600 text-white text-label-sm font-semibold disabled:opacity-50">
                Restart
              </button>
            )}
            <button onClick={onClose} className="h-10 px-4 rounded-xl bg-primary text-on-primary text-label-sm font-semibold">
              Close
            </button>
          </footer>
        </section>
      </div>
    </>
  );
}

function Metric({ label, value, icon }: { label: string; value: string; icon: string }) {
  return (
    <div className="rounded-xl bg-surface-container p-3">
      <span className="material-symbols-outlined text-[18px] text-primary">{icon}</span>
      <p className="text-title-md font-bold text-on-surface mt-1">{value}</p>
      <p className="text-label-xs text-outline">{label}</p>
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="px-4 py-3 flex items-center justify-between gap-4">
      <span className="text-label-sm text-outline">{label}</span>
      <span className="text-label-sm font-semibold text-on-surface text-right break-all">{value}</span>
    </div>
  );
}
