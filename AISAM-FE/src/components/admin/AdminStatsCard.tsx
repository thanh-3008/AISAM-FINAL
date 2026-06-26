export default function AdminStatsCard({
  label, value, icon, trend,
}: {
  label: string; value: string | number; icon: string; trend?: string;
}) {
  return (
    <div className="bg-white border border-gray-200 rounded-2xl p-5 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs text-[#424656] uppercase tracking-wider font-semibold">{label}</p>
          <p className="text-2xl font-bold text-[#191b24] mt-1">{value}</p>
          {trend && <p className="text-[11px] text-[#198038] mt-1">{trend}</p>}
        </div>
        <span className="material-symbols-outlined text-2xl text-[#004ccd]">{icon}</span>
      </div>
    </div>
  );
}
