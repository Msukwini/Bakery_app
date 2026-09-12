'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import ReviewApplicationModal from './review-modal';

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

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
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
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Name</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Email</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Phone</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Status</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Employee ID</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Submitted</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {apps.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50">
                  <td className="px-3 sm:px-4 py-2 sm:py-3 font-medium">{a.firstName} {a.lastName}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{a.email}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{a.phoneNumber}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[a.status]}`}>
                      {statusLabels[a.status]}
                    </span>
                  </td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{a.employeeIdCode ?? '—'}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3 text-gray-500">{new Date(a.submittedAt).toLocaleDateString()}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
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

      <ReviewApplicationModal
        application={selected}
        isOpen={!!selected}
        onClose={() => setSelected(null)}
        onSuccess={() => {
          setMessage('Application processed successfully.');
          load();
        }}
      />
    </div>
  );
}
