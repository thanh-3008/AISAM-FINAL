"use client";

import { type TeamMember } from "@/services/teamService";

interface TransferOwnershipConfirmModalProps {
  member: TeamMember | null;
  isLoading: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function TransferOwnershipConfirmModal({
  member,
  isLoading,
  onConfirm,
  onCancel,
}: TransferOwnershipConfirmModalProps) {
  if (!member) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-[60]" onClick={onCancel} />
      <div className="fixed inset-0 z-[60] flex items-center justify-center p-4" onClick={onCancel}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(event) => event.stopPropagation()}>
          <div className="p-6">
            <div className="flex items-center gap-3 mb-5">
              <div className="w-12 h-12 rounded-xl bg-amber-50 flex items-center justify-center">
                <span className="material-symbols-outlined text-amber-600 text-[24px]">admin_panel_settings</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Transfer Ownership</h3>
                <p className="text-body-sm text-on-surface-variant">Confirm the new workspace owner</p>
              </div>
            </div>

            <div className="p-4 rounded-xl border border-outline-variant/20 bg-surface-container-low mb-4">
              <p className="text-label-xs text-outline mb-1">New Owner</p>
              <p className="text-body-sm font-semibold text-on-surface">{member.name}</p>
              <p className="text-label-xs text-on-surface-variant">{member.email}</p>
            </div>

            <div className="mb-6 p-4 rounded-xl bg-amber-50 border border-amber-200/50">
              <div className="flex items-start gap-2">
                <span className="material-symbols-outlined text-amber-600 text-[18px] mt-0.5">warning</span>
                <div className="text-body-sm text-amber-800">
                  <p className="font-semibold mb-1">Important:</p>
                  <ul className="space-y-1 text-amber-700">
                    <li>You will become a Manager after transfer.</li>
                    <li>The new Owner will control billing, settings, and members.</li>
                    <li>This can only be reversed by the new Owner.</li>
                  </ul>
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={onCancel}
                disabled={isLoading}
                className="px-5 py-2.5 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={onConfirm}
                disabled={isLoading}
                className="px-5 py-2.5 rounded-xl bg-amber-600 text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">workspace_premium</span>
                )}
                {isLoading ? "Transferring..." : "Confirm Transfer"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
