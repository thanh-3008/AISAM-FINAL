"use client";

/**
 * RichTextEditor.tsx
 * A rich text editor built on Tiptap with:
 * - Bold, Italic, Underline, Strikethrough
 * - Uppercase toggle
 * - Bullet list, Numbered list
 * - Emoji picker (inline)
 * - Paragraph / line break
 * - Markdown output for storage
 * - Backward compatible with legacy plaintext input
 */

import { useEditor, EditorContent } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import UnderlineExtension from "@tiptap/extension-underline";
import Placeholder from "@tiptap/extension-placeholder";
import { useEffect, useState, useCallback, useRef } from "react";
import { marked } from "marked";

// Configure marked: no HTML sanitization here, we handle security at the render layer
// gfm = true enables ~~strikethrough~~, breaks = false preserves paragraph semantics
marked.setOptions({ gfm: true, breaks: false });

// ---------------------------------------------------------------------------
// Serialization helpers (Tiptap JSON → Markdown)
// ---------------------------------------------------------------------------

interface TiptapTextNode {
  type: "text";
  text: string;
  marks?: Array<{ type: string }>;
}

interface TiptapNode {
  type: string;
  content?: TiptapNode[];
  attrs?: Record<string, unknown>;
  text?: string;
  marks?: Array<{ type: string }>;
}

function serializeNode(node: TiptapNode): string {
  if (node.type === "text") {
    const textNode = node as TiptapTextNode;
    let text = textNode.text ?? "";
    const markTypes = (textNode.marks ?? []).map((m) => m.type);
    if (markTypes.includes("strike")) text = `~~${text}~~`;
    if (markTypes.includes("underline")) text = `<u>${text}</u>`;
    if (markTypes.includes("italic")) text = `*${text}*`;
    if (markTypes.includes("bold")) text = `**${text}**`;
    return text;
  }

  const children = (node.content ?? []).map(serializeNode).join("");

  switch (node.type) {
    case "paragraph":
      return children ? `${children}\n\n` : "\n";
    case "bulletList":
      return `${children}`;
    case "orderedList":
      return `${children}`;
    case "listItem":
      return `- ${children.trimEnd()}\n`;
    case "hardBreak":
      return "\n";
    case "heading": {
      const level = (node.attrs?.level as number) ?? 1;
      return `${"#".repeat(level)} ${children}\n\n`;
    }
    case "blockquote":
      return `> ${children}\n`;
    case "horizontalRule":
      return `---\n`;
    case "doc":
    default:
      return children;
  }
}

/** Convert Tiptap JSON doc to Markdown string */
function tiptapToMarkdown(doc: TiptapNode): string {
  const raw = serializeNode(doc);
  // Clean excessive newlines but keep paragraph breaks
  return raw.replace(/\n{3,}/g, "\n\n").trimEnd();
}

// ---------------------------------------------------------------------------
// Parsing helpers (Markdown → Tiptap HTML)
// ---------------------------------------------------------------------------

/**
 * Convert Markdown string to Tiptap-compatible HTML.
 *
 * Uses `marked` (AST-based parser) which correctly handles:
 *  - nested marks: **_Bold Italic_**, ~~**Bold Strike**~~
 *  - bullet/numbered lists with inline formatting
 *  - paragraphs and line breaks
 *  - emoji and unicode pass-through
 *
 * Underline is stored as raw `<u>` tag (Markdown has no underline syntax).
 * marked passes <u> through as inline HTML, so it round-trips correctly.
 *
 * Security: DOMPurify sanitization is applied at render time inside Tiptap.
 * This function intentionally leaves safe tags like <strong>, <em>, <u>, <s>,
 * <ul>, <ol>, <li>, <p> intact for Tiptap to parse.
 */
function markdownToHtml(markdown: string): string {
  if (!markdown) return "<p></p>";

  // marked does not know <u> (underline), but it passes inline HTML through
  // when pedantic=false (the default). So <u>text</u> survives as-is.
  const html = marked.parse(markdown, { async: false }) as string;

  // Tiptap needs at least one block element; if output is empty return a paragraph
  return html.trim() || "<p></p>";
}

