"use client";

import Link from "next/link";
import { useEffect, useRef } from "react";

interface AuthShellProps {
  children: React.ReactNode;
  badge: {
    icon: string;
    text: string;
  };
  headline: string;
  headlineHighlight?: string;
  description: string;
  backgroundImage: string;
  stats?: Array<{
    label: string;
    value: string;
  }>;
  showSocialProof?: boolean;
}

export default function AuthShell({
  children,
  badge,
  headline,
  headlineHighlight,
  description,
  backgroundImage,
  stats,
  showSocialProof = false,
}: AuthShellProps) {
  const leftPanelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!leftPanelRef.current) return;
      const x = e.clientX / window.innerWidth;
      const y = e.clientY / window.innerHeight;
      const moveX = (x - 0.5) * 14;
      const moveY = (y - 0.5) * 14;
      leftPanelRef.current.style.backgroundPosition = `${50 + moveX}% ${50 + moveY}%`;
    };
    window.addEventListener("mousemove", handleMouseMove);
    return () => window.removeEventListener("mousemove", handleMouseMove);
  }, []);

  return (
    <main className="min-h-screen grid grid-cols-1 lg:grid-cols-2">
      {/* Left Column: Branding */}
      <section
        ref={leftPanelRef}
        className="hidden lg:flex flex-col justify-between p-margin-desktop bg-enterprise-navy relative overflow-hidden"
        style={{ backgroundSize: "120% 120%", transition: "background-position 0.1s ease" }}
      >
        <div className="absolute inset-0 z-0">
          <img
            className="w-full h-full object-cover opacity-40"
            alt="AI Visualization"
            src={backgroundImage}
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-enterprise-navy via-enterprise-navy/80 to-transparent" />
        </div>

        {/* Logo */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
            <span
              className="material-symbols-outlined text-on-primary text-[20px]"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              psychology
            </span>
          </div>
          <h1 className="font-headline-sm text-headline-sm font-bold text-surface-bright tracking-tight">
            AISAM
          </h1>
        </div>

        {/* Headline */}
        <div className="relative z-10 max-w-lg mb-24">
          <span className="inline-block px-3 py-1 rounded-full bg-secondary-container/20 border border-secondary-container/30 text-secondary-fixed-dim text-label-md mb-stack-md">
            {badge.text}
          </span>
          <h2 className="font-display-lg text-display-lg text-surface-bright mb-stack-md leading-tight">
            {headline}{" "}
            {headlineHighlight && (
              <span className="text-primary-fixed-dim">{headlineHighlight}</span>
            )}
          </h2>
          <p className="font-body-lg text-body-lg text-outline-variant leading-relaxed">
            {description}
          </p>
        </div>

        {/* Stats or Social Proof */}
        {showSocialProof ? (
          <div className="relative z-10 flex gap-gutter items-center border-t border-white/10 pt-stack-lg">
            <div className="flex -space-x-3">
              {[
                "https://lh3.googleusercontent.com/aida-public/AB6AXuDsjiSkj92yGwUuOpNfSbZVJnVo_5OxNAyl5Q733FQj-Ig1XkkMRuMBk8wJRY9LYX1F0SeXgzl6MFszqE0nje1lxb05caPk8o6NvBI1CXxb0s6McwHJSYZ-hO50ULQC0Pqscd-8Ws9dEDgmgX_Y5AvQ8QEvWWo43yxMRkOp3XtmCIToRGupYo9Jma1L2qIPyTXyHjGoapTgZigu78QZKDkgPP8SJuVXlsjisa3w2mUoNH_ZWXalcQReX4AAQjNjRMzj5szQlpSnSAk",
                "https://lh3.googleusercontent.com/aida-public/AB6AXuANo2kG_jhYcmYHQZqeImKsNNcpgYtGMduA_fjnoYLC9J64mjEM2gH8dvQ3In0HitMYyLJropVr40fZqEIFTQ6gHtWIsm1Xcz79dpKHnJFVHR3tBPYCUWFPE7iZjPiBKwQXW5nTKQMSILkkywbHfCT3hFxxdaePbZVudh9aWpDUmnQ3TpAsC2jgUbyxmy0KxCzjwwyNmREO3a0EZZR5sa95cL-zebJ6JMA3m6e5rCpWLSFObHjhcUEK2uoM2F4EdJHViX3zgyt1hLU",
                "https://lh3.googleusercontent.com/aida-public/AB6AXuCyqS1j6p5iR8y7iIJHfbCrQU2cdXPsCBnci0xoqm_3UU5DpSKewtU28CAYkZ_uK-R7OEyeb_5xmcDvOp9MRHRAg9_6p6r-X1GkcqlmZldaBxJzH7DlUUrWuCgyWeYQQQ3butWT38GIDA9WTW23UXx1XehQ563-D9bvzuAIBHhKt3yckiQLiJXcHwCVinFuUU-scEGrr-gpzjMZt2EWutjAtmgDIGJIfenVEos9_GCIZYiL9hVk_D3V4v34KXwdwzkFmFi9YpcbZv0",
              ].map((src, i) => (
                <img
                  key={i}
                  src={src}
                  alt={`User ${i + 1}`}
                  className="w-10 h-10 rounded-full border-2 border-enterprise-navy object-cover"
                />
              ))}
            </div>
            <p className="font-label-md text-label-md text-outline-variant">
              Trusted by 500+ global brands
            </p>
          </div>
        ) : stats ? (
          <div className="relative z-10 flex gap-gutter">
            {stats.map((stat, i) => (
              <div key={i} className="flex-1 p-stack-md rounded-xl bg-white/5 backdrop-blur-xl border border-white/10">
                <span className="text-label-md text-primary-fixed-dim block mb-1">{stat.label}</span>
                <span className="font-headline-md text-headline-md text-surface-bright">{stat.value}</span>
              </div>
            ))}
          </div>
        ) : null}
      </section>

      {/* Right Column: Form */}
      <section className="flex items-center justify-center p-margin-mobile md:p-margin-desktop bg-surface relative">
        {/* Mobile Logo */}
        <div className="absolute top-margin-mobile left-margin-mobile lg:hidden flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
            <span
              className="material-symbols-outlined text-on-primary text-[20px]"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              psychology
            </span>
          </div>
          <span className="font-headline-sm text-headline-sm font-bold text-on-surface tracking-tight">
            AISAM
          </span>
        </div>

        <div className="w-full max-w-md">
          {children}
        </div>

        {/* Footer */}
        <footer className="absolute bottom-6 left-0 w-full px-margin-desktop hidden lg:flex justify-between items-center text-outline/60">
          <p className="font-label-sm text-label-sm">
            © 2024 AISAM. All rights reserved.
          </p>
          <div className="flex gap-4">
            <Link
              href="#"
              className="font-label-sm text-label-sm hover:text-primary transition-colors"
            >
              Terms
            </Link>
            <Link
              href="#"
              className="font-label-sm text-label-sm hover:text-primary transition-colors"
            >
              Privacy
            </Link>
          </div>
        </footer>
      </section>
    </main>
  );
}
