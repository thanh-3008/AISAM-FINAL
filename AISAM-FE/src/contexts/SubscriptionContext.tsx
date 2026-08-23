"use client";

import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { getCurrentSubscription } from "@/services/profileSettingsService";

export type SubscriptionStatus = "active" | "expired" | "limited" | "archived" | "none";

interface SubscriptionContextValue {
  status: SubscriptionStatus;
  daysUntilExpiry: number;
  isLimited: boolean;
  isExpired: boolean;
  isArchived: boolean;
  subscriptionEndDate: string | null;
  refresh: () => Promise<void>;
}

const SubscriptionContext = createContext<SubscriptionContextValue>({
  status: "none",
  daysUntilExpiry: 0,
  isLimited: false,
  isExpired: false,
  isArchived: false,
  subscriptionEndDate: null,
  refresh: async () => {},
});

export function SubscriptionProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<SubscriptionStatus>("none");
  const [subscriptionEndDate, setSubscriptionEndDate] = useState<string | null>(null);

  const checkSubscription = useCallback(async () => {
    try {
      const subscription = await getCurrentSubscription();
      if (subscription?.endDate) {
        setSubscriptionEndDate(subscription.endDate);
        const now = new Date();
        const endDate = new Date(subscription.endDate);
        const daysDiff = Math.floor((endDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

        if (subscription.status === "Active" || daysDiff > 0) {
          setStatus("active");
        } else if (daysDiff <= -180) {
          setStatus("archived");
        } else if (daysDiff <= -90) {
          setStatus("archived");
        } else {
          setStatus("limited");
        }
      } else {
        setStatus("active");
      }
    } catch {
      setStatus("active");
    }
  }, []);

  useEffect(() => {
    checkSubscription();
  }, [checkSubscription]);

  const [now] = useState(() => Date.now());
  const daysUntilExpiry = subscriptionEndDate
    ? Math.max(0, Math.floor((new Date(subscriptionEndDate).getTime() - now) / (1000 * 60 * 60 * 24)))
    : 0;

  const isLimited = status === "limited";
  const isExpired = status === "expired" || isLimited;
  const isArchived = status === "archived";

  return (
    <SubscriptionContext.Provider value={{ status, daysUntilExpiry, isLimited, isExpired, isArchived, subscriptionEndDate, refresh: checkSubscription }}>
      {children}
      {isLimited && <LimitedModeBanner daysUntilExpiry={daysUntilExpiry} />}
      {isArchived && <ArchivedBanner />}
    </SubscriptionContext.Provider>
  );
}

function LimitedModeBanner({ daysUntilExpiry }: { daysUntilExpiry: number }) {
  return (
    <div className="fixed bottom-4 left-1/2 -translate-x-1/2 z-50 px-6 py-3 bg-error/10 border border-error/30 rounded-2xl shadow-xl backdrop-blur-xl flex items-center gap-3 max-w-lg">
      <span className="material-symbols-outlined text-error text-[20px]">info</span>
      <p className="text-label-sm text-error font-medium">
        Workspace in limited mode. Premium features are locked.
        {daysUntilExpiry > 0 ? ` Renew within ${daysUntilExpiry} days to keep full access.` : ""}
      </p>
    </div>
  );
}

function ArchivedBanner() {
  return (
    <div className="fixed bottom-4 left-1/2 -translate-x-1/2 z-50 px-6 py-3 bg-outline/10 border border-outline/30 rounded-2xl shadow-xl backdrop-blur-xl flex items-center gap-3 max-w-lg">
      <span className="material-symbols-outlined text-outline text-[20px]">archive</span>
      <p className="text-label-sm text-outline font-medium">
        Workspace is archived. Contact your admin to restore.
      </p>
    </div>
  );
}

export function useSubscription() {
  return useContext(SubscriptionContext);
}
