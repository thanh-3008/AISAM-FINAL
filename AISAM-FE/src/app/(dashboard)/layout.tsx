"use client";

import Sidebar from "@/components/layout/Sidebar";
import { SidebarProvider, useSidebar } from "@/contexts/SidebarContext";
import { SubscriptionProvider } from "@/contexts/SubscriptionContext";

import { useEffect, useState } from "react";
import { setToken, removeToken, getStoredUser } from "@/lib/auth";

function DashboardInner({ children }: { children: React.ReactNode }) {
  const { open } = useSidebar();
  const [adminToken, setAdminToken] = useState<string | null>(null);
  const [impersonatedUser, setImpersonatedUser] = useState<{ email: string } | null>(null);

  useEffect(() => {
    if (typeof window !== "undefined") {
      const token = localStorage.getItem("aisam_admin_token");
      if (token) {
        setAdminToken(token);
        setImpersonatedUser(getStoredUser());
      }
    }
  }, []);

  const handleStopImpersonating = () => {
    if (adminToken) {
      setToken(adminToken);
      localStorage.removeItem("aisam_admin_token");
      window.location.href = "/admin/users";
    }
  };

  return (
    <div className="min-h-screen bg-surface-gray flex overflow-x-hidden w-full relative">
      {adminToken && (
        <div className="absolute top-0 left-0 right-0 bg-amber-500 text-white px-4 py-2 flex items-center justify-between z-100 text-sm font-medium shadow-md">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-[18px]">admin_panel_settings</span>
            <span>You are currently impersonating <strong>{impersonatedUser?.email || "a user"}</strong>. Any actions you take will be recorded under this user.</span>
          </div>
          <button 
            onClick={handleStopImpersonating}
            className="bg-white text-amber-600 px-3 py-1 rounded shadow-sm hover:bg-gray-50 transition-colors"
          >
            Stop Impersonating
          </button>
        </div>
      )}
      <Sidebar />
      <div
        className={`flex-1 flex flex-col transition-all duration-300 min-w-0 max-w-full ${adminToken ? 'mt-10' : ''}`}
        style={{ marginLeft: open ? "var(--spacing-sidebar-width)" : "0" }}
      >
        {children}
      </div>
    </div>
  );
}

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <SidebarProvider>
      <SubscriptionProvider>
        <DashboardInner>{children}</DashboardInner>
      </SubscriptionProvider>
    </SidebarProvider>
  );
}
