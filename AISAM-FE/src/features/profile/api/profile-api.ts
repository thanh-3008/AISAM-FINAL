import { apiRequest } from "@/lib/api/fetcher";
import type { ProfileResponseDto } from "@/features/profile/types/profile";

export type ProfileInput = {
  name: string;
  profileType: "Free" | "Basic" | "Pro";
  companyName?: string;
  bio?: string;
  avatarUrl?: string;
};

function toFormData(input: Partial<ProfileInput>) {
  const formData = new FormData();
  Object.entries(input).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      formData.append(key, value);
    }
  });
  return formData;
}

export const profileApi = {
  listByUser: (userId: string, search?: string, isDeleted?: boolean) => {
    const params = new URLSearchParams();
    if (search) params.set("search", search);
    if (typeof isDeleted === "boolean") params.set("isDeleted", String(isDeleted));
    const qs = params.toString();
    return apiRequest<ProfileResponseDto[]>(`/api/profiles/user/${userId}${qs ? `?${qs}` : ""}`, {
      method: "GET",
      auth: true
    });
  },
  detail: (id: string) =>
    apiRequest<ProfileResponseDto>(`/api/profiles/${id}`, {
      method: "GET",
      auth: true
    }),
  create: (userId: string, payload: ProfileInput) =>
    apiRequest<ProfileResponseDto>(`/api/profiles/user/${userId}`, {
      method: "POST",
      auth: true,
      body: toFormData(payload)
    }),
  update: (id: string, payload: Partial<ProfileInput>) =>
    apiRequest<ProfileResponseDto>(`/api/profiles/${id}`, {
      method: "PUT",
      auth: true,
      body: toFormData(payload)
    }),
  delete: (id: string) =>
    apiRequest<boolean>(`/api/profiles/${id}`, {
      method: "DELETE",
      auth: true
    }),
  restore: (id: string) =>
    apiRequest<boolean>(`/api/profiles/${id}/restore`, {
      method: "PATCH",
      auth: true
    })
};
