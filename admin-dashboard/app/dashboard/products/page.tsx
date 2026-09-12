'use client';

import { useEffect, useState, useRef } from 'react';
import { api } from '@/lib/api';
import { Package, Plus, X, RefreshCw, Camera, CirclePlus, Trash2, Pencil } from 'lucide-react';

interface Variant {
  id: string;
  sizeName: string;
  unitPrice: number;
  guestPrice: number | null;
  stockCost: number | null;
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
    variants: [{ sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' }],
  });

  const [addVariantTo, setAddVariantTo] = useState<Product | null>(null);
  const [variantForm, setVariantForm] = useState({ sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' });

  const [editing, setEditing] = useState<Variant | null>(null);
  const [editForm, setEditForm] = useState({ unitPrice: '', guestPrice: '', stockCost: '' });

  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/admin/variants');
      // Group by product
      const productMap = new Map<string, any>();
      for (const v of res.data) {
        if (!productMap.has(v.productId)) {
          productMap.set(v.productId, {
            id: v.productId,
            name: v.productName,
            description: '',
            isActive: v.isActive,
            hasImage: false,
            variants: [],
          });
        }
        productMap.get(v.productId).variants.push({
          id: v.id,
          sizeName: v.sizeName,
          unitPrice: v.unitPrice,
          guestPrice: v.guestPrice,
          stockCost: v.stockCost,
          isActive: v.isActive,
        });
      }
      setProducts(Array.from(productMap.values()));
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
        .map((v) => ({
          sizeName: v.sizeName.trim(),
          unitPrice: parseFloat(v.unitPrice),
          guestPrice: v.guestPrice ? parseFloat(v.guestPrice) : null,
          stockCost: v.stockCost ? parseFloat(v.stockCost) : null,
        }));

      if (cleanVariants.length === 0) {
        setError('Add at least one variant with a price.');
        return;
      }

      // Create product
      const res = await api.post('/api/admin/products', {
        name: form.name,
        description: form.description,
        variants: cleanVariants.map(v => ({ sizeName: v.sizeName, unitPrice: v.unitPrice })),
      });

      // Then update pricing for each variant
      const variantList = await api.get('/api/admin/variants');
      const newProduct = variantList.data.filter((v: any) => v.productId === res.data.id);

      for (let i = 0; i < newProduct.length && i < cleanVariants.length; i++) {
        const v = cleanVariants[i];
        await api.put(`/api/admin/variants/${newProduct[i].id}/pricing`, {
          guestPrice: v.guestPrice,
          stockCost: v.stockCost,
        });
      }

      setMessage('Product created.');
      setShowModal(false);
      setForm({ name: '', description: '', variants: [{ sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' }] });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create');
    }
  };

  const addVariantRow = () => {
    setForm({ ...form, variants: [...form.variants, { sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' }] });
  };

  const removeVariantRow = (i: number) => {
    setForm({ ...form, variants: form.variants.filter((_, idx) => idx !== i) });
  };

  const updateVariant = (i: number, field: 'sizeName' | 'unitPrice' | 'guestPrice' | 'stockCost', value: string) => {
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
      const res = await api.post(`/api/admin/products/${addVariantTo.id}/variants`, {
        sizeName: variantForm.sizeName,
        unitPrice: parseFloat(variantForm.unitPrice),
      });
      // Then update pricing
      await api.put(`/api/admin/variants/${res.data.id}/pricing`, {
        guestPrice: variantForm.guestPrice ? parseFloat(variantForm.guestPrice) : null,
        stockCost: variantForm.stockCost ? parseFloat(variantForm.stockCost) : null,
      });
      setMessage('Variant added.');
      setAddVariantTo(null);
      setVariantForm({ sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const handleUpdatePricing = async () => {
    if (!editing) return;
    try {
      await api.put(`/api/admin/variants/${editing.id}/pricing`, {
        unitPrice: parseFloat(editForm.unitPrice) || editing.unitPrice,
        guestPrice: editForm.guestPrice ? parseFloat(editForm.guestPrice) : null,
        stockCost: editForm.stockCost ? parseFloat(editForm.stockCost) : null,
      });
      setMessage('Pricing updated.');
      setEditing(null);
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
      await api.put(`/api/admin/products/${product.id}`, { isActive: !product.isActive });
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
          <p className="text-sm text-gray-500 mt-1">Manage products, pricing, and cost</p>
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
                  <img src={`${apiUrl}/api/products/picture/${p.id}`} alt={p.name} className="w-full h-full object-cover" />
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
              </div>

              <div className="p-4">
                <h3 className="font-semibold text-gray-800 mb-3">{p.name}</h3>

                <div className="space-y-2 mb-3">
                  {p.variants.filter(v => v.isActive).map((v) => (
                    <div key={v.id} className="border border-gray-100 rounded-lg p-2.5">
                      <div className="flex items-center justify-between mb-1.5">
                        <span className="font-medium text-sm text-gray-700">{v.sizeName}</span>
                        <div className="flex items-center gap-1">
                          <button
                            onClick={() => {
                              setEditing(v);
                              setEditForm({
                                unitPrice: String(v.unitPrice),
                                guestPrice: v.guestPrice != null ? String(v.guestPrice) : '',
                                stockCost: v.stockCost != null ? String(v.stockCost) : '',
                              });
                            }}
                            className="text-gray-400 hover:text-blue-600"
                            title="Edit pricing"
                          >
                            <Pencil size={12} />
                          </button>
                          <button onClick={() => deactivateVariant(v.id)} className="text-gray-300 hover:text-red-600" title="Deactivate">
                            <Trash2 size={12} />
                          </button>
                        </div>
                      </div>
                      <div className="grid grid-cols-3 gap-1 text-[10px]">
                        <div>
                          <p className="text-gray-400 uppercase">Reseller</p>
                          <p className="font-medium text-gray-700">R {v.unitPrice.toFixed(2)}</p>
                        </div>
                        <div>
                          <p className="text-gray-400 uppercase">Guest</p>
                          <p className="font-medium text-amber-700">
                            {v.guestPrice != null ? `R ${v.guestPrice.toFixed(2)}` : <span className="text-gray-400">—</span>}
                          </p>
                        </div>
                        <div>
                          <p className="text-gray-400 uppercase">Cost</p>
                          <p className="font-medium text-gray-500">
                            {v.stockCost != null ? `R ${v.stockCost.toFixed(2)}` : <span className="text-gray-400">—</span>}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="flex gap-2 pt-3 border-t border-gray-100">
                  <button
                    onClick={() => { setAddVariantTo(p); setVariantForm({ sizeName: '', unitPrice: '', guestPrice: '', stockCost: '' }); }}
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

      {/* Edit Pricing Modal */}
      {editing && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Edit Pricing — {editing.sizeName}</h2>
              <button onClick={() => setEditing(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Reseller Price (R)</label>
                <input type="number" step="0.01" value={editForm.unitPrice}
                  onChange={(e) => setEditForm({ ...editForm, unitPrice: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" />
                <p className="text-xs text-gray-500 mt-1">Price resellers pay</p>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Guest Price (R)</label>
                <input type="number" step="0.01" placeholder="Leave empty to use reseller price"
                  value={editForm.guestPrice}
                  onChange={(e) => setEditForm({ ...editForm, guestPrice: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" />
                <p className="text-xs text-gray-500 mt-1">Price direct customers pay on the homepage</p>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Stock Cost (R)</label>
                <input type="number" step="0.01" placeholder="How much it costs to produce 1 unit"
                  value={editForm.stockCost}
                  onChange={(e) => setEditForm({ ...editForm, stockCost: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" />
                <p className="text-xs text-gray-500 mt-1">Used for profit calculations</p>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => setEditing(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button onClick={handleUpdatePricing} className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Save
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Create Product Modal */}
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
                <input required value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., Chocolate Cake" />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Description</label>
                <textarea value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" rows={2} />
              </div>
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="block text-sm font-medium">Sizes <span className="text-red-500">*</span></label>
                  <button type="button" onClick={addVariantRow} className="text-xs text-blue-600 hover:text-blue-800">
                    + Add another size
                  </button>
                </div>
                {form.variants.map((v, i) => (
                  <div key={i} className="bg-gray-50 p-3 rounded-lg mb-2 space-y-2">
                    <div className="flex gap-2">
                      <input placeholder="Size name" value={v.sizeName}
                        onChange={(e) => updateVariant(i, 'sizeName', e.target.value)}
                        className="flex-1 px-3 py-2 border rounded-lg text-sm bg-white" />
                      {form.variants.length > 1 && (
                        <button type="button" onClick={() => removeVariantRow(i)} className="text-red-500 hover:text-red-700">
                          <X size={16} />
                        </button>
                      )}
                    </div>
                    <div className="grid grid-cols-3 gap-2">
                      <div>
                        <label className="text-[10px] text-gray-500 uppercase">Reseller R</label>
                        <input type="number" step="0.01" placeholder="e.g., 6.00"
                          value={v.unitPrice}
                          onChange={(e) => updateVariant(i, 'unitPrice', e.target.value)}
                          className="w-full px-2 py-1.5 border rounded-lg text-sm bg-white" />
                      </div>
                      <div>
                        <label className="text-[10px] text-gray-500 uppercase">Guest R</label>
                        <input type="number" step="0.01" placeholder="Optional"
                          value={v.guestPrice}
                          onChange={(e) => updateVariant(i, 'guestPrice', e.target.value)}
                          className="w-full px-2 py-1.5 border rounded-lg text-sm bg-white" />
                      </div>
                      <div>
                        <label className="text-[10px] text-gray-500 uppercase">Cost R</label>
                        <input type="number" step="0.01" placeholder="Optional"
                          value={v.stockCost}
                          onChange={(e) => updateVariant(i, 'stockCost', e.target.value)}
                          className="w-full px-2 py-1.5 border rounded-lg text-sm bg-white" />
                      </div>
                    </div>
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

      {/* Add Variant Modal */}
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
                <input required value={variantForm.sizeName}
                  onChange={(e) => setVariantForm({ ...variantForm, sizeName: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm" />
              </div>
              <div className="grid grid-cols-3 gap-2">
                <div>
                  <label className="block text-xs font-medium mb-1">Reseller R</label>
                  <input required type="number" step="0.01" value={variantForm.unitPrice}
                    onChange={(e) => setVariantForm({ ...variantForm, unitPrice: e.target.value })}
                    className="w-full px-2 py-2 border rounded-lg text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium mb-1">Guest R</label>
                  <input type="number" step="0.01" value={variantForm.guestPrice}
                    onChange={(e) => setVariantForm({ ...variantForm, guestPrice: e.target.value })}
                    className="w-full px-2 py-2 border rounded-lg text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium mb-1">Cost R</label>
                  <input type="number" step="0.01" value={variantForm.stockCost}
                    onChange={(e) => setVariantForm({ ...variantForm, stockCost: e.target.value })}
                    className="w-full px-2 py-2 border rounded-lg text-sm" />
                </div>
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
