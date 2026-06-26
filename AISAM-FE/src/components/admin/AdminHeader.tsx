"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useEffect } from "react";
import { getUserFromToken, logout } from "@/lib/auth";

export default function AdminHeader() {
  const router = useRouter();
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    setUser(getUserFromToken());
  }, []);

  const handleLogout = async () => {
    await logout();
    router.replace("/login");
  };

  const initial = user?.name?.charAt(0)?.toUpperCase() || user?.email?.charAt(0)?.toUpperCase() || "A";

  return (
    <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 sticky top-0 z-30">
      <div />
      <div className="relative">
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className="flex items-center gap-3 px-3 py-2 rounded-xl hover:bg-gray-100 transition-colors"
        >
          <div className="w-8 h-8 rounded-full bg-[#731be5]/20 flex items-center justify-center">
            <span className="text-xs font-bold text-[#731be5]">{initial}</span>
          </div>
          <div className="text-left hidden sm:block">
            <p className="text-sm font-semibold text-[#191b24]">{user?.name || "Admin"}</p>
            <p className="text-[11px] text-[#424656]">{user?.email}</p>
          </div>
          <span className="material-symbols-outlined text-[18px] text-[#424656]">expand_more</span>
        </button>

        {menuOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
            <div className="absolute right-0 top-full mt-2 w-56 bg-white border border-gray-200 rounded-xl shadow-lg z-20 py-2">
              <Link
                href="/dashboard"
                className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-[#191b24] hover:bg-gray-100 transition-colors"
                onClick={() => setMenuOpen(false)}
              >
                <span className="material-symbols-outlined text-[18px]">open_in_new</span>
                User App
              </Link>
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-[#DA1E28] hover:bg-gray-100 transition-colors"
              >
                <span className="material-symbols-outlined text-[18px]">logout</span>
                Logout
              </button>
            </div>
          </>
        )}
      </div>
    </header>
  );
}
