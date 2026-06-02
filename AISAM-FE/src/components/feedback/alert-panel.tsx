type AlertPanelProps = {
  title: string;
  description: string;
  tone?: "neutral" | "error" | "success";
};

export function AlertPanel({ title, description, tone = "neutral" }: AlertPanelProps) {
  const toneClass =
    tone === "error"
      ? "border-destructive/30 bg-destructive/5 text-destructive"
      : tone === "success"
        ? "border-primary/30 bg-primary/5 text-primary"
        : "border-border bg-card text-foreground";

  return (
    <div className={`rounded-xl border p-4 ${toneClass}`}>
      <h3 className="font-medium">{title}</h3>
      <p className="mt-1 text-sm opacity-90">{description}</p>
    </div>
  );
}
