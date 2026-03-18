import { auth } from "@/lib/auth";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Dashboard — Last Mile TMS",
  description: "Last Mile TMS delivery system dashboard",
};

export default async function DashboardPage() {
  const session = await auth();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Dashboard
        </h1>
        <p className="text-muted-foreground">
          Welcome, {session?.user?.name ?? session?.user?.email}
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Orders</h3>
          <p className="mt-2 text-2xl font-bold">—</p>
        </div>
        <div className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">
            Drivers
          </h3>
          <p className="mt-2 text-2xl font-bold">—</p>
        </div>
        <div className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">
            Active Routes
          </h3>
          <p className="mt-2 text-2xl font-bold">—</p>
        </div>
        <div className="rounded-xl border border-border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Warehouses</h3>
          <p className="mt-2 text-2xl font-bold">—</p>
        </div>
      </div>
    </div>
  );
}
