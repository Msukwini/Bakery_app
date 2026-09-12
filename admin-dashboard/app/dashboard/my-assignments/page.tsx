'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

export default function MyAssignmentsPage() {
  const [assignments, setAssignments] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const employeeId = localStorage.getItem('employeeGuid') || '';
    api.get(`/api/Delivery/deliveryemployee/${employeeId}/assignments`)
      .then((r) => setAssignments(r.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-2">My Assignments</h1>
      <p className="text-sm text-gray-500 mb-6">Resellers assigned to you</p>

      {loading ? <p>Loading...</p> : assignments.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No assignments yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Reseller</th>
                <th className="px-3 sm:px-4 py-3">Type</th>
                <th className="px-3 sm:px-4 py-3">Since</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {assignments.map((a) => (
                <tr key={a.id}>
                  <td className="px-3 sm:px-4 py-3 font-medium">{a.resellerCode}</td>
                  <td className="px-3 sm:px-4 py-3">{a.type === 0 ? 'PERMANENT' : 'TEMPORARY'}</td>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {new Date(a.startDate).toLocaleDateString()}
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
