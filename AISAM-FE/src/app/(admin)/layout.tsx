"use client";

import AdminSidebar from "@/components/admin/AdminSidebar";
import { useAdminGuard } from "@/hooks/useAdminGuard";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  useAdminGuard();

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <AdminSidebar />
      <div className="flex-1 flex flex-col ml-64">
        {children}
      </div>
    </div>
  );
}
