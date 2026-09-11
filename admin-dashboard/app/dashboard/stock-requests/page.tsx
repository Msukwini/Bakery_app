'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { CheckCircle, XCircle } from 'lucide-react';

interface StockRequest {
  id: string;
  resellerCode: string;
  productName: string;
  variantName: string;
  requestedQuantity: number;
  allocatedQuantity: number | null;
  status: number;
  requestedAt: string;
  resellerNotes: string | null;
}

const statusLabels = ['PENDING', 'APPROVED', 'ALLOCATED', 'RECEIVED', 'REJECTED', 'CANCELLED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800',
  'bg-blue-100 text-blue-800',
  'bg-purple-100 text-purple-800',
  'bg-green-100 text-green-800',
  'bg-red-100 text-red-800',
  'bg-gray-100 text-gray-800',
];

export default function StockRequestsPage() {
  const [requests, setRequests] = useState<StockRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<StockRequest | null>(null);
  const [allocatedQty, setAllocatedQty] = useState('');
  const [adminNotes, setAdminNotes] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [action, setAction] = useState<'approve' | 'reject' | null>(null);
  const [message, setMessage] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/StockRequests');
      setRequests(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleApprove = async () => {
    if (!selected || !allocatedQty) return;
    try {
      await api.put(`/api/StockRequests/${selected.id}/approve`, {
        allocatedQuantity: parseInt(allocatedQty),
        adminNotes,
      });
      setMessage('Request approved and stock allocated.');
      setSelected(null);
      setAllocatedQty('');
      setAdminNotes('');
      setAction(null);
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to approve');
    }
  };

  const handleReject = async () => {
    if (!selected || !rejectionReason) return;
    try {
      await api.put(`/api/StockRequests/${selected.id}/reject`, {
        rejectionReason,
        allocatedQuantity: 0,
      });
      setMessage('Request rejected.');
      setSelected(null);
      setRejectionReason('');
      setAction(null);
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to reject');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Stock Requests</h1>
        <button onClick={load} className="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
          Refresh
        </button>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg">{message}</div>
      )}

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : requests.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border border-gray-200 text-center text-gray-500">
          No stock requests yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Reseller</th>
                <th className="px-4 py-3">Product</th>
                <th className="px-4 py-3">Requested</th>
                <th className="px-4 py-3">Allocated</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Date</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {requests.map((r) => (
                <tr key={r.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{r.resellerCode}</td>
                  <td className="px-4 py-3">
                    {r.productName} <span className="text-gray-400">·</span> {r.variantName}
                  </td>
                  <td className="px-4 py-3">{r.requestedQuantity}</td>
                  <td className="px-4 py-3">{r.allocatedQuantity ?? '—'}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[r.status]}`}>
                      {statusLabels[r.status]}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {new Date(r.requestedAt).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    {r.status === 0 && (
                      <div className="flex gap-2">
                        <button
                          onClick={() => { setSelected(r); setAllocatedQty(r.requestedQuantity.toString()); setAction('approve'); }}
                          className="flex items-center gap-1 text-xs text-green-700 hover:text-green-900"
                        >
                          <CheckCircle size={14} /> Approve
                        </button>
                        <button
                          onClick={() => { setSelected(r); setAction('reject'); }}
                          className="flex items-center gap-1 text-xs text-red-700 hover:text-red-900"
                        >
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

      {selected && action === 'approve' && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <h2 className="text-lg font-semibold mb-4">Approve Request</h2>
            <p className="text-sm text-gray-600 mb-4">
              {selected.resellerCode} requested {selected.requestedQuantity} × {selected.productName} ({selected.variantName})
            </p>
            <label className="block text-sm font-medium mb-1">Allocated Quantity</label>
            <input
              type="number"
              value={allocatedQty}
              onChange={(e) => setAllocatedQty(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg mb-3"
            />
            <label className="block text-sm font-medium mb-1">Notes (optional)</label>
            <textarea
              value={adminNotes}
              onChange={(e) => setAdminNotes(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg mb-4"
              rows={2}
            />
            <div className="flex justify-end gap-2">
              <button onClick={() => { setSelected(null); setAction(null); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                Cancel
              </button>
              <button onClick={handleApprove} className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white rounded-lg">
                Approve
              </button>
            </div>
          </div>
        </div>
      )}

      {selected && action === 'reject' && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <h2 className="text-lg font-semibold mb-4">Reject Request</h2>
            <label className="block text-sm font-medium mb-1">Reason (required)</label>
            <textarea
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg mb-4"
              rows={3}
            />
            <div className="flex justify-end gap-2">
              <button onClick={() => { setSelected(null); setAction(null); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                Cancel
              </button>
              <button onClick={handleReject} className="px-4 py-2 text-sm bg-red-600 hover:bg-red-700 text-white rounded-lg">
                Reject
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
