import { describe, expect, it, beforeEach, vi, afterEach } from "vitest";
import {
  setToken,
  getToken,
  removeToken,
  setRefreshToken,
  getRefreshToken,
  removeRefreshToken,
  setStoredUser,
  getStoredUser,
  removeStoredUser,
  refreshAccessToken,
  isAdmin
} from "../auth";

describe("auth utils", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe("Token Management", () => {
    it("setToken and getToken work correctly", () => {
      expect(getToken()).toBeNull();
      setToken("test-token");
      expect(getToken()).toBe("test-token");
    });

    it("removeToken clears token", () => {
      setToken("test-token");
      removeToken();
      expect(getToken()).toBeNull();
    });
  });

  describe("Refresh Token Management", () => {
    it("setRefreshToken and getRefreshToken work correctly", () => {
      expect(getRefreshToken()).toBeNull();
      setRefreshToken("refresh-test");
      expect(getRefreshToken()).toBe("refresh-test");
    });

    it("removeRefreshToken clears refresh token", () => {
      setRefreshToken("refresh-test");
      removeRefreshToken();
      expect(getRefreshToken()).toBeNull();
    });

    it("shares one rotating refresh request across concurrent callers", async () => {
      setRefreshToken("refresh-old");
      let resolveFetch!: (response: unknown) => void;
      global.fetch = vi.fn(() => new Promise((resolve) => {
        resolveFetch = resolve;
      })) as unknown as typeof fetch;

      const first = refreshAccessToken();
      const second = refreshAccessToken();

      expect(global.fetch).toHaveBeenCalledTimes(1);
      resolveFetch({
        ok: true,
        status: 200,
        json: async () => ({
          success: true,
          data: {
            accessToken: "access-new",
            refreshToken: "refresh-new",
          },
        }),
      });

      await expect(Promise.all([first, second])).resolves.toEqual(["access-new", "access-new"]);
      expect(getToken()).toBe("access-new");
      expect(getRefreshToken()).toBe("refresh-new");
    });
  });

  describe("User Management", () => {
    const mockUser = { id: "1", fullName: "Test User", email: "test@example.com" };

    it("setStoredUser and getStoredUser work correctly", () => {
      expect(getStoredUser()).toBeNull();
      setStoredUser(mockUser);
      expect(getStoredUser()).toEqual(mockUser);
    });

    it("getStoredUser handles invalid JSON", () => {
      localStorage.setItem("aisam_user", "invalid");
      expect(getStoredUser()).toBeNull();
    });

    it("removeStoredUser clears user", () => {
      setStoredUser(mockUser);
      removeStoredUser();
      expect(getStoredUser()).toBeNull();
    });
  });

  describe("isAdmin", () => {
    it("returns false if no token", () => {
      expect(isAdmin()).toBe(false);
    });

    it("returns true if token has Admin role", () => {
      // payload: { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role": "Admin" }
      const payload = btoa(JSON.stringify({
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role": "Admin"
      }));
      setToken(`header.${payload}.signature`);
      expect(isAdmin()).toBe(true);
    });

    it("returns false if token has another role", () => {
      const payload = btoa(JSON.stringify({
        role: "User"
      }));
      setToken(`header.${payload}.signature`);
      expect(isAdmin()).toBe(false);
    });
  });
});
