"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchCreditWallet, type CreditWallet } from "@/services/workspaceService";
import { createPayment, checkPaymentStatus, type PayOSPaymentResponse } from "@/services/paymentService";

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

const CREDIT_PACKS: CreditPack[] = [
  {
    id: "starter",
    name: "Starter",
    credits: 100,
    price: 29000,
    priceFormatted: "29,000₫",
    icon: "bolt",
    description: "Perfect for trying out AI features",
  },
  {
    id: "standard",
    name: "Standard",
    credits: 500,
    price: 99000,
    priceFormatted: "99,000₫",
    icon: "electric_bolt",
    popular: true,
    description: "Best value for regular creators",
  },
  {
    id: "growth",
    name: "Growth",
    credits: 1500,
    price: 249000,
    priceFormatted: "249,000₫",
    icon: "local_fire_department",
    description: "For growing businesses",
  },
  {
    id: "business",
    name: "Business",
    credits: 5000,
    price: 699000,
    priceFormatted: "699,000₫",
    icon: "whatshot",
    description: "Maximum credits for teams",
  },
];

export default function CreditPackPage() {
  const router = useRouter();
  const { activeWorkspace } = useWorkspaces();
  const [creditWallet, setCreditWallet] = useState<CreditWallet | null>(null);
  const [selectedPack, setSelectedPack] = useState<CreditPack | null>(null);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [purchasing, setPurchasing] = useState(false);
  const [purchaseSuccess, setPurchaseSuccess] = useState(false);

  // Payment QR flow
  const [showPaymentQR, setShowPaymentQR] = useState(false);
  const [paymentData, setPaymentData] = useState<PayOSPaymentResponse | null>(null);
  const [paymentStatus, setPaymentStatus] = useState<"pending" | "completed" | "failed">("pending");
  const [paymentError, setPaymentError] = useState("");
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    const loadWallet = async () => {
      const wallet = await fetchCreditWallet();
      setCreditWallet(wallet);
    };
    loadWallet();
  }, [activeWorkspace?.id]);

  useEffect(() => {
    return () => {
      if (pollRef.current) clearInterval(pollRef.current);
    };
  }, []);

  const handleSelectPack = (pack: CreditPack) => {
    setSelectedPack(pack);
    setShowConfirmDialog(true);
  };

  const handleConfirmPurchase = async () => {
    if (!selectedPack) return;
    setPurchasing(true);

    try {
      const payment = await createPayment({
        packName: selectedPack.name,
        credits: selectedPack.credits,
        amount: selectedPack.price,
        returnUrl: window.location.origin + "/credit-pack?payment=success",
        cancelUrl: window.location.origin + "/credit-pack?payment=cancelled",
        paymentType: "CreditPack",
      });

      if (payment) {
        setPaymentData(payment);
        setShowPaymentQR(true);
        setShowConfirmDialog(false);
        setPaymentStatus("pending");

        // Poll for payment status every 3 seconds (mock)
        let attempts = 0;
        pollRef.current = setInterval(async () => {
          attempts++;
          const status = await checkPaymentStatus(payment.orderId);
          if (status?.status === "completed") {
            if (pollRef.current) clearInterval(pollRef.current);
            setPaymentStatus("completed");
            if (creditWallet) {
              setCreditWallet({
                ...creditWallet,
                balance: creditWallet.balance + selectedPack.credits,
              });
            }
            setPurchaseSuccess(true);
            setTimeout(() => {
              setShowPaymentQR(false);
              setPurchaseSuccess(false);
            }, 3000);
          } else if (status?.status === "failed" || attempts > 20) {
            if (pollRef.current) clearInterval(pollRef.current);
            setPaymentStatus("failed");
            setPaymentError("Payment was not completed. Please try again.");
          }
        }, 3000);
      } else {
        setPaymentError("Failed to create payment. Please try again.");
      }
    } catch {
      setPaymentError("Network error. Please try again.");
    } finally {
      setPurchasing(false);
    }
  };

  const handleCloseQR = () => {
    if (pollRef.current) clearInterval(pollRef.current);
    setShowPaymentQR(false);
    setPaymentData(null);
    setPaymentStatus("pending");
    setPaymentError("");
  };

  const handleMockPayment = async () => {
    if (!paymentData) return;
    setPaymentStatus("completed");
    if (pollRef.current) clearInterval(pollRef.current);
    if (creditWallet && selectedPack) {
      setCreditWallet({
        ...creditWallet,
        balance: creditWallet.balance + selectedPack.credits,
      });
    }
    setPurchaseSuccess(true);
    setTimeout(() => {
      setShowPaymentQR(false);
      setPurchaseSuccess(false);
    }, 3000);
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
                  {creditWallet?.maxBalance.toLocaleString() || 15000} credits
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
              {CREDIT_PACKS.map((pack) => (
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

        {/* QR Payment Modal */}
        {showPaymentQR && paymentData && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6">
              {paymentStatus === "pending" && (
                <>
                  <div className="text-center mb-6">
                    <div className="w-16 h-16 rounded-2xl bg-primary/10 flex items-center justify-center mx-auto mb-4">
                      <span className="material-symbols-outlined text-primary text-[32px]">qr_code_scanner</span>
                    </div>
                    <h3 className="text-body-lg font-bold text-on-surface">Scan to Pay</h3>
                    <p className="text-label-sm text-on-surface-variant mt-1">
                      Use your banking app to scan the QR code
                    </p>
                  </div>

                  {/* QR Code Display */}
                  <div className="flex justify-center mb-6">
                    <div className="w-64 h-64 rounded-2xl bg-white border-2 border-outline-variant/20 p-4 shadow-inner flex items-center justify-center">
                      <div className="w-full h-full rounded-xl bg-gradient-to-br from-gray-50 to-gray-100 flex flex-col items-center justify-center relative">
                        {/* Mock QR Code pattern */}
                        <div className="absolute inset-4 flex flex-wrap gap-[3px] opacity-30">
                          {Array.from({ length: 35 }).map((_, i) => (
                            <div
                              key={i}
                              className={`w-[calc(14.28%-3px)] aspect-square rounded-sm ${
                                [1, 2, 4, 7, 9, 12, 15, 18, 22, 25, 28, 31, 34].includes(i % 35)
                                  ? "bg-gray-800" : "bg-transparent"
                              }`}
                            />
                          ))}
                        </div>
                        {/* PayOS Logo placeholder */}
                        <div className="relative z-10 bg-white rounded-xl px-4 py-2 shadow-sm border border-gray-200">
                          <span className="text-label-sm font-bold text-primary">PayOS</span>
                        </div>
                        {/* Corner squares for QR feel */}
                        <div className="absolute top-3 left-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                        <div className="absolute top-3 right-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                        <div className="absolute bottom-3 left-3 w-12 h-12 border-2 border-gray-800 rounded-lg" />
                      </div>
                    </div>
                  </div>

                  {/* Order Info */}
                  <div className="space-y-3 mb-6">
                    <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                      <span className="text-label-sm text-on-surface-variant">Order ID</span>
                      <span className="text-label-sm font-mono font-semibold text-on-surface">{paymentData.orderId}</span>
                    </div>
                    <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                      <span className="text-label-sm text-on-surface-variant">Amount</span>
                      <span className="text-label-sm font-bold text-primary">
                        {(paymentData.amount || 0).toLocaleString()}₫
                      </span>
                    </div>
                    <div className="flex items-center justify-between p-3 rounded-xl bg-amber-50 border border-amber-200/30">
                      <span className="flex items-center gap-1.5 text-label-sm text-amber-700">
                        <span className="material-symbols-outlined text-[14px]">hourglass_empty</span>
                        Waiting for payment...
                      </span>
                      <span className="w-2 h-2 rounded-full bg-amber-400 animate-pulse" />
                    </div>
                  </div>

                  {/* Payment Methods Info */}
                  <div className="text-center mb-6">
                    <p className="text-label-xs text-outline mb-3">Supported payment methods</p>
                    <div className="flex items-center justify-center gap-4">
                      <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-surface-container">
                        <span className="material-symbols-outlined text-[14px] text-blue-600">account_balance</span>
                        <span className="text-label-xs font-semibold">Bank Transfer</span>
                      </div>
                      <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-surface-container">
                        <span className="material-symbols-outlined text-[14px] text-green-600">credit_card</span>
                        <span className="text-label-xs font-semibold">Credit Card</span>
                      </div>
                      <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-surface-container">
                        <span className="material-symbols-outlined text-[14px] text-purple-600">smartphone</span>
                        <span className="text-label-xs font-semibold">Mobile Banking</span>
                      </div>
                    </div>
                  </div>

                  {/* Mock: Simulate payment button (for demo) */}
                  <button
                    onClick={handleMockPayment}
                    className="w-full py-3 rounded-xl text-body-sm font-semibold bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high transition-all mb-2"
                  >
                    Simulate Payment (Demo)
                  </button>

                  <button
                    onClick={handleCloseQR}
                    className="w-full py-3 rounded-xl text-body-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container/50 transition-all"
                  >
                    Cancel Payment
                  </button>
                </>
              )}

              {paymentStatus === "completed" && (
                <div className="text-center py-8">
                  <div className="w-20 h-20 rounded-full bg-emerald-100 flex items-center justify-center mx-auto mb-4">
                    <span className="material-symbols-outlined text-emerald-500 text-[40px]">check_circle</span>
                  </div>
                  <h3 className="text-headline-sm font-bold text-on-surface mb-2">Payment Successful!</h3>
                  <p className="text-body-sm text-on-surface-variant mb-6">
                    Credits have been added to your workspace.
                  </p>
                  <button
                    onClick={handleCloseQR}
                    className="px-8 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:opacity-90 transition-all shadow-lg shadow-primary/20"
                  >
                    Done
                  </button>
                </div>
              )}

              {paymentStatus === "failed" && (
                <div className="text-center py-8">
                  <div className="w-20 h-20 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-4">
                    <span className="material-symbols-outlined text-red-500 text-[40px]">error</span>
                  </div>
                  <h3 className="text-headline-sm font-bold text-on-surface mb-2">Payment Failed</h3>
                  <p className="text-body-sm text-on-surface-variant mb-6">
                    {paymentError || "Payment was not completed. Please try again."}
                  </p>
                  <button
                    onClick={handleCloseQR}
                    className="px-8 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:opacity-90 transition-all shadow-lg shadow-primary/20"
                  >
                    Try Again
                  </button>
                </div>
              )}
            </div>
          </div>
        )}
      </main>
    </>
  );
}
