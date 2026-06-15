import { PostItem, PostStatus } from "@/services/postService";

export function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
}

export function formatDateTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("en-GB", { 
    day: "numeric", 
    month: "short", 
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

export function daysUntil(iso: string): string | null {
  const diff = Math.ceil((new Date(iso).getTime() - Date.now()) / 86400000);
  if (diff <= 0) return null;
  return diff === 1 ? "Tomorrow" : `In ${diff} days`;
}

export function getStatusStyle(status: PostStatus): string {
  const styles: Record<PostStatus, string> = {
    Published: "bg-emerald-50 text-emerald-600 border-emerald-500/20",
    Scheduled: "bg-blue-50 text-blue-600 border-blue-500/20",
    Failed: "bg-danger-red/10 text-danger-red border-danger-red/20",
    Draft: "bg-gray-50 text-gray-600 border-gray-500/20"
  };
  return styles[status];
}

export function getStatusIcon(status: PostStatus): string {
  const icons: Record<PostStatus, string> = {
    Published: "check_circle",
    Scheduled: "schedule",
    Failed: "error",
    Draft: "draft"
  };
  return icons[status];
}

export function filterPosts(
  posts: PostItem[],
  filters: {
    search?: string;
    brand?: string;
    platform?: string;
    status?: string;
    type?: string;
    dateFrom?: string;
    dateTo?: string;
    minLikes?: number;
    minComments?: number;
    minShares?: number;
  }
): PostItem[] {
  let filtered = [...posts];
  
  if (filters.search) {
    const q = filters.search.toLowerCase();
    filtered = filtered.filter((p) => 
      (p.contentTitle || "").toLowerCase().includes(q) || 
      (p.brandName || "").toLowerCase().includes(q) ||
      (p.caption || "").toLowerCase().includes(q)
    );
  }
  
  if (filters.brand) filtered = filtered.filter((p) => p.brandName === filters.brand);
  if (filters.platform) filtered = filtered.filter((p) => p.platform === filters.platform);
  if (filters.status) filtered = filtered.filter((p) => p.status === filters.status);
  if (filters.type) filtered = filtered.filter((p) => p.type === filters.type);
  
  if (filters.dateFrom) {
    filtered = filtered.filter((p) => new Date(p.publishedAt) >= new Date(filters.dateFrom!));
  }
  
  if (filters.dateTo) {
    filtered = filtered.filter((p) => new Date(p.publishedAt) <= new Date(filters.dateTo! + "T23:59:59"));
  }
  
  if (filters.minLikes && filters.minLikes > 0) {
    filtered = filtered.filter((p) => (p.likes || 0) >= filters.minLikes!);
  }
  
  if (filters.minComments && filters.minComments > 0) {
    filtered = filtered.filter((p) => (p.comments || 0) >= filters.minComments!);
  }
  
  if (filters.minShares && filters.minShares > 0) {
    filtered = filtered.filter((p) => (p.shares || 0) >= filters.minShares!);
  }
  
  return filtered;
}

export function sortPosts(
  posts: PostItem[],
  sortKey: "publishedAt" | "contentTitle" | "brandName" | "status" | "likes" | "comments" | "shares",
  sortDir: "asc" | "desc"
): PostItem[] {
  const sorted = [...posts];
  
  sorted.sort((a, b) => {
    let cmp = 0;
    
    switch (sortKey) {
      case "publishedAt":
        cmp = new Date(a.publishedAt).getTime() - new Date(b.publishedAt).getTime();
        break;
      case "contentTitle":
        cmp = (a.contentTitle || "").localeCompare(b.contentTitle || "");
        break;
      case "brandName":
        cmp = (a.brandName || "").localeCompare(b.brandName || "");
        break;
      case "status":
        cmp = a.status.localeCompare(b.status);
        break;
      case "likes":
        cmp = (a.likes || 0) - (b.likes || 0);
        break;
      case "comments":
        cmp = (a.comments || 0) - (b.comments || 0);
        break;
      case "shares":
        cmp = (a.shares || 0) - (b.shares || 0);
        break;
    }
    
    return sortDir === "asc" ? cmp : -cmp;
  });
  
  return sorted;
}

export function exportToCSV(posts: PostItem[]): string {
  const headers = [
    "ID",
    "Title",
    "Brand",
    "Platform",
    "Type",
    "Status",
    "Published Date",
    "Likes",
    "Comments",
    "Shares",
    "Caption",
    "Error Message"
  ];
  
  const rows = posts.map((post) => [
    post.id,
    `"${(post.contentTitle || "").replace(/"/g, '""')}"`,
    `"${(post.brandName || "").replace(/"/g, '""')}"`,
    post.platform || "",
    post.type || "",
    post.status,
    formatDateTime(post.publishedAt),
    post.likes || 0,
    post.comments || 0,
    post.shares || 0,
    `"${(post.caption || "").replace(/"/g, '""')}"`,
    `"${(post.errorMessage || "").replace(/"/g, '""')}"`
  ]);
  
  const csvContent = [
    headers.join(","),
    ...rows.map(row => row.join(","))
  ].join("\n");
  
  return csvContent;
}
