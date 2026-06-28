"use client";

import { motion } from "motion/react";

interface Props {
  message?: string;
  icon?: string;
  title?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export default function AdminEmptyState({
  message = "No data found.",
  icon = "inbox",
  title,
  actionLabel,
  onAction,
}: Props) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex flex-col items-center justify-center py-16 px-4"
    >
      <div className="w-20 h-20 rounded-full bg-surface-container flex items-center justify-center mb-4">
        <span className="material-symbols-outlined text-5xl text-on-surface-variant/25">{icon}</span>
      </div>
      {title && <h3 className="text-headline-sm text-on-surface font-semibold mt-2">{title}</h3>}
      <p className="text-body-sm text-on-surface-variant mt-1 text-center max-w-sm">{message}</p>
      {actionLabel && onAction && (
        <button
          onClick={onAction}
          className="mt-5 px-5 py-2.5 rounded-xl bg-primary text-on-primary text-body-sm font-semibold hover:bg-primary-container transition-colors"
        >
          {actionLabel}
        </button>
      )}
    </motion.div>
  );
}
