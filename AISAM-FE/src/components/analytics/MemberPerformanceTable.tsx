"use client";

import { useState, useMemo } from "react";
import { MemberPerformanceItem } from "@/services/analyticsService";
import { formatNumber } from "./analyticsUtils";

interface MemberPerformanceTableProps {
  members: MemberPerformanceItem[];
  loading?: boolean;
  error?: string | null;
  onRefresh?: () => void;
}

type SortField = "totalContentCreated" | "totalImpressions" | "totalEngagement" | "engagementRate" | "totalPosts";

export default function MemberPerformanceTable({
  members,
  loading = false,
  error = null,
  onRefresh,
}: MemberPerformanceTableProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");
  const [sortField, setSortField] = useState<SortField>("totalContentCreated");
  const [sortAsc, setSortAsc] = useState(false);

  const filteredMembers = useMemo(() => {
    return members
      .filter((m) => {
        const matchSearch =
          m.displayName.toLowerCase().includes(searchTerm.toLowerCase().trim()) ||
          m.email.toLowerCase().includes(searchTerm.toLowerCase().trim());
        if (!matchSearch) return false;
        if (roleFilter === "all") return true;
        return m.role.toLowerCase() === roleFilter.toLowerCase();
      })
      .sort((a, b) => {
        const valA = a[sortField];
        const valB = b[sortField];
        if (valA < valB) return sortAsc ? -1 : 1;
        if (valA > valB) return sortAsc ? 1 : -1;
        return 0;
      });
  }, [members, searchTerm, roleFilter, sortField, sortAsc]);

  const maxImpressions = Math.max(...members.map((m) => m.totalImpressions), 1);

  // Aggregated KPI summary
  const summary = useMemo(() => {
    const totalMembers = members.length;
    const totalCreated = members.reduce((sum, m) => sum + m.totalContentCreated, 0);
    const totalPublished = members.reduce((sum, m) => sum + m.publishedCount, 0);
    const totalImpressions = members.reduce((sum, m) => sum + m.totalImpressions, 0);
    const totalEngagement = members.reduce((sum, m) => sum + m.totalEngagement, 0);
    const avgEngagementRate =
      totalImpressions > 0 ? ((totalEngagement / totalImpressions) * 100).toFixed(2) : "0.00";

    return { totalMembers, totalCreated, totalPublished, totalImpressions, totalEngagement, avgEngagementRate };
  }, [members]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortAsc(!sortAsc);
    } else {
      setSortField(field);
      setSortAsc(false);
    }
  };

  const getRoleBadge = (role: string) => {
    const r = role.toLowerCase();
    if (r.includes("owner")) {
      return { label: "Owner", bg: "bg-amber-500/10 text-amber-600 border-amber-500/20" };
    }
    if (r.includes("manager")) {
      return { label: "Manager", bg: "bg-purple-500/10 text-purple-600 border-purple-500/20" };
    }
    if (r.includes("creator")) {
      return { label: "Creator", bg: "bg-emerald-500/10 text-emerald-600 border-emerald-500/20" };
    }
    return { label: role, bg: "bg-slate-500/10 text-slate-600 border-slate-500/20" };
  };

  return (
    <div className="space-y-6 animate-fade-up">
      {/* Top Stat Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/30 rounded-2xl p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-body-sm font-semibold text-outline">Thành viên theo dõi</span>
            <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
              <span className="material-symbols-outlined text-[18px]">group</span>
            </div>
          </div>
          <div className="text-2xl font-bold text-on-surface mt-2">{summary.totalMembers}</div>
          <p className="text-label-xs text-outline mt-1">Trong phạm vi nhóm quản lý</p>
        </div>

        <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/30 rounded-2xl p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-body-sm font-semibold text-outline">Tổng nội dung tạo</span>
            <div className="w-9 h-9 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-600">
              <span className="material-symbols-outlined text-[18px]">post_add</span>
            </div>
          </div>
          <div className="text-2xl font-bold text-on-surface mt-2">{formatNumber(summary.totalCreated)}</div>
          <p className="text-label-xs text-outline mt-1">{summary.totalPublished} bài đã xuất bản</p>
        </div>

        <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/30 rounded-2xl p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-body-sm font-semibold text-outline">Tổng lượt hiển thị</span>
            <div className="w-9 h-9 rounded-xl bg-blue-500/10 flex items-center justify-center text-blue-600">
              <span className="material-symbols-outlined text-[18px]">visibility</span>
            </div>
          </div>
          <div className="text-2xl font-bold text-on-surface mt-2">{formatNumber(summary.totalImpressions)}</div>
          <p className="text-label-xs text-outline mt-1">{formatNumber(summary.totalEngagement)} lượt tương tác</p>
        </div>

        <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low border border-outline-variant/30 rounded-2xl p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-body-sm font-semibold text-outline">Tỷ lệ tương tác TB</span>
            <div className="w-9 h-9 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-600">
              <span className="material-symbols-outlined text-[18px]">percent</span>
            </div>
          </div>
          <div className="text-2xl font-bold text-on-surface mt-2">{summary.avgEngagementRate}%</div>
          <p className="text-label-xs text-outline mt-1">Đánh giá mức độ thu hút</p>
        </div>
      </div>

      {/* Main Table Container */}
      <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/40 overflow-hidden shadow-lg">
        {/* Header & Controls */}
        <div className="p-6 border-b border-outline-variant/20 flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <h3 className="text-headline-sm font-bold text-on-surface flex items-center gap-2">
              <span className="material-symbols-outlined text-primary text-[22px]">badge</span>
              Bảng Đánh Giá Hiệu Suất Thành Viên
            </h3>
            <p className="text-body-sm text-outline mt-1">
              Thống kê bài viết, lượt tương tác và hiệu quả sáng tạo nội dung của từng nhân viên
            </p>
          </div>

          <div className="flex items-center gap-3 flex-wrap">
            {/* Search Input */}
            <div className="relative min-w-[220px]">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[18px] text-outline">
                search
              </span>
              <input
                type="text"
                placeholder="Tìm tên hoặc email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-9 pr-3 py-2 bg-surface-container border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:border-primary/40 transition-all"
              />
            </div>

            {/* Role Filter */}
            <select
              value={roleFilter}
              onChange={(e) => setRoleFilter(e.target.value)}
              className="px-3 py-2 bg-surface-container border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:border-primary/40 transition-all cursor-pointer"
            >
              <option value="all">Tất cả vai trò</option>
              <option value="contentcreator">Content Creator</option>
              <option value="manager">Manager</option>
            </select>

            {onRefresh && (
              <button
                onClick={onRefresh}
                disabled={loading}
                className="p-2 rounded-xl border border-outline-variant/20 hover:bg-surface-container text-outline hover:text-on-surface transition-all"
                title="Làm mới"
              >
                <span className={`material-symbols-outlined text-[18px] ${loading ? "animate-spin" : ""}`}>
                  refresh
                </span>
              </button>
            )}
          </div>
        </div>

        {/* Content */}
        {loading ? (
          <div className="p-12 text-center space-y-3">
            <div className="w-10 h-10 border-3 border-primary/20 border-t-primary rounded-full animate-spin mx-auto" />
            <p className="text-body-sm text-outline">Đang phân tích dữ liệu thành viên...</p>
          </div>
        ) : error ? (
          <div className="p-8 text-center space-y-2">
            <span className="material-symbols-outlined text-danger-red text-[36px]">error</span>
            <p className="text-body-sm text-danger-red font-medium">{error}</p>
          </div>
        ) : filteredMembers.length === 0 ? (
          <div className="p-12 text-center space-y-2">
            <span className="material-symbols-outlined text-outline/40 text-[48px]">person_search</span>
            <p className="text-body-sm text-on-surface font-semibold">Không tìm thấy thành viên phù hợp</p>
            <p className="text-label-xs text-outline">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc vai trò.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead className="bg-surface-container-high/40 text-label-xs uppercase font-bold text-outline border-b border-outline-variant/20">
                <tr>
                  <th className="px-6 py-3.5">Thành viên</th>
                  <th className="px-4 py-3.5">Vai trò</th>
                  <th
                    className="px-4 py-3.5 cursor-pointer hover:text-primary transition-colors"
                    onClick={() => handleSort("totalContentCreated")}
                  >
                    <div className="flex items-center gap-1">
                      Đã tạo
                      {sortField === "totalContentCreated" && (
                        <span className="material-symbols-outlined text-[14px]">
                          {sortAsc ? "arrow_upward" : "arrow_downward"}
                        </span>
                      )}
                    </div>
                  </th>
                  <th
                    className="px-4 py-3.5 cursor-pointer hover:text-primary transition-colors"
                    onClick={() => handleSort("totalPosts")}
                  >
                    <div className="flex items-center gap-1">
                      Đã đăng
                      {sortField === "totalPosts" && (
                        <span className="material-symbols-outlined text-[14px]">
                          {sortAsc ? "arrow_upward" : "arrow_downward"}
                        </span>
                      )}
                    </div>
                  </th>
                  <th
                    className="px-6 py-3.5 cursor-pointer hover:text-primary transition-colors"
                    onClick={() => handleSort("totalImpressions")}
                  >
                    <div className="flex items-center gap-1">
                      Lượt hiển thị
                      {sortField === "totalImpressions" && (
                        <span className="material-symbols-outlined text-[14px]">
                          {sortAsc ? "arrow_upward" : "arrow_downward"}
                        </span>
                      )}
                    </div>
                  </th>
                  <th
                    className="px-4 py-3.5 cursor-pointer hover:text-primary transition-colors"
                    onClick={() => handleSort("totalEngagement")}
                  >
                    <div className="flex items-center gap-1">
                      Tương tác
                      {sortField === "totalEngagement" && (
                        <span className="material-symbols-outlined text-[14px]">
                          {sortAsc ? "arrow_upward" : "arrow_downward"}
                        </span>
                      )}
                    </div>
                  </th>
                  <th
                    className="px-4 py-3.5 cursor-pointer hover:text-primary transition-colors"
                    onClick={() => handleSort("engagementRate")}
                  >
                    <div className="flex items-center gap-1">
                      Tỷ lệ tương tác
                      {sortField === "engagementRate" && (
                        <span className="material-symbols-outlined text-[14px]">
                          {sortAsc ? "arrow_upward" : "arrow_downward"}
                        </span>
                      )}
                    </div>
                  </th>
                  <th className="px-6 py-3.5 text-right">Trạng thái bài viết</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-outline-variant/10 text-body-sm text-on-surface">
                {filteredMembers.map((member) => {
                  const roleBadge = getRoleBadge(member.role);
                  const impressionBarPercent = (member.totalImpressions / maxImpressions) * 100;

                  return (
                    <tr
                      key={member.userId}
                      className="hover:bg-primary/5 transition-colors duration-150 group"
                    >
                      {/* Member Info */}
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold overflow-hidden shrink-0 border border-primary/20">
                            {member.avatarUrl ? (
                              <img
                                src={member.avatarUrl}
                                alt={member.displayName}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              member.displayName.slice(0, 2).toUpperCase()
                            )}
                          </div>
                          <div className="min-w-0">
                            <p className="font-semibold text-on-surface truncate group-hover:text-primary transition-colors">
                              {member.displayName}
                            </p>
                            <p className="text-label-xs text-outline truncate">{member.email}</p>
                          </div>
                        </div>
                      </td>

                      {/* Role Badge */}
                      <td className="px-4 py-4">
                        <span
                          className={`inline-flex items-center px-2 py-0.5 rounded-md text-label-2xs font-semibold border ${roleBadge.bg}`}
                        >
                          {roleBadge.label}
                        </span>
                      </td>

                      {/* Created Count */}
                      <td className="px-4 py-4 font-semibold tabular-nums">
                        {member.totalContentCreated}
                        {member.totalContentParticipated > 0 && (
                          <span
                            className="text-label-2xs text-outline font-normal ml-1"
                            title="Nội dung cùng tham gia"
                          >
                            (+{member.totalContentParticipated})
                          </span>
                        )}
                      </td>

                      {/* Posts Count */}
                      <td className="px-4 py-4 font-semibold tabular-nums text-emerald-600">
                        {member.publishedCount}
                      </td>

                      {/* Impressions with bar */}
                      <td className="px-6 py-4">
                        <div className="space-y-1 min-w-[130px]">
                          <span className="font-semibold tabular-nums">
                            {formatNumber(member.totalImpressions)}
                          </span>
                          <div className="w-full h-1.5 bg-surface-container rounded-full overflow-hidden">
                            <div
                              className="h-full bg-gradient-to-r from-blue-500 to-indigo-500 rounded-full transition-all duration-500"
                              style={{ width: `${Math.max(impressionBarPercent, 2)}%` }}
                            />
                          </div>
                        </div>
                      </td>

                      {/* Engagement & Clicks */}
                      <td className="px-4 py-4">
                        <div className="tabular-nums">
                          <p className="font-semibold">{formatNumber(member.totalEngagement)}</p>
                          <p className="text-label-2xs text-outline">
                            {formatNumber(member.totalClicks)} clicks
                          </p>
                        </div>
                      </td>

                      {/* Engagement Rate */}
                      <td className="px-4 py-4">
                        <span
                          className={`inline-flex items-center px-2.5 py-1 rounded-lg text-label-xs font-bold tabular-nums ${
                            member.engagementRate >= 5
                              ? "bg-emerald-500/10 text-emerald-600"
                              : member.engagementRate >= 2
                              ? "bg-blue-500/10 text-blue-600"
                              : "bg-surface-container text-outline"
                          }`}
                        >
                          {member.engagementRate}%
                        </span>
                      </td>

                      {/* Content breakdown status pills */}
                      <td className="px-6 py-4 text-right">
                        <div className="inline-flex items-center gap-1.5 text-label-2xs">
                          {member.approvedCount > 0 && (
                            <span
                              className="px-1.5 py-0.5 rounded bg-blue-500/10 text-blue-600 font-medium"
                              title="Đã duyệt"
                            >
                              {member.approvedCount} duyệt
                            </span>
                          )}
                          {member.inReviewCount > 0 && (
                            <span
                              className="px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-600 font-medium"
                              title="Chờ duyệt"
                            >
                              {member.inReviewCount} chờ
                            </span>
                          )}
                          {member.rejectedCount > 0 && (
                            <span
                              className="px-1.5 py-0.5 rounded bg-rose-500/10 text-rose-600 font-medium"
                              title="Bị từ chối"
                            >
                              {member.rejectedCount} từ chối
                            </span>
                          )}
                          {member.draftCount > 0 && (
                            <span
                              className="px-1.5 py-0.5 rounded bg-slate-500/10 text-slate-600 font-medium"
                              title="Nháp"
                            >
                              {member.draftCount} nháp
                            </span>
                          )}
                          {member.totalContentCreated === 0 && (
                            <span className="text-outline text-label-xs">Chưa tạo bài</span>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
