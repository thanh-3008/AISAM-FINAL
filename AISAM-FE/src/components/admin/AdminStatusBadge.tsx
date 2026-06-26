const variants: Record<string, string> = {
  active: "bg-[#198038]/10 text-[#198038]",
  Active: "bg-[#198038]/10 text-[#198038]",
  success: "bg-[#198038]/10 text-[#198038]",
  Success: "bg-[#198038]/10 text-[#198038]",
  suspended: "bg-[#F1C21B]/10 text-[#F1C21B]",
  Suspended: "bg-[#F1C21B]/10 text-[#F1C21B]",
  pending: "bg-[#F1C21B]/10 text-[#F1C21B]",
  Pending: "bg-[#F1C21B]/10 text-[#F1C21B]",
  cancelled: "bg-[#DA1E28]/10 text-[#DA1E28]",
  Cancelled: "bg-[#DA1E28]/10 text-[#DA1E28]",
  failed: "bg-[#DA1E28]/10 text-[#DA1E28]",
  Failed: "bg-[#DA1E28]/10 text-[#DA1E28]",
  archived: "bg-gray-100 text-[#424656]",
  Archived: "bg-gray-100 text-[#424656]",
  limited: "bg-gray-100 text-[#424656]",
  Limited: "bg-gray-100 text-[#424656]",
  free: "bg-gray-100 text-[#424656]",
  Free: "bg-gray-100 text-[#424656]",
  admin: "bg-[#731be5]/10 text-[#731be5]",
  Admin: "bg-[#731be5]/10 text-[#731be5]",
  user: "bg-[#004ccd]/10 text-[#004ccd]",
  User: "bg-[#004ccd]/10 text-[#004ccd]",
  vendor: "bg-[#F1C21B]/10 text-[#F1C21B]",
  Vendor: "bg-[#F1C21B]/10 text-[#F1C21B]",
};

export default function AdminStatusBadge({ status }: { status: string }) {
  const classes = variants[status] || "bg-gray-100 text-[#424656]";
  return (
    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${classes}`}>
      {status}
    </span>
  );
}
