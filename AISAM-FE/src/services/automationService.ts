import { apiClient, apiFetch } from "@/lib/apiClient";

export interface AutomationItem {
  id: string;
  rowIndex: number;
  platform: string;
  brandId: string;
  brandName: string;
  productId?: string;
  contentId?: string;
  contentCalendarId?: string;
  topic: string;
  objective?: string;
  contentType: string;
  tone?: string;
  cta?: string;
  notes?: string;
  scheduledAt: string;
  status: string;
  estimatedCredits: number;
  usedCredits: number;
  generationAttemptCount: number;
  lastError?: string;
  generatedText?: string;
  generatedImageUrl?: string;
  generatedVideoUrl?: string;
  videoProvider?: string;
  validationErrors: string[];
}

export interface AutomationPlan {
  id: string;
  name: string;
  sourceFileName?: string;
  timezone: string;
  status: string;
  totalItems: number;
  validItems: number;
  failedItems: number;
  estimatedCredits: number;
  reservedCredits: number;
  usedCredits: number;
  releasedCredits: number;
  autoApprove: boolean;
  templateSourcePlanId?: string;
  createdAt: string;
  confirmedAt?: string;
  items: AutomationItem[];
}

interface ApiResponse<T> { success: boolean; message?: string; data?: T; error?: { errorMessage?: string } }

export interface AutomationPerformance {
  planId: string;
  totalItems: number;
  scheduledItems: number;
  publishedItems: number;
  failedItems: number;
  impressions: number;
  engagement: number;
  averageCtr: number;
  estimatedRevenue: number;
}

export interface AutomationTarget {
  integrationId: string;
  platform: string;
  name: string;
  externalId?: string;
  isScheduled: boolean;
  scheduleId?: string;
}

export async function fetchAutomationPlans(): Promise<AutomationPlan[]> {
  const response = await apiClient("/automation-plans") as ApiResponse<AutomationPlan[]>;
  return response?.data ?? [];
}

export async function fetchAutomationPlan(id: string): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${id}`) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || "Automation plan not found.");
  return response.data;
}

export async function importAutomationCsv(name: string, timezone: string, file: File): Promise<AutomationPlan> {
  const body = new FormData();
  body.append("name", name);
  body.append("timezone", timezone);
  body.append("file", file);
  const response = await apiFetch("/automation-plans/import-csv", { method: "POST", body }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to import CSV.");
  return response.data;
}

export async function confirmAutomationPlan(id: string): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${id}/confirm`, { method: "POST" }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to confirm plan.");
  return response.data;
}

export async function retryAutomationPlan(id: string, itemId?: string): Promise<AutomationPlan> {
  const query = itemId ? `?itemId=${encodeURIComponent(itemId)}` : "";
  const response = await apiClient(`/automation-plans/${id}/retry${query}`, { method: "POST" }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to retry generation.");
  return response.data;
}

export async function cancelAutomationPlan(id: string): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${id}/cancel`, { method: "POST" }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to cancel plan.");
  return response.data;
}

export async function approveAutomationPlan(id: string, itemId?: string): Promise<AutomationPlan> {
  const query = itemId ? `?itemId=${encodeURIComponent(itemId)}` : "";
  const response = await apiClient(`/automation-plans/${id}/approve${query}`, { method: "POST" }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to approve automation content.");
  return response.data;
}

export async function rejectAutomationItem(planId: string, itemId: string, notes?: string): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${planId}/items/${itemId}/reject`, {
    method: "POST",
    data: { notes },
  }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to reject automation content.");
  return response.data;
}

export async function importAutomationGoogleSheet(name: string, timezone: string, url: string): Promise<AutomationPlan> {
  const response = await apiClient("/automation-plans/import-google-sheet", { method: "POST", data: { name, timezone, url } }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to import Google Sheet.");
  return response.data;
}

export async function cloneAutomationPlan(id: string, name: string, shiftDays: number): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${id}/clone`, { method: "POST", data: { name, shiftDays } }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to clone plan.");
  return response.data;
}

export async function setAutomationAutoApprove(id: string, enabled: boolean): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${id}/auto-approve`, { method: "PUT", data: { enabled } }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to change auto-approve.");
  return response.data;
}

export async function fetchAutomationPerformance(id: string): Promise<AutomationPerformance> {
  const response = await apiClient(`/automation-plans/${id}/performance`) as ApiResponse<AutomationPerformance>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to load performance.");
  return response.data;
}

export async function updateAutomationItem(planId: string, itemId: string, request: {
  brandId: string; productId?: string; topic: string; platform: string; contentType: string;
  objective?: string; tone?: string; cta?: string; notes?: string; scheduledAt: string;
}): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${planId}/items/${itemId}`, { method: "PUT", data: request }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to update automation item.");
  return response.data;
}

export async function fetchAutomationTargets(planId: string, itemId: string): Promise<AutomationTarget[]> {
  const response = await apiClient(`/automation-plans/${planId}/items/${itemId}/targets`) as ApiResponse<AutomationTarget[]>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to load linked pages.");
  return response.data;
}

export async function approveAutomationTargets(planId: string, itemId: string, integrationIds: string[]): Promise<AutomationPlan> {
  const response = await apiClient(`/automation-plans/${planId}/items/${itemId}/approve-targets`, { method: "POST", data: { integrationIds } }) as ApiResponse<AutomationPlan>;
  if (!response?.data) throw new Error(response?.message || response?.error?.errorMessage || "Unable to schedule selected pages.");
  return response.data;
}
