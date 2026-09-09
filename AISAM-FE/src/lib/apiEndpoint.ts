/**
 * Dual-API Endpoint Manager with Automatic Failover
 * 
 * Manages active API endpoint between Primary (e.g. https://api.aisam.io.vn/api)
 * and Fallback (e.g. https://aisam.ddns.net/api).
 * Automatically fails over on network error, DNS resolution failure, connection timeout,
 * or 502/503/504 gateway errors.
 */

const STORAGE_KEY = "aisam_active_api_url";
const DEFAULT_CONNECT_TIMEOUT_MS = 8000;

function cleanUrl(url: string): string {
  return url.trim().replace(/\/+$/, "");
}

export function getApiEndpoints(): string[] {
  const endpoints: string[] = [];

  // 1. Support comma-separated list
  const listEnv = process.env.NEXT_PUBLIC_API_URLS;
  if (listEnv) {
    for (const item of listEnv.split(",")) {
      const cleaned = cleanUrl(item);
      if (cleaned && !endpoints.includes(cleaned)) {
        endpoints.push(cleaned);
      }
    }
  }

  // 2. Primary endpoint
  const primary = cleanUrl(process.env.NEXT_PUBLIC_API_URL || "http://localhost:5027/api");
  if (!endpoints.includes(primary)) {
    endpoints.push(primary);
  }

  // 3. Fallback endpoint
  const fallback = cleanUrl(process.env.NEXT_PUBLIC_FALLBACK_API_URL || "https://aisam.ddns.net/api");
  if (!endpoints.includes(fallback)) {
    endpoints.push(fallback);
  }

  return endpoints;
}

const endpoints = getApiEndpoints();
let currentActiveUrl = endpoints[0] || "http://localhost:5027/api";

export function getActiveApiUrl(): string {
  if (typeof window !== "undefined") {
    try {
      const saved = sessionStorage.getItem(STORAGE_KEY);
      if (saved) {
        const cleaned = cleanUrl(saved);
        const all = getApiEndpoints();
        if (all.includes(cleaned)) {
          currentActiveUrl = cleaned;
          API_URL = cleaned;
          return cleaned;
        }
      }
    } catch {
      // Ignore sessionStorage read errors
    }
  }
  return currentActiveUrl;
}

export function setActiveApiUrl(url: string): void {
  const cleaned = cleanUrl(url);
  currentActiveUrl = cleaned;
  API_URL = cleaned;
  if (typeof window !== "undefined") {
    try {
      sessionStorage.setItem(STORAGE_KEY, cleaned);
    } catch {
      // Ignore sessionStorage write errors
    }
  }
}

export function switchActiveApiUrl(): string {
  const all = getApiEndpoints();
  if (all.length <= 1) {
    return currentActiveUrl;
  }

  const currentIndex = all.indexOf(currentActiveUrl);
  const nextIndex = (currentIndex + 1) % all.length;
  const nextUrl = all[nextIndex];

  setActiveApiUrl(nextUrl);
  if (typeof console !== "undefined") {
    console.warn(`[API Failover] Switched active API endpoint to: ${nextUrl}`);
  }
  return nextUrl;
}

export function resetToPrimaryApiUrl(): void {
  const all = getApiEndpoints();
  if (all.length > 0) {
    setActiveApiUrl(all[0]);
  }
}

export let API_URL: string = getActiveApiUrl();

/**
 * Normalizes an endpoint or full URL against a given base URL.
 */
export function buildTargetUrl(pathOrUrl: string, baseUrl: string): string {
  const cleanedBase = cleanUrl(baseUrl);
  const trimmed = pathOrUrl.trim();

  // If pathOrUrl is already an absolute URL starting with one of our known endpoints, swap base
  for (const ep of getApiEndpoints()) {
    if (trimmed.startsWith(ep)) {
      const relative = trimmed.slice(ep.length);
      return `${cleanedBase}${relative.startsWith("/") ? relative : `/${relative}`}`;
    }
  }

  // If absolute URL with other scheme, keep as is
  if (/^https?:\/\//i.test(trimmed)) {
    return trimmed;
  }

  // Relative path
  return `${cleanedBase}${trimmed.startsWith("/") ? trimmed : `/${trimmed}`}`;
}

export interface FetchWithFailoverOptions {
  timeoutMs?: number;
  disableFailover?: boolean;
}

/**
 * Execute fetch with automatic failover to the fallback endpoint if the primary fails.
 */
export async function fetchWithFailover(
  pathOrUrl: string,
  init?: RequestInit,
  options?: FetchWithFailoverOptions
): Promise<Response> {
  const allEndpoints = getApiEndpoints();
  const maxAttempts = options?.disableFailover ? 1 : Math.min(allEndpoints.length, 3);
  const timeoutMs = options?.timeoutMs ?? DEFAULT_CONNECT_TIMEOUT_MS;

  let lastError: unknown = null;
  let lastResponse: Response | null = null;

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    const activeBase = getActiveApiUrl();
    const targetUrl = buildTargetUrl(pathOrUrl, activeBase);

    // Timeout controller linked with caller signal
    const timeoutController = new AbortController();
    let isTimeout = false;
    const timeoutId = setTimeout(() => {
      isTimeout = true;
      timeoutController.abort(new DOMException("Connection timeout", "TimeoutError"));
    }, timeoutMs);

    const callerSignal = init?.signal;
    if (callerSignal?.aborted) {
      clearTimeout(timeoutId);
      throw callerSignal.reason || new DOMException("The user aborted a request.", "AbortError");
    }

    const onCallerAbort = () => {
      timeoutController.abort(callerSignal?.reason);
    };
    callerSignal?.addEventListener("abort", onCallerAbort, { once: true });

    try {
      const response = await fetch(targetUrl, {
        ...init,
        signal: timeoutController.signal,
      });

      clearTimeout(timeoutId);
      callerSignal?.removeEventListener("abort", onCallerAbort);

      // Treat 502 Bad Gateway, 503 Service Unavailable, 504 Gateway Timeout as eligible for failover
      if (
        (response.status === 502 || response.status === 503 || response.status === 504) &&
        attempt < maxAttempts - 1
      ) {
        lastResponse = response;
        if (typeof console !== "undefined") {
          console.warn(
            `[API Failover] Endpoint ${activeBase} returned HTTP ${response.status}. Attempting failover...`
          );
        }
        switchActiveApiUrl();
        continue;
      }

      // Success or application-level status (2xx, 4xx, 500, etc.)
      return response;
    } catch (err: any) {
      clearTimeout(timeoutId);
      callerSignal?.removeEventListener("abort", onCallerAbort);

      // If caller intentionally aborted, do not failover!
      if (callerSignal?.aborted && !isTimeout) {
        throw err;
      }

      lastError = err;

      if (attempt < maxAttempts - 1) {
        if (typeof console !== "undefined") {
          console.warn(
            `[API Failover] Endpoint ${activeBase} failed (${err?.message || err}). Attempting failover...`
          );
        }
        switchActiveApiUrl();
        continue;
      }

      break;
    }
  }

  if (lastResponse) {
    return lastResponse;
  }

  throw lastError || new Error("Failed to connect to any API endpoint.");
}
