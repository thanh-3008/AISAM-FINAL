"use client";

import { useState } from "react";
import Link from "next/link";

interface HeaderProps {
  title?: string;
  breadcrumbs?: { label: string; href?: string }[];
}

export default function Header({ title, breadcrumbs }: HeaderProps) {
  const [notifOpen, setNotifOpen] = useState(false);

  return (
    <header className="h-16 border-b border-outline-variant/40 bg-surface-container-lowest/80 backdrop-blur-sm flex items-center px-gutter gap-4 sticky top-0 z-30">
      {/* Breadcrumbs / Title */}
      <div className="flex-1 min-w-0">
        {breadcrumbs && breadcrumbs.length > 0 ? (
          <nav className="flex items-center gap-1.5">
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="flex items-center gap-1.5">
                {i > 0 && (
                  <span className="material-symbols-outlined text-on-surface-variant text-[14px]">
                    chevron_right
                  </span>
                )}
                {crumb.href && i < breadcrumbs.length - 1 ? (
                  <Link
                    href={crumb.href}
                    className="text-body-sm text-on-surface-variant hover:text-on-surface transition-colors"
                  >
                    {crumb.label}
                  </Link>
                ) : (
                  <span className="text-body-sm font-medium text-on-surface">
                    {crumb.label}
                  </span>
                )}
              </span>
            ))}
          </nav>
        ) : (
          <h1 className="text-headline-sm text-on-surface truncate">{title}</h1>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2">
        {/* Search */}
        <button className="w-9 h-9 flex items-center justify-center rounded-xl text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors">
          <span className="material-symbols-outlined text-[22px]">search</span>
        </button>

        {/* Notifications */}
        <div className="relative">
          <button
            id="notif-btn"
            onClick={() => setNotifOpen(!notifOpen)}
            className="w-9 h-9 flex items-center justify-center rounded-xl text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors relative"
          >
            <span className="material-symbols-outlined text-[22px]">
              notifications
            </span>
            {/* Unread badge */}
            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-danger-red rounded-full border-2 border-surface-container-lowest" />
          </button>

          {/* Dropdown */}
          {notifOpen && (
            <div className="absolute right-0 top-11 w-80 bg-surface-container-lowest border border-outline-variant/40 rounded-2xl shadow-xl z-50">
              <div className="flex items-center justify-between px-4 py-3 border-b border-outline-variant/30">
                <span className="text-headline-sm">Thông báo</span>
                <button className="text-label-md text-primary hover:text-primary-container transition-colors">
                  Đọc tất cả
                </button>
              </div>
              <div className="divide-y divide-outline-variant/20 max-h-72 overflow-y-auto">
                {[
                  { icon: "check_circle", color: "text-success-green", bg: "bg-success-green/10", title: "Bài đăng đã được publish", time: "2 phút trước", unread: true },
                  { icon: "auto_awesome", color: "text-secondary", bg: "bg-secondary/10", title: "AI đã tạo xong nội dung", time: "15 phút trước", unread: true },
                  { icon: "schedule", color: "text-primary", bg: "bg-primary/10", title: "Lịch đăng sắp đến hạn", time: "1 giờ trước", unread: false },
                ].map((n, i) => (
                  <div
                    key={i}
                    className={`flex items-start gap-3 px-4 py-3 hover:bg-surface-container transition-colors cursor-pointer ${n.unread ? "bg-primary/5" : ""}`}
                  >
                    <div className={`w-9 h-9 rounded-full ${n.bg} flex items-center justify-center shrink-0 mt-0.5`}>
                      <span className={`material-symbols-outlined ${n.color} text-[18px]`}>
                        {n.icon}
                      </span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-body-sm text-on-surface font-medium">{n.title}</p>
                      <p className="text-label-sm text-on-surface-variant">{n.time}</p>
                    </div>
                    {n.unread && (
                      <span className="w-2 h-2 bg-primary rounded-full shrink-0 mt-2" />
                    )}
                  </div>
                ))}
              </div>
              <div className="px-4 py-3 border-t border-outline-variant/30">
                <Link href="/notifications" className="text-label-md text-primary hover:text-primary-container transition-colors flex items-center gap-1 justify-center">
                  Xem tất cả thông báo
                  <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
                </Link>
              </div>
            </div>
          )}
        </div>

        {/* Divider */}
        <div className="h-6 w-px bg-outline-variant/40" />

        {/* User Avatar */}
        <button className="flex items-center gap-2 px-2 py-1.5 rounded-xl hover:bg-surface-container transition-colors">
          <div className="w-7 h-7 rounded-full bg-secondary flex items-center justify-center">
            <span className="text-on-secondary text-label-md">U</span>
          </div>
          <span className="text-body-sm font-medium text-on-surface hidden sm:block">
            User
          </span>
          <span className="material-symbols-outlined text-on-surface-variant text-[18px]">
            expand_more
          </span>
        </button>
      </div>
    </header>
  );
}
