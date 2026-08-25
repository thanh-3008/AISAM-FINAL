"use client";

import { useState, useEffect, useRef, Suspense } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { motion, useReducedMotion } from "motion/react";
import { invalidateWorkspaceCache, useWorkspaces } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { useToast } from "@/contexts/ToastContext";
import { fetchCreditWallet, type CreditWallet } from "@/services/workspaceService";
import { createBusinessWorkspacePayment, createPayment, exitPayment, synchronizeBusinessWorkspacePayment, syncPayOSCallback, PLAN_CODES, CREDIT_PACK_CODES_BY_ID, fetchPublicPricing } from "@/services/paymentService";
import { PlanType, PLAN_NAMES, PLAN_HIERARCHY } from "@/lib/featureConfig";
import { PLAN_PRICING, CREDIT_PACK_PRICING, type PlanPricing, type CreditPackPricing } from "@/lib/pricing";
import { getCurrentSubscription } from "@/services/profileSettingsService";

type TabType = "subscription" | "credits";
type PlanCategory = "personal" | "business";
const CREATED_WORKSPACE_PAYMENT_KEY = "aisam-created-workspace-payment";
const PRICING_PAYMENT_TYPE_KEY = "aisam-pricing-payment-type";
const PRICING_PAYMENT_REFERENCE_KEY = "aisam-pricing-payment-reference";
const PRICING_PAYMENT_ACTIVE_KEY = "aisam-pricing-payment-active";
type PricingPaymentType = "subscription" | "credits";

function getErrorMessage(error: unknown, fallback: string) {
  return error instanceof Error && error.message ? error.message : fallback;
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount);
}

const PRICING_SETTING_ID_BY_PLAN_TYPE: Record<PlanType, string> = {
  [PlanType.Free]: "free",
  [PlanType.PersonalPlus]: "plus",
  [PlanType.PersonalPro]: "premium",
  [PlanType.BusinessPlus]: "business-plus",
  [PlanType.BusinessPro]: "business-pro",
};

function PricingContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { activeWorkspace, updateWorkspacePlan } = useWorkspaces();
  const hasPaymentRedirect = searchParams.has("orderCode") || searchParams.has("id");
  const featureGate = useFeatureGate(!hasPaymentRedirect);
  const { showToast } = useToast();
  const reduceMotion = useReducedMotion();

  const createMode = searchParams.get("create") === "business";
  const categoryParam = searchParams.get("category");
  const activeWorkspaceCategory: PlanCategory = activeWorkspace?.workspaceType === 2 ? "business" : "personal";

  const [activeTab, setActiveTab] = useState<TabType>("subscription");
  const [planCategory, setPlanCategory] = useState<PlanCategory>(
    createMode || categoryParam === "business" ? "business" : activeWorkspaceCategory
  );
  const [yearly, setYearly] = useState(false);
  const [creditWallet, setCreditWallet] = useState<CreditWallet | null>(null);
  const [processing, setProcessing] = useState<number | null>(null);
  const [syncingPayment, setSyncingPayment] = useState(false);
  const [displayPlans, setDisplayPlans] = useState<PlanPricing[]>(PLAN_PRICING);
  const [displayPacks, setDisplayPacks] = useState<CreditPackPricing[]>(CREDIT_PACK_PRICING);
  const [pricingLoaded, setPricingLoaded] = useState(false);

  // Create workspace form state
  const [wsName, setWsName] = useState("");
  const [businessTaxId, setBusinessTaxId] = useState("");
  const [businessLegalName, setBusinessLegalName] = useState("");
  const [creating, setCreating] = useState(false);

  // Payment state
  const [showQRModal, setShowQRModal] = useState(false);
  const [qrData, setQrData] = useState<{
    checkoutUrl: string;
    amount: number;
    description: string;
    summary: string;
    type: "subscription" | "credits" | "workspace";
  } | null>(null);
  const [selectedPack, setSelectedPack] = useState<CreditPackPricing | null>(null);
  const [isClient, setIsClient] = useState(false);
  const paymentSyncStartedRef = useRef(false);
  const paymentExitStartedRef = useRef(false);

  useEffect(() => { setIsClient(true); }, []);

  useEffect(() => {
    let mounted = true;
    fetchPublicPricing()
      .then((res) => {
        if (!mounted) return;
        if (res) {
          setDisplayPlans(prev => prev.map(p => {
            const expectedId = PRICING_SETTING_ID_BY_PLAN_TYPE[p.planType];
            const matched = res.plans.find(bp =>
              String(bp.id ?? "").toLowerCase() === expectedId ||
              String(bp.name ?? "").toLowerCase() === p.name.toLowerCase()
            );
            if (matched) {
              const postsPerMonth = Number(matched.postsPerMonth);
              const price = Number(matched.price ?? p.price);
              const credits = Number(matched.credits ?? p.credits);
              return {
                ...p,
                price: Number.isFinite(price) ? price : p.price,
                priceFormatted: formatCurrency(Number.isFinite(price) ? price : p.price),
                credits: Number.isFinite(credits) ? credits : p.credits,
                postQuota: Number.isFinite(postsPerMonth) ? `${postsPerMonth.toLocaleString()} posts/month` : p.postQuota,
              };
            }
            return p;
          }));
          setDisplayPacks(prev => prev.map(p => {
            const matched = res.creditPacks.find(bp =>
              String(bp.id ?? "").toLowerCase() === p.id.toLowerCase() ||
              String(bp.name ?? "").toLowerCase() === p.name.toLowerCase()
            );
            if (matched) {
              const price = Number(matched.price ?? p.price);
              const credits = Number(matched.credits ?? p.credits);
              return {
                ...p,
                price: Number.isFinite(price) ? price : p.price,
                priceFormatted: formatCurrency(Number.isFinite(price) ? price : p.price),
                credits: Number.isFinite(credits) ? credits : p.credits
              };
            }
            return p;
          }));
        }
      })
      .finally(() => {
        if (mounted) setPricingLoaded(true);
      });
    return () => {
      mounted = false;
    };
  }, []);

  useEffect(() => {
    if (hasPaymentRedirect || !activeWorkspace?.id) return;
    fetchCreditWallet().then(w => setCreditWallet(w));
  }, [activeWorkspace?.id, hasPaymentRedirect]);

  useEffect(() => {
    const pendingBusinessReference = window.sessionStorage.getItem(CREATED_WORKSPACE_PAYMENT_KEY);
    const isBusinessCreation = Boolean(pendingBusinessReference);
    const pendingPricingPaymentType = window.sessionStorage.getItem(PRICING_PAYMENT_TYPE_KEY) as PricingPaymentType | null;
    const pendingPricingReference = window.sessionStorage.getItem(PRICING_PAYMENT_REFERENCE_KEY);
    const redirectStatus = searchParams.get("status")?.toUpperCase();
    const isCancelled = searchParams.get("cancel") === "true" || redirectStatus === "CANCELLED" || searchParams.get("payment") === "cancelled";
    const redirectPaid = !isCancelled &&
      (redirectStatus === "PAID" || redirectStatus === "SUCCESS" || redirectStatus === "COMPLETED" || searchParams.get("code") === "00");
    if (!hasPaymentRedirect || (!activeWorkspace?.id && !isBusinessCreation)) return;
    if (paymentSyncStartedRef.current) return;

    paymentSyncStartedRef.current = true;
    setSyncingPayment(true);
    const synchronizePayment = async () => {
      try {
        if (!redirectPaid) {
          // Notify the backend so the dangling Pending record is marked Failed
          try { await syncPayOSCallback(searchParams); } catch { /* ignore – best-effort */ }
          if (isBusinessCreation) {
            window.sessionStorage.removeItem(CREATED_WORKSPACE_PAYMENT_KEY);
          }
          window.sessionStorage.removeItem(PRICING_PAYMENT_TYPE_KEY);
          window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
          showToast({ type: "warning", title: "Payment cancelled", message: "You have cancelled the payment." });
          router.replace(isBusinessCreation ? "/pricing?create=business" : "/pricing");
          return;
        }

        if (isBusinessCreation) {
          const reference = pendingBusinessReference || searchParams.get("orderCode");
          const success = reference ? await synchronizeBusinessWorkspacePayment(reference) : false;
          if (!success) {
            showToast({ type: "error", title: "Payment verification failed", message: "PayOS has not confirmed this payment yet. Please retry from the workspace overview." });
            paymentSyncStartedRef.current = false;
            return;
          }
          window.sessionStorage.removeItem(CREATED_WORKSPACE_PAYMENT_KEY);
          invalidateWorkspaceCache();
          showToast({
            type: "success",
            title: "Workspace created",
            message: "Your paid Business workspace is now active.",
          });
          window.location.replace("/overview");
          return;
        }

        const reference = searchParams.get("orderCode") || pendingPricingReference || searchParams.get("id");
        const subscriptionBefore = await getCurrentSubscription();
        const success = reference ? await synchronizeBusinessWorkspacePayment(reference) : false;
        if (!success) {
          showToast({ type: "error", title: "Payment sync failed", message: "Payment was received but could not be synchronized. Please contact support." });
          router.replace("/pricing");
          return;
        }

        const subscription = await getCurrentSubscription();
        const wallet = await fetchCreditWallet();

        const subscriptionPlanName = subscription?.planName;
        if (pendingPricingPaymentType === "subscription" && !subscriptionPlanName) {
          window.sessionStorage.removeItem(PRICING_PAYMENT_TYPE_KEY);
          window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
          showToast({ type: "error", title: "Subscription refresh failed", message: "Payment was synchronized, but the active subscription could not be loaded." });
          router.replace("/pricing");
          return;
        }

        const isSubscriptionUpgrade = Boolean(subscriptionPlanName &&
          (pendingPricingPaymentType === "subscription" ||
            (!pendingPricingPaymentType && subscriptionPlanName !== subscriptionBefore?.planName)));
        if (isSubscriptionUpgrade && subscriptionPlanName) {
          window.sessionStorage.removeItem(PRICING_PAYMENT_TYPE_KEY);
          window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
          updateWorkspacePlan(activeWorkspace.id, subscriptionPlanName);
          showToast({
            type: "success",
            title: "Payment successful",
            message: `${subscriptionPlanName} is now active for this workspace.`,
          });
          router.replace("/dashboard");
          return;
        }

        window.sessionStorage.removeItem(PRICING_PAYMENT_TYPE_KEY);
        window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
        if (wallet) setCreditWallet(wallet);
        showToast({
          type: "success",
          title: "Payment successful",
          message: "Your credit balance has been updated.",
        });
        router.replace("/pricing");
      } catch (error) {
        paymentSyncStartedRef.current = false;
        const message = error instanceof Error ? error.message : "Payment was received, but subscription refresh failed.";
        showToast({ type: "error", title: "Subscription refresh failed", message });
      } finally {
        setSyncingPayment(false);
      }
    };

    synchronizePayment();
  }, [activeWorkspace?.id, hasPaymentRedirect, router, searchParams, showToast, updateWorkspacePlan]);

  useEffect(() => {
    const activeReference = window.sessionStorage.getItem(PRICING_PAYMENT_ACTIVE_KEY);
    const reference = window.sessionStorage.getItem(PRICING_PAYMENT_REFERENCE_KEY) || window.sessionStorage.getItem(CREATED_WORKSPACE_PAYMENT_KEY);
    if (hasPaymentRedirect || !activeReference || !reference || paymentExitStartedRef.current) return;

    paymentExitStartedRef.current = true;
    exitPayment(activeReference).then((result) => {
      window.sessionStorage.removeItem(PRICING_PAYMENT_ACTIVE_KEY);
      window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
      window.sessionStorage.removeItem(CREATED_WORKSPACE_PAYMENT_KEY);
      if (result?.status === "Success") {
        showToast({ type: "success", title: "Payment successful", message: "Your payment was confirmed." });
      } else {
        showToast({ type: "warning", title: "Payment cancelled", message: "You have cancelled the payment." });
      }
      router.replace(createMode ? "/pricing?create=business" : "/pricing");
    }).catch(() => {
      paymentExitStartedRef.current = false;
    });
  }, [createMode, hasPaymentRedirect, router, showToast]);

  useEffect(() => {
    if (createMode || categoryParam === "business") {
      setPlanCategory("business");
      return;
    }
    setPlanCategory(activeWorkspaceCategory);
  }, [activeWorkspaceCategory, categoryParam, createMode]);

  const currentPlan = featureGate.plan;
  const isCurrentPlan = (planType: PlanType) => currentPlan === planType;
  const isLowerPlan = (planType: PlanType) => PLAN_HIERARCHY[planType] < PLAN_HIERARCHY[currentPlan];

  const filteredPlans = displayPlans.filter(p => p.category === planCategory);

  const handleUpgrade = async (plan: PlanPricing) => {
    if (!createMode && isCurrentPlan(plan.planType)) return;
    setProcessing(plan.planType);

    if (createMode) {
      if (!wsName.trim()) {
        showToast({ type: "error", title: "Missing name", message: "Please enter a workspace name." });
        setProcessing(null);
        return;
      }
      if (!businessLegalName.trim()) {
        showToast({ type: "error", title: "Missing legal name", message: "Please enter the registered business name." });
        setProcessing(null);
        return;
      }
      if (!businessTaxId.trim()) {
        showToast({ type: "error", title: "Missing tax ID", message: "Please enter the business tax ID." });
        setProcessing(null);
        return;
      }

      setCreating(true);
      try {
        const planCode = PLAN_CODES[plan.planType];
        if (!planCode) {
          throw new Error(`Unsupported plan: ${plan.name}`);
        }
        const payment = await createBusinessWorkspacePayment({
          workspaceName: wsName.trim(),
          legalBusinessName: businessLegalName.trim(),
          taxId: businessTaxId.trim(),
          planCode,
          returnUrl: window.location.origin + "/pricing",
          cancelUrl: window.location.origin + "/pricing?create=business",
        });
        if (!payment?.checkoutUrl) {
          throw new Error("PayOS checkout URL was not returned.");
        }
        const paymentReference = payment.orderCode || payment.paymentLinkId;
        if (!paymentReference) {
          throw new Error("PayOS payment reference was not returned.");
        }
        window.sessionStorage.setItem(CREATED_WORKSPACE_PAYMENT_KEY, paymentReference);
        setQrData({
          checkoutUrl: payment.checkoutUrl,
          amount: plan.price,
          description: plan.name,
          summary: `Create Business workspace "${wsName.trim()}" with ${plan.name}.`,
          type: "workspace",
        });
        setShowQRModal(true);
      } catch (error) {
        const message = getErrorMessage(error, "Failed to start Business workspace checkout.");
        showToast({ type: "error", title: "Checkout failed", message });
      } finally {
        setCreating(false);
        setProcessing(null);
      }
      return;
    }

    try {
      const planCode = PLAN_CODES[plan.planType] || "Plus";
      if (!planCode) {
        throw new Error(`Unsupported plan: ${plan.name}`);
      }
      const payment = await createPayment({
        paymentType: 1,
        planCode,
        returnUrl: window.location.origin + "/pricing",
        cancelUrl: window.location.origin + "/pricing",
      });

      if (payment?.checkoutUrl) {
        window.sessionStorage.setItem(PRICING_PAYMENT_TYPE_KEY, "subscription");
        if (payment.orderCode) {
          window.sessionStorage.setItem(PRICING_PAYMENT_REFERENCE_KEY, payment.orderCode);
        }
        setQrData({
          checkoutUrl: payment.checkoutUrl,
          amount: plan.price,
          description: plan.name,
          summary: `${plan.postQuota} · ${plan.credits.toLocaleString()} AI credits`,
          type: "subscription",
        });
        setShowQRModal(true);
      } else {
        showToast({ type: "info", title: "Upgrade", message: `PayOS checkout will redirect for ${plan.name} plan.` });
      }
    } catch (error) {
      showToast({
        type: "error",
        title: "Checkout failed",
        message: getErrorMessage(error, `Failed to process ${plan.name}.`),
      });
    } finally {
      setProcessing(null);
    }
  };

  const handleBuyCreditPack = async (pack: CreditPackPricing) => {
    setSelectedPack(pack);
    setProcessing(-1);

    try {
      const creditPackCode = CREDIT_PACK_CODES_BY_ID[pack.id] || 1;
      const payment = await createPayment({
        paymentType: 2,
        creditPackCode,
        returnUrl: window.location.origin + "/pricing",
        cancelUrl: window.location.origin + "/pricing",
      });

      if (payment?.checkoutUrl) {
        window.sessionStorage.setItem(PRICING_PAYMENT_TYPE_KEY, "credits");
        if (payment.orderCode) {
          window.sessionStorage.setItem(PRICING_PAYMENT_REFERENCE_KEY, payment.orderCode);
        }
        setQrData({
          checkoutUrl: payment.checkoutUrl,
          amount: pack.price,
          description: pack.name,
          summary: `${pack.credits.toLocaleString()} additional AI credits`,
          type: "credits",
        });
        setShowQRModal(true);
      } else {
        showToast({ type: "info", title: "Purchase", message: `PayOS checkout will redirect for ${pack.name} pack.` });
      }
    } catch {
      showToast({ type: "error", title: "Error", message: "Failed to process purchase." });
    } finally {
      setProcessing(null);
    }
  };

  const handlePaymentExit = async () => {
    if (paymentExitStartedRef.current) return;
    paymentExitStartedRef.current = true;
    const reference = window.sessionStorage.getItem(CREATED_WORKSPACE_PAYMENT_KEY) || window.sessionStorage.getItem(PRICING_PAYMENT_REFERENCE_KEY);
    setShowQRModal(false);
    if (!reference) {
      showToast({ type: "warning", title: "Payment cancelled", message: "You have cancelled the payment." });
      return;
    }
    try {
      const result = await exitPayment(reference);
      window.sessionStorage.removeItem(PRICING_PAYMENT_ACTIVE_KEY);
      window.sessionStorage.removeItem(PRICING_PAYMENT_REFERENCE_KEY);
      window.sessionStorage.removeItem(CREATED_WORKSPACE_PAYMENT_KEY);
      showToast(result?.status === "Success"
        ? { type: "success", title: "Payment successful", message: "Your payment was confirmed." }
        : { type: "warning", title: "Payment cancelled", message: "You have cancelled the payment." });
    } catch {
      paymentExitStartedRef.current = false;
      showToast({ type: "error", title: "Payment update failed", message: "We could not update the payment status. Please try again." });
    }
  };

  const container = { hidden: { opacity: 0 }, show: { opacity: 1, transition: { staggerChildren: 0.08 } } };
  const item = { hidden: { opacity: 0, y: 20 }, show: { opacity: 1, y: 0 } };
  const qrStatus = (() => "pending" as "pending" | "completed" | "failed")();
  const handleMockPayment = () => undefined;

  return (
    <main className="min-h-[100dvh] bg-surface flex flex-col relative overflow-hidden">
      {/* Background decoration */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-primary/[0.03] rounded-full blur-[120px] -translate-y-1/2 translate-x-1/4" />
        <div className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-secondary/[0.03] rounded-full blur-[100px] translate-y-1/3 -translate-x-1/4" />
      </div>

      {syncingPayment && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-surface/80 backdrop-blur-sm">
          <div className="flex flex-col items-center gap-3">
            <svg className="w-8 h-8 animate-spin text-primary" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            <p className="text-body-sm text-on-surface-variant">Processing payment...</p>
          </div>
        </div>
      )}

      <div className="flex-1 px-6 md:px-8 lg:px-12 max-w-6xl mx-auto w-full py-12 md:py-16 relative z-10">
        {/* Top bar - back to dashboard */}
        <div className="flex items-center justify-between mb-10">
          <button
            onClick={() => router.push(activeWorkspace ? "/dashboard" : "/overview")}
            className="inline-flex items-center gap-2 px-4 py-2 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container/50 transition-all bg-surface-container-lowest/60 border border-outline-variant/20"
          >
            <span className="material-symbols-outlined text-[18px]">arrow_back</span>
            Back to {isClient && activeWorkspace ? "Dashboard" : "Overview"}
          </button>
          <Link href="/" className="flex items-center gap-2.5 rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary">
            <div className="w-8 h-8 bg-gradient-to-br from-primary to-primary-container rounded-lg flex items-center justify-center shadow-sm shadow-primary/20">
              <span className="material-symbols-outlined text-on-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>psychology</span>
            </div>
            <span className="text-headline-xs font-bold text-on-surface tracking-tight">AISAM</span>
          </Link>
        </div>

        <div className="space-y-8">
          {/* Header */}
          <div className="text-center">
            {createMode && (
              <div className="flex items-center justify-center gap-2 mb-4">
                <div className="flex items-center gap-1.5">
                  <span className="w-6 h-6 rounded-full bg-primary text-on-primary flex items-center justify-center text-label-2xs font-bold">1</span>
                  <span className="text-label-xs text-on-surface-variant">Overview</span>
                </div>
                <span className="material-symbols-outlined text-outline/40 text-[14px]">chevron_right</span>
                <div className="flex items-center gap-1.5">
                  <span className="w-6 h-6 rounded-full bg-primary text-on-primary flex items-center justify-center text-label-2xs font-bold">2</span>
                  <span className="text-label-xs font-semibold text-primary">Workspace &amp; Plan</span>
                </div>
                <span className="material-symbols-outlined text-outline/40 text-[14px]">chevron_right</span>
                <div className="flex items-center gap-1.5">
                  <span className="w-6 h-6 rounded-full bg-outline/20 text-outline flex items-center justify-center text-label-2xs font-bold">3</span>
                  <span className="text-label-xs text-outline">Payment</span>
                </div>
              </div>
            )}
            <h1 className="text-headline-lg font-bold text-on-surface mb-2">
              {createMode ? "Choose a Business Plan" : "Choose Your Plan"}
            </h1>
            <p className="text-body-md text-on-surface-variant max-w-2xl mx-auto">
              {createMode
                ? "Enter your workspace details, then select a business plan to get started."
                : "Unlock the full potential of AI-powered social media management. Upgrade your plan to access more features, credits, and team collaboration tools."}
            </p>
          </div>

          {/* Tab Switcher: Subscription | Credits - hide in create mode */}
          {!createMode && (
            <div className="flex items-center justify-center gap-2 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-1.5 w-fit mx-auto">
              <button
                onClick={() => setActiveTab("subscription")}
                className={`px-6 py-2.5 rounded-xl text-label-sm font-semibold transition-all ${
                  activeTab === "subscription"
                    ? "bg-surface-container text-on-surface shadow-sm"
                    : "text-outline hover:text-on-surface"
                }`}
              >
                <span className="material-symbols-outlined text-[16px] align-middle mr-1.5">workspace_premium</span>
                Subscription Plans
              </button>
              <button
                onClick={() => setActiveTab("credits")}
                className={`px-6 py-2.5 rounded-xl text-label-sm font-semibold transition-all ${
                  activeTab === "credits"
                    ? "bg-surface-container text-on-surface shadow-sm"
                    : "text-outline hover:text-on-surface"
                }`}
              >
                <span className="material-symbols-outlined text-[16px] align-middle mr-1.5">token</span>
                Credit Packs
              </button>
            </div>
          )}

          {/* Current Plan Badge - hide in create mode */}
          {!createMode && (
            <div className="flex items-center justify-center gap-3 px-5 py-3 rounded-xl bg-primary/5 border border-primary/20">
              <span className="material-symbols-outlined text-primary text-[20px]">check_circle</span>
              <span className="text-body-sm text-on-surface">
                Current plan: <strong>{PLAN_NAMES[currentPlan]}</strong>
                {creditWallet && (
                  <span className="ml-2 text-outline">· {creditWallet.balance.toLocaleString()} credits remaining</span>
                )}
              </span>
              <span className="text-label-xs text-outline/50">·</span>
              <button
                onClick={() => {
                  router.push(activeWorkspace ? `/profiles/${activeWorkspace.id}?section=subscription` : "/overview");
                }}
                className="text-label-sm font-semibold text-primary hover:underline"
              >
                Manage
              </button>
            </div>
          )}

          {/* Subscription Plans */}
          {activeTab === "subscription" && (
            <motion.div variants={container} initial="hidden" animate="show" className="space-y-6">
              {/* Create Mode Form */}
              {createMode && (
                <motion.div variants={item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 max-w-xl mx-auto w-full">
                  <div className="flex items-center gap-3 mb-5">
                    <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-primary text-[22px]">business</span>
                    </div>
                    <div>
                      <h3 className="text-body-md font-bold text-on-surface">Workspace Details</h3>
                      <p className="text-label-xs text-on-surface-variant">Name your workspace to continue</p>
                    </div>
                  </div>
                  <div className="space-y-4">
                    <div>
                      <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">
                        Workspace Name <span className="text-danger-red">*</span>
                      </label>
                      <input
                        className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                        placeholder="e.g. Acme Marketing"
                        value={wsName}
                        onChange={(e) => setWsName(e.target.value)}
                        autoFocus
                      />
                    </div>
                    <div>
                      <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">
                        Legal Business Name <span className="text-danger-red">*</span>
                      </label>
                      <input
                        className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                        placeholder="e.g. CÔNG TY TNHH ACME"
                        value={businessLegalName}
                        onChange={(e) => setBusinessLegalName(e.target.value)}
                      />
                    </div>
                    <div>
                      <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">
                        Tax ID <span className="text-danger-red">*</span>
                      </label>
                      <input
                        className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                        placeholder="e.g. 0312345678"
                        value={businessTaxId}
                        onChange={(e) => setBusinessTaxId(e.target.value)}
                      />
                      <p className="mt-1.5 text-label-xs text-on-surface-variant">
                        AISAM will verify this tax ID and legal name before opening Business checkout.
                      </p>
                    </div>
                  </div>
                </motion.div>
              )}

              {/* Plan category follows the active workspace type. */}
              <div className="flex items-center justify-center gap-2">
                <button
                  onClick={() => { if (activeWorkspaceCategory === "personal" && !createMode) setPlanCategory("personal"); }}
                  className={`px-5 py-2 rounded-lg text-label-sm font-semibold transition-all ${
                    planCategory === "personal"
                      ? "bg-primary text-on-primary shadow-sm shadow-primary/20"
                      : createMode || activeWorkspaceCategory === "business"
                      ? "bg-surface-container-lowest border border-outline-variant/20 text-outline/40 cursor-not-allowed"
                      : "bg-surface-container-lowest border border-outline-variant/20 text-outline hover:text-on-surface hover:border-primary/30"
                  }`}
                >
                  <span className="material-symbols-outlined text-[16px] align-middle mr-1.5">person</span>
                  Personal
                  {createMode && <span className="ml-1.5 text-label-2xs text-outline/40">· locked</span>}
                </button>
                <button
                  onClick={() => { if (activeWorkspaceCategory === "business" || createMode) setPlanCategory("business"); }}
                  className={`px-5 py-2 rounded-lg text-label-sm font-semibold transition-all ${
                    planCategory === "business"
                      ? "bg-primary text-on-primary shadow-sm shadow-primary/20"
                      : activeWorkspaceCategory === "personal" && !createMode
                      ? "bg-surface-container-lowest border border-outline-variant/20 text-outline/40 cursor-not-allowed"
                      : "bg-surface-container-lowest border border-outline-variant/20 text-outline hover:text-on-surface hover:border-primary/30"
                  }`}
                >
                  <span className="material-symbols-outlined text-[16px] align-middle mr-1.5">business</span>
                  Business
                  {activeWorkspaceCategory === "personal" && !createMode && <span className="ml-1.5 text-label-2xs text-outline/40">· locked</span>}
                </button>
              </div>

              {/* Billing Toggle */}
              <div className="flex items-center justify-center gap-3">
                <span className={`text-label-sm font-semibold ${!yearly ? "text-on-surface" : "text-outline"}`}>Monthly</span>
                <button
                  onClick={() => setYearly(!yearly)}
                  className={`relative w-12 h-6 rounded-full transition-colors ${yearly ? "bg-primary" : "bg-outline/30"}`}
                >
                  <div className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow-sm transition-transform ${yearly ? "translate-x-6" : "translate-x-0.5"}`} />
                </button>
                <div className="flex items-center gap-1">
                  <span className={`text-label-sm font-semibold ${yearly ? "text-on-surface" : "text-outline"}`}>Yearly</span>
                  <span className="px-1.5 py-0.5 rounded-md bg-emerald-100 text-emerald-700 text-label-2xs font-bold">Save ~17%</span>
                </div>
              </div>

              {/* Plans Grid */}
              <div className="flex flex-wrap justify-center gap-4">
                {!pricingLoaded ? (
                  Array.from({ length: planCategory === "business" ? 2 : 3 }).map((_, index) => (
                    <div
                      key={index}
                      className="w-[280px] sm:w-[260px] rounded-2xl border border-outline-variant/20 bg-surface-container-lowest p-5"
                    >
                      <div className="h-6 w-36 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-3 h-4 w-44 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-8 h-9 w-32 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-8 space-y-3">
                        {Array.from({ length: 5 }).map((__, lineIndex) => (
                          <div key={lineIndex} className="h-4 w-full rounded bg-surface-container-high animate-pulse" />
                        ))}
                      </div>
                      <div className="mt-8 h-11 w-full rounded-xl bg-surface-container-high animate-pulse" />
                    </div>
                  ))
                ) : filteredPlans.map((plan) => {
                  const current = !createMode && isCurrentPlan(plan.planType);
                  const lower = !createMode && isLowerPlan(plan.planType);

                  return (
                    <motion.div
                      key={plan.planType}
                      variants={item}
                      className={`relative rounded-2xl border p-5 flex flex-col w-[280px] sm:w-[260px] transition-all duration-300 hover:-translate-y-1 hover:shadow-xl ${
                        plan.popular
                          ? "border-primary shadow-lg shadow-primary/10 bg-gradient-to-b from-primary/5 to-transparent ring-1 ring-primary/20 hover:shadow-primary/20"
                          : current
                          ? "border-primary/30 bg-primary/5 hover:shadow-primary/10"
                          : "border-outline-variant/20 bg-surface-container-lowest hover:shadow-black/5"
                      }`}
                    >
                      {plan.popular && (
                        <div className="absolute -top-3 left-1/2 -translate-x-1/2 z-10">
                          <span className="px-3 py-1 bg-primary text-on-primary text-label-2xs font-bold rounded-full shadow-sm whitespace-nowrap">
                            Most Popular
                          </span>
                        </div>
                      )}

                      <div className="mb-4">
                        <h3 className="text-body-lg font-bold text-on-surface">{plan.name}</h3>
                        <p className="text-label-xs text-on-surface-variant mt-0.5">{plan.postQuota} · {plan.credits.toLocaleString()} credits</p>
                      </div>

                      <div className="mb-4">
                        <div className="flex items-baseline gap-1">
                          <span className="text-3xl font-bold text-on-surface">
                            {yearly ? `${(plan.price * 10).toLocaleString("vi-VN")}₫` : plan.priceFormatted}
                          </span>
                          <span className="text-label-sm text-outline">{yearly ? "/year" : plan.period}</span>
                        </div>
                        {yearly && plan.price > 0 && (
                          <>
                            <p className="text-label-xs text-emerald-600 mt-0.5">
                              {(plan.price * 10 / 12).toLocaleString("vi-VN", { maximumFractionDigits: 0 })}₫/mo billed yearly
                            </p>
                            <p className="text-label-xs text-emerald-600/80 mt-0.5">
                              {(plan.price * 2).toLocaleString("vi-VN")}₫ saved vs monthly
                            </p>
                          </>
                        )}
                      </div>

                      <ul className="space-y-2 mb-6 flex-1">
                        {plan.features.slice(0, 6).map((feature) => (
                          <li key={feature} className="flex items-start gap-2">
                            <span className="material-symbols-outlined text-emerald-500 text-[14px] mt-0.5 shrink-0">check_circle</span>
                            <span className="text-label-xs text-on-surface-variant leading-relaxed">{feature}</span>
                          </li>
                        ))}
                        {plan.features.length > 6 && (
                          <li className="text-label-xs text-outline pl-6">+{plan.features.length - 6} more features</li>
                        )}
                      </ul>

                      <button
                        onClick={() => handleUpgrade(plan)}
                        disabled={current || creating || processing === plan.planType}
                        className={`w-full py-2.5 rounded-xl text-label-sm font-semibold transition-all disabled:opacity-60 disabled:cursor-not-allowed ${
                          plan.popular
                            ? "bg-primary text-on-primary hover:bg-primary/90 shadow-sm shadow-primary/20"
                            : current
                            ? "bg-primary/10 text-primary border border-primary/20 cursor-default"
                            : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
                        }`}
                      >
                        {creating && plan.popular ? (
                          <span className="flex items-center justify-center gap-1.5">
                            <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                            </svg>
                            Creating workspace...
                          </span>
                        ) : processing === plan.planType ? (
                          <span className="flex items-center justify-center gap-1.5">
                            <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                            </svg>
                            Processing...
                          </span>
                        ) : createMode ? (
                          "Subscribe"
                        ) : current ? (
                          "Current Plan"
                        ) : lower ? (
                          "Downgrade"
                        ) : (
                          plan.cta
                        )}
                      </button>
                    </motion.div>
                  );
                })}
              </div>

            </motion.div>
          )}

          {/* Credit Packs */}
          {activeTab === "credits" && (
            <motion.div variants={container} initial="hidden" animate="show" className="space-y-6">
              {/* Wallet Balance */}
              {creditWallet && (
                <div className="bg-gradient-to-br from-emerald-50 to-emerald-50/50 rounded-2xl border border-emerald-200/30 p-5">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 rounded-2xl bg-emerald-100 flex items-center justify-center">
                        <span className="material-symbols-outlined text-emerald-600 text-[24px]">account_balance_wallet</span>
                      </div>
                      <div>
                        <p className="text-label-sm text-on-surface-variant">Current Balance</p>
                        <p className="text-2xl font-bold text-emerald-600">{creditWallet.balance.toLocaleString()} credits</p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-label-2xs text-outline">Maximum</p>
                      <p className="text-label-sm font-semibold text-on-surface">{creditWallet.maxBalance.toLocaleString()} credits</p>
                    </div>
                  </div>
                  <div className="mt-3 h-2 bg-emerald-100 rounded-full overflow-hidden">
                    <div className="h-full bg-gradient-to-r from-emerald-400 to-emerald-500 rounded-full transition-all duration-500"
                      style={{ width: `${(creditWallet.balance / creditWallet.maxBalance) * 100}%` }} />
                  </div>
                </div>
              )}

              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                {!pricingLoaded ? (
                  Array.from({ length: 4 }).map((_, index) => (
                    <div
                      key={index}
                      className="rounded-2xl border border-outline-variant/20 bg-surface-container-lowest p-5"
                    >
                      <div className="h-6 w-28 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-6 h-8 w-24 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-4 h-4 w-32 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-5 h-7 w-24 rounded bg-surface-container-high animate-pulse" />
                      <div className="mt-6 h-10 w-full rounded-xl bg-surface-container-high animate-pulse" />
                    </div>
                  ))
                ) : displayPacks.map((pack) => {
                  const pricePerCredit = (pack.price / pack.credits).toFixed(0);
                  return (
                    <div
                      key={pack.id}
                      className={`relative rounded-2xl border p-5 ${
                        pack.popular
                          ? "border-primary shadow-md shadow-primary/10 bg-gradient-to-b from-primary/5 to-transparent"
                          : "border-outline-variant/20 bg-surface-container-lowest"
                      }`}
                    >
                      {pack.popular && (
                        <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                          <span className="px-3 py-1 bg-primary text-on-primary text-label-xs font-bold rounded-full shadow-sm">
                            Best Value
                          </span>
                        </div>
                      )}
                      <div className="flex items-center gap-2 mb-3">
                        <span className={`material-symbols-outlined text-[22px] ${pack.popular ? "text-primary" : "text-outline"}`}>{pack.icon}</span>
                        <h4 className="text-body-md font-bold text-on-surface">{pack.name}</h4>
                      </div>
                      <div className="mb-2">
                        <span className="text-2xl font-bold text-on-surface">{pack.credits.toLocaleString()}</span>
                        <span className="text-label-sm text-outline ml-1">Credits</span>
                      </div>
                      <p className="text-label-xs text-outline mb-3">~{pricePerCredit}₫ per credit</p>
                      <p className="text-body-lg font-semibold text-primary mb-4">{pack.priceFormatted}</p>
                      <button
                        onClick={() => handleBuyCreditPack(pack)}
                        disabled={processing === -1}
                        className={`w-full py-2.5 rounded-xl text-label-sm font-semibold transition-all disabled:opacity-60 ${
                          pack.popular
                            ? "bg-primary text-on-primary hover:bg-primary/90 shadow-sm shadow-primary/20"
                            : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
                        }`}
                      >
                        {processing === -1 && selectedPack?.id === pack.id ? (
                          <span className="flex items-center justify-center gap-1.5">
                            <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                            </svg>
                            Processing...
                          </span>
                        ) : (
                          "Buy Now"
                        )}
                      </button>
                    </div>
                  );
                })}
              </div>

              <div className="text-center">
                <p className="text-label-sm text-outline">
                  Credits never expire · Can be combined with subscription credits · <span className="text-primary font-semibold">PayOS secure payment</span>
                </p>
              </div>
            </motion.div>
          )}

          {/* Feature Comparison Table */}
          {activeTab === "subscription" && (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
              <h2 className="text-headline-sm font-bold text-on-surface mb-4">Full Feature Comparison</h2>
              <div className="overflow-x-auto">
                <table className="w-full text-left">
                  <thead>
                    <tr className="border-b border-outline-variant/20">
                      <th className="py-3 pr-4 text-label-sm font-semibold text-outline">Feature</th>
                      {filteredPlans.map(p => (
                        <th key={p.planType} className="py-3 px-3 text-label-sm font-semibold text-outline text-center min-w-[100px]">
                          <div className={`px-2 py-1 rounded-lg ${!createMode && isCurrentPlan(p.planType) ? "bg-primary/10 text-primary" : ""}`}>
                            {p.name}
                          </div>
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {[
                      { label: "Generate Text", key: true },
                      { label: "Manual Post", key: true },
                      { label: "Basic Analytics", key: true },
                      { label: "AI Image Generation", key: PlanType.PersonalPlus },
                      { label: "Content Calendar", key: PlanType.PersonalPlus },
                      { label: "Schedule Post", key: PlanType.PersonalPlus },
                      { label: "Multi Platform Publish", key: PlanType.PersonalPlus },
                      { label: "Trend Analysis", key: PlanType.PersonalPro },
                      { label: "Holiday Suggestion", key: PlanType.PersonalPro },
                      { label: "AI Video Generation", key: PlanType.PersonalPro },
                      { label: "Advanced Analytics", key: PlanType.PersonalPro },
                      { label: "Campaign Recommendation", key: PlanType.PersonalPro },
                      { label: "Team Management", key: PlanType.BusinessPlus },
                      { label: "Shared Credits Pool", key: PlanType.BusinessPlus },
                      { label: "Workspace Dashboard", key: PlanType.BusinessPlus },
                      { label: "Lifetime Assigned Limit", key: PlanType.BusinessPro },
                      { label: "Monthly Assigned Limit", key: PlanType.BusinessPro },
                      { label: "Credit Usage Report", key: PlanType.BusinessPro },
                    ].map((row) => (
                      <tr key={row.label} className="border-b border-outline-variant/10 hover:bg-surface-container/30 transition-colors">
                        <td className="py-2.5 pr-4 text-label-sm text-on-surface">{row.label}</td>
                        {filteredPlans.map(p => {
                          const available = row.key === true || (typeof row.key === "number" && p.planType >= row.key);
                          return (
                            <td key={p.planType} className="py-2.5 px-3 text-center">
                              {available ? (
                                <span className="material-symbols-outlined text-emerald-500 text-[18px]">check</span>
                              ) : (
                                <span className="material-symbols-outlined text-outline/30 text-[18px]">remove</span>
                              )}
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* AISAM Checkout Modal */}
      {showQRModal && qrData && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-4 backdrop-blur-md">
          <motion.div
            initial={reduceMotion ? false : { opacity: 0, scale: 0.96, y: 16 }}
            animate={reduceMotion ? undefined : { opacity: 1, scale: 1, y: 0 }}
            className="relative w-full max-w-[520px] overflow-hidden rounded-[32px] border border-outline-variant/25 bg-surface-container-lowest shadow-2xl"
          >
            <div className="absolute -right-16 -top-16 h-44 w-44 rounded-full bg-primary/15 blur-3xl" />
            <div className="absolute -left-20 bottom-0 h-44 w-44 rounded-full bg-secondary/10 blur-3xl" />

            <div className="relative p-6 sm:p-7">
              <div className="mb-6 flex items-start justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary text-on-primary shadow-lg shadow-primary/20">
                    <span className="material-symbols-outlined text-[30px]">
                      {qrData.type === "credits" ? "toll" : qrData.type === "workspace" ? "domain_add" : "workspace_premium"}
                    </span>
                  </div>
                  <div>
                    <p className="text-label-sm font-semibold uppercase tracking-[0.22em] text-primary">AISAM Checkout</p>
                    <h3 className="text-headline-sm font-bold text-on-surface">Review your payment</h3>
                  </div>
                </div>
                <button
                  onClick={handlePaymentExit}
                  className="flex h-10 w-10 items-center justify-center rounded-full text-on-surface-variant transition hover:bg-surface-container-high hover:text-on-surface"
                  aria-label="Close checkout"
                >
                  <span className="material-symbols-outlined">close</span>
                </button>
              </div>

              <div className="mb-5 rounded-3xl border border-outline-variant/30 bg-surface-container/60 p-4">
                <div className="mb-4 flex items-start justify-between gap-4">
                  <div>
                    <p className="text-label-sm text-on-surface-variant">Selected item</p>
                    <p className="mt-1 text-title-md font-bold text-on-surface">{qrData.description}</p>
                    <p className="mt-1 text-body-sm text-on-surface-variant">{qrData.summary}</p>
                  </div>
                  <div className="rounded-2xl bg-primary/10 px-3 py-2 text-right">
                    <p className="text-label-sm text-primary">Amount</p>
                    <p className="text-title-md font-bold text-primary">{formatCurrency(qrData.amount)}</p>
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-2">
                  {[
                    ["lock", "Secure"],
                    ["qr_code_2", "VietQR"],
                    ["verified", "PayOS"],
                  ].map(([icon, label]) => (
                    <div key={label} className="flex flex-col items-center gap-1 rounded-2xl bg-surface-container-lowest/80 px-3 py-3 text-center">
                      <span className="material-symbols-outlined text-primary text-[20px]">{icon}</span>
                      <span className="text-label-sm font-semibold text-on-surface-variant">{label}</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="mb-6 rounded-2xl border border-blue-200/60 bg-blue-50 px-4 py-3 text-body-sm text-blue-800">
                Bạn sẽ được chuyển sang PayOS để quét QR hoặc chuyển khoản. Sau khi thanh toán thành công,
                AISAM sẽ tự đồng bộ gói/credit khi bạn quay lại hệ thống.
              </div>

              <div className="flex flex-col-reverse gap-3 sm:flex-row">
                <button
                  onClick={handlePaymentExit}
                  className="flex-1 rounded-2xl border border-outline-variant/40 px-5 py-3 text-label-md font-semibold text-on-surface transition hover:bg-surface-container-high"
                >
                  Hủy
                </button>
                <button
                  onClick={() => {
                    const activeReference = qrData.type === "workspace"
                      ? window.sessionStorage.getItem(CREATED_WORKSPACE_PAYMENT_KEY) || ""
                      : window.sessionStorage.getItem(PRICING_PAYMENT_REFERENCE_KEY) || "";
                    if (activeReference) window.sessionStorage.setItem(PRICING_PAYMENT_ACTIVE_KEY, activeReference);
                    window.location.href = qrData.checkoutUrl;
                  }}
                  className="flex flex-1 items-center justify-center gap-2 rounded-2xl bg-primary px-5 py-3 text-label-md font-bold text-on-primary shadow-lg shadow-primary/25 transition hover:opacity-90"
                >
                  <span className="material-symbols-outlined text-[20px]">open_in_new</span>
                  Thanh toán với PayOS
                </button>
              </div>
            </div>
          </motion.div>
        </div>
      )}

      {/* Legacy mock QR Payment Modal */}
      {false && showQRModal && qrData && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
          <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6">
            {qrStatus === "pending" && (
              <>
                <div className="text-center mb-6">
                  <div className="w-16 h-16 rounded-2xl bg-primary/10 flex items-center justify-center mx-auto mb-4">
                    <span className="material-symbols-outlined text-primary text-[32px]">qr_code_scanner</span>
                  </div>
                  <h3 className="text-body-lg font-bold text-on-surface">Scan to Pay</h3>
                  <p className="text-label-sm text-on-surface-variant mt-1">Use your banking app to scan the QR code</p>
                </div>

                {/* QR Code */}
                <div className="flex justify-center mb-6">
                  <div className="w-64 h-64 rounded-2xl bg-white border-2 border-outline-variant/20 p-4 shadow-inner flex items-center justify-center">
                    <div className="w-full h-full rounded-xl bg-gradient-to-br from-gray-50 to-gray-100 flex flex-col items-center justify-center relative">
                      <div className="absolute inset-4 flex flex-wrap gap-[3px] opacity-30">
                        {Array.from({ length: 35 }).map((_, i) => (
                          <div key={i} className={`w-[calc(14.28%-3px)] aspect-square rounded-sm ${[1, 2, 4, 7, 9, 12, 15, 18, 22, 25, 28, 31, 34].includes(i % 35) ? "bg-gray-800" : "bg-transparent"}`} />
                        ))}
                      </div>
                      <div className="relative z-10 bg-white rounded-xl px-4 py-2 shadow-sm border border-gray-200">
                        <span className="text-label-sm font-bold text-primary">PayOS</span>
                      </div>
                      <div className="absolute top-3 left-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                      <div className="absolute top-3 right-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                      <div className="absolute bottom-3 left-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                    </div>
                  </div>
                </div>

                {/* Order Info */}
                <div className="space-y-2 mb-6">
                  <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                    <span className="text-label-sm text-on-surface-variant">Order</span>
                    <span className="text-label-sm font-semibold text-on-surface">{qrData?.description}</span>
                  </div>
                  <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                    <span className="text-label-sm text-on-surface-variant">Amount</span>
                    <span className="text-label-sm font-bold text-primary">{qrData?.amount.toLocaleString()}₫</span>
                  </div>
                  <div className="flex items-center justify-center gap-2 p-3 rounded-xl bg-amber-50 border border-amber-200/30">
                    <span className="w-2 h-2 rounded-full bg-amber-400 animate-pulse" />
                    <span className="text-label-sm text-amber-700">Waiting for payment...</span>
                  </div>
                </div>

                <button onClick={handleMockPayment}
                  className="w-full py-3 rounded-xl text-label-sm font-semibold bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high transition-all mb-2">
                  Simulate Payment (Demo)
                </button>
                <button onClick={() => setShowQRModal(false)}
                  className="w-full py-3 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container/50 transition-all">
                  Cancel
                </button>
              </>
            )}

            {qrStatus === "completed" && (
              <div className="text-center py-8">
                <div className="w-20 h-20 rounded-full bg-emerald-100 flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-emerald-500 text-[40px]">check_circle</span>
                </div>
                <h3 className="text-headline-sm font-bold text-on-surface mb-2">Payment Successful!</h3>
                <p className="text-body-sm text-on-surface-variant mb-6">Your transaction has been completed.</p>
                <button onClick={() => setShowQRModal(false)}
                  className="px-8 py-3 rounded-xl text-label-sm font-semibold bg-primary text-on-primary hover:opacity-90 transition-all shadow-lg shadow-primary/20">
                  Done
                </button>
              </div>
            )}

            {qrStatus === "failed" && (
              <div className="text-center py-8">
                <div className="w-20 h-20 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                  <span className="material-symbols-outlined text-red-500 text-[40px]">error</span>
                </div>
                <h3 className="text-headline-sm font-bold text-on-surface mb-2">Payment Failed</h3>
                <p className="text-body-sm text-on-surface-variant mb-6">Please try again.</p>
                <button onClick={() => setShowQRModal(false)}
                  className="px-8 py-3 rounded-xl text-label-sm font-semibold bg-primary text-on-primary hover:opacity-90 transition-all shadow-lg shadow-primary/20">
                  Try Again
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </main>
  );
}

export default function PricingPage() {
  return (
    <Suspense fallback={null}>
      <PricingContent />
    </Suspense>
  );
}
