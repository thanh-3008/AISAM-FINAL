export type ProfileType = number | "Free" | "Basic" | "Pro";

export type ProfileStatus = number | "Pending" | "Active" | "Suspended" | "Cancelled";

export type ProfileResponseDto = {
  id: string;
  userId: string;
  name: string;
  profileType: ProfileType;
  subscriptionId?: string | null;
  companyName?: string | null;
  bio?: string | null;
  avatarUrl?: string | null;
  status: ProfileStatus;
  createdAt: string;
  updatedAt: string;
  isOwner: boolean;
  memberRole?: string | null;
};
