import React from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, act } from "@testing-library/react";
import RichTextEditor from "@/components/content/RichTextEditor";

afterEach(() => {
  cleanup();
  vi.restoreAllMocks();
  vi.useRealTimers();
});

describe("RichTextEditor Component", () => {
  it("prevents focus stealing by calling preventDefault on mouseDown for toolbar buttons", () => {
    const handleChange = vi.fn();
    const { getByTitle } = render(
      <RichTextEditor value="Hello" richTextJson={null} onChange={handleChange} />
    );

    const boldBtn = getByTitle("Bold (Ctrl+B)");
    expect(boldBtn).toBeTruthy();

    const mouseDownEvent = new MouseEvent("mousedown", {
      bubbles: true,
      cancelable: true,
    });
    const dispatched = boldBtn.dispatchEvent(mouseDownEvent);

    // If e.preventDefault() was called, dispatched is false and defaultPrevented is true
    expect(mouseDownEvent.defaultPrevented).toBe(true);
    expect(dispatched).toBe(false);
  });

  it("prevents focus stealing on quick emoji buttons", () => {
    const handleChange = vi.fn();
    const { getByTitle } = render(
      <RichTextEditor value="Hello" richTextJson={null} onChange={handleChange} />
    );

    // Open emoji picker
    const emojiTrigger = getByTitle("Insert Emoji");
    fireEvent.click(emojiTrigger);

    // Get an emoji button
    const fireEmojiBtn = getByTitle("🔥");
    expect(fireEmojiBtn).toBeTruthy();

    const mouseDownEvent = new MouseEvent("mousedown", {
      bubbles: true,
      cancelable: true,
    });
    const dispatched = fireEmojiBtn.dispatchEvent(mouseDownEvent);

    expect(mouseDownEvent.defaultPrevented).toBe(true);
    expect(dispatched).toBe(false);
  });

  it("renders caption character count and word count from state", () => {
    const handleChange = vi.fn();
    const { getByText } = render(
      <RichTextEditor value="Xin chao AISAM" richTextJson={null} onChange={handleChange} />
    );

    expect(getByText(/14 caption characters/i)).toBeTruthy();
    expect(getByText(/3 words/i)).toBeTruthy();
  });

  it("debounces onChange and flushes immediately on aisam-flush-editor event", () => {
    vi.useFakeTimers();
    const handleChange = vi.fn();
    const { getByTitle } = render(
      <RichTextEditor value="Initial" richTextJson={null} onChange={handleChange} />
    );

    // Open emoji picker and click emoji to trigger an edit via commands
    const emojiTrigger = getByTitle("Insert Emoji");
    fireEvent.click(emojiTrigger);
    const fireEmojiBtn = getByTitle("🔥");
    fireEvent.click(fireEmojiBtn);

    // Immediately after typing/inserting, onChange should NOT have fired yet due to 200ms debounce
    expect(handleChange).not.toHaveBeenCalled();

    // Advance by 100ms - still shouldn't have fired
    act(() => {
      vi.advanceTimersByTime(100);
    });
    expect(handleChange).not.toHaveBeenCalled();

    // Dispatch synchronous flush event (simulating Save button click)
    act(() => {
      window.dispatchEvent(new CustomEvent("aisam-flush-editor"));
    });

    // Should have flushed immediately!
    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange.mock.calls[0][0]).toContain("🔥");
  });

  it("automatically fires debounced onChange after 200ms if no flush event is triggered", () => {
    vi.useFakeTimers();
    const handleChange = vi.fn();
    const { getByTitle } = render(
      <RichTextEditor value="Initial" richTextJson={null} onChange={handleChange} />
    );

    const emojiTrigger = getByTitle("Insert Emoji");
    fireEvent.click(emojiTrigger);
    const rocketEmojiBtn = getByTitle("🚀");
    fireEvent.click(rocketEmojiBtn);

    expect(handleChange).not.toHaveBeenCalled();

    // Advance beyond 200ms
    act(() => {
      vi.advanceTimersByTime(250);
    });

    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange.mock.calls[0][0]).toContain("🚀");
  });

  it("flushes debounced changes when editor loses focus (blur)", () => {
    vi.useFakeTimers();
    const handleChange = vi.fn();
    const { container, getByTitle } = render(
      <RichTextEditor value="Initial" richTextJson={null} onChange={handleChange} />
    );

    const emojiTrigger = getByTitle("Insert Emoji");
    fireEvent.click(emojiTrigger);
    const gemEmojiBtn = getByTitle("💎");
    fireEvent.click(gemEmojiBtn);

    expect(handleChange).not.toHaveBeenCalled();

    const proseMirror = container.querySelector(".ProseMirror");
    expect(proseMirror).toBeTruthy();

    act(() => {
      fireEvent.blur(proseMirror!);
    });

    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange.mock.calls[0][0]).toContain("💎");
  });
});
