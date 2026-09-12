import React from "react";
import { describe, it, expect } from "vitest";
import { render, cleanup } from "@testing-library/react";
import { Editor, Mark } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { documentText, readDocument, formatCaption, safeLink, type RichNode } from "../richTextDocument";
import RichTextPreview from "@/components/content/RichTextPreview";

const document: RichNode = { type: "doc", content: [
  { type: "heading", attrs: { level: 2 }, content: [{ type: "text", text: "Xin chào 👩‍💻 #AISAM", marks: [{ type: "bold" }, { type: "highlight" }] }] },
  { type: "orderedList", attrs: { start: 3 }, content: [{ type: "listItem", content: [{ type: "paragraph", content: [{ type: "text", text: "Mua", marks: [{ type: "link", attrs: { href: "https://example.test/p" } }] }] }] }] },
] };
describe("Rich text v1", () => {
  it("matches the backend Unicode/list/link caption contract on every platform", () => {
    const expected = "Xin chào 👩‍💻 #AISAM\n\n3. Mua (https://example.test/p)";
    for (const platform of ["facebook", "instagram", "tiktok", "google"]) {
      expect(formatCaption(document, platform)).toEqual({ text: expected, characters: Array.from(expected).length });
    }
  });
  it("preserves literal legacy text and line breaks", () => {
    const text = "**literal** <img src=x onerror=alert(1)>\n\n#tag";
    expect(documentText(readDocument(null, text))).toBe(text);
  });
  it("round trips real Tiptap JSON with highlight, links and numbered lists", () => {
    const extensions = [StarterKit.configure({ trailingNode: false }), Mark.create({ name: "highlight", parseHTML: () => [{ tag: "mark" }], renderHTML: () => ["mark", {}, 0] })];
    const first = new Editor({ extensions, content: document });
    const stored = first.getJSON();
    const second = new Editor({ extensions, content: stored });
    expect(second.getJSON()).toEqual(stored);
    expect(documentText(second.getJSON())).toBe(documentText(document));
    second.commands.selectAll(); second.commands.toggleUnderline();
    const changed = second.getJSON();
    expect(changed).not.toEqual(stored);
    second.commands.undo(); expect(second.getJSON()).toEqual(stored);
    second.commands.redo(); expect(second.getJSON()).toEqual(changed);
    first.destroy(); second.destroy();
  });
  it("renders hostile text as text and never renders unsafe links", () => {
    const hostile = { type: "doc", content: [{ type: "paragraph", content: [{ type: "text", text: "<img src=x onerror=alert(1)>", marks: [{ type: "link", attrs: { href: "javascript:alert(1)" } }] }] }] };
    const result = render(<RichTextPreview content="" richTextJson={JSON.stringify(hostile)} />);
    expect(result.container.querySelector("img")).toBeNull();
    expect(result.container.querySelector("a")).toBeNull();
    expect(result.container.textContent).toContain("<img");
    expect(safeLink("data:text/html,hello")).toBe(false);
    cleanup();
  });
});
