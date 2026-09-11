'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface Assignment {
  id: string;
  resellerCode: string;
  permanentDeliveryCode: string;
  actualDeliveryCode: string | null;
  type: number;
  startDate: string;
  endDate: string | null;
  reason: string;
}

export default function DeliveryPage() {
  const [employeeId, setEmployeeId] = useState('');
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(false);

  const load = async () => {
    if (!employeeId) return;
    setLoading(true);
    try {
      const res = await api.get(`/api/Delivery/deliveryemployee/${employeeId}/assignments`);
      setAssignments(res.data);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Delivery</h1>

      <div className="bg-white p-6 rounded-xl border mb-6">
        <label className="block text-sm font-medium mb-2">Delivery Employee ID (GUID)</label>
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="Paste delivery employee GUID"
            value={employeeId}
            onChange={(e) => setEmployeeId(e.target.value)}
            className="flex-1 px-3 py-2 border rounded-lg text-sm font-mono"
          />
          <button onClick={load} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
            Load
          </button>
        </div>
      </div>

      {loading ? <p>Loading...</p> : assignments.length > 0 && (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Reseller</th>
                <th className="px-4 py-3">Permanent Delivery</th>
                <th className="px-4 py-3">Actual Delivery</th>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Period</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {assignments.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{a.resellerCode}</td>
                  <td className="px-4 py-3">{a.permanentDeliveryCode}</td>
                  <td className="px-4 py-3">{a.actualDeliveryCode ?? '—'}</td>
                  <td className="px-4 py-3">{a.type === 0 ? 'PERMANENT' : 'TEMPORARY'}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {new Date(a.startDate).toLocaleDateString()} → {a.endDate ? new Date(a.endDate).toLocaleDateString() : 'present'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
