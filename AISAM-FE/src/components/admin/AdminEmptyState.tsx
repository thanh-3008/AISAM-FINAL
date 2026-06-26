export default function AdminEmptyState({ message = "No data found.", icon = "inbox" }: { message?: string; icon?: string }) {
  return (
    <div className="text-center py-16">
      <span className="material-symbols-outlined text-5xl text-gray-300">{icon}</span>
      <p className="text-sm text-[#424656] mt-3">{message}</p>
    </div>
  );
}
