const DEFAULT_API_URL = "http://localhost:5116/api";

export function getApiBaseUrl() {
  const rawUrl = process.env.NEXT_PUBLIC_API_URL || DEFAULT_API_URL;
  const trimmedUrl = rawUrl.replace(/\/+$/, "");
  return trimmedUrl.endsWith("/api") ? trimmedUrl : `${trimmedUrl}/api`;
}
