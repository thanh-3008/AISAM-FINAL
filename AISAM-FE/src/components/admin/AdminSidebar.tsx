"use client";

import { usePathname, useRouter } from "next/navigation";
import { logout } from "@/lib/auth";

type NavItem = {
  label: string;
  href: string;
  icon: string;
};

type NavGroup = {
  label: string;
  icon: string;
  items: NavItem[];
};

const primaryNavItems: NavItem[] = [
  { label: "Dashboard", href: "/admin/dashboard", icon: "space_dashboard" },
  { label: "Users", href: "/admin/users", icon: "group" },
  { label: "Workspaces", href: "/admin/workspaces", icon: "apartment" },
  { label: "Content Moderation", href: "/admin/content", icon: "fact_check" },
  { label: "AI & Credit", href: "/admin/credit-oversight", icon: "smart_toy" },
  { label: "Analytics", href: "/admin/analytics", icon: "bar_chart" },
];

const navGroups: NavGroup[] = [
  {
    label: "Billing",
    icon: "payments",
    items: [
      { label: "Payments", href: "/admin/payments", icon: "receipt_long" },
      { label: "Subscriptions", href: "/admin/subscriptions", icon: "subscriptions" },
      { label: "Plans", href: "/admin/plans", icon: "sell" },
    ],
  },
  {
    label: "Audit & Monitoring",
    icon: "monitoring",
    items: [
      { label: "Overview", href: "/admin/system-health", icon: "dashboard" },
      { label: "Audit Logs", href: "/admin/audit-logs", icon: "history" },
      { label: "Background Services", href: "/admin/service-health", icon: "monitor_heart" },
    ],
  },
  {
    label: "System Configuration",
    icon: "settings",
    items: [
      { label: "Overview", href: "/admin/settings", icon: "dashboard" },
      { label: "Broadcast", href: "/admin/broadcast", icon: "campaign" },
      ...(process.env.NODE_ENV === "development"
        ? [{ label: "Dev Tools", href: "/admin/tools", icon: "build" }]
        : []),
    ],
  },
];

export default function AdminSidebar() {
  const pathname = usePathname();
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    window.location.href = "/login";
 window.location.href = "/login";
  };

  const isItemActive = (href: string) =>
    pathname === href || pathname.startsWith(href + "/");

  const renderNavItem = (item: NavItem, nested = false) => {
    const isActive = isItemActive(item.href);
    return (
      <button
        key={item.href}
        onClick={() => router.push(item.href)}
        className={`w-full flex items-center gap-3 rounded-lg text-sm transition-colors ${
          nested ? "py-2 pl-9 pr-3" : "px-3 py-2.5"
        } ${
          isActive
            ? "bg-blue-600 text-white"
            : "text-gray-400 hover:bg-gray-800 hover:text-gray-200"
        }`}
      >
        <span className={`material-symbols-outlined ${nested ? "text-[17px]" : "text-[20px]"}`}>
          {item.icon}
        </span>
        <span className="truncate">{item.label}</span>
      </button>
    );
  };

  return (
    <aside className="fixed left-0 top-0 h-full w-64 bg-gray-950 text-gray-100 flex flex-col z-50">
      <div className="p-6 border-b border-gray-800">
        <h1 className="text-xl font-bold text-white">AISAM</h1>
        <p className="text-xs text-gray-500 mt-1">Admin Panel</p>
      </div>

      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {primaryNavItems.slice(0, 3).map((item) => renderNavItem(item))}

        {navGroups.slice(0, 1).map((group) => {
          const isActive = group.items.some((item) => isItemActive(item.href));
          return (
            <details key={group.label} className="group" open={isActive || undefined}>
              <summary className={`list-none cursor-pointer flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                isActive ? "text-white bg-gray-800" : "text-gray-400 hover:bg-gray-800 hover:text-gray-200"
              }`}>
                <span className="material-symbols-outlined text-[20px]">{group.icon}</span>
                <span className="flex-1">{group.label}</span>
                <span className="material-symbols-outlined text-[17px] transition-transform group-open:rotate-180">expand_more</span>
              </summary>
              <div className="mt-1 space-y-1">{group.items.map((item) => renderNavItem(item, true))}</div>
            </details>
          );
        })}

        {primaryNavItems.slice(3).map((item) => renderNavItem(item))}

        {navGroups.slice(1).map((group) => {
          const isActive = group.items.some((item) => isItemActive(item.href));
          return (
            <details key={group.label} className="group" open={isActive || undefined}>
              <summary className={`list-none cursor-pointer flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                isActive ? "text-white bg-gray-800" : "text-gray-400 hover:bg-gray-800 hover:text-gray-200"
              }`}>
                <span className="material-symbols-outlined text-[20px]">{group.icon}</span>
                <span className="flex-1">{group.label}</span>
                <span className="material-symbols-outlined text-[17px] transition-transform group-open:rotate-180">expand_more</span>
              </summary>
              <div className="mt-1 space-y-1">{group.items.map((item) => renderNavItem(item, true))}</div>
            </details>
          );
        })}
      </nav>

      <div className="p-4 border-t border-gray-800">
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-gray-400 hover:bg-gray-800 hover:text-red-400 transition-colors"
        >
          <span className="material-symbols-outlined text-[20px]">logout</span>
          Logout
        </button>
      </div>
    </aside>
  );
}


