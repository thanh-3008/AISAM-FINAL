"use client";

import Link from "next/link";
import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { setToken } from "@/lib/auth";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const leftPanelRef = useRef<HTMLDivElement>(null);
  const router = useRouter();

  // Atmospheric mouse parallax on left panel
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const result = await apiClient("/auth/login", {
        data: { email, password },
      });

      if (result.success && result.data?.accessToken) {
        setToken(result.data.accessToken);
        setIsSuccess(true);
        router.push("/dashboard");
      } else {
        setError("Đăng nhập thất bại, vui lòng thử lại.");
      }
    } catch (err: any) {
      setError(err.message || "Email hoặc mật khẩu không chính xác.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="min-h-screen grid grid-cols-1 lg:grid-cols-2">
      {/* ── Left Column: Branding ── */}
      <section
        ref={leftPanelRef}
        className="hidden lg:flex flex-col justify-between p-margin-desktop bg-enterprise-navy relative overflow-hidden"
        style={{ backgroundSize: "120% 120%", transition: "background-position 0.1s ease" }}
      >
        {/* Background Image + Overlay */}
        <div className="absolute inset-0 z-0">
          <img
            className="w-full h-full object-cover opacity-40"
            alt="Abstract neural network connections"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuB9aMAbRqJJ9ilmlGLShVEUiz0ld2SmU1DXq3l-WUQAFgZgmGlYOnJryDpkUAeYfYzripf7sZ4FUQtez1hlchpmrBr_E5LQ5NbmWm4Zlmo8avlH3KkpoHBoqaonTnK6pAhFFmZe9iq8t6MUub0elaFwMFm7zV1G7E7bamnPcgA4o-eAWWmsT1_enXw1GBfszBdgwaPeAZ2nVzN47TZ9nulCNEnZPeY4eHdLDdKHJ0nLnsibjNDYDE5ctee_DCUmd0QXJ8Qd2HxG-AA"
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-enterprise-navy via-enterprise-navy/80 to-transparent" />
        </div>

        {/* Logo */}
        <div className="relative z-10 flex items-center gap-2">
          <span
            className="material-symbols-outlined text-primary-fixed-dim text-headline-sm"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            auto_awesome
          </span>
          <h1 className="font-headline-sm text-headline-sm font-bold text-surface-bright tracking-tight">
            AISAM
          </h1>
        </div>

        {/* Headline */}
        <div className="relative z-10 max-w-lg mb-24">
          <span className="inline-block px-3 py-1 rounded-full bg-secondary-container/20 border border-secondary-container/30 text-secondary-fixed-dim font-label-md text-label-md mb-stack-md uppercase tracking-widest">
            Enterprise Precision
          </span>
          <h2 className="font-display-lg text-display-lg text-surface-bright mb-stack-md leading-tight">
            AI-Powered Precision in every campaign.
          </h2>
          <p className="font-body-lg text-body-lg text-outline-variant leading-relaxed">
            Harness the power of surgical-grade neural networks to optimize your
            social media advertising with unparalleled accuracy and real-time
            intelligence.
          </p>
        </div>

        {/* Social Proof */}
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
      </section>

      {/* ── Right Column: Login Form ── */}
      <section className="flex items-center justify-center p-margin-mobile md:p-margin-desktop bg-surface relative">
        {/* Mobile Logo */}
        <div className="absolute top-margin-mobile left-margin-mobile lg:hidden flex items-center gap-2">
          <span
            className="material-symbols-outlined text-primary text-headline-sm"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            auto_awesome
          </span>
          <span className="font-headline-sm text-headline-sm font-bold text-enterprise-navy tracking-tight">
            AISAM
          </span>
        </div>

        <div className="w-full max-w-md">
          {/* Heading */}
          <div className="mb-stack-lg text-center lg:text-left">
            <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
              Welcome back
            </h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Sign in to your AISAM command center.
            </p>
          </div>

          {/* Error State */}
          {error && (
            <div className="mb-stack-md p-stack-md bg-error-container/50 border border-error/20 rounded-lg flex items-start gap-3">
              <span
                className="material-symbols-outlined text-error shrink-0"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                error
              </span>
              <p className="font-body-sm text-body-sm text-on-error-container">
                {error}
              </p>
            </div>
          )}

          {/* Google Button */}
          <button
            type="button"
            className="w-full h-12 flex items-center justify-center gap-3 bg-surface-container-low border border-outline-variant rounded-lg hover:bg-surface-container-high transition-colors duration-200 active:scale-[0.98] mb-stack-lg"
          >
            <svg className="w-5 h-5" viewBox="0 0 24 24">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
            </svg>
            <span className="font-label-md text-label-md text-on-surface">
              Continue with Google
            </span>
          </button>

          {/* Divider */}
          <div className="flex items-center gap-4 mb-stack-lg">
            <div className="h-px bg-outline-variant flex-1" />
            <span className="font-label-sm text-label-sm text-outline uppercase tracking-widest">
              Or email
            </span>
            <div className="h-px bg-outline-variant flex-1" />
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-stack-md">
            {/* Email */}
            <div>
              <label
                htmlFor="email"
                className="block font-label-md text-label-md text-on-surface-variant mb-1"
              >
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@company.com"
                required
                className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
              />
            </div>

            {/* Password */}
            <div>
              <div className="flex justify-between items-center mb-1">
                <label
                  htmlFor="password"
                  className="block font-label-md text-label-md text-on-surface-variant"
                >
                  Password
                </label>
                <Link
                  href="/forgot-password"
                  className="font-label-md text-label-md text-primary hover:underline transition-all"
                >
                  Forgot password?
                </Link>
              </div>
              <div className="relative">
                <input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  required
                  className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
                >
                  <span className="material-symbols-outlined text-[20px]">
                    {showPassword ? "visibility_off" : "visibility"}
                  </span>
                </button>
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading || isSuccess}
              className={`w-full h-12 font-headline-sm text-headline-sm rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:cursor-not-allowed ${
                isSuccess
                  ? "bg-success-green text-white"
                  : "bg-primary-container text-on-primary-container"
              }`}
            >
              {isLoading ? (
                <>
                  <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
                  <span>Authenticating...</span>
                </>
              ) : isSuccess ? (
                <>
                  <span className="material-symbols-outlined text-[20px]">
                    check_circle
                  </span>
                  <span>Welcome back</span>
                </>
              ) : (
                <span>Sign In</span>
              )}
            </button>
          </form>

          {/* Sign Up Link */}
          <div className="mt-stack-lg text-center">
            <p className="font-body-sm text-body-sm text-on-surface-variant">
              Don&apos;t have an account?{" "}
              <Link
                href="/register"
                className="text-primary font-bold hover:underline"
              >
                Sign up
              </Link>
            </p>
          </div>

          {/* Security Note */}
          <div className="mt-12 flex items-center justify-center gap-2 text-outline">
            <span className="material-symbols-outlined text-[16px]">lock</span>
            <span className="font-label-sm text-label-sm">
              Secure, encrypted login
            </span>
          </div>
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
