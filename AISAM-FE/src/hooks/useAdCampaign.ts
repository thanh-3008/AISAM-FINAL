"use client";

import { useCallback, useEffect, useState } from "react";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import {
  createAdCampaign,
  deleteAdCampaign,
  fetchAdCampaignPage,
  getAdCampaign,
  syncAdCampaign,
  updateAdCampaign,
  type AdCampaignDto,
  type AdCampaignQuery,
  type CreateAdCampaignRequest,
  type UpdateAdCampaignRequest,
} from "@/services/adCampaignService";

export function useAdCampaign(initialQuery: AdCampaignQuery = {}) {
  const { activeWorkspace } = useWorkspaces();
  const [campaigns, setCampaigns] = useState<AdCampaignDto[]>([]);
  const [selectedCampaign, setSelectedCampaign] = useState<AdCampaignDto | null>(null);
  const [query, setQuery] = useState<AdCampaignQuery>(initialQuery);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(initialQuery.page ?? 1);
  const [pageSize, setPageSize] = useState(initialQuery.pageSize ?? 50);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const loadCampaigns = useCallback(async (nextQuery: AdCampaignQuery = query) => {
    if (!activeWorkspace?.id) {
      setCampaigns([]);
      setSelectedCampaign(null);
      return [];
    }

    setIsLoading(true);
    try {
      const pageResult = await fetchAdCampaignPage(nextQuery);
      setCampaigns(pageResult.items);
      setTotalCount(pageResult.totalCount);
      setPage(pageResult.page);
      setPageSize(pageResult.pageSize);
      setError(null);
      return pageResult.items;
    } catch (err) {
      const nextError = err instanceof Error ? err : new Error("Failed to load ad campaigns.");
      setError(nextError);
      setCampaigns([]);
      setTotalCount(0);
      return [];
    } finally {
      setIsLoading(false);
    }
  }, [activeWorkspace?.id, query]);

  const loadCampaign = useCallback(async (id: string) => {
    setIsLoading(true);
    try {
      const data = await getAdCampaign(id);
      setSelectedCampaign(data);
      setError(null);
      return data;
    } catch (err) {
      const nextError = err instanceof Error ? err : new Error("Failed to load ad campaign.");
      setError(nextError);
      setSelectedCampaign(null);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const createCampaign = useCallback(async (payload: CreateAdCampaignRequest) => {
    const data = await createAdCampaign(payload);
    if (data) {
      setCampaigns((prev) => [data, ...prev.filter((item) => item.id !== data.id)]);
    }
    return data;
  }, []);

  const updateCampaign = useCallback(async (id: string, payload: UpdateAdCampaignRequest) => {
    const data = await updateAdCampaign(id, payload);
    if (data) {
      setCampaigns((prev) => prev.map((item) => item.id === data.id ? data : item));
      setSelectedCampaign((prev) => prev?.id === data.id ? data : prev);
    }
    return data;
  }, []);

  const deleteCampaign = useCallback(async (id: string) => {
    const ok = await deleteAdCampaign(id);
    if (ok) {
      setCampaigns((prev) => prev.filter((item) => item.id !== id));
      setSelectedCampaign((prev) => prev?.id === id ? null : prev);
    }
    return ok;
  }, []);

  const syncCampaign = useCallback(async (id: string) => {
    const data = await syncAdCampaign(id);
    if (data) {
      setCampaigns((prev) => prev.map((item) => item.id === data.id ? data : item));
      setSelectedCampaign((prev) => prev?.id === data.id ? data : prev);
    }
    return data;
  }, []);

  useEffect(() => {
    void loadCampaigns(query);
  }, [loadCampaigns, query]);

  return {
    campaigns,
    selectedCampaign,
    query,
    setQuery,
    totalCount,
    page,
    pageSize,
    totalPages: Math.max(1, Math.ceil(totalCount / Math.max(pageSize, 1))),
    isLoading,
    error,
    refresh: loadCampaigns,
    loadCampaign,
    createCampaign,
    updateCampaign,
    deleteCampaign,
    syncCampaign,
  };
}
