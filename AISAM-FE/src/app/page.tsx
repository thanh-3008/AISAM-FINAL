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
  const transforms: Record<string, string> = { up: "translateY(48px)", left: "translateX(-48px)", right: "translateX(48px)", scale: "scale(0.92)" };
  return (
    <div ref={ref} className={className} style={{ opacity: inView ? 1 : 0, transform: inView ? "none" : transforms[direction], transition: `opacity 0.8s cubic-bezier(0.16,1,0.3,1) ${delay}ms, transform 0.8s cubic-bezier(0.16,1,0.3,1) ${delay}ms` }}>
      {children}
    </div>
  );
}

function Stagger({ children, className = "" }: { children: ReactNode[]; className?: string }) {
  const { ref, inView } = useInView(0.05);
  return (
    <div ref={ref} className={className}>
      {children.map((child, i) => (
        <div key={i} style={{ opacity: inView ? 1 : 0, transform: inView ? "none" : "translateY(40px)", transition: `opacity 0.7s cubic-bezier(0.16,1,0.3,1) ${i * 120}ms, transform 0.7s cubic-bezier(0.16,1,0.3,1) ${i * 120}ms` }}>
          {child}
        </div>
      ))}
    </div>
  );
}

function CountUp({ value, suffix = "", duration = 2200 }: { value: number; suffix?: string; duration?: number }) {
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

function MouseGlow() {
  const [pos, setPos] = useState({ x: 0, y: 0 });
  useEffect(() => {
    const handler = (e: MouseEvent) => setPos({ x: e.clientX, y: e.clientY });
    window.addEventListener("mousemove", handler);
    return () => window.removeEventListener("mousemove", handler);
  }, []);
  return <div className="pointer-events-none fixed inset-0 z-30 opacity-0 hover:opacity-100 transition-opacity duration-700" style={{ background: `radial-gradient(600px circle at ${pos.x}px ${pos.y}px, rgba(0,76,205,0.04), transparent 60%)` }} />;
}

function DotGrid() {
  return (
    <div className="absolute inset-0 overflow-hidden opacity-[0.15]" style={{ backgroundImage: "radial-gradient(circle, #004ccd 1px, transparent 1px)", backgroundSize: "32px 32px" }} />
  );
}

function MorphBlob({ className = "" }: { className?: string }) {
  return (
    <div className={`absolute rounded-full blur-[100px] opacity-30 ${className}`} style={{ animation: "morph 12s ease-in-out infinite", background: "linear-gradient(135deg, #004ccd, #731be5, #0f62fe)" }} />
  );
}

function Marquee({ items }: { items: string[] }) {
  const doubled = [...items, ...items];
  return (
    <div className="overflow-hidden [mask-image:linear-gradient(to_right,transparent,black_15%,black_85%,transparent)]">
      <div className="flex gap-16 items-center animate-marquee whitespace-nowrap">
        {doubled.map((item, i) => (
          <span key={i} className="text-headline-sm font-bold text-outline/40 hover:text-outline/70 transition-colors cursor-default select-none flex-shrink-0">{item}</span>
        ))}
      </div>
    </div>
  );
}

function ScrollIndicator() {
  return (
    <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 animate-bounce-slow">
      <span className="text-label-xs text-outline uppercase tracking-widest">Scroll</span>
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
    <button onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })} className="fixed bottom-8 right-8 z-50 w-12 h-12 bg-primary text-on-primary rounded-full shadow-xl shadow-primary/25 flex items-center justify-center hover:scale-110 active:scale-95 transition-all" style={{ opacity: show ? 1 : 0, transform: show ? "translateY(0)" : "translateY(20px)", pointerEvents: show ? "auto" : "none", transition: "opacity 0.3s, transform 0.3s" }}>
      <span className="material-symbols-outlined text-[20px]">keyboard_arrow_up</span>
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
    el.style.transform = `perspective(800px) rotateY(${x * 8}deg) rotateX(${-y * 8}deg) scale3d(1.02,1.02,1.02)`;
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

  return (
    <div className="bg-background text-on-surface font-body-md overflow-x-hidden">
      <style>{`
        @keyframes fade-up { from { opacity:0; transform:translateY(32px); } to { opacity:1; transform:translateY(0); } }
        @keyframes float { 0%,100% { transform:translateY(0px) rotate(0deg); } 50% { transform:translateY(-14px) rotate(1deg); } }
        @keyframes float-reverse { 0%,100% { transform:translateY(0px) rotate(0deg); } 50% { transform:translateY(10px) rotate(-1deg); } }
        @keyframes pulse-glow { 0%,100% { opacity:0.3; transform:scale(1); } 50% { opacity:0.6; transform:scale(1.05); } }
        @keyframes gradient-shift { 0% { background-position:0% 50%; } 50% { background-position:100% 50%; } 100% { background-position:0% 50%; } }
        @keyframes shimmer { 0% { transform:translateX(-100%); } 100% { transform:translateX(100%); } }
        @keyframes morph { 0%,100% { border-radius:60% 40% 30% 70%/60% 30% 70% 40%; } 50% { border-radius:30% 60% 70% 40%/50% 60% 30% 60%; } }
        @keyframes marquee { 0% { transform:translateX(0); } 100% { transform:translateX(-50%); } }
        @keyframes scroll-dot { 0%,100% { transform:translateY(0); opacity:1; } 50% { transform:translateY(8px); opacity:0.3; } }
        @keyframes spin-slow { 0% { transform:rotate(0deg); } 100% { transform:rotate(360deg); } }
        @keyframes dash { to { stroke-dashoffset:0; } }
        @keyframes gradient-border { 0% { background-position:0% 50%; } 50% { background-position:100% 50%; } 100% { background-position:0% 50%; } }
        @keyframes text-reveal { from { clip-path:inset(0 100% 0 0); } to { clip-path:inset(0 0 0 0); } }
        @keyframes scale-in { from { transform:scale(0.8); opacity:0; } to { transform:scale(1); opacity:1; } }
        @keyframes orbit { 0% { transform:rotate(0deg) translateX(120px) rotate(0deg); } 100% { transform:rotate(360deg) translateX(120px) rotate(-360deg); } }
        .animate-fade-up { animation:fade-up 0.7s cubic-bezier(0.16,1,0.3,1) forwards; }
        .animate-float { animation:float 6s ease-in-out infinite; }
        .animate-float-reverse { animation:float-reverse 5s ease-in-out infinite; }
        .animate-pulse-glow { animation:pulse-glow 4s ease-in-out infinite; }
        .animate-gradient { background-size:200% 200%; animation:gradient-shift 6s ease infinite; }
        .animate-marquee { animation:marquee 30s linear infinite; }
        .animate-spin-slow { animation:spin-slow 20s linear infinite; }
        .glass-card { background:rgba(255,255,255,0.8); backdrop-filter:blur(20px); -webkit-backdrop-filter:blur(20px); border:1px solid rgba(225,225,238,0.4); }
        .feature-card { transition:all 0.5s cubic-bezier(0.16,1,0.3,1); }
        .feature-card:hover { transform:translateY(-8px); box-shadow:0 24px 64px -16px rgba(0,0,0,0.12); }
        .gradient-border { position:relative; }
        .gradient-border::before { content:''; position:absolute; inset:-2px; background:linear-gradient(135deg,#004ccd,#731be5,#0f62fe,#8d42ff); background-size:300% 300%; animation:gradient-border 4s ease infinite; border-radius:inherit; z-index:-1; opacity:0; transition:opacity 0.4s; }
        .gradient-border:hover::before { opacity:1; }
        .text-gradient-clip { background:linear-gradient(135deg,#004ccd 0%,#731be5 50%,#0f62fe 100%); background-size:200% 200%; animation:gradient-shift 4s ease infinite; -webkit-background-clip:text; background-clip:text; -webkit-text-fill-color:transparent; }
        .hover-lift { transition:transform 0.4s cubic-bezier(0.16,1,0.3,1), box-shadow 0.4s; }
        .hover-lift:hover { transform:translateY(-6px); box-shadow:0 20px 40px -12px rgba(0,0,0,0.1); }
        .step-line { position:relative; }
        .step-line::after { content:''; position:absolute; top:50%; right:-50%; width:100%; height:2px; background:linear-gradient(to_right,#004ccd,#731be5); opacity:0.2; }
        @media(max-width:768px) { .step-line::after { display:none; } }
        .dark body { background-color:#191b24; color:#eff0fd; }
        .dark .bg-background { background-color:#191b24 !important; }
        .dark .text-on-surface { color:#eff0fd !important; }
        .dark .text-on-surface-variant { color:#c3c6d8 !important; }
        .dark .bg-surface-container-lowest { background-color:#2e303a !important; }
        .dark .bg-surface-container-low { background-color:#23252e !important; }
        .dark .bg-surface-container { background-color:#2e303a !important; }
        .dark .border-outline-variant { border-color:rgba(255,255,255,0.1) !important; }
        .dark .glass-card { background:rgba(46,48,58,0.8) !important; border-color:rgba(255,255,255,0.1) !important; }
      `}</style>

      <MouseGlow />

      {/* Scroll Progress */}
      <div className="fixed top-0 left-0 w-full h-[3px] z-[60]">
        <div className="h-full bg-gradient-to-r from-primary via-secondary to-primary-container transition-[width] duration-150 ease-out" style={{ width: `${progress * 100}%` }} />
      </div>

      {/* Navbar */}
      <nav className={`fixed top-0 w-full z-50 transition-all duration-500 ${scrolled ? "bg-surface-container-lowest/90 backdrop-blur-2xl shadow-lg shadow-black/5 border-b border-outline-variant/10" : "bg-transparent"}`}>
        <div className="flex justify-between items-center px-6 lg:px-8 max-w-7xl mx-auto h-20">
          <Link href="/" className="flex items-center gap-3 group">
            <div className="relative w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20 group-hover:scale-110 group-hover:shadow-xl group-hover:shadow-primary/30 transition-all duration-300">
              <span className="material-symbols-outlined text-on-primary text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }}>psychology</span>
              <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-secondary opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            </div>
            <span className="text-headline-sm font-bold text-on-surface tracking-tight">AISAM</span>
          </Link>

          <div className="hidden md:flex items-center gap-8">
            {[["#features", "Features"], ["#how-it-works", "How it Works"], ["#stats", "Results"], ["#testimonials", "Testimonials"]].map(([href, label]) => (
              <Link key={href} href={href} className="relative text-body-sm text-outline hover:text-on-surface font-semibold transition-colors group py-1">
                {label}
                <span className="absolute bottom-0 left-0 w-0 h-0.5 bg-gradient-to-r from-primary to-secondary group-hover:w-full transition-all duration-300" />
              </Link>
            ))}
          </div>

          <div className="hidden md:flex items-center gap-4">
            <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} className="relative w-8 h-8 rounded-lg bg-surface-container hover:bg-surface-container-high flex items-center justify-center transition-all hover:scale-110 active:scale-95 group" aria-label="Toggle dark mode">
              <span className={`material-symbols-outlined text-[16px] transition-all duration-300 ${theme === 'dark' ? "text-warning-amber" : "text-on-surface"}`} style={{ fontVariationSettings: "'FILL' 1" }}>
                {theme === 'dark' ? "dark_mode" : "light_mode"}
              </span>
            </button>
            <Link href="/login" className="text-body-sm text-outline hover:text-on-surface font-semibold transition-colors">Log In</Link>
            <Link href="/register" className="relative px-6 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 hover:shadow-xl hover:shadow-primary/30 transition-all active:scale-95 overflow-hidden group">
              <span className="relative z-10">Start Free Trial</span>
              <div className="absolute inset-0 bg-gradient-to-r from-secondary to-primary opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            </Link>
          </div>

          <button onClick={() => setMobileMenuOpen(!mobileMenuOpen)} className="md:hidden p-2 rounded-xl hover:bg-surface-container transition-colors">
            <span className="material-symbols-outlined text-on-surface text-[24px]">{mobileMenuOpen ? "close" : "menu"}</span>
          </button>
        </div>

        {mobileMenuOpen && (
          <div className="md:hidden bg-surface-container-lowest/95 backdrop-blur-2xl border-t border-outline-variant/10 shadow-xl" style={{ animation: "fade-up 0.3s ease-out" }}>
            <div className="px-6 py-6 space-y-4">
              {[["#features", "Features"], ["#how-it-works", "How it Works"], ["#stats", "Results"], ["#testimonials", "Testimonials"]].map(([href, label]) => (
                <Link key={href} href={href} className="block text-body-lg text-on-surface font-semibold py-2">{label}</Link>
              ))}
              <div className="pt-4 border-t border-outline-variant/20 flex flex-col gap-3">
                <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} className="flex items-center gap-3 py-3 text-body-md text-on-surface font-semibold">
                  <span className="material-symbols-outlined text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }}>{theme === 'dark' ? "light_mode" : "dark_mode"}</span>
                  {theme === 'dark' ? "Light Mode" : "Dark Mode"}
                </button>
                <Link href="/login" className="text-center py-3 text-body-lg text-on-surface font-semibold">Log In</Link>
                <Link href="/register" className="text-center py-3 bg-primary text-on-primary rounded-xl text-body-lg font-bold">Start Free Trial</Link>
              </div>
            </div>
          </div>
        )}
      </nav>

      <main className="pt-20">
        {/* Hero Section */}
        <section className="relative overflow-hidden min-h-[95vh] flex items-center py-20 px-6 lg:px-8">
          <div className="absolute inset-0 -z-10">
            <DotGrid />
            <MorphBlob className="w-[600px] h-[600px] top-[-10%] left-[-5%]" />
            <MorphBlob className="w-[500px] h-[500px] bottom-[-10%] right-[-5%]" />
            <div className="absolute top-1/3 right-1/4 w-[300px] h-[300px] bg-secondary/10 rounded-full blur-[120px] animate-pulse-glow" />
            <div className="absolute bottom-1/3 left-1/3 w-[400px] h-[400px] bg-primary/8 rounded-full blur-[150px] animate-pulse-glow" style={{ animationDelay: "2s" }} />
          </div>

          <div className="max-w-7xl mx-auto w-full grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-20 items-center relative z-10">
            <div>
              <Reveal delay={0}>
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/15 rounded-full mb-8 hover:bg-primary/10 transition-colors group cursor-default">
                  <span className="material-symbols-outlined text-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
                  <span className="text-label-sm text-primary font-semibold group-hover:tracking-wider transition-all">AI-Powered Precision</span>
                  <span className="w-2 h-2 rounded-full bg-success-green animate-pulse" />
                </div>
              </Reveal>

              <Reveal delay={100}>
                <h1 className="text-display-lg text-on-surface font-bold leading-[1.05] mb-6">
                  Master Social Ads with{" "}
                  <span className="text-gradient-clip">AI-Powered</span>{" "}
                  Intelligence
                </h1>
              </Reveal>

              <Reveal delay={200}>
                <p className="text-body-lg text-on-surface-variant mb-10 max-w-xl leading-relaxed">
                  Automate content creation, optimize ad spend, and scale your brand across all social platforms — all powered by enterprise-grade AI.
                </p>
              </Reveal>

              <Reveal delay={300}>
                <div className="flex flex-col sm:flex-row gap-4 mb-10">
                  <Link href="/register" className="group relative inline-flex items-center justify-center gap-2 px-8 py-4 bg-primary text-on-primary rounded-2xl text-headline-sm font-bold shadow-xl shadow-primary/25 hover:scale-[1.03] hover:shadow-2xl hover:shadow-primary/40 transition-all active:scale-[0.98] overflow-hidden">
                    <span className="relative z-10">Start Free Trial</span>
                    <span className="material-symbols-outlined text-[20px] relative z-10 group-hover:translate-x-1 transition-transform">arrow_forward</span>
                    <div className="absolute inset-0 bg-gradient-to-r from-secondary to-primary-container opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
                  </Link>
                  <Link href="#features" className="group inline-flex items-center justify-center gap-2 px-8 py-4 bg-surface-container-lowest/80 backdrop-blur text-on-surface rounded-2xl text-headline-sm font-semibold border border-outline-variant/20 hover:border-primary/30 hover:bg-surface-container transition-all active:scale-[0.98]">
                    <span className="material-symbols-outlined text-[20px] text-primary group-hover:scale-110 transition-transform">play_circle</span>
                    See Features
                  </Link>
                </div>
              </Reveal>

              <Reveal delay={400}>
                <div className="flex flex-wrap items-center gap-6 text-label-sm text-outline">
                  {[["No credit card"], ["14-day free trial"], ["Cancel anytime"]].map(([text], i) => (
                    <div key={i} className="flex items-center gap-2">
                      <span className="material-symbols-outlined text-success-green text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                      <span>{text}</span>
                    </div>
                  ))}
                </div>
              </Reveal>
            </div>

            {/* Hero Visual */}
            <div className="relative lg:block">
              <Reveal delay={300} direction="right">
                <div className="relative">
                  {/* Orbiting elements */}
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div className="w-[280px] h-[280px] border border-outline-variant/10 rounded-full animate-spin-slow" />
                  </div>
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div className="w-[380px] h-[380px] border border-outline-variant/5 rounded-full animate-spin-slow" style={{ animationDirection: "reverse", animationDuration: "30s" }} />
                  </div>

                  <div className="relative rounded-3xl overflow-hidden shadow-2xl shadow-black/15 border border-outline-variant/15 hover-lift">
                    <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-secondary/5 z-10" />
                    <img alt="AISAM Dashboard" className="w-full h-full object-cover aspect-[4/3]" src="https://lh3.googleusercontent.com/aida-public/AB6AXuBsAfZbKZ3IOo1LjRLd0u1lBBYKzL_wvAzTf5wrxIFXfZF3d0uyJoK9F9JcLjRA53GNwAvVAqalluJWOMZOGHIL5ZH1rYX8_oCrru380oL7v7XQ-J1fZmtOtdx6fika1eGrJJMwgzPnktI8lA4ftCTSenjNsup_Z34n-mmBG790ybRc24vmGKxyiFXysrO6Y_9RFxBjWyEBdNYwrrZFMXsfPX9RsMOXc7bgR4l_YxqLwYxahJEDGQBi34vQ5pZqGQPEZ8GGB4vs5Ec" />
                  </div>

                  {/* Floating Card - ROI */}
                  <div className="absolute -bottom-6 -left-6 glass-card p-5 rounded-2xl shadow-xl animate-float z-20">
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 rounded-xl bg-success-green/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-success-green text-[24px]">trending_up</span>
                      </div>
                      <div>
                        <div className="text-headline-md text-on-surface font-bold">+142%</div>
                        <div className="text-label-xs text-outline">Average ROI</div>
                      </div>
                    </div>
                  </div>

                  {/* Floating Card - Active */}
                  <div className="absolute -top-4 -right-4 glass-card p-4 rounded-2xl shadow-xl animate-float-reverse z-20" style={{ animationDelay: "0.5s" }}>
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-primary text-[20px]">campaign</span>
                      </div>
                      <div>
                        <div className="text-body-sm text-on-surface font-bold">24 Active</div>
                        <div className="text-label-2xs text-outline">Campaigns</div>
                      </div>
                    </div>
                  </div>

                  {/* Floating Card - AI Score */}
                  <div className="absolute top-1/2 -right-10 glass-card p-3 rounded-2xl shadow-xl animate-float z-20 hidden lg:block" style={{ animationDelay: "1.5s" }}>
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 rounded-lg bg-secondary/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-secondary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
                      </div>
                      <div>
                        <div className="text-body-sm text-on-surface font-bold">AI Score: 98</div>
                        <div className="text-label-2xs text-outline">Performance</div>
                      </div>
                    </div>
                  </div>
                </div>
              </Reveal>
            </div>
          </div>

          <ScrollIndicator />
        </section>

        {/* Trusted By Section */}
        <section className="py-16 px-6 lg:px-8 border-y border-outline-variant/10 bg-surface-container-low/20">
          <div className="max-w-7xl mx-auto">
            <Reveal>
              <p className="text-center text-label-sm text-outline uppercase tracking-[0.2em] font-semibold mb-12">Trusted by 10,000+ brands worldwide</p>
            </Reveal>
            <Marquee items={["Google", "Meta", "Shopify", "HubSpot", "Salesforce", "Stripe", "Notion", "Figma"]} />
          </div>
        </section>

        {/* Features Section */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative" id="features">
          <div className="absolute top-0 right-0 w-[500px] h-[500px] bg-primary/5 rounded-full blur-[200px] -z-10" />
          <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-secondary/5 rounded-full blur-[200px] -z-10" />

          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16 lg:mb-20">
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-secondary/5 border border-secondary/15 rounded-full mb-6">
                  <span className="material-symbols-outlined text-secondary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>spark</span>
                  <span className="text-label-sm text-secondary font-semibold">Powerful Features</span>
                </div>
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Everything You Need to Scale</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">Our AI agents work 24/7 to optimize your social media strategy for peak performance.</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Feature 1: AI Studio */}
              <div>
                <TiltCard>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 lg:p-10 rounded-3xl border border-outline-variant/15 shadow-sm overflow-hidden relative group h-full">
                    <div className="absolute top-0 right-0 w-72 h-72 bg-primary/5 rounded-full blur-[100px] -mr-36 -mt-36 group-hover:bg-primary/10 transition-colors duration-700" />
                    <div className="relative z-10">
                      <div className="mb-6 w-14 h-14 bg-gradient-to-br from-primary to-primary/70 flex items-center justify-center rounded-2xl shadow-lg shadow-primary/20 group-hover:scale-110 group-hover:rotate-3 transition-all duration-500">
                        <span className="material-symbols-outlined text-on-primary text-[28px]" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
                      </div>
                      <h3 className="text-headline-md text-on-surface font-bold mb-3">AI Studio</h3>
                      <p className="text-body-md text-on-surface-variant mb-8 max-w-sm">Generate high-converting copy and photorealistic images in seconds. Tailored to your brand&apos;s unique voice.</p>
                      <div className="relative rounded-2xl overflow-hidden h-48 border border-outline-variant/15 shadow-inner group-hover:shadow-lg transition-shadow duration-500">
                        <img alt="AI Studio" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" src="https://lh3.googleusercontent.com/aida-public/AB6AXuBTgt3rCAc24KNrdsrNae8VEPCAqWpfmt-IspWuU_kH9DoH1zEwhoLqUxnEeAxzXvRLnA6dEmfub8-xJay6Hui_4V-sdwlishZRJ7vb67lCXuUOT9Zm3iSAUhnMGUyQTXHpxei4yPwt3mEyAKj1vpdf0_DWv0UzTsnRFLTL4Bplych6hQuWcUFIxxbufLCvlst1M4KFYU4YUMRmvkVhEYZ7Qem3S39dCTo2tINq6PCVUx98bdtxWb9iq85uhiAc2z3JxQbIPRyRTyA" />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent" />
                      </div>
                    </div>
                  </div>
                </TiltCard>
              </div>

              {/* Feature 2: Smart Campaigns */}
              <div>
                <TiltCard>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 lg:p-10 rounded-3xl border border-outline-variant/15 shadow-sm overflow-hidden relative group h-full">
                    <div className="absolute top-0 right-0 w-72 h-72 bg-secondary/5 rounded-full blur-[100px] -mr-36 -mt-36 group-hover:bg-secondary/10 transition-colors duration-700" />
                    <div className="relative z-10">
                      <div className="mb-6 w-14 h-14 bg-gradient-to-br from-secondary to-secondary/70 flex items-center justify-center rounded-2xl shadow-lg shadow-secondary/20 group-hover:scale-110 group-hover:rotate-3 transition-all duration-500">
                        <span className="material-symbols-outlined text-on-primary text-[28px]" style={{ fontVariationSettings: "'FILL' 1" }}>campaign</span>
                      </div>
                      <h3 className="text-headline-md text-on-surface font-bold mb-3">Smart Campaigns</h3>
                      <p className="text-body-md text-on-surface-variant mb-8 max-w-sm">Automated ad management with real-time bid and targeting optimization for maximum efficiency.</p>
                      <div className="relative rounded-2xl overflow-hidden h-48 border border-outline-variant/15 shadow-inner group-hover:shadow-lg transition-shadow duration-500">
                        <img alt="Campaigns" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" src="https://lh3.googleusercontent.com/aida-public/AB6AXuCOzlVuqVmDNE2z_zBTMW5P4zZfhMQQBkgecnfo87Ldj2XKgUj-8zAje0poJKofypLe57wgfUz_Knv6PW6mpwz2c2fKe26DXSm1N2NJwHY0Mgnxx5cPstuRzEr5ebqnfsGb1b9B52099F3oGoLpf66PbPVgkHhnWYfkGQXVnmU_ws8_gdO2tJrQ4cA2UnUnba2kp8dbErUvlDaLTlTraVs4ADV-vX1xzspCXKAfC39X88CTINMPd2KCZAds50__jUApAauBhOi22rw" />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent" />
                      </div>
                    </div>
                  </div>
                </TiltCard>
              </div>

              {/* Feature 3: Content Library */}
              <div>
                <TiltCard>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 lg:p-10 rounded-3xl border border-outline-variant/15 shadow-sm overflow-hidden relative group h-full">
                    <div className="absolute top-0 right-0 w-72 h-72 bg-tertiary/5 rounded-full blur-[100px] -mr-36 -mt-36 group-hover:bg-tertiary/10 transition-colors duration-700" />
                    <div className="relative z-10">
                      <div className="mb-6 w-14 h-14 bg-gradient-to-br from-tertiary to-tertiary/70 flex items-center justify-center rounded-2xl shadow-lg shadow-tertiary/20 group-hover:scale-110 group-hover:rotate-3 transition-all duration-500">
                        <span className="material-symbols-outlined text-on-primary text-[28px]" style={{ fontVariationSettings: "'FILL' 1" }}>folder_special</span>
                      </div>
                      <h3 className="text-headline-md text-on-surface font-bold mb-3">Content Library</h3>
                      <p className="text-body-md text-on-surface-variant mb-8 max-w-sm">Centralized hub for all brand assets. Manage, tag, and deploy across campaigns.</p>
                      <div className="grid grid-cols-3 gap-2 mb-6">
                        {["#004ccd", "#731be5", "#0f62fe", "#f58529", "#dd2a7b", "#34a853"].map((color, i) => (
                          <div key={i} className="aspect-video rounded-lg overflow-hidden relative group/thumb" style={{ animation: `fade-up 0.5s ease-out ${i * 0.08}s both` }}>
                            <div className="w-full h-full" style={{ background: `linear-gradient(135deg, ${color}, ${color}88)` }} />
                            <div className="absolute inset-0 bg-white/0 group-hover/thumb:bg-white/10 transition-colors duration-300" />
                            <span className="absolute bottom-1 right-1 material-symbols-outlined text-white/60 text-[10px]">check_circle</span>
                          </div>
                        ))}
                      </div>
                      <span className="inline-flex items-center gap-2 text-label-sm text-tertiary font-semibold cursor-default">
                        Browse Library
                        <span className="material-symbols-outlined text-[16px]">arrow_right_alt</span>
                      </span>
                    </div>
                  </div>
                </TiltCard>
              </div>

              {/* Feature 4: Analytics - Dark */}
              <div>
                <div className="feature-card bg-gradient-to-br from-enterprise-navy to-enterprise-navy/90 p-6 lg:p-8 rounded-3xl shadow-xl group relative overflow-hidden h-full">
                  <div className="absolute top-0 right-0 w-56 h-56 bg-primary/20 rounded-full blur-[80px] -mr-28 -mt-28 group-hover:bg-primary/30 transition-colors duration-700" />
                  <div className="absolute bottom-0 left-0 w-40 h-40 bg-secondary/15 rounded-full blur-[60px] -ml-20 -mb-20 group-hover:bg-secondary/25 transition-colors duration-700" />
                  <div className="relative z-10 flex items-center gap-8">
                    <div className="flex-1">
                      <div className="mb-5 w-12 h-12 bg-primary/20 flex items-center justify-center rounded-xl group-hover:scale-110 transition-transform duration-300">
                        <span className="material-symbols-outlined text-primary-fixed-dim text-[24px]">insights</span>
                      </div>
                      <h3 className="text-headline-sm text-white font-bold mb-2">Advanced Analytics</h3>
                      <p className="text-body-sm text-outline-variant mb-6">Real-time performance tracking with enterprise-grade reporting.</p>
                      <span className="inline-flex items-center gap-2 text-label-sm text-primary-fixed-dim font-semibold cursor-default">
                        Explore Analytics
                        <span className="material-symbols-outlined text-[16px]">arrow_right_alt</span>
                      </span>
                    </div>
                    <div className="hidden sm:flex flex-col gap-3 w-32">
                      {[100, 75, 85, 65].map((w, i) => (
                        <div key={i} className="h-2 bg-primary/20 rounded-full overflow-hidden">
                          <div className="h-full bg-gradient-to-r from-primary to-primary-container rounded-full" style={{ width: `${w}%`, animation: `fade-up 1s ease-out ${i * 0.2}s both` }} />
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </Stagger>
          </div>
        </section>

        {/* How it Works Section */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative overflow-hidden" id="how-it-works">
          <div className="absolute inset-0 -z-10">
            <div className="absolute top-0 left-1/4 w-[500px] h-[500px] bg-primary/5 rounded-full blur-[200px]" />
            <div className="absolute bottom-0 right-1/4 w-[400px] h-[400px] bg-secondary/5 rounded-full blur-[200px]" />
          </div>

          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-20 lg:mb-28">
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/15 rounded-full mb-6">
                  <span className="material-symbols-outlined text-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>route</span>
                  <span className="text-label-sm text-primary font-semibold">Simple Workflow</span>
                </div>
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">How It Works</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">Get started in minutes, not days. Our streamlined workflow makes it effortless.</p>
              </div>
            </Reveal>

            <div className="space-y-24 lg:space-y-32">
              {[
                { step: "01", title: "Connect Your Accounts", desc: "Link your social media and ad platforms in one click. We support Facebook, Instagram, and TikTok — the platforms that matter most.", icon: "link", color: "from-primary to-primary-container", features: ["One-click OAuth", "Facebook, Instagram, TikTok", "Secure encryption"] },
                { step: "02", title: "AI Analyzes & Plans", desc: "Our AI engine studies your audience demographics, competitor strategies, and trending content to craft the perfect content strategy tailored to your brand.", icon: "neurology", color: "from-secondary to-secondary-container", features: ["Audience insights", "Competitor analysis", "Trend detection"] },
                { step: "03", title: "Launch & Optimize", desc: "Deploy campaigns automatically across all channels. Real-time AI optimization adjusts bids, targeting, and creative to ensure peak performance 24/7.", icon: "rocket_launch", color: "from-tertiary to-tertiary-container", features: ["Auto-deployment", "Real-time optimization", "Performance alerts"] },
              ].map((item, i) => (
                <div key={i} className="relative">
                  <div className={`grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-16 items-center ${i % 2 === 1 ? "" : ""}`}>
                    {/* Visual Panel */}
                    <Reveal direction={i % 2 === 0 ? "left" : "right"} delay={100} className={i % 2 === 1 ? "lg:order-2" : ""}>
                      <div className="relative group">
                        <div className="absolute inset-0 bg-gradient-to-br from-primary/10 to-secondary/10 rounded-[2rem] blur-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-700" />
                        <div className="relative bg-gradient-to-br from-surface-container-low to-surface-container-lowest rounded-[2rem] border border-outline-variant/20 p-12 lg:p-16 shadow-xl overflow-hidden">
                          <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-primary/10 to-transparent rounded-full blur-3xl -mr-32 -mt-32" />
                          <div className="absolute bottom-0 left-0 w-48 h-48 bg-gradient-to-br from-secondary/10 to-transparent rounded-full blur-3xl -ml-24 -mb-24" />
                          
                          <div className="relative z-10 flex flex-col items-center justify-center min-h-[280px]">
                            <div className={`w-24 h-24 bg-gradient-to-br ${item.color} rounded-3xl flex items-center justify-center shadow-2xl mb-6 group-hover:scale-110 group-hover:rotate-6 transition-all duration-500`}>
                              <span className="material-symbols-outlined text-on-primary text-[48px]">{item.icon}</span>
                            </div>
                            
                            <div className="flex items-center gap-2 mb-4">
                              {[0, 1, 2].map((dot) => (
                                <div key={dot} className="w-2 h-2 rounded-full bg-primary/30" style={{ animation: `pulse-glow 2s ease-in-out ${dot * 0.3}s infinite` }} />
                              ))}
                            </div>
                            
                            <div className="text-display-lg font-bold text-gradient-clip">{item.step}</div>
                          </div>

                          {/* Decorative elements */}
                          <div className="absolute top-6 left-6 w-12 h-12 border-2 border-primary/20 rounded-xl" style={{ animation: "float 4s ease-in-out infinite" }} />
                          <div className="absolute bottom-6 right-6 w-8 h-8 border-2 border-secondary/20 rounded-full" style={{ animation: "float-reverse 3s ease-in-out infinite" }} />
                        </div>
                      </div>
                    </Reveal>

                    {/* Content Panel */}
                    <Reveal direction={i % 2 === 0 ? "right" : "left"} delay={200} className={i % 2 === 1 ? "lg:order-1" : ""}>
                      <div className="space-y-6">
                        <div className="inline-flex items-center gap-3">
                          <div className={`w-12 h-12 bg-gradient-to-br ${item.color} rounded-xl flex items-center justify-center shadow-lg`}>
                            <span className="text-on-primary text-headline-sm font-bold">{item.step}</span>
                          </div>
                          <div className="h-px w-12 bg-gradient-to-r from-outline-variant/50 to-transparent" />
                        </div>

                        <h3 className="text-headline-lg text-on-surface font-bold">{item.title}</h3>
                        
                        <p className="text-body-lg text-on-surface-variant leading-relaxed">{item.desc}</p>

                        <div className="space-y-3 pt-4">
                          {item.features.map((feature, fi) => (
                            <div key={fi} className="flex items-center gap-3 group/feature">
                              <div className="w-6 h-6 rounded-full bg-success-green/10 flex items-center justify-center flex-shrink-0 group-hover/feature:scale-110 transition-transform">
                                <span className="material-symbols-outlined text-success-green text-[14px]" style={{ fontVariationSettings: "'FILL' 1" }}>check</span>
                              </div>
                              <span className="text-body-md text-on-surface-variant group-hover/feature:text-on-surface transition-colors">{feature}</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    </Reveal>
                  </div>

                  {/* Connector Line */}
                  {i < 2 && (
                    <div className="hidden lg:flex absolute left-1/2 top-full w-px h-24 -translate-x-1/2 items-center justify-center">
                      <div className="w-px h-full bg-gradient-to-b from-primary/30 via-secondary/20 to-transparent relative">
                        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-3 h-3 rounded-full bg-gradient-to-br from-primary to-secondary shadow-lg" style={{ animation: "pulse-glow 2s ease-in-out infinite" }} />
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Stats Section */}
        <section className="py-20 lg:py-28 px-6 lg:px-8 relative" id="stats">
          <div className="absolute inset-0 -z-10">
            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[400px] bg-primary/5 rounded-full blur-[200px]" />
          </div>

          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16">
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Proven Results</h2>
                <p className="text-body-lg text-on-surface-variant">Numbers that speak for themselves</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {[
                { value: 50000, suffix: "+", label: "Ads Managed", desc: "Scale with confidence across regions", icon: "rocket_launch", color: "from-primary to-primary/70" },
                { value: 95, suffix: "%", label: "Time Saved", desc: "Automate tedious manual work", icon: "schedule", color: "from-secondary to-secondary/70" },
                { value: 3, suffix: "x", label: "Better ROI", desc: "Optimized spend for higher growth", icon: "trending_up", color: "from-tertiary to-tertiary/70" },
              ].map((stat, i) => (
                <TiltCard key={i}>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/15 shadow-sm text-center group">
                    <div className={`w-16 h-16 mx-auto mb-6 bg-gradient-to-br ${stat.color} rounded-2xl flex items-center justify-center shadow-lg group-hover:scale-110 group-hover:rotate-6 transition-all duration-500`}>
                      <span className="material-symbols-outlined text-on-primary text-[32px]">{stat.icon}</span>
                    </div>
                    <div className="text-display-lg text-on-surface font-bold mb-2">
                      <CountUp value={stat.value} suffix={stat.suffix} />
                    </div>
                    <p className="text-headline-sm text-on-surface font-semibold mb-2">{stat.label}</p>
                    <p className="text-body-sm text-on-surface-variant">{stat.desc}</p>
                  </div>
                </TiltCard>
              ))}
            </Stagger>
          </div>
        </section>

        {/* Testimonials Section */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 bg-gradient-to-b from-surface-container-low/20 to-background relative overflow-hidden" id="testimonials">
          <div className="absolute top-0 left-1/4 w-[400px] h-[400px] bg-secondary/5 rounded-full blur-[200px] -z-10" />

          <div className="max-w-7xl mx-auto">
            <Reveal>
              <div className="text-center mb-16 lg:mb-20">
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/15 rounded-full mb-6">
                  <span className="material-symbols-outlined text-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>format_quote</span>
                  <span className="text-label-sm text-primary font-semibold">Testimonials</span>
                </div>
                <h2 className="text-headline-lg text-on-surface font-bold mb-4">Loved by Marketers</h2>
                <p className="text-body-lg text-on-surface-variant max-w-2xl mx-auto">See what industry leaders say about transforming their workflow with AISAM.</p>
              </div>
            </Reveal>

            <Stagger className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {[
                { name: "Sarah Chen", role: "VP Marketing, TechFlow", text: "AISAM cut our campaign setup time by 80%. The AI suggestions are incredibly accurate and have boosted our ROAS significantly.", avatar: "SC" },
                { name: "Marcus Rivera", role: "Head of Growth, ScaleUp", text: "The analytics alone are worth it. We discovered insights that would have taken our team weeks to find manually. Game changer.", avatar: "MR" },
                { name: "Emily Zhang", role: "CMO, BrandForge", text: "We manage 50+ campaigns across 8 platforms. AISAM handles it all seamlessly. Our team can finally focus on strategy, not grunt work.", avatar: "EZ" },
              ].map((t, i) => (
                <div key={i}>
                  <div className="feature-card gradient-border bg-surface-container-lowest p-8 rounded-3xl border border-outline-variant/15 shadow-sm group h-full flex flex-col">
                    <div className="flex items-center gap-1 mb-4">
                      {[1, 2, 3, 4, 5].map((s) => (
                        <span key={s} className="material-symbols-outlined text-warning-amber text-[18px]" style={{ fontVariationSettings: "'FILL' 1" }}>star</span>
                      ))}
                    </div>
                    <p className="text-body-md text-on-surface-variant leading-relaxed mb-6 flex-1">&ldquo;{t.text}&rdquo;</p>
                    <div className="flex items-center gap-3 pt-4 border-t border-outline-variant/10">
                      <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary to-secondary flex items-center justify-center text-white text-label-sm font-bold">{t.avatar}</div>
                      <div>
                        <div className="text-body-sm text-on-surface font-semibold">{t.name}</div>
                        <div className="text-label-xs text-outline">{t.role}</div>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </Stagger>
          </div>
        </section>

        {/* CTA Section */}
        <section className="py-24 lg:py-32 px-6 lg:px-8 relative overflow-hidden">
          <div className="absolute inset-0 -z-10">
            <MorphBlob className="w-[500px] h-[500px] -right-20 -top-20 opacity-20" />
            <MorphBlob className="w-[400px] h-[400px] -left-20 -bottom-20 opacity-15" />
          </div>

          <div className="max-w-4xl mx-auto">
            <Reveal direction="scale">
              <div className="relative glass-card p-10 lg:p-16 rounded-[2rem] shadow-2xl text-center overflow-hidden">
                <div className="absolute top-0 left-0 w-full h-1.5 bg-gradient-to-r from-primary via-secondary to-tertiary animate-gradient" />
                <div className="absolute -top-24 -right-24 w-48 h-48 bg-primary/10 rounded-full blur-[80px]" />
                <div className="absolute -bottom-24 -left-24 w-48 h-48 bg-secondary/10 rounded-full blur-[80px]" />

                <div className="relative z-10">
                  <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/5 border border-primary/15 rounded-full mb-6">
                    <span className="material-symbols-outlined text-primary text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>celebration</span>
                    <span className="text-label-sm text-primary font-semibold">Limited Offer</span>
                  </div>

                  <h2 className="text-headline-lg text-on-surface font-bold mb-4">Ready to Transform Your Strategy?</h2>
                  <p className="text-body-lg text-on-surface-variant mb-10 max-w-2xl mx-auto">Join 10,000+ brands scaling their presence with the world&apos;s most intelligent ad manager.</p>

                  <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-8">
                    <Link href="/register" className="w-full sm:w-auto group relative inline-flex items-center justify-center gap-2 px-10 py-5 bg-primary text-on-primary rounded-2xl text-headline-sm font-bold shadow-xl shadow-primary/25 hover:scale-[1.03] hover:shadow-2xl transition-all active:scale-[0.98] overflow-hidden">
                      <span className="relative z-10">Get Started for Free</span>
                      <span className="material-symbols-outlined text-[20px] relative z-10 group-hover:translate-x-1 transition-transform">arrow_forward</span>
                      <div className="absolute inset-0 bg-gradient-to-r from-secondary to-primary-container opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
                    </Link>
                    <Link href="/login" className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-10 py-5 bg-surface-container-lowest/80 backdrop-blur text-on-surface rounded-2xl text-headline-sm font-semibold border border-outline-variant/20 hover:border-primary/30 hover:bg-surface-container transition-all active:scale-[0.98]">
                      Sign In to Account
                    </Link>
                  </div>

                  <div className="flex flex-wrap justify-center items-center gap-6 text-label-sm text-outline">
                    {[["No credit card required"], ["14-day free trial"], ["Cancel anytime"]].map(([text], i) => (
                      <div key={i} className="flex items-center gap-2">
                        <span className="material-symbols-outlined text-success-green text-[16px]" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
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
        <div className="absolute inset-0 opacity-[0.03]" style={{ backgroundImage: "radial-gradient(circle, #ffffff 1px, transparent 1px)", backgroundSize: "32px 32px" }} />

        <div className="relative z-10 max-w-7xl mx-auto px-6 lg:px-8 py-16">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12">
            <div className="lg:col-span-1">
              <Link href="/" className="flex items-center gap-3 mb-6 group">
                <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-on-primary text-[20px]" style={{ fontVariationSettings: "'FILL' 1" }}>psychology</span>
                </div>
                <span className="text-headline-sm font-bold">AISAM</span>
              </Link>
              <p className="text-body-sm text-outline-variant mb-6 leading-relaxed">The future of social media advertising, powered by enterprise-grade AI.</p>
              <div className="flex gap-3">
                {["language", "share", "mail"].map((icon) => (
                  <span key={icon} className="w-10 h-10 rounded-xl bg-white/5 flex items-center justify-center text-outline hover:text-white hover:bg-white/10 cursor-pointer transition-all hover:scale-110">
                    <span className="material-symbols-outlined text-[18px]">{icon}</span>
                  </span>
                ))}
              </div>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Platform</h4>
              <ul className="space-y-4">
                {["Features", "Pricing", "Case Studies", "Integrations"].map((item) => (
                  <li key={item}><span className="text-body-sm text-outline-variant cursor-default inline-block hover:pl-1 transition-all">{item}</span></li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Company</h4>
              <ul className="space-y-4">
                {["About Us", "Careers", "Terms of Service", "Privacy Policy", "Contact Support"].map((item) => (
                  <li key={item}><span className="text-body-sm text-outline-variant cursor-default inline-block hover:pl-1 transition-all">{item}</span></li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="text-label-sm font-bold text-white uppercase tracking-wider mb-6">Newsletter</h4>
              <p className="text-body-sm text-outline-variant mb-4">Get the latest ad trends delivered to your inbox.</p>
              <div className="flex gap-2">
                <input className="flex-1 bg-white/5 border border-white/10 text-white rounded-xl px-4 py-3 text-body-sm outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50 placeholder:text-outline-variant/50 transition-all" placeholder="Enter your email" type="email" />
                <button className="p-3 bg-primary text-on-primary rounded-xl hover:bg-primary-container hover:scale-105 active:scale-95 transition-all">
                  <span className="material-symbols-outlined text-[20px]">send</span>
                </button>
              </div>
            </div>
          </div>
        </div>

          <div className="relative z-10 border-t border-white/10">
            <div className="max-w-7xl mx-auto px-6 lg:px-8 py-6 flex flex-col md:flex-row justify-between items-center gap-4">
              <p className="text-label-sm text-outline-variant">&copy; 2026 AISAM Intelligence. All rights reserved.</p>
            <div className="flex gap-6">
              <span className="text-label-sm text-outline-variant cursor-default">Privacy</span>
              <span className="text-label-sm text-outline-variant cursor-default">Terms</span>
              <span className="text-label-sm text-outline-variant cursor-default">Cookies</span>
            </div>
          </div>
        </div>
      </footer>

      <BackToTop />
    </div>
  );
}
