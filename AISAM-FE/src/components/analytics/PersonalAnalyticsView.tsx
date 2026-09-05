"use client";

import { useEffect, useState, useMemo } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { useAccessContext } from "@/contexts/AccessContext";
import { formatNumber } from "./analyticsUtils";

type Summary = {
  contentCount: number;
  impressions: number;
  engagement: number;
  clicks: number;
};

type HistoryItem = {
  id: string;
  title: string | null;
  status?: string | number | null;
  createdAt: string;
};

function getStatusBadge(status?: string | number | null) {
  const str = String(status || "").toLowerCase();
  if (str.includes("publish") || str === "3") {
    return {
      label: "Published",
      className: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20",
      dot: "bg-emerald-500",
    };
  }
  if (str.includes("approve") || str === "2") {
    return {
      label: "Approved",
      className: "bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20",
      dot: "bg-blue-500",
    };
  }
  if (str.includes("pending") || str.includes("review") || str === "1") {
    return {
      label: "In Review",
      className: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20",
      dot: "bg-amber-500",
    };
  }
  if (str.includes("reject") || str === "4") {
    return {
      label: "Rejected",
      className: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20",
      dot: "bg-rose-500",
    };
  }
  return {
    label: "Draft",
    className: "bg-slate-500/10 text-slate-600 dark:text-slate-400 border-slate-500/20",
    dot: "bg-slate-400",
  };
}

