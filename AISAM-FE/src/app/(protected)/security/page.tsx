"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { FormField } from "@/components/shared/form-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useChangePassword, useLogoutAll, useSessions } from "@/features/auth/hooks/use-auth";
import { changePasswordSchema } from "@/features/auth/schemas/auth-schemas";

export default function SecurityPage() {
  const sessions = useSessions();
  const changePassword = useChangePassword();
  const logoutAll = useLogoutAll();
  const form = useForm<z.infer<typeof changePasswordSchema>>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: ""
    }
  });

  return (
    <section className="space-y-8">
      <div>
        <h2 className="text-2xl font-semibold">Security</h2>
        <p className="text-sm text-muted-foreground">Manage sessions and password from the active backend contract.</p>
      </div>
      <div className="grid gap-8 lg:grid-cols-[1.1fr_0.9fr]">
        <form
          className="space-y-5 rounded-2xl bg-card p-6 shadow-panel"
          onSubmit={form.handleSubmit((values) => changePassword.mutate(values))}
        >
          <h3 className="text-lg font-semibold">Change password</h3>
          {changePassword.isSuccess ? (
            <AlertPanel title="Password updated" description="Please log in again after changing your password." tone="success" />
          ) : null}
          {changePassword.error ? (
            <AlertPanel title="Update failed" description={(changePassword.error as Error).message} tone="error" />
          ) : null}
          <FormField id="currentPassword" label="Current password" error={form.formState.errors.currentPassword?.message}>
            <Input id="currentPassword" type="password" {...form.register("currentPassword")} />
          </FormField>
          <FormField id="newPassword" label="New password" error={form.formState.errors.newPassword?.message}>
            <Input id="newPassword" type="password" {...form.register("newPassword")} />
          </FormField>
          <FormField id="confirmPassword" label="Confirm password" error={form.formState.errors.confirmPassword?.message}>
            <Input id="confirmPassword" type="password" {...form.register("confirmPassword")} />
          </FormField>
          <Button type="submit" disabled={changePassword.isPending}>
            {changePassword.isPending ? "Updating..." : "Change password"}
          </Button>
        </form>
        <div className="space-y-4 rounded-2xl bg-card p-6 shadow-panel">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Active sessions</h3>
            <Button variant="outline" onClick={() => logoutAll.mutate()} disabled={logoutAll.isPending}>
              Log out all
            </Button>
          </div>
          {sessions.isLoading ? <p className="text-sm text-muted-foreground">Loading sessions...</p> : null}
          {sessions.error ? (
            <AlertPanel title="Could not load sessions" description={(sessions.error as Error).message} tone="error" />
          ) : null}
          <div className="space-y-3">
            {sessions.data?.map((session) => (
              <div key={session.id} className="rounded-xl border p-4">
                <p className="text-sm font-medium">{session.userAgent ?? "Unknown device"}</p>
                <p className="text-xs text-muted-foreground">{session.ipAddress ?? "Unknown IP"}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
