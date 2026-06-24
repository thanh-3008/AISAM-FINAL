const DEFAULT_API_URL = "http://localhost:5116/api";

export function getApiBaseUrl() {
  const rawUrl = process.env.NEXT_PUBLIC_API_URL || DEFAULT_API_URL;
  const trimmedUrl = rawUrl.replace(/\/+$/, "");
  return trimmedUrl.endsWith("/api") ? trimmedUrl : `${trimmedUrl}/api`;
}

export function getApiOrigin() {
  return getApiBaseUrl().replace(/\/api$/i, "");
}

export function resolveApiMediaUrl(url?: string | null) {
  if (!url) return "";
  let value = url.trim();
  if (!value) return "";
  if (value.startsWith("[") && value.endsWith("]")) {
    try {
      const parsed = JSON.parse(value);
      if (Array.isArray(parsed)) {
        value = parsed.find((item) => typeof item === "string" && item.trim().length > 0)?.trim() ?? "";
      }
    } catch {
      return "";
    }
  }
  if (!value || value.startsWith("blob:")) return "";
  if (/^(data:|https?:\/\/)/i.test(value)) return value;
  const normalized = value.replace(/\\/g, "/");
  return `${getApiOrigin()}${normalized.startsWith("/") ? normalized : `/${normalized}`}`;
}
