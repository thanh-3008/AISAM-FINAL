import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, render, screen } from "@testing-library/react";
import PostNowModal from "../content/PostNowModal";
vi.mock("@/hooks/useResourcePermissions", () => ({ useResourcePermissions: () => (index: number) => index === 0 }));
vi.mock("@/services/socialAccountService", () => ({ fetchSocialIntegrations: async () => [
  { id: "allowed", accountName: "Allowed channel", provider: "facebook", isActive: true },
  { id: "denied", accountName: "View only channel", provider: "facebook", isActive: true },
] }));
vi.mock("@/services/contentService", () => ({ fetchContentById: async () => ({ type: "TEXT" }), publishContent: vi.fn() }));
afterEach(cleanup);
it("does not offer a visible channel without permission to publish this content", async () => {
  render(<PostNowModal contentId="content" brandId="brand" onClose={() => {}} onSuccess={() => {}} />);
  await screen.findByText("Allowed channel");
  expect(screen.queryByText("View only channel")).toBeNull();
  expect(screen.getAllByRole("checkbox")).toHaveLength(1);
});
