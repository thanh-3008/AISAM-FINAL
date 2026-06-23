"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { fetchWorkspaceDashboard, fetchCreditWallet, fetchPostQuota } from "@/services/workspaceService";
import type { WorkspaceDashboard, CreditWallet } from "@/services/workspaceService";
import CreateProfileModal from "@/components/profiles/CreateProfileModal";

export default function WorkspaceDashboardPage() {
  const { activeWorkspace } = useWorkspaces();
  const featureGate = useFeatureGate();
  const [dashboard, setDashboard] = useState<WorkspaceDashboard | null>(null);
  const [creditWallet, setCreditWallet] = useState<CreditWallet | null>(null);
  const [postQuota, setPostQuota] = useState<{ used: number; total: number } | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        const [dashData, walletData, quotaData] = await Promise.all([
          fetchWorkspaceDashboard(),
          fetchCreditWallet(),
          fetchPostQuota(),
        ]);
        setDashboard(dashData);
        setCreditWallet(walletData);
        setPostQuota(quotaData);
      } catch (error) {
        console.error("Failed to load workspace dashboard:", error);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, [activeWorkspace?.id]);

  const creditsUsed = creditWallet ? creditWallet.maxBalance - creditWallet.balance : 0;
  const creditsPct = creditWallet ? Math.round((creditsUsed / creditWallet.maxBalance) * 100) : 0;
  const postsPct = postQuota ? Math.round((postQuota.used / postQuota.total) * 100) : 0;

  if (!featureGate.canAccess("workspaceDashboard")) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Workspace Overview" }]} />
        <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="max-w-7xl mx-auto flex items-center justify-center min-h-[60vh]">
            <div className="text-center max-w-md">
              <div className="w-16 h-16 mx-auto mb-6 bg-outline/10 rounded-2xl flex items-center justify-center">
                <span className="material-symbols-outlined text-outline text-[32px]">lock</span>
              </div>
              <h2 className="text-headline-md text-on-surface font-bold mb-2">Workspace Dashboard</h2>
              <p className="text-body-md text-on-surface-variant mb-6">This feature requires a <strong>Business plan</strong>. Upgrade to access workspace-level analytics and overview.</p>
              <div className="flex items-center justify-center gap-3">
                <Link href="/pricing" className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all">
                  View Plans
                  <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
                </Link>
                <button
                  onClick={() => setShowCreateModal(true)}
                  className="inline-flex items-center gap-2 px-6 py-3 bg-surface-container-high text-on-surface rounded-xl text-label-sm font-bold hover:scale-105 transition-all"
                >
                  <span className="material-symbols-outlined text-[16px]">add</span>
                  Create Workspace
                </button>
              </div>
            </div>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Workspace Overview" },
      ]} />
      <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              <span className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-secondary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[22px]">dashboard</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Workspace Dashboard</h1>
                <p className="text-body-sm text-on-surface-variant">
                  {activeWorkspace?.name || "Workspace"} - Overview & Analytics
                </p>
              </div>
            </div>
            <button
              onClick={() => setShowCreateModal(true)}
              className="flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all shrink-0"
            >
              <span className="material-symbols-outlined text-[16px]">add</span>
              Create Workspace
            </button>
          </div>

          {loading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 animate-pulse">
                  <div className="h-4 w-24 bg-surface-container rounded mb-3" />
                  <div className="h-8 w-32 bg-surface-container rounded" />
                </div>
              ))}
            </div>
          ) : (
            <>
              {/* KPI Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {/* Credits Remaining */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500/20 to-emerald-600/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-emerald-500 text-[20px]">token</span>
                    </div>
                    <span className="text-label-sm text-on-surface-variant font-medium">Credits</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-baseline gap-2">
                      <span className="text-kpi-lg text-on-surface">{creditWallet?.balance.toLocaleString() || 0}</span>
                      <span className="text-label-sm text-outline">/ {creditWallet?.maxBalance.toLocaleString() || 0}</span>
                    </div>
                    <div className="h-2 bg-surface-container rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-emerald-400 to-emerald-500 rounded-full transition-all duration-500"
                        style={{ width: `${100 - creditsPct}%` }}
                      />
                    </div>
                    <p className="text-label-xs text-outline">{100 - creditsPct}% remaining</p>
                  </div>
                </div>

                {/* Posts This Month */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500/20 to-blue-600/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-blue-500 text-[20px]">send</span>
                    </div>
                    <span className="text-label-sm text-on-surface-variant font-medium">Posts</span>
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-baseline gap-2">
                      <span className="text-kpi-lg text-on-surface">{postQuota?.used.toLocaleString() || 0}</span>
                      <span className="text-label-sm text-outline">/ {postQuota?.total.toLocaleString() || 0}</span>
                    </div>
                    <div className="h-2 bg-surface-container rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-blue-400 to-blue-500 rounded-full transition-all duration-500"
                        style={{ width: `${postsPct}%` }}
                      />
                    </div>
                    <p className="text-label-xs text-outline">{100 - postsPct}% remaining this month</p>
                  </div>
                </div>

                {/* Total AI Usage */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500/20 to-purple-600/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-purple-500 text-[20px]">auto_awesome</span>
                    </div>
                    <span className="text-label-sm text-on-surface-variant font-medium">AI Usage</span>
                  </div>
                  <div className="space-y-2">
                    <span className="text-kpi-lg text-on-surface">{dashboard?.aiUsageCount.toLocaleString() || 0}</span>
                    <p className="text-label-xs text-outline">Total generations this month</p>
                  </div>
                </div>

                {/* Workspace Type */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500/20 to-amber-600/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-amber-500 text-[20px]">
                        {activeWorkspace?.workspaceType === 2 ? "business" : "person"}
                      </span>
                    </div>
                    <span className="text-label-sm text-on-surface-variant font-medium">Type</span>
                  </div>
                  <div className="space-y-2">
                    <span className="text-body-lg text-on-surface font-semibold">
                      {activeWorkspace?.workspaceType === 2 ? "Business" : "Personal"}
                    </span>
                    <p className="text-label-xs text-outline">
                      {activeWorkspace?.workspaceType === 2 ? "Team workspace" : "Individual workspace"}
                    </p>
                  </div>
                </div>
              </div>

              {/* Top Members & Usage Chart */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Top Members by Usage */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center justify-between mb-5">
                    <div className="flex items-center gap-2">
                      <span className="material-symbols-outlined text-primary text-[20px]">leaderboard</span>
                      <h3 className="text-headline-sm text-on-surface">Top Members</h3>
                    </div>
                    <span className="text-label-xs text-outline">By AI Usage</span>
                  </div>
                  <div className="space-y-3">
                    {dashboard?.topMembers && dashboard.topMembers.length > 0 ? (
                      dashboard.topMembers.map((member, index) => {
                        const maxUsage = dashboard.topMembers[0]?.usage || 1;
                        const pct = Math.round((member.usage / maxUsage) * 100);
                        return (
                          <div key={member.userId} className="flex items-center gap-3">
                            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-label-sm font-bold text-primary">
                              {index + 1}
                            </div>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center justify-between mb-1">
                                <span className="text-body-sm text-on-surface font-medium truncate">{member.name}</span>
                                <span className="text-label-sm text-outline">{member.usage} credits</span>
                              </div>
                              <div className="h-1.5 bg-surface-container rounded-full overflow-hidden">
                                <div
                                  className="h-full bg-gradient-to-r from-primary to-primary-container rounded-full transition-all duration-500"
                                  style={{ width: `${pct}%` }}
                                />
                              </div>
                            </div>
                          </div>
                        );
                      })
                    ) : (
                      <div className="text-center py-8">
                        <span className="material-symbols-outlined text-outline/40 text-4xl mb-2 block">group_off</span>
                        <p className="text-body-sm text-on-surface-variant">No member data yet</p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Usage Breakdown */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                  <div className="flex items-center justify-between mb-5">
                    <div className="flex items-center gap-2">
                      <span className="material-symbols-outlined text-secondary text-[20px]">pie_chart</span>
                      <h3 className="text-headline-sm text-on-surface">Usage Breakdown</h3>
                    </div>
                  </div>
                  <div className="space-y-4">
                    {[
                      { label: "Text Generation", value: 45, color: "from-blue-400 to-blue-500", icon: "text_fields" },
                      { label: "Image Generation", value: 30, color: "from-purple-400 to-purple-500", icon: "image" },
                      { label: "Video Generation", value: 15, color: "from-pink-400 to-pink-500", icon: "videocam" },
                      { label: "Other", value: 10, color: "from-gray-400 to-gray-500", icon: "more_horiz" },
                    ].map((item) => (
                      <div key={item.label} className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-surface-container flex items-center justify-center">
                          <span className="material-symbols-outlined text-on-surface-variant text-[16px]">{item.icon}</span>
                        </div>
                        <div className="flex-1">
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-body-sm text-on-surface">{item.label}</span>
                            <span className="text-label-sm text-outline">{item.value}%</span>
                          </div>
                          <div className="h-1.5 bg-surface-container rounded-full overflow-hidden">
                            <div
                              className={`h-full bg-gradient-to-r ${item.color} rounded-full transition-all duration-500`}
                              style={{ width: `${item.value}%` }}
                            />
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              {/* Quick Actions */}
              <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
                <h3 className="text-headline-sm text-on-surface mb-4">Quick Actions</h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  <Link
                    href="/content/ai-generate"
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-surface-container hover:bg-surface-container-high transition-colors"
                  >
                    <span className="material-symbols-outlined text-primary text-[28px]">auto_awesome</span>
                    <span className="text-label-sm text-on-surface font-medium">Generate Content</span>
                  </Link>
                  <Link
                    href="/posts"
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-surface-container hover:bg-surface-container-high transition-colors"
                  >
                    <span className="material-symbols-outlined text-blue-500 text-[28px]">send</span>
                    <span className="text-label-sm text-on-surface font-medium">View Posts</span>
                  </Link>
                  <Link
                    href={activeWorkspace ? `/profiles/${activeWorkspace.id}?section=billing` : "/profiles"}
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-surface-container hover:bg-surface-container-high transition-colors"
                  >
                    <span className="material-symbols-outlined text-emerald-500 text-[28px]">token</span>
                    <span className="text-label-sm text-on-surface font-medium">Buy Credits</span>
                  </Link>
                  <Link
                    href="/team"
                    className="flex flex-col items-center gap-2 p-4 rounded-xl bg-surface-container hover:bg-surface-container-high transition-colors"
                  >
                    <span className="material-symbols-outlined text-purple-500 text-[28px]">group</span>
                    <span className="text-label-sm text-on-surface font-medium">Manage Team</span>
                  </Link>
                </div>
              </div>
            </>
          )}
        </div>
      </main>

      <CreateProfileModal open={showCreateModal} onClose={() => setShowCreateModal(false)} />
    </>
  );
}
