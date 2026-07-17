"use client";

import { type TopPostItem } from "@/services/analyticsService";
import { PLATFORM_CONFIG } from "@/lib/contentConstants";
import { formatNumber, formatPercent } from "./analyticsUtils";

interface AnalyticsTopPostsProps {
  posts: TopPostItem[];
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  return d.toLocaleDateString("en-US", { day: "numeric", month: "short", year: "numeric" });
}

export default function AnalyticsTopPosts({ posts }: AnalyticsTopPostsProps) {
  const maxEngagement = Math.max(...posts.map((p) => p.engagement), 1);

  return (
    <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 overflow-hidden shadow-xl animate-fade-up" style={{ animationDelay: "0.5s" }}>
      <div className="p-6 border-b border-outline-variant/30 bg-gradient-to-r from-secondary/5 to-transparent">
        <div>
          <h4 className="text-headline-sm text-on-surface">
            Top Posts Performance
          </h4>
          <p className="text-body-sm text-outline mt-1">Post engagement and reach metrics</p>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-surface-container-high/50">
            <tr>
              <th className="px-5 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Post
              </th>
              <th className="px-5 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Brand
              </th>
              <th className="px-5 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Platform
              </th>
              <th className="px-5 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Published
              </th>
              <th className="px-5 py-4 text-right text-label-sm font-bold text-outline uppercase tracking-wider">
                Impressions
              </th>
              <th className="px-5 py-4 text-right text-label-sm font-bold text-outline uppercase tracking-wider">
                Engagement
              </th>
              <th className="px-5 py-4 text-right text-label-sm font-bold text-outline uppercase tracking-wider">
                Clicks
              </th>
              <th className="px-5 py-4 text-right text-label-sm font-bold text-outline uppercase tracking-wider">
                CTR
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/20">
            {posts.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-6 py-16 text-center">
                  <div className="flex flex-col items-center gap-3">
                    <span className="material-symbols-outlined text-outline text-4xl">article</span>
                    <p className="text-body-sm text-outline">No post data available for this period</p>
                  </div>
                </td>
              </tr>
            ) : (
              posts.map((post, index) => {
                const platformCfg = PLATFORM_CONFIG[post.platform?.toLowerCase()];
                const engagementPercent = (post.engagement / maxEngagement) * 100;

                return (
                  <tr
                    key={post.postId}
                    className="group hover:bg-gradient-to-r hover:from-secondary/5 hover:to-transparent transition-all duration-300 animate-fade-up"
                    style={{ animationDelay: `${0.55 + index * 0.05}s` }}
                  >
                    <td className="px-5 py-4">
                      <span className="text-body-sm font-semibold text-on-surface group-hover:text-secondary transition-colors duration-300 line-clamp-1 max-w-[200px]">
                        {post.contentTitle || "Untitled Post"}
                      </span>
                    </td>
                    <td className="px-5 py-4">
                      <span className="text-body-sm text-on-surface-variant">
                        {post.brandName || "—"}
                      </span>
                    </td>
                    <td className="px-5 py-4">
                      {platformCfg ? (
                        <span
                          className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-label-xs font-semibold"
                          style={{ backgroundColor: `${platformCfg.color}15`, color: platformCfg.color }}
                        >
                          {platformCfg.label}
                        </span>
                      ) : (
                        <span className="text-body-sm text-outline">{post.platform || "—"}</span>
                      )}
                    </td>
                    <td className="px-5 py-4">
                      <span className="text-body-sm text-on-surface-variant whitespace-nowrap">
                        {formatDate(post.publishedAt)}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-right">
                      <span className="text-body-sm font-semibold text-on-surface tabular-nums">
                        {formatNumber(post.impressions)}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-right">
                      <div className="flex flex-col items-end gap-1">
                        <span className="text-body-sm font-semibold text-on-surface tabular-nums">
                          {formatNumber(post.engagement)}
                        </span>
                        <div className="w-16 h-1 bg-surface-container-high rounded-full overflow-hidden">
                          <div
                            className="h-full bg-gradient-to-r from-secondary to-secondary-container rounded-full transition-all duration-1000"
                            style={{ width: `${engagementPercent}%` }}
                          />
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-4 text-right">
                      <span className="text-body-sm font-semibold text-on-surface tabular-nums">
                        {formatNumber(post.clicks)}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-right">
                      <span className="inline-flex items-center px-3 py-1 bg-primary/10 text-primary rounded-xl font-bold text-label-sm">
                        {formatPercent(post.ctr)}
                      </span>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-up {
          animation: fade-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
      `}</style>
    </div>
  );
}
