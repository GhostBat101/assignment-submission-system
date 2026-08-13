import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { AuthProvider } from "../context/AuthContext";

/*
 * File: layout.tsx (src/app/layout.tsx)
 * Purpose: Root layout for the Next.js application, wrapping all pages with the AuthProvider context.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: AuthProvider context.
 * 
 * Used By: Next.js App Router root layout engine.
 */

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Assignment & Submission Management System",
  description: "Role-based school/college assignment management application",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        suppressHydrationWarning
        className={`${geistSans.variable} ${geistMono.variable} antialiased bg-gray-50 min-h-screen text-gray-900`}
      >
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
