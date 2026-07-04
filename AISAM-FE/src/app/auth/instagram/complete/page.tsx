"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { getAvailableTargets, handleInstagramCallback, linkTargets } from "@/services/socialAccountService";

export default function InstagramCompletePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState("Processing Instagram authorization...");

  useEffect(() => {
    const code = searchParams.get("code");
    const state = searchParams.get("state");
    const oauthError = searchParams.get("error_description") || searchParams.get("error");
    const brandId = sessionStorage.getItem("social_connect_brand_id");

    if (oauthError || !code || !state) {
      setStatus(oauthError || "Instagram returned invalid callback parameters.");
      return;
    }

    const connect = async () => {
      try {
        setStatus("Linking Instagram account...");
        const account = await handleInstagramCallback(code, state);
        if (brandId) {
          setStatus("Linking Instagram Professional account to the selected brand...");
          const targets = await getAvailableTargets(account.id);
          if (!targets.length) {
            throw new Error("No Instagram Professional account linked to an accessible Facebook Page was found.");
          }
          await linkTargets(account.id, targets.map(target => target.providerTargetId), brandId, "instagram");
        }

        sessionStorage.removeItem("social_connect_brand_id");
        router.replace("/social");
      } catch (error) {
        setStatus(error instanceof Error ? error.message : "Failed to connect Instagram account.");
        setTimeout(() => router.replace("/social"), 3000);
      }
    };

    connect();
  }, [router, searchParams]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-surface-container-low">
      <div className="px-6 py-4 bg-surface-container-lowest rounded-2xl shadow-lg">
        <p className="text-body-sm text-on-surface font-medium">{status}</p>
      </div>
    </div>
  );
}
