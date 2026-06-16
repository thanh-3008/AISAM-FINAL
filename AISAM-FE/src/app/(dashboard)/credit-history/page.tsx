"use client";

import { useState, useEffect } from "react";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchCreditUsageHistory, type CreditUsageRecord } from "@/services/workspaceService";

function getActionIcon(action: string): string {
  switch (action.toLowerCase()) {
    case "generate text":
      return "text_fields";
    case "generate image":
      return "image";
    case "generate video":
      return "videocam";
    case "regenerate":
    case "refine":
      return "refresh";
    case "trend analysis":
      return "trending_up";
    case "campaign recommendation":
      return "campaign";
    default:
      return "auto_awesome";
  }
}

function getActionColor(action: string): string {
  switch (action.toLowerCase()) {
    case "generate text":
      return "text-blue-500 bg-blue-50";
    case "generate image":
      return "text-purple-500 bg-purple-50";
    case "generate video":
      return "text-pink-500 bg-pink-50";
    case "regenerate":
    case "refine":
      return "text-amber-500 bg-amber-50";
    case "trend analysis":
      return "text-emerald-500 bg-emerald-50";
    case "campaign recommendation":
      return "text-indigo-500 bg-indigo-50";
    default:
      return "text-primary bg-primary/5";
  }
}

