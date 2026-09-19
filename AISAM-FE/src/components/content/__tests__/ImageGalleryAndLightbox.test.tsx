import React from "react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render } from "@testing-library/react";
import ImageGalleryView from "@/components/content/ImageGalleryView";
import ImageLightboxModal from "@/components/content/ImageLightboxModal";
import { parseMultipleImageUrls } from "@/services/contentService";

afterEach(() => {
  cleanup();
  vi.restoreAllMocks();
});

describe("parseMultipleImageUrls utility", () => {
  it("parses single string url correctly", () => {
    const url = "https://images.unsplash.com/photo-1";
    expect(parseMultipleImageUrls(url)).toEqual([url]);
  });

  it("parses JSON array of urls correctly", () => {
    const raw = JSON.stringify([
      "https://images.unsplash.com/photo-1",
      "https://images.unsplash.com/photo-2",
      "https://images.unsplash.com/photo-3",
    ]);
    expect(parseMultipleImageUrls(raw)).toEqual([
      "https://images.unsplash.com/photo-1",
      "https://images.unsplash.com/photo-2",
      "https://images.unsplash.com/photo-3",
    ]);
  });

  it("parses JSON object with urls property", () => {
    const raw = JSON.stringify({
      urls: ["https://img1.jpg", "https://img2.jpg"],
    });
    expect(parseMultipleImageUrls(raw)).toEqual(["https://img1.jpg", "https://img2.jpg"]);
  });

  it("handles empty or invalid inputs gracefully", () => {
    expect(parseMultipleImageUrls(null)).toEqual([]);
    expect(parseMultipleImageUrls(undefined)).toEqual([]);
    expect(parseMultipleImageUrls("")).toEqual([]);
    expect(parseMultipleImageUrls("   ")).toEqual([]);
    expect(parseMultipleImageUrls("[]")).toEqual([]);
  });
});

describe("ImageGalleryView Component", () => {
  const sampleImages = [
    "https://img.example.com/1.jpg",
    "https://img.example.com/2.jpg",
    "https://img.example.com/3.jpg",
  ];

  it("renders cover badge and thumbnail strip for multi-image gallery", () => {
    const { getByText, getAllByText, getByAltText } = render(
      <ImageGalleryView images={sampleImages} title="Test Post" />
    );

    expect(getAllByText("Cover").length).toBeGreaterThan(0);
    expect(getByText("1 / 3")).toBeTruthy();
    expect(getByAltText("Test Post")).toBeTruthy();
    expect(getByText("All uploaded images (3)")).toBeTruthy();
  });

  it("navigates between images with next and prev buttons", () => {
    const { getByText, getByTitle } = render(
      <ImageGalleryView images={sampleImages} title="Test Post" />
    );

    const nextBtn = getByTitle("Next image");
    fireEvent.click(nextBtn);
    expect(getByText("2 / 3")).toBeTruthy();

    const prevBtn = getByTitle("Previous image");
    fireEvent.click(prevBtn);
    expect(getByText("1 / 3")).toBeTruthy();
  });

  it("opens lightbox when clicking on main image or fullscreen button", () => {
    const { getByText } = render(
      <ImageGalleryView images={sampleImages} title="Test Post" />
    );

    const fullscreenBtn = getByText("View full screen");
    fireEvent.click(fullscreenBtn);

    // Lightbox header counter
    expect(getByText("Image 1 / 3")).toBeTruthy();
  });
});

describe("ImageLightboxModal Component", () => {
  const sampleImages = [
    "https://img.example.com/1.jpg",
    "https://img.example.com/2.jpg",
  ];

  it("renders when open and responds to close button", () => {
    const handleClose = vi.fn();
    const { getByTitle } = render(
      <ImageLightboxModal
        images={sampleImages}
        isOpen={true}
        onClose={handleClose}
        initialIndex={0}
        title="Sample Gallery"
      />
    );

    const closeBtn = getByTitle("Close (Esc)");
    fireEvent.click(closeBtn);
    expect(handleClose).toHaveBeenCalledTimes(1);
  });

  it("handles zoom controls", () => {
    const { getByTitle } = render(
      <ImageLightboxModal
        images={sampleImages}
        isOpen={true}
        onClose={vi.fn()}
        initialIndex={0}
      />
    );

    const zoomInBtn = getByTitle("Zoom in (+)");
    fireEvent.click(zoomInBtn);
    expect(getByTitle("Zoom out (-)")).toBeTruthy();
  });
});
