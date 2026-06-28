"use client";

import AdminGuard from "@/components/admin/AdminGuard";
import AdminSidebar from "@/components/admin/AdminSidebar";
import Header from "@/components/layout/Header";
import { SidebarProvider, useSidebar } from "@/contexts/SidebarContext";

function AdminLayoutInner({ children }: { children: React.ReactNode }) {
  const { open } = useSidebar();

  return (
    <div className="min-h-screen bg-surface-gray flex">
      <AdminSidebar />
      <div
        className="flex-1 flex flex-col min-w-0 max-w-full transition-all duration-300"
        style={{ marginLeft: open ? "var(--spacing-sidebar-width)" : "72px" }}
      >
        <Header />
        <main className="flex-1 p-6">{children}</main>
      </div>
    </div>
  );
}

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AdminGuard>
      <SidebarProvider>
        <AdminLayoutInner>{children}</AdminLayoutInner>
      </SidebarProvider>
    </AdminGuard>
  );
}
