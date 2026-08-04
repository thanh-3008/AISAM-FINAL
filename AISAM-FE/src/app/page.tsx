"use client";

import { useEffect, useState, useRef, useCallback, type ReactNode } from "react";
import { useTheme } from "next-themes";
import Link from "next/link";

function useScrollProgress() {
  const [progress, setProgress] = useState(0);
  useEffect(() => {
    const handler = () => {
      const h = document.documentElement;
      const scrolled = h.scrollTop / (h.scrollHeight - h.clientHeight);
      setProgress(Math.min(scrolled, 1));
    };
    window.addEventListener("scroll", handler, { passive: true });
    return () => window.removeEventListener("scroll", handler);
  }, []);
  return progress;
}

function useInView(threshold = 0.15) {
  const ref = useRef<HTMLDivElement>(null);
  const [inView, setInView] = useState(false);
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const obs = new IntersectionObserver(([e]) => { if (e.isIntersecting) { setInView(true); obs.disconnect(); } }, { threshold });
    obs.observe(el);
    return () => obs.disconnect();
  }, [threshold]);
  return { ref, inView };
}

function Reveal({ children, delay = 0, direction = "up", className = "" }: { children: ReactNode; delay?: number; direction?: "up" | "left" | "right" | "scale"; className?: string }) {
  const { ref, inView } = useInView(0.1);
  const transforms: Record<string, string> = { up: "translateY(40px)", left: "translateX(-40px)", right: "translateX(40px)", scale: "scale(0.94)" };
  return (
    <div ref={ref} className={className} style={{ opacity: inView ? 1 : 0, transform: inView ? "none" : transforms[direction], transition: `opacity 0.7s cubic-bezier(0.16,1,0.3,1) ${delay}ms, transform 0.7s cubic-bezier(0.16,1,0.3,1) ${delay}ms` }}>
      {children}
    </div>
  );
}

function Stagger({ children, className = "" }: { children: ReactNode[]; className?: string }) {
  const { ref, inView } = useInView(0.05);
  return (
    <div ref={ref} className={className}>
      {children.map((child, i) => (
        <div key={i} style={{ opacity: inView ? 1 : 0, transform: inView ? "none" : "translateY(32px)", transition: `opacity 0.6s cubic-bezier(0.16,1,0.3,1) ${i * 100}ms, transform 0.6s cubic-bezier(0.16,1,0.3,1) ${i * 100}ms` }}>
          {child}
        </div>
      ))}
    </div>
  );
}

function CountUp({ value, suffix = "", duration = 1600 }: { value: number; suffix?: string; duration?: number }) {
  const [count, setCount] = useState(0);
  const { ref, inView } = useInView(0.5);
  const hasRun = useRef(false);
  useEffect(() => {
    if (!inView || hasRun.current) return;
    hasRun.current = true;
    const start = Date.now();
    const tick = () => {
      const p = Math.min((Date.now() - start) / duration, 1);
      setCount(Math.floor((1 - Math.pow(1 - p, 4)) * value));
      if (p < 1) requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
  }, [inView, value, duration]);
  return <span ref={ref}>{count.toLocaleString()}{suffix}</span>;
}

// Vệt sáng theo con trỏ chuột: cập nhật CSS variable qua ref thay vì
// setState mỗi lần mousemove, tránh re-render thừa.
function MouseGlow() {
  const elRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      elRef.current?.style.setProperty("--glow-x", `${e.clientX}px`);
      elRef.current?.style.setProperty("--glow-y", `${e.clientY}px`);
    };
    window.addEventListener("mousemove", handler, { passive: true });
    return () => window.removeEventListener("mousemove", handler);
  }, []);
  return (
    <div
      ref={elRef}
      className="pointer-events-none fixed inset-0 z-30 opacity-0 hover:opacity-100 transition-opacity duration-700"
      style={{ background: "radial-gradient(560px circle at var(--glow-x, 50%) var(--glow-y, 50%), rgba(0,62,199,0.05), transparent 60%)" }}
    />
  );
}

