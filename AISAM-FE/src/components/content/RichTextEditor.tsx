"use client";

/**
 * RichTextEditor.tsx
 * A rich text editor built on Tiptap with:
 * - Bold, Italic, Underline, Strikethrough
 * - Uppercase toggle
 * - Bullet list, Numbered list
 * - Emoji picker (inline)
 * - Paragraph / line break
 * - Versioned JSON and derived caption text for storage
 * - Backward compatible with legacy plaintext input
 */

import { useEditor, EditorContent } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import UnderlineExtension from "@tiptap/extension-underline";
import HighlightExtension from "@tiptap/extension-highlight";
import Placeholder from "@tiptap/extension-placeholder";
import { useEffect, useState, useCallback, useRef } from "react";
import { Mark } from "@tiptap/react";
import { readDocument, documentText, formatCaption, safeLink, type RichNode } from "@/lib/richTextDocument";


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
      onMouseDown={(e) => {
        e.preventDefault();
        e.stopPropagation();
      }}
      onClick={(e) => {
        e.preventDefault();
        e.stopPropagation();
        onClick();
      }}
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

function computeCounts(ed: any): { characters: number; words: number } {
  try {
    const json = ed.getJSON() as RichNode;
    const characters = formatCaption(json).characters;
    const words = ed.state.doc.textContent.split(/\s+/).filter(Boolean).length;
    return { characters, words };
  } catch {
    return { characters: 0, words: 0 };
  }
}

// ---------------------------------------------------------------------------
// Main Component
// ---------------------------------------------------------------------------

interface RichTextEditorProps {
  value: string;
  richTextJson?: string | null;
  onChange: (plainText: string, richTextJson: string) => void;
  placeholder?: string;
  minHeight?: number;
  className?: string;
}

