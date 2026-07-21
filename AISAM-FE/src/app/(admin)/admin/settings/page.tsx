"use client";

import AdminHeader from "@/components/admin/AdminHeader";
import Link from "next/link";

const settingCards = [
  { title: "AI Providers", description: "Configure AI models, API keys, and credit costs", href: "/admin/settings/ai-providers", icon: "smart_toy" },
  { title: "Email", description: "Configure SMTP server and email templates", href: "/admin/settings/email", icon: "mail" },
  { title: "System", description: "Rate limits, maintenance mode, feature toggles", href: "/admin/settings/system", icon: "tune" },
  { title: "Security", description: "Manage your account security and password", href: "/admin/settings/security", icon: "security" },
];

export default function AdminSettingsPage() {
  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Settings</h2>
          <p className="text-gray-500 mt-1">Configure system-wide settings.</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {settingCards.map((card) => (
            <Link
              key={card.href}
              href={card.href}
              className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 hover:shadow-md hover:border-blue-300 transition-all group"
            >
              <span className="material-symbols-outlined text-3xl text-blue-600 mb-3">{card.icon}</span>
              <h3 className="text-lg font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">{card.title}</h3>
              <p className="text-sm text-gray-500 mt-1">{card.description}</p>
            </Link>
          ))}
        </div>
      </main>
    </>
  );
}
