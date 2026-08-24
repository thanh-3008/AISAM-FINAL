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

export interface CreateBusinessWorkspaceCheckoutRequest {
  workspaceName: string;
  taxId: string;
  legalBusinessName: string;
  planCode: string;
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

export const CREDIT_PACK_CODES_BY_ID: Record<string, number> = {
  starter: 1,
  standard: 2,
  growth: 3,
  business: 4,
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

export async function createBusinessWorkspacePayment(
  data: CreateBusinessWorkspaceCheckoutRequest,
): Promise<CheckoutResponse | null> {
  const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/business-workspace-checkout", {
    data,
    method: "POST",
  });
  return res?.data ?? null;
}

export async function synchronizeBusinessWorkspacePayment(reference: string): Promise<boolean> {
  const res: GenericResponse<boolean> = await apiClient("/payment/business-workspace-checkout/sync", {
    data: { reference },
    method: "POST",
  });
  return res?.success === true && res.data === true;
}

export async function syncPayOSCallback(searchParams: URLSearchParams): Promise<boolean> {
  try {
    const PAYOS_PARAMS = ["id", "orderCode", "amount", "description", "cancelUrl", "returnUrl", "status", "code", "cancel", "signature"];
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

function readPricingList(data: any, key: "plans" | "creditPacks") {
  const pascalKey = key === "plans" ? "Plans" : "CreditPacks";
  return data?.data?.[key] ?? data?.data?.[pascalKey] ?? [];
}

const PRICING_CACHE_KEY = "aisam_pricing_cache";
const PRICING_CACHE_TTL = 5 * 60 * 1000;

async function fetchWithRetry(url: string, retries = 3, delay = 1000): Promise<Response> {
  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, { cache: "no-store" });
      if (res.ok) return res;
      if (res.status >= 500 && i < retries - 1) {
        await new Promise(r => setTimeout(r, delay * Math.pow(2, i)));
        continue;
      }
      return res;
    } catch (error) {
      if (i === retries - 1) throw error;
      await new Promise(r => setTimeout(r, delay * Math.pow(2, i)));
    }
  }
  throw new Error("Max retries exceeded");
}

export async function fetchPublicPricing(): Promise<{ plans: any[], creditPacks: any[] } | null> {
  if (typeof window !== "undefined") {
    const cached = localStorage.getItem(PRICING_CACHE_KEY);
    if (cached) {
      try {
        const { data, timestamp } = JSON.parse(cached);
        if (Date.now() - timestamp < PRICING_CACHE_TTL) {
          return data;
        }
      } catch {}
    }
  }

  try {
    const [plansRes, creditPacksRes] = await Promise.all([
      fetchWithRetry(`${API_URL}/pricing/plans?t=${Date.now()}`),
      fetchWithRetry(`${API_URL}/pricing/credit-packs?t=${Date.now()}`)
    ]);
    if (!plansRes.ok || !creditPacksRes.ok) {
      throw new Error("Pricing endpoint returned an error.");
    }
    const plansData = await plansRes.json();
    const creditPacksData = await creditPacksRes.json();
    const result = {
      plans: readPricingList(plansData, "plans"),
      creditPacks: readPricingList(creditPacksData, "creditPacks")
    };

    if (typeof window !== "undefined") {
      localStorage.setItem(PRICING_CACHE_KEY, JSON.stringify({ data: result, timestamp: Date.now() }));
    }

    return result;
  } catch (error) {
    console.error("fetchPublicPricing failed", error);
    return null;
  }
}
