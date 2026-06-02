"use client";

import { useSearchParams } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { AuthShell } from "@/components/layout/auth-shell";
import { FormField } from "@/components/shared/form-field";
import { Input } from "@/components/ui/input";
import { AuthSubmitButton } from "@/features/auth/components/auth-form-actions";
import { useResetPassword } from "@/features/auth/hooks/use-auth";
import { resetPasswordSchema } from "@/features/auth/schemas/auth-schemas";

export default function ResetPasswordPage() {
  const searchParams = useSearchParams();
  const mutation = useResetPassword();
  const form = useForm<z.infer<typeof resetPasswordSchema>>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: searchParams.get("email") ?? "",
      token: searchParams.get("token") ?? "",
      newPassword: "",
      confirmPassword: ""
    }
  });

  return (
    <AuthShell title="Choose a new password" description="Use the token from your reset email.">
      <form className="space-y-5" onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        {mutation.isSuccess ? (
          <AlertPanel
            title="Password reset"
            description="Your password has been updated. You can now log in."
            tone="success"
          />
        ) : null}
        {mutation.error ? (
          <AlertPanel title="Reset failed" description={(mutation.error as Error).message} tone="error" />
        ) : null}
        <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
          <Input id="email" type="email" {...form.register("email")} />
        </FormField>
        <FormField id="token" label="Token" error={form.formState.errors.token?.message}>
          <Input id="token" {...form.register("token")} />
        </FormField>
        <FormField id="newPassword" label="New password" error={form.formState.errors.newPassword?.message}>
          <Input id="newPassword" type="password" {...form.register("newPassword")} />
        </FormField>
        <FormField id="confirmPassword" label="Confirm new password" error={form.formState.errors.confirmPassword?.message}>
          <Input id="confirmPassword" type="password" {...form.register("confirmPassword")} />
        </FormField>
        <AuthSubmitButton pending={mutation.isPending} label="Reset password" />
      </form>
    </AuthShell>
  );
}
