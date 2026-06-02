"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { AuthShell } from "@/components/layout/auth-shell";
import { FormField } from "@/components/shared/form-field";
import { Input } from "@/components/ui/input";
import { AuthFooterLink, AuthSubmitButton } from "@/features/auth/components/auth-form-actions";
import { useForgotPassword } from "@/features/auth/hooks/use-auth";
import { forgotPasswordSchema } from "@/features/auth/schemas/auth-schemas";

export default function ForgotPasswordPage() {
  const mutation = useForgotPassword();
  const form = useForm<z.infer<typeof forgotPasswordSchema>>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" }
  });

  return (
    <AuthShell title="Reset your password" description="We will send a reset link if the email exists.">
      <form className="space-y-5" onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
        {mutation.isSuccess ? (
          <AlertPanel
            title="Request accepted"
            description="If the email exists, a password reset link has been sent."
            tone="success"
          />
        ) : null}
        {mutation.error ? (
          <AlertPanel title="Could not submit" description={(mutation.error as Error).message} tone="error" />
        ) : null}
        <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
          <Input id="email" type="email" {...form.register("email")} />
        </FormField>
        <AuthSubmitButton pending={mutation.isPending} label="Send reset link" />
        <AuthFooterLink copy="Back to sign in?" href="/login" cta="Login" />
      </form>
    </AuthShell>
  );
}
