/**
 * richTextUtils.ts
 * Utilities for rich text (Markdown) handling, multi-image support,
 * and backward-compatible content parsing.
 */

// ---------------------------------------------------------------------------
// Image URL helpers (JSONB array compatible)
// ---------------------------------------------------------------------------

/**
 * Parse the backend's image_url JSONB field into a list of URLs.
 * Handles:
 *  - null / empty string → []
 *  - JSON array: '["url1","url2"]' → ["url1", "url2"]
 *  - Single URL string: "https://..." → ["https://..."]
 */
export function parseImageUrls(imageUrl: string | null | undefined): string[] {
  const raw = (imageUrl ?? "").trim();
  if (!raw) return [];

  // JSON array format
  if (raw.startsWith("[")) {
    try {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed)) {
        return parsed
          .filter((u): u is string => typeof u === "string" && u.trim() !== "")
          .map((u) => u.trim());
      }
    } catch {
      // fall through
    }
  }

  // JSON object with url/secure_url fields
  if (raw.startsWith("{")) {
    try {
      const parsed = JSON.parse(raw) as Record<string, unknown>;
      for (const key of ["url", "secure_url", "imageUrl", "image_url", "src"]) {
        if (typeof parsed[key] === "string" && (parsed[key] as string).trim()) {
          return [(parsed[key] as string).trim()];
        }
      }
    } catch {
      // fall through
    }
  }

  // Plain URL with regex extraction
  const match = raw.match(/https?:\/\/[^\s"'[\]{}]+/i);
  return match ? [match[0]] : raw ? [raw] : [];
}

/**
 * Serialize an array of image URLs into the JSONB format the backend expects.
 * Returns null for empty arrays.
 */
export function serializeImageUrls(urls: string[]): string | null {
  const valid = urls.filter((u) => u.trim() !== "");
  if (valid.length === 0) return null;
  return JSON.stringify(valid);
}

/** Maximum images allowed per post */
export const MAX_IMAGES_PER_POST = 5;

// ---------------------------------------------------------------------------
// Markdown / plaintext detection
// ---------------------------------------------------------------------------

/**
 * Detect if the stored content appears to be legacy plaintext
 * (i.e. has no Markdown formatting markers).
 */
export function isLegacyPlaintext(text: string): boolean {
  if (!text) return true;
  // Simple heuristic: no Markdown markers
  const markdownPatterns = [
    /\*\*[^*]+\*\*/,   // bold
    /\*[^*]+\*/,       // italic
    /~~[^~]+~~/,       // strikethrough
    /^#{1,6} /m,       // heading
    /^- /m,            // bullet
    /^\d+\. /m,        // numbered list
    /__[^_]+__/,       // underline (custom)
  ];
  return !markdownPatterns.some((pattern) => pattern.test(text));
}

// ---------------------------------------------------------------------------
// Markdown sanitization
// ---------------------------------------------------------------------------

const ALLOWED_MARKDOWN_MARKS = ["bold", "italic", "underline", "strikethrough"];

/**
 * Basic markdown sanitization — strips any HTML that might sneak in.
 * The full DOMPurify sanitization happens at render time in RichTextPreview.
 */
export function sanitizeMarkdown(markdown: string): string {
  if (!markdown) return "";
  // Strip HTML tags that could be injected in content
  return markdown.replace(/<(script|iframe|object|embed|form|input|button)[^>]*>[\s\S]*?<\/\1>/gi, "")
    .replace(/<(script|iframe|object|embed|form|input|button)[^>]*(\/?)>/gi, "")
    .replace(/javascript:/gi, "")
    .replace(/on\w+\s*=/gi, "");
}

// ---------------------------------------------------------------------------
// Markdown → plaintext (for platform fallback rendering)
// ---------------------------------------------------------------------------

/**
 * Convert Markdown to plaintext, preserving emoji, hashtags, line breaks.
 * Used for platforms that don't support rich text (Facebook, Instagram, TikTok).
 */
export function markdownToPlaintext(markdown: string): string {
  if (!markdown) return "";

  return markdown
    // Remove bold markers
    .replace(/\*\*([^*]+)\*\*/g, "$1")
    // Remove italic markers
    .replace(/\*([^*]+)\*/g, "$1")
    // Remove underline markers (if using __text__)
    .replace(/__([^_]+)__/g, "$1")
    // Remove strikethrough markers
    .replace(/~~([^~]+)~~/g, "$1")
    // Convert headings to uppercase text
    .replace(/^#{1,6} (.+)$/gm, (_, text: string) => text.toUpperCase())
    // Convert bullet lists to plain bullets
    .replace(/^[-*+] (.+)$/gm, "• $1")
    // Convert numbered lists
    .replace(/^\d+\. (.+)$/gm, "$1")
    // Preserve line breaks
    .replace(/\n{3,}/g, "\n\n")
    .trim();
}

// ---------------------------------------------------------------------------
// Uppercase mark helper
// ---------------------------------------------------------------------------

/**
 * Convert selected text to uppercase. Used by toolbar "Aa" button.
 */
export function toUpperCase(text: string): string {
  return text.toUpperCase();
}
