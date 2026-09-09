import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, expect, it, vi } from "vitest";
import { useResourcePermissions } from "../useResourcePermissions";
import { checkPermissions } from "@/services/permissionService";
vi.mock("@/services/permissionService", () => ({ checkPermissions: vi.fn() }));
afterEach(cleanup);
it("fails closed and ignores a late permission result for the previous resource", async () => {
  let finish!: (value: boolean[]) => void;
  vi.mocked(checkPermissions).mockImplementationOnce(() => new Promise(resolve => { finish = resolve; })).mockResolvedValueOnce([false]);
  const { result, rerender } = renderHook(({ id }) => useResourcePermissions([{ kind: 2, resourceId: id, permission: 4 }]), { initialProps: { id: "A" } });
  expect(result.current(0)).toBe(false);
  rerender({ id: "B" });
  await waitFor(() => expect(checkPermissions).toHaveBeenCalledTimes(2));
  await act(async () => finish([true]));
  expect(result.current(0)).toBe(false);
});
