import { describe, it, expect } from "vitest";
import { isFailedOrRejectedStatus } from "@/lib/contentConstants";

describe("content status aggregation", () => {
  it("counts both Failed and Rejected as the failed bucket", () => {
    const items = [
      { status: "Failed" },
      { status: "Rejected" },
      { status: "Approved" },
    ];

    const failedCount = items.filter((item) => isFailedOrRejectedStatus(item.status)).length;

    expect(failedCount).toBe(2);
  });

  it("keeps Rejected as a distinct status when filtered individually", () => {
    expect(isFailedOrRejectedStatus("Rejected")).toBe(true);
    expect(isFailedOrRejectedStatus("Failed")).toBe(true);
    expect(isFailedOrRejectedStatus("Approved")).toBe(false);
  });
});
