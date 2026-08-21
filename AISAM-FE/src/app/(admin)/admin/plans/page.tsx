"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminPlans, saveAdminPlans, fetchAdminCreditPacks, saveAdminCreditPacks, SubscriptionPlanDto, CreditPackDto } from "@/services/adminService";

const allFeatures = [
  "generateText", "manualPost", "basicAnalytics", "aiImage", "contentCalendar",
  "schedulePost", "multiPlatformPublish", "trendAnalysis", "holidaySuggestion",
  "aiVideo", "advancedAnalytics", "campaignRecommendation", "teamManagement",
  "sharedCredits", "sharedWorkspace", "workspaceDashboard",
  "lifetimeAssignedLimit", "monthlyAssignedLimit", "creditUsageReport", "topMemberAnalytics"
];

export default function AdminPlansPage() {
  const [activeTab, setActiveTab] = useState<"plans" | "credits">("plans");
  const [plans, setPlans] = useState<SubscriptionPlanDto[]>([]);
  const [creditPacks, setCreditPacks] = useState<CreditPackDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saved, setSaved] = useState(false);
  const [editingPlan, setEditingPlan] = useState<SubscriptionPlanDto | null>(null);
  const [editingPack, setEditingPack] = useState<CreditPackDto | null>(null);

  useEffect(() => {
    Promise.all([fetchAdminPlans(), fetchAdminCreditPacks()]).then(([p, c]) => {
      if (p) setPlans(p);
      if (c) setCreditPacks(c);
      setLoading(false);
    });
  }, []);

  const handleSave = async () => {
    let ok = false;
    if (activeTab === "plans") {
      ok = await saveAdminPlans(plans);
    } else {
      ok = await saveAdminCreditPacks(creditPacks);
    }
    if (ok) { setSaved(true); setTimeout(() => setSaved(false), 2000); }
  };

  // --- Plans Handlers ---
  const handleEditPlan = (plan: SubscriptionPlanDto) => {
    setEditingPlan({ ...plan, features: [...plan.features] });
  };
  const handleSavePlan = () => {
    if (!editingPlan) return;
    setPlans((prev) => prev.map((p) => p.id === editingPlan.id ? editingPlan : p));
    setEditingPlan(null);
  };
  const handleToggleFeature = (feature: string) => {
    if (!editingPlan) return;
    setEditingPlan((prev) => ({
      ...prev!,
      features: prev!.features.includes(feature)
        ? prev!.features.filter((f) => f !== feature)
        : [...prev!.features, feature]
    }));
  };
  const handleAddPlan = () => {
    const newPlan: SubscriptionPlanDto = {
      id: `plan-${Date.now()}`,
      name: "New Plan",
      price: 0,
      credits: 100,
      postsPerMonth: 50,
      members: 1,
      features: ["basicAnalytics", "generateText"],
      isActive: true
    };
    setPlans([...plans, newPlan]);
  };
  const handleDeletePlan = (id: string) => {
    if (!confirm("Delete this plan?")) return;
    setPlans((prev) => prev.filter((p) => p.id !== id));
  };
  const handleTogglePlanActive = (id: string) => {
    setPlans((prev) => prev.map((p) => p.id === id ? { ...p, isActive: !p.isActive } : p));
  };

  // --- Credit Packs Handlers ---
  const handleEditPack = (pack: CreditPackDto) => {
    setEditingPack({ ...pack });
  };
  const handleSavePack = () => {
    if (!editingPack) return;
    setCreditPacks((prev) => prev.map((p) => p.id === editingPack.id ? editingPack : p));
    setEditingPack(null);
  };
  const handleAddPack = () => {
    const newPack: CreditPackDto = {
      id: `pack-${Date.now()}`,
      name: "New Pack",
      price: 1000,
      credits: 100,
      isActive: true
    };
    setCreditPacks([...creditPacks, newPack]);
  };
  const handleDeletePack = (id: string) => {
    if (!confirm("Delete this pack?")) return;
    setCreditPacks((prev) => prev.filter((p) => p.id !== id));
  };
  const handleTogglePackActive = (id: string) => {
    setCreditPacks((prev) => prev.map((p) => p.id === id ? { ...p, isActive: !p.isActive } : p));
  };

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Pricing Management" }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Pricing Management" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Pricing Management</h2>
            <p className="text-gray-500 mt-1">Manage pricing, quotas, and features for plans and credit packs.</p>
          </div>
          <div className="flex items-center gap-3">
            <button onClick={activeTab === "plans" ? handleAddPlan : handleAddPack} className="px-4 py-2 text-sm rounded-lg bg-emerald-600 text-white hover:bg-emerald-700">Add {activeTab === "plans" ? "Plan" : "Pack"}</button>
            <button onClick={handleSave} className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700">Save All</button>
            {saved && <span className="text-sm text-emerald-600">Saved!</span>}
          </div>
        </div>

        <div className="flex border-b border-gray-200">
          <button
            onClick={() => setActiveTab("plans")}
            className={`px-4 py-3 text-sm font-medium border-b-2 ${activeTab === "plans" ? "border-blue-600 text-blue-600" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"}`}
          >
            Subscription Plans
          </button>
          <button
            onClick={() => setActiveTab("credits")}
            className={`px-4 py-3 text-sm font-medium border-b-2 ${activeTab === "credits" ? "border-blue-600 text-blue-600" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"}`}
          >
            Credit Packs
          </button>
        </div>

        <div className="space-y-4">
          {activeTab === "plans" && plans.map((plan) => (
            <div key={plan.id} className={`bg-white rounded-xl border ${plan.isActive ? "border-gray-200" : "border-gray-100 opacity-60"} shadow-sm p-6`}>
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                  <h3 className="text-lg font-bold text-gray-900">{plan.name}</h3>
                  <StatusBadge status={plan.isActive ? "Active" : "Disabled"} variant={plan.isActive ? "success" : "warning"} />
                </div>
                <div className="flex items-center gap-2">
                  <button onClick={() => handleEditPlan(plan)} className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200">Edit</button>
                  <button onClick={() => handleTogglePlanActive(plan.id)} className="text-xs px-2 py-1 rounded bg-amber-50 hover:bg-amber-100 text-amber-700">{plan.isActive ? "Disable" : "Enable"}</button>
                  <button onClick={() => handleDeletePlan(plan.id)} className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600">Delete</button>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-5 gap-4 text-sm mb-3">
                <div><span className="text-gray-500">Price</span><p className="font-bold text-gray-900">{plan.price.toLocaleString()} VND</p></div>
                <div><span className="text-gray-500">Credits</span><p className="font-bold text-gray-900">{plan.credits.toLocaleString()}</p></div>
                <div><span className="text-gray-500">Posts/Month</span><p className="font-bold text-gray-900">{plan.postsPerMonth.toLocaleString()}</p></div>
                <div><span className="text-gray-500">Members</span><p className="font-bold text-gray-900">{plan.members}</p></div>
                <div><span className="text-gray-500">Features</span><p className="font-bold text-gray-900">{plan.features.length}</p></div>
              </div>

              {editingPlan?.id === plan.id && (
                <div className="border-t pt-4 mt-4 space-y-4">
                  <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
                    <div><label className="text-xs text-gray-500">Name</label><input value={editingPlan.name} onChange={(e) => setEditingPlan({ ...editingPlan, name: e.target.value })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Price</label><input type="number" value={editingPlan.price} onChange={(e) => setEditingPlan({ ...editingPlan, price: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Credits</label><input type="number" value={editingPlan.credits} onChange={(e) => setEditingPlan({ ...editingPlan, credits: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Posts/Mo</label><input type="number" value={editingPlan.postsPerMonth} onChange={(e) => setEditingPlan({ ...editingPlan, postsPerMonth: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Members</label><input type="number" value={editingPlan.members} onChange={(e) => setEditingPlan({ ...editingPlan, members: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500 mb-2 block">Features</label>
                    <div className="flex flex-wrap gap-2">
                      {allFeatures.map((f) => (
                        <button key={f} onClick={() => handleToggleFeature(f)} className={`text-xs px-2 py-1 rounded-full ${editingPlan.features.includes(f) ? "bg-blue-100 text-blue-700 border border-blue-300" : "bg-gray-100 text-gray-500 border border-gray-200"}`}>{f}</button>
                      ))}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <button onClick={handleSavePlan} className="px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700">Save Plan</button>
                    <button onClick={() => setEditingPlan(null)} className="px-3 py-1.5 text-xs rounded border border-gray-200 hover:bg-gray-50">Cancel</button>
                  </div>
                </div>
              )}
            </div>
          ))}

          {activeTab === "credits" && creditPacks.map((pack) => (
            <div key={pack.id} className={`bg-white rounded-xl border ${pack.isActive ? "border-gray-200" : "border-gray-100 opacity-60"} shadow-sm p-6`}>
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                  <h3 className="text-lg font-bold text-gray-900">{pack.name}</h3>
                  <StatusBadge status={pack.isActive ? "Active" : "Disabled"} variant={pack.isActive ? "success" : "warning"} />
                </div>
                <div className="flex items-center gap-2">
                  <button onClick={() => handleEditPack(pack)} className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200">Edit</button>
                  <button onClick={() => handleTogglePackActive(pack.id)} className="text-xs px-2 py-1 rounded bg-amber-50 hover:bg-amber-100 text-amber-700">{pack.isActive ? "Disable" : "Enable"}</button>
                  <button onClick={() => handleDeletePack(pack.id)} className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600">Delete</button>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm mb-3">
                <div><span className="text-gray-500">ID</span><p className="font-bold text-gray-900">{pack.id}</p></div>
                <div><span className="text-gray-500">Price</span><p className="font-bold text-gray-900">{pack.price.toLocaleString()} VND</p></div>
                <div><span className="text-gray-500">Credits</span><p className="font-bold text-gray-900">{pack.credits.toLocaleString()}</p></div>
              </div>

              {editingPack?.id === pack.id && (
                <div className="border-t pt-4 mt-4 space-y-4">
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                    <div><label className="text-xs text-gray-500">Name</label><input value={editingPack.name} onChange={(e) => setEditingPack({ ...editingPack, name: e.target.value })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Price</label><input type="number" value={editingPack.price} onChange={(e) => setEditingPack({ ...editingPack, price: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                    <div><label className="text-xs text-gray-500">Credits</label><input type="number" value={editingPack.credits} onChange={(e) => setEditingPack({ ...editingPack, credits: Number(e.target.value) })} className="mt-1 block w-full rounded border border-gray-300 px-2 py-1 text-sm" /></div>
                  </div>
                  <div className="flex items-center gap-2">
                    <button onClick={handleSavePack} className="px-3 py-1.5 text-xs rounded bg-blue-600 text-white hover:bg-blue-700">Save Pack</button>
                    <button onClick={() => setEditingPack(null)} className="px-3 py-1.5 text-xs rounded border border-gray-200 hover:bg-gray-50">Cancel</button>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </main>
    </>
  );
}
