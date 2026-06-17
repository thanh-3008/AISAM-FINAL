"use client";

import { useCallback } from "react";
import { useWorkspaces, addWorkspaceToCache, invalidateWorkspaceCache, type WorkspaceData } from "@/hooks/useWorkspaces";

export interface Profile {
  id: string;
  userId: string;
  name: string;
  profileType: number;
  subscriptionId: string | null;
  companyName: string | null;
  bio: string | null;
  avatarUrl: string | null;
  status: number;
  createdAt: string;
  updatedAt: string;
  isOwner: boolean;
  memberRole: string | null;
}

const workspaceTypeLabels: Record<number, string> = {
  1: "Personal",
  2: "Business",
};

export function getProfileTypeLabel(type: number): string {
  return workspaceTypeLabels[type] ?? "Unknown";
}

function workspaceToProfile(workspace: WorkspaceData): Profile {
  return {
    id: workspace.id,
    userId: workspace.userId,
    name: workspace.name,
    profileType: workspace.workspaceType,
    subscriptionId: null,
    companyName: workspace.companyName ?? null,
    bio: workspace.bio ?? null,
    avatarUrl: null,
    status: workspace.status,
    createdAt: workspace.createdAt,
    updatedAt: workspace.updatedAt,
    isOwner: workspace.isOwner,
    memberRole: workspace.memberRole,
  };
}

function profileToWorkspace(profile: Profile): WorkspaceData {
  return {
    id: profile.id,
    userId: profile.userId,
    name: profile.name,
    workspaceType: profile.profileType,
    plan: profile.profileType === 2 ? "Business" : "Personal",
    status: profile.status,
    bio: profile.bio,
    companyName: profile.companyName,
    createdAt: profile.createdAt,
    updatedAt: profile.updatedAt,
    isOwner: profile.isOwner,
    memberRole: profile.memberRole,
  };
}

export function invalidateProfileCache() {
  invalidateWorkspaceCache();
}

export function addProfileToCache(profile: Profile) {
  addWorkspaceToCache(profileToWorkspace(profile));
}

export function useProfiles() {
  const { workspaces, loading, error, activeWorkspace, selectWorkspace, clearSelectedWorkspace, refetch } = useWorkspaces();
  const profiles = workspaces.map(workspaceToProfile);
  const activeProfile = activeWorkspace ? workspaceToProfile(activeWorkspace) : null;

  const selectProfile = useCallback((profile: Profile) => {
    selectWorkspace(profileToWorkspace(profile));
  }, [selectWorkspace]);

  return {
    profiles,
    loading,
    error,
    activeProfile,
    selectProfile,
    clearSelectedProfile: clearSelectedWorkspace,
    refetch,
  };
}
