"use client";

interface AdminStatsCardProps {
  title: string;
  value: string | number;
  icon: string;
  change?: string;
  changePositive?: boolean;
}

export default function AdminStatsCard({ title, value, icon, change, changePositive }: AdminStatsCardProps) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
      <div className="flex items-center justify-between mb-3">
        <span className="text-sm text-gray-500">{title}</span>
        <span className="material-symbols-outlined text-2xl text-gray-400">{icon}</span>
      </div>
      <div className="text-2xl font-bold text-gray-900">{value}</div>
      {change && (
        <div className={`text-sm mt-1 ${changePositive ? "text-emerald-600" : "text-red-600"}`}>
          {change}
        </div>
      )}
    </div>
  );
}