function DotGrid() {
  return (
    <div className="absolute inset-0 overflow-hidden opacity-[0.12]" style={{ backgroundImage: "radial-gradient(circle, #003ec7 1px, transparent 1px)", backgroundSize: "30px 30px" }} />
  );
}

function ScrollIndicator() {
  return (
    <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 animate-bounce-slow">
      <span className="text-label-xs text-outline uppercase tracking-widest">Scroll down</span>
      <div className="w-6 h-10 rounded-full border-2 border-outline/30 flex justify-center pt-2">
        <div className="w-1.5 h-3 bg-primary/60 rounded-full" style={{ animation: "scroll-dot 2s ease-in-out infinite" }} />
      </div>
    </div>
  );
}

function BackToTop() {
  const [show, setShow] = useState(false);
  useEffect(() => {
    const handler = () => setShow(window.scrollY > 600);
    window.addEventListener("scroll", handler, { passive: true });
    return () => window.removeEventListener("scroll", handler);
  }, []);
  return (
    <button onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })} aria-label="Back to top" className="fixed bottom-8 right-8 z-50 w-12 h-12 bg-primary text-on-primary rounded-full shadow-xl shadow-primary/25 flex items-center justify-center hover:scale-110 active:scale-95 transition-all" style={{ opacity: show ? 1 : 0, transform: show ? "translateY(0)" : "translateY(20px)", pointerEvents: show ? "auto" : "none", transition: "opacity 0.3s, transform 0.3s" }}>
      <span className="material-symbols-outlined text-[20px]" aria-hidden="true">keyboard_arrow_up</span>
    </button>
  );
}

function TiltCard({ children, className = "" }: { children: ReactNode; className?: string }) {
  const cardRef = useRef<HTMLDivElement>(null);
  const handleMove = useCallback((e: React.MouseEvent) => {
    const el = cardRef.current;
    if (!el) return;
    const rect = el.getBoundingClientRect();
    const x = (e.clientX - rect.left) / rect.width - 0.5;
    const y = (e.clientY - rect.top) / rect.height - 0.5;
    el.style.transform = `perspective(800px) rotateY(${x * 6}deg) rotateX(${-y * 6}deg) scale3d(1.015,1.015,1.015)`;
  }, []);
  const handleLeave = useCallback(() => {
    const el = cardRef.current;
    if (!el) return;
    el.style.transform = "perspective(800px) rotateY(0deg) rotateX(0deg) scale3d(1,1,1)";
  }, []);
  return (
    <div ref={cardRef} onMouseMove={handleMove} onMouseLeave={handleLeave} className={`transition-[box-shadow] duration-300 ${className}`} style={{ transformStyle: "preserve-3d" }}>
      {children}
    </div>
  );
}

function WorkspaceMock() {
  return (
    <div className="relative group hover-lift">
      <div className="absolute -inset-1 bg-gradient-to-r from-primary/20 via-secondary/20 to-primary/20 rounded-[2.2rem] blur-xl opacity-0 group-hover:opacity-100 transition-all duration-700 -z-10" />
      <div className="relative bg-surface-container-lowest rounded-[2rem] border border-outline-variant/20 shadow-2xl overflow-hidden">
        <div className="flex items-center justify-between px-5 py-3 bg-surface-container-low/80 border-b border-outline-variant/15">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-danger-red/40" />
            <div className="w-3 h-3 rounded-full bg-amber-500/40" />
            <div className="w-3 h-3 rounded-full bg-emerald-500/40" />
          </div>
          <div className="flex items-center gap-1.5 px-3 py-1 rounded-full bg-surface-container-lowest/80 border border-outline-variant/20">
            <span className="material-symbols-outlined text-primary text-[14px]" aria-hidden="true">workspaces</span>
            <span className="text-label-2xs font-semibold text-on-surface-variant tracking-wider">Workspace — Bloom Cafe</span>
          </div>
          <div className="w-12" />
        </div>
        <div className="relative overflow-hidden bg-surface-container-low/30">
          <img
            src="/demo0.png"
            alt="AISAM Dashboard Demo"
            className="w-full h-auto object-cover group-hover:scale-[1.01] transition-transform duration-700"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/10 via-transparent to-transparent pointer-events-none" />
        </div>
      </div>
    </div>
  );
}

