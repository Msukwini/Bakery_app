'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { CheckCircle, XCircle } from 'lucide-react';

interface Deposit {
  id: string;
  referenceNumber: string;
  deliveryEmployeeCode: string;
  amount: number;
  depositDate: string;
  bankName: string | null;
  bankReference: string | null;
  status: number;
  submittedAt: string;
}

const statusLabels = ['SUBMITTED', 'UNDER_REVIEW', 'APPROVED', 'REJECTED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800',
  'bg-blue-100 text-blue-800',
  'bg-green-100 text-green-800',
  'bg-red-100 text-red-800',
];

export default function DepositsPage() {
  const [deposits, setDeposits] = useState<Deposit[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Collections/deposits');
      setDeposits(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const review = async (id: string, status: number) => {
    try {
      await api.put(`/api/Collections/deposits/${id}/review`, {
        status,
        adminNotes: status === 2 ? 'Verified' : '',
        rejectionReason: status === 3 ? 'Rejected by admin' : null,
      });
      setMessage(`Deposit ${statusLabels[status].toLowerCase()}.`);
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Deposits</h1>
        <button onClick={load} className="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
          Refresh
        </button>
      </div>

      {message && <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg">{message}</div>}

      {loading ? <p>Loading...</p> : deposits.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No deposits yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Reference</th>
                <th className="px-4 py-3">Employee</th>
                <th className="px-4 py-3">Amount</th>
                <th className="px-4 py-3">Bank</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Submitted</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {deposits.map((d) => (
                <tr key={d.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-xs">{d.referenceNumber}</td>
                  <td className="px-4 py-3 font-medium">{d.deliveryEmployeeCode}</td>
                  <td className="px-4 py-3">R {d.amount.toFixed(2)}</td>
                  <td className="px-4 py-3">{d.bankName ?? '—'} <span className="text-xs text-gray-400">{d.bankReference}</span></td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[d.status]}`}>
                      {statusLabels[d.status]}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">{new Date(d.submittedAt).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    {d.status === 0 && (
                      <div className="flex gap-2">
                        <button onClick={() => review(d.id, 2)} className="flex items-center gap-1 text-xs text-green-700">
                          <CheckCircle size={14} /> Approve
                        </button>
                        <button onClick={() => review(d.id, 3)} className="flex items-center gap-1 text-xs text-red-700">
                          <XCircle size={14} /> Reject
                        </button>
                      </div>
                    )}
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
