import { apiClient } from "@/lib/apiClient";
import { createPayment as createPayOSPayment, type CheckoutResponse } from "./paymentService";

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

export async function changePassword(data: ChangePasswordRequest): Promise<{ success: boolean; message?: string }> {
  try {
    const res: GenericResponse<null> = await apiClient("/auth/change-password", {
      data,
      method: "POST",
    });
    return { success: res?.success ?? false, message: res?.message || undefined };
  } catch (err: any) {
    return { success: false, message: err.message };
  }
}

export interface PaymentHistoryItem {
  id: string;
  amount: number;
  paymentMethod: string;
  status: string;
  createdAt: string;
}

export interface PaymentHistoryResponse {
  data: PaymentHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export async function getPaymentHistory(page = 1, pageSize = 10): Promise<PaymentHistoryResponse | null> {
  try {
    const res: GenericResponse<PaymentHistoryResponse> = await apiClient(
      `/payment/history?page=${page}&pageSize=${pageSize}`
    );
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export interface CurrentSubscription {
  subscriptionId: string;
  planName: string;
  status: string;
  startDate: string;
  endDate: string | null;
  promptQuota: number;
  imageQuota: number;
  postQuota: number;
  platformQuota: number;
  accountQuota: number;
  analysisLevel: number;
  adBudgetMonthly: number;
  adCampaignQuota: number;
}

export async function getCurrentSubscription(): Promise<CurrentSubscription | null> {
  try {
    const res: GenericResponse<CurrentSubscription> = await apiClient("/payment/subscription/current");
    return res?.data ?? null;
  } catch (error) {
    const message = error instanceof Error ? error.message : "";
    if (message.includes("Active subscription not found")) {
      return null;
    }
    throw error;
  }
}

export async function createCheckout(data: {
  planCode: string;
  returnUrl: string;
  cancelUrl: string;
}): Promise<CheckoutResponse | null> {
  return createPayOSPayment({
    paymentType: 1,
    planCode: data.planCode,
    returnUrl: data.returnUrl,
    cancelUrl: data.cancelUrl,
  });
}

export async function createCreditPackCheckout(data: {
  creditPackCode: number;
  returnUrl: string;
  cancelUrl: string;
}): Promise<CheckoutResponse | null> {
  return createPayOSPayment({
    paymentType: 2,
    creditPackCode: data.creditPackCode,
    returnUrl: data.returnUrl,
    cancelUrl: data.cancelUrl,
  });
}