export default function PersonalAnalyticsView() {
  const access = useAccessContext();
  const [summary, setSummary] = useState<Summary | null>(null);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const pageSize = 20;

  const loadData = async (currentPage: number) => {
    if (!access?.userId) return;
    setLoading(true);
    setError("");
    try {
      const [metricsRes, historyRes] = await Promise.all([
        apiClient("/access/me/analytics", { cache: "no-store" }),
        apiClient(
          `/access/creator-history/${access.userId}?page=${currentPage}&pageSize=${pageSize}`,
          { cache: "no-store" }
        ),
      ]);

      const metricsData = (metricsRes as any)?.data?.contentCount !== undefined
        ? (metricsRes as any).data
        : (metricsRes as any)?.contentCount !== undefined
        ? (metricsRes as any)
        : null;

      const recordsData = (historyRes as any)?.data?.data ?? (historyRes as any)?.data ?? [];
      const totalCount = (historyRes as any)?.data?.totalCount ?? (historyRes as any)?.totalCount ?? 0;

      setSummary(metricsData);
      setHistory(Array.isArray(recordsData) ? recordsData : []);
      setTotal(totalCount);
    } catch {
      setError("Không thể tải dữ liệu phân tích cá nhân. Vui lòng kiểm tra quyền truy cập hoặc thử lại.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (access?.canViewOwnAnalytics && access?.userId) {
      loadData(page);
    }
  }, [access?.workspaceId, access?.userId, access?.version, access?.canViewOwnAnalytics, page]);

  const filteredHistory = useMemo(() => {
    return history.filter((item) => {
      const titleMatch = (item.title || "Chưa có tiêu đề").toLowerCase().includes(searchTerm.toLowerCase().trim());
      if (!titleMatch) return false;

      if (statusFilter === "all") return true;
      const badge = getStatusBadge(item.status);
      return badge.label.toLowerCase() === statusFilter.toLowerCase();
    });
  }, [history, searchTerm, statusFilter]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div className="space-y-6">
      {/* Overview Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {/* Metric 1: Contents */}
        <div className="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/40 p-5 shadow-lg transition-all duration-300 hover:shadow-xl hover:-translate-y-0.5">
          <div className="absolute top-0 right-0 w-24 h-24 bg-emerald-500/10 rounded-full blur-2xl group-hover:bg-emerald-500/20 transition-all duration-500" />
          <div className="flex items-center justify-between relative z-10 mb-4">
            <span className="text-body-sm font-semibold text-outline">Nội dung đã tạo</span>
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white shadow-md shadow-emerald-500/20">
              <span className="material-symbols-outlined text-[20px]">post_add</span>
            </div>
          </div>
          <div className="relative z-10">
            <div className="text-3xl font-bold tracking-tight text-on-surface">
              {loading ? (
                <div className="h-8 w-16 bg-surface-container animate-pulse rounded-lg" />
              ) : (
                formatNumber(summary?.contentCount ?? 0)
              )}
            </div>
            <p className="text-label-xs text-outline/80 mt-1 flex items-center gap-1">
              <span className="material-symbols-outlined text-[14px] text-emerald-500">check_circle</span>
              Bài viết & ấn phẩm do bạn sáng tạo
            </p>
          </div>
        </div>

        {/* Metric 2: Impressions */}
        <div className="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/40 p-5 shadow-lg transition-all duration-300 hover:shadow-xl hover:-translate-y-0.5">
          <div className="absolute top-0 right-0 w-24 h-24 bg-blue-500/10 rounded-full blur-2xl group-hover:bg-blue-500/20 transition-all duration-500" />
          <div className="flex items-center justify-between relative z-10 mb-4">
            <span className="text-body-sm font-semibold text-outline">Lượt hiển thị</span>
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white shadow-md shadow-blue-500/20">
              <span className="material-symbols-outlined text-[20px]">visibility</span>
            </div>
          </div>
          <div className="relative z-10">
            <div className="text-3xl font-bold tracking-tight text-on-surface">
              {loading ? (
                <div className="h-8 w-20 bg-surface-container animate-pulse rounded-lg" />
              ) : (
                formatNumber(summary?.impressions ?? 0)
              )}
            </div>
            <p className="text-label-xs text-outline/80 mt-1 flex items-center gap-1">
              <span className="material-symbols-outlined text-[14px] text-blue-500">insights</span>
              Lượt hiển thị tổng từ các kênh
            </p>
          </div>
        </div>

        {/* Metric 3: Engagements */}
        <div className="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/40 p-5 shadow-lg transition-all duration-300 hover:shadow-xl hover:-translate-y-0.5">
          <div className="absolute top-0 right-0 w-24 h-24 bg-purple-500/10 rounded-full blur-2xl group-hover:bg-purple-500/20 transition-all duration-500" />
          <div className="flex items-center justify-between relative z-10 mb-4">
            <span className="text-body-sm font-semibold text-outline">Lượt tương tác</span>
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500 to-pink-600 flex items-center justify-center text-white shadow-md shadow-purple-500/20">
              <span className="material-symbols-outlined text-[20px]">forum</span>
            </div>
          </div>
          <div className="relative z-10">
            <div className="text-3xl font-bold tracking-tight text-on-surface">
              {loading ? (
                <div className="h-8 w-20 bg-surface-container animate-pulse rounded-lg" />
              ) : (
                formatNumber(summary?.engagement ?? 0)
              )}
            </div>
            <p className="text-label-xs text-outline/80 mt-1 flex items-center gap-1">
              <span className="material-symbols-outlined text-[14px] text-purple-500">favorite</span>
              Thích, bình luận & chia sẻ
            </p>
          </div>
        </div>

        {/* Metric 4: Clicks */}
        <div className="group relative overflow-hidden rounded-2xl bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/40 p-5 shadow-lg transition-all duration-300 hover:shadow-xl hover:-translate-y-0.5">
          <div className="absolute top-0 right-0 w-24 h-24 bg-orange-500/10 rounded-full blur-2xl group-hover:bg-orange-500/20 transition-all duration-500" />
          <div className="flex items-center justify-between relative z-10 mb-4">
            <span className="text-body-sm font-semibold text-outline">Lượt nhấp (Clicks)</span>
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-orange-500 to-amber-600 flex items-center justify-center text-white shadow-md shadow-orange-500/20">
              <span className="material-symbols-outlined text-[20px]">ads_click</span>
            </div>
          </div>
          <div className="relative z-10">
            <div className="text-3xl font-bold tracking-tight text-on-surface">
              {loading ? (
                <div className="h-8 w-16 bg-surface-container animate-pulse rounded-lg" />
              ) : (
                formatNumber(summary?.clicks ?? 0)
              )}
            </div>
            <p className="text-label-xs text-outline/80 mt-1 flex items-center gap-1">
              <span className="material-symbols-outlined text-[14px] text-orange-500">touch_app</span>
              Chuyển hướng từ liên kết
            </p>
          </div>
        </div>
      </div>

      {/* History Table Container */}
      <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/40 overflow-hidden shadow-xl">
        {/* Table Filter Header */}
        <div className="p-5 md:p-6 border-b border-outline-variant/30 flex flex-col md:flex-row md:items-center justify-between gap-4 bg-gradient-to-r from-primary/5 via-transparent to-transparent">
          <div>
            <h2 className="text-title-lg font-bold text-on-surface flex items-center gap-2">
              <span className="material-symbols-outlined text-primary text-[22px]">history_edu</span>
              Lịch sử bài viết & nội dung
            </h2>
            <p className="text-body-sm text-outline mt-0.5">
              Danh sách các nội dung bạn đã đóng góp và sáng tạo trong workspace này ({total} bài viết)
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {/* Search Box */}
            <div className="relative min-w-[220px]">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[18px]">
                search
              </span>
              <input
                type="text"
                placeholder="Tìm tiêu đề..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-9 pr-3 py-2 bg-surface-container-high/60 hover:bg-surface-container-high focus:bg-surface-container-lowest border border-outline-variant/40 focus:border-primary rounded-xl text-body-sm text-on-surface outline-none transition-all placeholder:text-outline/60"
              />
            </div>

            {/* Status Filter */}
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="px-3 py-2 bg-surface-container-high/60 hover:bg-surface-container-high border border-outline-variant/40 focus:border-primary rounded-xl text-body-sm text-on-surface outline-none cursor-pointer transition-all"
            >
              <option value="all">Tất cả trạng thái</option>
              <option value="draft">Bản nháp (Draft)</option>
              <option value="in review">Đang duyệt (In Review)</option>
              <option value="approved">Đã duyệt (Approved)</option>
              <option value="published">Đã đăng (Published)</option>
            </select>

            {/* Reload Button */}
            <button
              onClick={() => loadData(page)}
              disabled={loading}
              className="w-10 h-10 rounded-xl bg-surface-container-high/60 hover:bg-surface-container-high border border-outline-variant/40 flex items-center justify-center text-outline hover:text-primary transition-all disabled:opacity-50"
              title="Làm mới dữ liệu"
            >
              <span className={`material-symbols-outlined text-[20px] ${loading ? "animate-spin" : ""}`}>
                refresh
              </span>
            </button>
          </div>
        </div>

        {/* Error Notification */}
        {error && (
          <div className="m-5 p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-600 dark:text-rose-400 flex items-center gap-3 text-body-sm" role="alert">
            <span className="material-symbols-outlined text-[20px] shrink-0">error</span>
            <span>{error}</span>
          </div>
        )}

        {/* Table Contents */}
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead className="bg-surface-container-high/40 border-b border-outline-variant/30">
              <tr>
                <th className="px-6 py-3.5 text-label-sm font-bold text-outline uppercase tracking-wider">
                  Nội dung bài viết
                </th>
                <th className="px-6 py-3.5 text-label-sm font-bold text-outline uppercase tracking-wider w-44">
                  Trạng thái
                </th>
                <th className="px-6 py-3.5 text-label-sm font-bold text-outline uppercase tracking-wider w-48">
                  Thời gian tạo
                </th>
                <th className="px-6 py-3.5 text-right text-label-sm font-bold text-outline uppercase tracking-wider w-36">
                  Thao tác
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-outline-variant/20">
              {loading ? (
                Array.from({ length: 5 }).map((_, idx) => (
                  <tr key={idx} className="animate-pulse">
                    <td className="px-6 py-4">
                      <div className="h-5 bg-surface-container rounded w-3/4 mb-1" />
                      <div className="h-3 bg-surface-container/60 rounded w-1/4" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-6 w-24 bg-surface-container rounded-full" />
                    </td>
                    <td className="px-6 py-4">
                      <div className="h-4 w-28 bg-surface-container rounded" />
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="h-8 w-20 bg-surface-container rounded-lg ml-auto" />
                    </td>
                  </tr>
                ))
              ) : filteredHistory.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center">
                    <div className="w-14 h-14 mx-auto mb-3 bg-outline-variant/30 rounded-2xl flex items-center justify-center text-outline">
                      <span className="material-symbols-outlined text-[28px]">article</span>
                    </div>
                    <p className="text-body-md font-semibold text-on-surface">
                      {searchTerm || statusFilter !== "all"
                        ? "Không tìm thấy nội dung phù hợp với bộ lọc."
                        : "Chưa có nội dung nào được tạo."}
                    </p>
                    <p className="text-body-sm text-outline mt-1 mb-4">
                      {searchTerm || statusFilter !== "all"
                        ? "Hãy thử tìm kiếm với từ khóa khác hoặc xóa bộ lọc."
                        : "Bắt đầu sáng tạo bài viết mới để xem dữ liệu phân tích tại đây."}
                    </p>
                    <Link
                      href="/content"
                      className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-md shadow-primary/20 hover:scale-105 transition-all"
                    >
                      <span className="material-symbols-outlined text-[18px]">add</span>
                      Tạo nội dung mới
                    </Link>
                  </td>
                </tr>
              ) : (
                filteredHistory.map((item) => {
                  const badge = getStatusBadge(item.status);
                  const createdDate = new Date(item.createdAt);
                  const formattedDate = isNaN(createdDate.getTime())
                    ? item.createdAt
                    : createdDate.toLocaleString("vi-VN", {
                        year: "numeric",
                        month: "2-digit",
                        day: "2-digit",
                        hour: "2-digit",
                        minute: "2-digit",
                      });

                  return (
                    <tr
                      key={item.id}
                      className="group hover:bg-surface-container-high/30 transition-colors duration-150"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-9 h-9 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0 group-hover:scale-110 transition-transform">
                            <span className="material-symbols-outlined text-[18px]">description</span>
                          </div>
                          <div className="min-w-0">
                            <p className="text-body-md font-semibold text-on-surface truncate max-w-md group-hover:text-primary transition-colors">
                              {item.title || "Chưa có tiêu đề"}
                            </p>
                            <p className="text-label-xs text-outline font-mono truncate">
                              ID: {item.id.slice(0, 8)}...
                            </p>
                          </div>
                        </div>
                      </td>

                      <td className="px-6 py-4">
                        <span
                          className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-semibold border ${badge.className}`}
                        >
                          <span className={`w-1.5 h-1.5 rounded-full ${badge.dot}`} />
                          {badge.label}
                        </span>
                      </td>

                      <td className="px-6 py-4">
                        <div className="flex items-center gap-1.5 text-body-sm text-outline">
                          <span className="material-symbols-outlined text-[16px]">schedule</span>
                          <span>{formattedDate}</span>
                        </div>
                      </td>

                      <td className="px-6 py-4 text-right">
                        <Link
                          href={`/content`}
                          className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg bg-surface-container-high/60 hover:bg-primary hover:text-on-primary text-outline font-medium text-label-xs transition-all duration-200"
                        >
                          <span>Xem chi tiết</span>
                          <span className="material-symbols-outlined text-[14px]">arrow_forward</span>
                        </Link>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Bar */}
        <div className="p-4 border-t border-outline-variant/30 flex flex-col sm:flex-row items-center justify-between gap-3 bg-surface-container-high/20">
          <div className="text-body-sm text-outline">
            Hiển thị trang <strong className="text-on-surface">{page}</strong> / {totalPages} (Tổng cộng {total} mục)
          </div>

          <div className="flex items-center gap-2">
            <button
              disabled={page <= 1 || loading}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              className="inline-flex items-center gap-1 px-3 py-1.5 rounded-xl border border-outline-variant/40 bg-surface-container-lowest text-body-sm font-medium text-on-surface hover:bg-surface-container disabled:opacity-40 disabled:cursor-not-allowed transition-all"
            >
              <span className="material-symbols-outlined text-[16px]">chevron_left</span>
              Trang trước
            </button>

            <span className="px-3 py-1 text-label-sm font-bold text-primary bg-primary/10 rounded-lg">
              {page}
            </span>

            <button
              disabled={page >= totalPages || page * pageSize >= total || loading}
              onClick={() => setPage((p) => p + 1)}
              className="inline-flex items-center gap-1 px-3 py-1.5 rounded-xl border border-outline-variant/40 bg-surface-container-lowest text-body-sm font-medium text-on-surface hover:bg-surface-container disabled:opacity-40 disabled:cursor-not-allowed transition-all"
            >
              Trang sau
              <span className="material-symbols-outlined text-[16px]">chevron_right</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
