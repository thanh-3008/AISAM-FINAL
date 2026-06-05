"use client";

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { setToken, setRefreshToken, setStoredUser } from "@/lib/auth";
import { invalidateProfileCache } from "@/hooks/useProfiles";

export default function RegisterPage() {
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ full_name: "", email: "", password: "", confirm_password: "" });
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const router = useRouter();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    
    try {
      if (form.password !== form.confirm_password) {
        setError("Mật khẩu xác nhận không khớp.");
        setIsLoading(false);
        return;
      }
      const result = await apiClient("/auth/register", {
        data: {
          fullName: form.full_name,
          email: form.email,
          password: form.password,
          confirmPassword: form.confirm_password,
        },
      });

      if (result.success) {
        invalidateProfileCache();
        if (result.data?.accessToken) {
          setToken(result.data.accessToken);
        }
        if (result.data?.refreshToken) {
          setRefreshToken(result.data.refreshToken);
        }
        if (result.data?.user) {
          setStoredUser(result.data.user);
        }
        router.push("/overview");
      } else {
        setError("Đăng ký thất bại, vui lòng thử lại.");
      }
    } catch (err: any) {
      setError(err.message || "Có lỗi xảy ra trong quá trình đăng ký.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="w-full min-h-screen flex flex-col md:flex-row overflow-hidden">
      {/* ── Left Panel: Branding ── */}
      <section className="hidden md:flex md:w-1/2 lg:w-3/5 bg-enterprise-navy relative flex-col justify-between p-margin-desktop overflow-hidden">
        {/* Background */}
        <div className="absolute inset-0 z-0">
          <img
            alt="Enterprise Precision AI Visualization"
            className="w-full h-full object-cover opacity-40 mix-blend-screen"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
          />
          <div className="absolute inset-0 bg-gradient-to-tr from-enterprise-navy via-transparent to-primary/10" />
        </div>

        {/* Logo */}
        <div className="relative z-10 flex items-center gap-3">
          <div className="w-10 h-10 bg-primary-container rounded-lg flex items-center justify-center shadow-lg" style={{ boxShadow: "0 0 40px -10px rgba(15,98,254,0.3)" }}>
            <span className="material-symbols-outlined text-white" style={{ fontVariationSettings: "'FILL' 1" }}>
              neurology
            </span>
          </div>
          <span className="font-headline-sm text-headline-sm font-bold text-surface-bright tracking-tight">
            AISAM
          </span>
        </div>

        {/* Headline */}
        <div className="relative z-10 mb-12">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/20 border border-primary/30 text-primary-fixed-dim font-label-md text-label-md mb-stack-md">
            <span className="material-symbols-outlined text-[14px]">verified</span>
            ENTERPRISE PRECISION
          </div>
          <h1 className="font-display-lg text-display-lg text-surface-bright mb-stack-md leading-tight">
            Surgical-grade{" "}
            <span className="text-primary-fixed-dim">ad optimization</span>{" "}
            powered by neural networks.
          </h1>
          <p className="font-body-lg text-body-lg text-outline-variant max-w-xl">
            Deploy AISAM&apos;s advanced command center to manage, optimize, and scale
            your social advertising with unmatched intelligence and speed.
          </p>
        </div>

        {/* Stats */}
        <div className="relative z-10 flex gap-gutter">
          <div className="flex-1 p-stack-md rounded-xl bg-white/5 backdrop-blur-xl border border-white/10">
            <span className="text-label-md text-primary-fixed-dim block mb-1">Real-time ROAS</span>
            <span className="font-headline-md text-headline-md text-surface-bright">+24.8%</span>
          </div>
          <div className="flex-1 p-stack-md rounded-xl bg-white/5 backdrop-blur-xl border border-white/10">
            <span className="text-label-md text-primary-fixed-dim block mb-1">Neural Sync</span>
            <span className="font-headline-md text-headline-md text-surface-bright">ACTIVE</span>
          </div>
        </div>
      </section>

      {/* ── Right Panel: Sign Up Form ── */}
      <section className="w-full md:w-1/2 lg:w-2/5 bg-surface-container-lowest min-h-screen flex items-center justify-center p-margin-mobile md:p-margin-desktop overflow-y-auto">
        <div className="w-full max-w-md py-stack-lg">
          {/* Mobile Logo */}
          <div className="md:hidden flex items-center gap-2 mb-stack-lg">
            <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
              <span className="material-symbols-outlined text-white text-[18px]" style={{ fontVariationSettings: "'FILL' 1" }}>
                neurology
              </span>
            </div>
            <span className="font-headline-sm text-headline-sm font-bold text-enterprise-navy tracking-tight">
              AISAM
            </span>
          </div>

          {/* Heading */}
          <div className="mb-stack-lg">
            <h2 className="font-headline-lg text-headline-lg text-on-surface mb-1">
              Create your account
            </h2>
            <p className="font-body-md text-body-md text-outline">
              Get started with your AISAM command center.
            </p>
          </div>

          {/* Google Sign Up */}
          <button
            type="button"
            className="w-full flex items-center justify-center gap-3 bg-surface-container-low border border-outline-variant hover:bg-surface-container-high transition-colors py-3 px-4 rounded-lg font-body-md font-semibold text-on-surface active:scale-[0.98]"
          >
            <svg className="w-5 h-5" viewBox="0 0 24 24">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05" />
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
            </svg>
            Sign up with Google
          </button>

          {/* Divider */}
          <div className="relative my-stack-lg">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-outline-variant" />
            </div>
            <div className="relative flex justify-center">
              <span className="bg-surface-container-lowest px-4 text-label-sm text-outline font-semibold">
                Or email
              </span>
            </div>
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

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-stack-md">
            {/* Full Name */}
            <div>
              <label htmlFor="full_name" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
                Full Name
              </label>
              <input
                id="full_name"
                name="full_name"
                type="text"
                value={form.full_name}
                onChange={handleChange}
                placeholder="John Doe"
                required
                className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
              />
            </div>

            {/* Email */}
            <div>
              <label htmlFor="email" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
                Email Address
              </label>
              <input
                id="email"
                name="email"
                type="email"
                value={form.email}
                onChange={handleChange}
                placeholder="name@company.com"
                required
                className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface"
              />
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
                Password
              </label>
              <div className="relative">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? "text" : "password"}
                  value={form.password}
                  onChange={handleChange}
                  placeholder="Min. 8 characters"
                  required
                  className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border border-outline-variant focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors p-1"
                >
                  <span className="material-symbols-outlined text-[20px]">
                    {showPassword ? "visibility_off" : "visibility"}
                  </span>
                </button>
              </div>
            </div>

            {/* Confirm Password */}
            <div>
              <label htmlFor="confirm_password" className="block font-label-md text-label-md text-on-surface-variant mb-1 ml-1">
                Confirm Password
              </label>
              <div className="relative">
                <input
                  id="confirm_password"
                  name="confirm_password"
                  type={showConfirmPassword ? "text" : "password"}
                  value={form.confirm_password}
                  onChange={handleChange}
                  placeholder="Re-enter your password"
                  required
                  className={`w-full h-12 px-4 rounded-lg bg-surface-container-lowest border focus:border-primary-container focus:ring-1 focus:ring-primary-container outline-none transition-all placeholder:text-outline/50 text-body-md text-on-surface pr-12 ${
                    form.confirm_password && form.password !== form.confirm_password
                      ? "border-error focus:ring-error"
                      : "border-outline-variant"
                  }`}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors p-1"
                >
                  <span className="material-symbols-outlined text-[20px]">
                    {showConfirmPassword ? "visibility_off" : "visibility"}
                  </span>
                </button>
              </div>
              {form.confirm_password && form.password !== form.confirm_password && (
                <p className="mt-1 ml-1 font-label-sm text-label-sm text-error">Mật khẩu không khớp</p>
              )}
            </div>

            {/* Submit */}
            <div className="pt-2">
              <button
                type="submit"
                disabled={isLoading}
                className="w-full bg-primary-container text-on-primary font-body-md font-bold py-3 px-6 rounded-lg shadow-sm hover:bg-primary transition-all active:scale-[0.98] focus:ring-4 focus:ring-primary-container/20 disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                {isLoading ? (
                  <>
                    <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                    </svg>
                    Creating Account...
                  </>
                ) : (
                  "Create Account"
                )}
              </button>
            </div>
          </form>

          {/* Sign In Link */}
          <div className="mt-stack-lg text-center">
            <p className="font-body-sm text-body-sm text-on-surface-variant">
              Already have an account?{" "}
              <Link
                href="/login"
                className="text-primary font-semibold hover:underline decoration-2 underline-offset-4"
              >
                Sign In
              </Link>
            </p>
          </div>

          {/* Trust Note */}
          <div className="mt-12 pt-stack-lg border-t border-outline-variant/30 flex items-center justify-center gap-2">
            <span className="material-symbols-outlined text-[18px] text-success-green">lock</span>
            <span className="text-label-sm text-outline">
              Secure, encrypted registration
            </span>
          </div>
        </div>
      </section>
    </main>
  );
}
