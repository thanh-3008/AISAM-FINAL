"use client";

import { usePathname, useRouter } from "next/navigation";
import { logout } from "@/lib/auth";

type NavItem = {
  label: string;
  href: string;
  icon: string;
};

const adminNavItems: NavItem[] = [
  { label: "Dashboard", href: "/admin/dashboard", icon: "space_dashboard" },
  { label: "Users", href: "/admin/users", icon: "group" },
  { label: "Workspaces", href: "/admin/workspaces", icon: "apartment" },
  { label: "Payments", href: "/admin/payments", icon: "payments" },
  { label: "Content", href: "/admin/content", icon: "description" },
  { label: "Analytics", href: "/admin/analytics", icon: "bar_chart" },
  { label: "Audit Logs", href: "/admin/audit-logs", icon: "history" },
  { label: "Settings", href: "/admin/settings", icon: "settings" },
];

export default function AdminSidebar() {
  const pathname = usePathname();
  const router = useRouter();

  const handleLogout = async () => {
    await logout();
    router.push("/login");
  };

  return (
    <aside className="fixed left-0 top-0 h-full w-64 bg-gray-950 text-gray-100 flex flex-col z-50">
      <div className="p-6 border-b border-gray-800">
        <h1 className="text-xl font-bold text-white">AISAM</h1>
        <p className="text-xs text-gray-500 mt-1">Admin Panel</p>
      </div>

      <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
        {adminNavItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + "/");
          return (
            <button
              key={item.href}
              onClick={() => router.push(item.href)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                isActive
                  ? "bg-blue-600 text-white"
                  : "text-gray-400 hover:bg-gray-800 hover:text-gray-200"
              }`}
            >
              <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
              {item.label}
            </button>
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
