'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Package, Plus, X, RefreshCw, CircleCheck, CircleX, Clock, Truck, TriangleAlert } from 'lucide-react';

interface ProductVariant {
  id: string; productName: string; sizeName: string; unitPrice: number;
}

interface StockRequest {
  id: string;
  productName: string;
  variantName: string;
  requestedQuantity: number;
  allocatedQuantity: number | null;
  deliveredQuantity: number | null;
  receivedQuantity: number | null;
  status: number;
  requestedAt: string;
  reviewedAt: string | null;
  allocatedAt: string | null;
  deliveryCompletedAt: string | null;
  receivedAt: string | null;
  expectedDeliveryDate: string | null;
  assignedDeliveryCode: string | null;
  assignedDeliveryName: string | null;
  actualDeliveryCode: string | null;
  actualDeliveryName: string | null;
  adminNotes: string | null;
  rejectionReason: string | null;
  resellerNotes: string | null;
  deliveryNotes: string | null;
  receiptNotes: string | null;
  variance: number | null;
  varianceStatus: number;
  varianceNotes: string | null;
}

const statusLabels = ['PENDING', 'APPROVED', 'ALLOCATED', 'RECEIVED', 'REJECTED', 'CANCELLED', 'DELIVERED', 'VARIANCE', 'DISPUTED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800', 'bg-blue-100 text-blue-800',
  'bg-purple-100 text-purple-800', 'bg-green-100 text-green-800',
  'bg-red-100 text-red-800', 'bg-gray-100 text-gray-800',
  'bg-teal-100 text-teal-800', 'bg-orange-100 text-orange-800',
  'bg-red-200 text-red-900',
];

function formatDateTime(iso: string | null) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-ZA', {
    day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit',
  });
}

