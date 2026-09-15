"use client";

import { useEffect, useMemo, useState } from "react";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useRbac } from "@/contexts/RbacContext";
import { getCurrentSubscription } from "@/services/profileSettingsService";
import {
  getWorkspacePlanType,
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

export function useFeatureGate(enabled = true) {
  const rbac = useRbac();
  const { activeWorkspace, updateWorkspacePlan } = useWorkspaces();
  const [syncedPlanName, setSyncedPlanName] = useState<string | null>(null);
  const [isResolvingPlan, setIsResolvingPlan] = useState(false);

  useEffect(() => {
    if (!enabled || !activeWorkspace?.id || (rbac && !rbac.actions.includes("billing.read"))) {
      setSyncedPlanName(null);
      setIsResolvingPlan(false);
      return;
    }

    let cancelled = false;
    setSyncedPlanName(null);
    setIsResolvingPlan(true);

    getCurrentSubscription()
      .then((subscription) => {
        if (cancelled) return;

        const planName = subscription?.planName || null;
        setSyncedPlanName(planName);

        if (planName && planName !== activeWorkspace.plan) {
          updateWorkspacePlan(activeWorkspace.id, planName);
        }
      })
      .catch(() => {
        if (!cancelled) setSyncedPlanName(null);
      })
      .finally(() => {
        if (!cancelled) setIsResolvingPlan(false);
      });

    return () => {
      cancelled = true;
    };
  }, [activeWorkspace?.id, activeWorkspace?.plan, enabled, updateWorkspacePlan, rbac]);

  const plan = useMemo(() => {
    if (!activeWorkspace) return PlanType.Free;
    return getWorkspacePlanType(syncedPlanName || activeWorkspace.plan, activeWorkspace.workspaceType);
  }, [activeWorkspace, syncedPlanName]);

  const role = useMemo(() => {
    if (rbac) return (rbac.workspaceRole === "Owner" ? "Owner" : rbac.workspaceRole === "WorkspaceManager" ? "Manager" :
      rbac.scopes.some(s => s.role === "Manager" || s.role === "ContentCreator") ? "ContentCreator" : "Viewer") as WorkspaceRole;
    if (!activeWorkspace?.memberRole) return null;
    return activeWorkspace.memberRole as WorkspaceRole;
  }, [activeWorkspace, rbac]);

  return useMemo(() => ({
    plan,
    planName: PLAN_NAMES[plan] || "Free",
    role,
    isResolvingPlan,
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
  }), [plan, role, isResolvingPlan, activeWorkspace]);
}
