import type { ConversationMessage } from "@/services/contentService";

export interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  text: string;
  canApply?: boolean;
  contentId?: string;
  canCreateNew?: boolean;
}

export interface Variation {
  id: string;
  prompt: string;
  result: string;
  contentId?: string;
  canCreateNew?: boolean;
}

export interface MediaMarkers {
  cleanText: string;
  imageUrl?: string;
  videoUrl?: string;
  videoJobId?: string;
}

const IMAGE_MARKER = /\[IMAGE:\s*(.+?)\]/g;
const VIDEO_URL_MARKER = /\[VIDEO_URL:\s*(.+?)\]/g;
const VIDEO_JOB_MARKER = /\[VIDEO_JOB:\s*(.+?)\]/g;

export function parseMediaMarkers(text: string): MediaMarkers {
  const imageUrl = text.match(/\[IMAGE:\s*(.+?)\]/)?.[1]?.trim();
  const videoUrl = text.match(/\[VIDEO_URL:\s*(.+?)\]/)?.[1]?.trim();
  const videoJobId = text.match(/\[VIDEO_JOB:\s*(.+?)\]/)?.[1]?.trim();

  return {
    cleanText: text
      .replace(IMAGE_MARKER, "")
      .replace(VIDEO_URL_MARKER, "")
      .replace(VIDEO_JOB_MARKER, "")
      .trim(),
    imageUrl: imageUrl || undefined,
    videoUrl: videoUrl || undefined,
    videoJobId: videoJobId || undefined,
  };
}

export function replaceVideoJobMarker(text: string, jobId: string, replacement: string): string {
  return text.replace(/\[VIDEO_JOB:\s*(.+?)\]/g, (marker, markerJobId: string) =>
    markerJobId.trim() === jobId ? replacement : marker
  ).trim();
}

export function restoreConversationHistory(messages: ConversationMessage[]): {
  chatMessages: ChatMessage[];
  variations: Variation[];
} {
  let latestUserPrompt = "";
  const chatMessages: ChatMessage[] = [];
  const variations: Variation[] = [];

  for (const message of messages) {
    const isUser = message.senderType === 0;
    const contentId = message.contentId || undefined;

    chatMessages.push({
      id: message.id,
      role: isUser ? "user" : "assistant",
      text: message.message,
      canApply: !isUser && Boolean(contentId),
      contentId,
    });

    if (isUser) {
      latestUserPrompt = message.message;
    } else if (contentId) {
      variations.unshift({
        id: message.id,
        prompt: latestUserPrompt,
        result: message.message,
        contentId,
      });
    }
  }

  return { chatMessages, variations };
}
