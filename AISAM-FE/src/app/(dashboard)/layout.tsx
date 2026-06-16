"use client";

import Sidebar from "@/components/layout/Sidebar";
import { SidebarProvider, useSidebar } from "@/contexts/SidebarContext";
import { SubscriptionProvider } from "@/contexts/SubscriptionContext";

function DashboardInner({ children }: { children: React.ReactNode }) {
  const { open } = useSidebar();

  return (
    <div className="min-h-screen bg-surface-gray flex overflow-x-hidden w-full">
      <Sidebar />
      <div
        className="flex-1 flex flex-col transition-all duration-300 min-w-0 max-w-full"
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
