import { apiClient, API_URL } from "@/lib/apiClient";

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
  3: "PlusTrial",
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
  } catch {
    return null;
  }
}

export async function syncPayOSCallback(searchParams: URLSearchParams): Promise<boolean> {
  try {
    const PAYOS_PARAMS = ["id", "orderCode", "amount", "description", "cancelUrl", "returnUrl", "status", "signature"];
    const params = new URLSearchParams();
    for (const [key, value] of searchParams.entries()) {
      if (PAYOS_PARAMS.includes(key)) {
        params.append(key, value);
      }
    }
    const query = params.toString();
    if (!query) return false;
    const res = await fetch(`${API_URL}/payment/callback${query ? "?" + query : ""}`, {
      method: "POST",
    });
    const data = await res.json();
    return data?.success === true;
  } catch {
    return false;
  }
}
