import { describe, it, expect } from "vitest";
import { isFailedContentStatus, isRejectedContentStatus } from "@/lib/contentConstants";
import { mapContentApiStatus } from "@/services/contentService";

describe("content status aggregation", () => {
  const items = [
    { status: "Failed" },
    { status: "Rejected" },
    { status: "Rejected" },
    { status: "Approved" },
  ];

  it("counts only Failed in the failed bucket", () => {
    const failedCount = items.filter((item) => isFailedContentStatus(item.status)).length;

    expect(failedCount).toBe(1);
  });

  it("counts only rejected workflow statuses in the rejected bucket", () => {
    const rejectedCount = items.filter((item) => isRejectedContentStatus(item.status)).length;

    expect(rejectedCount).toBe(2);
  });

  it("keeps Rejected and Failed mutually exclusive", () => {
    expect(isRejectedContentStatus("Rejected")).toBe(true);
    expect(isRejectedContentStatus("Failed")).toBe(false);
    expect(isFailedContentStatus("Failed")).toBe(true);
    expect(isFailedContentStatus("Rejected")).toBe(false);
  });

  it("maps rejected, platform-rejected, and publishing-failed API statuses to separate categories", () => {
    expect(mapContentApiStatus(3)).toBe("Rejected");
    expect(mapContentApiStatus(6)).toBe("Rejected");
    expect(mapContentApiStatus(7)).toBe("Failed");
  });
});
