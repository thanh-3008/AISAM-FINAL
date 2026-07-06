"use client";

interface AdminHeaderProps {
  title?: string;
  breadcrumbs?: { label: string; href?: string }[];
}

export default function AdminHeader({ title, breadcrumbs }: AdminHeaderProps) {
  return (
    <header className="sticky top-0 z-40 h-16 bg-gray-50 border-b border-gray-200 flex items-center justify-between px-8">
      <div className="flex items-center gap-4">
        {breadcrumbs && breadcrumbs.length > 0 ? (
          <nav className="flex items-center gap-2 text-sm">
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="flex items-center gap-2">
                {i > 0 && <span className="text-gray-400">/</span>}
                {crumb.href ? (
                  <a href={crumb.href} className="text-gray-600 hover:text-gray-900">
                    {crumb.label}
                  </a>
                ) : (
                  <span className="text-gray-900 font-medium">{crumb.label}</span>
                )}
              </span>
            ))}
          </nav>
        ) : (
          <h2 className="text-lg font-semibold text-gray-900">{title || "Admin"}</h2>
        )}
      </div>
      <div className="flex items-center gap-3">
        <span className="text-xs px-2.5 py-1 rounded-full bg-red-100 text-red-700 font-medium">
          Admin
        </span>
      </div>
    </header>
  );
}
