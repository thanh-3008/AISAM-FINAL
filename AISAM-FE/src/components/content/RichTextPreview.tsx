"use client";

/**
 * RichTextPreview.tsx
 * Renders Markdown content as styled HTML with DOMPurify sanitization.
 * Backward compatible with legacy plaintext.
 * 
 * Security: Uses DOMPurify with a strict allowlist to prevent XSS.
 * Parser: Uses `marked` (AST-based) for robust nested formatting support.
 */

import { useMemo, useEffect, useState } from "react";
import DOMPurify from "dompurify";
import { marked } from "marked";

// ---------------------------------------------------------------------------
// Inline DOMPurify sanitizer (dynamic import to avoid SSR issues)
// ---------------------------------------------------------------------------

function sanitizeHtml(html: string): string {
  // Server-side: return stripped version
  if (typeof window === "undefined") {
    return html
      .replace(/<script[^>]*>[\s\S]*?<\/script>/gi, "")
      .replace(/<[^>]+>/g, "");
  }

  // Client-side: use DOMPurify
  try {
    return DOMPurify.sanitize(html, {
      ALLOWED_TAGS: [
        "p", "br", "b", "strong", "i", "em", "u", "s", "del",
        "ul", "ol", "li", "span", "div",
      ],
      ALLOWED_ATTR: ["class", "style"],
      FORBID_TAGS: ["script", "iframe", "form", "input", "button", "a", "img"],
      FORBID_ATTR: ["onclick", "onerror", "onload", "href", "src"],
    });
  } catch {
    // Fallback: strip all tags
    return html.replace(/<[^>]+>/g, "");
  }
}

// ---------------------------------------------------------------------------
// Markdown → HTML converter using `marked` (same library as the editor)
// ---------------------------------------------------------------------------

/**
 * Convert Markdown to sanitized HTML for preview.
 *
 * Uses `marked` with gfm=true for ~~strikethrough~~ and nested marks.
 * Underline is stored as raw <u> tag which marked passes through correctly.
 * Legacy plaintext (no markdown markers) is rendered as plain paragraphs.
 *
 * The output of this function is always passed through DOMPurify before
 * being set via dangerouslySetInnerHTML.
 */
function markdownToPreviewHtml(markdown: string): string {
  if (!markdown) return "";
  const html = marked.parse(markdown, { gfm: true, breaks: false, async: false }) as string;
  return html.trim();
}

// ---------------------------------------------------------------------------
// Props
// ---------------------------------------------------------------------------

interface RichTextPreviewProps {
  content: string;
  platform?: string;
  className?: string;
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export default function RichTextPreview({
  content,
  className = "",
}: RichTextPreviewProps) {
  const [safeHtml, setSafeHtml] = useState<string>("");

  const rawHtml = useMemo(() => markdownToPreviewHtml(content || ""), [content]);

  useEffect(() => {
    setSafeHtml(sanitizeHtml(rawHtml));
  }, [rawHtml]);

  if (!content) {
    return (
      <p className={`text-outline/40 text-body-sm italic ${className}`}>
        No content to preview...
      </p>
    );
  }

  return (
    <div
      className={`rich-text-preview text-body-sm text-on-surface leading-relaxed ${className}`}
      // Safe: content is DOMPurify-sanitized with strict allowlist
      dangerouslySetInnerHTML={{ __html: safeHtml }}
    />
  );
}
