'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Plus, X, Building2 } from 'lucide-react';

interface Residence {
  id: string;
  name: string;
  address: string;
  estimatedPopulation: number;
  maxResellerCapacity: number;
}

export default function ResidencesPage() {
  const [residences, setResidences] = useState<Residence[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [message, setMessage] = useState('');
  const [form, setForm] = useState({ name: '', address: '', population: '', capacity: '' });
  const [error, setError] = useState('');
  const [creating, setCreating] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Admin/residences');
      setResidences(res.data);
    } catch (err) {
      setMessage('Failed to load residences');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async () => {
    if (!form.name || !form.address) {
      setError('Name and address are required');
      return;
    }
    setError('');
    setCreating(true);
    try {
      await api.post('/api/Residences', {
        name: form.name,
        address: form.address,
        estimatedPopulation: parseInt(form.population) || 0,
        maxResellerCapacity: parseInt(form.capacity) || 1,
      });
      setMessage(`Residence "${form.name}" created.`);
      setShowModal(false);
      setForm({ name: '', address: '', population: '', capacity: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || err.response?.data?.title || 'Failed to create residence');
    } finally {
      setCreating(false);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Residences</h1>
          <p className="text-sm text-gray-500 mt-1">Manage student residences and territories</p>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
            Refresh
          </button>
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
          >
            <Plus size={14} /> Add Residence
          </button>
        </div>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : residences.length === 0 ? (
        <div className="bg-white p-12 rounded-xl border text-center">
          <Building2 className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No residences yet</p>
          <p className="text-sm text-gray-500 mt-1 mb-4">
            Add a residence so resellers can be assigned to it.
          </p>
          <button
            onClick={() => setShowModal(true)}
            className="inline-flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
          >
            <Plus size={14} /> Add Your First Residence
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {residences.map((r) => (
            <div key={r.id} className="bg-white p-5 rounded-xl border">
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-2">
                  <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
                    <Building2 size={18} />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-800">{r.name}</h3>
                    <p className="text-xs text-gray-500">{r.address}</p>
                  </div>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3 mt-4 pt-4 border-t border-gray-100">
                <div>
                  <p className="text-xs text-gray-500 uppercase tracking-wide">Residents</p>
                  <p className="text-xl font-bold text-gray-800 mt-1">{r.estimatedPopulation}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase tracking-wide">Max Resellers</p>
                  <p className="text-xl font-bold text-gray-800 mt-1">{r.maxResellerCapacity}</p>
                </div>
              </div>
              <p className="text-xs text-gray-400 mt-3 font-mono">{r.id}</p>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Add Residence</h2>
              <button onClick={() => { setShowModal(false); setError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="space-y-3">
              <input
                placeholder="Residence name *"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                className="w-full px-3 py-2 border rounded-lg text-sm"
              />
              <input
                placeholder="Address *"
                value={form.address}
                onChange={(e) => setForm({ ...form, address: e.target.value })}
                className="w-full px-3 py-2 border rounded-lg text-sm"
              />
              <div className="grid grid-cols-2 gap-3">
                <input
                  type="number"
                  placeholder="Population"
                  value={form.population}
                  onChange={(e) => setForm({ ...form, population: e.target.value })}
                  className="px-3 py-2 border rounded-lg text-sm"
                />
                <input
                  type="number"
                  placeholder="Max resellers"
                  value={form.capacity}
                  onChange={(e) => setForm({ ...form, capacity: e.target.value })}
                  className="px-3 py-2 border rounded-lg text-sm"
                />
              </div>

              {error && (
                <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>
              )}

              <div className="flex justify-end gap-2 pt-2">
                <button
                  onClick={() => { setShowModal(false); setError(''); }}
                  className="px-4 py-2 text-sm bg-gray-100 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreate}
                  disabled={creating}
                  className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg"
                >
                  {creating ? 'Creating...' : 'Create Residence'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
