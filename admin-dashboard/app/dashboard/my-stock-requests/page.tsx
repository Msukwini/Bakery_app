'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

const statusLabels = ['PENDING', 'APPROVED', 'ALLOCATED', 'RECEIVED', 'REJECTED', 'CANCELLED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800', 'bg-blue-100 text-blue-800',
  'bg-purple-100 text-purple-800', 'bg-green-100 text-green-800',
  'bg-red-100 text-red-800', 'bg-gray-100 text-gray-800',
];

export default function MyStockRequestsPage() {
  const [requests, setRequests] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/api/StockRequests')
      .then((r) => setRequests(r.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-2">My Stock Requests</h1>
      <p className="text-sm text-gray-500 mb-6">Requests you've made for stock</p>

      {loading ? <p>Loading...</p> : requests.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          You haven't made any stock requests yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Product</th>
                <th className="px-3 sm:px-4 py-3">Requested</th>
                <th className="px-3 sm:px-4 py-3">Allocated</th>
                <th className="px-3 sm:px-4 py-3">Status</th>
                <th className="px-3 sm:px-4 py-3">Date</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {requests.map((r) => (
                <tr key={r.id}>
                  <td className="px-3 sm:px-4 py-3">{r.productName} · {r.variantName}</td>
                  <td className="px-3 sm:px-4 py-3">{r.requestedQuantity}</td>
                  <td className="px-3 sm:px-4 py-3">{r.allocatedQuantity ?? '—'}</td>
                  <td className="px-3 sm:px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[r.status]}`}>
                      {statusLabels[r.status]}
                    </span>
                  </td>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {new Date(r.requestedAt).toLocaleDateString()}
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
