"use client";

import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { AuthShell } from "@/components/layout/auth-shell";
import { FormField } from "@/components/shared/form-field";
import { Input } from "@/components/ui/input";
import { AuthFooterLink, AuthSubmitButton } from "@/features/auth/components/auth-form-actions";
import { useRegister } from "@/features/auth/hooks/use-auth";
import { signUpSchema } from "@/features/auth/schemas/auth-schemas";

export default function SignUpPage() {
  const router = useRouter();
  const mutation = useRegister();
  const form = useForm<z.infer<typeof signUpSchema>>({
    resolver: zodResolver(signUpSchema),
    defaultValues: {
      fullName: "",
      email: "",
      password: "",
      confirmPassword: ""
    }
  });

  return (
    <AuthShell title="Create your account" description="Backend returns a session immediately after registration.">
      <form
        className="space-y-5"
        onSubmit={form.handleSubmit(async (values) => {
          await mutation.mutateAsync(values);
          router.replace("/profiles");
        })}
      >
        {mutation.isSuccess ? (
          <AlertPanel
            title="Account created"
            description="Your session is active. Please verify your email from the inbox when available."
            tone="success"
          />
        ) : null}
        {mutation.error ? (
          <AlertPanel title="Registration failed" description={(mutation.error as Error).message} tone="error" />
        ) : null}
        <FormField id="fullName" label="Full name" error={form.formState.errors.fullName?.message}>
          <Input id="fullName" {...form.register("fullName")} />
        </FormField>
        <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
          <Input id="email" type="email" {...form.register("email")} />
        </FormField>
        <FormField id="password" label="Password" error={form.formState.errors.password?.message}>
          <Input id="password" type="password" {...form.register("password")} />
        </FormField>
        <FormField id="confirmPassword" label="Confirm password" error={form.formState.errors.confirmPassword?.message}>
          <Input id="confirmPassword" type="password" {...form.register("confirmPassword")} />
        </FormField>
        <AuthSubmitButton pending={mutation.isPending} label="Create account" />
        <AuthFooterLink copy="Already have an account?" href="/login" cta="Login" />
      </form>
    </AuthShell>
  );
}
