const variants: Record<string, string> = {
  active: "bg-success-green/10 text-success-green",
  Active: "bg-success-green/10 text-success-green",
  success: "bg-success-green/10 text-success-green",
  Success: "bg-success-green/10 text-success-green",
  suspended: "bg-warning-amber/10 text-warning-amber",
  Suspended: "bg-warning-amber/10 text-warning-amber",
  pending: "bg-warning-amber/10 text-warning-amber",
  Pending: "bg-warning-amber/10 text-warning-amber",
  cancelled: "bg-danger-red/10 text-danger-red",
  Cancelled: "bg-danger-red/10 text-danger-red",
  failed: "bg-danger-red/10 text-danger-red",
  Failed: "bg-danger-red/10 text-danger-red",
  archived: "bg-on-surface-variant/10 text-on-surface-variant",
  Archived: "bg-on-surface-variant/10 text-on-surface-variant",
  limited: "bg-on-surface-variant/10 text-on-surface-variant",
  Limited: "bg-on-surface-variant/10 text-on-surface-variant",
  free: "bg-on-surface-variant/10 text-on-surface-variant",
  Free: "bg-on-surface-variant/10 text-on-surface-variant",
  admin: "bg-secondary/10 text-secondary",
  Admin: "bg-secondary/10 text-secondary",
  user: "bg-primary/10 text-primary",
  User: "bg-primary/10 text-primary",
  vendor: "bg-warning-amber/10 text-warning-amber",
  Vendor: "bg-warning-amber/10 text-warning-amber",
};

export default function AdminStatusBadge({ status }: { status: string }) {
  const classes = variants[status] || "bg-on-surface-variant/10 text-on-surface-variant";
  return (
    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-label-3xs font-semibold ${classes}`}>
      {status}
    </span>
  );
}
