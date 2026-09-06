"use client";

/**
 * SmartHashtagBar.tsx
 * Comprehensive hashtag management and intelligent contextual suggestions.
 * - Contextual suggestions based on Brand, Product, and Workspace recent tags
 * - Normalizes hashtags (strips leading #, trims, eliminates duplicates case-insensitively)
 * - Click to add to hashtag list OR insert directly into editor text
 * - Responsive layout organized into contextual categories
 */

import { useState, useEffect, useMemo, useCallback } from "react";
import { fetchTags } from "@/services/tagService";

interface SmartHashtagBarProps {
  hashtags: string[];
  onHashtagsChange: (tags: string[]) => void;
  brandName?: string;
  productName?: string;
  onInsertIntoEditor?: (hashtagWithHash: string) => void;
  className?: string;
}

export function normalizeHashtag(raw: string): string {
  if (!raw) return "";
  return raw
    .trim()
    .replace(/^#+/, "")
    .replace(/\s+/g, "")
    .replace(/[^\p{L}\p{N}_]/gu, "");
}

export default function SmartHashtagBar({
  hashtags,
  onHashtagsChange,
  brandName,
  productName,
  onInsertIntoEditor,
  className = "",
}: SmartHashtagBarProps) {
  const [inputValue, setInputValue] = useState("");
  const [workspaceTags, setWorkspaceTags] = useState<string[]>([]);

  useEffect(() => {
    fetchTags().then((tags) => {
      if (Array.isArray(tags)) setWorkspaceTags(tags);
    }).catch(() => {});
  }, []);

  const addTag = useCallback((raw: string) => {
    const clean = normalizeHashtag(raw);
    if (!clean) return;

    const exists = hashtags.some((h) => h.toLowerCase() === clean.toLowerCase());
    if (!exists) {
      onHashtagsChange([...hashtags, clean]);
    }
    setInputValue("");
  }, [hashtags, onHashtagsChange]);

  const removeTag = useCallback((tag: string) => {
    onHashtagsChange(hashtags.filter((h) => h.toLowerCase() !== tag.toLowerCase()));
  }, [hashtags, onHashtagsChange]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" || e.key === "," || e.key === " ") {
      e.preventDefault();
      addTag(inputValue);
    }
  };

  // Generate context-aware suggestions
  const brandSuggestions = useMemo(() => {
    if (!brandName || !brandName.trim()) return [];
    const base = normalizeHashtag(brandName);
    if (!base) return [];
    return [base, `${base}Official`, `${base}VN`].filter(
      (s) => !hashtags.some((h) => h.toLowerCase() === s.toLowerCase())
    );
  }, [brandName, hashtags]);

  const productSuggestions = useMemo(() => {
    if (!productName || !productName.trim()) return [];
    const base = normalizeHashtag(productName);
    if (!base) return [];
    return [base, `${base}Review`].filter(
      (s) => !hashtags.some((h) => h.toLowerCase() === s.toLowerCase())
    );
  }, [productName, hashtags]);

  const recentWorkspaceSuggestions = useMemo(() => {
    return workspaceTags
      .map(normalizeHashtag)
      .filter((t) => t.length > 0)
      .filter((t) => !hashtags.some((h) => h.toLowerCase() === t.toLowerCase()))
      .slice(0, 8);
  }, [workspaceTags, hashtags]);

  const hasAnySuggestions = brandSuggestions.length > 0 || productSuggestions.length > 0 || recentWorkspaceSuggestions.length > 0;

  return (
    <div className={`space-y-3 ${className}`}>
      {/* Active Hashtags Input Box */}
      <div>
        <div className="flex items-center justify-between mb-1.5">
          <label className="text-label-sm text-on-surface-variant font-semibold block">
            Hashtags ({hashtags.length})
          </label>
          {hashtags.length > 0 && onInsertIntoEditor && (
            <button
              type="button"
              onClick={() => {
                const combined = hashtags.map((h) => `#${h}`).join(" ");
                onInsertIntoEditor(combined);
              }}
              className="text-[11px] text-primary hover:underline font-medium flex items-center gap-1"
              title="Chèn tất cả hashtag vào nội dung bài viết"
            >
              <span className="material-symbols-outlined text-[13px]">add_notes</span>
              Chèn tất cả vào bài viết
            </button>
          )}
        </div>

        <div
          className="flex items-center flex-wrap gap-1.5 px-3 py-2 bg-surface-container border border-outline-variant/20 rounded-xl min-h-[44px] cursor-text transition-all focus-within:border-primary/40 focus-within:ring-2 focus-within:ring-primary/5"
          onClick={() => document.getElementById("smart-hashtag-input")?.focus()}
        >
          {hashtags.map((tag) => (
            <span
              key={tag}
              className="inline-flex items-center gap-1 px-2 py-0.5 rounded-lg bg-primary/10 text-primary text-label-xs font-semibold group hover:bg-primary/20 transition-colors"
            >
              <span>#{tag}</span>
              {onInsertIntoEditor && (
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    onInsertIntoEditor(`#${tag}`);
                  }}
                  title="Chèn vào nội dung bài viết"
                  className="opacity-60 hover:opacity-100 p-0.5 rounded hover:bg-primary/20"
                >
                  <span className="material-symbols-outlined text-[11px]">add</span>
                </button>
              )}
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  removeTag(tag);
                }}
                className="opacity-60 hover:opacity-100 p-0.5 rounded hover:bg-rose-500/20 hover:text-rose-600"
                title="Xóa hashtag"
              >
                <span className="material-symbols-outlined text-[11px]">close</span>
              </button>
            </span>
          ))}

          <input
            id="smart-hashtag-input"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            onBlur={() => {
              if (inputValue.trim()) {
                addTag(inputValue);
              }
            }}
            className="flex-1 min-w-[120px] bg-transparent border-none outline-none text-body-sm text-on-surface placeholder:text-outline/30"
            placeholder={hashtags.length === 0 ? "Nhập hashtag và nhấn Enter..." : "Thêm hashtag..."}
          />
        </div>
      </div>

      {/* Contextual Smart Suggestions */}
      {hasAnySuggestions && (
        <div className="p-3 rounded-xl bg-surface-container/50 border border-outline-variant/15 space-y-2.5">
          <div className="flex items-center gap-1.5 text-label-xs text-outline font-medium">
            <span className="material-symbols-outlined text-[14px] text-amber-500">auto_awesome</span>
            <span>Gợi ý thông minh (nhấn để thêm, dấu ➕ để chèn vào nội dung):</span>
          </div>

          <div className="space-y-2">
            {/* Brand suggestions */}
            {brandSuggestions.length > 0 && (
              <div className="flex items-center flex-wrap gap-1.5">
                <span className="text-[10px] font-bold text-outline/70 uppercase tracking-wider min-w-[50px]">Brand:</span>
                {brandSuggestions.map((s) => (
                  <span
                    key={s}
                    className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300 border border-indigo-200/50 dark:border-indigo-800/40 text-label-xs font-medium cursor-pointer hover:bg-indigo-100 dark:hover:bg-indigo-900/60 transition-colors"
                  >
                    <span onClick={() => addTag(s)}>#{s}</span>
                    {onInsertIntoEditor && (
                      <button
                        type="button"
                        onClick={() => {
                          addTag(s);
                          onInsertIntoEditor(`#${s}`);
                        }}
                        title="Thêm và chèn vào bài viết"
                        className="text-indigo-500 hover:text-indigo-800 font-bold"
                      >
                        +
                      </button>
                    )}
                  </span>
                ))}
              </div>
            )}

            {/* Product suggestions */}
            {productSuggestions.length > 0 && (
              <div className="flex items-center flex-wrap gap-1.5">
                <span className="text-[10px] font-bold text-outline/70 uppercase tracking-wider min-w-[50px]">Sản phẩm:</span>
                {productSuggestions.map((s) => (
                  <span
                    key={s}
                    className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 border border-emerald-200/50 dark:border-emerald-800/40 text-label-xs font-medium cursor-pointer hover:bg-emerald-100 dark:hover:bg-emerald-900/60 transition-colors"
                  >
                    <span onClick={() => addTag(s)}>#{s}</span>
                    {onInsertIntoEditor && (
                      <button
                        type="button"
                        onClick={() => {
                          addTag(s);
                          onInsertIntoEditor(`#${s}`);
                        }}
                        title="Thêm và chèn vào bài viết"
                        className="text-emerald-500 hover:text-emerald-800 font-bold"
                      >
                        +
                      </button>
                    )}
                  </span>
                ))}
              </div>
            )}

            {/* Workspace / Recent tags */}
            {recentWorkspaceSuggestions.length > 0 && (
              <div className="flex items-center flex-wrap gap-1.5">
                <span className="text-[10px] font-bold text-outline/70 uppercase tracking-wider min-w-[50px]">Đã dùng:</span>
                {recentWorkspaceSuggestions.map((s) => (
                  <span
                    key={s}
                    className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 border border-slate-200 dark:border-slate-700 text-label-xs font-medium cursor-pointer hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
                  >
                    <span onClick={() => addTag(s)}>#{s}</span>
                    {onInsertIntoEditor && (
                      <button
                        type="button"
                        onClick={() => {
                          addTag(s);
                          onInsertIntoEditor(`#${s}`);
                        }}
                        title="Thêm và chèn vào bài viết"
                        className="text-slate-500 hover:text-slate-900 font-bold"
                      >
                        +
                      </button>
                    )}
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
