import { describe, expect, it } from "vitest";
import { isOwnershipTransferCandidate } from "@/lib/ownershipTransfer";

describe("isOwnershipTransferCandidate", () => {
  it("allows an RBAC v2 Workspace Manager", () => {
    expect(isOwnershipTransferCandidate({ workspaceRole: "WorkspaceManager" })).toBe(true);
  });

  it.each([
    { workspaceRole: "Owner" },
    { workspaceRole: "Member" },
    { workspaceRole: null },
    {},
  ])("does not allow a non-Workspace Manager: %o", member => {
    expect(isOwnershipTransferCandidate(member)).toBe(false);
  });
});