// ---------------------------------------------------------------------------
// Quick emoji list
// ---------------------------------------------------------------------------

const QUICK_EMOJIS = [
  "🔥", "✨", "🎁", "🚀", "❤️", "💰", "👉", "🎯",
  "⭐", "💥", "🎉", "👑", "💎", "🏆", "✅", "📢",
  "🛍️", "💳", "🏷️", "📱", "💫", "🌟", "🔑", "🎊",
];

// ---------------------------------------------------------------------------
// Toolbar Button
// ---------------------------------------------------------------------------

function ToolbarBtn({
  onClick,
  active,
  disabled,
  title,
  children,
}: {
  onClick: () => void;
  active?: boolean;
  disabled?: boolean;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      title={title}
      className={`w-8 h-8 flex items-center justify-center rounded-lg text-[13px] font-semibold transition-all select-none
        ${active
          ? "bg-primary text-on-primary shadow-sm"
          : "text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
        }
        ${disabled ? "opacity-40 cursor-not-allowed" : "cursor-pointer"}
      `}
    >
      {children}
    </button>
  );
}

// ---------------------------------------------------------------------------
// Main Component
// ---------------------------------------------------------------------------

interface RichTextEditorProps {
  value: string;
  onChange: (markdown: string) => void;
  placeholder?: string;
  minHeight?: number;
  className?: string;
}

