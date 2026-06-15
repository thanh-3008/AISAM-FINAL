import { PostItem } from "@/services/postService";

export function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
}

export function getStatusStyle(status: string): string {
  const styles: Record<string, string> = {
    Published: "bg-emerald-50 text-emerald-600 border-emerald-500/20",
    Draft: "bg-gray-50 text-gray-600 border-gray-500/20",
    PendingApproval: "bg-amber-50 text-amber-600 border-amber-500/20",
    Approved: "bg-sky-50 text-sky-600 border-sky-500/20",
    Rejected: "bg-danger-red/10 text-danger-red border-danger-red/20",
  };
  return styles[status] || "bg-gray-50 text-gray-600 border-gray-500/20";
}

export function sortPosts(
  posts: PostItem[],
  sortKey: "publishedAt" | "contentTitle" | "brandName" | "status",
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
    }

    return sortDir === "asc" ? cmp : -cmp;
  });

  return sorted;
}