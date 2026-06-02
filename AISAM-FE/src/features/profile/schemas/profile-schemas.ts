import { z } from "zod";

export const profileSchema = z.object({
  name: z.string().min(1).max(255),
  profileType: z.enum(["Free", "Basic", "Pro"]),
  companyName: z.string().max(255).optional().or(z.literal("")),
  bio: z.string().max(1000).optional().or(z.literal("")),
  avatarUrl: z.string().url().optional().or(z.literal(""))
});
