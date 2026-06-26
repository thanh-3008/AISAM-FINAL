"use client";

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
  isLoading?: boolean;
  variant?: "danger" | "warning";
}

export default function AdminConfirmDialog({
  open, title, message, confirmLabel = "Confirm", onConfirm, onCancel, isLoading, variant = "danger",
}: Props) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/30" onClick={onCancel} />
      <div className="relative bg-white rounded-2xl p-6 max-w-md w-full mx-4 shadow-xl border border-gray-200">
        <h3 className="text-xl font-semibold text-[#191b24]">{title}</h3>
        <p className="text-sm text-[#424656] mt-2">{message}</p>
        <div className="flex justify-end gap-3 mt-6">
          <button onClick={onCancel} disabled={isLoading}
            className="px-4 py-2 rounded-xl text-sm border border-gray-200 hover:bg-gray-100 transition-colors">
            Cancel
          </button>
          <button onClick={onConfirm} disabled={isLoading}
            className={`px-4 py-2 rounded-xl text-sm text-white transition-colors ${
              variant === "danger" ? "bg-[#DA1E28] hover:bg-[#DA1E28]/90" : "bg-[#F1C21B] hover:bg-[#F1C21B]/90"
            }`}>
            {isLoading ? "Processing..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
