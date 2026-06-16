import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export async function changePassword(data: ChangePasswordRequest): Promise<boolean> {
  const res: GenericResponse<null> = await apiClient("/auth/change-password", {
    data,
    method: "POST",
  });
  return res?.success ?? false;
}

export interface PaymentHistoryItem {
  id: string;
  amount: number;
  currency?: string;
  status: string;
  paymentMethod: string;
  createdAt: string;
  description?: string;
}

export interface PaymentHistoryResponse {
  data: PaymentHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export async function getPaymentHistory(page = 1, pageSize = 10): Promise<PaymentHistoryResponse | null> {
  const res: GenericResponse<PaymentHistoryResponse> = await apiClient(
    `/payment/history?page=${page}&pageSize=${pageSize}`
  );
  return res?.data ?? null;
}

export interface CurrentSubscription {
  id: string;
  planName: string;
  status: string;
  startDate: string;
  endDate: string | null;
}

export async function getCurrentSubscription(): Promise<CurrentSubscription | null> {
  const res: GenericResponse<{
    subscriptionId: string;
    planName: string;
    status: string;
    startDate: string;
    endDate: string | null;
  }> = await apiClient("/payment/subscription/current");
  return res?.data ? { ...res.data, id: res.data.subscriptionId } : null;
}

export interface CreateCheckoutRequest {
  planType: number;
  returnUrl: string;
  cancelUrl: string;
}

export interface CheckoutResponse {
  checkoutUrl: string;
  paymentLinkId?: string | null;
  orderCode?: string | null;
}

export async function cancelSubscription(): Promise<boolean> {
  return false;
}

export interface CreateCreditPackCheckoutRequest {
  packName: string;
  credits: number;
  price: string;
  returnUrl: string;
  cancelUrl: string;
}

export async function createCheckout(data: CreateCheckoutRequest): Promise<CheckoutResponse | null> {
  const planCodes = ["Free", "Plus", "Premium", "Plus", "Premium"];
  const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/checkout", {
    data: {
      paymentType: 1,
      planCode: planCodes[data.planType] ?? "Free",
      returnUrl: data.returnUrl,
      cancelUrl: data.cancelUrl,
    },
    method: "POST",
  });
  return res?.data ?? null;
}

export async function createCreditPackCheckout(data: CreateCreditPackCheckoutRequest): Promise<CheckoutResponse | null> {
  const packCodes: Record<string, number> = { Starter: 1, Standard: 2, Growth: 3, Business: 4 };
  const creditPackCode = packCodes[data.packName];
  if (!creditPackCode) return null;

  const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/checkout", {
    data: {
      paymentType: 2,
      planCode: "",
      creditPackCode,
      returnUrl: data.returnUrl,
      cancelUrl: data.cancelUrl,
    },
    method: "POST",
  });
  return res?.data ?? null;
}
