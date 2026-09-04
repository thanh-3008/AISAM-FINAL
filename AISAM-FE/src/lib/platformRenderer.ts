/**
 * platformRenderer.ts
 * Converts rich Markdown content to platform-appropriate text format.
 * 
 * Platform support matrix:
 * - Facebook:  No native bold/italic → strip markers, keep emoji + hashtag + line breaks
 * - Instagram: Same as Facebook
 * - TikTok:    Plain text only, keep emoji + hashtag
 * - YouTube:   Basic formatting ok via description, keep most markdown
 */

import { markdownToPlaintext } from "./richTextUtils";

export type SocialPlatform = "facebook" | "instagram" | "tiktok" | "youtube" | string;

/**
 * Render Markdown content for a specific social platform.
 * Returns a string suitable for posting to that platform.
 */
export function renderForPlatform(markdown: string, platform: SocialPlatform): string {
  if (!markdown) return "";

  const normalized = platform.toLowerCase();

  switch (normalized) {
    case "facebook":
    case "instagram":
      return renderForFacebookInstagram(markdown);
    case "tiktok":
      return renderForTikTok(markdown);
    case "youtube":
      return renderForYouTube(markdown);
    default:
      // Default: strip markdown formatting, preserve structure
      return markdownToPlaintext(markdown);
  }
}

/**
 * Facebook & Instagram: plain text with emoji, hashtags, line breaks.
 * Uppercase CTA/hook words where bold was used.
 */
function renderForFacebookInstagram(markdown: string): string {
  let text = markdown;

  // Bold text → UPPERCASE (Facebook convention for emphasis)
  text = text.replace(/\*\*([^*]+)\*\*/g, (_, inner: string) => inner.toUpperCase());

  // Italic → plain text
  text = text.replace(/\*([^*]+)\*/g, "$1");

  // Underline (custom __ syntax) → plain
  text = text.replace(/__([^_]+)__/g, "$1");

  // Strikethrough → plain (Facebook doesn't render ~~)
  text = text.replace(/~~([^~]+)~~/g, "$1");

  // Headings → plain + newline
  text = text.replace(/^#{1,6} (.+)$/gm, "$1");

  // Bullet list items → • emoji bullet
  text = text.replace(/^[-*+] (.+)$/gm, "• $1");

  // Numbered list items
  text = text.replace(/^\d+\. (.+)$/gm, (match: string, item: string, offset: number, str: string) => {
    // Count which numbered item this is
    const before = str.slice(0, offset);
    const num = (before.match(/^\d+\. /gm) || []).length + 1;
    return `${num}. ${item}`;
  });

  // Clean up excessive newlines
  text = text.replace(/\n{3,}/g, "\n\n").trim();

  return text;
}

/**
 * TikTok: plain text with emoji and hashtags only.
 */
function renderForTikTok(markdown: string): string {
  let text = markdownToPlaintext(markdown);

  // TikTok: keep hashtags intact
  // Collapse to max 2 newlines
  text = text.replace(/\n{3,}/g, "\n\n").trim();

  return text;
}

/**
 * YouTube: mostly preserve markdown as YouTube descriptions support some formatting.
 */
function renderForYouTube(markdown: string): string {
  let text = markdown;

  // YouTube doesn't render markdown natively but descriptions support line breaks
  // Remove markdown syntax markers but keep structure
  text = text.replace(/\*\*([^*]+)\*\*/g, "$1");
  text = text.replace(/\*([^*]+)\*/g, "$1");
  text = text.replace(/__([^_]+)__/g, "$1");
  text = text.replace(/~~([^~]+)~~/g, "$1");
  text = text.replace(/^#{1,6} (.+)$/gm, "$1\n");
  text = text.replace(/^[-*+] (.+)$/gm, "• $1");
  text = text.replace(/\n{3,}/g, "\n\n").trim();

  return text;
}
