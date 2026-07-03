"use client";

import { useState, useEffect, useRef } from "react";
import { fetchTags } from "@/services/tagService";

interface TagPickerProps {
  selected: string[];
  onChange: (tags: string[]) => void;
  placeholder?: string;
}

export default function TagPicker({ selected, onChange, placeholder = "Add tags" }: TagPickerProps) {
  const [open, setOpen] = useState(false);
  const [allTags, setAllTags] = useState<string[]>([]);
  const [input, setInput] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    fetchTags().then(setAllTags);
  }, []);

  const filtered = allTags.filter(
    (t) => t.toLowerCase().includes(input.toLowerCase()) && !selected.includes(t)
  );

  const addTag = (tag: string) => {
    const trimmed = tag.trim();
    if (trimmed && !selected.includes(trimmed)) {
      onChange([...selected, trimmed]);
    }
    setInput("");
    inputRef.current?.focus();
  };

  const removeTag = (tag: string) => {
    onChange(selected.filter((t) => t !== tag));
  };

  return (
    <div className="relative">
      <div
        className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 flex flex-wrap gap-1.5 cursor-text focus-within:border-primary/40 transition-all"
        onClick={() => { setOpen(true); inputRef.current?.focus(); }}
      >
        {selected.map((t) => (
          <span key={t} className="px-2 py-0.5 rounded-md bg-surface-container-high text-label-xs font-semibold text-on-surface-variant flex items-center gap-1">
            {t}
            <button type="button" onClick={(e) => { e.stopPropagation(); removeTag(t); }} className="hover:opacity-60">
              <span className="material-symbols-outlined text-[12px]">close</span>
            </button>
          </span>
        ))}
        <input
          ref={inputRef}
          type="text"
          value={input}
          onChange={(e) => { setInput(e.target.value); setOpen(true); }}
          onKeyDown={(e) => {
            if (e.key === "Enter") { e.preventDefault(); addTag(input); }
            if (e.key === "Backspace" && !input && selected.length > 0) {
              removeTag(selected[selected.length - 1]);
            }
          }}
          onFocus={() => setOpen(true)}
          placeholder={selected.length === 0 ? placeholder : ""}
          className="flex-1 min-w-[80px] bg-transparent outline-none text-body-sm text-on-surface placeholder:text-outline/40"
        />
      </div>

      {open && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setOpen(false)} />
          <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 max-h-[200px] overflow-y-auto dropdown-enter">
            {input && !allTags.some((t) => t.toLowerCase() === input.toLowerCase()) && (
              <button
                type="button"
                onClick={() => addTag(input)}
                className="w-full flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-surface-container text-label-sm text-primary transition-colors"
              >
                <span className="material-symbols-outlined text-[14px]">add</span>
                Add &quot;{input}&quot;
              </button>
            )}
            {filtered.length === 0 && !input && (
              <p className="px-3 py-2 text-label-sm text-outline/40">No tags yet. Type to create.</p>
            )}
            {filtered.map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => addTag(t)}
                className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container text-label-sm text-on-surface transition-colors"
              >
                <span className="material-symbols-outlined text-[14px] text-outline">add</span>
                {t}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
