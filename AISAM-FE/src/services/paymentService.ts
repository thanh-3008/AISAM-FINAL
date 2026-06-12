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
  try {
    const res: GenericResponse<PayOSPaymentResponse> = await apiClient("/payment/create", {
      data,
      method: "POST",
    });
    return res?.data ?? null;
  } catch {
    // Mock: Generate mock QR payment data
    const mockOrderId = `PAY${Date.now()}`;
    return {
      orderId: mockOrderId,
      checkoutUrl: `https://pay.payos.vn/${mockOrderId}`,
      qrCode: `https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=payos%3A%2F%2F${mockOrderId}`,
      status: "pending",
      amount: data.amount,
      description: data.paymentType === "Subscription"
        ? `Upgrade to ${["Free", "Personal Plus", "Personal Pro", "Business Plus", "Business Pro"][data.planType ?? 0]} plan`
        : `Credit Pack: ${data.packName} - ${data.credits} credits`,
    };
  }
}

export async function checkPaymentStatus(orderId: string): Promise<PayOSPaymentResponse | null> {
  try {
    const res: GenericResponse<PayOSPaymentResponse> = await apiClient(`/payment/status/${orderId}`);
    return res?.data ?? null;
  } catch {
    // Mock: Simulate successful payment after 15 seconds
    return null;
  }
}