export default function MyStockRequestsPage() {
  const [requests, setRequests] = useState<StockRequest[]>([]);
  const [products, setProducts] = useState<ProductVariant[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [message, setMessage] = useState('');
  const [expanded, setExpanded] = useState<string | null>(null);
  const [form, setForm] = useState({ productVariantId: '', requestedQuantity: '', resellerNotes: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [confirming, setConfirming] = useState<StockRequest | null>(null);
  const [receivedQty, setReceivedQty] = useState('');
  const [receiptNotes, setReceiptNotes] = useState('');
  const [confirmError, setConfirmError] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const [req, prod] = await Promise.all([
        api.get('/api/StockRequests'),
        api.get('/api/Products/variants'),
      ]);
      setRequests(req.data);
      setProducts(prod.data);
      if (prod.data.length > 0 && !form.productVariantId) {
        setForm((f) => ({ ...f, productVariantId: prod.data[0].id }));
      }
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      await api.post('/api/StockRequests', {
        productVariantId: form.productVariantId,
        requestedQuantity: parseInt(form.requestedQuantity),
        resellerNotes: form.resellerNotes || null,
      });
      setMessage('Stock request submitted!');
      setShowModal(false);
      setForm({ productVariantId: products[0]?.id || '', requestedQuantity: '', resellerNotes: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to submit');
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmReceipt = async () => {
    if (!confirming) return;
    const qty = parseInt(receivedQty);
    if (isNaN(qty) || qty < 0) { setConfirmError('Enter a valid quantity'); return; }
    setConfirmError('');
    try {
      await api.put(`/api/StockRequests/${confirming.id}/receive`, {
        receivedQuantity: qty,
        receiptNotes,
      });
      setMessage('Receipt confirmed!');
      setConfirming(null);
      setReceivedQty('');
      setReceiptNotes('');
      load();
    } catch (err: any) {
      setConfirmError(err.response?.data?.error || 'Failed to confirm');
    }
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">My Stock Requests</h1>
          <p className="text-sm text-gray-500 mt-1">Request stock and confirm receipt</p>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
            <RefreshCw size={14} /> Refresh
          </button>
          <button
            onClick={() => setShowModal(true)}
            disabled={products.length === 0}
            className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg"
          >
            <Plus size={14} /> Request Stock
          </button>
        </div>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}

      {loading ? <p className="text-gray-500">Loading...</p> : requests.length === 0 ? (
        <div className="bg-white p-8 sm:p-12 rounded-xl border text-center">
          <Package className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No stock requests yet</p>
          <button
            onClick={() => setShowModal(true)}
            disabled={products.length === 0}
            className="mt-4 inline-flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg"
          >
            <Plus size={14} /> Request Stock
          </button>
        </div>
      ) : (
        <div className="space-y-3">
          {requests.map((r) => {
            const isExpanded = expanded === r.id;
            const needsConfirmation = r.status === 6;
            return (
              <div key={r.id} className={`bg-white rounded-xl border overflow-hidden ${needsConfirmation ? 'border-teal-300 shadow-sm' : ''}`}>
                <button
                  onClick={() => setExpanded(isExpanded ? null : r.id)}
                  className="w-full p-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 hover:bg-gray-50 text-left"
                >
                  <div className="flex items-center gap-3 min-w-0 flex-1">
                    <div className="min-w-0 flex-1">
                      <p className="font-medium text-gray-800 truncate">
                        {r.productName} · {r.variantName}
                      </p>
                      <p className="text-xs text-gray-500 mt-0.5">
                        Requested {r.requestedQuantity}
                        {r.allocatedQuantity != null && ` · Allocated ${r.allocatedQuantity}`}
                        {' · '}{formatDateTime(r.requestedAt)}
                      </p>
                    </div>
                  </div>
                  <span className={`px-2 py-1 rounded text-xs font-semibold self-start sm:self-auto ${statusColors[r.status]}`}>
                    {statusLabels[r.status]}
                  </span>
                </button>

                {needsConfirmation && (
                  <div className="px-4 pb-3 flex justify-end">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        setConfirming(r);
                        setReceivedQty(String(r.deliveredQuantity ?? r.allocatedQuantity ?? ''));
                        setReceiptNotes('');
                      }}
                      className="flex items-center gap-2 px-4 py-2 text-sm bg-teal-600 hover:bg-teal-700 text-white rounded-lg"
                    >
                      <CircleCheck size={14} /> Confirm Receipt
                    </button>
                  </div>
                )}

                {isExpanded && (
                  <div className="border-t border-gray-100 p-4 bg-gray-50">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <div>
                        <h3 className="text-xs font-semibold text-gray-600 uppercase tracking-wide mb-3">Timeline</h3>
                        <div className="space-y-3">
                          <TL icon={Clock} label="Requested" value={formatDateTime(r.requestedAt)} />
                          {r.reviewedAt && <TL icon={CircleCheck} label={r.status === 4 ? 'Rejected' : 'Reviewed'} value={formatDateTime(r.reviewedAt)} color="text-blue-600" />}
                          {r.allocatedAt && <TL icon={CircleCheck} label="Stock Allocated" value={formatDateTime(r.allocatedAt)} color="text-purple-600" />}
                          {r.deliveryCompletedAt && <TL icon={Truck} label="Delivered by Driver" value={formatDateTime(r.deliveryCompletedAt)} color="text-teal-600" />}
                          {r.receivedAt && <TL icon={CircleCheck} label="You Confirmed Receipt" value={formatDateTime(r.receivedAt)} color="text-green-600" />}
                        </div>
                      </div>

                      <div>
                        <h3 className="text-xs font-semibold text-gray-600 uppercase tracking-wide mb-3">Details</h3>
                        <div className="space-y-2 text-sm">
                          <div className="flex justify-between">
                            <span className="text-gray-500">Requested</span>
                            <span className="font-medium">{r.requestedQuantity} units</span>
                          </div>
                          {r.allocatedQuantity != null && (
                            <div className="flex justify-between">
                              <span className="text-gray-500">Approved</span>
                              <span className="font-medium text-green-700">{r.allocatedQuantity} units</span>
                            </div>
                          )}
                          {r.deliveredQuantity != null && (
                            <div className="flex justify-between">
                              <span className="text-gray-500">Driver delivered</span>
                              <span className="font-medium text-teal-700">{r.deliveredQuantity} units</span>
                            </div>
                          )}
                          {r.receivedQuantity != null && (
                            <div className="flex justify-between">
                              <span className="text-gray-500">You received</span>
                              <span className="font-medium text-green-700">{r.receivedQuantity} units</span>
                            </div>
                          )}
                          {r.assignedDeliveryCode && (
                            <div className="pt-2 border-t border-gray-200">
                              <p className="text-xs text-gray-500 mb-1">Assigned delivery</p>
                              <p className="text-gray-700">{r.assignedDeliveryName} ({r.assignedDeliveryCode})</p>
                            </div>
                          )}
                          {r.deliveryNotes && (
                            <div className="pt-2 border-t border-gray-200">
                              <p className="text-xs text-gray-500 mb-1">Driver notes</p>
                              <p className="text-gray-700">{r.deliveryNotes}</p>
                            </div>
                          )}
                          {r.variance != null && r.variance !== 0 && (
                            <div className="pt-2 border-t border-gray-200">
                              <div className="flex items-center gap-1.5 text-red-700">
                                <TriangleAlert size={14} />
                                <span className="font-medium text-xs">Variance: {r.variance} units</span>
                              </div>
                              {r.varianceNotes && <p className="text-xs text-gray-600 mt-1">{r.varianceNotes}</p>}
                            </div>
                          )}
                          {r.adminNotes && (
                            <div className="pt-2 border-t border-gray-200">
                              <p className="text-xs text-gray-500 mb-1">Admin notes</p>
                              <p className="text-gray-700">{r.adminNotes}</p>
                            </div>
                          )}
                          {r.rejectionReason && (
                            <div className="pt-2 border-t border-gray-200">
                              <p className="text-xs text-red-600 mb-1 font-medium">Rejection reason</p>
                              <p className="text-red-700">{r.rejectionReason}</p>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Request Stock</h2>
              <button onClick={() => { setShowModal(false); setError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Product <span className="text-red-500">*</span></label>
                <select required value={form.productVariantId}
                  onChange={(e) => setForm({ ...form, productVariantId: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white">
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>{p.productName} · {p.sizeName}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Quantity <span className="text-red-500">*</span></label>
                <input type="number" required min="1" value={form.requestedQuantity}
                  onChange={(e) => setForm({ ...form, requestedQuantity: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" placeholder="e.g., 10" />
                <p className="text-xs text-gray-500 mt-1">Requests after 11 AM will be scheduled for next-day delivery.</p>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Notes (optional)</label>
                <textarea value={form.resellerNotes}
                  onChange={(e) => setForm({ ...form, resellerNotes: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" rows={2} placeholder="e.g., Need for weekend rush" />
              </div>
              {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>}
              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => { setShowModal(false); setError(''); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button type="submit" disabled={submitting}
                  className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg">
                  {submitting ? 'Submitting...' : 'Submit Request'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {confirming && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Confirm Receipt</h2>
              <button onClick={() => { setConfirming(null); setConfirmError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="text-gray-700"><strong>Product:</strong> {confirming.productName} · {confirming.variantName}</p>
              <p className="text-gray-700"><strong>Driver delivered:</strong> {confirming.deliveredQuantity} units</p>
              {confirming.actualDeliveryCode && (
                <p className="text-gray-700"><strong>Delivered by:</strong> {confirming.actualDeliveryName} ({confirming.actualDeliveryCode})</p>
              )}
            </div>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Quantity You Received <span className="text-red-500">*</span></label>
                <input type="number" min="0" value={receivedQty}
                  onChange={(e) => setReceivedQty(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg text-sm" />
                <p className="text-xs text-gray-500 mt-1">If this differs from what the driver says, a variance will be flagged for admin review.</p>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Notes (optional)</label>
                <textarea value={receiptNotes}
                  onChange={(e) => setReceiptNotes(e.target.value)}
                  rows={2} className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., 2 units were missing" />
              </div>
              {confirmError && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{confirmError}</p>}
              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => { setConfirming(null); setConfirmError(''); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button onClick={handleConfirmReceipt}
                  className="px-4 py-2 text-sm bg-teal-600 hover:bg-teal-700 text-white rounded-lg">
                  Confirm Receipt
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function TL({ icon: Icon, label, value, color = 'text-gray-600' }: any) {
  return (
    <div className="flex items-start gap-3">
      <div className={`mt-0.5 ${color}`}><Icon size={16} /></div>
      <div className="flex-1">
        <p className="text-sm font-medium text-gray-700">{label}</p>
        <p className="text-xs text-gray-500">{value}</p>
      </div>
    </div>
  );
}
