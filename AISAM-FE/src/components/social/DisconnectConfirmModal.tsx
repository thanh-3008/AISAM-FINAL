import { type SocialAccount } from "@/services/socialAccountService";
import { getAccountDisplayName } from "./socialUtils";

interface DisconnectConfirmModalProps {
  account: SocialAccount | null;
  isLoading: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function DisconnectConfirmModal({
  account,
  isLoading,
  onConfirm,
  onCancel,
}: DisconnectConfirmModalProps) {
  if (!account) return null;

  const displayName = getAccountDisplayName(account);

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onCancel} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onCancel}>
        <div className="w-full max-w-sm bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">link_off</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Account</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{displayName}</span>? All linked targets will be unlinked.
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={onCancel}
                className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">
                Cancel
              </button>
              <button onClick={onConfirm} disabled={isLoading}
                className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 disabled:opacity-50">
                {isLoading ? (
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">delete</span>
                )}
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
