"use client";

import Link from "next/link";
import { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { useAuthStore } from "@/stores/auth-store";
import { useProfileStore } from "@/stores/profile-store";

const navItems = [
  { href: "/dashboard", label: "Dashboard" },
  { href: "/profiles", label: "Profiles" },
  { href: "/account", label: "Account" },
  { href: "/security", label: "Security" }
];

export function ProtectedShell({ children }: { children: ReactNode }) {
  const user = useAuthStore((state) => state.user);
  const clearSession = useAuthStore((state) => state.clearSession);
  const activeProfile = useProfileStore((state) => state.activeProfile);
  const clearActiveProfile = useProfileStore((state) => state.clearActiveProfile);

  return (
    <div className="min-h-screen">
      <header className="border-b bg-card/80 backdrop-blur">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-xs uppercase tracking-[0.25em] text-muted-foreground">AISAM FE</p>
            <h1 className="text-lg font-semibold">{activeProfile?.name ?? "Select a profile"}</h1>
          </div>
          <div className="flex items-center gap-3 text-sm">
            <span className="hidden text-muted-foreground md:inline">{user?.email}</span>
            <Button
              variant="outline"
              onClick={() => {
                clearSession();
                clearActiveProfile();
                window.location.assign("/login");
              }}
            >
              Log out
            </Button>
          </div>
        </div>
      </header>
      <div className="mx-auto grid max-w-7xl gap-8 px-6 py-8 lg:grid-cols-[220px_1fr]">
        <aside className="space-y-2">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="block rounded-xl px-4 py-3 text-sm font-medium hover:bg-card"
            >
              {item.label}
            </Link>
          ))}
        </aside>
        <main>{children}</main>
      </div>
    </div>
  );
}
