'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Croissant, Plus, Minus, ShoppingCart, X, CircleCheck } from 'lucide-react';

interface ProductVariant {
  id: string;
  productName: string;
  sizeName: string;
  unitPrice: number;
}

interface CartItem {
  variantId: string;
  productName: string;
  sizeName: string;
  unitPrice: number;
  quantity: number;
}

export default function GuestOrderPage() {
  const [products, setProducts] = useState<ProductVariant[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [orderNumber, setOrderNumber] = useState('');
  const [error, setError] = useState('');

  const [customer, setCustomer] = useState({
    customerName: '',
    customerPhone: '',
    customerEmail: '',
    deliveryAddress: '',
    requiredDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
    paymentMethod: 'Cash',
  });

  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  useEffect(() => {
    fetch(`${apiUrl}/api/Products/variants`)
      .then((r) => r.json())
      .then((data) => setProducts(data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [apiUrl]);

  const addToCart = (p: ProductVariant) => {
    setCart((prev) => {
      const existing = prev.find((i) => i.variantId === p.id);
      if (existing) {
        return prev.map((i) => i.variantId === p.id ? { ...i, quantity: i.quantity + 1 } : i);
      }
      return [...prev, {
        variantId: p.id,
        productName: p.productName,
        sizeName: p.sizeName,
        unitPrice: p.unitPrice,
        quantity: 1,
      }];
    });
  };

  const updateQty = (variantId: string, delta: number) => {
    setCart((prev) =>
      prev
        .map((i) => i.variantId === variantId ? { ...i, quantity: i.quantity + delta } : i)
        .filter((i) => i.quantity > 0)
    );
  };

  const total = cart.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (cart.length === 0) { setError('Your cart is empty.'); return; }
    setError('');
    setSubmitting(true);

    try {
      const res = await fetch(`${apiUrl}/api/Orders`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...customer,
          requiredDate: new Date(customer.requiredDate).toISOString(),
          items: cart.map((i) => ({
            productVariantId: i.variantId,
            quantity: i.quantity,
            unitPrice: i.unitPrice,
          })),
        }),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.error || 'Order failed');
      }

      const data = await res.json();
      setOrderNumber(data.orderNumber);
      setSubmitted(true);
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setSubmitting(false);
    }
  };

  if (submitted) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-amber-50 to-orange-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-lg p-8 max-w-md text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <CircleCheck className="w-8 h-8 text-green-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2">Order Placed!</h1>
          <p className="text-gray-600 mb-4 text-sm">
            Your order <strong className="text-amber-600">{orderNumber}</strong> has been received.
          </p>
          <p className="text-sm text-gray-500 mb-6">
            We'll contact you at <strong>{customer.customerPhone}</strong> to confirm delivery.
          </p>
          <Link href="/" className="inline-block px-6 py-2.5 bg-amber-600 hover:bg-amber-700 text-white font-medium rounded-lg">
            Back to Home
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-100">
        <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2">
            <img src="/logo-v2.png" alt="Ndlovu Bakery" className="w-8 h-8 object-contain" />
            <span className="font-bold text-lg text-gray-800">Ndlovu Bakery</span>
          </Link>
          <div className="flex items-center gap-2">
            <ShoppingCart size={20} className="text-amber-600" />
            <span className="text-sm text-gray-600">{cart.length} item(s)</span>
          </div>
        </div>
      </header>

      <div className="max-w-6xl mx-auto p-4 sm:p-6 lg:p-8">
        <h1 className="text-2xl sm:text-3xl font-bold text-gray-800 mb-6">Order as Guest</h1>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Products */}
          <div className="lg:col-span-2">
            <h2 className="font-semibold text-gray-700 mb-3">Choose Products</h2>

            {loading ? <p className="text-gray-400">Loading...</p> : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {products.map((p) => (
                  <div key={p.id} className="bg-white border border-gray-100 rounded-xl p-4 flex items-center justify-between gap-3">
                    <div className="min-w-0 flex-1">
                      <p className="font-medium text-gray-800 truncate">{p.productName}</p>
                      <p className="text-xs text-gray-500">{p.sizeName}</p>
                      <p className="text-sm font-semibold text-amber-600 mt-1">R {p.unitPrice.toFixed(2)}</p>
                    </div>
                    <button
                      onClick={() => addToCart(p)}
                      className="flex items-center justify-center w-8 h-8 bg-amber-100 hover:bg-amber-200 text-amber-700 rounded-full flex-shrink-0"
                    >
                      <Plus size={16} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Cart & Checkout */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl border border-gray-100 p-5 sticky top-4">
              <h2 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
                <ShoppingCart size={18} /> Your Cart
              </h2>

              {cart.length === 0 ? (
                <p className="text-sm text-gray-400 py-4 text-center">Cart is empty. Add items to continue.</p>
              ) : (
                <div className="space-y-3 mb-4 max-h-64 overflow-y-auto">
                  {cart.map((item) => (
                    <div key={item.variantId} className="flex items-start justify-between gap-2 text-sm">
                      <div className="min-w-0 flex-1">
                        <p className="font-medium text-gray-800 truncate">{item.productName}</p>
                        <p className="text-xs text-gray-500">{item.sizeName}</p>
                        <p className="text-xs text-amber-600 font-medium mt-1">R {(item.unitPrice * item.quantity).toFixed(2)}</p>
                      </div>
                      <div className="flex items-center gap-1 flex-shrink-0">
                        <button onClick={() => updateQty(item.variantId, -1)} className="w-6 h-6 flex items-center justify-center bg-gray-100 rounded-full hover:bg-gray-200">
                          <Minus size={12} />
                        </button>
                        <span className="w-6 text-center font-medium">{item.quantity}</span>
                        <button onClick={() => updateQty(item.variantId, 1)} className="w-6 h-6 flex items-center justify-center bg-gray-100 rounded-full hover:bg-gray-200">
                          <Plus size={12} />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {cart.length > 0 && (
                <div className="pt-3 border-t border-gray-100 mb-4">
                  <div className="flex justify-between items-center">
                    <span className="text-gray-600">Total</span>
                    <span className="text-xl font-bold text-gray-800">R {total.toFixed(2)}</span>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Checkout Form */}
        {cart.length > 0 && (
          <div className="mt-8 bg-white rounded-xl border border-gray-100 p-5 sm:p-6">
            <h2 className="font-semibold text-gray-800 mb-4">Your Details</h2>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Field label="Full Name" required value={customer.customerName} onChange={(v: string) => setCustomer({ ...customer, customerName: v })} />
                <Field label="Phone Number" required value={customer.customerPhone} onChange={(v: string) => setCustomer({ ...customer, customerPhone: v })} />
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Field label="Email (optional)" type="email" value={customer.customerEmail} onChange={(v: string) => setCustomer({ ...customer, customerEmail: v })} />
                <Field label="Payment Method" value={customer.paymentMethod} onChange={(v: string) => setCustomer({ ...customer, paymentMethod: v })} placeholder="Cash / EFT" />
              </div>
              <Field label="Delivery Address" required value={customer.deliveryAddress} onChange={(v: string) => setCustomer({ ...customer, deliveryAddress: v })} />
              <Field label="Required By" required type="date" value={customer.requiredDate} onChange={(v: string) => setCustomer({ ...customer, requiredDate: v })} />

              {error && <div className="p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">{error}</div>}

              <button
                type="submit"
                disabled={submitting}
                className="w-full py-3 bg-amber-600 hover:bg-amber-700 disabled:bg-amber-300 text-white font-semibold rounded-lg transition"
              >
                {submitting ? 'Placing Order...' : `Place Order — R ${total.toFixed(2)}`}
              </button>

              <p className="text-xs text-center text-gray-500">
                No account required. We'll contact you to confirm.
              </p>
            </form>
          </div>
        )}
      </div>
    </div>
  );
}

function Field({ label, value, onChange, type = 'text', required, placeholder }: any) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label} {required && <span className="text-red-500">*</span>}
      </label>
      <input
        type={type}
        required={required}
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-amber-500 focus:border-transparent"
      />
    </div>
  );
}
