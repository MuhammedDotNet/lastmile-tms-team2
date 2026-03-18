import { Truck } from "lucide-react";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background via-background to-muted p-4">
      <div className="w-full max-w-md space-y-8">
        <div className="flex flex-col items-center space-y-2">
          <div className="flex size-12 items-center justify-center rounded-xl bg-primary">
            <Truck className="size-6 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Last Mile TMS</h1>
          <p className="text-sm text-muted-foreground">
            Delivery Management System
          </p>
        </div>
        {children}
      </div>
    </div>
  );
}
