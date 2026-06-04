import Link from "next/link";

export default function AiLandingPage() {
  return (
    <div className="flex flex-col items-center justify-center gap-6 py-20">
      <h1 className="text-2xl font-semibold">AI Assistant</h1>
      <p className="text-muted-foreground">Select a feature to get started.</p>
      <div className="flex gap-4">
        <Link
          href="/ai/conversations"
          className="rounded-xl border bg-card px-6 py-4 font-medium shadow-panel transition-colors hover:bg-muted"
        >
          Conversation History
        </Link>
      </div>
    </div>
  );
}
