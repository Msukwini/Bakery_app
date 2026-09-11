'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface DeliveryEmployee {
  id: string;
  code: string;
  name: string;
  email: string;
}

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
  const [employees, setEmployees] = useState<DeliveryEmployee[]>([]);
  const [selectedId, setSelectedId] = useState('');
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    api.get('/api/Admin/delivery-employees')
      .then((res) => setEmployees(res.data))
      .catch(() => setMessage('Failed to load delivery employees'));
  }, []);

  const load = async () => {
    if (!selectedId) return;
    setLoading(true);
    setMessage('');
    try {
      const res = await api.get(`/api/Delivery/deliveryemployee/${selectedId}/assignments`);
      setAssignments(res.data);
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load assignments');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Delivery</h1>

      <div className="bg-white p-6 rounded-xl border mb-6">
        <label className="block text-sm font-medium mb-2">Select Delivery Employee</label>
        <div className="flex gap-2">
          <select
            value={selectedId}
            onChange={(e) => setSelectedId(e.target.value)}
            className="flex-1 px-3 py-2 border rounded-lg text-sm bg-white"
          >
            <option value="">— Choose an employee —</option>
            {employees.map((e) => (
              <option key={e.id} value={e.id}>
                {e.code} · {e.name}
              </option>
            ))}
          </select>
          <button
            onClick={load}
            disabled={!selectedId}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg text-sm"
          >
            Load
          </button>
        </div>
      </div>

      {message && <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{message}</div>}

      {loading ? <p className="text-gray-500">Loading...</p> : assignments.length > 0 ? (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Reseller</th>
                <th className="px-4 py-3">Permanent</th>
                <th className="px-4 py-3">Actual</th>
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
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      a.type === 0 ? 'bg-blue-100 text-blue-800' : 'bg-orange-100 text-orange-800'
                    }`}>
                      {a.type === 0 ? 'PERMANENT' : 'TEMPORARY'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {new Date(a.startDate).toLocaleDateString()} → {a.endDate ? new Date(a.endDate).toLocaleDateString() : 'present'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : selectedId ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No active assignments for this employee.
        </div>
      ) : null}
    </div>
  );
}
