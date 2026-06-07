"use client";

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { apiClient } from "@/lib/apiClient";
import { setToken, setRefreshToken, setStoredUser } from "@/lib/auth";
import { invalidateProfileCache } from "@/hooks/useProfiles";
import AuthShell from "@/components/auth/AuthShell";

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
    <AuthShell
      badge={{ icon: "verified", text: "Enterprise Precision" }}
      headline="Surgical-grade"
      headlineHighlight="ad optimization powered by neural networks."
      description="Deploy AISAM's advanced command center to manage, optimize, and scale your social advertising with unmatched intelligence and speed."
      backgroundImage="https://lh3.googleusercontent.com/aida-public/AB6AXuCGvsY8rjCVQDijqL44Y_QXXhr7BfVfQmXFJZbMl8bkDB62NW9Z-QHRj6yrcGuPRKLpwdKWOA9t03UQaArlYvvKmSekL0FHmpfqguHFQLMB6gbAzifMIQ7X1dDwiItEZRdU_wkSJgdnzBQvQqTEEXht3FjMt2Ioqnmc72KPbGLpTxQBDWbKFqbhzWcIRx2jtW3TAvf7QudIDZFalDqFPLniX8utIDB3KOQtHs6k2RZ88uolRoPkhPJp4kugJI8Hqj1b8_o6R5BhHow"
      stats={[
        { label: "Real-time ROAS", value: "+24.8%" },
        { label: "Neural Sync", value: "ACTIVE" },
      ]}
    >
      {/* Heading */}
      <div className="mb-stack-lg text-center lg:text-left">
        <h2 className="font-headline-lg text-headline-lg text-on-surface mb-2">
          Create your account
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant">
          Get started with your AISAM command center.
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

      {/* Google Sign Up */}
      <button
        type="button"
        className="w-full h-12 flex items-center justify-center gap-3 bg-surface-container-low border border-outline-variant rounded-lg hover:bg-surface-container-high transition-colors duration-200 active:scale-[0.98] mb-stack-lg"
      >
        <svg className="w-5 h-5" viewBox="0 0 24 24">
          <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
          <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
          <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05" />
          <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
        </svg>
        <span className="font-label-md text-label-md text-on-surface">
          Sign up with Google
        </span>
      </button>

      {/* Divider */}
      <div className="flex items-center gap-4 mb-stack-lg">
        <div className="h-px bg-outline-variant flex-1" />
        <span className="text-label-sm text-outline font-semibold">
          Or email
        </span>
        <div className="h-px bg-outline-variant flex-1" />
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-stack-md">
        {/* Full Name */}
        <div>
          <label htmlFor="full_name" className="block font-label-md text-label-md text-on-surface-variant mb-1">
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
          <label htmlFor="email" className="block font-label-md text-label-md text-on-surface-variant mb-1">
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
          <label htmlFor="password" className="block font-label-md text-label-md text-on-surface-variant mb-1">
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
              className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
            >
              <span className="material-symbols-outlined text-[20px]">
                {showPassword ? "visibility_off" : "visibility"}
              </span>
            </button>
          </div>
        </div>

        {/* Confirm Password */}
        <div>
          <label htmlFor="confirm_password" className="block font-label-md text-label-md text-on-surface-variant mb-1">
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
              className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
            >
              <span className="material-symbols-outlined text-[20px]">
                {showConfirmPassword ? "visibility_off" : "visibility"}
              </span>
            </button>
          </div>
          {form.confirm_password && form.password !== form.confirm_password && (
            <p className="mt-1 font-label-sm text-label-sm text-error">Mật khẩu không khớp</p>
          )}
        </div>

        {/* Submit */}
        <button
          type="submit"
          disabled={isLoading}
          className="w-full h-12 bg-primary-container text-on-primary-container font-label-md text-label-md rounded-lg hover:shadow-lg hover:opacity-90 active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <span className="w-5 h-5 border-2 border-white border-b-transparent rounded-full animate-spin inline-block" />
              <span>Creating Account...</span>
            </>
          ) : (
            <span>Create Account</span>
          )}
        </button>
      </form>

      {/* Sign In Link */}
      <div className="mt-stack-lg text-center">
        <p className="font-body-sm text-body-sm text-on-surface-variant">
          Already have an account?{" "}
          <Link
            href="/login"
            className="text-primary font-bold hover:underline"
          >
            Sign In
          </Link>
        </p>
      </div>

      {/* Trust Note */}
      <div className="mt-12 flex items-center justify-center gap-2 text-outline">
        <span className="material-symbols-outlined text-[16px] text-success-green">lock</span>
        <span className="font-label-sm text-label-sm">
          Secure, encrypted registration
        </span>
      </div>
    </AuthShell>
  );
}
