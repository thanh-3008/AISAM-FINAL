"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { useAccessContext } from "@/contexts/AccessContext";

type Summary = { contentCount: number; impressions: number; engagement: number; clicks: number };
type HistoryItem = { id: string; title: string | null; createdAt: string };

export default function OwnAnalyticsPage() {
  const access = useAccessContext();
  const [summary, setSummary] = useState<Summary | null>(null);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [error, setError] = useState("");
  useEffect(() => {
    let cancelled = false;
    setSummary(null); setHistory([]); setError("");
    if (!access?.canViewOwnAnalytics) return;
    Promise.all([
      apiClient("/access/me/analytics", { cache: "no-store" }),
      apiClient(`/access/creator-history/${access.userId}?page=${page}&pageSize=20`, { cache: "no-store" }),
    ]).then(([metrics, records]) => {
      if (!cancelled) { setSummary(metrics.data); setHistory(records.data?.data ?? []); setTotal(records.data?.totalCount ?? 0); }
    }).catch(() => { if (!cancelled) setError("Không thể tải dữ liệu trong phạm vi được phép."); });
    return () => { cancelled = true; };
  }, [access?.workspaceId, access?.userId, access?.version, access?.canViewOwnAnalytics, page]);
  return <main className="p-6 space-y-6">
    <h1 className="text-2xl font-semibold">Analytics và lịch sử cá nhân</h1>
    {error && <p role="alert">{error}</p>}
    {summary && <dl className="grid grid-cols-2 md:grid-cols-4 gap-4">{
      [["Content", summary.contentCount], ["Impressions", summary.impressions], ["Engagement", summary.engagement], ["Clicks", summary.clicks]].map(([label, value]) =>
        <div key={label}><dt>{label}</dt><dd className="text-xl font-semibold">{value}</dd></div>)
    }</dl>}
    <table className="w-full text-left"><thead><tr><th>Nội dung</th><th>Ngày tạo</th></tr></thead><tbody>
      {history.map(item => <tr key={item.id}><td>{item.title || "Chưa có tiêu đề"}</td><td>{new Date(item.createdAt).toLocaleDateString()}</td></tr>)}
    </tbody></table>
    <div className="flex gap-4"><button disabled={page === 1} onClick={() => setPage(p => p - 1)}>Trước</button>
      <span>Trang {page}</span><button disabled={page * 20 >= total} onClick={() => setPage(p => p + 1)}>Sau</button></div>
  </main>;
}
