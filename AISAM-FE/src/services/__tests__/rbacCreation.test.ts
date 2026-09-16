import { beforeEach, expect, it, vi } from "vitest";
import { apiClient } from "@/lib/apiClient";
import { chatWithAI } from "../contentService";
import { updateAutomationItem } from "../automationService";
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(), apiFetch: vi.fn() }));
beforeEach(() => { vi.mocked(apiClient).mockReset(); });

it("sends the selected Team with AI generation and returns denied requests as errors", async () => {
  vi.mocked(apiClient).mockRejectedValue(new Error("Team access denied"));
  const result = await chatWithAI("Create an ad", 0, "brand", undefined, undefined, [], { teamId: "team-a" });
  expect(apiClient).toHaveBeenCalledWith("/ai/chat", expect.objectContaining({ data: expect.objectContaining({ teamId: "team-a", brandId: "brand" }) }));
  expect(result).toEqual({ errorMessage: "Team access denied" });
  expect(result?.createdContentId).toBeUndefined();
});

it("sends automation Team scope and preserves a failed save for the caller", async () => {
  vi.mocked(apiClient).mockRejectedValue(new Error("Team cannot be changed"));
  await expect(updateAutomationItem("plan", "row", { brandId: "brand", teamId: "team-b", topic: "New ad", platform: "facebook", contentType: "Text", scheduledAt: "2026-10-01T10:00:00Z" })).rejects.toThrow("Team cannot be changed");
  expect(apiClient).toHaveBeenCalledWith("/automation-plans/plan/items/row", expect.objectContaining({ method: "PUT", data: expect.objectContaining({ teamId: "team-b" }) }));
});
