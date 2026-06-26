"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import AdminEmptyState from "@/components/admin/AdminEmptyState";

interface PlanItem { id: string; name: string; planType: number; price: number; currency: string; billingCycle: string; creditsPerCycle: number; postQuotaPerCycle: number; memberLimit: number; maxCreditBalance: number; isActive: boolean; sortOrder: number; createdAt: string; }

export default function AdminPlansPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["admin", "plans"],
    queryFn: async () => {
      const res = await apiClient("/admin/plans");
      return res.data as PlanItem[];
    },
  });

  if (isLoading) return <div className="animate-pulse space-y-4"><div className="h-12 bg-gray-100 rounded-2xl" /></div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-[#191b24]">Plans</h1>
        <button className="px-4 py-2 rounded-xl bg-[#004ccd] text-white text-sm font-semibold hover:bg-[#004ccd]/90 transition-colors">
          + Create Plan
        </button>
      </div>

      {(!data || data.length === 0) ? <AdminEmptyState message="No plans configured." icon="auto_awesome" /> : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {data.map((p) => (
            <div key={p.id} className="bg-white border border-gray-200 rounded-2xl p-5 hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-lg font-semibold text-[#191b24]">{p.name}</h3>
                <AdminStatusBadge status={p.isActive ? "Active" : "Inactive"} />
              </div>
              <p className="text-2xl font-bold text-[#191b24] mb-4">{(p.price / 1000).toFixed(0)}K {p.currency}<span className="text-sm font-normal text-[#424656]">/{p.billingCycle}</span></p>
              <div className="space-y-2 text-sm text-[#424656]">
                <div className="flex justify-between"><span>Credits</span><span className="font-medium">{p.creditsPerCycle.toLocaleString()}</span></div>
                <div className="flex justify-between"><span>Post Quota</span><span className="font-medium">{p.postQuotaPerCycle.toLocaleString()}</span></div>
                <div className="flex justify-between"><span>Member Limit</span><span className="font-medium">{p.memberLimit}</span></div>
                <div className="flex justify-between"><span>Max Credit Balance</span><span className="font-medium">{p.maxCreditBalance.toLocaleString()}</span></div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
