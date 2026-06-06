"use client";

import Link from "next/link";
import { Suspense, useCallback, useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Header from "@/components/layout/Header";
import { apiFetch } from "@/lib/apiClient";
import { getStoredActiveProfile } from "@/stores/profile-store";

type ConnectionState = "processing" | "success" | "error";

type SocialAccount = {
  id: string;
  provider: string;
  providerUserId: string;
  isActive: boolean;
  expiresAt?: string | null;
};

type AvailableTarget = {
  providerTargetId: string;
  name: string;
  type: string;
  category?: string | null;
  profilePictureUrl?: string | null;
  isActive: boolean;
};

type Brand = {
  id: string;
  name: string;
};

function FacebookGlyph() {
  return (
    <svg className="w-8 h-8 fill-current" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M24 12.073C24 5.446 18.627.073 12 .073S0 5.446 0 12.073c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
    </svg>
  );
}

function SocialCallbackContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [state, setState] = useState<ConnectionState>("processing");
  const [account, setAccount] = useState<SocialAccount | null>(null);
  const [targets, setTargets] = useState<AvailableTarget[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [selectedTargetIds, setSelectedTargetIds] = useState<string[]>([]);
  const [selectedBrandId, setSelectedBrandId] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [linking, setLinking] = useState(false);
  const [linked, setLinked] = useState(false);

  const activeProfile = getStoredActiveProfile();
  const selectedCount = selectedTargetIds.length;

  const activeProfileLabel = useMemo(() => {
    if (!activeProfile) return "No active profile";
    return activeProfile.name || activeProfile.id;
  }, [activeProfile]);

  const loadBrands = useCallback(async () => {
    try {
      const result = await apiFetch(activeProfile ? `/brands?profileId=${activeProfile.id}&pageSize=100` : "/brands?pageSize=100");
      if (result?.success && Array.isArray(result.data?.data)) {
        setBrands(result.data.data.map((brand: Brand) => ({ id: brand.id, name: brand.name })));
      }
    } catch {
      setBrands([]);
    }
  }, [activeProfile]);

  const processCallback = useCallback(async () => {
    const oauthError = searchParams.get("error");
    const oauthErrorDescription = searchParams.get("error_description");
    const code = searchParams.get("code");
    const oauthState = searchParams.get("state");

    if (oauthError) {
      setState("error");
      setErrorMessage(oauthErrorDescription || oauthError);
      return;
    }

    if (!code || !oauthState) {
      setState("error");
      setErrorMessage("Facebook callback is missing code or state.");
      return;
    }

    try {
      const callbackResult = await apiFetch("/social-auth/facebook/callback", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code, state: oauthState }),
      });

      if (!callbackResult?.success || !callbackResult.data?.id) {
        throw new Error(callbackResult?.message || "Facebook account could not be verified.");
      }

      const connectedAccount = callbackResult.data as SocialAccount;
      setAccount(connectedAccount);

      const [targetsResult] = await Promise.all([
        apiFetch(`/social/accounts/${connectedAccount.id}/available-targets`),
        loadBrands(),
      ]);

      if (targetsResult?.success && Array.isArray(targetsResult.data)) {
        setTargets(targetsResult.data);
        setSelectedTargetIds(targetsResult.data.map((target: AvailableTarget) => target.providerTargetId));
      }

      setState("success");
    } catch (error) {
      setState("error");
      setErrorMessage(error instanceof Error ? error.message : "Unable to finalize Facebook connection.");
    }
  }, [loadBrands, searchParams]);

  useEffect(() => {
    let cancelled = false;
    const run = async () => {
      await Promise.resolve();
      if (!cancelled) await processCallback();
    };
    run();
    return () => {
      cancelled = true;
    };
  }, [processCallback]);

  const toggleTarget = (targetId: string) => {
    setSelectedTargetIds((prev) =>
      prev.includes(targetId) ? prev.filter((id) => id !== targetId) : [...prev, targetId]
    );
  };

  const linkSelectedTargets = async () => {
    if (!account || !selectedBrandId || selectedTargetIds.length === 0) return;
    setLinking(true);
    setLinked(false);
    try {
      const result = await apiFetch(`/social/accounts/${account.id}/link-targets`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          provider: "facebook",
          providerTargetIds: selectedTargetIds,
          brandId: selectedBrandId,
        }),
      });

      if (!result?.success) {
        throw new Error(result?.message || "Unable to link selected Facebook pages.");
      }

      setLinked(true);
      const firstTarget = targets.find((target) => selectedTargetIds.includes(target.providerTargetId));
      const accountName = encodeURIComponent(firstTarget?.name || "Facebook Account");
      const handle = encodeURIComponent(firstTarget?.providerTargetId ? `@${firstTarget.providerTargetId}` : "@facebook");
      router.push(`/social-callback/success?status=success&account=${accountName}&handle=${handle}`);
    } catch (error) {
      setState("error");
      setErrorMessage(error instanceof Error ? error.message : "Unable to link selected Facebook pages.");
    } finally {
      setLinking(false);
    }
  };

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Social Accounts", href: "/social" }, { label: "Social Connection" }]} />
      <main className="p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-6xl mx-auto">
          <section className="mb-8">
            <div className="flex items-center gap-2 text-label-md text-outline mb-2">
              <span>Marketing</span>
              <span className="material-symbols-outlined text-[14px]">chevron_right</span>
              <span className="text-primary font-bold">Social Connection</span>
            </div>
            <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
              <div>
                <h1 className="text-headline-lg text-on-surface tracking-tight mb-2">Finalize Facebook Connection</h1>
                <p className="text-body-lg text-on-surface-variant max-w-3xl">
                  AISAM is securely retrieving available social assets for <span className="font-bold text-primary">{activeProfileLabel}</span>.
                </p>
              </div>
              <Link href="/social" className="inline-flex items-center justify-center gap-2 px-5 py-2.5 border border-outline-variant text-on-surface text-label-md font-semibold rounded-xl hover:bg-surface-container-high transition-colors">
                <span className="material-symbols-outlined text-[18px]">arrow_back</span>
                Return to Social Settings
              </Link>
            </div>
          </section>

          <div className="grid grid-cols-12 gap-8">
            <div className="col-span-12 lg:col-span-8 space-y-8">
              {state === "processing" && (
                <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-12 shadow-sm text-center flex flex-col items-center">
                  <div className="relative w-24 h-24 mb-8">
                    <div className="absolute inset-0 rounded-full border-4 border-primary/10" />
                    <div className="absolute inset-0 rounded-full border-4 border-primary border-r-transparent animate-spin" />
                    <div className="absolute inset-4 bg-[#1877F2] rounded-2xl flex items-center justify-center text-white shadow-lg shadow-primary/20">
                      <FacebookGlyph />
                    </div>
                  </div>
                  <h2 className="text-headline-sm text-on-surface mb-2">Verifying Facebook Authorization</h2>
                  <p className="text-body-md text-on-surface-variant max-w-md">
                    Please keep this page open while AISAM validates the OAuth state and retrieves your Facebook assets.
                  </p>
                </div>
              )}

              {state === "success" && (
                <div className="space-y-8">
                  {linked && (
                    <div className="bg-success-green/5 border border-success-green/20 text-success-green rounded-2xl px-5 py-3 flex items-center gap-2">
                      <span className="material-symbols-outlined text-[18px]">check_circle</span>
                      <span className="text-label-md">Selected Facebook pages linked successfully.</span>
                    </div>
                  )}

                  <div className="bg-surface-container-lowest p-6 rounded-2xl border border-outline-variant shadow-sm flex flex-col gap-6">
                    <div className="flex justify-between items-start gap-4">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-12 h-12 bg-[#1877F2] rounded-xl flex items-center justify-center text-white shrink-0">
                          <FacebookGlyph />
                        </div>
                        <div className="min-w-0">
                          <h2 className="text-headline-sm text-on-surface">Connection Successful</h2>
                          <p className="text-[10px] uppercase tracking-widest text-outline font-semibold truncate">
                            Facebook user {account?.providerUserId || "verified"}
                          </p>
                        </div>
                      </div>
                      <span className="bg-success-green/10 text-success-green px-3 py-1 rounded-full text-[11px] font-bold uppercase tracking-wider border border-success-green/20">
                        Verified
                      </span>
                    </div>
                  </div>

                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant shadow-sm overflow-hidden">
                    <div className="px-6 py-5 border-b border-outline-variant">
                      <h2 className="text-headline-sm text-on-surface">Detected Facebook Pages</h2>
                    </div>
                    <div className="divide-y divide-outline-variant/30">
                      {targets.length === 0 ? (
                        <div className="p-8 text-center text-body-sm text-on-surface-variant">
                          No Facebook Pages were returned for this account.
                        </div>
                      ) : targets.map((target) => {
                        const checked = selectedTargetIds.includes(target.providerTargetId);
                        return (
                          <button
                            key={target.providerTargetId}
                            onClick={() => toggleTarget(target.providerTargetId)}
                            className="w-full flex items-center justify-between p-4 hover:bg-primary/5 transition-colors text-left"
                          >
                            <div className="flex items-center gap-4 min-w-0">
                              <div className="w-12 h-12 rounded-lg bg-surface-container-low border border-outline-variant flex items-center justify-center text-primary shrink-0">
                                <span className="material-symbols-outlined">flag</span>
                              </div>
                              <div className="min-w-0">
                                <p className="text-body-md font-bold text-on-surface truncate">{target.name || target.providerTargetId}</p>
                                <p className="text-label-sm text-on-surface-variant truncate">{target.category || "Facebook Page"} • {target.providerTargetId}</p>
                              </div>
                            </div>
                            <span className={`w-5 h-5 rounded border flex items-center justify-center shrink-0 ${checked ? "bg-primary border-primary text-on-primary" : "border-outline-variant bg-surface-container-lowest"}`}>
                              {checked && <span className="material-symbols-outlined text-[14px]">check</span>}
                            </span>
                          </button>
                        );
                      })}
                    </div>
                    <div className="px-6 py-5 border-t border-outline-variant bg-surface-container-low/30">
                      <h2 className="text-headline-sm text-on-surface">Ad Accounts</h2>
                    </div>
                    <div className="p-5 flex items-center justify-between opacity-60">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-full bg-primary-container/10 flex items-center justify-center">
                          <span className="material-symbols-outlined text-primary">payments</span>
                        </div>
                        <div>
                          <p className="text-body-md font-bold text-on-surface">Ad Account linking</p>
                          <p className="text-label-sm text-on-surface-variant">Backend currently stores Facebook Page targets for publishing.</p>
                        </div>
                      </div>
                      <span className="bg-surface-container-highest px-2 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider text-on-surface-variant">Coming Soon</span>
                    </div>
                    <div className="p-6 border-t border-outline-variant flex flex-col gap-4 bg-surface-container-low/20 lg:flex-row lg:items-center lg:justify-between">
                      <label className="flex-1">
                        <span className="text-label-md text-outline uppercase">Attach selected pages to brand</span>
                        <select
                          value={selectedBrandId}
                          onChange={(event) => setSelectedBrandId(event.target.value)}
                          className="mt-2 w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-4 py-3 text-body-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/10"
                        >
                          <option value="">Select brand</option>
                          {brands.map((brand) => (
                            <option key={brand.id} value={brand.id}>{brand.name}</option>
                          ))}
                        </select>
                      </label>
                      <div className="flex gap-3 lg:self-end">
                        <Link href="/social" className="px-5 py-3 rounded-xl font-bold text-on-surface-variant hover:bg-surface-container transition-colors">
                          Return
                        </Link>
                        <button
                          onClick={linkSelectedTargets}
                          disabled={linking || !account || !selectedBrandId || selectedCount === 0}
                          className="px-6 py-3 rounded-xl bg-primary text-on-primary font-bold shadow-lg hover:opacity-90 active:scale-[0.98] transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                        >
                          {linking ? "Linking..." : `Link Selected${selectedCount ? ` (${selectedCount})` : ""}`}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {state === "error" && (
                <div className="bg-surface-container-lowest border border-danger-red/30 rounded-2xl p-12 shadow-sm text-center flex flex-col items-center">
                  <div className="w-20 h-20 rounded-full bg-error-container/20 flex items-center justify-center text-danger-red mb-6 border border-danger-red/10">
                    <span className="material-symbols-outlined text-[48px]">error</span>
                  </div>
                  <h2 className="text-headline-sm text-on-surface mb-2">Connection Permissions Denied</h2>
                  <p className="text-body-md text-on-surface-variant max-w-sm mb-8">
                    {errorMessage || "Facebook returned a permission error. AISAM needs Page access to proceed."}
                  </p>
                  <div className="flex flex-col sm:flex-row items-center gap-4">
                    <Link href="/social" className="px-8 py-3 rounded-xl bg-danger-red text-on-primary font-bold shadow-lg hover:opacity-90 transition-all flex items-center gap-2">
                      <span className="material-symbols-outlined text-[20px]">refresh</span>
                      Retry Connection
                    </Link>
                    <Link href="/social" className="px-6 py-3 rounded-xl font-bold text-on-surface-variant hover:bg-surface-container transition-colors">
                      Return to Social Settings
                    </Link>
                  </div>
                </div>
              )}
            </div>

            <div className="col-span-12 lg:col-span-4 space-y-8">
              <div className="bg-surface-container-lowest border border-outline-variant rounded-2xl p-6 shadow-sm ai-glow">
                <div className="flex items-center gap-2 mb-6">
                  <span className="material-symbols-outlined text-secondary" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
                  <h2 className="text-label-md font-bold text-secondary uppercase tracking-widest">Next Steps Guide</h2>
                </div>
                <div className="space-y-8">
                  {[
                    ["1", "Verify Assets", "Select the Facebook Pages that AISAM should manage.", state === "success"],
                    ["2", "Brand Binding", "Attach selected Pages to the brand that owns the content.", linked],
                    ["3", "Launch Studio", "Use scheduled publishing once social targets are active.", false],
                  ].map(([step, title, desc, active]) => (
                    <div key={step as string} className={`flex gap-4 ${active ? "" : "opacity-60"}`}>
                      <div className={`shrink-0 w-8 h-8 rounded-full flex items-center justify-center font-bold text-label-md ${active ? "bg-primary-container text-on-primary-container" : "bg-surface-container-high text-outline"}`}>
                        {step}
                      </div>
                      <div>
                        <p className="text-body-md font-bold text-on-surface mb-1">{title}</p>
                        <p className="text-body-sm text-on-surface-variant leading-relaxed">{desc}</p>
                      </div>
                    </div>
                  ))}
                </div>
                <div className="mt-8 bg-surface-container-low/50 rounded-xl p-5 border border-outline-variant/30">
                  <p className="text-label-sm font-bold text-on-surface mb-2 flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-primary">security</span>
                    Security Protocol
                  </p>
                  <p className="text-[11px] leading-relaxed text-on-surface-variant uppercase tracking-tight">
                    Social OAuth tokens are encrypted before storage. AISAM only requests platform access needed for publishing workflows.
                  </p>
                </div>
              </div>

              <div className="bg-surface-container-low text-on-surface p-6 rounded-2xl border border-outline-variant">
                <p className="text-label-sm font-bold text-outline uppercase tracking-widest mb-6">Verification Context</p>
                <div className="space-y-4">
                  <div className="flex items-center justify-between gap-4">
                    <span className="text-body-sm text-on-surface-variant">Active Profile</span>
                    <span className="font-mono text-body-sm bg-surface-container-lowest px-3 py-1 rounded border border-outline-variant truncate max-w-[180px]">{activeProfile?.id || "missing"}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-body-sm text-on-surface-variant">Endpoint Status</span>
                    <span className={`text-label-sm font-bold flex items-center gap-1 ${state === "success" ? "text-success-green" : state === "error" ? "text-danger-red" : "text-warning-amber"}`}>
                      <span className="material-symbols-outlined text-[14px]">{state === "success" ? "verified_user" : state === "error" ? "error" : "sync"}</span>
                      {state === "success" ? "VERIFIED" : state === "error" ? "FAILED" : "NEED_VERIFY"}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  );
}

export default function FacebookSocialCallbackPage() {
  return (
    <Suspense fallback={null}>
      <SocialCallbackContent />
    </Suspense>
  );
}