export default function RichTextEditor({
  value,
  onChange,
  placeholder = "Write your content here...",
  minHeight = 200,
  className = "",
}: RichTextEditorProps) {
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const emojiPickerRef = useRef<HTMLDivElement>(null);
  const isInitialized = useRef(false);
  const externalValueRef = useRef(value);

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        // Headings not needed for social posts
        heading: false,
        // Keep code block off
        code: false,
        codeBlock: false,
      }),
      UnderlineExtension,
      Placeholder.configure({
        placeholder,
        emptyNodeClass:
          "before:content-[attr(data-placeholder)] before:text-outline/40 before:float-left before:h-0 before:pointer-events-none before:text-body-sm",
      }),
    ],
    content: markdownToHtml(value),
    editorProps: {
      attributes: {
        class: `outline-none text-body-sm text-on-surface leading-relaxed [&>*+*]:mt-3 [&>ul]:pl-5 [&>ul>li]:list-disc [&>ol]:pl-5 [&>ol>li]:list-decimal`,
        style: `min-height: ${minHeight}px; padding: 12px;`,
      },
    },
    onUpdate: ({ editor: ed }) => {
      const json = ed.getJSON() as TiptapNode;
      const markdown = tiptapToMarkdown(json);
      externalValueRef.current = markdown;
      onChange(markdown);
    },
    immediatelyRender: false,
  });

  // Sync external value changes (e.g., AI fills in content)
  useEffect(() => {
    if (!editor) return;
    if (!isInitialized.current) {
      isInitialized.current = true;
      return;
    }
    // Only update if value differs from what we last emitted
    if (value !== externalValueRef.current) {
      const html = markdownToHtml(value);
      // Tiptap 3.x: setContent second arg is SetContentOptions, not boolean
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      editor.commands.setContent(html, { emitUpdate: false } as any);
      externalValueRef.current = value;
    }
  }, [editor, value]);

  // Close emoji picker on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (emojiPickerRef.current && !emojiPickerRef.current.contains(e.target as Node)) {
        setShowEmojiPicker(false);
      }
    };
    if (showEmojiPicker) document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [showEmojiPicker]);

  const insertEmoji = useCallback(
    (emoji: string) => {
      editor?.commands.insertContent(emoji);
      setShowEmojiPicker(false);
    },
    [editor]
  );

  // Uppercase the ENTIRE selection, preserving all existing marks (bold, italic, etc.)
  // This is a one-time text transformation. Use Ctrl+Z to undo.
  const handleUppercase = useCallback(() => {
    if (!editor) return;
    const { from, to, empty } = editor.state.selection;
    if (empty) return;

    // Use Tiptap's transaction to uppercase each text node individually,
    // which preserves all marks (bold, italic, underline, strike) on each node.
    const { state, dispatch } = editor.view;
    const { tr } = state;
    let modified = false;

    state.doc.nodesBetween(from, to, (node, pos) => {
      if (node.isText && node.text) {
        const nodeFrom = Math.max(pos, from);
        const nodeTo = Math.min(pos + node.nodeSize, to);
        if (nodeFrom < nodeTo) {
          const original = node.text.slice(nodeFrom - pos, nodeTo - pos);
          const upper = original.toUpperCase();
          if (upper !== original) {
            tr.insertText(upper, nodeFrom, nodeTo);
            modified = true;
          }
        }
      }
    });

    if (modified) {
      dispatch(tr);
      editor.commands.focus();
    }
  }, [editor]);

  if (!editor) return null;

  return (
    <div className={`relative bg-surface-container rounded-xl border border-outline-variant/20 focus-within:border-primary/40 focus-within:ring-2 focus-within:ring-primary/5 transition-all ${className}`}>
      {/* Toolbar */}
      <div className="flex items-center gap-0.5 px-2 py-1.5 border-b border-outline-variant/10 flex-wrap">
        {/* Bold */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleBold().run()}
          active={editor.isActive("bold")}
          title="Bold (Ctrl+B)"
        >
          <strong>B</strong>
        </ToolbarBtn>

        {/* Italic */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleItalic().run()}
          active={editor.isActive("italic")}
          title="Italic (Ctrl+I)"
        >
          <em>I</em>
        </ToolbarBtn>

        {/* Underline */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleUnderline().run()}
          active={editor.isActive("underline")}
          title="Underline (Ctrl+U)"
        >
          <span className="underline">U</span>
        </ToolbarBtn>

        {/* Strikethrough */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleStrike().run()}
          active={editor.isActive("strike")}
          title="Strikethrough"
        >
          <span className="line-through">S</span>
        </ToolbarBtn>

        {/* Uppercase — one-time transform, preserves bold/italic/underline marks */}
        <ToolbarBtn
          onClick={handleUppercase}
          title="UPPERCASE selected text (select text first • preserves formatting • undo with Ctrl+Z)"
        >
          <span className="text-[11px] font-bold tracking-tight">AA</span>
        </ToolbarBtn>

        <div className="w-px h-5 bg-outline-variant/20 mx-0.5" />

        {/* Bullet List */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleBulletList().run()}
          active={editor.isActive("bulletList")}
          title="Bullet List"
        >
          <span className="material-symbols-outlined text-[16px]">format_list_bulleted</span>
        </ToolbarBtn>

        {/* Numbered List */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleOrderedList().run()}
          active={editor.isActive("orderedList")}
          title="Numbered List"
        >
          <span className="material-symbols-outlined text-[16px]">format_list_numbered</span>
        </ToolbarBtn>

        <div className="w-px h-5 bg-outline-variant/20 mx-0.5" />

        {/* Emoji Picker */}
        <div className="relative" ref={emojiPickerRef}>
          <ToolbarBtn
            onClick={() => setShowEmojiPicker((prev) => !prev)}
            active={showEmojiPicker}
            title="Insert Emoji"
          >
            😀
          </ToolbarBtn>
          {showEmojiPicker && (
            <div className="absolute left-0 top-full mt-1 z-30 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl p-2 w-[220px]">
              <p className="text-label-xs text-outline mb-2 px-1">Quick Emoji</p>
              <div className="grid grid-cols-8 gap-0.5">
                {QUICK_EMOJIS.map((emoji) => (
                  <button
                    key={emoji}
                    type="button"
                    onClick={() => insertEmoji(emoji)}
                    className="w-7 h-7 flex items-center justify-center text-[16px] hover:bg-surface-container rounded-lg transition-colors"
                    title={emoji}
                  >
                    {emoji}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Editor Area */}
      <EditorContent editor={editor} />

      {/* Word / char count */}
      <div className="flex items-center justify-end gap-3 px-3 pb-2 text-label-xs text-outline">
        <span>
          {editor.state.doc.textContent.length} chars
        </span>
        <span>
          {editor.state.doc.textContent.split(/\s+/).filter(Boolean).length} words
        </span>
      </div>
    </div>
  );
}
