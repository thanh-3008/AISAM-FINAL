"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Header from "@/components/layout/Header";
import { apiFetch } from "@/lib/apiClient";
import { useProfiles } from "@/hooks/useProfiles";

type SocialTarget = {
  id: string;
  providerTargetId: string;
  name: string;
  type: string;
  category?: string | null;
  profilePictureUrl?: string | null;
  isActive: boolean;
};

type SocialAccount = {
  id: string;
  profileId: string;
  provider: string;
  providerUserId: string;
  isActive: boolean;
  expiresAt?: string | null;
  createdAt: string;
  updatedAt: string;
  targets: SocialTarget[];
};

type Brand = {
  id: string;
  name: string;
};

type AvailableTarget = {
  providerTargetId: string;
  name: string;
  type: string;
  category?: string | null;
  profilePictureUrl?: string | null;
  isActive: boolean;
};

type Notice = {
  tone: "success" | "error" | "info";
  message: string;
};

type PlatformId = "facebook" | "instagram";

type PlatformOption =
  | {
      id: PlatformId;
      name: string;
      icon: string;
      iconClass: string;
      enabled: true;
    }
  | {
      id: "tiktok" | "linkedin";
      name: string;
      icon: string;
      iconClass: string;
      enabled: false;
    };

const platformOptions: PlatformOption[] = [
  {
    id: "facebook" as const,
    name: "Facebook",
    icon: "social_leaderboard",
    iconClass: "bg-[#1877F2]/10 text-[#1877F2]",
    enabled: true,
  },
  {
    id: "instagram" as const,
    name: "Instagram",
    icon: "photo_camera",
    iconClass: "bg-[#E4405F]/10 text-[#E4405F]",
    enabled: true,
  },
  {
    id: "tiktok",
    name: "TikTok",
    icon: "music_note",
    iconClass: "bg-on-surface/5 text-on-surface-variant",
    enabled: false,
  },
  {
    id: "linkedin",
    name: "LinkedIn",
    icon: "work",
    iconClass: "bg-on-surface/5 text-on-surface-variant",
    enabled: false,
  },
];

const futureIntegrations = [
  {
    name: "Instagram",
    desc: "Direct creative sync and multi-asset optimization for Reels and Stories.",
    badge: "Coming Soon",
    badgeClass: "text-primary bg-primary/10",
    iconClass: "bg-gradient-to-tr from-[#f9ce34] via-[#ee2a7b] to-[#6228d7]",
    icon: "alternate_email",
    dim: "opacity-80",
  },
  {
    name: "TikTok",
    desc: "Trend analysis and automated short-form video placement.",
    badge: "Planned",
    badgeClass: "text-outline bg-outline-variant/20",
    iconClass: "bg-black",
    icon: "music_note",
    dim: "opacity-60 grayscale",
  },
  {
    name: "Twitter (X)",
    desc: "Real-time keyword campaign manager and sentiment targeting.",
    badge: "Planned",
    badgeClass: "text-outline bg-outline-variant/20",
    iconClass: "bg-[#0F1419]",
    icon: "close",
    dim: "opacity-60 grayscale",
  },
];

function formatDate(value?: string | null) {
  if (!value) return "Not available";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Not available";
  return new Intl.DateTimeFormat("en", { month: "short", day: "2-digit", year: "numeric" }).format(date);
}

function getDaysUntil(value?: string | null) {
  if (!value) return null;
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return null;
  return Math.ceil((date.getTime() - Date.now()) / 86_400_000);
}

function getAccountLabel(account?: SocialAccount) {
  if (!account) return "No account connected";
  return account.providerUserId ? `Facebook user ${account.providerUserId}` : "Facebook account";
}

function FacebookGlyph() {
  return (
    <svg className="w-7 h-7 fill-current" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M24 12.073C24 5.446 18.627.073 12 .073S0 5.446 0 12.073c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
    </svg>
  );
}

