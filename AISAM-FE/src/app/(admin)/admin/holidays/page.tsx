"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminHolidays, updateAdminHoliday, AdminHolidayDto } from "@/services/adminService";

export default function AdminHolidaysPage() {
  const [holidays, setHolidays] = useState<AdminHolidayDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [year, setYear] = useState<number>(new Date().getFullYear());
  const [editingHoliday, setEditingHoliday] = useState<AdminHolidayDto | null>(null);

  useEffect(() => {
    loadHolidays();
  }, [year]);

  const loadHolidays = async () => {
    setLoading(true);
    const data = await fetchAdminHolidays(year);
    if (data) setHolidays(data);
    setLoading(false);
  };

  const handleEdit = (holiday: AdminHolidayDto) => {
    setEditingHoliday({ ...holiday });
  };

  const handleSave = async () => {
    if (!editingHoliday) return;
    
    const success = await updateAdminHoliday(editingHoliday.id, {
      name: editingHoliday.name,
      localName: editingHoliday.localName,
      isActive: editingHoliday.isActive,
    });

    if (success) {
      setHolidays((prev) => prev.map((h) => h.id === editingHoliday.id ? editingHoliday : h));
      setEditingHoliday(null);
    } else {
      alert("Failed to update holiday.");
    }
  };

  const handleToggleActive = async (holiday: AdminHolidayDto) => {
    const success = await updateAdminHoliday(holiday.id, {
      isActive: !holiday.isActive,
    });

    if (success) {
      setHolidays((prev) => prev.map((h) => h.id === holiday.id ? { ...h, isActive: !h.isActive, isManuallyOverridden: true } : h));
    } else {
      alert("Failed to toggle status.");
    }
  };

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Holiday Management" }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Holiday Management" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Holiday Management</h2>
            <p className="text-gray-500 mt-1">Manage system holidays, customize names, and toggle availability.</p>
          </div>
          <div className="flex items-center gap-3">
            <select 
              value={year} 
              onChange={(e) => setYear(Number(e.target.value))}
              className="px-4 py-2 border border-gray-300 rounded-lg text-sm bg-white"
            >
              {[2024, 2025, 2026, 2027, 2028, 2029, 2030].map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Local Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Global Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {holidays.map((holiday) => (
                <tr key={holiday.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {new Date(holiday.exactDate).toLocaleDateString("vi-VN")}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {editingHoliday?.id === holiday.id ? (
                      <input 
                        className="border rounded px-2 py-1 w-full text-sm"
                        value={editingHoliday.localName}
                        onChange={(e) => setEditingHoliday({...editingHoliday, localName: e.target.value})}
                      />
                    ) : (
                      holiday.localName
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {editingHoliday?.id === holiday.id ? (
                      <input 
                        className="border rounded px-2 py-1 w-full text-sm"
                        value={editingHoliday.name}
                        onChange={(e) => setEditingHoliday({...editingHoliday, name: e.target.value})}
                      />
                    ) : (
                      holiday.name
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <StatusBadge status={holiday.isActive ? "Active" : "Disabled"} variant={holiday.isActive ? "success" : "warning"} />
                    {holiday.isManuallyOverridden && <span className="text-[10px] ml-2 text-gray-400">(Overridden)</span>}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 flex gap-2">
                    {editingHoliday?.id === holiday.id ? (
                      <>
                        <button onClick={handleSave} className="text-emerald-600 hover:text-emerald-900 bg-emerald-50 px-2 py-1 rounded">Save</button>
                        <button onClick={() => setEditingHoliday(null)} className="text-gray-600 hover:text-gray-900 bg-gray-100 px-2 py-1 rounded">Cancel</button>
                      </>
                    ) : (
                      <>
                        <button onClick={() => handleEdit(holiday)} className="text-blue-600 hover:text-blue-900 bg-blue-50 px-2 py-1 rounded">Edit</button>
                        <button onClick={() => handleToggleActive(holiday)} className={`${holiday.isActive ? "text-amber-600 bg-amber-50" : "text-emerald-600 bg-emerald-50"} px-2 py-1 rounded`}>
                          {holiday.isActive ? "Disable" : "Enable"}
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
              {holidays.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-6 py-4 text-center text-sm text-gray-500">No holidays found for this year.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </main>
    </>
  );
}
