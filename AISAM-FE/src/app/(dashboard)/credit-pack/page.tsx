"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchCreditWallet, type CreditWallet } from "@/services/workspaceService";
import { createPayment, CREDIT_PACK_CODES_BY_ID, fetchPublicPricing, syncPayOSCallback } from "@/services/paymentService";

interface CreditPack {
  id: string;
  name: string;
  credits: number;
  price: number;
  priceFormatted: string;
  icon: string;
  popular?: boolean;
  description: string;
}

const DEFAULT_CREDIT_PACKS: CreditPack[] = [
  {
    id: "starter",
    name: "Starter",
    credits: 100,
    price: 2000,
    priceFormatted: "2,000₫",
    icon: "bolt",
    description: "Perfect for trying out AI features",
  },
  {
    id: "standard",
    name: "Standard",
    credits: 500,
    price: 3000,
    priceFormatted: "3,000₫",
    icon: "electric_bolt",
    popular: true,
    description: "Best value for regular creators",
  },
  {
    id: "growth",
    name: "Growth",
    credits: 1500,
    price: 4000,
    priceFormatted: "4,000₫",
    icon: "local_fire_department",
    description: "For growing businesses",
  },
  {
    id: "business",
    name: "Business",
    credits: 5000,
    price: 5000,
    priceFormatted: "5,000₫",
    icon: "whatshot",
    description: "Maximum credits for teams",
  },
];

