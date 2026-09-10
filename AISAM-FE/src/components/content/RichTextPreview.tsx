"use client";
import { Fragment, type ReactNode } from "react";
import { readDocument, safeLink, formatCaption, type RichNode } from "@/lib/richTextDocument";

function render(node: RichNode, depth = 0): ReactNode {
  if (depth > 32) return null;
  if (node.type === "text") {
    let result: ReactNode = node.text ?? "";
    for (const mark of node.marks ?? []) {
      switch (mark.type) {
        case "bold": result = <strong>{result}</strong>; break;
        case "italic": result = <em>{result}</em>; break;
        case "underline": result = <u>{result}</u>; break;
        case "strike": result = <s>{result}</s>; break;
        case "highlight": result = <mark>{result}</mark>; break;
        case "link": {
          const href = mark.attrs?.href;
          if (typeof href === "string" && safeLink(href)) result = <a href={href} rel="noopener noreferrer" target="_blank">{result}</a>;
          break;
        }
      }
    }
    return result;
  }
  const children = (node.content ?? []).map((c, i) => <Fragment key={i}>{render(c, depth + 1)}</Fragment>);
  switch (node.type) {
    case "paragraph": return <p>{children.length ? children : <br />}</p>;
    case "heading": return <h2 className="font-bold text-lg">{children}</h2>;
    case "bulletList": return <ul className="list-disc pl-6">{children}</ul>;
    case "orderedList": return <ol className="list-decimal pl-6" start={Number(node.attrs?.start ?? 1)}>{children}</ol>;
    case "listItem": return <li>{children}</li>;
    case "hardBreak": return <br />;
    case "blockquote": return <blockquote>{children}</blockquote>;
    default: return children;
  }
}
export default function RichTextPreview({ content, richTextJson, platform, className = "" }:
  { content: string; richTextJson?: string | null; platform?: string; className?: string }) {
  const document = readDocument(richTextJson, content);
  return <div className={`rich-text-preview whitespace-pre-wrap ${className}`}>
    {platform ? formatCaption(document, platform).text : render(document)}
  </div>;
}