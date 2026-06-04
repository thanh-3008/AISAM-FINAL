"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";

const navItems = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: "dashboard",
  },
  {
    label: "AI Studio",
    href: "/ai-studio",
    icon: "auto_awesome",
    isAI: true,
  },
  {
    label: "Campaigns",
    href: "/campaigns",
    icon: "campaign",
  },
  {
    label: "Content Library",
    href: "/content",
    icon: "description",
  },
  {
    label: "Brands",
    href: "/brands",
    icon: "workspaces",
  },
  {
    label: "Analytics",
    href: "/analytics",
    icon: "bar_chart",
  },
  {
    label: "Scheduling",
    href: "/scheduling",
    icon: "schedule",
  },
  {
    label: "Notifications",
    href: "/notifications",
    icon: "notifications",
  },
];

const bottomItems = [
  { label: "Settings", href: "/settings", icon: "settings" },
  { label: "Help", href: "/help", icon: "help" },
];

export default function Sidebar() {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState(false);

  const isActive = (href: string) =>
    pathname === href || pathname.startsWith(href + "/");

  return (
    <aside
      className={`fixed left-0 top-0 h-full bg-surface-container-lowest border-r border-outline-variant/40 flex flex-col z-40 transition-all duration-300 ${
        collapsed ? "w-16" : "w-sidebar-width"
      }`}
    >
      {/* Logo */}
      <div className="flex items-center gap-3 px-4 h-16 border-b border-outline-variant/40 shrink-0">
        <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center shrink-0">
          <span className="material-symbols-outlined text-white text-lg">
            auto_awesome
          </span>
        </div>
        {!collapsed && (
          <span className="text-headline-sm font-bold text-on-surface">
            AISAM
          </span>
        )}
        <button
          onClick={() => setCollapsed(!collapsed)}
          className={`ml-auto text-on-surface-variant hover:text-on-surface transition-colors ${
            collapsed ? "hidden" : ""
          }`}
          title="Collapse sidebar"
        >
          <span className="material-symbols-outlined text-[20px]">
            left_panel_close
          </span>
        </button>
      </div>

      {collapsed && (
        <button
          onClick={() => setCollapsed(false)}
          className="flex items-center justify-center h-12 text-on-surface-variant hover:text-on-surface transition-colors"
          title="Expand sidebar"
        >
          <span className="material-symbols-outlined text-[20px]">
            left_panel_open
          </span>
        </button>
      )}

      {/* Profile / Business Selector */}
      {!collapsed && (
        <div className="mx-3 mt-4 mb-2 px-3 py-2.5 rounded-xl bg-surface-container border border-outline-variant/30 flex items-center gap-2.5 cursor-pointer hover:bg-surface-container-high transition-colors">
          <div className="w-7 h-7 rounded-full bg-primary flex items-center justify-center shrink-0">
            <span className="text-on-primary text-label-md">A</span>
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-label-md text-on-surface truncate">
              My Business
            </p>
            <p className="text-label-sm text-on-surface-variant truncate">
              Free Plan
            </p>
          </div>
          <span className="material-symbols-outlined text-on-surface-variant text-[18px] shrink-0">
            unfold_more
          </span>
        </div>
      )}

      {/* Nav Items */}
      <nav className="flex-1 overflow-y-auto px-3 py-2 space-y-0.5">
        {navItems.map((item) => {
          const active = isActive(item.href);
          return (
            <Link
              key={item.href}
              href={item.href}
              title={collapsed ? item.label : undefined}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-150 group relative ${
                active
                  ? "bg-primary/10 text-primary"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              } ${collapsed ? "justify-center" : ""}`}
            >
              <span
                className={`material-symbols-outlined text-[22px] shrink-0 ${
                  item.isAI && !active ? "text-secondary" : ""
                }`}
                style={active && item.isAI ? { color: "var(--color-secondary)" } : undefined}
              >
                {item.icon}
              </span>
              {!collapsed && (
                <span className="text-body-sm font-medium">{item.label}</span>
              )}
              {!collapsed && item.isAI && (
                <span className="ml-auto px-1.5 py-0.5 bg-secondary/10 text-secondary rounded-full text-label-sm">
                  AI
                </span>
              )}
              {active && (
                <span className="absolute right-2 w-1.5 h-1.5 rounded-full bg-primary" />
              )}
            </Link>
          );
        })}
      </nav>

      {/* Bottom Items */}
      <div className="px-3 pb-4 border-t border-outline-variant/40 pt-3 space-y-0.5">
        {bottomItems.map((item) => (
          <Link
            key={item.href}
            href={item.href}
            title={collapsed ? item.label : undefined}
            className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors ${
              collapsed ? "justify-center" : ""
            }`}
          >
            <span className="material-symbols-outlined text-[22px] shrink-0">
              {item.icon}
            </span>
            {!collapsed && (
              <span className="text-body-sm font-medium">{item.label}</span>
            )}
          </Link>
        ))}

        {/* User Avatar */}
        <div
          className={`flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-surface-container cursor-pointer transition-colors mt-1 ${
            collapsed ? "justify-center" : ""
          }`}
        >
          <div className="w-8 h-8 rounded-full bg-secondary flex items-center justify-center shrink-0">
            <span className="text-on-secondary text-label-md">U</span>
          </div>
          {!collapsed && (
            <>
              <div className="flex-1 min-w-0">
                <p className="text-body-sm font-medium text-on-surface truncate">
                  User Name
                </p>
                <p className="text-label-sm text-on-surface-variant truncate">
                  user@example.com
                </p>
              </div>
              <span className="material-symbols-outlined text-on-surface-variant text-[18px]">
                logout
              </span>
            </>
          )}
        </div>
      </div>
    </aside>
  );
}
