"use client";

import { useEffect } from "react";
import Link from "next/link";

export default function LandingPage() {
  useEffect(() => {
    // Smooth scroll offset for header
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
      anchor.addEventListener("click", function (this: HTMLAnchorElement, e) {
        e.preventDefault();
        const targetId = this.getAttribute("href");
        if (!targetId || targetId === "#") return;
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
          const headerOffset = 64;
          const elementPosition = targetElement.getBoundingClientRect().top;
          const offsetPosition =
            elementPosition + window.scrollY - headerOffset;

          window.scrollTo({
            top: offsetPosition,
            behavior: "smooth",
          });
        }
      });
    });

    // Reveal animations on scroll
    const observerOptions = {
      threshold: 0.1,
    };

    const observer = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add("opacity-100", "translate-y-0");
          entry.target.classList.remove("opacity-0", "translate-y-10");
        }
      });
    }, observerOptions);

    document.querySelectorAll("section").forEach((section) => {
      section.classList.add(
        "transition-all",
        "duration-700",
        "opacity-0",
        "translate-y-10"
      );
      observer.observe(section);
    });
    
    return () => {
      observer.disconnect();
    };
  }, []);

  return (
    <div className="bg-background text-on-surface font-body-md">
      {/* TopNavBar */}
      <nav className="fixed top-0 w-full z-50 bg-surface/80 dark:bg-enterprise-navy/80 backdrop-blur-md border-b border-outline-variant/30 dark:border-outline/20 shadow-sm dark:shadow-none h-16">
        <div className="flex justify-between items-center px-margin-desktop max-w-7xl mx-auto h-full">
          <div className="font-headline-sm text-headline-sm font-bold text-enterprise-navy dark:text-surface-bright tracking-tight">
            AISAM
          </div>
          <div className="hidden md:flex items-center gap-gutter">
            <Link
              className="font-label-md text-label-md text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed-dim transition-colors duration-200"
              href="#features"
            >
              Features
            </Link>
            <Link
              className="font-label-md text-label-md text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed-dim transition-colors duration-200"
              href="#pricing"
            >
              Pricing
            </Link>
            <Link
              className="font-label-md text-label-md text-on-surface-variant dark:text-outline-variant hover:text-primary dark:hover:text-primary-fixed-dim transition-colors duration-200"
              href="#blog"
            >
              Blog
            </Link>
          </div>
          <div className="flex items-center gap-stack-md">
            <Link
              className="font-label-md text-label-md text-primary dark:text-inverse-primary hover:text-primary dark:hover:text-primary-fixed-dim transition-colors duration-200 active:scale-95"
              href="/login"
            >
              Log In
            </Link>
            <Link
              className="bg-primary-container text-on-primary-container px-4 py-2 rounded-lg font-label-md text-label-md hover:bg-primary transition-colors duration-200 active:scale-95"
              href="/register"
            >
              Sign Up
            </Link>
          </div>
        </div>
      </nav>

      <main className="pt-16">
        {/* Hero Section */}
        <section className="relative overflow-hidden pt-24 pb-32 px-margin-mobile md:px-margin-desktop bg-surface-bright opacity-100 translate-y-0">
          <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-gutter items-center">
            <div className="relative z-10">
              <span className="inline-block py-1 px-3 bg-secondary-container/10 text-secondary-container rounded-full font-label-md text-label-md mb-stack-md border border-secondary-container/20">
                AI-POWERED PRECISION
              </span>
              <h1 className="font-display-lg text-display-lg md:text-[48px] text-on-surface mb-stack-lg leading-tight">
                Master Social Media Ads with{" "}
                <span className="text-primary">AI-Powered</span> Precision
              </h1>
              <p className="font-body-lg text-body-lg text-on-surface-variant mb-12 max-w-xl">
                Automate content creation, optimize ad spend, and scale your brand
                across all social platforms with AISAM.
              </p>
              <div className="flex flex-wrap gap-stack-md">
                <Link
                  className="bg-primary-container text-on-primary-container px-8 py-4 rounded-lg font-headline-sm text-headline-sm flex items-center gap-2 hover:bg-primary transition-all duration-300 shadow-lg shadow-primary/20 active:scale-95"
                  href="/register"
                >
                  Start Free Trial
                  <span className="material-symbols-outlined">
                    arrow_forward
                  </span>
                </Link>
                <Link
                  className="bg-surface-container-high text-on-surface px-8 py-4 rounded-lg font-headline-sm text-headline-sm border border-outline-variant/30 hover:bg-surface-container-highest transition-all duration-300 active:scale-95"
                  href="#demo"
                >
                  Book a Demo
                </Link>
              </div>
            </div>
            <div className="relative lg:block mt-8 lg:mt-0">
              <div className="relative rounded-2xl overflow-hidden shadow-2xl border border-outline-variant/20 ai-glow-border">
                <img
                  alt="AISAM Dashboard Preview"
                  className="w-full h-full object-cover aspect-[4/3]"
                  src="https://lh3.googleusercontent.com/aida-public/AB6AXuBsAfZbKZ3IOo1LjRLd0u1lBBYKzL_wvAzTf5wrxIFXfZF3d0uyJoK9F9JcLjRA53GNwAvVAqalluJWOMZOGHIL5ZH1rYX8_oCrru380oL7v7XQ-J1fZmtOtdx6fika1eGrJJMwgzPnktI8lA4ftCTSenjNsup_Z34n-mmBG790ybRc24vmGKxyiFXysrO6Y_9RFxBjWyEBdNYwrrZFMXsfPX9RsMOXc7bgR4l_YxqLwYxahJEDGQBi34vQ5pZqGQPEZ8GGB4vs5Ec"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent"></div>
              </div>
              {/* Floating Stats Badge */}
              <div className="absolute -bottom-6 -left-6 bg-white p-6 rounded-xl shadow-xl border border-outline-variant/10 flex items-center gap-4 animate-bounce-slow">
                <div className="w-12 h-12 rounded-full bg-success-green/10 flex items-center justify-center">
                  <span className="material-symbols-outlined text-success-green">
                    trending_up
                  </span>
                </div>
                <div>
                  <div className="font-headline-sm text-headline-sm text-on-surface">
                    +142%
                  </div>
                  <div className="font-label-sm text-label-sm text-on-surface-variant">
                    Average Ad ROI
                  </div>
                </div>
              </div>
            </div>
          </div>
          {/* Decorative AI Glow */}
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full h-full -z-10 opacity-20 pointer-events-none">
            <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-primary rounded-full blur-[120px]"></div>
            <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-secondary rounded-full blur-[120px]"></div>
          </div>
        </section>

        {/* Features Section (Bento Grid) */}
        <section
          className="py-32 px-margin-mobile md:px-margin-desktop bg-surface-gray/50 opacity-100 translate-y-0"
          id="features"
        >
          <div className="max-w-7xl mx-auto">
            <div className="text-center mb-24">
              <h2 className="font-headline-lg text-headline-lg text-on-surface mb-stack-md">
                Unleash the Power of Intelligent Ads
              </h2>
              <p className="font-body-lg text-body-lg text-on-surface-variant max-w-2xl mx-auto">
                Our specialized AI agents work around the clock to ensure your
                social media strategy is always optimized for peak performance.
              </p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-6 gap-gutter">
              {/* Feature 1: AI Studio */}
              <div className="md:col-span-3 bg-white p-10 rounded-2xl border border-outline-variant/30 shadow-sm hover:shadow-md transition-all group overflow-hidden relative">
                <div className="mb-8 w-14 h-14 bg-primary-container/10 flex items-center justify-center rounded-xl group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-primary text-3xl">
                    spark
                  </span>
                </div>
                <h3 className="font-headline-md text-headline-md text-on-surface mb-stack-md">
                  AI Studio
                </h3>
                <p className="font-body-md text-body-md text-on-surface-variant mb-8 max-w-sm">
                  Generate high-converting text and photorealistic images in
                  seconds. Tailored to your brand&apos;s unique voice and aesthetic.
                </p>
                <div className="relative rounded-lg overflow-hidden h-48 border border-outline-variant/20">
                  <img
                    alt="Creative Generation"
                    className="w-full h-full object-cover"
                    src="https://lh3.googleusercontent.com/aida-public/AB6AXuBTgt3rCAc24KNrdsrNae8VEPCAqWpfmt-IspWuU_kH9DoH1zEwhoLqUxnEeAxzXvRLnA6dEmfub8-xJay6Hui_4V-sdwlishZRJ7vb67lCXuUOT9Zm3iSAUhnMGUyQTXHpxei4yPwt3mEyAKj1vpdf0_DWv0UzTsnRFLTL4Bplych6hQuWcUFIxxbufLCvlst1M4KFYU4YUMRmvkVhEYZ7Qem3S39dCTo2tINq6PCVUx98bdtxWb9iq85uhiAc2z3JxQbIPRyRTyA"
                  />
                </div>
              </div>

              {/* Feature 2: Smart Campaigns */}
              <div className="md:col-span-3 bg-white p-10 rounded-2xl border border-outline-variant/30 shadow-sm hover:shadow-md transition-all group overflow-hidden relative">
                <div className="mb-8 w-14 h-14 bg-secondary-container/10 flex items-center justify-center rounded-xl group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-secondary text-3xl">
                    speaker_phone
                  </span>
                </div>
                <h3 className="font-headline-md text-headline-md text-on-surface mb-stack-md">
                  Smart Campaigns
                </h3>
                <p className="font-body-md text-body-md text-on-surface-variant mb-8 max-w-sm">
                  Automated Facebook &amp; Instagram ad management. Our algorithms
                  adjust bids and targeting in real-time for maximum efficiency.
                </p>
                <div className="relative rounded-lg overflow-hidden h-48 border border-outline-variant/20">
                  <img
                    alt="Campaign Management"
                    className="w-full h-full object-cover"
                    src="https://lh3.googleusercontent.com/aida-public/AB6AXuCOzlVuqVmDNE2z_zBTMW5P4zZfhMQQBkgecnfo87Ldj2XKgUj-8zAje0poJKofypLe57wgfUz_Knv6PW6mpwz2c2fKe26DXSm1N2NJwHY0Mgnxx5cPstuRzEr5ebqnfsGb1b9B52099F3oGoLpf66PbPVgkHhnWYfkGQXVnmU_ws8_gdO2tJrQ4cA2UnUnba2kp8dbErUvlDaLTlTraVs4ADV-vX1xzspCXKAfC39X88CTINMPd2KCZAds50__jUApAauBhOi22rw"
                  />
                </div>
              </div>

              {/* Feature 3: Content Library */}
              <div className="md:col-span-2 bg-white p-8 rounded-2xl border border-outline-variant/30 shadow-sm hover:shadow-md transition-all group">
                <div className="mb-6 w-12 h-12 bg-surface-container-highest flex items-center justify-center rounded-lg group-hover:rotate-6 transition-transform">
                  <span className="material-symbols-outlined text-on-surface-variant text-2xl">
                    description
                  </span>
                </div>
                <h3 className="font-headline-sm text-headline-sm text-on-surface mb-stack-sm">
                  Content Library
                </h3>
                <p className="font-body-sm text-body-sm text-on-surface-variant">
                  Centralized hub for all your brand assets. Manage, tag, and deploy
                  assets across campaigns with ease.
                </p>
              </div>

              {/* Feature 4: Advanced Analytics */}
              <div className="md:col-span-4 bg-enterprise-navy text-surface-bright p-8 rounded-2xl border border-outline/20 shadow-xl hover:shadow-2xl transition-all group flex items-center gap-gutter">
                <div className="flex-1">
                  <div className="mb-6 w-12 h-12 bg-primary/20 flex items-center justify-center rounded-lg">
                    <span className="material-symbols-outlined text-primary-fixed-dim text-2xl">
                      bar_chart
                    </span>
                  </div>
                  <h3 className="font-headline-sm text-headline-sm text-surface-bright mb-stack-sm">
                    Advanced Analytics
                  </h3>
                  <p className="font-body-sm text-body-sm text-outline-variant mb-6">
                    Real-time performance tracking and enterprise-grade reporting.
                    Deep dive into every impression and conversion.
                  </p>
                  <Link
                    className="text-primary-fixed-dim font-label-md text-label-md flex items-center gap-1 hover:gap-2 transition-all"
                    href="#"
                  >
                    Explore Analytics
                    <span className="material-symbols-outlined text-[16px]">
                      arrow_right_alt
                    </span>
                  </Link>
                </div>
                <div className="hidden sm:block w-1/3 opacity-50 group-hover:opacity-100 transition-opacity">
                  {/* Simple Graphic Representation */}
                  <div className="space-y-2">
                    <div className="h-2 bg-primary rounded-full w-full"></div>
                    <div className="h-2 bg-primary/40 rounded-full w-2/3"></div>
                    <div className="h-2 bg-primary/60 rounded-full w-4/5"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Social Proof/Stats Section */}
        <section className="py-24 bg-white border-y border-outline-variant/20 opacity-100 translate-y-0">
          <div className="max-w-7xl mx-auto px-margin-desktop">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter text-center divide-y md:divide-y-0 md:divide-x divide-outline-variant/30">
              <div className="py-8 md:py-0 px-8">
                <div className="font-display-lg text-display-lg text-primary mb-2">
                  50k+
                </div>
                <p className="font-headline-sm text-headline-sm text-on-surface">
                  Ads Managed
                </p>
                <p className="font-body-sm text-body-sm text-on-surface-variant mt-2">
                  Scale with confidence across regions
                </p>
              </div>
              <div className="py-8 md:py-0 px-8">
                <div className="font-display-lg text-display-lg text-primary mb-2">
                  95%
                </div>
                <p className="font-headline-sm text-headline-sm text-on-surface">
                  Time Saved
                </p>
                <p className="font-body-sm text-body-sm text-on-surface-variant mt-2">
                  Automate the tedious manual work
                </p>
              </div>
              <div className="py-8 md:py-0 px-8">
                <div className="font-display-lg text-display-lg text-primary mb-2">
                  3x
                </div>
                <p className="font-headline-sm text-headline-sm text-on-surface">
                  Better ROI
                </p>
                <p className="font-body-sm text-body-sm text-on-surface-variant mt-2">
                  Optimized spend for higher growth
                </p>
              </div>
            </div>
          </div>
        </section>

        {/* Conversion Section */}
        <section className="py-32 px-margin-mobile md:px-margin-desktop bg-surface-bright relative overflow-hidden opacity-100 translate-y-0">
          {/* Background Decoration */}
          <div className="absolute inset-0 z-0 overflow-hidden pointer-events-none">
            <div className="absolute -right-20 -top-20 w-80 h-80 bg-primary/10 rounded-full blur-[100px]"></div>
            <div className="absolute -left-20 -bottom-20 w-80 h-80 bg-secondary/10 rounded-full blur-[100px]"></div>
          </div>
          <div className="max-w-4xl mx-auto text-center relative z-10 glass-card p-16 rounded-3xl border border-outline-variant/20 shadow-2xl">
            <h2 className="font-headline-lg text-headline-lg text-on-surface mb-stack-lg">
              Ready to transform your social media strategy?
            </h2>
            <p className="font-body-lg text-body-lg text-on-surface-variant mb-12">
              Join 10,000+ brands scaling their presence with the world&apos;s most
              intelligent ad manager.
            </p>
            <div className="flex flex-col sm:flex-row items-center justify-center gap-stack-md">
              <Link
                className="w-full sm:w-auto bg-primary-container text-on-primary-container px-10 py-5 rounded-xl font-headline-sm text-headline-sm hover:bg-primary transition-all duration-300 shadow-xl shadow-primary/30 active:scale-95"
                href="/register"
              >
                Get Started for Free
              </Link>
              <Link
                className="w-full sm:w-auto bg-white text-on-surface px-10 py-5 rounded-xl font-headline-sm text-headline-sm border border-outline-variant/40 hover:bg-surface-container-low transition-all duration-300 active:scale-95"
                href="/login"
              >
                Sign In to Your Account
              </Link>
            </div>
            <p className="mt-8 font-label-sm text-label-sm text-on-surface-variant">
              No credit card required. 14-day free trial.
            </p>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="w-full bg-enterprise-navy dark:bg-black border-t border-outline/20">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-gutter px-margin-desktop py-stack-lg max-w-7xl mx-auto">
          <div className="col-span-1">
            <div className="font-headline-sm text-headline-sm font-bold text-surface-bright mb-stack-md">
              AISAM
            </div>
            <p className="font-body-sm text-body-sm text-outline-variant">
              The future of social media advertising, powered by enterprise-grade
              artificial intelligence.
            </p>
          </div>
          <div className="space-y-4">
            <div className="text-surface-bright font-bold text-label-md">
              Platform
            </div>
            <ul className="space-y-2">
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Features
                </Link>
              </li>
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Pricing
                </Link>
              </li>
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Case Studies
                </Link>
              </li>
            </ul>
          </div>
          <div className="space-y-4">
            <div className="text-surface-bright font-bold text-label-md">
              Company
            </div>
            <ul className="space-y-2">
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Terms of Service
                </Link>
              </li>
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Privacy Policy
                </Link>
              </li>
              <li>
                <Link
                  className="font-body-sm text-body-sm text-outline-variant hover:text-primary-fixed-dim transition-colors"
                  href="#"
                >
                  Contact Support
                </Link>
              </li>
            </ul>
          </div>
          <div className="space-y-4">
            <div className="text-surface-bright font-bold text-label-md">
              Newsletter
            </div>
            <p className="font-body-sm text-body-sm text-outline-variant mb-4">
              Get the latest ad trends delivered to your inbox.
            </p>
            <div className="flex gap-2">
              <input
                className="bg-white/10 border-white/20 text-surface-bright rounded px-3 py-2 text-sm w-full focus:ring-primary focus:border-primary"
                placeholder="Email"
                type="email"
              />
              <button className="bg-primary text-white p-2 rounded hover:bg-primary-container transition-colors flex items-center justify-center">
                <span className="material-symbols-outlined text-[18px]">
                  send
                </span>
              </button>
            </div>
          </div>
        </div>
        <div className="border-t border-outline/10 py-8 px-margin-desktop max-w-7xl mx-auto flex justify-between items-center">
          <div className="font-body-sm text-body-sm text-outline-variant">
            © 2024 AISAM. All rights reserved.
          </div>
          <div className="flex gap-gutter">
            <span className="material-symbols-outlined text-outline hover:text-surface-bright cursor-pointer transition-colors">
              language
            </span>
            <span className="material-symbols-outlined text-outline hover:text-surface-bright cursor-pointer transition-colors">
              share
            </span>
          </div>
        </div>
      </footer>
    </div>
  );
}
