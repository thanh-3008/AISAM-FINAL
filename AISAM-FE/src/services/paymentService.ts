import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
}

export interface PayOSPaymentResponse {
  orderId: string;
  checkoutUrl: string;
  qrCode?: string;
  status: "pending" | "completed" | "failed" | "cancelled";
  amount: number;
  description: string;
}

export interface CreatePaymentRequest {
  planType?: number;
  packName?: string;
  credits?: number;
  amount: number;
  returnUrl: string;
  cancelUrl: string;
  paymentType: "Subscription" | "CreditPack";
}

export async function createPayment(data: CreatePaymentRequest): Promise<PayOSPaymentResponse | null> {
  const planCodes = ["Free", "Plus", "Premium", "Plus", "Premium"];
  const packCodes: Record<string, number> = { Starter: 1, Standard: 2, Growth: 3, Business: 4 };
  const res: GenericResponse<{ checkoutUrl: string; paymentLinkId?: string | null; orderCode?: string | null }> =
    await apiClient("/payment/checkout", {
      data: {
        paymentType: data.paymentType === "Subscription" ? 1 : 2,
        planCode: data.paymentType === "Subscription" ? planCodes[data.planType ?? 0] ?? "Free" : "",
        creditPackCode: data.paymentType === "CreditPack" && data.packName ? packCodes[data.packName] : null,
        returnUrl: data.returnUrl,
        cancelUrl: data.cancelUrl,
      },
      method: "POST",
    });

  if (!res?.data?.checkoutUrl) return null;
  return {
    orderId: res.data.orderCode ?? res.data.paymentLinkId ?? "",
    checkoutUrl: res.data.checkoutUrl,
    status: "pending",
    amount: data.amount,
    description: data.paymentType === "Subscription"
      ? `Upgrade to ${["Free", "Personal Plus", "Personal Pro", "Business Plus", "Business Pro"][data.planType ?? 0]} plan`
      : `Credit Pack: ${data.packName} - ${data.credits} credits`,
  };
}

export async function checkPaymentStatus(_orderId: string): Promise<PayOSPaymentResponse | null> {
  void _orderId;
  return null;
}

export async function syncPaymentReturn(params: URLSearchParams): Promise<boolean> {
  const query = params.toString();
  const res: GenericResponse<boolean> = await apiClient(`/payment/return-sync${query ? `?${query}` : ""}`, {
    method: "POST",
  });
  return Boolean(res.success && res.data !== false);
}
