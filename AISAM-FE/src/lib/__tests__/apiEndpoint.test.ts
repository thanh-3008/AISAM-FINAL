import { describe, expect, it, vi, beforeEach, afterEach, Mock } from "vitest";
import {
  getActiveApiUrl,
  setActiveApiUrl,
  switchActiveApiUrl,
  resetToPrimaryApiUrl,
  buildTargetUrl,
  fetchWithFailover,
  getApiEndpoints,
} from "../apiEndpoint";

describe("apiEndpoint", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();
    resetToPrimaryApiUrl();
    global.fetch = vi.fn() as unknown as typeof fetch;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("retrieves configured endpoints with defaults", () => {
    const endpoints = getApiEndpoints();
    expect(endpoints.length).toBeGreaterThanOrEqual(2);
    expect(endpoints[0]).toBe("http://localhost:5027/api");
    expect(endpoints[1]).toBe("https://aisam.ddns.net/api");
  });

  it("buildTargetUrl formats relative and absolute URLs correctly", () => {
    const base = "https://api.aisam.io.vn/api";
    expect(buildTargetUrl("/auth/login", base)).toBe("https://api.aisam.io.vn/api/auth/login");
    expect(buildTargetUrl("auth/login", base)).toBe("https://api.aisam.io.vn/api/auth/login");

    // When passing an absolute URL with the fallback base, swaps to target base
    expect(buildTargetUrl("https://aisam.ddns.net/api/auth/login", base)).toBe("https://api.aisam.io.vn/api/auth/login");
  });

  it("switches active API url and persists to sessionStorage", () => {
    const initial = getActiveApiUrl();
    const switched = switchActiveApiUrl();

    expect(switched).not.toBe(initial);
    expect(getActiveApiUrl()).toBe(switched);
    expect(sessionStorage.getItem("aisam_active_api_url")).toBe(switched);
  });

  it("succeeds on primary endpoint without triggering failover", async () => {
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: true,
      status: 200,
      json: async () => ({ success: true }),
    });

    const response = await fetchWithFailover("/test");
    expect(response.status).toBe(200);
    expect(global.fetch).toHaveBeenCalledTimes(1);
    expect((global.fetch as Mock).mock.calls[0][0]).toContain("5027");
  });

  it("automatically fails over to backup endpoint on network error", async () => {
    // Attempt 1: Network failure (DNS error, connection refused)
    (global.fetch as Mock).mockRejectedValueOnce(new TypeError("Failed to fetch"));
    // Attempt 2: Backup endpoint succeeds
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: true,
      status: 200,
      json: async () => ({ success: true, backup: true }),
    });

    const response = await fetchWithFailover("/content");
    expect(response.status).toBe(200);
    expect(global.fetch).toHaveBeenCalledTimes(2);

    // Call 1 was to primary, Call 2 was to fallback
    expect((global.fetch as Mock).mock.calls[0][0]).toContain("5027");
    expect((global.fetch as Mock).mock.calls[1][0]).toContain("ddns.net");

    // Active API is now the fallback URL
    expect(getActiveApiUrl()).toContain("ddns.net");
    expect(sessionStorage.getItem("aisam_active_api_url")).toContain("ddns.net");
  });

  it("automatically fails over on HTTP 502 / 503 / 504 gateway errors", async () => {
    // Attempt 1: HTTP 502 Bad Gateway
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: false,
      status: 502,
      statusText: "Bad Gateway",
    });
    // Attempt 2: Fallback succeeds
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: true,
      status: 200,
      statusText: "OK",
    });

    const response = await fetchWithFailover("/analytics");
    expect(response.status).toBe(200);
    expect(global.fetch).toHaveBeenCalledTimes(2);
    expect(getActiveApiUrl()).toContain("ddns.net");
  });

  it("does not failover when caller signal is intentionally aborted", async () => {
    const controller = new AbortController();
    controller.abort(new DOMException("User cancelled", "AbortError"));

    await expect(
      fetchWithFailover("/test", { signal: controller.signal })
    ).rejects.toThrow();

    expect(global.fetch).toHaveBeenCalledTimes(0);
  });
});
