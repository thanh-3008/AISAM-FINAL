import { motion } from "motion/react";

export default function AdminStatsCard({
  label, value, icon, trend,
}: {
  label: string; value: string | number; icon: string; trend?: string;
}) {
  const isNegative = trend?.startsWith("-");
  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-surface-container-lowest/80 backdrop-blur-sm border border-outline-variant/30 rounded-2xl shadow-sm p-5 hover:shadow-md hover:border-outline-variant/50 transition-all duration-200"
    >
      <div className="flex items-start justify-between">
        <div>
          <p className="text-label-xs text-on-surface-variant uppercase font-semibold">{label}</p>
          <p className="text-headline-sm text-on-surface font-bold mt-1">{value}</p>
          {trend && (
            <p className={`text-label-xs mt-1 font-semibold ${isNegative ? "text-danger-red" : "text-success-green"}`}>
              {trend}
            </p>
          )}
        </div>
        <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
          <span className="material-symbols-outlined text-xl text-primary">{icon}</span>
        </div>
      </div>
    </motion.div>
  );
}
