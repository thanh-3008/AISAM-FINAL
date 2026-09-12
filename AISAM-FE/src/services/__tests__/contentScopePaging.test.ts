import { expect, it, vi } from "vitest";
import { apiClient } from "@/lib/apiClient";
import { fetchAllVisibleContents } from "../contentService";
vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn(), apiFetch: vi.fn() }));
it("keeps the server scope filter across every loaded page", async () => {
  const item = { id: "one", brandId: "brand", title: "Draft", textContent: "Text", adType: 0, status: 0, tags: null, createdAt: "2026-09-08" };
  vi.mocked(apiClient).mockResolvedValueOnce({ data: { data: [item], totalCount: 2, page: 1, pageSize: 100 } })
    .mockResolvedValueOnce({ data: { data: [{ ...item, id: "two" }], totalCount: 2, page: 2, pageSize: 100 } });
  const result = await fetchAllVisibleContents({ mine: true });
  expect(result?.items.map(item => item.id)).toEqual(["one", "two"]);
  expect(apiClient).toHaveBeenNthCalledWith(1, expect.stringMatching(/\/content\?.*page=1.*mine=true/));
  expect(apiClient).toHaveBeenNthCalledWith(2, expect.stringMatching(/\/content\?.*page=2.*mine=true/));
});
