"use client";

import { type Campaign } from "@/services/campaignService";

interface StartConfirmModalProps {
  campaign: Campaign | null;
  isLoading: boolean;
  onConfirm: (campaign: Campaign) => void;
  onCancel: () => void;
}

export default function StartConfirmModal({ campaign, isLoading, onConfirm, onCancel }: StartConfirmModalProps) {
  if (!campaign) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onCancel} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onCancel}>
        <div className="w-full max-w-sm bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center">
                <span className="material-symbols-outlined text-amber-600 text-[22px]">warning</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Start Campaign</h3>
                <p className="text-body-sm text-on-surface-variant">This will begin spending</p>
              </div>
            </div>

            <p className="text-body-sm text-on-surface-variant mb-4">
              Are you sure you want to start <span className="font-semibold text-on-surface">{campaign.name}</span>?
            </p>

            <div className="p-3 bg-amber-50 rounded-xl mb-6">
              <p className="text-[11px] text-amber-700 flex items-start gap-2">
                <span className="material-symbols-outlined text-[14px] mt-0.5">payments</span>
                <span>
                  This campaign will be set to <strong>ACTIVE</strong> on Facebook and real
                  advertising charges may apply based on your budget and targeting settings.
                </span>
              </p>
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={onCancel}
                className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]"
              >
                Cancel
              </button>
              <button
                onClick={() => onConfirm(campaign)}
                disabled={isLoading}
                className="px-5 py-2 rounded-xl bg-emerald-500 text-white text-label-md hover:bg-emerald-600 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 disabled:opacity-50"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">play_arrow</span>
                )}
                Start Campaign
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
