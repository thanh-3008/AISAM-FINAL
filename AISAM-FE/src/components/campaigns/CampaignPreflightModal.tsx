"use client";

import type { Campaign, CampaignPreflightResult } from "@/services/campaignService";

interface CampaignPreflightModalProps {
  campaign: Campaign;
  result: CampaignPreflightResult;
  isDeploying: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export default function CampaignPreflightModal({
  campaign,
  result,
  isDeploying,
  onClose,
  onConfirm,
}: CampaignPreflightModalProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/45 p-4">
      <div className="w-full max-w-xl rounded-2xl border border-outline-variant/30 bg-surface-container-lowest shadow-2xl">
        <div className="border-b border-outline-variant/20 p-6">
          <h2 className="text-title-lg font-bold text-on-surface">Campaign preflight</h2>
          <p className="mt-1 text-body-sm text-on-surface-variant">{campaign.name}</p>
        </div>

        <div className="max-h-[55vh] space-y-2 overflow-y-auto p-6">
          {result.checks.map((check) => {
            const failed = check.status === "failed";
            const warning = check.status === "warning";
            return (
              <div
                key={check.key}
                className={`flex items-start gap-3 rounded-xl border p-3 ${
                  failed
                    ? "border-red-200 bg-red-50 text-red-800"
                    : warning
                      ? "border-amber-200 bg-amber-50 text-amber-800"
                      : "border-green-200 bg-green-50 text-green-800"
                }`}
              >
                <span className="material-symbols-outlined text-xl">
                  {failed ? "error" : warning ? "warning" : "check_circle"}
                </span>
                <div>
                  <p className="text-label-sm font-bold capitalize">{check.key.replaceAll("_", " ")}</p>
                  <p className="text-body-sm">{check.message}</p>
                </div>
              </div>
            );
          })}
        </div>

        <div className="flex items-center justify-between gap-3 border-t border-outline-variant/20 p-6">
          <p className="text-body-sm text-on-surface-variant">
            {result.ready
              ? "All required checks passed."
              : `${result.errors} blocking issue(s) must be fixed before deployment.`}
          </p>
          <div className="flex gap-2">
            <button type="button" onClick={onClose} className="rounded-xl border border-outline-variant/30 px-4 py-2 text-label-sm font-semibold">
              Close
            </button>
            <button
              type="button"
              onClick={onConfirm}
              disabled={!result.ready || isDeploying}
              className="rounded-xl bg-primary px-4 py-2 text-label-sm font-bold text-on-primary disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isDeploying ? "Deploying…" : "Confirm deploy"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
