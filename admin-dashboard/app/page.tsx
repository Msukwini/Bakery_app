'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Croissant, ShoppingBag, UserPlus, LogIn, Phone, MapPin, Clock, ShoppingCart, Mail } from 'lucide-react';

interface ProductVariant {
  id: string;
  sizeName: string;
  unitPrice: number;
}

interface Product {
  id: string;
  name: string;
  description: string;
  variants: ProductVariant[];
}

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  useEffect(() => {
    fetch(`${apiUrl}/api/Products/catalog`)
      .then((r) => r.json())
      .then((data) => setProducts(data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [apiUrl]);

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="fixed top-0 left-0 right-0 bg-white/95 backdrop-blur border-b border-gray-100 z-40">
        <div className="max-w-6xl mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Croissant className="text-amber-600" size={28} />
            <span className="font-bold text-xl text-gray-800">Ndlovu Bakery</span>
          </div>
          <div className="flex items-center gap-2">
            <Link
              href="/login"
              className="flex items-center gap-1.5 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg"
            >
              <LogIn size={16} />
              <span className="hidden sm:inline">Login</span>
            </Link>
            <Link
              href="/order"
              className="flex items-center gap-1.5 px-4 py-2 text-sm bg-amber-600 hover:bg-amber-700 text-white rounded-lg"
            >
              <ShoppingCart size={16} />
              <span className="hidden sm:inline">Order</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Hero */}
      <section className="pt-24 pb-16 px-4 bg-gradient-to-br from-amber-50 via-orange-50 to-yellow-50">
        <div className="max-w-6xl mx-auto text-center">
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold text-gray-900 mb-6">
            Freshly baked, <span className="text-amber-600">delivered to you</span>
          </h1>
          <p className="text-lg sm:text-xl text-gray-600 max-w-2xl mx-auto mb-10">
            Artisan sourdough, buttery croissants, and more. Order for your event, business, or residence.
          </p>
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link
              href="/order"
              className="flex items-center justify-center gap-2 px-8 py-4 bg-amber-600 hover:bg-amber-700 text-white font-semibold rounded-xl shadow-lg transition"
            >
              <ShoppingBag size={20} />
              Order as Guest
            </Link>
            <Link
              href="/apply"
              className="flex items-center justify-center gap-2 px-8 py-4 bg-white hover:bg-gray-50 text-gray-800 font-semibold rounded-xl shadow border border-gray-200 transition"
            >
              <UserPlus size={20} />
              Become a Reseller
            </Link>
          </div>
        </div>
      </section>

      {/* Products */}
      <section className="py-16 px-4">
        <div className="max-w-6xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-3">Our Products</h2>
            <p className="text-gray-600">Baked fresh every day</p>
          </div>

          {loading ? (
            <p className="text-center text-gray-400">Loading products...</p>
          ) : products.length === 0 ? (
            <p className="text-center text-gray-400">No products available right now.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {products.map((p) => (
                <div key={p.id} className="bg-white border border-gray-100 rounded-2xl overflow-hidden hover:shadow-lg transition">
                  <div className="h-40 bg-gradient-to-br from-amber-100 to-orange-100 flex items-center justify-center">
                    <Croissant size={64} className="text-amber-600" />
                  </div>
                  <div className="p-5">
                    <h3 className="font-bold text-lg text-gray-800 mb-1">{p.name}</h3>
                    <p className="text-sm text-gray-500 mb-4 line-clamp-2">{p.description}</p>
                    <div className="space-y-2">
                      {p.variants.map((v) => (
                        <div key={v.id} className="flex justify-between items-center text-sm">
                          <span className="text-gray-600">{v.sizeName}</span>
                          <span className="font-semibold text-gray-800">R {v.unitPrice.toFixed(2)}</span>
                        </div>
                      ))}
                    </div>
                    <Link
                      href="/order"
                      className="mt-4 block text-center py-2 bg-amber-50 hover:bg-amber-100 text-amber-700 font-medium rounded-lg text-sm transition"
                    >
                      Order Now
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </section>

      {/* Why Us */}
      <section className="py-16 px-4 bg-gray-50">
        <div className="max-w-6xl mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Croissant className="text-amber-600" size={28} />
              </div>
              <h3 className="font-bold text-lg mb-2">Freshly Baked</h3>
              <p className="text-sm text-gray-600">Made daily with quality ingredients</p>
            </div>
            <div className="text-center">
              <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <ShoppingBag className="text-amber-600" size={28} />
              </div>
              <h3 className="font-bold text-lg mb-2">Bulk Orders</h3>
              <p className="text-sm text-gray-600">Perfect for events, offices, and residences</p>
            </div>
            <div className="text-center">
              <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <UserPlus className="text-amber-600" size={28} />
              </div>
              <h3 className="font-bold text-lg mb-2">Reseller Program</h3>
              <p className="text-sm text-gray-600">Earn commission by selling in your area</p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-16 px-4 bg-amber-600">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl sm:text-4xl font-bold text-white mb-4">
            Want to earn extra income?
          </h2>
          <p className="text-amber-50 mb-8 text-lg">
            Become an Ndlovu Bakery reseller. Sell to your residence or community.
            Earn commission on every sale.
          </p>
          <Link
            href="/apply"
            className="inline-flex items-center gap-2 px-8 py-4 bg-white hover:bg-gray-50 text-amber-700 font-semibold rounded-xl shadow-lg transition"
          >
            <UserPlus size={20} />
            Apply Now
          </Link>
        </div>
      </section>

      {/* Footer */}
      <footer className="py-12 px-4 bg-gray-900 text-gray-300">
        <div className="max-w-6xl mx-auto grid grid-cols-1 sm:grid-cols-3 gap-8">
          <div>
            <div className="flex items-center gap-2 mb-4">
              <Croissant className="text-amber-500" size={24} />
              <span className="font-bold text-white">Ndlovu Bakery</span>
            </div>
            <p className="text-sm text-gray-400">Freshly baked goods, delivered daily.</p>
          </div>
          <div>
            <h3 className="font-semibold text-white mb-3">Contact</h3>
            <div className="space-y-2 text-sm">
              <div className="flex items-center gap-2">
                <Phone size={14} />
                <span>082 123 4567</span>
              </div>
              <div className="flex items-center gap-2">
                <Mail size={14} />
                <span>bakeryndlovu@gmail.com</span>
              </div>
              <div className="flex items-center gap-2">
                <MapPin size={14} />
                <span>Durban, KZN</span>
              </div>
            </div>
          </div>
          <div>
            <h3 className="font-semibold text-white mb-3">Quick Links</h3>
            <div className="space-y-2 text-sm">
              <div><Link href="/order" className="hover:text-white">Order Now</Link></div>
              <div><Link href="/apply" className="hover:text-white">Become a Reseller</Link></div>
              <div><Link href="/login" className="hover:text-white">Staff Login</Link></div>
            </div>
          </div>
        </div>
        <div className="max-w-6xl mx-auto mt-8 pt-6 border-t border-gray-800 text-center text-xs text-gray-500">
          © {new Date().getFullYear()} Ndlovu Bakery. All rights reserved.
        </div>
      </footer>
    </div>
  );
}