export default function SocialAccountsPage() {
  const { activeProfile } = useProfiles();
  const [accounts, setAccounts] = useState<SocialAccount[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [availableTargets, setAvailableTargets] = useState<AvailableTarget[]>([]);
  const [selectedAccountId, setSelectedAccountId] = useState("");
  const [selectedTargetIds, setSelectedTargetIds] = useState<string[]>([]);
  const [selectedBrandId, setSelectedBrandId] = useState("");
  const [loading, setLoading] = useState(true);
  const [linking, setLinking] = useState(false);
  const [platformModalOpen, setPlatformModalOpen] = useState(false);
  const [selectedPlatform, setSelectedPlatform] = useState<PlatformId | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [notice, setNotice] = useState<Notice | null>(null);

  const primaryAccount = accounts[0];
  const linkedTargets = useMemo(
    () => accounts.flatMap((account) => account.targets.map((target) => ({ ...target, account }))),
    [accounts]
  );
  const expiringInDays = getDaysUntil(primaryAccount?.expiresAt);

  const loadAccounts = useCallback(async () => {
    try {
      const result = await apiFetch("/social/accounts/me");
      if (result?.success && Array.isArray(result.data)) {
        setAccounts(result.data);
      } else {
        setAccounts([]);
      }
    } catch (error) {
      setAccounts([]);
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to load social accounts." });
    }
  }, []);

  const loadBrands = useCallback(async () => {
    try {
      const result = await apiFetch("/brands?pageSize=100");
      if (result?.success && Array.isArray(result.data?.data)) {
        setBrands(result.data.data.map((brand: Brand) => ({ id: brand.id, name: brand.name })));
      }
    } catch {
      setBrands([]);
    }
  }, []);

  const refreshPage = useCallback(async () => {
    setLoading(true);
    await Promise.all([loadAccounts(), loadBrands()]);
    setLoading(false);
  }, [loadAccounts, loadBrands]);

  useEffect(() => {
    let cancelled = false;

    const loadInitialData = async () => {
      await Promise.resolve();
      if (cancelled) return;
      setLoading(true);
      await Promise.all([loadAccounts(), loadBrands()]);
      if (!cancelled) setLoading(false);
    };

    loadInitialData();
    return () => {
      cancelled = true;
    };
  }, [loadAccounts, loadBrands, activeProfile?.id]);

  const connectFacebook = async () => {
    setNotice(null);
    try {
      const result = await apiFetch("/social-auth/facebook");
      if (result?.success && result.data?.authUrl) {
        window.location.href = result.data.authUrl;
        return;
      }
      setNotice({ tone: "error", message: "Facebook authorization URL was not returned." });
    } catch (error) {
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to start Facebook connection." });
    }
  };

  const loadAvailableTargets = async (accountId: string) => {
    setSelectedAccountId(accountId);
    setSelectedTargetIds([]);
    setAvailableTargets([]);
    if (!accountId) return;

    try {
      const result = await apiFetch(`/social/accounts/${accountId}/available-targets`);
      if (result?.success && Array.isArray(result.data)) {
        setAvailableTargets(result.data);
      }
    } catch (error) {
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to load available Facebook pages." });
    }
  };

  const openLinkTargetModal = async () => {
    setNotice(null);
    setModalOpen(true);
    if (primaryAccount) {
      await loadAvailableTargets(primaryAccount.id);
    }
  };

  const openPlatformModal = () => {
    setNotice(null);
    setSelectedPlatform(null);
    setPlatformModalOpen(true);
  };

  const continueSelectedPlatform = async () => {
    if (!selectedPlatform) return;

    if (selectedPlatform === "facebook") {
      setPlatformModalOpen(false);
      if (primaryAccount) {
        await openLinkTargetModal();
      } else {
        await connectFacebook();
      }
      return;
    }

    setPlatformModalOpen(false);
    setNotice({
      tone: "info",
      message: "Instagram connection UI is ready, but backend currently supports Facebook only.",
    });
  };

  const toggleSelectedTarget = (targetId: string) => {
    setSelectedTargetIds((prev) =>
      prev.includes(targetId) ? prev.filter((id) => id !== targetId) : [...prev, targetId]
    );
  };

  const linkSelectedTargets = async () => {
    if (!selectedAccountId || !selectedBrandId || selectedTargetIds.length === 0) return;
    setLinking(true);
    try {
      const result = await apiFetch(`/social/accounts/${selectedAccountId}/link-targets`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          provider: "facebook",
          providerTargetIds: selectedTargetIds,
          brandId: selectedBrandId,
        }),
      });
      if (result?.success) {
        setNotice({ tone: "success", message: "Selected Facebook page linked successfully." });
        setModalOpen(false);
        await loadAccounts();
      }
    } catch (error) {
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to link selected target." });
    } finally {
      setLinking(false);
    }
  };

  const unlinkTarget = async (integrationId: string) => {
    try {
      await apiFetch(`/social/integrations/${integrationId}`, { method: "DELETE" });
      setNotice({ tone: "success", message: "Social target unlinked successfully." });
      await loadAccounts();
    } catch (error) {
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to unlink target." });
    }
  };

  const disconnectAccount = async (accountId: string) => {
    try {
      await apiFetch(`/social/accounts/${accountId}`, { method: "DELETE" });
      setNotice({ tone: "success", message: "Facebook account disconnected successfully." });
      await loadAccounts();
    } catch (error) {
      setNotice({ tone: "error", message: error instanceof Error ? error.message : "Unable to disconnect account." });
    }
  };

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Social Accounts" }]} />
      <main className="p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-8">
        <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <div className="flex items-center gap-2 text-label-md text-outline mb-2">
              <span>Marketing</span>
              <span className="material-symbols-outlined text-[14px]">chevron_right</span>
              <span className="text-primary font-bold">Social Accounts</span>
            </div>
            <h1 className="text-headline-lg text-on-surface tracking-tight mb-2">Social Accounts & Integrations</h1>
            <p className="text-body-lg text-on-surface-variant">Manage Facebook connections and page targets used for scheduled publishing.</p>
          </div>
          <div className="flex flex-wrap gap-3">
            <button
              onClick={refreshPage}
              className="px-5 py-2.5 border border-outline-variant text-on-surface font-semibold text-label-md rounded-xl hover:bg-surface-container-high transition-colors flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[18px]">refresh</span>
              Refresh
            </button>
            <button
              onClick={openPlatformModal}
              className="px-5 py-2.5 bg-primary text-on-primary font-semibold text-label-md rounded-xl hover:opacity-90 shadow-md shadow-primary/20 transition-all flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[18px]">add</span>
              Link New Target
            </button>
          </div>
        </section>

        {notice && (
          <div className={`px-5 py-3 rounded-2xl border flex items-center justify-between gap-4 ${
            notice.tone === "success"
              ? "bg-success-green/5 border-success-green/20 text-success-green"
              : notice.tone === "error"
                ? "bg-error-container/30 border-error/20 text-error"
                : "bg-primary/5 border-primary/20 text-primary"
          }`}>
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-[18px]">{notice.tone === "success" ? "check_circle" : notice.tone === "error" ? "error" : "info"}</span>
              <span className="text-label-md">{notice.message}</span>
            </div>
            <button onClick={() => setNotice(null)} className="text-current/70 hover:text-current">
              <span className="material-symbols-outlined text-[16px]">close</span>
            </button>
          </div>
        )}

        <div className="grid grid-cols-12 gap-gutter">
          <div className="col-span-12 lg:col-span-4 flex flex-col gap-gutter">
            <div className="bg-surface-container-lowest p-6 rounded-2xl border border-outline-variant/50 shadow-sm flex flex-col gap-6">
              <div className="flex justify-between items-start gap-4">
                <div className="flex items-center gap-3 min-w-0">
                  <div className="w-12 h-12 bg-[#1877F2] rounded-xl flex items-center justify-center text-white shrink-0">
                    <FacebookGlyph />
                  </div>
                  <div className="min-w-0">
                    <h2 className="text-headline-sm text-on-surface">Facebook</h2>
                    <p className="text-[10px] uppercase tracking-widest text-outline font-semibold">Meta Business Suite</p>
                  </div>
                </div>
                <span className={`px-3 py-1 rounded-full text-label-md flex items-center gap-1 shrink-0 ${
                  primaryAccount ? "bg-success-green/10 text-success-green" : "bg-outline-variant/30 text-outline"
                }`}>
                  <span className={`w-2 h-2 rounded-full ${primaryAccount ? "bg-success-green" : "bg-outline"}`} />
                  {primaryAccount ? "Connected" : "Not Connected"}
                </span>
              </div>

              <div className="flex items-center gap-4 bg-surface-container-low p-4 rounded-xl border border-outline-variant/30">
                <div className="w-14 h-14 rounded-full bg-primary-fixed text-primary flex items-center justify-center border-2 border-white shadow-sm shrink-0">
                  <span className="material-symbols-outlined">person</span>
                </div>
                <div className="min-w-0">
                  <p className="text-headline-sm text-on-surface leading-tight truncate">{getAccountLabel(primaryAccount)}</p>
                  <p className="text-body-sm text-on-surface-variant">
                    {primaryAccount ? `Admin access to ${linkedTargets.length} linked target${linkedTargets.length === 1 ? "" : "s"}` : "Connect to manage Facebook Pages"}
                  </p>
                </div>
              </div>

              <div className="space-y-3">
                <button
                  onClick={connectFacebook}
                  className="w-full py-2.5 bg-surface-container-high text-on-surface font-semibold text-label-md rounded-xl hover:bg-outline-variant/30 transition-all"
                >
                  {primaryAccount ? "Reconnect Account" : "Connect Facebook Account"}
                </button>
                <button
                  disabled={!primaryAccount}
                  onClick={() => primaryAccount && disconnectAccount(primaryAccount.id)}
                  className="w-full py-2.5 text-error font-semibold text-label-md rounded-xl hover:bg-error-container/20 transition-all disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Disconnect Account
                </button>
              </div>

              <div className="pt-4 border-t border-outline-variant/30 flex items-center justify-between gap-3">
                <span className="text-label-sm text-outline">Token Expiry Tracking</span>
                <span className="text-secondary bg-secondary/10 px-2 py-0.5 rounded font-bold text-[10px]">
                  {expiringInDays === null ? "COMING SOON" : `${Math.max(expiringInDays, 0)} DAYS`}
                </span>
              </div>
            </div>

            <div className="bg-warning-amber/10 border border-warning-amber/30 rounded-2xl p-4 flex items-start gap-3">
              <span className="material-symbols-outlined text-warning-amber">warning</span>
              <div>
                <p className="text-label-md text-on-surface font-bold">Security Notice</p>
                <p className="text-body-sm text-on-surface-variant">
                  {expiringInDays !== null && expiringInDays <= 7
                    ? `Your Meta token expires in ${Math.max(expiringInDays, 0)} day${expiringInDays === 1 ? "" : "s"}. Reconnect soon to keep scheduled posting active.`
                    : "Access tokens are encrypted in backend storage. Reconnect if Facebook permissions change."}
                </p>
              </div>
            </div>
          </div>

          <div className="col-span-12 lg:col-span-8 flex flex-col">
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/50 shadow-sm flex flex-col h-full overflow-hidden">
              <div className="px-6 py-5 border-b border-outline-variant flex justify-between items-center gap-4">
                <h2 className="text-headline-sm text-on-surface">Linked Pages & Ad Accounts</h2>
                <div className="flex gap-2">
                  <button className="p-2 rounded-xl hover:bg-surface-container-high text-outline transition-all" title="Filter">
                    <span className="material-symbols-outlined">filter_list</span>
                  </button>
                  <button className="p-2 rounded-xl hover:bg-surface-container-high text-outline transition-all" title="Export">
                    <span className="material-symbols-outlined">download</span>
                  </button>
                </div>
              </div>

              {loading ? (
                <div className="p-6 space-y-3 animate-pulse">
                  {Array.from({ length: 4 }).map((_, index) => (
                    <div key={index} className="h-14 rounded-xl bg-surface-container" />
                  ))}
                </div>
              ) : linkedTargets.length === 0 ? (
                <div className="flex-1 min-h-[320px] flex flex-col items-center justify-center text-center p-10">
                  <div className="w-16 h-16 rounded-2xl bg-surface-container-high flex items-center justify-center text-outline mb-5">
                    <span className="material-symbols-outlined text-[32px]">add_link</span>
                  </div>
                  <h3 className="text-headline-sm text-on-surface mb-2">No linked pages yet</h3>
                  <p className="text-body-sm text-on-surface-variant max-w-sm mb-6">
                    Connect Facebook, then link a Page to one of your brands so content can be published from schedules.
                  </p>
                  <button
                    onClick={openPlatformModal}
                    className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-label-md font-semibold hover:opacity-90"
                  >
                    <span className="material-symbols-outlined text-[18px]">add</span>
                    Link New Target
                  </button>
                </div>
              ) : (
                <div className="flex-1 overflow-x-auto">
                  <table className="w-full text-left">
                    <thead>
                      <tr className="bg-surface-container-low/50 text-outline text-label-md border-b border-outline-variant/30">
                        <th className="px-6 py-4 font-semibold uppercase tracking-wider">Type</th>
                        <th className="px-6 py-4 font-semibold uppercase tracking-wider">Name</th>
                        <th className="px-6 py-4 font-semibold uppercase tracking-wider">Status</th>
                        <th className="px-6 py-4 font-semibold uppercase tracking-wider">Linked Since</th>
                        <th className="px-6 py-4 font-semibold uppercase tracking-wider text-right">Action</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-outline-variant/20">
                      {linkedTargets.map((target) => (
                        <tr key={target.id} className="hover:bg-primary/5 transition-colors group">
                          <td className="px-6 py-4">
                            <span className="flex items-center gap-2 text-on-surface-variant text-label-md">
                              <span className="material-symbols-outlined text-primary text-[20px]">flag</span>
                              Facebook Page
                            </span>
                          </td>
                          <td className="px-6 py-4 text-body-md text-on-surface font-semibold">{target.name || target.providerTargetId}</td>
                          <td className="px-6 py-4">
                            <span className={`px-2 py-0.5 rounded-full text-[12px] font-bold ${
                              target.isActive ? "bg-success-green/10 text-success-green" : "bg-outline-variant/30 text-outline"
                            }`}>
                              {target.isActive ? "Active" : "Inactive"}
                            </span>
                          </td>
                          <td className="px-6 py-4 text-on-surface-variant text-body-sm">{formatDate(target.account.createdAt)}</td>
                          <td className="px-6 py-4 text-right">
                            <button onClick={() => unlinkTarget(target.id)} className="text-error text-label-md hover:underline">
                              Unlink
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              <div className="p-6 bg-surface-container-low/50 flex flex-col sm:flex-row gap-4 mt-auto">
                <button
                  onClick={openPlatformModal}
                  className="flex-1 py-3 border-2 border-dashed border-outline-variant hover:border-primary hover:bg-primary/5 text-outline hover:text-primary text-label-md font-semibold rounded-xl transition-all flex items-center justify-center gap-2 group"
                >
                  <span className="material-symbols-outlined group-hover:scale-110 transition-transform">add_link</span>
                  Link New Target
                </button>
                <button className="flex-1 py-3 border-2 border-dashed border-outline-variant text-outline/50 text-label-md font-semibold rounded-xl flex items-center justify-center gap-2 cursor-not-allowed" title="Coming soon">
                  <span className="material-symbols-outlined">payments</span>
                  Link New Ad Account
                </button>
              </div>
            </div>
          </div>
        </div>

        <section>
          <div className="mb-6">
            <h2 className="text-headline-sm text-on-surface">Future Integrations</h2>
            <p className="text-body-sm text-on-surface-variant">AISAM will expand beyond Facebook after the current publishing flow is stable.</p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
            {futureIntegrations.map((item) => (
              <div key={item.name} className={`bg-surface-container-lowest border border-outline-variant/50 rounded-2xl p-6 transition-all ${item.dim}`}>
                <div className="flex items-center gap-4 mb-4">
                  <div className={`w-10 h-10 ${item.iconClass} rounded-xl flex items-center justify-center text-white`}>
                    <span className="material-symbols-outlined">{item.icon}</span>
                  </div>
                  <h3 className="text-headline-sm text-on-surface">{item.name}</h3>
                </div>
                <p className="text-body-sm text-on-surface-variant mb-4">{item.desc}</p>
                <span className={`${item.badgeClass} font-bold text-label-md px-3 py-1 rounded-full`}>{item.badge}</span>
              </div>
            ))}
          </div>
        </section>

        <footer className="mt-12 flex flex-col sm:flex-row justify-between gap-3 text-outline/50 hover:text-outline transition-colors">
          <div className="flex items-center gap-4">
            <span className="text-[10px] font-semibold">VER: 1.4.2-ALPHA</span>
            <span className="text-[10px] font-semibold flex items-center gap-1">
              <span className="w-2 h-2 bg-success-green rounded-full" />
              SYSTEMS OPERATIONAL
            </span>
          </div>
          <div className="text-[10px] font-semibold uppercase">API_HEALTH_OK_200</div>
        </footer>
      </main>

      {platformModalOpen && (
        <div className="fixed inset-0 z-[70] flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-enterprise-navy/40 backdrop-blur-sm" onClick={() => setPlatformModalOpen(false)} />
          <div className="relative w-full max-w-2xl bg-surface-container-lowest rounded-xl border border-outline-variant shadow-2xl overflow-hidden">
            <div className="p-stack-lg border-b border-outline-variant flex justify-between items-start gap-4">
              <div>
                <h2 className="text-headline-sm text-on-surface">Link New Social Target</h2>
                <p className="text-body-sm text-on-surface-variant mt-1">
                  Select a platform to connect your brand assets and start publishing AI-powered campaigns.
                </p>
              </div>
              <button className="p-2 hover:bg-surface-container-high rounded-full transition-colors" onClick={() => setPlatformModalOpen(false)}>
                <span className="material-symbols-outlined text-on-surface-variant">close</span>
              </button>
            </div>

            <div className="p-stack-lg grid grid-cols-1 sm:grid-cols-2 gap-stack-md">
              {platformOptions.map((platform) => {
                const selected = platform.enabled && selectedPlatform === platform.id;
                if (!platform.enabled) {
                  return (
                    <div key={platform.id} className="relative flex flex-col items-center justify-center p-6 border-2 border-outline-variant rounded-xl bg-surface-container opacity-60 cursor-not-allowed min-h-40">
                      <div className={`w-12 h-12 ${platform.iconClass} rounded-full flex items-center justify-center mb-stack-sm`}>
                        <span className="material-symbols-outlined text-3xl">{platform.icon}</span>
                      </div>
                      <span className="text-label-md text-on-surface-variant">{platform.name}</span>
                      <span className="absolute top-2 right-2 bg-surface-container-highest px-2 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider text-on-surface-variant">
                        Coming Soon
                      </span>
                    </div>
                  );
                }

                return (
                  <button
                    key={platform.id}
                    onClick={() => setSelectedPlatform(platform.id)}
                    className={`group relative flex flex-col items-center justify-center p-6 border-2 rounded-xl bg-surface hover:bg-surface-container-low transition-all min-h-40 ${
                      selected ? "border-primary bg-surface-container-low shadow-[0_0_15px_rgba(0,76,205,0.15)]" : "border-outline-variant hover:border-secondary"
                    }`}
                  >
                    <div className={`w-12 h-12 ${platform.iconClass} rounded-full flex items-center justify-center mb-stack-sm group-hover:scale-110 transition-transform`}>
                      <span className="material-symbols-outlined text-3xl" style={platform.id === "facebook" ? { fontVariationSettings: "'FILL' 1" } : undefined}>
                        {platform.icon}
                      </span>
                    </div>
                    <span className="text-label-md text-on-surface">{platform.name}</span>
                    <div className={`absolute top-2 right-2 transition-opacity ${selected ? "opacity-100" : "opacity-0"}`}>
                      <span className="material-symbols-outlined text-primary">check_circle</span>
                    </div>
                  </button>
                );
              })}
            </div>

            <div className="p-stack-lg bg-surface-container-low border-t border-outline-variant flex justify-end gap-stack-md">
              <button
                className="px-6 py-2.5 rounded-lg text-label-md text-on-surface-variant hover:bg-surface-container-high transition-colors"
                onClick={() => setPlatformModalOpen(false)}
              >
                Cancel
              </button>
              <button
                className={`px-8 py-2.5 rounded-lg text-label-md transition-all shadow-sm ${
                  selectedPlatform
                    ? "bg-primary text-on-primary hover:opacity-90"
                    : "bg-outline-variant text-on-surface-variant cursor-not-allowed"
                }`}
                disabled={!selectedPlatform}
                onClick={continueSelectedPlatform}
              >
                Continue
              </button>
            </div>
          </div>
        </div>
      )}

      {modalOpen && (
        <div className="fixed inset-0 z-[70] flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-enterprise-navy/40 backdrop-blur-sm" onClick={() => setModalOpen(false)} />
          <div className="relative w-full max-w-2xl bg-surface-container-lowest rounded-2xl border border-outline-variant shadow-2xl overflow-hidden">
            <div className="px-6 py-5 border-b border-outline-variant/30 flex items-center justify-between">
              <div>
                <h2 className="text-headline-sm text-on-surface">Link Facebook Page</h2>
                <p className="text-body-sm text-on-surface-variant">Select a Facebook page and attach it to a brand.</p>
              </div>
              <button onClick={() => setModalOpen(false)} className="p-2 rounded-xl hover:bg-surface-container">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>

            <div className="p-6 space-y-5">
              <label className="block">
                <span className="text-label-md text-outline uppercase">Facebook Account</span>
                <select
                  value={selectedAccountId}
                  onChange={(event) => loadAvailableTargets(event.target.value)}
                  className="mt-2 w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-4 py-3 text-body-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/10"
                >
                  <option value="">Select account</option>
                  {accounts.map((account) => (
                    <option key={account.id} value={account.id}>{getAccountLabel(account)}</option>
                  ))}
                </select>
              </label>

              <label className="block">
                <span className="text-label-md text-outline uppercase">Brand</span>
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

              <div>
                <div className="text-label-md text-outline uppercase mb-2">Available Pages</div>
                <div className="max-h-64 overflow-y-auto border border-outline-variant/40 rounded-2xl divide-y divide-outline-variant/20">
                  {availableTargets.length === 0 ? (
                    <div className="p-5 text-body-sm text-on-surface-variant">No Facebook pages returned for this account.</div>
                  ) : availableTargets.map((target) => {
                    const checked = selectedTargetIds.includes(target.providerTargetId);
                    return (
                      <button
                        key={target.providerTargetId}
                        onClick={() => toggleSelectedTarget(target.providerTargetId)}
                        className={`w-full flex items-center gap-4 px-5 py-4 text-left hover:bg-primary/5 transition-colors ${checked ? "bg-primary/5" : ""}`}
                      >
                        <span className={`w-5 h-5 rounded-md border flex items-center justify-center shrink-0 ${checked ? "bg-primary border-primary text-on-primary" : "border-outline-variant"}`}>
                          {checked && <span className="material-symbols-outlined text-[14px]">check</span>}
                        </span>
                        <span className="w-10 h-10 rounded-xl bg-primary-fixed text-primary flex items-center justify-center shrink-0">
                          <span className="material-symbols-outlined">flag</span>
                        </span>
                        <span className="min-w-0">
                          <span className="block text-body-sm font-semibold text-on-surface truncate">{target.name || target.providerTargetId}</span>
                          <span className="block text-label-sm text-outline truncate">{target.category || target.type}</span>
                        </span>
                      </button>
                    );
                  })}
                </div>
              </div>
            </div>

            <div className="px-6 py-4 bg-surface-container-low flex justify-end gap-3">
              <button onClick={() => setModalOpen(false)} className="px-5 py-2.5 rounded-xl border border-outline-variant text-on-surface text-label-md font-semibold hover:bg-surface-container">
                Cancel
              </button>
              <button
                onClick={linkSelectedTargets}
                disabled={linking || !selectedAccountId || !selectedBrandId || selectedTargetIds.length === 0}
                className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-md font-semibold hover:opacity-90 disabled:opacity-40 disabled:cursor-not-allowed"
              >
                {linking ? "Linking..." : "Link Selected"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
