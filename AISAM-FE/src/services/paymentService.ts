import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
}

/* ─── BE DTOs ─── */

/** Matches BE `PayOSCheckoutResponse` */
export interface CheckoutResponse {
  checkoutUrl: string;
  paymentLinkId?: string;
  orderCode?: string;
}

/** Maps to BE `CreateCheckoutRequest`:
 *  paymentType: 1=Subscription, 2=CreditPack
 *  planCode: "Free" | "Plus" | "Premium" | "PlusTrial"
 *  creditPackCode: 1=Starter, 2=Standard, 3=Growth, 4=Business
 */
export interface CreateCheckoutRequest {
  paymentType: 1 | 2;
  planCode?: string;
  creditPackCode?: number;
  returnUrl: string;
  cancelUrl: string;
}

/** Plan codes understood by BE SubscriptionPlanEnum */
export const PLAN_CODES: Record<number, string> = {
  0: "Free",
  1: "Plus",
  2: "Premium",
  3: "Plus",
  4: "Premium",
};

/** Credit‑pack codes understood by BE CreditPackCodeEnum */
export const CREDIT_PACK_CODES: Record<string, number> = {
  Starter: 1,
  Standard: 2,
  Growth: 3,
  Business: 4,
};

/* ─── API calls ─── */

export async function createPayment(data: CreateCheckoutRequest): Promise<CheckoutResponse | null> {
  try {
    const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/checkout", {
      data,
      method: "POST",
    });
    return res?.data ?? null;
  } catch (error) {
    console.error("createPayment failed", error);
    throw error;
  }
}

export async function checkPaymentStatus(_orderCode: string): Promise<CheckoutResponse | null> {
  // No dedicated BE endpoint; rely on PayOS webhook + callback
  return null;
}
