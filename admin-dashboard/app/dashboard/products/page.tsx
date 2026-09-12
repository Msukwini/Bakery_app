'use client';

import { useEffect, useState, useRef } from 'react';
import { api } from '@/lib/api';
import { Package, Plus, X, RefreshCw, Camera, CirclePlus, Trash2 } from 'lucide-react';

interface Variant {
  id: string;
  sizeName: string;
  unitPrice: number;
  isActive: boolean;
}

interface Product {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  hasImage: boolean;
  variants: Variant[];
}

export default function AdminProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState<string | null>(null);
  const fileInputs = useRef<Record<string, HTMLInputElement | null>>({});

  const [form, setForm] = useState({
    name: '',
    description: '',
    variants: [{ sizeName: '', unitPrice: '' }],
  });

  const [addVariantTo, setAddVariantTo] = useState<Product | null>(null);
  const [variantForm, setVariantForm] = useState({ sizeName: '', unitPrice: '' });

  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/admin/products');
      setProducts(res.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const cleanVariants = form.variants
        .filter((v) => v.sizeName.trim() && parseFloat(v.unitPrice) > 0)
        .map((v) => ({ sizeName: v.sizeName.trim(), unitPrice: parseFloat(v.unitPrice) }));

      if (cleanVariants.length === 0) {
        setError('Add at least one variant with a price.');
        return;
      }

      await api.post('/api/admin/products', {
        name: form.name,
        description: form.description,
        variants: cleanVariants,
      });

      setMessage('Product created.');
      setShowModal(false);
      setForm({ name: '', description: '', variants: [{ sizeName: '', unitPrice: '' }] });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create');
    }
  };

  const addVariantRow = () => {
    setForm({ ...form, variants: [...form.variants, { sizeName: '', unitPrice: '' }] });
  };

  const removeVariantRow = (i: number) => {
    setForm({ ...form, variants: form.variants.filter((_, idx) => idx !== i) });
  };

  const updateVariant = (i: number, field: 'sizeName' | 'unitPrice', value: string) => {
    setForm({
      ...form,
      variants: form.variants.map((v, idx) => idx === i ? { ...v, [field]: value } : v),
    });
  };

  const handleUploadPicture = async (productId: string, file: File) => {
    setUploading(productId);
    setError('');
    try {
      const fd = new FormData();
      fd.append('file', file);
      await api.post(`/api/admin/products/${productId}/picture`, fd, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setMessage('Picture uploaded.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Upload failed');
    } finally {
      setUploading(null);
      if (fileInputs.current[productId]) fileInputs.current[productId]!.value = '';
    }
  };

  const handleAddVariant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!addVariantTo) return;
    try {
      await api.post(`/api/admin/products/${addVariantTo.id}/variants`, {
        sizeName: variantForm.sizeName,
        unitPrice: parseFloat(variantForm.unitPrice),
      });
      setMessage('Variant added.');
      setAddVariantTo(null);
      setVariantForm({ sizeName: '', unitPrice: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const deactivateVariant = async (variantId: string) => {
    if (!confirm('Deactivate this variant?')) return;
    try {
      await api.delete(`/api/admin/products/variants/${variantId}`);
      setMessage('Variant deactivated.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const toggleProductActive = async (product: Product) => {
    try {
      await api.put(`/api/admin/products/${product.id}`, {
        isActive: !product.isActive,
      });
      setMessage(`Product ${product.isActive ? 'deactivated' : 'activated'}.`);
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">Products</h1>
          <p className="text-sm text-gray-500 mt-1">Manage products shown on the homepage</p>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
            <RefreshCw size={14} /> Refresh
          </button>
          <button onClick={() => setShowModal(true)} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
            <Plus size={14} /> New Product
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
        <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg flex justify-between items-center">
          <span>{error}</span>
          <button onClick={() => setError('')}><X size={16} /></button>
        </div>
      )}

      {loading ? <p className="text-gray-500">Loading...</p> : products.length === 0 ? (
        <div className="bg-white p-12 rounded-xl border text-center">
          <Package className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No products yet</p>
          <button onClick={() => setShowModal(true)} className="mt-4 inline-flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
            <Plus size={14} /> Add Your First Product
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {products.map((p) => (
            <div key={p.id} className={`bg-white rounded-xl border overflow-hidden ${!p.isActive ? 'opacity-60' : ''}`}>
              <div className="relative h-40 bg-gradient-to-br from-amber-100 to-orange-100 flex items-center justify-center">
                {p.hasImage ? (
                  <img
                    src={`${apiUrl}/api/products/picture/${p.id}`}
                    alt={p.name}
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <Package size={48} className="text-amber-500" />
                )}
                <button
                  onClick={() => fileInputs.current[p.id]?.click()}
                  disabled={uploading === p.id}
                  className="absolute bottom-2 right-2 p-2 bg-white/90 hover:bg-white text-gray-700 rounded-full shadow-md"
                >
                  <Camera size={16} />
                </button>
                <input
                  ref={(el) => { fileInputs.current[p.id] = el; }}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  className="hidden"
                  onChange={(e) => {
                    const f = e.target.files?.[0];
                    if (f) handleUploadPicture(p.id, f);
                  }}
                />
                {uploading === p.id && (
                  <div className="absolute inset-0 bg-black/40 flex items-center justify-center text-white text-xs">
                    Uploading...
                  </div>
                )}
              </div>

              <div className="p-4">
                <div className="flex items-start justify-between gap-2 mb-2">
                  <h3 className="font-semibold text-gray-800">{p.name}</h3>
                  {!p.isActive && <span className="text-[10px] bg-gray-100 text-gray-600 px-2 py-0.5 rounded">INACTIVE</span>}
                </div>
                {p.description && (
                  <p className="text-xs text-gray-500 mb-3 line-clamp-2">{p.description}</p>
                )}

                <div className="space-y-1 mb-3">
                  {p.variants.filter(v => v.isActive).map((v) => (
                    <div key={v.id} className="flex items-center justify-between text-sm">
                      <span className="text-gray-600">{v.sizeName}</span>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">R {v.unitPrice.toFixed(2)}</span>
                        <button
                          onClick={() => deactivateVariant(v.id)}
                          className="text-gray-300 hover:text-red-600"
                          title="Deactivate variant"
                        >
                          <Trash2 size={12} />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="flex gap-2 pt-3 border-t border-gray-100">
                  <button
                    onClick={() => { setAddVariantTo(p); setVariantForm({ sizeName: '', unitPrice: '' }); }}
                    className="flex-1 flex items-center justify-center gap-1 text-xs py-2 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded-lg"
                  >
                    <CirclePlus size={12} /> Add Size
                  </button>
                  <button
                    onClick={() => toggleProductActive(p)}
                    className="flex-1 text-xs py-2 bg-gray-50 hover:bg-gray-100 text-gray-700 rounded-lg"
                  >
                    {p.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">New Product</h2>
              <button onClick={() => { setShowModal(false); setError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Product Name <span className="text-red-500">*</span></label>
                <input
                  required
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., Chocolate Cake"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Description</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  rows={2}
                  placeholder="Brief description for the homepage"
                />
              </div>

              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="block text-sm font-medium">Sizes / Variants <span className="text-red-500">*</span></label>
                  <button type="button" onClick={addVariantRow} className="text-xs text-blue-600 hover:text-blue-800">
                    + Add another size
                  </button>
                </div>
                {form.variants.map((v, i) => (
                  <div key={i} className="flex gap-2 mb-2">
                    <input
                      placeholder="Size name (e.g., 5L bucket)"
                      value={v.sizeName}
                      onChange={(e) => updateVariant(i, 'sizeName', e.target.value)}
                      className="flex-1 px-3 py-2 border rounded-lg text-sm"
                    />
                    <input
                      type="number"
                      step="0.01"
                      placeholder="Price"
                      value={v.unitPrice}
                      onChange={(e) => updateVariant(i, 'unitPrice', e.target.value)}
                      className="w-24 px-3 py-2 border rounded-lg text-sm"
                    />
                    {form.variants.length > 1 && (
                      <button type="button" onClick={() => removeVariantRow(i)} className="text-red-500 hover:text-red-700">
                        <X size={16} />
                      </button>
                    )}
                  </div>
                ))}
              </div>

              {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>}

              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => { setShowModal(false); setError(''); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button type="submit" className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Create Product
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {addVariantTo && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Add Size to {addVariantTo.name}</h2>
              <button onClick={() => setAddVariantTo(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <form onSubmit={handleAddVariant} className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Size Name</label>
                <input
                  required
                  value={variantForm.sizeName}
                  onChange={(e) => setVariantForm({ ...variantForm, sizeName: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., 10L bucket"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Unit Price</label>
                <input
                  required
                  type="number"
                  step="0.01"
                  value={variantForm.unitPrice}
                  onChange={(e) => setVariantForm({ ...variantForm, unitPrice: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., 6.00"
                />
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => setAddVariantTo(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button type="submit" className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Add Size
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
