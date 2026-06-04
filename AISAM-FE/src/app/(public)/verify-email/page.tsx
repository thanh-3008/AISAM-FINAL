"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { AuthShell } from "@/components/layout/auth-shell";
import { useVerifyEmail } from "@/features/auth/hooks/use-auth";

export default function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");
  const query = useVerifyEmail(token);

  return (
    <AuthShell title="Verify email" description="We are checking your verification token.">
      <div className="space-y-4">
        {!token ? <AlertPanel title="Missing token" description="Verification token is missing." tone="error" /> : null}
        {query.isLoading ? <AlertPanel title="Checking token" description="Please wait..." /> : null}
        {query.isSuccess ? (
          <AlertPanel title="Email verified" description="You can now continue using AISAM." tone="success" />
        ) : null}
        {query.error ? (
          <AlertPanel title="Verification failed" description={(query.error as Error).message} tone="error" />
        ) : null}
        <Link className="text-sm font-medium text-primary" href="/login">
          Go to login
        </Link>
      </div>
    </AuthShell>
  );
}
