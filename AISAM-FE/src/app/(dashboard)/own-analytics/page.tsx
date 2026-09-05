"use client";

import Header from "@/components/layout/Header";
import PersonalAnalyticsView from "@/components/analytics/PersonalAnalyticsView";
import Link from "next/link";

export default function OwnAnalyticsPage() {
  return (
    <>
      <Header
        breadcrumbs={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Analysis", href: "/analytics" },
          { label: "Lịch sử cá nhân" },
        ]}
      />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto overflow-x-hidden bg-linear-to-br from-surface-gray via-surface to-surface-gray">
        <div className="max-w-7xl mx-auto space-y-6">
          <div className="p-4 rounded-xl bg-primary/10 border border-primary/20 flex items-center justify-between gap-4">
            <div className="flex items-center gap-2.5 text-body-sm text-primary font-medium">
              <span className="material-symbols-outlined text-[20px]">info</span>
              <span>Lịch sử cá nhân hiện đã được tích hợp trực tiếp vào trang <strong>Analysis</strong>.</span>
            </div>
            <Link
              href="/analytics?tab=personal"
              className="px-3 py-1.5 rounded-lg bg-primary text-on-primary text-label-xs font-bold hover:scale-105 transition-all shrink-0"
            >
              Xem trong Analysis
            </Link>
          </div>

          <PersonalAnalyticsView />
        </div>
      </main>
    </>
  );
}