export default function CreditHistoryPage() {
  const { activeWorkspace } = useWorkspaces();
  const [history, setHistory] = useState<CreditUsageRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<"all" | "success" | "failed">("all");

  useEffect(() => {
    const loadHistory = async () => {
      setLoading(true);
      try {
        const data = await fetchCreditUsageHistory(page, 10);
        if (data) {
          setHistory(data.data);
          setTotalPages(data.totalPages);
          setTotalCount(data.totalCount);
        }
      } catch (error) {
        console.error("Failed to load credit history:", error);
      } finally {
        setLoading(false);
      }
    };
    loadHistory();
  }, [page, activeWorkspace?.id]);

  const filteredHistory = history.filter((record) => {
    if (filter === "all") return true;
    return record.status.toLowerCase() === filter;
  });

  const totalCreditsUsed = history
    .filter((r) => r.status === "Success")
    .reduce((sum, r) => sum + r.credits, 0);

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Credit History" },
      ]} />
      <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-6xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <span className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500/10 to-emerald-600/10 text-emerald-500 flex items-center justify-center">
                <span className="material-symbols-outlined text-[22px]">history</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Credit Usage History</h1>
                <p className="text-body-sm text-on-surface-variant">
                  Track your AI credit consumption
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-surface-container-lowest border border-outline-variant/20">
              <span className="material-symbols-outlined text-emerald-500 text-[20px]">token</span>
              <span className="text-body-sm font-semibold text-on-surface">{totalCreditsUsed}</span>
              <span className="text-body-sm text-outline">credits used</span>
            </div>
          </div>

          {/* Filters */}
          <div className="flex items-center gap-2">
            {[
              { key: "all" as const, label: "All", count: totalCount },
              { key: "success" as const, label: "Success", count: history.filter((r) => r.status === "Success").length },
              { key: "failed" as const, label: "Failed", count: history.filter((r) => r.status === "Failed").length },
            ].map((f) => (
              <button
                key={f.key}
                onClick={() => setFilter(f.key)}
                className={`px-4 py-2 rounded-xl text-label-sm font-medium transition-all ${
                  filter === f.key
                    ? "bg-primary text-on-primary shadow-sm"
                    : "bg-surface-container-lowest text-on-surface-variant hover:bg-surface-container border border-outline-variant/20"
                }`}
              >
                {f.label}
                <span className={`ml-2 px-1.5 py-0.5 rounded-full text-label-xs ${
                  filter === f.key ? "bg-white/20" : "bg-surface-container"
                }`}>
                  {f.count}
                </span>
              </button>
            ))}
          </div>

          {/* History Table */}
          {loading ? (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
              <div className="space-y-4">
                {[1, 2, 3, 4, 5].map((i) => (
                  <div key={i} className="flex items-center gap-4 animate-pulse">
                    <div className="w-10 h-10 rounded-xl bg-surface-container" />
                    <div className="flex-1 space-y-2">
                      <div className="h-4 w-48 bg-surface-container rounded" />
                      <div className="h-3 w-32 bg-surface-container rounded" />
                    </div>
                    <div className="h-6 w-16 bg-surface-container rounded" />
                  </div>
                ))}
              </div>
            </div>
          ) : filteredHistory.length === 0 ? (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-12 text-center">
              <span className="material-symbols-outlined text-outline/40 text-5xl mb-4 block">history</span>
              <p className="text-body-md text-on-surface font-semibold mb-2">No credit usage yet</p>
              <p className="text-body-sm text-on-surface-variant">
                Your AI generation history will appear here
              </p>
            </div>
          ) : (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 overflow-hidden">
              {/* Table Header */}
              <div className="grid grid-cols-12 gap-4 px-6 py-3 bg-surface-container/50 border-b border-outline-variant/20 text-label-sm font-semibold text-outline">
                <div className="col-span-5">Action</div>
                <div className="col-span-3">Feature</div>
                <div className="col-span-2 text-center">Credits</div>
                <div className="col-span-2 text-right">Time</div>
              </div>

              {/* Table Body */}
              <div className="divide-y divide-outline-variant/10">
                {filteredHistory.map((record) => {
                  const actionIcon = getActionIcon(record.action);
                  const actionColor = getActionColor(record.action);
                  return (
                    <div
                      key={record.id}
                      className="grid grid-cols-12 gap-4 px-6 py-4 hover:bg-surface-container/30 transition-colors"
                    >
                      {/* Action */}
                      <div className="col-span-5 flex items-center gap-3">
                        <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${actionColor}`}>
                          <span className="material-symbols-outlined text-[20px]">{actionIcon}</span>
                        </div>
                        <div>
                          <p className="text-body-sm text-on-surface font-medium">{record.action}</p>
                          <p className="text-label-xs text-outline">{record.userName}</p>
                        </div>
                      </div>

                      {/* Feature */}
                      <div className="col-span-3 flex items-center">
                        <span className="text-body-sm text-on-surface-variant">{record.featureUsed}</span>
                      </div>

                      {/* Credits */}
                      <div className="col-span-2 flex items-center justify-center">
                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-sm font-semibold ${
                          record.status === "Success"
                            ? "bg-emerald-50 text-emerald-700"
                            : "bg-red-50 text-red-700"
                        }`}>
                          {record.status === "Failed" && (
                            <span className="material-symbols-outlined text-[14px]">close</span>
                          )}
                          {record.status === "Success" ? `-${record.credits}` : "0"}
                        </span>
                      </div>

                      {/* Time */}
                      <div className="col-span-2 flex items-center justify-end">
                        <span className="text-label-sm text-outline">
                          {new Date(record.createdAt).toLocaleDateString("en-US", {
                            month: "short",
                            day: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </span>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="flex items-center justify-between px-6 py-4 border-t border-outline-variant/20">
                  <span className="text-label-sm text-outline">
                    Page {page} of {totalPages}
                  </span>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                      className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Previous
                    </button>
                    <button
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page === totalPages}
                      className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Summary Card */}
          <div className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
            <div className="flex items-start gap-4">
              <div className="w-12 h-12 rounded-xl bg-white/80 flex items-center justify-center shrink-0">
                <span className="material-symbols-outlined text-primary text-[24px]">info</span>
              </div>
              <div>
                <h4 className="text-body-md font-semibold text-on-surface mb-1">About Credit Usage</h4>
                <ul className="space-y-1.5 text-body-sm text-on-surface-variant">
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                    Text generation costs 1 credit per request
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                    Image generation costs 5 credits per request
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                    Video generation costs 20 credits per request
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                    Failed requests do not consume credits
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  );
}
