const TITLE_LABEL = /^(?:title|tiêu đề|headline)\s*:\s*/i;
const CAPTION_LABEL = /^(?:caption|content|nội dung|noi dung|description|mô tả)\s*:\s*/i;
const MEDIA_MARKER = /^\[(?:IMAGE|VIDEO_URL|VIDEO_JOB):/i;

function normalizeCaptionLine(line: string): string | null {
  const withoutMarkdown = line.trim().replace(/^(?:#+\s*)?(?:\*\*)?\s*/, "").replace(/\*\*\s*$/, "").trim();
  if (!withoutMarkdown || TITLE_LABEL.test(withoutMarkdown) || MEDIA_MARKER.test(withoutMarkdown)) {
    return null;
  }

  return withoutMarkdown.replace(CAPTION_LABEL, "").trim() || null;
}

function isUppercaseHeadline(line: string): boolean {
  const upper = line.toLocaleUpperCase("vi-VN");
  const lower = line.toLocaleLowerCase("vi-VN");
  return upper !== lower && line === upper;
}

export function deriveTitleFromCaption(caption: string, fallback = "Untitled Post"): string {
  const lines = caption
    .replace(/\r\n?/g, "\n")
    .split("\n")
    .map((line, sourceIndex) => ({ sourceIndex, text: normalizeCaptionLine(line) }))
    .filter((line): line is { sourceIndex: number; text: string } => Boolean(line.text));

  const uppercaseStart = lines.slice(0, 3).findIndex((line) => isUppercaseHeadline(line.text));
  if (uppercaseStart >= 0) {
    const headlineLines = [lines[uppercaseStart]];
    for (let index = uppercaseStart + 1; index < lines.length; index += 1) {
      const current = lines[index];
      const previous = headlineLines[headlineLines.length - 1];
      if (current.sourceIndex !== previous.sourceIndex + 1 || !isUppercaseHeadline(current.text)) {
        break;
      }
      headlineLines.push(current);
    }

    return headlineLines.map((line) => line.text).join(" ").slice(0, 255);
  }

  const firstLine = lines[0]?.text;
  if (!firstLine) return fallback;

  const firstSentence = firstLine.match(/^.*?[.!?](?:\s|$)/)?.[0]?.trim() || firstLine;
  return firstSentence.slice(0, 255);
}
