"use client";

/*
 * File: page.tsx (src/app/page.tsx)
 * Purpose: Application entry root page performing automatic role-based redirection.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Checks `user` and `isLoading` states.
 * 
 * Used By: Next.js App Router root `/` route.
 */

import { useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { useRouter } from "next/navigation";

export default function Home() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading) {
      if (!user) {
        router.push("/login");
      } else if (user.role === "Admin") {
        router.push("/admin");
      } else if (user.role === "Teacher") {
        router.push("/teacher");
      } else if (user.role === "Student") {
        router.push("/student");
      }
    }
  }, [user, isLoading, router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-gray-500 animate-pulse font-medium">Loading Assignment System...</div>
    </div>
  );
}
