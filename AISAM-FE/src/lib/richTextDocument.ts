export interface RichNode {
  type: string;
  text?: string;
  attrs?: Record<string, unknown>;
  content?: RichNode[];
  marks?: { type: string; attrs?: Record<string, unknown> }[];
}
export const RICH_TEXT_VERSION = 1;
export function safeLink(value: string): boolean {
  if (value.length > 2048 || /[\u0000-\u001f\u007f]/.test(value)) return false;
  try { return ["http:", "https:", "mailto:"].includes(new URL(value).protocol); } catch { return false; }
}
export function plainDocument(text: string): RichNode {
  return { type: "doc", content: text.split("\n").map(line => ({ type: "paragraph", content: line ? [{ type: "text", text: line }] : [] })) };
}

const INLINE_MARKERS = [
  { marker: "**", type: "bold" },
  { marker: "__", type: "underline" },
  { marker: "~~", type: "strike" },
  { marker: "*", type: "italic" },
] as const;

function markdownInline(text: string): RichNode[] {
  const nodes: RichNode[] = [];
  let cursor = 0;

  while (cursor < text.length) {
    let next: { start: number; end: number; marker: string; type: string } | null = null;
    for (const definition of INLINE_MARKERS) {
      const start = text.indexOf(definition.marker, cursor);
      if (start < 0) continue;
      const end = text.indexOf(definition.marker, start + definition.marker.length);
      if (end <= start + definition.marker.length) continue;
      if (!next || start < next.start || (start === next.start && definition.marker.length > next.marker.length)) {
        next = { start, end, ...definition };
      }
    }

    if (!next) {
      nodes.push({ type: "text", text: text.slice(cursor) });
      break;
    }
    if (next.start > cursor) nodes.push({ type: "text", text: text.slice(cursor, next.start) });
    nodes.push({
      type: "text",
      text: text.slice(next.start + next.marker.length, next.end),
      marks: [{ type: next.type }],
    });
    cursor = next.end + next.marker.length;
  }

  return nodes.filter(node => node.text !== "");
}

/** Convert the limited Markdown emitted by AI into the editor's versioned document. */
export function markdownDocument(markdown: string): RichNode {
  const content: RichNode[] = [];
  const lines = markdown.replace(/\r\n?/g, "\n").split("\n");
  let index = 0;

  while (index < lines.length) {
    const line = lines[index];
    const bullet = line.match(/^\s*[-+]\s+(.+)$/);
    const ordered = line.match(/^\s*(\d+)\.\s+(.+)$/);
    if (bullet || ordered) {
      const orderedStart = ordered ? Number(ordered[1]) : 1;
      const items: RichNode[] = [];
      while (index < lines.length) {
        const match = ordered
          ? lines[index].match(/^\s*\d+\.\s+(.+)$/)
          : lines[index].match(/^\s*[-+]\s+(.+)$/);
        if (!match) break;
        const value = ordered ? match[1].replace(/^\d+\.\s+/, "") : match[1];
        items.push({ type: "listItem", content: [{ type: "paragraph", content: markdownInline(value) }] });
        index += 1;
      }
      content.push({ type: ordered ? "orderedList" : "bulletList", attrs: ordered ? { start: orderedStart } : undefined, content: items });
      continue;
    }

    const heading = line.match(/^#{1,3}\s+(.+)$/);
    content.push({
      type: heading ? "heading" : "paragraph",
      attrs: heading ? { level: Math.min(3, line.match(/^#+/)?.[0].length ?? 1) } : undefined,
      content: markdownInline(heading?.[1] ?? line),
    });
    index += 1;
  }

  return { type: "doc", content };
}
export function readDocument(json: string | null | undefined, text: string): RichNode {
  if (json) try { const value = JSON.parse(json); if (value.type === "doc") return value; } catch { /* preserve fallback */ }
  // Legacy text is literal, never interpreted as HTML or guessed Markdown.
  return { type: "doc", content: [{ type: "paragraph", content: text.split("\n").flatMap((line, i) => [
    ...(i ? [{ type: "hardBreak" }] : []), ...(line ? [{ type: "text", text: line }] : [])]) }] };
}
export function documentText(node: RichNode): string {
  if (node.type === "text") {
    let text = node.text ?? "";
    for (const mark of node.marks ?? []) {
      const href = mark.attrs?.href;
      if (mark.type === "link" && typeof href === "string" && safeLink(href) && text !== href) text += ` (${href})`;
    }
    return text;
  }
  if (node.type === "hardBreak") return "\n";
  const children = (node.content ?? []).map(documentText);
  switch (node.type) {
    case "bulletList": return children.map(c => `• ${c.replaceAll("\n", "\n  ")}`).join("\n");
    case "orderedList": return children.map((c, i) => `${Number(node.attrs?.start ?? 1) + i}. ${c.replaceAll("\n", "\n   ")}`).join("\n");
    case "doc": case "blockquote": return children.join("\n\n");
    case "listItem": return children.join("\n");
    default: return children.join("");
  }
}
export function formatCaption(document: RichNode, platform = "facebook") {
  // Explicit fallback contract for the current caption APIs. No Unicode faux styles.
  switch (platform.toLowerCase()) {
    case "facebook": case "instagram": case "tiktok": case "google": break;
    default: break;
  }
  const text = documentText(document);
  return { text, characters: Array.from(text).length };
}
