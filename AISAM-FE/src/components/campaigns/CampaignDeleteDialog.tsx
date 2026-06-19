"use client";

import { type AdCampaignDto } from "@/services/adCampaignService";

interface CampaignDeleteDialogProps {
  campaigns: AdCampaignDto[];
  isLoading: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export default function CampaignDeleteDialog({ campaigns, isLoading, onCancel, onConfirm }: CampaignDeleteDialogProps) {
  if (campaigns.length === 0) return null;

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/50" onClick={onCancel} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onCancel}>
        <section className="w-full max-w-md rounded-2xl bg-surface-container-lowest shadow-2xl" onClick={(event) => event.stopPropagation()}>
          <div className="p-6 space-y-4">
            <div className="flex items-center gap-3">
              <span className="w-10 h-10 rounded-xl bg-danger-red/10 text-danger-red flex items-center justify-center material-symbols-outlined">
                delete
              </span>
              <div>
                <h2 className="text-title-md font-bold text-on-surface">
                  Delete {campaigns.length > 1 ? `${campaigns.length} campaigns` : "campaign"}
                </h2>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone.</p>
              </div>
            </div>
            <div className="rounded-xl bg-surface-container p-3 max-h-40 overflow-y-auto">
              {campaigns.map((campaign) => (
                <p key={campaign.id} className="text-body-sm text-on-surface truncate py-1">{campaign.name}</p>
              ))}
            </div>
            <div className="flex justify-end gap-2">
              <button onClick={onCancel} className="h-10 px-4 rounded-xl border border-outline-variant/30 text-label-sm font-semibold">
                Cancel
              </button>
              <button
                onClick={onConfirm}
                disabled={isLoading}
                className="h-10 px-4 rounded-xl bg-danger-red text-white text-label-sm font-semibold disabled:opacity-50 flex items-center gap-2"
              >
                {isLoading && <span className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />}
                Delete
              </button>
            </div>
          </div>
        </section>
      </div>
    </>
  );
}
