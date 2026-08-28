import React from "react";
import { afterEach, describe, expect, it } from "vitest";
import { cleanup, fireEvent, render } from "@testing-library/react";
import VideoPreview from "@/components/content/VideoPreview";

afterEach(cleanup);

describe("VideoPreview", () => {
  it("renders the selected video source with controls and poster", () => {
    const { getByLabelText } = render(<VideoPreview src="blob:demo-video" poster="/poster.jpg" />);
    const video = getByLabelText("Selected video preview") as HTMLVideoElement;

    expect(video.getAttribute("src")).toBe("blob:demo-video");
    expect(video.getAttribute("poster")).toBe("/poster.jpg");
    expect(video.controls).toBe(true);
  });

  it("shows a graceful error state when the browser cannot load the video", () => {
    const { getByLabelText, getByText } = render(<VideoPreview src="blob:unsupported-video" />);
    fireEvent.error(getByLabelText("Selected video preview"));

    expect(getByText(/cannot be previewed/i)).toBeTruthy();
  });

  it("shows an unavailable state when no source exists", () => {
    const { getByText } = render(<VideoPreview src={null} />);
    expect(getByText("Video preview is unavailable.")).toBeTruthy();
  });
});
