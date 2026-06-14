import { type SocialAccount, type AccountStatus, getAccountStatus } from "@/services/socialAccountService";

export const PLATFORM_INFO: Record<string, { label: string; color: string; bg: string; gradient: string; icon: string }> = {
  facebook: { label: "Facebook", color: "#1877F2", bg: "bg-blue-50", gradient: "from-blue-500 to-blue-600", icon: "facebook" },
  instagram: { label: "Instagram", color: "#DD2A7B", bg: "bg-pink-50", gradient: "from-[#F58529] via-[#DD2A7B] to-[#8134AF]", icon: "instagram" },
  tiktok: { label: "TikTok", color: "#111111", bg: "bg-gray-100", gradient: "from-gray-900 to-gray-700", icon: "tiktok" },
};

export const STATUS_CONFIG: Record<AccountStatus, { label: string; color: string; bg: string; dot: string }> = {
  connected: { label: "Connected", color: "text-emerald-600", bg: "bg-emerald-50 border-emerald-200/40", dot: "bg-emerald-500" },
  expired: { label: "Expired", color: "text-amber-600", bg: "bg-amber-50 border-amber-200/40", dot: "bg-amber-500" },
  error: { label: "Error", color: "text-danger-red", bg: "bg-danger-red/5 border-danger-red/20", dot: "bg-danger-red" },
};

export function formatNumber(n: number): string {
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M`;
  if (n >= 1000) return `${(n / 1000).toFixed(1)}K`;
  return n.toString();
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

export function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "Just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export function getExpiresDays(iso: string | null): number | null {
  if (!iso) return null;
  return Math.ceil((new Date(iso).getTime() - Date.now()) / 86400000);
}

export function getExpiresColor(days: number | null): { color: string; bg: string; label: string } {
  if (days === null) return { color: "text-outline", bg: "bg-outline/10", label: "No expiry" };
  if (days <= 0) return { color: "text-danger-red", bg: "bg-danger-red/10", label: "Expired" };
  if (days <= 7) return { color: "text-danger-red", bg: "bg-danger-red/10", label: `${days}d left` };
  if (days <= 30) return { color: "text-amber-600", bg: "bg-amber-50", label: `${days}d left` };
  return { color: "text-emerald-600", bg: "bg-emerald-50", label: `${days}d left` };
}

export function getExpiresProgress(days: number | null): number {
  if (days === null) return 100;
  if (days <= 0) return 0;
  if (days >= 90) return 100;
  return Math.round((days / 90) * 100);
}

export function getInitials(name: string): string {
  return name
    .split(" ")
    .map((w) => w[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

export function getAccountDisplayName(account: SocialAccount): string {
  if (account.targets && account.targets.length > 0) {
    return account.targets.map((t) => t.name).join(", ");
  }
  return account.accountName || `${account.provider} Account`;
}

export function getAccountHandle(account: SocialAccount): string {
  return account.accountHandle || `User: ${account.providerUserId}`;
}

export { getAccountStatus };
