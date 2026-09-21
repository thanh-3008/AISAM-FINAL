import { afterEach, describe, expect, it } from "vitest";
import { NextRequest } from "next/server";
import { GET } from "./route";

const originalCallbackUrl = process.env.TIKTOK_LOCAL_CALLBACK_URL;

afterEach(() => {
  if (originalCallbackUrl === undefined) {
    delete process.env.TIKTOK_LOCAL_CALLBACK_URL;
  } else {
    process.env.TIKTOK_LOCAL_CALLBACK_URL = originalCallbackUrl;
  }
});

describe("TikTok callback relay", () => {
  it("redirects the public ngrok callback to localhost with its query", () => {
    process.env.TIKTOK_LOCAL_CALLBACK_URL = "http://localhost:3000/social-callback/tiktok";
    const request = new NextRequest(
      "https://example.ngrok-free.app/social-callback/tiktok?code=oauth-code&state=oauth-state",
    );

    const response = GET(request);

    expect(response.status).toBe(302);
    expect(response.headers.get("location")).toBe(
      "http://localhost:3000/social-callback/tiktok?code=oauth-code&state=oauth-state",
    );
    expect(response.headers.get("cache-control")).toBe("no-store");
  });

  it("renders the callback processor after returning to localhost", async () => {
    process.env.TIKTOK_LOCAL_CALLBACK_URL = "http://localhost:3000/social-callback/tiktok";
    const request = new NextRequest(
      "http://localhost:3000/social-callback/tiktok?code=oauth-code&state=oauth-state",
    );

    const response = GET(request);

    expect(response.status).toBe(200);
    const html = await response.text();
    expect(html).toContain("Processing TikTok authorization");
    expect(html).toContain("'X-RBAC-Contract-Version': '2'");
  });

  it("relays callback to DDNS domain when state Origin is aisam.ddns.net", () => {
    const payload = Buffer.from(
      JSON.stringify({
        State: "test-state-1234",
        ProfileId: "00000000-0000-0000-0000-000000000001",
        Provider: "tiktok",
        Origin: "https://aisam.ddns.net",
        RedirectUri: "https://aisam.ddns.net/social-callback/tiktok",
      }),
    ).toString("base64url");
    const state = `${payload}.mock_signature`;

    // TikTok callback hits the primary domain (e.g. registered redirect URI on TikTok Developer Portal)
    const request = new NextRequest(
      `https://aisam.io.vn/social-callback/tiktok?code=test-auth-code&state=${state}`,
    );

    const response = GET(request);

    expect(response.status).toBe(302);
    expect(response.headers.get("location")).toBe(
      `https://aisam.ddns.net/social-callback/tiktok?code=test-auth-code&state=${state}`,
    );
    expect(response.headers.get("cache-control")).toBe("no-store");
  });

  it("renders callback processor when accessed directly on aisam.ddns.net matching state origin", async () => {
    const payload = Buffer.from(
      JSON.stringify({
        State: "test-state-1234",
        ProfileId: "00000000-0000-0000-0000-000000000001",
        Provider: "tiktok",
        Origin: "https://aisam.ddns.net",
        RedirectUri: "https://aisam.ddns.net/social-callback/tiktok",
      }),
    ).toString("base64url");
    const state = `${payload}.mock_signature`;

    // Request on aisam.ddns.net with matching state origin
    const request = new NextRequest(
      `https://aisam.ddns.net/social-callback/tiktok?code=test-auth-code&state=${state}`,
    );

    const response = GET(request);

    expect(response.status).toBe(200);
    const html = await response.text();
    expect(html).toContain("Processing TikTok authorization");
    expect(html).toContain("aisam.ddns.net");
    expect(html).toContain("fallbackApiBaseUrl");
  });
});
