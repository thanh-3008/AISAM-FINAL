import type { ProfileStatus, ProfileType } from "@/features/profile/types/profile";

export function profileTypeLabel(value: ProfileType) {
  if (value === 0 || value === "Free") return "Free";
  if (value === 1 || value === "Basic") return "Basic";
  if (value === 2 || value === "Pro") return "Pro";
  return String(value);
}

export function profileStatusLabel(value: ProfileStatus) {
  if (value === 0 || value === "Pending") return "Pending";
  if (value === 1 || value === "Active") return "Active";
  if (value === 2 || value === "Suspended") return "Suspended";
  if (value === 3 || value === "Cancelled") return "Cancelled";
  return String(value);
}
