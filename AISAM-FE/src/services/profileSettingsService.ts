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
  try {
    const res: GenericResponse<null> = await apiClient("/auth/change-password", {
      data,
      method: "POST",
    });
    return res?.success ?? false;
  } catch {
    // Fallback: Simulate success for demo
    console.log("Mock: Password changed successfully");
    return true;
  }
}

export interface PaymentHistoryItem {
  id: string;
  amount: number;
  currency: string;
  status: string;
  paymentMethod: string;
  createdAt: string;
  description: string;
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

// Mock payment history (static timestamps to avoid hydration mismatch)
const MOCK_PAYMENT_HISTORY: PaymentHistoryItem[] = [
  {
    id: "pay-1",
    amount: 29.00,
    currency: "USD",
    status: "Completed",
    paymentMethod: "Visa •••• 4242",
    createdAt: "2026-05-08T10:00:00.000Z",
    description: "Basic Plan - Monthly Subscription",
  },
  {
    id: "pay-2",
    amount: 29.00,
    currency: "USD",
    status: "Completed",
    paymentMethod: "Visa •••• 4242",
    createdAt: "2026-04-08T10:00:00.000Z",
    description: "Basic Plan - Monthly Subscription",
  },
  {
    id: "pay-3",
    amount: 29.00,
    currency: "USD",
    status: "Completed",
    paymentMethod: "Visa •••• 4242",
    createdAt: "2026-03-08T10:00:00.000Z",
    description: "Basic Plan - Monthly Subscription",
  },
];

export async function getPaymentHistory(page = 1, pageSize = 10): Promise<PaymentHistoryResponse | null> {
  try {
    const res: GenericResponse<PaymentHistoryResponse> = await apiClient(
      `/payment/history?page=${page}&pageSize=${pageSize}`
    );
    return res?.data ?? null;
  } catch {
    // Fallback to mock data
    const start = (page - 1) * pageSize;
    const end = start + pageSize;
    const paginatedData = MOCK_PAYMENT_HISTORY.slice(start, end);
    
    return {
      data: paginatedData,
      totalCount: MOCK_PAYMENT_HISTORY.length,
      page,
      pageSize,
      totalPages: Math.ceil(MOCK_PAYMENT_HISTORY.length / pageSize),
      hasNextPage: end < MOCK_PAYMENT_HISTORY.length,
      hasPreviousPage: page > 1,
    };
  }
}

export interface CurrentSubscription {
  id: string;
  planName: string;
  planType: number;
  status: string;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  amount: number;
  currency: string;
}

// Mock subscription (static timestamps to avoid hydration mismatch)
const MOCK_SUBSCRIPTION: CurrentSubscription = {
  id: "sub-1",
  planName: "Basic",
  planType: 1,
  status: "Active",
  startDate: "2026-03-08T10:00:00.000Z",
  endDate: "2026-07-08T10:00:00.000Z",
  autoRenew: true,
  amount: 29.00,
  currency: "USD",
};

export async function getCurrentSubscription(): Promise<CurrentSubscription | null> {
  try {
    const res: GenericResponse<CurrentSubscription> = await apiClient("/payment/subscription/current");
    return res?.data ?? null;
  } catch {
    // Fallback to mock data
    return MOCK_SUBSCRIPTION;
  }
}

export interface CreateCheckoutRequest {
  planType: number;
  returnUrl: string;
  cancelUrl: string;
}

export interface CheckoutResponse {
  checkoutUrl: string;
  orderId: string;
}

export async function cancelSubscription(): Promise<boolean> {
  // BE currently exposes current subscription/history/checkout, but no cancel endpoint.
  console.log("Mock: Subscription cancelled");
  return true;
}

export interface CreateCreditPackCheckoutRequest {
  packName: string;
  credits: number;
  price: string;
  returnUrl: string;
  cancelUrl: string;
}

export async function createCheckout(data: CreateCheckoutRequest): Promise<CheckoutResponse | null> {
  try {
    const planCodes = ["Free", "PersonalPlus", "PersonalPro", "BusinessPlus", "BusinessPro"];
    const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/checkout", {
      data: {
        paymentType: "Subscription",
        planCode: planCodes[data.planType] ?? "Free",
        returnUrl: data.returnUrl,
        cancelUrl: data.cancelUrl,
      },
      method: "POST",
    });
    return res?.data ?? null;
  } catch {
    console.log("Mock: Creating checkout for plan type", data.planType);
    return null;
  }
}

export async function createCreditPackCheckout(data: CreateCreditPackCheckoutRequest): Promise<CheckoutResponse | null> {
  try {
    const res: GenericResponse<CheckoutResponse> = await apiClient("/payment/checkout", {
      data: {
        paymentType: "CreditPack",
        planCode: "",
        creditPackCode: data.packName,
        returnUrl: data.returnUrl,
        cancelUrl: data.cancelUrl,
      },
      method: "POST",
    });
    return res?.data ?? null;
  } catch {
    console.log("Mock: Creating credit pack checkout for", data.packName);
    return null;
  }
}
