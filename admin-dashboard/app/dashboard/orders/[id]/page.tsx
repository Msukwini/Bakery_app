'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import {
  ArrowLeft, User, Phone, Mail, MapPin, Calendar,
  Package, CheckCircle, Clock, X, CircleCheck, CircleX
} from 'lucide-react';

interface OrderItem {
  id: string;
  productVariantId: string;
  productName: string;
  variantName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

interface Order {
  id: string;
  orderNumber: string;
  customerName: string | null;
  customerPhone: string | null;
  customerEmail: string | null;
  deliveryAddress: string | null;
  requiredDate: string;
  status: number;
  totalAmount: number;
  paymentMethod: string | null;
  paymentReference: string | null;
  isPaid: boolean;
  createdAt: string;
  updatedAt: string | null;
  adminNotes: string | null;
  items: OrderItem[];
}

const statusLabels = ['PENDING', 'CONFIRMED', 'PREPARING', 'READY', 'OUT_FOR_DELIVERY', 'DELIVERED', 'PAID', 'CANCELLED', 'REFUNDED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800',
  'bg-blue-100 text-blue-800',
  'bg-purple-100 text-purple-800',
  'bg-indigo-100 text-indigo-800',
  'bg-orange-100 text-orange-800',
  'bg-teal-100 text-teal-800',
  'bg-green-100 text-green-800',
  'bg-red-100 text-red-800',
  'bg-gray-100 text-gray-800',
];

function fmtDate(iso: string | null) {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('en-ZA', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
  });
}

