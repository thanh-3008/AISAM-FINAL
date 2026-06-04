"use client";

import { useMe } from "@/features/auth/hooks/use-auth";
import { AlertPanel } from "@/components/feedback/alert-panel";

export default function AccountPage() {
  const query = useMe();

  if (query.isLoading) {
    return <div className="rounded-2xl bg-card p-6 shadow-panel">Loading account...</div>;
  }

  if (query.error) {
    return <AlertPanel title="Could not load account" description={(query.error as Error).message} tone="error" />;
  }

  const user = query.data;
  if (!user) {
    return <AlertPanel title="No account data" description="The backend returned no user payload." tone="error" />;
  }

  return (
    <section className="space-y-6">
      <div>
        <h2 className="text-2xl font-semibold">Account overview</h2>
        <p className="text-sm text-muted-foreground">Account data comes from `/api/Auth/me`.</p>
      </div>
      <div className="grid gap-4 md:grid-cols-2">
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">Full name</p>
          <p className="mt-2 font-medium">{user.fullName ?? "Not set"}</p>
        </div>
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">Email</p>
          <p className="mt-2 font-medium">{user.email}</p>
        </div>
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">Role</p>
          <p className="mt-2 font-medium">{user.role}</p>
        </div>
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">User id</p>
          <p className="mt-2 break-all font-medium">{user.id}</p>
        </div>
      </div>
    </section>
  );
}
