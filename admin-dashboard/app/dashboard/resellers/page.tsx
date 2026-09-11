'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface Application {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  residenceName: string | null;
  status: number;
  submittedAt: string;
  employeeIdCode: string | null;
}

const statusLabels = ['PENDING', 'UNDER_REVIEW', 'APPROVED', 'REJECTED', 'WAITLISTED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800',
  'bg-blue-100 text-blue-800',
  'bg-green-100 text-green-800',
  'bg-red-100 text-red-800',
  'bg-gray-100 text-gray-800',
];

export default function ResellersPage() {
  const [apps, setApps] = useState<Application[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<Application | null>(null);
  const [residenceId, setResidenceId] = useState('');
  const [adminNotes, setAdminNotes] = useState('');
  const [message, setMessage] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/ResellerApplications');
      setApps(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleReview = async (status: number) => {
    if (!selected) return;
    try {
      await api.put(`/api/ResellerApplications/${selected.id}/review`, {
        status,
        residenceId: status === 2 ? residenceId : null,
        adminNotes,
      });
      setMessage(`Application ${statusLabels[status].toLowerCase()}.`);
      setSelected(null);
      setResidenceId('');
      setAdminNotes('');
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Reseller Applications</h1>
        <button onClick={load} className="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
          Refresh
        </button>
      </div>

      {message && <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg">{message}</div>}

      {loading ? <p>Loading...</p> : apps.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No applications yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Phone</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Employee ID</th>
                <th className="px-4 py-3">Submitted</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {apps.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{a.firstName} {a.lastName}</td>
                  <td className="px-4 py-3">{a.email}</td>
                  <td className="px-4 py-3">{a.phoneNumber}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[a.status]}`}>
                      {statusLabels[a.status]}
                    </span>
                  </td>
                  <td className="px-4 py-3">{a.employeeIdCode ?? '—'}</td>
                  <td className="px-4 py-3 text-gray-500">{new Date(a.submittedAt).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    {(a.status === 0 || a.status === 1) && (
                      <button
                        onClick={() => setSelected(a)}
                        className="text-xs text-blue-700 hover:text-blue-900"
                      >
                        Review
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selected && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <h2 className="text-lg font-semibold mb-4">Review Application</h2>
            <p className="text-sm text-gray-600 mb-4">
              {selected.firstName} {selected.lastName} · {selected.email}
            </p>
            <label className="block text-sm font-medium mb-1">Residence ID (required for approval)</label>
            <input
              type="text"
              placeholder="e.g. 11111111-2222-3333-4444-555555555555"
              value={residenceId}
              onChange={(e) => setResidenceId(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg mb-3 text-sm font-mono"
            />
            <p className="text-xs text-gray-500 mb-3">
              Get a residence ID from the database: <code>sqlite3 /opt/bakery/bakery.db "SELECT Id FROM Residences LIMIT 1;"</code>
            </p>
            <label className="block text-sm font-medium mb-1">Admin Notes (optional)</label>
            <textarea
              value={adminNotes}
              onChange={(e) => setAdminNotes(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg mb-4"
              rows={2}
            />
            <div className="flex justify-end gap-2">
              <button onClick={() => setSelected(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                Cancel
              </button>
              <button onClick={() => handleReview(3)} className="px-4 py-2 text-sm bg-red-600 text-white rounded-lg">
                Reject
              </button>
              <button onClick={() => handleReview(2)} className="px-4 py-2 text-sm bg-green-600 text-white rounded-lg">
                Approve
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
