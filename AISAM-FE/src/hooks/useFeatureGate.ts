"use client";

import { useMemo } from "react";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import {
  getPlanType,
  canAccessFeature,
  hasPermission,
  FEATURE_MATRIX,
  PLAN_HIERARCHY,
  PLAN_NAMES,
  PlanType,
  type Feature,
  type Permission,
  type WorkspaceRole,
} from "@/lib/featureConfig";

export function useFeatureGate() {
  const { activeWorkspace } = useWorkspaces();

  const plan = useMemo(() => {
    if (!activeWorkspace) return PlanType.Free;
    return getPlanType(activeWorkspace.plan);
  }, [activeWorkspace]);

  const role = useMemo(() => {
    if (!activeWorkspace?.memberRole) return null;
    return activeWorkspace.memberRole as WorkspaceRole;
  }, [activeWorkspace]);

  return useMemo(() => ({
    plan,
    planName: PLAN_NAMES[plan] || "Free",
    role,
    isOwner: role === "Owner",
    isManager: role === "Manager",
    isContentCreator: role === "ContentCreator",
    isViewer: role === "Viewer",
    isBusiness: activeWorkspace?.workspaceType === 2,

    canAccess(feature: Feature): boolean {
      return canAccessFeature(plan, feature);
    },

    can(permission: Permission): boolean {
      if (!role) return false;
      return hasPermission(role, permission);
    },

    getAvailableFeatures(): Feature[] {
      return (Object.keys(FEATURE_MATRIX) as Feature[]).filter(
        (f) => canAccessFeature(plan, f)
      );
    },

    getLockedFeatures(): Feature[] {
      return (Object.keys(FEATURE_MATRIX) as Feature[]).filter(
        (f) => !canAccessFeature(plan, f)
      );
    },

    getPlanLevel(): number {
      return PLAN_HIERARCHY[plan] ?? 0;
    },
  }), [plan, role, activeWorkspace]);
}
