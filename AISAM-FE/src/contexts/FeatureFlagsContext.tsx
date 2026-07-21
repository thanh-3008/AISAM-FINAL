"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import { usePathname } from "next/navigation";
import { apiClient } from "@/lib/apiClient";

interface FeatureFlags {
  enabledFeatures: string[];
  maintenanceMode: boolean;
}

interface FeatureFlagsContextType {
  features: FeatureFlags | null;
  hasFeature: (featureKey: string) => boolean;
}

const FeatureFlagsContext = createContext<FeatureFlagsContextType>({
  features: null,
  hasFeature: () => true, // default to true if loading
});

export function FeatureFlagsProvider({ children }: { children: React.ReactNode }) {
  const [features, setFeatures] = useState<FeatureFlags | null>(null);
  const pathname = usePathname();

  useEffect(() => {
    async function fetchFlags() {
      try {
        const res: any = await apiClient("/feature-flags");
        if (res?.success && res?.data) {
          setFeatures(res.data);
        }
      } catch (err) {
        console.error("Failed to load feature flags:", err);
      }
    }
    fetchFlags();
  }, []);

  const hasFeature = (key: string) => {
    if (!features) return true; // optimistic
    return features.enabledFeatures.includes(key);
  };

  // If in maintenance mode and not on an admin route, block rendering
  if (features?.maintenanceMode && !pathname?.startsWith("/admin")) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 text-center p-6">
        <span className="material-symbols-outlined text-6xl text-blue-600 mb-4">build_circle</span>
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Under Maintenance</h1>
        <p className="text-gray-500 max-w-md">
          We are currently performing scheduled maintenance to improve the platform. Please check back shortly.
        </p>
      </div>
    );
  }

  return (
    <FeatureFlagsContext.Provider value={{ features, hasFeature }}>
      {children}
    </FeatureFlagsContext.Provider>
  );
}

export const useFeatureFlags = () => useContext(FeatureFlagsContext);
