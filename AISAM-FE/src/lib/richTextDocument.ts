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
