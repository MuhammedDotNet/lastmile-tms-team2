import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { auth } from "@/lib/auth";
import BinLocationsPage from "@/components/bin-locations/bin-locations-page";

export const metadata: Metadata = {
  title: "Bin Locations - Last Mile TMS",
  description: "Manage depot storage zones, aisles, and bins.",
};

export default async function BinLocationsRoute() {
  const session = await auth();

  if (!session?.user.roles.some((role) => role === "Admin" || role === "OperationsManager")) {
    redirect("/dashboard");
  }

  return <BinLocationsPage />;
}
