import Sidebar from "@/components/layout/Sidebar";
import Header from "@/components/layout/Header";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-surface-gray flex">
      <Sidebar />
      {/* Main content area — offset by sidebar width */}
      <div className="flex-1 flex flex-col ml-sidebar-width transition-all duration-300">
        <Header title="Dashboard" />
        <main className="flex-1 overflow-auto p-gutter">{children}</main>
      </div>
    </div>
  );
}
