export type CreditTransactionTone = "positive" | "negative" | "neutral";

export interface CreditTransactionLike {
  action: string;
  credits: number;
  status: string;
}

export interface CreditTransactionPresentation {
  label: string;
  tone: CreditTransactionTone;
}

const CREDIT_INCREASE_ACTIONS = new Set([
  "subscriptiongrant",
  "creditpackpurchase",
]);

function normalizeCreditAction(action: string): string {
  return action.replace(/[\s_-]/g, "").toLowerCase();
}

export function formatVndAmount(amount: number): string {
  const safeAmount = Number.isFinite(amount) ? amount : 0;
  const formatted = new Intl.NumberFormat("en-US", {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(safeAmount);

  return `${formatted} VNĐ`;
}

export function isCreditIncreaseAction(action: string, credits: number): boolean {
  const normalizedAction = normalizeCreditAction(action);

  if (normalizedAction === "adminadjust") {
    return credits > 0;
  }

  return CREDIT_INCREASE_ACTIONS.has(normalizedAction);
}

export function getCreditTransactionPresentation(
  transaction: CreditTransactionLike
): CreditTransactionPresentation {
  if (transaction.status.toLowerCase() !== "success" || transaction.credits === 0) {
    return { label: "0 credit", tone: "neutral" };
  }

  const isIncrease = isCreditIncreaseAction(transaction.action, transaction.credits);
  const sign = isIncrease ? "+" : "-";
  const amount = Math.abs(transaction.credits).toLocaleString("en-US");

  return {
    label: `${sign}${amount} credit`,
    tone: isIncrease ? "positive" : "negative",
  };
}
