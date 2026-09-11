'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { UserPlus, Truck, CheckCircle, RefreshCw, Users, X } from 'lucide-react';

interface PendingUser {
  employeeId: string;
  personId: string;
  code: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  registeredAt: string;
}

interface Role {
  employeeId: string;
  code: string;
  role: string;
  residenceId: string | null;
}

interface Person {
  personId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  roles: Role[];
}

interface Residence {
  id: string;
  name: string;
  estimatedPopulation: number;
  maxResellerCapacity: number;
}

export default function UsersPage() {
  const [pending, setPending] = useState<PendingUser[]>([]);
  const [people, setPeople] = useState<Person[]>([]);
  const [residences, setResidences] = useState<Residence[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');

  // Modal state
  const [assignModal, setAssignModal] = useState<{ open: boolean; user: PendingUser | null; mode: 'reseller' | 'delivery' }>({
    open: false,
    user: null,
    mode: 'reseller',
  });
  const [selectedResidenceId, setSelectedResidenceId] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const [p, a, r] = await Promise.all([
        api.get('/api/admin/pending-users'),
        api.get('/api/admin/people'),
        api.get('/api/Admin/residences'),
      ]);
      setPending(p.data);
      setPeople(a.data);
      setResidences(r.data);
    } catch (err) {
      setMessage('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleAssignReseller = async () => {
    if (!assignModal.user || !selectedResidenceId) return;
    try {
      const res = await api.post(
        `/api/admin/people/${assignModal.user.personId}/assign-reseller`,
        { residenceId: selectedResidenceId }
      );
      setMessage(`${res.data.code} assigned to ${assignModal.user.firstName}`);
      setAssignModal({ open: false, user: null, mode: 'reseller' });
      setSelectedResidenceId('');
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  const handleAssignDelivery = async () => {
    if (!assignModal.user) return;
    try {
      const res = await api.post(
        `/api/admin/people/${assignModal.user.personId}/assign-delivery`
      );
      setMessage(`${res.data.code} (Delivery) assigned to ${assignModal.user.firstName}`);
      setAssignModal({ open: false, user: null, mode: 'delivery' });
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  const handleAddDeliveryToExisting = async (personId: string, firstName: string) => {
    if (!confirm(`Add Delivery role to ${firstName}?`)) return;
    try {
      const res = await api.post(`/api/admin/people/${personId}/assign-delivery`);
      setMessage(`${res.data.code} (Delivery) added to ${firstName}`);
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  const handleDeactivate = async (employeeId: string, code: string) => {
    if (!confirm(`Deactivate ${code}? The person will lose access to this role.`)) return;
    try {
      await api.post(`/api/admin/employees/${employeeId}/deactivate`);
      setMessage(`${code} deactivated`);
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Users & Roles</h1>
          <p className="text-sm text-gray-500 mt-1">Approve applicants and assign roles</p>
        </div>
        <button onClick={load} className="flex items-center gap-2 px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}

      {/* PENDING USERS */}
      <div className="mb-8">
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <UserPlus size={20} className="text-orange-600" />
          Pending Applicants ({pending.length})
        </h2>

        {loading ? (
          <p className="text-gray-500">Loading...</p>
        ) : pending.length === 0 ? (
          <div className="bg-white p-6 rounded-xl border text-center text-gray-500 text-sm">
            No pending applicants. New users will appear here after self-registering.
          </div>
        ) : (
          <div className="bg-white rounded-xl border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 text-left">
                <tr>
                  <th className="px-4 py-3">Code</th>
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">Email</th>
                  <th className="px-4 py-3">Phone</th>
                  <th className="px-4 py-3">Registered</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {pending.map((u) => (
                  <tr key={u.employeeId} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-mono text-xs">{u.code}</td>
                    <td className="px-4 py-3 font-medium">{u.firstName} {u.lastName}</td>
                    <td className="px-4 py-3">{u.email}</td>
                    <td className="px-4 py-3">{u.phone}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs">
                      {new Date(u.registeredAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2 justify-end">
                        <button
                          onClick={() => setAssignModal({ open: true, user: u, mode: 'reseller' })}
                          className="flex items-center gap-1 text-xs px-2 py-1 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded"
                        >
                          <CheckCircle size={12} /> As Reseller
                        </button>
                        <button
                          onClick={() => setAssignModal({ open: true, user: u, mode: 'delivery' })}
                          className="flex items-center gap-1 text-xs px-2 py-1 bg-green-50 text-green-700 hover:bg-green-100 rounded"
                        >
                          <Truck size={12} /> As Delivery
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* ALL PEOPLE */}
      <div>
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <Users size={20} className="text-blue-600" />
          All Users with Roles
        </h2>

        {loading ? (
          <p className="text-gray-500">Loading...</p>
        ) : (
          <div className="bg-white rounded-xl border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 text-left">
                <tr>
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">Email</th>
                  <th className="px-4 py-3">Roles</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {people.map((p) => {
                  const hasReseller = p.roles.some(r => r.role === 'Reseller');
                  const hasDelivery = p.roles.some(r => r.role === 'Delivery');
                  return (
                    <tr key={p.personId} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium">{p.firstName} {p.lastName}</td>
                      <td className="px-4 py-3 text-gray-600">{p.email}</td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap gap-1">
                          {p.roles.length === 0 && <span className="text-xs text-gray-400">No active roles</span>}
                          {p.roles.map((r) => (
                            <span
                              key={r.employeeId}
                              className={`px-2 py-1 rounded text-xs font-medium ${
                                r.role === 'Admin' ? 'bg-purple-100 text-purple-800' :
                                r.role === 'Reseller' ? 'bg-blue-100 text-blue-800' :
                                'bg-green-100 text-green-800'
                              }`}
                            >
                              {r.code}
                            </span>
                          ))}
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2 justify-end">
                          {!hasDelivery && p.roles.length > 0 && (
                            <button
                              onClick={() => handleAddDeliveryToExisting(p.personId, p.firstName)}
                              className="flex items-center gap-1 text-xs px-2 py-1 bg-green-50 text-green-700 hover:bg-green-100 rounded"
                            >
                              <Truck size={12} /> + Delivery
                            </button>
                          )}
                          {p.roles.map((r) => (
                            r.role !== 'Admin' && (
                              <button
                                key={r.employeeId}
                                onClick={() => handleDeactivate(r.employeeId, r.code)}
                                className="text-xs px-2 py-1 bg-red-50 text-red-700 hover:bg-red-100 rounded"
                              >
                                Deactivate {r.code}
                              </button>
                            )
                          ))}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* ASSIGN MODAL */}
      {assignModal.open && assignModal.user && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <h2 className="text-lg font-semibold mb-2">
              {assignModal.mode === 'reseller' ? 'Approve as Reseller' : 'Add Delivery Role'}
            </h2>
            <p className="text-sm text-gray-600 mb-4">
              {assignModal.user.firstName} {assignModal.user.lastName} ({assignModal.user.email})
            </p>

            {assignModal.mode === 'reseller' && (
              <div className="mb-4">
                <label className="block text-sm font-medium mb-1">Residence</label>
                <select
                  value={selectedResidenceId}
                  onChange={(e) => setSelectedResidenceId(e.target.value)}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
                >
                  <option value="">— Select residence —</option>
                  {residences.map((r) => (
                    <option key={r.id} value={r.id}>
                      {r.name} ({r.estimatedPopulation} pop, max {r.maxResellerCapacity})
                    </option>
                  ))}
                </select>
                {residences.length === 0 && (
                  <p className="text-xs text-amber-600 mt-2">No residences available. Create one first.</p>
                )}
              </div>
            )}

            {assignModal.mode === 'delivery' && (
              <p className="text-sm text-gray-500 mb-4">
                This will create a DEL-XXXX employee ID for this person. They'll be able to receive delivery assignments.
              </p>
            )}

            <div className="flex justify-end gap-2 pt-2">
              <button
                onClick={() => { setAssignModal({ open: false, user: null, mode: 'reseller' }); setSelectedResidenceId(''); }}
                className="px-4 py-2 text-sm bg-gray-100 rounded-lg"
              >
                Cancel
              </button>
              <button
                onClick={assignModal.mode === 'reseller' ? handleAssignReseller : handleAssignDelivery}
                disabled={assignModal.mode === 'reseller' && !selectedResidenceId}
                className={`px-4 py-2 text-sm text-white rounded-lg disabled:opacity-50 ${
                  assignModal.mode === 'reseller' ? 'bg-blue-600 hover:bg-blue-700' : 'bg-green-600 hover:bg-green-700'
                }`}
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
