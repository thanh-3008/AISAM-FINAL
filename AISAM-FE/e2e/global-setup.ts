import { execFileSync, spawn } from "node:child_process";
import type { FullConfig } from "@playwright/test";

export default async function globalSetup(config: FullConfig) {
  if (process.env.E2E_BASE_URL) return;

  const baseURL = String(config.projects[0].use.baseURL);
  const url = new URL(baseURL);
  const child = spawn(
    process.execPath,
    ["node_modules/next/dist/bin/next", "start", "--hostname", url.hostname, "--port", url.port],
    {
      cwd: process.cwd(),
      env: { ...process.env, NEXT_PUBLIC_API_URL: "http://api.aisam.e2e/api" },
      detached: false,
      stdio: "ignore",
      windowsHide: true,
    },
  );

  const deadline = Date.now() + 60_000;
  while (Date.now() < deadline) {
    if (child.exitCode !== null) throw new Error("AISAM production server exited before E2E started. Run `npm run build` first.");
    try {
      const response = await fetch(baseURL);
      if (response.ok) break;
    } catch {}
    await new Promise((resolve) => setTimeout(resolve, 250));
  }
  if (Date.now() >= deadline) throw new Error(`Timed out waiting for AISAM at ${baseURL}`);

  return async () => {
    if (child.exitCode !== null || !child.pid) return;
    if (process.platform === "win32") {
      try { execFileSync("taskkill", ["/pid", String(child.pid), "/T", "/F"], { stdio: "ignore" }); } catch {}
    } else {
      child.kill("SIGTERM");
    }
  };
}
