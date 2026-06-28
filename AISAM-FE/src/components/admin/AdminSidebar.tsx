"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSidebar } from "@/contexts/SidebarContext";

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
  const { open, toggle } = useSidebar();

  return (
    <aside
      className="fixed left-0 top-0 h-screen z-50 flex flex-col bg-surface-container-lowest/90 backdrop-blur-xl border-r border-outline-variant/30 transition-all duration-300 overflow-hidden"
      style={{ width: open ? "var(--spacing-sidebar-width)" : "72px" }}
    >
      <div className="p-4 flex items-center gap-3 border-b border-outline-variant/10">
        <Link href="/admin/dashboard" className="flex items-center gap-3 shrink-0">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center">
            <span className="material-symbols-outlined text-white text-xl">admin_panel_settings</span>
          </div>
          {open && <span className="text-headline-sm font-bold text-on-surface whitespace-nowrap">AISAM Admin</span>}
        </Link>
      </div>

      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {navItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <Link
              key={item.href}
              href={item.href}
              title={open ? undefined : item.label}
              className={`relative flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm font-semibold transition-all duration-200 ${
                isActive
                  ? "bg-gradient-to-r from-primary/10 to-transparent text-primary"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className="material-symbols-outlined text-[20px] shrink-0">{item.icon}</span>
              {open && <span className="whitespace-nowrap">{item.label}</span>}
              {isActive && (
                <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 rounded-r-full bg-primary" />
              )}
            </Link>
          );
        })}
      </nav>

      {open && (
        <div className="p-3 border-t border-outline-variant/10">
          <Link
            href="/dashboard"
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-colors"
          >
            <span className="material-symbols-outlined text-[20px]">open_in_new</span>
            <span>User App</span>
          </Link>
        </div>
      )}

      <button
        onClick={toggle}
        className="absolute bottom-20 right-0 translate-x-1/2 w-6 h-6 rounded-full bg-surface-container-lowest border border-outline-variant/30 shadow-sm flex items-center justify-center hover:bg-surface-container transition-colors"
      >
        <span className="material-symbols-outlined text-[14px] text-on-surface-variant">
          {open ? "chevron_left" : "chevron_right"}
        </span>
      </button>
    </aside>
  );
}