export default function OrderDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [notes, setNotes] = useState('');
  const [updating, setUpdating] = useState(false);
  const [paymentRef, setPaymentRef] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get(`/api/Orders/${id}`);
      setOrder(res.data);
      setNotes(res.data.adminNotes || '');
      setPaymentRef(res.data.paymentReference || '');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Order not found');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (id) load(); }, [id]);

  const updateStatus = async (status: number) => {
    if (!order) return;
    setUpdating(true);
    setError('');
    try {
      await api.put(`/api/Orders/${order.id}/status`, { status, adminNotes: notes });
      setMessage(`Status updated to ${statusLabels[status]}.`);
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to update');
    } finally {
      setUpdating(false);
    }
  };

  const updatePayment = async (isPaid: boolean) => {
    if (!order) return;
    setUpdating(true);
    setError('');
    try {
      await api.put(`/api/Orders/${order.id}/payment`, {
        paymentReference: paymentRef || order.orderNumber,
        isPaid,
      });
      setMessage(isPaid ? 'Marked as paid.' : 'Marked as unpaid.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to update payment');
    } finally {
      setUpdating(false);
    }
  };

  if (loading) return <p className="text-gray-500 p-8">Loading order...</p>;
  if (!order) return (
    <div className="p-8 text-center">
      <p className="text-gray-500 mb-4">{error || 'Order not found'}</p>
      <button onClick={() => router.push('/dashboard/orders')} className="text-blue-600 hover:underline">
        ← Back to orders
      </button>
    </div>
  );

  return (
    <div>
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div className="flex items-center gap-3">
          <button
            onClick={() => router.push('/dashboard/orders')}
            className="p-2 bg-gray-100 hover:bg-gray-200 rounded-lg"
          >
            <ArrowLeft size={18} />
          </button>
          <div>
            <h1 className="text-xl sm:text-2xl font-bold text-gray-800 flex items-center gap-3">
              {order.orderNumber}
              <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[order.status]}`}>
                {statusLabels[order.status]}
              </span>
            </h1>
            <p className="text-xs text-gray-500 mt-1">Placed {fmtDate(order.createdAt)}</p>
          </div>
        </div>
        {order.isPaid && (
          <span className="flex items-center gap-2 px-3 py-2 bg-green-50 text-green-700 text-sm rounded-lg self-start sm:self-auto">
            <CircleCheck size={16} /> PAID
          </span>
        )}
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}
      {error && (
        <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{error}</div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left column: customer + items */}
        <div className="lg:col-span-2 space-y-6">
          {/* Customer */}
          <div className="bg-white rounded-xl border p-5">
            <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
              <User size={18} /> Customer
            </h2>
            <div className="space-y-3 text-sm">
              <div className="flex items-center gap-3">
                <User size={16} className="text-gray-400" />
                <span className="font-medium">{order.customerName ?? '—'}</span>
              </div>
              <div className="flex items-center gap-3">
                <Phone size={16} className="text-gray-400" />
                <span>{order.customerPhone ?? '—'}</span>
              </div>
              <div className="flex items-center gap-3">
                <Mail size={16} className="text-gray-400" />
                <span>{order.customerEmail ?? '—'}</span>
              </div>
              <div className="flex items-start gap-3">
                <MapPin size={16} className="text-gray-400 mt-0.5" />
                <span>{order.deliveryAddress ?? '—'}</span>
              </div>
              <div className="flex items-center gap-3 pt-3 border-t">
                <Calendar size={16} className="text-gray-400" />
                <div>
                  <p className="text-xs text-gray-500">Required by</p>
                  <p className="font-medium">{fmtDate(order.requiredDate)}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Items */}
          <div className="bg-white rounded-xl border overflow-hidden">
            <div className="p-5 border-b border-gray-100">
              <h2 className="font-semibold text-gray-800 flex items-center gap-2">
                <Package size={18} /> Items ({order.items.length})
              </h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600 text-left">
                  <tr>
                    <th className="px-4 py-3">Product</th>
                    <th className="px-4 py-3 text-right">Qty</th>
                    <th className="px-4 py-3 text-right">Unit Price</th>
                    <th className="px-4 py-3 text-right">Subtotal</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {order.items.map((item) => (
                    <tr key={item.id}>
                      <td className="px-4 py-3">
                        <p className="font-medium text-gray-800">{item.productName}</p>
                        <p className="text-xs text-gray-500">{item.variantName}</p>
                      </td>
                      <td className="px-4 py-3 text-right">{item.quantity}</td>
                      <td className="px-4 py-3 text-right">R {item.unitPrice.toFixed(2)}</td>
                      <td className="px-4 py-3 text-right font-medium">R {item.totalPrice.toFixed(2)}</td>
                    </tr>
                  ))}
                  <tr className="bg-gray-50">
                    <td colSpan={3} className="px-4 py-3 text-right font-semibold text-gray-700">
                      Total
                    </td>
                    <td className="px-4 py-3 text-right text-lg font-bold text-gray-800">
                      R {order.totalAmount.toFixed(2)}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        {/* Right column: actions */}
        <div className="space-y-6">
          {/* Status update */}
          <div className="bg-white rounded-xl border p-5">
            <h2 className="font-semibold text-gray-800 mb-3 flex items-center gap-2">
              <Clock size={18} /> Order Status
            </h2>
            <div className="space-y-2 mb-4">
              {statusLabels.map((label, i) => (
                <button
                  key={i}
                  onClick={() => updateStatus(i)}
                  disabled={updating || order.status === i}
                  className={`w-full text-left px-3 py-2 rounded-lg text-sm transition ${
                    order.status === i
                      ? 'bg-blue-50 text-blue-700 font-medium border border-blue-200'
                      : 'text-gray-700 hover:bg-gray-50 border border-transparent'
                  }`}
                >
                  {order.status === i && '● '}
                  {label}
                </button>
              ))}
            </div>

            <label className="block text-xs font-medium text-gray-600 mb-1">Admin Notes</label>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              className="w-full px-3 py-2 border rounded-lg text-sm"
              placeholder="Optional note to save with status"
            />
          </div>

          {/* Payment */}
          <div className="bg-white rounded-xl border p-5">
            <h2 className="font-semibold text-gray-800 mb-3">Payment</h2>
            <div className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Method</span>
                <span className="font-medium">{order.paymentMethod ?? '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Reference</span>
                <span className="font-mono text-xs">{order.paymentReference ?? '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Status</span>
                <span className={order.isPaid ? 'text-green-600 font-medium' : 'text-red-600 font-medium'}>
                  {order.isPaid ? 'PAID' : 'UNPAID'}
                </span>
              </div>
            </div>

            <label className="block text-xs font-medium text-gray-600 mb-1 mt-4">Payment Reference</label>
            <input
              value={paymentRef}
              onChange={(e) => setPaymentRef(e.target.value)}
              placeholder="e.g., EFT-12345"
              className="w-full px-3 py-2 border rounded-lg text-sm mb-3"
            />

            {order.isPaid ? (
              <button
                onClick={() => updatePayment(false)}
                disabled={updating}
                className="w-full flex items-center justify-center gap-2 px-4 py-2 text-sm bg-red-50 hover:bg-red-100 disabled:bg-red-50 text-red-700 rounded-lg"
              >
                <CircleX size={14} /> Mark as Unpaid
              </button>
            ) : (
              <button
                onClick={() => updatePayment(true)}
                disabled={updating}
                className="w-full flex items-center justify-center gap-2 px-4 py-2 text-sm bg-green-600 hover:bg-green-700 disabled:bg-green-300 text-white rounded-lg"
              >
                <CheckCircle size={14} /> Mark as Paid
              </button>
            )}
          </div>

          {/* Timeline */}
          <div className="bg-white rounded-xl border p-5">
            <h2 className="font-semibold text-gray-800 mb-3">Timeline</h2>
            <div className="space-y-3 text-sm">
              <div className="flex items-start gap-3">
                <CheckCircle size={16} className="text-blue-600 mt-0.5" />
                <div>
                  <p className="font-medium text-gray-700">Order Placed</p>
                  <p className="text-xs text-gray-500">{fmtDate(order.createdAt)}</p>
                </div>
              </div>
              {order.updatedAt && order.updatedAt !== order.createdAt && (
                <div className="flex items-start gap-3">
                  <CheckCircle size={16} className="text-gray-400 mt-0.5" />
                  <div>
                    <p className="font-medium text-gray-700">Last Updated</p>
                    <p className="text-xs text-gray-500">{fmtDate(order.updatedAt)}</p>
                  </div>
                </div>
              )}
            </div>
            {order.adminNotes && (
              <div className="mt-4 pt-4 border-t border-gray-100">
                <p className="text-xs font-medium text-gray-500 mb-1">Admin Notes</p>
                <p className="text-sm text-gray-700">{order.adminNotes}</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
