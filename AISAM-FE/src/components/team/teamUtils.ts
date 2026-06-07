import { type MemberRole, type MemberStatus } from "@/services/teamService";

export const ROLE_CONFIG: Record<MemberRole, { label: string; color: string; bg: string }> = {
  Owner: { label: "Owner", color: "text-primary", bg: "bg-primary-fixed" },
  Admin: { label: "Admin", color: "text-secondary", bg: "bg-secondary-fixed" },
  Editor: { label: "Editor", color: "text-tertiary", bg: "bg-tertiary-fixed" },
  Member: { label: "Member", color: "text-on-surface", bg: "bg-surface-container-high" },
  Viewer: { label: "Viewer", color: "text-outline", bg: "bg-surface-container" },
};

export const STATUS_CONFIG: Record<MemberStatus, { label: string; color: string; dot: string }> = {
  Active: { label: "Active", color: "text-success-green", dot: "bg-success-green" },
  Pending: { label: "Pending", color: "text-warning-amber", dot: "bg-warning-amber" },
  Inactive: { label: "Inactive", color: "text-outline", dot: "bg-outline" },
};

export const TEAM_COLORS = [
  { bg: "from-primary to-primary/70", text: "text-primary", badge: "bg-primary-fixed text-primary", iconBg: "bg-primary/10" },
  { bg: "from-secondary to-secondary/70", text: "text-secondary", badge: "bg-secondary-fixed text-secondary", iconBg: "bg-secondary/10" },
  { bg: "from-tertiary to-tertiary/70", text: "text-tertiary", badge: "bg-tertiary-fixed text-tertiary", iconBg: "bg-tertiary/10" },
];

export const BRANDS = [
  { id: "b1", name: "Lumina Tech" },
  { id: "b2", name: "Summit Outdoor" },
  { id: "b3", name: "Heritage Motors" },
  { id: "b4", name: "GreenLeaf Organics" },
  { id: "b5", name: "Pulse Finance" },
];

export function getInitials(name: string): string {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

export function calcTimeAgo(now: number, iso: string): string {
  const diff = now - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "Just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}
