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
    expect(await response.text()).toContain("Processing TikTok authorization");
  });
});
