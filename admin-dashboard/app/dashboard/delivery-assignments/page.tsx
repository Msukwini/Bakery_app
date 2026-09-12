'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Truck, Plus, X, RefreshCw, User, Users } from 'lucide-react';
import ProfilePic from '@/components/ProfilePic';

interface Assignment {
  id: string;
  resellerEmployeeId: string;
  resellerCode: string;
  resellerName: string;
  resellerPersonId: string | null;
  resellerHasPicture: boolean;
  permanentDeliveryEmployeeId: string;
  permanentDeliveryCode: string;
  permanentDeliveryName: string;
  permanentPersonId: string | null;
  permanentHasPicture: boolean;
  actualDeliveryEmployeeId: string | null;
  actualDeliveryCode: string | null;
  actualDeliveryName: string | null;
  actualPersonId: string | null;
  actualHasPicture: boolean;
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
      setResellers(r.data);
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

  const getAvailableResellers = () => {
    if (form.type === 1) return resellers;
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
        <div className="space-y-3">
          {assignments.map((a) => (
            <div key={a.id} className="bg-white rounded-xl border p-4">
              <div className="flex flex-col sm:flex-row sm:items-center gap-3">
                {/* Reseller */}
                <div className="flex items-center gap-3 flex-1 min-w-0">
                  {a.resellerPersonId ? (
                    <ProfilePic
                      personId={a.resellerPersonId}
                      hasPicture={a.resellerHasPicture}
                      firstName={a.resellerName.split(' ')[0]}
                      lastName={a.resellerName.split(' ')[1]}
                      size={48}
                    />
                  ) : (
                    <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center">
                      <User size={20} className="text-gray-400" />
                    </div>
                  )}
                  <div className="min-w-0">
                    <div className="font-medium text-gray-800 truncate">{a.resellerName}</div>
                    <div className="text-xs text-gray-500 font-mono">{a.resellerCode}</div>
                  </div>
                </div>

                {/* Arrow / Type */}
                <div className="flex items-center gap-2 sm:flex-col">
                  <div className={`px-2 py-1 rounded text-[10px] font-semibold ${
                    a.type === 'PERMANENT' ? 'bg-blue-100 text-blue-800' : 'bg-orange-100 text-orange-800'
                  }`}>
                    {a.type}
                  </div>
                  <span className="text-gray-400 text-lg">→</span>
                </div>

                {/* Permanent driver */}
                <div className="flex items-center gap-3 flex-1 min-w-0">
                  {a.permanentPersonId ? (
                    <ProfilePic
                      personId={a.permanentPersonId}
                      hasPicture={a.permanentHasPicture}
                      firstName={a.permanentDeliveryName.split(' ')[0]}
                      lastName={a.permanentDeliveryName.split(' ')[1]}
                      size={48}
                    />
                  ) : (
                    <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center">
                      <Truck size={20} className="text-gray-400" />
                    </div>
                  )}
                  <div className="min-w-0">
                    <div className="text-xs text-gray-500 mb-0.5">Permanent</div>
                    <div className="font-medium text-gray-800 truncate text-sm">{a.permanentDeliveryName}</div>
                    <div className="text-xs text-gray-500 font-mono">{a.permanentDeliveryCode}</div>
                  </div>
                </div>

                {/* Actual driver (if different) */}
                {a.actualDeliveryEmployeeId && a.actualDeliveryEmployeeId !== a.permanentDeliveryEmployeeId && (
                  <>
                    <span className="text-gray-400 text-lg hidden sm:block">→</span>
                    <div className="flex items-center gap-3 flex-1 min-w-0">
                      {a.actualPersonId ? (
                        <ProfilePic
                          personId={a.actualPersonId}
                          hasPicture={a.actualHasPicture}
                          firstName={a.actualDeliveryName?.split(' ')[0]}
                          lastName={a.actualDeliveryName?.split(' ')[1]}
                          size={48}
                        />
                      ) : (
                        <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center">
                          <Truck size={20} className="text-gray-400" />
                        </div>
                      )}
                      <div className="min-w-0">
                        <div className="text-xs text-orange-500 mb-0.5 font-medium">Covering</div>
                        <div className="font-medium text-gray-800 truncate text-sm">{a.actualDeliveryName}</div>
                        <div className="text-xs text-gray-500 font-mono">{a.actualDeliveryCode}</div>
                      </div>
                    </div>
                  </>
                )}

                {/* Actions */}
                <div className="flex flex-col items-end gap-2 sm:ml-4 flex-shrink-0">
                  <div className="text-xs text-gray-500">
                    Since {new Date(a.startDate).toLocaleDateString('en-ZA')}
                  </div>
                  {a.endDate ? (
                    <div className="text-xs text-gray-400">
                      Ended {new Date(a.endDate).toLocaleDateString('en-ZA')}
                    </div>
                  ) : (
                    <>
                      <span className="text-xs text-green-600 font-medium">Active</span>
                      <button
                        onClick={() => endAssignment(a.id)}
                        className="text-xs text-red-600 hover:text-red-800"
                      >
                        End
                      </button>
                    </>
                  )}
                </div>
              </div>

              {a.reason && (
                <div className="mt-3 pt-3 border-t border-gray-100 text-xs text-gray-500">
                  {a.reason}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Assign Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
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
