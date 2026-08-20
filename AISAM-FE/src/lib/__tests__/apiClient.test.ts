import { describe, expect, it, vi, beforeEach, afterEach, Mock } from "vitest";
import { apiClient } from "../apiClient";
import * as auth from "../auth";

vi.mock("../auth", () => ({
  getToken: vi.fn(),
  refreshAccessToken: vi.fn(),
  removeToken: vi.fn(),
  removeRefreshToken: vi.fn(),
  ensureValidToken: vi.fn(),
}));

// Mock workspace store
vi.mock("@/stores/workspace-store", () => ({
  getStoredActiveWorkspace: vi.fn(() => ({ id: "12345678-1234-1234-1234-123456789012" })),
  clearActiveWorkspace: vi.fn(),
}));

// Mock profile store
vi.mock("@/stores/profile-store", () => ({
  getStoredActiveProfile: vi.fn(() => ({ id: "87654321-4321-4321-4321-210987654321" })),
}));

describe("apiClient", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn() as unknown as typeof fetch;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("sends request with correct headers on happy path", async () => {
    (auth.getToken as Mock).mockReturnValue("test-token");
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: true,
      text: async () => JSON.stringify({ success: true, data: "test" }),
    });

    const result = await apiClient("/test");

    expect(result).toEqual({ success: true, data: "test" });
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/test"),
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: "Bearer test-token",
          "X-Workspace-Id": "12345678-1234-1234-1234-123456789012",
          "X-Profile-Id": "87654321-4321-4321-4321-210987654321",
        }),
      })
    );
  });

  it("translates error messages using ERROR_MAP", async () => {
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: false,
      status: 403,
      text: async () => JSON.stringify({ error: "Missing or invalid X-Workspace-Id header." }),
    });

    await expect(apiClient("/test")).rejects.toThrow("Chưa chọn Workspace. Vào Overview để chọn workspace.");
  });

  it("retries with refresh token on 401 if not login/refresh endpoint", async () => {
    (auth.getToken as Mock).mockReturnValue("old-token");
    
    // First call returns 401
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: false,
      status: 401,
      text: async () => JSON.stringify({ error: "Unauthorized" }),
    });

    // Refresh succeeds
    (auth.refreshAccessToken as Mock).mockResolvedValueOnce("new-token");

    // Second call returns 200
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: true,
      text: async () => JSON.stringify({ success: true }),
    });

    const result = await apiClient("/test");

    expect(result).toEqual({ success: true });
    expect(auth.refreshAccessToken).toHaveBeenCalled();
    expect(global.fetch).toHaveBeenCalledTimes(2);
    // Second fetch should use new token
    expect(global.fetch).toHaveBeenLastCalledWith(
      expect.stringContaining("/test"),
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: "Bearer new-token",
        }),
      })
    );
  });

  it("throws error and clears token if refresh token fails on 401", async () => {
    // Mock window location to prevent the hanging promise in apiClient
    const originalLocation = window.location;
    // @ts-expect-error - deleting window.location is required for mocking
    delete window.location;
    window.location = { ...originalLocation, pathname: "/login", replace: vi.fn() } as any;

    (auth.getToken as Mock).mockReturnValue("old-token");
    
    // First call returns 401
    (global.fetch as Mock).mockResolvedValueOnce({
      ok: false,
      status: 401,
      text: async () => JSON.stringify({ error: "Unauthorized" }),
    });

    // Refresh fails
    (auth.refreshAccessToken as Mock).mockResolvedValueOnce(null);

    await expect(apiClient("/test")).rejects.toThrow("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.");
    expect(auth.removeToken).toHaveBeenCalled();
    expect(auth.removeRefreshToken).toHaveBeenCalled();
    // Restore window.location
    window.location = originalLocation as any;
  });
});
