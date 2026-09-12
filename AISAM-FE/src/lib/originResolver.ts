import type { NextRequest } from "next/server";

function parseConfiguredOrigins(): string[] {
  const envList = process.env.NEXT_PUBLIC_ALLOWED_ORIGINS;
  if (envList) {
    const list = envList.split(",").map(normalizeOrigin).filter(Boolean);
    if (!list.includes("http://localhost:3000")) list.push("http://localhost:3000");
    if (!list.includes("https://aisam.ddns.net")) list.push("https://aisam.ddns.net");
    return list;
  }
  return [
    "http://localhost:3000",
    "https://aisam.io.vn",
    "https://www.aisam.io.vn",
    "https://aisam.ddns.net",
  ];
}

export const ALLOWED_ORIGINS = parseConfiguredOrigins();

export function getDefaultOrigin(): string {
  return (
    process.env.NEXT_PUBLIC_DEFAULT_ORIGIN?.trim().replace(/\/+$/, "") ||
    process.env.NEXT_PUBLIC_APP_URL?.trim().replace(/\/+$/, "") ||
    process.env.FRONTEND_BASE_URL?.trim().replace(/\/+$/, "") ||
    "https://aisam.io.vn"
  );
}

function normalizeOrigin(origin: string): string {
  try {
    const url = new URL(origin.trim());
    return `${url.protocol}//${url.host}`.toLowerCase();
  } catch {
    return origin.trim().replace(/\/+$/, "").toLowerCase();
  }
}

export function isAllowedOrigin(origin: string): boolean {
  if (!origin) return false;
  const normalized = normalizeOrigin(origin);
  const currentAllowed = parseConfiguredOrigins();
  return currentAllowed.includes(normalized);
}

/**
 * Resolves the current origin on the client side (browser) with fallback for SSR.
 */
export function resolveClientOrigin(): string {
  if (typeof window !== "undefined" && window.location?.origin) {
    const origin = normalizeOrigin(window.location.origin);
    if (isAllowedOrigin(origin)) {
      return origin;
    }
    return window.location.origin.replace(/\/+$/, "");
  }

  const envOrigin = process.env.NEXT_PUBLIC_APP_URL || process.env.FRONTEND_BASE_URL;
  if (envOrigin) {
    return normalizeOrigin(envOrigin);
  }

  return getDefaultOrigin();
}

/**
 * Resolves the client origin on the Next.js server side (Route Handlers/Middleware)
 * by inspecting X-Forwarded-Host, Host, or NextRequest nextUrl.
 */
export function resolveServerOrigin(request: NextRequest): string {
  // 1. Check X-Forwarded-Host (from Nginx)
  const forwardedHost = request.headers.get("x-forwarded-host");
  if (forwardedHost) {
    const host = forwardedHost.split(",")[0].trim();
    const proto = request.headers.get("x-forwarded-proto")?.split(",")[0].trim() || "https";
    const candidate = normalizeOrigin(`${proto}://${host}`);
    if (isAllowedOrigin(candidate)) {
      return candidate;
    }
  }

  // 2. Check Host header
  const host = request.headers.get("host");
  if (host) {
    const proto = request.headers.get("x-forwarded-proto")?.split(",")[0].trim() || (request.nextUrl.protocol.replace(":", "") || "https");
    const candidate = normalizeOrigin(`${proto}://${host}`);
    if (isAllowedOrigin(candidate)) {
      return candidate;
    }
  }

  // 3. Fallback to request nextUrl origin
  if (request.nextUrl?.origin) {
    const candidate = normalizeOrigin(request.nextUrl.origin);
    if (isAllowedOrigin(candidate)) {
      return candidate;
    }
  }

  return getDefaultOrigin();
}