export default function LandingPage() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);
  const { theme, setTheme } = useTheme();
  const progress = useScrollProgress();

  useEffect(() => {
    const handler = () => setScrolled(window.scrollY > 20);
    window.addEventListener("scroll", handler, { passive: true });
    return () => window.removeEventListener("scroll", handler);
  }, []);

  useEffect(() => {
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
      anchor.addEventListener("click", function (this: HTMLAnchorElement, e) {
        e.preventDefault();
        const targetId = this.getAttribute("href");
        if (!targetId || targetId === "#") return;
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
          const headerOffset = 80;
          const elementPosition = targetElement.getBoundingClientRect().top;
          const offsetPosition = elementPosition + window.scrollY - headerOffset;
          window.scrollTo({ top: offsetPosition, behavior: "smooth" });
          setMobileMenuOpen(false);
        }
      });
    });
  }, []);

  const navLinks: [string, string][] = [
    ["#features", "Features"],
    ["#how-it-works", "How it works"],
    ["#for-business", "For business"],
    ["#decisions", "Why AISAM"],
  ];

  const features = [
    {
      title: "Dedicated workspace for each brand",
      desc: "Every brand gets its own workspace: assets, campaigns, and team access permissions are fully isolated — not mingled under a single personal account.",
      icon: "workspaces",
    },
    {
      title: "Shared credit wallet",
      desc: "AI content generation, scheduling, and publishing all draw from a single unified credit wallet, eliminating complex per-feature quotas.",
      icon: "account_balance_wallet",
    },
    {
      title: "Content review pipeline",
      desc: "Import content plans in bulk, let AI draft each post, then review and approve before any content is scheduled for publication.",
      icon: "dynamic_feed",
    },
  ];

  const steps = [
    { step: "01", title: "Import content plan", desc: "Upload bulk schedules or briefs to a workspace. AISAM automatically separates them into individual posts tailored for each platform.", icon: "upload_file", image: "/demo1.png", features: ["Bulk import via file", "Auto-segment by platform", "Maintain brand tone of voice"] },
    { step: "02", title: "AI drafts, you approve", desc: "Content and images are generated by AI for each post and placed into an approval queue. Nothing goes live without your confirmation.", icon: "fact_check", image: "/demo2.png", features: ["Swipe-style review on app", "Edit before publishing", "Version history tracking"] },
    { step: "03", title: "Schedule and synchronize", desc: "Approved posts are queued and automatically published to connected accounts at the precisely scheduled times.", icon: "schedule_send", image: "/demo3.png", features: ["Facebook, Instagram, TikTok", "Auto-retry on publish error", "Notification upon completion"] },
  ];

  const businessPoints = [
    { value: 0, suffix: "Unlimited", label: "Workspaces / brands", desc: "Add new brands without needing separate individual accounts", display: "Unlimited" },
    { value: 3, suffix: "", label: "Directly synced platforms", desc: "Facebook, Instagram, TikTok — no manual publishing needed", display: null },
    { value: 100, suffix: "%", label: "AI content human-verified", desc: "No post goes live without explicit human confirmation", display: null },
  ];

  const decisions = [
    { title: "Workspace ownership, not personal", desc: "Campaigns and assets belong to the brand workspace — your team won't lose work history when someone leaves the group.", icon: "groups" },
    { title: "Credit wallet instead of fragmented quotas", desc: "A single balance pays for AI content generation, scheduling, and posting, rather than separate limits for each feature.", icon: "savings" },
    { title: "Nothing publishes without approval", desc: "All AI-drafted content stays in the approval queue. The AI proposes, humans decide.", icon: "verified" },
  ];

  return (
    <div className="bg-background text-on-surface font-body-md overflow-x-hidden">
      <style>{`
        @keyframes fade-up { from { opacity:0; transform:translateY(28px); } to { opacity:1; transform:translateY(0); } }
        @keyframes gradient-shift { 0% { background-position:0% 50%; } 50% { background-position:100% 50%; } 100% { background-position:0% 50%; } }
        @keyframes scroll-dot { 0%,100% { transform:translateY(0); opacity:1; } 50% { transform:translateY(8px); opacity:0.3; } }
        @keyframes gradient-border { 0% { background-position:0% 50%; } 50% { background-position:100% 50%; } 100% { background-position:0% 50%; } }
        .animate-bounce-slow { animation: scroll-dot 2.4s ease-in-out infinite; }
        .animate-gradient { background-size:200% 200%; animation:gradient-shift 6s ease infinite; }
        .glass-card { background:rgba(255,255,255,0.85); backdrop-filter:blur(20px); -webkit-backdrop-filter:blur(20px); border:1px solid rgba(225,225,238,0.4); }
        .feature-card { transition:all 0.5s cubic-bezier(0.16,1,0.3,1); }
        .feature-card:hover { transform:translateY(-6px); box-shadow:0 20px 48px -16px rgba(0,0,0,0.1); }
        .gradient-border { position:relative; }
        .gradient-border::before { content:''; position:absolute; inset:-1.5px; background:linear-gradient(135deg,#003ec7,#0f62fe); background-size:200% 200%; animation:gradient-border 5s ease infinite; border-radius:inherit; z-index:-1; opacity:0; transition:opacity 0.4s; }
        .gradient-border:hover::before { opacity:1; }
        .text-gradient-clip { background:linear-gradient(135deg,#003ec7 0%,#0f62fe 100%); -webkit-background-clip:text; background-clip:text; -webkit-text-fill-color:transparent; }
        .hover-lift { transition:transform 0.4s cubic-bezier(0.16,1,0.3,1), box-shadow 0.4s; }
        .hover-lift:hover { transform:translateY(-4px); box-shadow:0 16px 36px -12px rgba(0,0,0,0.1); }
        @media (prefers-reduced-motion: reduce) {
          * { animation-duration: 0.01ms !important; animation-iteration-count: 1 !important; transition-duration: 0.01ms !important; }
        }
        .dark body { background-color:#191b24; color:#eff0fd; }
        .dark .bg-background { background-color:#191b24 !important; }
        .dark .text-on-surface { color:#eff0fd !important; }
        .dark .text-on-surface-variant { color:#c3c6d8 !important; }
        .dark .bg-surface-container-lowest { background-color:#2e303a !important; }
        .dark .bg-surface-container-low { background-color:#23252e !important; }
        .dark .bg-surface-container { background-color:#2e303a !important; }
        .dark .border-outline-variant { border-color:rgba(255,255,255,0.1) !important; }
        .dark .glass-card { background:rgba(46,48,58,0.85) !important; border-color:rgba(255,255,255,0.1) !important; }
      `}</style>

      <MouseGlow />

      {/* Thanh tiến trình cuộn */}
      <div className="fixed top-0 left-0 w-full h-[3px] z-[60]">
        <div className="h-full bg-primary transition-[width] duration-150 ease-out" style={{ width: `${progress * 100}%` }} />
      </div>

      {/* Thanh điều hướng */}
      <nav className={`fixed top-0 w-full z-50 transition-all duration-500 ${scrolled ? "bg-surface-container-lowest/90 backdrop-blur-2xl shadow-lg shadow-black/5 border-b border-outline-variant/10" : "bg-transparent"}`}>
        <div className="flex justify-between items-center px-6 lg:px-8 max-w-7xl mx-auto h-20">
          <Link href="/" className="flex items-center gap-3 group">
            <div className="relative w-10 h-10 bg-primary rounded-xl flex items-center justify-center shadow-lg shadow-primary/20 group-hover:scale-105 transition-all duration-300">
              <span className="material-symbols-outlined text-on-primary text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">psychology</span>
            </div>
            <span className="text-headline-sm font-bold text-on-surface tracking-tight">AISAM</span>
          </Link>

          <div className="hidden md:flex items-center gap-8">
            {navLinks.map(([href, label]) => (
              <Link key={href} href={href} className="relative text-body-sm text-outline hover:text-on-surface font-semibold transition-colors group py-1">
                {label}
                <span className="absolute bottom-0 left-0 w-0 h-0.5 bg-primary group-hover:w-full transition-all duration-300" />
              </Link>
            ))}
          </div>

          <div className="hidden md:flex items-center gap-4">
            <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} className="relative w-8 h-8 rounded-lg bg-surface-container hover:bg-surface-container-high flex items-center justify-center transition-all hover:scale-110 active:scale-95" aria-label="Toggle light/dark theme">
              <span className={`material-symbols-outlined text-[16px] transition-all duration-300 ${theme === 'dark' ? "text-warning-amber" : "text-on-surface"}`} style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">
                {theme === 'dark' ? "dark_mode" : "light_mode"}
              </span>
            </button>
            <Link href="/login" className="text-body-sm text-outline hover:text-on-surface font-semibold transition-colors">Sign in</Link>
            <Link href="/register" className="relative px-6 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-all active:scale-95">
              Start free trial
            </Link>
          </div>

          <button onClick={() => setMobileMenuOpen(!mobileMenuOpen)} className="md:hidden p-2 rounded-xl hover:bg-surface-container transition-colors" aria-label="Open menu">
            <span className="material-symbols-outlined text-on-surface text-[24px]" aria-hidden="true">{mobileMenuOpen ? "close" : "menu"}</span>
          </button>
        </div>

        {mobileMenuOpen && (
          <div className="md:hidden bg-surface-container-lowest/95 backdrop-blur-2xl border-t border-outline-variant/10 shadow-xl" style={{ animation: "fade-up 0.3s ease-out" }}>
            <div className="px-6 py-6 space-y-4">
              {navLinks.map(([href, label]) => (
                <Link key={href} href={href} className="block text-body-lg text-on-surface font-semibold py-2">{label}</Link>
              ))}
              <div className="pt-4 border-t border-outline-variant/20 flex flex-col gap-3">
                <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} className="flex items-center gap-3 py-3 text-body-md text-on-surface font-semibold">
                  <span className="material-symbols-outlined text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">{theme === 'dark' ? "light_mode" : "dark_mode"}</span>
                  {theme === 'dark' ? "Light mode" : "Dark mode"}
                </button>
                <Link href="/login" className="text-center py-3 text-body-lg text-on-surface font-semibold">Sign in</Link>
                <Link href="/register" className="text-center py-3 bg-primary text-on-primary rounded-xl text-body-lg font-bold">Start free trial</Link>
              </div>
            </div>
          </div>
        )}
      </nav>

      <main className="pt-20">
        {/* Hero */}
        <section className="relative overflow-hidden min-h-[90vh] flex items-center py-20 px-6 lg:px-8">
          <div className="absolute inset-0 -z-10">
            <DotGrid />
            <div className="absolute top-[-10%] left-[-5%] w-[500px] h-[500px] bg-primary/8 rounded-full blur-[140px]" />
          </div>

          <div className="max-w-7xl mx-auto w-full grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-20 items-center relative z-10">
            <div>
              <Reveal delay={0}>
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/15 rounded-full mb-8">
                  <span className="material-symbols-outlined text-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">storefront</span>
                  <span className="text-label-sm text-primary font-semibold">For teams managing multiple brands</span>
                </div>
              </Reveal>

              <Reveal delay={100}>
                <h1 className="text-display-lg text-on-surface font-bold leading-[1.05] mb-6">
                  Draft, approve, and publish{" "}
                  <span className="text-gradient-clip">social media</span>{" "}
                  content from one workspace
                </h1>
              </Reveal>

              <Reveal delay={200}>
                <p className="text-body-lg text-on-surface-variant mb-10 max-w-xl leading-relaxed">
                  AISAM imports content plans in bulk, lets AI draft every post, and holds them for your review before scheduling to Facebook, Instagram, and TikTok — all tracked via a unified credit wallet per workspace.
                </p>
              </Reveal>

              <Reveal delay={300}>
                <div className="flex flex-col sm:flex-row gap-4 mb-10">
                  <Link href="/register" className="group relative inline-flex items-center justify-center gap-2 px-8 py-4 bg-primary text-on-primary rounded-2xl text-headline-sm font-bold shadow-xl shadow-primary/25 hover:scale-[1.02] transition-all active:scale-[0.98]">
                    <span>Start free trial</span>
                    <span className="material-symbols-outlined text-[20px] group-hover:translate-x-1 transition-transform" aria-hidden="true">arrow_forward</span>
                  </Link>
                  <Link href="#how-it-works" className="group inline-flex items-center justify-center gap-2 px-8 py-4 bg-surface-container-lowest/80 backdrop-blur text-on-surface rounded-2xl text-headline-sm font-semibold border border-outline-variant/20 hover:border-primary/30 hover:bg-surface-container transition-all active:scale-[0.98]">
                    <span className="material-symbols-outlined text-[20px] text-primary" aria-hidden="true">play_circle</span>
                    See how it works
                  </Link>
                </div>
              </Reveal>

              <Reveal delay={400}>
                <div className="flex flex-wrap items-center gap-6 text-label-sm text-outline">
                  <span>Now connected with</span>
                  {["Facebook", "Instagram", "TikTok"].map((p) => (
                    <span key={p} className="px-2.5 py-1 rounded-md border border-outline-variant/25 text-on-surface-variant font-medium">{p}</span>
                  ))}
                </div>
              </Reveal>
            </div>

            {/* Hình minh họa — giao diện thật của sản phẩm, không phải ảnh stock */}
            <div className="relative lg:block">
              <Reveal delay={300} direction="right">
                <WorkspaceMock />
              </Reveal>
            </div>
          </div>

          <ScrollIndicator />
        </section>

        {/* Tính năng */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative" id="features">
          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16 lg:mb-20">
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">What is actually inside a workspace</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">Three core pillars that your entire content operation revolves around.</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {features.map((f) => (
                <div key={f.title}>
                  <TiltCard>
                    <div className="feature-card gradient-border bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/15 shadow-sm h-full">
                      <div className="mb-6 w-12 h-12 bg-primary/10 flex items-center justify-center rounded-2xl">
                        <span className="material-symbols-outlined text-primary text-[24px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">{f.icon}</span>
                      </div>
                      <h3 className="text-headline-sm text-on-surface font-bold mb-3">{f.title}</h3>
                      <p className="text-body-md text-on-surface-variant">{f.desc}</p>
                    </div>
                  </TiltCard>
                </div>
              ))}
            </Stagger>

            <Reveal delay={100}>
              <div className="mt-6 rounded-3xl bg-enterprise-navy p-8 lg:p-10 flex flex-col sm:flex-row items-start sm:items-center gap-6 justify-between">
                <div>
                  <h3 className="text-headline-sm text-white font-bold mb-2">Platform Synchronization</h3>
                  <p className="text-body-sm text-outline-variant max-w-xl">Approved posts are pushed directly to connected accounts — no manual uploads needed for individual platforms.</p>
                </div>
                <div className="flex gap-2 flex-wrap">
                  {["Facebook", "Instagram", "TikTok"].map((p) => (
                    <span key={p} className="text-label-sm text-white/90 px-3 py-1.5 rounded-lg bg-white/10 border border-white/10">{p}</span>
                  ))}
                </div>
              </div>
            </Reveal>
          </div>
        </section>

        {/* Quy trình hoạt động */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative overflow-hidden" id="how-it-works">
          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-20 lg:mb-28">
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Content review pipeline</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">Every single post moves through exactly these three steps, in order.</p>
              </div>
            </Reveal>

            <div className="space-y-20 lg:space-y-28">
              {steps.map((item, i) => (
                <div key={item.step} className="relative">
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-16 items-center">
                    <Reveal direction={i % 2 === 0 ? "left" : "right"} delay={100} className={i % 2 === 1 ? "lg:order-2" : ""}>
                      <div className="relative group hover-lift">
                        <div className="absolute -inset-1 bg-gradient-to-r from-primary/20 via-secondary/20 to-primary/20 rounded-[2.2rem] blur-xl opacity-0 group-hover:opacity-100 transition-all duration-700 -z-10" />
                        <div className="relative bg-surface-container-lowest rounded-[2rem] border border-outline-variant/20 shadow-2xl overflow-hidden">
                          <div className="flex items-center justify-between px-5 py-3 bg-surface-container-low/80 border-b border-outline-variant/15">
                            <div className="flex items-center gap-2">
                              <div className="w-3 h-3 rounded-full bg-danger-red/40" />
                              <div className="w-3 h-3 rounded-full bg-amber-500/40" />
                              <div className="w-3 h-3 rounded-full bg-emerald-500/40" />
                            </div>
                            <div className="flex items-center gap-1.5 px-3 py-1 rounded-full bg-surface-container-lowest/80 border border-outline-variant/20">
                              <span className="material-symbols-outlined text-primary text-[14px]" aria-hidden="true">{item.icon}</span>
                              <span className="text-label-2xs font-semibold text-on-surface-variant uppercase tracking-wider">Step {item.step}</span>
                            </div>
                            <div className="w-12" />
                          </div>
                          <div className="relative overflow-hidden aspect-[16/10] bg-surface-container-low/30">
                            <img
                              src={item.image}
                              alt={`Step ${item.step} - ${item.title}`}
                              className="w-full h-full object-cover object-top group-hover:scale-[1.02] transition-transform duration-700"
                            />
                            <div className="absolute inset-0 bg-gradient-to-t from-black/10 via-transparent to-transparent pointer-events-none" />
                          </div>
                        </div>
                      </div>
                    </Reveal>

                    <Reveal direction={i % 2 === 0 ? "right" : "left"} delay={200} className={i % 2 === 1 ? "lg:order-1" : ""}>
                      <div className="space-y-6">
                        <h3 className="text-headline-lg text-on-surface font-bold">{item.title}</h3>
                        <p className="text-body-lg text-on-surface-variant leading-relaxed">{item.desc}</p>
                        <div className="space-y-3 pt-4">
                          {item.features.map((feature) => (
                            <div key={feature} className="flex items-center gap-3">
                              <div className="w-6 h-6 rounded-full bg-success-green/10 flex items-center justify-center flex-shrink-0">
                                <span className="material-symbols-outlined text-success-green text-[14px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">check</span>
                              </div>
                              <span className="text-body-md text-on-surface-variant">{feature}</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    </Reveal>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Dành cho doanh nghiệp */}
        <section className="py-20 lg:py-28 px-6 lg:px-8 relative bg-surface-container-low/30" id="for-business">
          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16">
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Built for teams managing multiple brands</h2>
                <p className="text-body-lg text-on-surface-variant">One system, multiple workspaces, with each brand having fully isolated data and budgets.</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {businessPoints.map((stat) => (
                <div key={stat.label} className="bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/15 shadow-sm text-center">
                  <div className="text-display-lg text-on-surface font-bold mb-2">
                    {stat.display ? stat.display : <CountUp value={stat.value} suffix={stat.suffix} />}
                  </div>
                  <p className="text-headline-sm text-on-surface font-semibold mb-2">{stat.label}</p>
                  <p className="text-body-sm text-on-surface-variant">{stat.desc}</p>
                </div>
              ))}
            </Stagger>
          </div>
        </section>

        {/* Why AISAM */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative" id="decisions">
          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16 lg:mb-20">
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Key architectural decisions</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">The foundational principles that shape how AISAM operates beyond just surface-level UI.</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {decisions.map((d) => (
                <div key={d.title}>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/15 shadow-sm h-full">
                    <div className="mb-5 w-11 h-11 bg-primary/10 rounded-xl flex items-center justify-center">
                      <span className="material-symbols-outlined text-primary text-[22px]" aria-hidden="true">{d.icon}</span>
                    </div>
                    <h3 className="text-headline-sm text-on-surface font-bold mb-3">{d.title}</h3>
                    <p className="text-body-md text-on-surface-variant leading-relaxed">{d.desc}</p>
                  </div>
                </div>
              ))}
            </Stagger>
          </div>
        </section>

        {/* CTA */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative overflow-hidden">
          <div className="max-w-4xl mx-auto">
            <Reveal direction="scale">
              <div className="relative glass-card p-10 lg:p-16 rounded-[2rem] shadow-2xl text-center overflow-hidden">
                <div className="absolute top-0 left-0 w-full h-1 bg-primary" />

                <div className="relative z-10">
                  <h2 className="text-headline-lg text-on-surface font-bold mb-4">Ready to streamline your social content?</h2>
                  <p className="text-body-lg text-on-surface-variant mb-10 max-w-2xl mx-auto">Create your first workspace, connect your platforms, and let AI handle drafting — all you do is approve.</p>

                  <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                    <Link href="/register" className="w-full sm:w-auto group relative inline-flex items-center justify-center gap-2 px-10 py-5 bg-primary text-on-primary rounded-2xl text-headline-sm font-bold shadow-xl shadow-primary/25 hover:scale-[1.02] transition-all active:scale-[0.98]">
                      <span>Create free workspace</span>
                      <span className="material-symbols-outlined text-[20px] group-hover:translate-x-1 transition-transform" aria-hidden="true">arrow_forward</span>
                    </Link>
                    <Link href="/login" className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-10 py-5 bg-surface-container-lowest/80 backdrop-blur text-on-surface rounded-2xl text-headline-sm font-semibold border border-outline-variant/20 hover:border-primary/30 hover:bg-surface-container transition-all active:scale-[0.98]">
                      Sign in
                    </Link>
                  </div>

                  <div className="flex flex-wrap justify-center items-center gap-6 text-label-sm text-outline mt-8">
                    {["No credit card required", "14-day free trial", "Cancel anytime"].map((text) => (
                      <div key={text} className="flex items-center gap-2">
                        <span className="material-symbols-outlined text-success-green text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">check_circle</span>
                        <span>{text}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </Reveal>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="bg-enterprise-navy text-white relative overflow-hidden">
        <div className="relative z-10 max-w-7xl mx-auto px-6 lg:px-8 py-16">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12">
            <div className="lg:col-span-1">
              <Link href="/" className="flex items-center gap-3 mb-6 group">
                <div className="w-10 h-10 bg-primary rounded-xl flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }} aria-hidden="true">psychology</span>
                </div>
                <span className="text-headline-sm font-bold">AISAM</span>
              </Link>
              <p className="text-body-sm text-outline-variant mb-6 leading-relaxed">A unified platform for automating social media content and advertising management across multiple brands.</p>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Product</h4>
              <ul className="space-y-4">
                {navLinks.map(([href, label]) => (
                  <li key={href}><Link href={href} className="text-body-sm text-outline-variant hover:text-white transition-colors inline-block">{label}</Link></li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Legal</h4>
              <ul className="space-y-4">
                <li><Link href="/terms" className="text-body-sm text-outline-variant hover:text-white transition-colors inline-block">Terms of Service</Link></li>
                <li><Link href="/privacy" className="text-body-sm text-outline-variant hover:text-white transition-colors inline-block">Privacy Policy</Link></li>
              </ul>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Contact</h4>
              <p className="text-body-sm text-outline-variant leading-relaxed">Need support or want to learn more about AISAM? Contact our customer success team for guidance.</p>
            </div>
          </div>
        </div>

        <div className="relative z-10 border-t border-white/10">
          <div className="max-w-7xl mx-auto px-6 lg:px-8 py-6 flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-label-sm text-outline-variant">&copy; 2026 AISAM. All rights reserved.</p>
            <div className="flex gap-6">
              <Link href="/privacy" className="text-label-sm text-outline-variant hover:text-white">Privacy</Link>
              <Link href="/terms" className="text-label-sm text-outline-variant hover:text-white">Terms</Link>
            </div>
          </div>
        </div>
      </footer>

      <BackToTop />
    </div>
  );
}