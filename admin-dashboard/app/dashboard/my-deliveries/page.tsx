'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Truck, MapPin, Package, CheckCircle, X, RefreshCw, Clock } from 'lucide-react';

interface Delivery {
  id: string;
  resellerCode: string;
  productName: string;
  variantName: string;
  allocatedQuantity: number;
  status: number;
  requestedAt: string;
  allocatedAt: string | null;
  expectedDeliveryDate: string | null;
  deliveryCompletedAt: string | null;
  deliveredQuantity: number | null;
  adminNotes: string | null;
}

const statusLabels = ['PENDING', 'APPROVED', 'ALLOCATED', 'RECEIVED', 'REJECTED', 'CANCELLED', 'DELIVERED', 'VARIANCE_PENDING', 'DISPUTED'];

export default function MyDeliveriesPage() {
  const [deliveries, setDeliveries] = useState<Delivery[]>([]);
  const [loading, setLoading] = useState(true);
  const [marking, setMarking] = useState<Delivery | null>(null);
  const [deliveredQty, setDeliveredQty] = useState('');
  const [deliveryNotes, setDeliveryNotes] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/StockRequests/my-deliveries');
      setDeliveries(res.data);
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load deliveries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleMarkDelivered = async () => {
    if (!marking) return;
    const qty = parseInt(deliveredQty);
    if (!qty || qty <= 0) { setError('Quantity must be positive'); return; }
    setError('');
    try {
      await api.put(`/api/StockRequests/${marking.id}/deliver`, {
        deliveredQuantity: qty,
        deliveryNotes,
      });
      setMessage(`Marked delivered: ${qty} × ${marking.productName}`);
      setMarking(null);
      setDeliveredQty('');
      setDeliveryNotes('');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const pending = deliveries.filter(d => d.status === 2); // ALLOCATED
  const completed = deliveries.filter(d => d.status >= 6); // DELIVERED or later

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">My Deliveries</h1>
          <p className="text-sm text-gray-500 mt-1">Deliver stock to resellers and mark as completed</p>
        </div>
        <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg self-start sm:self-auto">
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}

      {/* Pending deliveries */}
      <div className="mb-8">
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <Clock size={20} className="text-orange-600" />
          Pending Deliveries ({pending.length})
        </h2>

        {loading ? <p className="text-gray-500">Loading...</p> : pending.length === 0 ? (
          <div className="bg-white p-8 rounded-xl border text-center text-gray-500 text-sm">
            No pending deliveries right now.
          </div>
        ) : (
          <div className="space-y-3">
            {pending.map((d) => (
              <div key={d.id} className="bg-white rounded-xl border p-4">
                <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-2">
                      <Package size={16} className="text-blue-600" />
                      <span className="font-semibold text-gray-800">
                        {d.allocatedQuantity} × {d.productName} · {d.variantName}
                      </span>
                    </div>
                    <div className="flex items-center gap-2 text-sm text-gray-600">
                      <MapPin size={14} className="text-gray-400" />
                      <span>Deliver to: <strong>{d.resellerCode}</strong></span>
                    </div>
                    {d.expectedDeliveryDate && (
                      <p className="text-xs text-gray-500 mt-2">
                        Expected by: <strong>{new Date(d.expectedDeliveryDate).toLocaleDateString('en-ZA', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })}</strong>
                      </p>
                    )}
                    {d.adminNotes && (
                      <p className="text-xs text-gray-500 mt-1 italic">Admin: {d.adminNotes}</p>
                    )}
                  </div>
                  <button
                    onClick={() => {
                      setMarking(d);
                      setDeliveredQty(String(d.allocatedQuantity));
                      setDeliveryNotes('');
                    }}
                    className="flex items-center gap-2 px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white rounded-lg self-start"
                  >
                    <CheckCircle size={14} /> Mark Delivered
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Completed deliveries */}
      {completed.length > 0 && (
        <div>
          <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
            <CheckCircle size={20} className="text-green-600" />
            Completed ({completed.length})
          </h2>
          <div className="bg-white rounded-xl border overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 text-left">
                <tr>
                  <th className="px-3 sm:px-4 py-3">Reseller</th>
                  <th className="px-3 sm:px-4 py-3">Product</th>
                  <th className="px-3 sm:px-4 py-3">Delivered</th>
                  <th className="px-3 sm:px-4 py-3">Delivered At</th>
                  <th className="px-3 sm:px-4 py-3">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {completed.map((d) => (
                  <tr key={d.id}>
                    <td className="px-3 sm:px-4 py-3 font-medium">{d.resellerCode}</td>
                    <td className="px-3 sm:px-4 py-3">{d.productName} · {d.variantName}</td>
                    <td className="px-3 sm:px-4 py-3">{d.deliveredQuantity} / {d.allocatedQuantity}</td>
                    <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                      {d.deliveryCompletedAt ? new Date(d.deliveryCompletedAt).toLocaleString('en-ZA') : '—'}
                    </td>
                    <td className="px-3 sm:px-4 py-3">
                      <span className={`px-2 py-1 rounded text-xs font-medium ${
                        d.status === 6 ? 'bg-teal-100 text-teal-800' :
                        d.status === 7 ? 'bg-red-100 text-red-800' :
                        'bg-green-100 text-green-800'
                      }`}>
                        {statusLabels[d.status]}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Mark Delivered Modal */}
      {marking && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Mark as Delivered</h2>
              <button onClick={() => { setMarking(null); setError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="text-gray-700"><strong>Reseller:</strong> {marking.resellerCode}</p>
              <p className="text-gray-700"><strong>Product:</strong> {marking.productName} · {marking.variantName}</p>
              <p className="text-gray-700"><strong>Allocated:</strong> {marking.allocatedQuantity} units</p>
            </div>

            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Quantity Delivered <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min="1"
                  value={deliveredQty}
                  onChange={(e) => setDeliveredQty(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Change this if you delivered a different amount than allocated.
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Notes (optional)</label>
                <textarea
                  value={deliveryNotes}
                  onChange={(e) => setDeliveryNotes(e.target.value)}
                  rows={2}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., Left with receptionist"
                />
              </div>

              {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>}

              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => { setMarking(null); setError(''); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button
                  onClick={handleMarkDelivered}
                  className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white rounded-lg"
                >
                  Confirm Delivery
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
