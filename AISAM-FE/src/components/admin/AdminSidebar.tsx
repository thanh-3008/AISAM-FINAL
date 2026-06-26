"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const navItems = [
  { label: "Dashboard", href: "/admin/dashboard", icon: "space_dashboard" },
  { label: "Users", href: "/admin/users", icon: "group" },
  { label: "Workspaces", href: "/admin/workspaces", icon: "workspaces" },
  { label: "Subscriptions", href: "/admin/subscriptions", icon: "subscriptions" },
  { label: "Payments", href: "/admin/payments", icon: "payments" },
  { label: "Plans", href: "/admin/plans", icon: "auto_awesome" },
  { label: "Audit Logs", href: "/admin/audit-logs", icon: "history" },
  { label: "Tools", href: "/admin/tools", icon: "build" },
  { label: "Configuration", href: "/admin/config", icon: "settings" },
];

export default function AdminSidebar() {
  const pathname = usePathname();

  return (
    <aside className="fixed left-0 top-0 h-screen w-[260px] bg-white border-r border-gray-200 flex flex-col z-40">
      <div className="p-6 border-b border-gray-200">
        <Link href="/admin/dashboard" className="flex items-center gap-2">
          <span className="material-symbols-outlined text-[#731be5] text-2xl">admin_panel_settings</span>
          <span className="text-xl font-bold text-[#191b24]">AISAM Admin</span>
        </Link>
      </div>

      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {navItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-semibold transition-all duration-200 ${
                isActive
                  ? "bg-[#004ccd]/10 text-[#004ccd]"
                  : "text-[#424656] hover:bg-gray-100 hover:text-[#191b24]"
              }`}
            >
              <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
              {item.label}
              {isActive && (
                <span className="ml-auto w-1.5 h-1.5 rounded-full bg-[#004ccd]" />
              )}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t border-gray-200">
        <Link
          href="/dashboard"
          className="flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm text-[#424656] hover:bg-gray-100 hover:text-[#191b24] transition-colors"
        >
          <span className="material-symbols-outlined text-[20px]">open_in_new</span>
          User App
        </Link>
      </div>
    </aside>
  );
}
