import { apiClient } from "@/lib/apiClient";

interface GenericResponse<T> {
  success: boolean;
  data?: T;
}

export async function fetchTags(): Promise<string[]> {
  try {
    const res: GenericResponse<string[]> = await apiClient("/tags");
    return res?.data ?? [];
  } catch {
    return [];
  }
}
