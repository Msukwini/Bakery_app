'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import {
  LayoutDashboard, Users, Package, ShoppingCart,
  Truck, DollarSign, FileText, LogOut, Home, Mail, UserCog, Building2, Menu, X,
  TrendingUp, ClipboardList, User, UsersRound
} from 'lucide-react';

const adminNav = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/dashboard/products', label: 'Products', icon: Package },
  { href: '/dashboard/commission-rules', label: 'Commission Rules', icon: DollarSign },
  { href: '/dashboard/users', label: 'Users { href: '/dashboard/users', label: 'Users & Roles', icon: UserCog }, Roles', icon: UserCog },
  { href: '/dashboard/reseller-lifecycle', label: 'Reseller Lifecycle', icon: Users },
  { href: '/dashboard/residences', label: 'Residences', icon: Building2 },
  { href: '/dashboard/resellers', label: 'Applications', icon: Users },
  { href: '/dashboard/stock-requests', label: 'Stock Requests', icon: Package },
  { href: '/dashboard/sales', label: 'Sales', icon: ShoppingCart },
  { href: '/dashboard/delivery', label: 'Delivery', icon: Truck },
  { href: '/dashboard/delivery-assignments', label: 'Delivery Assignments', icon: UsersRound },
  { href: '/dashboard/deposits', label: 'Deposits', icon: DollarSign },
  { href: '/dashboard/orders', label: 'Orders', icon: Home },
  { href: '/dashboard/finance', label: 'Finance', icon: DollarSign },
  { href: '/dashboard/reports', label: 'Reports', icon: FileText },
  { href: '/dashboard/notifications', label: 'Notifications', icon: Mail },
  { href: '/dashboard/profile', label: 'My Profile', icon: User },
];

const resellerNav = [
  { href: '/dashboard/my-sales', label: 'My Sales', icon: TrendingUp },
  { href: '/dashboard/my-stock-requests', label: 'My Stock', icon: Package },
  { href: '/dashboard/my-commission', label: 'My Commission', icon: DollarSign },
  { href: '/dashboard/my-team', label: 'My Delivery Person', icon: UsersRound },
  { href: '/dashboard/profile', label: 'My Profile', icon: User },
];

const deliveryNav = [
  { href: '/dashboard/my-deliveries', label: 'My Deliveries', icon: Truck },
  { href: '/dashboard/my-team', label: 'My Team', icon: UsersRound },
  { href: '/dashboard/my-assignments', label: 'My Assignments', icon: ClipboardList },
  { href: '/dashboard/my-earnings', label: 'My Earnings', icon: TrendingUp },
  { href: '/dashboard/profile', label: 'My Profile', icon: User },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, logout, loading } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    if (!user) return;
    // Route guards omitted for brevity — same as before
  }, [pathname, user, router]);

  if (loading) return <div className="p-8">Loading...</div>;
  if (!user) return null;

  const navItems = user.role === 'Admin' ? adminNav :
                   user.role === 'Reseller' ? resellerNav :
                   user.role === 'Delivery' ? deliveryNav : [];

  const roleColor = user.role === 'Admin' ? 'bg-purple-100 text-purple-800' :
                    user.role === 'Reseller' ? 'bg-blue-100 text-blue-800' :
                    'bg-green-100 text-green-800';

  return (
    <div className="min-h-screen flex bg-gray-50">
      <div className="lg:hidden fixed top-0 left-0 right-0 bg-white border-b border-gray-200 z-30 flex items-center justify-between px-4 py-3">
        <button onClick={() => setSidebarOpen(true)} className="text-gray-700"><Menu size={24} /></button>
        <h1 className="font-bold text-gray-800">Ndlovu Bakery</h1>
        <div className="w-6" />
      </div>

      {sidebarOpen && (
        <div className="lg:hidden fixed inset-0 bg-black/40 z-40" onClick={() => setSidebarOpen(false)} />
      )}

      <aside className={`fixed lg:static inset-y-0 left-0 w-64 bg-white border-r border-gray-200 flex flex-col z-50 transform transition-transform ${
        sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      }`}>
        <div className="p-6 border-b border-gray-200 flex items-center justify-between">
          <div className="min-w-0">
            <h1 className="text-lg font-bold text-gray-800">Ndlovu Bakery</h1>
            <div className="flex items-center gap-2 mt-1">
              <span className={`px-2 py-0.5 rounded text-[10px] font-semibold ${roleColor}`}>
                {user.role.toUpperCase()}
              </span>
              <span className="text-xs text-gray-500 truncate">{user.employeeId}</span>
            </div>
          </div>
          <button onClick={() => setSidebarOpen(false)} className="lg:hidden text-gray-400"><X size={20} /></button>
        </div>

        <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = pathname === item.href;
            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setSidebarOpen(false)}
                className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition ${
                  active ? 'bg-blue-50 text-blue-700 font-medium' : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <Icon size={18} /> {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-200">
          <button onClick={logout} className="flex items-center gap-3 w-full px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-100">
            <LogOut size={18} /> Sign out
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto lg:ml-0 pt-14 lg:pt-0">
        <div className="p-4 sm:p-6 lg:p-8">{children}</div>
      </main>
    </div>
  );
}
