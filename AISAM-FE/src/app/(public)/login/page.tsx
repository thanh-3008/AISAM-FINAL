"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { AuthShell } from "@/components/layout/auth-shell";
import { FormField } from "@/components/shared/form-field";
import { Input } from "@/components/ui/input";
import { AuthFooterLink, AuthSubmitButton } from "@/features/auth/components/auth-form-actions";
import { useLogin } from "@/features/auth/hooks/use-auth";
import { loginSchema } from "@/features/auth/schemas/auth-schemas";

export default function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const next = searchParams.get("next") ?? "/dashboard";
  const mutation = useLogin();
  const form = useForm<z.infer<typeof loginSchema>>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: ""
    }
  });

  return (
    <AuthShell title="Welcome back" description="Sign in with your AISAM credentials.">
      <form
        className="space-y-5"
        onSubmit={form.handleSubmit(async (values) => {
          await mutation.mutateAsync(values);
          router.replace(next);
        })}
      >
        {mutation.error ? (
          <AlertPanel title="Login failed" description={(mutation.error as Error).message} tone="error" />
        ) : null}
        <FormField id="email" label="Email" error={form.formState.errors.email?.message}>
          <Input id="email" type="email" {...form.register("email")} />
        </FormField>
        <FormField id="password" label="Password" error={form.formState.errors.password?.message}>
          <Input id="password" type="password" {...form.register("password")} />
        </FormField>
        <div className="flex items-center justify-between text-sm">
          <Link className="text-primary" href="/forgot-password">
            Forgot password?
          </Link>
          <button
            className="text-primary"
            type="button"
            onClick={() => {
              window.alert("Google login UI placeholder. Wire Google Identity before production use.");
            }}
          >
            Continue with Google
          </button>
        </div>
        <AuthSubmitButton pending={mutation.isPending} label="Login" />
        <AuthFooterLink copy="Need an account?" href="/sign-up" cta="Create one" />
      </form>
    </AuthShell>
  );
}
