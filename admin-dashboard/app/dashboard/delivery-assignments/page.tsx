'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Truck, Plus, X, RefreshCw, User, Users } from 'lucide-react';

interface Assignment {
  id: string;
  resellerEmployeeId: string;
  resellerCode: string;
  resellerName: string;
  permanentDeliveryEmployeeId: string;
  permanentDeliveryCode: string;
  permanentDeliveryName: string;
  actualDeliveryEmployeeId: string | null;
  actualDeliveryCode: string | null;
  actualDeliveryName: string | null;
  type: string;
  startDate: string;
  endDate: string | null;
  reason: string;
}

interface Reseller {
  id: string;
  code: string;
  name: string;
  residenceName: string | null;
}

interface DeliveryEmp {
  id: string;
  code: string;
  name: string;
}

export default function DeliveryAssignmentsPage() {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [resellers, setResellers] = useState<Reseller[]>([]);
  const [deliveryEmps, setDeliveryEmps] = useState<DeliveryEmp[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [activeOnly, setActiveOnly] = useState(true);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const [form, setForm] = useState({
    resellerEmployeeId: '',
    deliveryEmployeeId: '',
    type: 0,
    startDate: new Date().toISOString().split('T')[0],
    endDate: '',
    reason: '',
  });

  const load = async () => {
    setLoading(true);
    try {
      const [a, r, d] = await Promise.all([
        api.get(`/api/Delivery/assignments?activeOnly=${activeOnly}`),
        api.get('/api/Admin/resellers'),
        api.get('/api/Admin/delivery-employees'),
      ]);
      setAssignments(a.data);
      setResellers(r.data);  // ← Show ALL resellers
      setDeliveryEmps(d.data);
      if (d.data.length > 0 && !form.deliveryEmployeeId) {
        setForm((f) => ({ ...f, deliveryEmployeeId: d.data[0].id }));
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [activeOnly]);

  // Get current permanent assignment for a reseller
  const getCurrentAssignment = (resellerId: string) => {
    return assignments.find(a => a.resellerEmployeeId === resellerId && a.type === 'PERMANENT' && !a.endDate);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await api.post('/api/Delivery/assign', {
        resellerEmployeeId: form.resellerEmployeeId,
        deliveryEmployeeId: form.deliveryEmployeeId,
        type: Number(form.type),
        startDate: form.startDate,
        endDate: form.endDate || null,
        reason: form.reason || null,
      });
      setMessage('Assignment created.');
      setShowModal(false);
      setForm({ ...form, resellerEmployeeId: '', reason: '', endDate: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to assign');
    }
  };

  const endAssignment = async (id: string) => {
    if (!confirm('End this assignment today? The reseller will be free to reassign.')) return;
    try {
      // Set EndDate to yesterday so it's immediately inactive
      const yesterday = new Date(Date.now() - 86400000).toISOString();
      await api.put(`/api/Delivery/assignment/${id}`, {
        endDate: yesterday,
        reason: 'Ended by admin',
      });
      setMessage('Assignment ended.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  // Filter resellers based on type (only for UX validation)
  const getAvailableResellers = () => {
    if (form.type === 1) {
      // TEMPORARY — show all resellers
      return resellers;
    }
    // PERMANENT — hide those with an active permanent assignment
    return resellers.filter(r => !getCurrentAssignment(r.id));
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">Delivery Assignments</h1>
          <p className="text-sm text-gray-500 mt-1">Assign resellers to delivery employees</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setActiveOnly(!activeOnly)}
            className="px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
          >
            {activeOnly ? 'Show All' : 'Show Active Only'}
          </button>
          <button
            onClick={load}
            className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
          >
            <RefreshCw size={14} /> Refresh
          </button>
          <button
            onClick={() => setShowModal(true)}
            disabled={deliveryEmps.length === 0}
            className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg"
          >
            <Plus size={14} /> Assign
          </button>
        </div>
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

      {deliveryEmps.length === 0 && (
        <div className="mb-4 p-3 bg-amber-50 text-amber-700 text-sm rounded-lg">
          No delivery employees yet. Create one first from <strong>Users &amp; Roles</strong>.
        </div>
      )}

      {loading ? <p className="text-gray-500">Loading...</p> : assignments.length === 0 ? (
        <div className="bg-white p-12 rounded-xl border text-center">
          <Truck className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No assignments yet</p>
          <p className="text-sm text-gray-500 mt-1">Click "Assign" to connect a reseller to a delivery employee.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Reseller</th>
                <th className="px-3 sm:px-4 py-3">Permanent</th>
                <th className="px-3 sm:px-4 py-3">Currently Delivering</th>
                <th className="px-3 sm:px-4 py-3">Type</th>
                <th className="px-3 sm:px-4 py-3">Since</th>
                <th className="px-3 sm:px-4 py-3">Ends</th>
                <th className="px-3 sm:px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {assignments.map((a) => (
                <tr key={a.id} className="hover:bg-gray-50">
                  <td className="px-3 sm:px-4 py-3">
                    <div className="font-medium">{a.resellerName}</div>
                    <div className="text-xs text-gray-500">{a.resellerCode}</div>
                  </td>
                  <td className="px-3 sm:px-4 py-3">
                    <div className="flex items-center gap-2">
                      <User size={14} className="text-gray-400" />
                      <div>
                        <div>{a.permanentDeliveryName}</div>
                        <div className="text-xs text-gray-500">{a.permanentDeliveryCode}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-3 sm:px-4 py-3">
                    {a.actualDeliveryName ? (
                      <div className="flex items-center gap-2">
                        <User size={14} className="text-blue-500" />
                        <div>
                          <div>{a.actualDeliveryName}</div>
                          <div className="text-xs text-gray-500">{a.actualDeliveryCode}</div>
                        </div>
                      </div>
                    ) : <span className="text-gray-400">—</span>}
                  </td>
                  <td className="px-3 sm:px-4 py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      a.type === 'PERMANENT' ? 'bg-blue-100 text-blue-800' : 'bg-orange-100 text-orange-800'
                    }`}>
                      {a.type}
                    </span>
                  </td>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {new Date(a.startDate).toLocaleDateString('en-ZA')}
                  </td>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {a.endDate ? new Date(a.endDate).toLocaleDateString('en-ZA') : <span className="text-green-600 font-medium">Active</span>}
                  </td>
                  <td className="px-3 sm:px-4 py-3 text-right">
                    {!a.endDate && (
                      <button
                        onClick={() => endAssignment(a.id)}
                        className="text-xs text-red-600 hover:text-red-800"
                      >
                        End
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Assign Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Assign Reseller</h2>
              <button onClick={() => setShowModal(false)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Assignment Type</label>
                <select
                  value={form.type}
                  onChange={(e) => setForm({ ...form, type: Number(e.target.value) })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
                >
                  <option value={0}>PERMANENT — Assign ownership</option>
                  <option value={1}>TEMPORARY — Cover for another driver</option>
                </select>
                {form.type === 1 && (
                  <p className="text-xs text-gray-500 mt-1">
                    Shows all resellers. Permanently-assigned resellers can receive temporary cover.
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Reseller <span className="text-red-500">*</span>
                </label>
                <select
                  required
                  value={form.resellerEmployeeId}
                  onChange={(e) => setForm({ ...form, resellerEmployeeId: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
                >
                  <option value="">— Select reseller —</option>
                  {getAvailableResellers().map((r) => {
                    const current = getCurrentAssignment(r.id);
                    return (
                      <option key={r.id} value={r.id}>
                        {r.code} · {r.name}
                        {current ? ` (currently: ${current.permanentDeliveryName})` : ''}
                        {r.residenceName ? ` · ${r.residenceName}` : ''}
                      </option>
                    );
                  })}
                </select>
                {form.type === 0 && getAvailableResellers().length === 0 && (
                  <p className="text-xs text-amber-600 mt-1">
                    All resellers are permanently assigned. Use TEMPORARY to cover a specific one.
                  </p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Delivery Employee <span className="text-red-500">*</span>
                </label>
                <select
                  required
                  value={form.deliveryEmployeeId}
                  onChange={(e) => setForm({ ...form, deliveryEmployeeId: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
                >
                  {deliveryEmps.map((d) => (
                    <option key={d.id} value={d.id}>{d.code} · {d.name}</option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Start Date</label>
                  <input
                    type="date"
                    required
                    value={form.startDate}
                    onChange={(e) => setForm({ ...form, startDate: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg text-sm"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">End Date (optional)</label>
                  <input
                    type="date"
                    value={form.endDate}
                    onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg text-sm"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Reason (optional)</label>
                <textarea
                  value={form.reason}
                  onChange={(e) => setForm({ ...form, reason: e.target.value })}
                  rows={2}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., Permanent assignment, or cover for sick driver"
                />
              </div>

              {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>}

              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => setShowModal(false)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
                >
                  Create Assignment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
