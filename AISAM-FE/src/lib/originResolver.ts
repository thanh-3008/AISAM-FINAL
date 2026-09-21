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
 * Safely extracts the Origin field from an AISAM signed OAuth state parameter.
 * State format: <base64url_json_payload>.<base64url_hmac_signature>
 * Returns the origin only if it passes the allowed-origins check.
 */
export function extractOriginFromOAuthState(stateParam: string | null | undefined): string | null {
  if (!stateParam) return null;
  try {
    const dotIndex = stateParam.indexOf(".");
    if (dotIndex <= 0) return null;

    const payloadPart = stateParam.substring(0, dotIndex);

    // Base64url → standard Base64
    let base64 = payloadPart.replace(/-/g, "+").replace(/_/g, "/");
    const padding = base64.length % 4;
    if (padding > 0) {
      base64 += "=".repeat(4 - padding);
    }

    // Decode using atob (available in Node.js ≥ 16 and Edge Runtime)
    const jsonStr = atob(base64);
    const payload = JSON.parse(jsonStr);

    if (payload?.Origin && typeof payload.Origin === "string") {
      const normalized = normalizeOrigin(payload.Origin);
      if (normalized && isAllowedOrigin(normalized)) {
        return normalized;
      }
    }
  } catch {
    // State is not valid AISAM JSON or decode failed — ignore safely
  }
  return null;
}

/**
 * Resolves the client origin on the Next.js server side (Route Handlers/Middleware)
 * by inspecting OAuth state, X-Forwarded-Host, Host, or NextRequest nextUrl.
 */
export function resolveServerOrigin(request: NextRequest): string {
  // 0. Highest priority: extract Origin from the signed OAuth state parameter.
  //    The backend embeds the user's actual origin domain into the state when
  //    generating the OAuth authorization URL. This is immune to proxy header
  //    misconfiguration and guarantees correct domain resolution for OAuth callbacks.
  const stateOrigin = extractOriginFromOAuthState(
    request.nextUrl.searchParams.get("state"),
  );
  if (stateOrigin) {
    return stateOrigin;
  }

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
