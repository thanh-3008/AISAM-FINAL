interface StatsCardsProps {
  publishedCount: number;
  totalCount: number;
  quotaUsed: number | null;
  quotaTotal: number | null;
}

export default function StatsCards({ publishedCount, totalCount, quotaUsed, quotaTotal }: StatsCardsProps) {
  const quotaPercent = quotaTotal && quotaTotal > 0
    ? Math.round((quotaUsed! / quotaTotal) * 100)
    : 0;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
      {/* Published Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm">
        <div className="w-14 h-14 rounded-2xl bg-emerald-50 flex items-center justify-center text-emerald-600">
          <span className="material-symbols-outlined text-[28px]">task_alt</span>
        </div>
        <div>
          <p className="text-label-sm text-outline uppercase font-semibold">Published</p>
          <h3 className="text-headline-md text-on-surface">{publishedCount}</h3>
        </div>
      </div>

      {/* Total Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 flex items-center gap-5 shadow-sm">
        <div className="w-14 h-14 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600">
          <span className="material-symbols-outlined text-[28px]">inventory_2</span>
        </div>
        <div>
          <p className="text-label-sm text-outline uppercase font-semibold">Total</p>
          <h3 className="text-headline-md text-on-surface">{totalCount}</h3>
        </div>
      </div>

      {/* Quota Card */}
      <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 shadow-sm">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-10 h-10 rounded-xl bg-purple-50 flex items-center justify-center text-purple-600">
            <span className="material-symbols-outlined text-[22px]">data_usage</span>
          </div>
          <div>
            <p className="text-label-sm text-outline uppercase font-semibold">Quota</p>
            <p className="text-label-xs text-outline/60">
              {quotaUsed ?? "—"} / {quotaTotal ?? "—"} used
            </p>
          </div>
        </div>
        {quotaTotal && quotaTotal > 0 && (
          <div className="w-full bg-surface-container-high rounded-full h-2 overflow-hidden">
            <div
              className={`h-full rounded-full transition-all duration-500 ${
                quotaPercent > 80 ? "bg-danger-red" : quotaPercent > 50 ? "bg-warning-amber" : "bg-primary"
              }`}
              style={{ width: `${Math.min(quotaPercent, 100)}%` }}
            />
          </div>
        )}
      </div>
    </div>
  );
}