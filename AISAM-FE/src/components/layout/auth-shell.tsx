import { ReactNode } from "react";

export function AuthShell({ title, description, children }: { title: string; description: string; children: ReactNode }) {
  return (
    <div className="grid min-h-screen lg:grid-cols-[1.1fr_0.9fr]">
      <div className="hidden bg-primary px-12 py-16 text-primary-foreground lg:flex lg:flex-col lg:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.3em] opacity-80">AISAM</p>
          <h1 className="mt-8 max-w-md text-5xl font-semibold leading-tight">
            AI-powered social media workflows for small teams.
          </h1>
        </div>
        <p className="max-w-md text-sm opacity-85">
          Foundation scaffolded against the active AISAM backend contracts so frontend implementation can proceed story by story.
        </p>
      </div>
      <div className="flex items-center justify-center px-6 py-10">
        <div className="w-full max-w-md rounded-3xl bg-card p-8 shadow-panel">
          <div className="mb-8">
            <h2 className="text-3xl font-semibold">{title}</h2>
            <p className="mt-2 text-sm text-muted-foreground">{description}</p>
          </div>
          {children}
        </div>
      </div>
    </div>
  );
}