export default function RichTextEditor({
  value,
  richTextJson,
  onChange,
  placeholder = "Write your content here...",
  minHeight = 200,
  className = "",
}: RichTextEditorProps) {
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const emojiPickerRef = useRef<HTMLDivElement>(null);
  const externalJsonRef = useRef(richTextJson);
  const externalValueRef = useRef(value);

  const [counts, setCounts] = useState<{ characters: number; words: number }>(() => {
    try {
      if (richTextJson) {
        const json = JSON.parse(richTextJson) as RichNode;
        return {
          characters: formatCaption(json).characters,
          words: (value || "").split(/\s+/).filter(Boolean).length,
        };
      }
      return {
        characters: (value || "").length,
        words: (value || "").split(/\s+/).filter(Boolean).length,
      };
    } catch {
      return { characters: (value || "").length, words: 0 };
    }
  });

  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
  const pendingUpdateRef = useRef<{ markdown: string; json: string } | null>(null);
  const onChangeRef = useRef(onChange);

  useEffect(() => {
    onChangeRef.current = onChange;
  }, [onChange]);

  const flushChange = useCallback(() => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
      debounceTimerRef.current = null;
    }
    if (pendingUpdateRef.current) {
      const { markdown, json } = pendingUpdateRef.current;
      pendingUpdateRef.current = null;
      externalValueRef.current = markdown;
      externalJsonRef.current = json;
      onChangeRef.current(markdown, json);
    }
  }, []);

  const flushChangeRef = useRef(flushChange);
  useEffect(() => {
    flushChangeRef.current = flushChange;
  }, [flushChange]);

  useEffect(() => {
    const handleFlush = () => {
      flushChangeRef.current();
    };
    window.addEventListener("aisam-flush-editor", handleFlush);
    return () => {
      window.removeEventListener("aisam-flush-editor", handleFlush);
      flushChangeRef.current();
    };
  }, []);

  const editor = useEditor({
    extensions: [
      StarterKit.configure({
        trailingNode: false,
        // Headings not needed for social posts
        heading: { levels: [1, 2, 3] }, underline: false, horizontalRule: false,
        link: { openOnClick: false, isAllowedUri: safeLink },
        // Keep code block off
        code: false,
        codeBlock: false,
      }),
      UnderlineExtension,
      HighlightExtension,
      Placeholder.configure({
        placeholder,
        emptyNodeClass:
          "before:content-[attr(data-placeholder)] before:text-outline/40 before:float-left before:h-0 before:pointer-events-none before:text-body-sm",
      }),
    ],
    content: readDocument(richTextJson, value),
    editorProps: {
      attributes: {
        class: `outline-none text-body-sm text-on-surface leading-relaxed [&>*+*]:mt-3 [&>ul]:pl-5 [&>ul>li]:list-disc [&>ol]:pl-5 [&>ol>li]:list-decimal`,
        style: `min-height: ${minHeight}px; padding: 12px;`,
      },
    },
    onCreate: ({ editor: ed }) => {
      setCounts(computeCounts(ed));
    },
    onBlur: () => {
      flushChangeRef.current();
    },
    onUpdate: ({ editor: ed }) => {
      const json = ed.getJSON() as RichNode;
      const markdown = documentText(json);
      const jsonStr = JSON.stringify(json);
      externalValueRef.current = markdown;
      externalJsonRef.current = jsonStr;
      setCounts(computeCounts(ed));

      pendingUpdateRef.current = { markdown, json: jsonStr };
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
      debounceTimerRef.current = setTimeout(() => {
        if (pendingUpdateRef.current) {
          const updateData = pendingUpdateRef.current;
          pendingUpdateRef.current = null;
          debounceTimerRef.current = null;
          onChangeRef.current(updateData.markdown, updateData.json);
        }
      }, 200);
    },
    immediatelyRender: false,
    shouldRerenderOnTransaction: true,
  });

  // Sync external value changes (e.g., AI fills in content)
  useEffect(() => {
    if (!editor) return;
    if (pendingUpdateRef.current) return;
    // Do not overwrite editor content from external props while user is actively focused in this editor
    if (editor.isFocused) return;
    // Only update if value differs from what we last emitted
    if (value !== externalValueRef.current || richTextJson !== externalJsonRef.current) {
      const html = readDocument(richTextJson, value);
      // Tiptap 3.x: setContent second arg is SetContentOptions, not boolean
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      editor.commands.setContent(html, { emitUpdate: false } as any);
      externalValueRef.current = value;
      externalJsonRef.current = richTextJson;
      setCounts(computeCounts(editor));
    }
  }, [editor, value, richTextJson]);

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
            tr.replaceWith(tr.mapping.map(nodeFrom), tr.mapping.map(nodeTo), state.schema.text(upper, node.marks));
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
        <ToolbarBtn title="Heading" active={editor.isActive("heading")} onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}>H2</ToolbarBtn>
        <ToolbarBtn title="Highlight" active={editor.isActive("highlight")} onClick={() => editor.chain().focus().toggleMark("highlight").run()}>▰</ToolbarBtn>
        <ToolbarBtn title="Link" active={editor.isActive("link")} onClick={() => {
          const href = window.prompt("Link URL (https://…)", editor.getAttributes("link").href ?? "");
          if (href === null) return;
          if (!href) { editor.chain().focus().unsetLink().run(); return; }
          if (safeLink(href)) editor.chain().focus().extendMarkRange("link").setLink({ href }).run();
        }}>↗</ToolbarBtn>
        <ToolbarBtn title="Undo" disabled={!editor.can().undo()} onClick={() => editor.chain().focus().undo().run()}>↶</ToolbarBtn>
        <ToolbarBtn title="Redo" disabled={!editor.can().redo()} onClick={() => editor.chain().focus().redo().run()}>↷</ToolbarBtn>
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

        {/* Highlight */}
        <ToolbarBtn
          onClick={() => editor.chain().focus().toggleHighlight().run()}
          active={editor.isActive("highlight")}
          title="Highlight"
        >
          <span className="relative inline-flex items-center justify-center w-full h-full">
            <span className="absolute inset-x-0.5 bottom-0.5 h-[60%] bg-yellow-300/60 rounded-sm" />
            <span className="relative font-bold text-[12px]">H</span>
          </span>
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
                    onMouseDown={(e) => e.preventDefault()}
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
          {counts.characters} caption characters
        </span>
        <span>
          {counts.words} words
        </span>
      </div>
    </div>
  );
}
