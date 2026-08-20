import { apiClient, apiFetch } from "@/lib/apiClient";

export interface HolidayEventDto {
  id: string;
  name: string;
  localName?: string;
  exactDate: string;
  year: number;
  countryCode: string;
}

export interface SuggestHolidayCaptionRequest {
  brandId: string;
}

export const holidayService = {
  getUpcoming: async (workspaceId: string, days: number = 30): Promise<HolidayEventDto[]> => {
    const response = await apiFetch(`/workspace-context/${workspaceId}/holidays/upcoming?days=${days}`);
    return response?.data || [];
  },

  suggestCaption: async (workspaceId: string, holidayId: string, request: SuggestHolidayCaptionRequest) => {
    const response = await apiFetch(`/workspace-context/${workspaceId}/holidays/${holidayId}/suggest-caption`, {
      method: "POST",
      body: JSON.stringify(request)
    });
    if (!response?.success) throw new Error(response?.message || "Failed to suggest caption");
    return response?.data;
  },

  generateVideo: async (workspaceId: string, holidayId: string, request: SuggestHolidayCaptionRequest) => {
    const response = await apiFetch(`/workspace-context/${workspaceId}/holidays/${holidayId}/generate-video`, {
      method: "POST",
      body: JSON.stringify(request)
    });
    if (!response?.success) throw new Error(response?.message || "Failed to generate video");
    return response?.data;
  },

  suggestCustomEvent: async (workspaceId: string, request: { brandId: string; eventName: string; adType: number }) => {
    const response = await apiFetch(`/workspace-context/${workspaceId}/holidays/custom-event`, {
      method: "POST",
      body: JSON.stringify(request)
    });
    if (!response?.success) throw new Error(response?.message || "Failed to suggest custom event");
    return response?.data;
  }
};
