"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { handleFacebookCallback, getAvailableTargets, linkTargets } from "@/services/socialAccountService";

export default function FacebookCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState("Processing...");

  useEffect(() => {
    let timer: NodeJS.Timeout | null = null;
    const code = searchParams.get("code");
    const state = searchParams.get("state");
    const brandId = sessionStorage.getItem("facebook_connect_brand_id");

    if (!code || !state) {
      setStatus("Invalid callback parameters");
      return;
    }

    const process = async () => {
      try {
        setStatus("Linking Facebook account...");
        const account = await handleFacebookCallback(code, state);

        if (brandId) {
          setStatus("Connecting targets...");
          const targets = await getAvailableTargets(account.id);
          const targetIds = targets.map((t) => t.providerTargetId);
          if (targetIds.length > 0) {
            await linkTargets(account.id, targetIds, brandId);
          }
        }

        sessionStorage.removeItem("facebook_connect_brand_id");
        router.push("/social");
      } catch {
        setStatus("Failed to connect Facebook account. Redirecting...");
        timer = setTimeout(() => router.push("/social"), 2000);
      }
    };

    process();

    return () => { if (timer) clearTimeout(timer); };
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
