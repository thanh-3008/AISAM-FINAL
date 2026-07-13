"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { handleFacebookCallback } from "@/services/socialAccountService";

export default function FacebookCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState("Processing...");

  useEffect(() => {
    const code = searchParams.get("code");
    const state = searchParams.get("state");
    if (!code || !state) {
      setStatus("Invalid callback parameters");
      return;
    }

    const process = async () => {
      try {
        setStatus("Linking Facebook account...");
        const account = await handleFacebookCallback(code, state);

        sessionStorage.removeItem("facebook_connect_brand_id");
        setStatus("Choose the Page you want to link to this brand...");
        router.push(`/social?manageAccount=${encodeURIComponent(account.id)}`);
      } catch {
        setStatus("Failed to connect Facebook account. Redirecting...");
        setTimeout(() => router.push("/social"), 2000);
      }
    };

    process();
  }, [router, searchParams]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-surface-container-low">
      <div className="flex items-center gap-3 px-6 py-4 bg-surface-container-lowest rounded-2xl shadow-lg">
        <span className="w-5 h-5 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
        <p className="text-body-sm text-on-surface font-medium">{status}</p>
      </div>
    </div>
  );
}
