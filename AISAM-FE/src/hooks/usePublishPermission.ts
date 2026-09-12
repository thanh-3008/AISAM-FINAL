"use client";
import { useEffect, useState } from "react";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";
import { Kind, Permission } from "@/services/permissionService";
import { useResourcePermissions } from "./useResourcePermissions";

export function usePublishPermission(contentId: string, brandId?: string) {
  const [channels, setChannels] = useState<SocialIntegration[]>([]);
  useEffect(() => {
    let cancelled = false; setChannels([]);
    if (brandId) fetchSocialIntegrations(brandId).then(items => { if (!cancelled) setChannels(items.filter(i => i.isActive)); }).catch(() => {});
    return () => { cancelled = true; };
  }, [brandId]);
  const allowed = useResourcePermissions(channels.map(channel => ({ kind: Kind.Content, resourceId: contentId, permission: Permission.PostPublish, channelId: channel.id })));
  return channels.some((_, index) => allowed(index));
}
