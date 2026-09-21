import React from "react";
import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, expect, it, vi } from "vitest";
import PostRow from "@/components/posts/PostRow";
import type { PostItem } from "@/services/postService";

afterEach(cleanup);

it("shows creator, reviewer and Team attribution for a published post", () => {
  const post: PostItem = {
    id: "post", contentId: "content", integrationId: "integration", externalPostId: null,
    publishedAt: "2026-09-21T00:00:00Z", status: "Published", contentTitle: "Campaign post",
    brandId: "brand", brandName: "Brand A", platform: "facebook", type: "TEXT", caption: "Caption",
    creatorName: "Content Creator", reviewerName: "Team Reviewer", teamName: "Marketing Team",
  };
  render(<table><tbody><PostRow post={post} isSelected={false} onSelect={vi.fn()} onView={vi.fn()} /></tbody></table>);

  expect(screen.getByText("Content Creator")).toBeTruthy();
  expect(screen.getByText("Team Reviewer")).toBeTruthy();
  expect(screen.getByText("Marketing Team")).toBeTruthy();
});
