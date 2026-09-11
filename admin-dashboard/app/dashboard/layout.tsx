'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import {
  LayoutDashboard, Users, Package, ShoppingCart,
  Truck, DollarSign, FileText, LogOut, Home, Mail, UserCog, Building2
} from 'lucide-react';

const navItems = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/dashboard/users', label: 'Users & Roles', icon: UserCog },
  { href: '/dashboard/residences', label: 'Residences', icon: Building2 },
  { href: '/dashboard/resellers', label: 'Applications', icon: Users },
  { href: '/dashboard/stock-requests', label: 'Stock Requests', icon: Package },
  { href: '/dashboard/sales', label: 'Sales', icon: ShoppingCart },
  { href: '/dashboard/delivery', label: 'Delivery', icon: Truck },
  { href: '/dashboard/deposits', label: 'Deposits', icon: DollarSign },
  { href: '/dashboard/orders', label: 'Orders', icon: Home },
  { href: '/dashboard/reports', label: 'Reports', icon: FileText },
  { href: '/dashboard/notifications', label: 'Notifications', icon: Mail },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { user, logout, loading } = useAuth();

  if (loading) return <div className="p-8">Loading...</div>;
  if (!user) return null;

  return (
    <div className="min-h-screen flex bg-gray-50">
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6 border-b border-gray-200">
          <h1 className="text-lg font-bold text-gray-800">Ndlovu Bakery</h1>
          <p className="text-xs text-gray-500 mt-1">{user.role} · {user.employeeId}</p>
        </div>

        <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = pathname === item.href;
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition ${
                  active
                    ? 'bg-blue-50 text-blue-700 font-medium'
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <Icon size={18} />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-200">
          <button
            onClick={logout}
            className="flex items-center gap-3 w-full px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-100"
          >
            <LogOut size={18} />
            Sign out
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">
        <div className="p-8">{children}</div>
      </main>
    </div>
  );
}
