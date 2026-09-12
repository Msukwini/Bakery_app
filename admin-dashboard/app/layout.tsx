import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Ndlovu Bakery',
  description: 'Freshly baked, delivered to you',
  icons: {
    icon: '/logo.png',
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">{children}</body>
    </html>
  );
}