export default function CreditPackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { activeWorkspace } = useWorkspaces();
  const [creditWallet, setCreditWallet] = useState<CreditWallet | null>(null);
  const [creditPacks, setCreditPacks] = useState<CreditPack[]>(DEFAULT_CREDIT_PACKS);
  const [selectedPack, setSelectedPack] = useState<CreditPack | null>(null);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [purchasing, setPurchasing] = useState(false);
  const [syncingPayment, setSyncingPayment] = useState(false);
  const [purchaseSuccess, setPurchaseSuccess] = useState(false);

  const [paymentError, setPaymentError] = useState("");
  const paymentSyncStartedRef = useRef(false);

  function formatCurrency(amount: number) {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
      maximumFractionDigits: 0,
    }).format(amount);
  }

  useEffect(() => {
    const loadWallet = async () => {
      const wallet = await fetchCreditWallet();
      setCreditWallet(wallet);
    };
    loadWallet();
  }, [activeWorkspace?.id]);

  useEffect(() => {
    const hasPayOSRedirect = searchParams.has("orderCode") || searchParams.has("id");
    if (!hasPayOSRedirect || !activeWorkspace?.id || paymentSyncStartedRef.current) return;

    const redirectStatus = searchParams.get("status")?.toUpperCase();
    const redirectPaid = searchParams.get("cancel") !== "true" &&
      (redirectStatus === "PAID" || redirectStatus === "SUCCESS" || redirectStatus === "COMPLETED" || searchParams.get("code") === "00");

    paymentSyncStartedRef.current = true;
    setSyncingPayment(true);

    const synchronizePayment = async () => {
      try {
        if (!redirectPaid) {
          setPaymentError("Payment was not completed.");
          router.replace("/credit-pack?payment=cancelled");
          return;
        }

        const synced = await syncPayOSCallback(searchParams);
        if (!synced) {
          paymentSyncStartedRef.current = false;
          setPaymentError("Payment was received, but credits could not be synchronized yet. Please refresh or contact support.");
          return;
        }

        const wallet = await fetchCreditWallet();
        setCreditWallet(wallet);
        setPurchaseSuccess(true);
        setShowConfirmDialog(false);
        setSelectedPack(null);
        router.replace("/credit-pack?payment=success");
        setTimeout(() => setPurchaseSuccess(false), 4000);
      } catch {
        paymentSyncStartedRef.current = false;
        setPaymentError("Payment sync failed. Please try refreshing the page.");
      } finally {
        setSyncingPayment(false);
      }
    };

    synchronizePayment();
  }, [activeWorkspace?.id, router, searchParams]);

  useEffect(() => {
    fetchPublicPricing().then((res) => {
      if (res?.creditPacks?.length) {
        setCreditPacks((prev) =>
          prev.map((pack) => {
            const matched = res.creditPacks.find(
              (bp) =>
                String(bp.id ?? "").toLowerCase() === pack.id.toLowerCase() ||
                String(bp.name ?? "").toLowerCase() === pack.name.toLowerCase()
            );
            if (matched) {
              return {
                ...pack,
                price: Number(matched.price ?? pack.price),
                priceFormatted: `${Number(matched.price ?? pack.price).toLocaleString()}₫`,
                credits: Number(matched.credits ?? pack.credits),
              };
            }
            return pack;
          })
        );
      }
    });
  }, []);

  const handleSelectPack = (pack: CreditPack) => {
    if (creditWallet && creditWallet.balance + pack.credits > creditWallet.maxBalance) {
      setPaymentError(`Cannot purchase: balance would exceed the maximum of ${creditWallet.maxBalance.toLocaleString()} credits.`);
      setTimeout(() => setPaymentError(""), 4000);
      return;
    }
    setSelectedPack(pack);
    setShowConfirmDialog(true);
  };

  const handleConfirmPurchase = async () => {
    if (!selectedPack) return;
    setPurchasing(true);

    try {
      const creditPackCode = CREDIT_PACK_CODES_BY_ID[selectedPack.id] || 1;
      const payment = await createPayment({
        paymentType: 2,
        creditPackCode,
        returnUrl: window.location.origin + "/credit-pack?payment=success",
        cancelUrl: window.location.origin + "/credit-pack?payment=cancelled",
      });

      if (payment?.checkoutUrl) {
        window.location.href = payment.checkoutUrl;
      } else {
        setPaymentError("Failed to create payment. Please try again.");
      }
    } catch {
      setPaymentError("Network error. Please try again.");
    } finally {
      setPurchasing(false);
    }
  };

  const getPricePerCredit = (pack: CreditPack) => {
    return (pack.price / pack.credits).toFixed(0);
  };

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Buy Credits" },
      ]} />
      <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-5xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center gap-4">
            <span className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500/10 to-emerald-600/10 text-emerald-500 flex items-center justify-center">
              <span className="material-symbols-outlined text-[22px]">token</span>
            </span>
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">Buy Credits</h1>
              <p className="text-body-sm text-on-surface-variant">
                Purchase additional AI credits for your workspace
              </p>
            </div>
          </div>

          {/* Success Toast */}
          {purchaseSuccess && (
            <div className="fixed top-20 right-6 z-50 flex items-center gap-3 px-5 py-3 rounded-xl bg-emerald-600 text-white shadow-lg animate-in slide-in-from-right">
              <span className="material-symbols-outlined text-[20px]">check_circle</span>
              <span className="text-body-sm font-semibold">Credits added successfully!</span>
            </div>
          )}

          {syncingPayment && (
            <div className="fixed top-20 right-6 z-50 flex items-center gap-3 px-5 py-3 rounded-xl bg-primary text-white shadow-lg">
              <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              <span className="text-body-sm font-semibold">Syncing payment...</span>
            </div>
          )}

          {/* Current Balance */}
          <div className="bg-gradient-to-br from-emerald-50 to-emerald-50/50 rounded-2xl border border-emerald-200/30 p-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <div className="w-14 h-14 rounded-2xl bg-emerald-100 flex items-center justify-center">
                  <span className="material-symbols-outlined text-emerald-600 text-[28px]">account_balance_wallet</span>
                </div>
                <div>
                  <p className="text-body-sm text-on-surface-variant">Current Balance</p>
                  <p className="text-3xl font-bold text-emerald-600">
                    {creditWallet?.balance.toLocaleString() || 0} credits
                  </p>
                </div>
              </div>
              <div className="text-right">
                <p className="text-label-sm text-outline">Maximum</p>
                <p className="text-body-md font-semibold text-on-surface">
                  {creditWallet?.maxBalance.toLocaleString() || "—"} credits
                </p>
              </div>
            </div>
            <div className="mt-4 h-2 bg-emerald-100 rounded-full overflow-hidden">
              <div
                className="h-full bg-gradient-to-r from-emerald-400 to-emerald-500 rounded-full transition-all duration-500"
                style={{ width: `${creditWallet ? (creditWallet.balance / creditWallet.maxBalance) * 100 : 0}%` }}
              />
            </div>
          </div>

          {/* Credit Packs */}
          <div>
            <h2 className="text-headline-sm text-on-surface mb-4">Available Packs</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {creditPacks.map((pack) => (
                <div
                  key={pack.id}
                  className={`relative rounded-2xl border p-6 transition-all hover:shadow-lg ${
                    pack.popular
                      ? "border-primary shadow-md shadow-primary/10 bg-gradient-to-b from-primary/5 to-transparent"
                      : "border-outline-variant/20 bg-surface-container-lowest hover:border-primary/30"
                  }`}
                >
                  {pack.popular && (
                    <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                      <span className="px-3 py-1 bg-gradient-to-r from-primary to-secondary text-white text-label-xs font-bold rounded-full shadow-md">
                        Most Popular
                      </span>
                    </div>
                  )}

                  <div className="flex items-center gap-3 mb-4">
                    <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${
                      pack.popular ? "bg-primary/10" : "bg-surface-container"
                    }`}>
                      <span className={`material-symbols-outlined text-[24px] ${
                        pack.popular ? "text-primary" : "text-on-surface-variant"
                      }`}>
                        {pack.icon}
                      </span>
                    </div>
                    <div>
                      <h3 className="text-body-lg font-bold text-on-surface">{pack.name}</h3>
                      <p className="text-label-xs text-on-surface-variant">{pack.description}</p>
            </div>
            {creditWallet && creditWallet.balance === 0 && (
              <p className="mt-3 text-body-sm text-danger-red font-medium flex items-center gap-1">
                <span className="material-symbols-outlined text-[16px]">warning</span>
                No credits remaining. Purchase a pack below to continue using AI features.
              </p>
            )}
          </div>

                  <div className="mb-4">
                    <div className="flex items-baseline gap-1">
                      <span className="text-3xl font-bold text-on-surface">{pack.credits.toLocaleString()}</span>
                      <span className="text-label-sm text-outline">credits</span>
                    </div>
                    <p className="text-label-xs text-outline mt-1">
                      ~{getPricePerCredit(pack)}₫ per credit
                    </p>
                  </div>

                  <div className="mb-4 pt-4 border-t border-outline-variant/10">
                    <p className="text-2xl font-bold text-primary">{pack.priceFormatted}</p>
                  </div>

                  <button
                    onClick={() => handleSelectPack(pack)}
                    className={`w-full py-3 rounded-xl text-body-sm font-semibold transition-all ${
                      pack.popular
                        ? "bg-gradient-to-r from-primary to-secondary text-white hover:opacity-90 shadow-md shadow-primary/20"
                        : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
                    }`}
                  >
                    Purchase
                  </button>
                </div>
              ))}
            </div>
          </div>

          {/* Info Section */}
          <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-xl bg-primary/5 flex items-center justify-center shrink-0">
                <span className="material-symbols-outlined text-primary text-[20px]">info</span>
              </div>
              <div className="space-y-3">
                <h4 className="text-body-md font-semibold text-on-surface">About Credit Packs</h4>
                <ul className="space-y-2 text-body-sm text-on-surface-variant">
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-emerald-500">check_circle</span>
                    Credits never expire
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-emerald-500">check_circle</span>
                    Credits are added immediately after payment
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-emerald-500">check_circle</span>
                    Can be combined with subscription credits
                  </li>
                  <li className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px] text-emerald-500">check_circle</span>
                    Payment via PayOS (Vietnam banking, QR code, credit card)
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>

        {/* Confirm Purchase Dialog */}
        {showConfirmDialog && selectedPack && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
                  <span className="material-symbols-outlined text-primary text-[24px]">shopping_cart</span>
                </div>
                <div>
                  <h3 className="text-body-lg font-bold text-on-surface">Confirm Purchase</h3>
                  <p className="text-label-sm text-on-surface-variant">Review your order</p>
                </div>
              </div>

              <div className="space-y-4 mb-6">
                <div className="flex items-center justify-between p-4 rounded-xl bg-surface-container/50">
                  <div className="flex items-center gap-3">
                    <span className="material-symbols-outlined text-primary text-[20px]">{selectedPack.icon}</span>
                    <span className="text-body-sm font-semibold text-on-surface">{selectedPack.name} Pack</span>
                  </div>
                  <span className="text-body-sm font-bold text-on-surface">{selectedPack.credits.toLocaleString()} credits</span>
                </div>

                <div className="flex items-center justify-between p-4 rounded-xl bg-surface-container/50">
                  <span className="text-body-sm text-on-surface-variant">Current Balance</span>
                  <span className="text-body-sm font-semibold text-on-surface">
                    {creditWallet?.balance.toLocaleString() || 0} credits
                  </span>
                </div>

                <div className="flex items-center justify-between p-4 rounded-xl bg-emerald-50 border border-emerald-200/30">
                  <span className="text-body-sm font-semibold text-emerald-700">New Balance</span>
                  <span className="text-body-sm font-bold text-emerald-700">
                    {((creditWallet?.balance || 0) + selectedPack.credits).toLocaleString()} credits
                  </span>
                </div>

                <div className="pt-4 border-t border-outline-variant/20">
                  <div className="flex items-center justify-between">
                    <span className="text-body-md font-semibold text-on-surface">Total</span>
                    <span className="text-2xl font-bold text-primary">{selectedPack.priceFormatted}</span>
                  </div>
                  <p className="mt-2 text-label-xs text-amber-600 flex items-center gap-1">
                    <span className="material-symbols-outlined text-[14px]">warning</span>
                    Credits cannot be refunded.
                  </p>
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setShowConfirmDialog(false)}
                  disabled={purchasing}
                  className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={handleConfirmPurchase}
                  disabled={purchasing}
                  className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-gradient-to-r from-primary to-secondary text-white hover:opacity-90 transition-all disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {purchasing ? (
                    <>
                      <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                      </svg>
                      Processing...
                    </>
                  ) : (
                    "Confirm & Pay"
                  )}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Error display */}
        {paymentError && (
          <div className="mb-6 p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-body-sm">
            {paymentError}
          </div>
        )}
      </main>
    </>
  );
}
