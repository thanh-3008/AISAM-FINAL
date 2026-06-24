"use client";

import Link from "next/link";
import { useEffect, useRef } from "react";

export default function NotFound() {
  const glowsRef = useRef<NodeListOf<HTMLElement> | null>(null);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!glowsRef.current) return;
      const x = e.clientX / window.innerWidth;
      const y = e.clientY / window.innerHeight;

      glowsRef.current.forEach((glow, index) => {
        const speed = (index + 1) * 20;
        (glow as HTMLElement).style.transform = `translate(${x * speed}px, ${y * speed}px)`;
      });
    };

    glowsRef.current = document.querySelectorAll(
      "[data-glow]"
    ) as NodeListOf<HTMLElement>;
    document.addEventListener("mousemove", handleMouseMove);
    return () => document.removeEventListener("mousemove", handleMouseMove);
  }, []);

  return (
    <>
      <style>{`
        .ai-glow-card {
          box-shadow: 0 0 40px -10px rgba(141, 66, 255, 0.3);
        }
        .ai-gradient-text {
          background: linear-gradient(135deg, #0f62fe 0%, #8d42ff 100%);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
        }
        .floating-anim {
          animation: floating 6s ease-in-out infinite;
        }
        @keyframes floating {
          0%   { transform: translateY(0px); }
          50%  { transform: translateY(-20px); }
          100% { transform: translateY(0px); }
        }
        .spin-slow {
          animation: spin 30s linear infinite;
        }
        .spin-reverse {
          animation: spin-reverse 20s linear infinite;
        }
        @keyframes spin {
          from { transform: rotate(0deg) scale(1.5); }
          to   { transform: rotate(360deg) scale(1.5); }
        }
        @keyframes spin-reverse {
          from { transform: rotate(360deg) scale(1.1); }
          to   { transform: rotate(0deg) scale(1.1); }
        }
      `}</style>

      {/* Top Nav */}
      <header className="fixed top-0 w-full z-50 bg-white/80 backdrop-blur-md border-b border-outline-variant/30 shadow-sm">
        <nav className="flex justify-between items-center px-8 max-w-7xl mx-auto h-16">
          <div className="flex items-center gap-2">
            <span className="text-headline-sm font-bold text-enterprise-navy tracking-tight">
              AISAM
            </span>
          </div>
          <div className="hidden md:flex items-center gap-8">
            {["Features", "Pricing", "Blog"].map((item) => (
              <a
                key={item}
                href="#"
                className="text-label-md text-on-surface-variant hover:text-primary transition-colors duration-200"
              >
                {item}
              </a>
            ))}
          </div>
          <div className="flex items-center gap-4">
            <Link
              href="/login"
              className="text-label-md px-4 py-2 text-primary hover:text-primary/80 transition-colors active:scale-95"
            >
              Log In
            </Link>
            <Link
              href="/register"
              className="text-label-md px-5 py-2.5 bg-primary-container text-on-primary-container rounded-lg font-semibold hover:opacity-90 active:scale-95 transition-all"
            >
              Sign Up
            </Link>
          </div>
        </nav>
      </header>

      {/* Main */}
      <main className="min-h-screen flex items-center justify-center pt-24 pb-16 px-4 md:px-8 relative overflow-hidden bg-background">
        {/* Ambient Background Glows */}
        <div
          data-glow
          className="absolute top-1/4 left-1/4 w-96 h-96 bg-primary/5 rounded-full blur-[100px] pointer-events-none transition-transform duration-75"
        />
        <div
          data-glow
          className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-secondary/5 rounded-full blur-[100px] pointer-events-none transition-transform duration-75"
        />

        <div className="max-w-5xl w-full grid grid-cols-1 md:grid-cols-2 gap-6 items-center z-10">
          {/* Content Section */}
          <div className="text-center md:text-left space-y-6 order-2 md:order-1">
            {/* Badge */}
            <div className="inline-flex items-center gap-2 px-3 py-1 bg-surface-container-high rounded-full border border-outline-variant/30">
              <span
                className="material-symbols-outlined text-[18px] text-primary"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                error
              </span>
              <span className="text-label-sm text-on-surface-variant tracking-wider uppercase">
                System Error 404
              </span>
            </div>

            {/* Headline */}
            <h1 className="text-display-lg text-enterprise-navy leading-tight">
              Intelligence{" "}
              <span className="ai-gradient-text">Disconnected</span>.
            </h1>

            {/* Description */}
            <p className="text-body-lg text-on-surface-variant max-w-lg mx-auto md:mx-0">
              The node you&apos;re attempting to access does not exist in our
              neural network. It might have been moved, deleted, or never existed
              in this dimension.
            </p>

            {/* CTA Buttons */}
            <div className="flex flex-col sm:flex-row gap-4 pt-4 justify-center md:justify-start">
              <Link
                href="/dashboard"
                className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-primary text-white rounded-xl font-semibold shadow-lg shadow-primary/20 hover:bg-primary/90 transition-all active:scale-[0.98]"
              >
                <span className="material-symbols-outlined text-[20px]">
                  home
                </span>
                Return to Home
              </Link>
              <button className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-white border border-outline-variant text-enterprise-navy rounded-xl font-semibold hover:bg-surface-gray transition-all active:scale-[0.98]">
                Contact Support
              </button>
            </div>

            {/* Social Proof */}
            <div className="pt-8 flex items-center justify-center md:justify-start gap-6 border-t border-outline-variant/20">
              <div className="flex -space-x-3">
                {[
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuB4n_jv967XdaOfvpmDbPNwkGEWPGpbxri41X_ZRGMiZmtaFYIk1oqUF9pAchT2Rfax6w0iMN9Kyy1wShrwvDDH72CwFGdiiVc2jWjj0eIQB4JFIh1T9cMB8-uzgRNsx1xpgVfDUrLLXnPhAmY0PSL6ZTHDwGuPZUvt2aBSg-Tt7UR-cfNVKPY57sXIT8kC1JOGV6JNI8k97kz-EDAuG1gUxjKGWEXeZDeY1bLM0gLO7qjFjryKFJKWIgIolkSQHNSLiH-DR4Ac5kU",
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuCjAtdClqH3Q36zwQZvjZDrKKWbECpUoaZ5WbGYTKnbttSu3uFL5BHtiAxk_TrNyZp5wu1ADU0H6S-xQ_YvbnAJZ2XyvowS7ZaRgc5DrlJP0Vg_Iyk3VFI-DglZkclVmHzJjEmycXPb02opW0U5BNMay2mcy5vKMGCfwoiDpzuWGOzd5oY6zae69cIbCq9K2UHCGDn-DhcHex5aREHgjZRdppwBXhthL9KSw01zKQR3xdJx3afV7_Buuo6agpabXYYH2ctHCXbJDE8",
                  "https://lh3.googleusercontent.com/aida-public/AB6AXuCEC3fh7cpZ8qVoHMTjuC6OjRpBTRqNLNDglTNaSNzHeq-WJEwyaMPGv1xyrWBSKOqstnqp8zLdUF5tk0YhLvqGX9m4Pn9aliB-jO92rVvNsJqeR31PeKmy7NpjGJwOrCLLQwrkBPLUHoE54OphTVBj6beSpebiXSFqB8yjHoLBHYRX7u8OjHMRseFQia3xXoIiGM584tMfUhIrUoWKpUPzwPaMOUYD-zjUBO4mhahb9c-jEr4N7QVlz73kh14y6kmNA6_BA1aIFC8",
                ].map((src, i) => (
                  <div
                    key={i}
                    className="w-8 h-8 rounded-full border-2 border-white bg-surface-container overflow-hidden"
                  >
                    <img
                      src={src}
                      alt="User"
                      className="w-full h-full object-cover"
                    />
                  </div>
                ))}
              </div>
              <p className="text-label-sm text-outline">
                Join 2,400+ advertisers today.
              </p>
            </div>
          </div>

          {/* Visual Section */}
          <div className="order-1 md:order-2 flex justify-center items-center relative py-12">
            <div className="relative w-full aspect-square max-w-[400px]">
              {/* Large 404 watermark */}
              <div className="absolute inset-0 flex items-center justify-center">
                <span className="text-[160px] md:text-[200px] font-black opacity-5 select-none text-enterprise-navy leading-none">
                  404
                </span>
              </div>

              {/* Floating AI Brain Graphic */}
              <div className="relative z-10 w-full h-full flex items-center justify-center floating-anim">
                <div className="w-64 h-64 rounded-[32%] bg-gradient-to-br from-primary to-secondary p-1 ai-glow-card">
                  <div className="w-full h-full bg-white rounded-[31%] flex items-center justify-center overflow-hidden relative">
                    {/* Spinning rings */}
                    <div className="absolute inset-0 opacity-10">
                      <div className="absolute inset-0 border border-dashed border-enterprise-navy rounded-full spin-slow" />
                      <div className="absolute inset-0 border border-dashed border-enterprise-navy rounded-full spin-reverse" />
                    </div>
                    <span
                      className="material-symbols-outlined text-[80px] ai-gradient-text"
                      style={{
                        fontVariationSettings: "'FILL' 0, 'wght' 200",
                        fontSize: "80px",
                      }}
                    >
                      psychology_alt
                    </span>
                  </div>
                </div>

                {/* Floating data nodes */}
                <div
                  className="absolute top-10 right-10 w-12 h-12 bg-white rounded-xl shadow-md flex items-center justify-center border border-outline-variant/30 floating-anim"
                  style={{ animationDelay: "1s" }}
                >
                  <span className="material-symbols-outlined text-primary text-[20px]">
                    trending_up
                  </span>
                </div>
                <div
                  className="absolute bottom-16 left-0 w-10 h-10 bg-white rounded-lg shadow-md flex items-center justify-center border border-outline-variant/30 floating-anim"
                  style={{ animationDelay: "2.5s" }}
                >
                  <span className="material-symbols-outlined text-secondary text-[18px]">
                    bar_chart
                  </span>
                </div>
                <div
                  className="absolute top-1/2 -left-8 w-14 h-14 bg-white rounded-2xl shadow-xl flex items-center justify-center border border-outline-variant/30 floating-anim"
                  style={{ animationDelay: "0.5s" }}
                >
                  <span className="material-symbols-outlined text-danger-red text-[24px]">
                    link_off
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="w-full bg-enterprise-navy border-t border-outline/20">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 px-8 py-6 max-w-7xl mx-auto">
          <div className="space-y-4">
            <span className="text-headline-sm font-bold text-surface-bright">
              AISAM
            </span>
            <p className="text-body-sm text-outline-variant max-w-xs">
              Empowering advertisers with surgical AI precision for social media
              growth.
            </p>
          </div>
          <div className="flex flex-col gap-3">
            <h4 className="text-label-md text-surface-bright tracking-widest">
              Platform
            </h4>
            {["Features", "Pricing", "Case Studies"].map((item) => (
              <a
                key={item}
                href="#"
                className="text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors cursor-pointer"
              >
                {item}
              </a>
            ))}
          </div>
          <div className="flex flex-col gap-3">
            <h4 className="text-label-md text-surface-bright tracking-widest">
              Support
            </h4>
            {["Contact Support", "Terms of Service", "Privacy Policy"].map(
              (item) => (
                <a
                  key={item}
                  href="#"
                  className="text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors cursor-pointer"
                >
                  {item}
                </a>
              )
            )}
          </div>
          <div className="flex flex-col gap-3">
            <h4 className="text-label-md text-surface-bright tracking-widest">
              Newsletter
            </h4>
            <input
              type="email"
              placeholder="Email Address"
              className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2 text-surface-bright text-body-sm focus:ring-1 focus:ring-primary outline-none transition-all"
            />
            <p className="text-body-sm text-outline-variant">
              © 2024 AISAM. All rights reserved.
            </p>
          </div>
        </div>
      </footer>
    </>
  );
}
