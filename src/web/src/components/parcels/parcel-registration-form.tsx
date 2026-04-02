"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { CheckCircle } from "lucide-react";

import { ParcelEditorForm } from "@/components/parcels/parcel-editor-form";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { appToast } from "@/lib/toast/app-toast";
import { useRegisterParcel } from "@/queries/parcels";
import { parcelsService } from "@/services/parcels.service";
import type {
  ParcelFormData,
  RegisteredParcelResult,
} from "@/types/parcels";

interface ParcelRegistrationFormProps {
  onSuccess?: (parcel: RegisteredParcelResult) => void;
  onCancel?: () => void;
  onViewQueue?: () => void;
}

export function ParcelRegistrationForm({
  onSuccess,
  onCancel,
  onViewQueue,
}: ParcelRegistrationFormProps) {
  const router = useRouter();
  const registerParcel = useRegisterParcel();
  const [result, setResult] = useState<RegisteredParcelResult | null>(null);

  async function handleSubmit(form: ParcelFormData) {
    const parcel = await registerParcel.mutateAsync(form);
    setResult(parcel);
    onSuccess?.(parcel);
  }

  async function handleDownload(format: "zpl" | "pdf") {
    if (!result) {
      return;
    }

    try {
      await parcelsService.downloadLabel(result.id, format);
    } catch (downloadError) {
      appToast.errorFromUnknown(downloadError);
    }
  }

  if (result) {
    return (
      <Card className="max-w-2xl">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 py-8 text-center">
            <CheckCircle className="h-16 w-16 text-green-600" />
            <div>
              <h2 className="text-2xl font-bold">Parcel Registered</h2>
              <p className="mt-1 text-muted-foreground">
                The parcel has been successfully registered.
              </p>
            </div>
            <div className="w-full rounded-xl border bg-muted/40 p-4 text-left">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground">Tracking Number</p>
                  <p className="font-mono font-semibold">{result.trackingNumber}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Status</p>
                  <p className="font-medium">{result.status}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Service</p>
                  <p>{result.serviceType}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Zone</p>
                  <p>{result.zoneName ?? "Unassigned"}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Weight</p>
                  <p>
                    {result.weight} {result.weightUnit}
                  </p>
                </div>
                <div>
                  <p className="text-muted-foreground">Dimensions</p>
                  <p>
                    {result.length} x {result.width} x {result.height}{" "}
                    {result.dimensionUnit}
                  </p>
                </div>
              </div>
            </div>
            <div className="flex flex-wrap justify-center gap-3">
              <Button variant="outline" onClick={() => void handleDownload("zpl")}>
                Download 4x6 ZPL
              </Button>
              <Button variant="outline" onClick={() => void handleDownload("pdf")}>
                Download A4 PDF
              </Button>
            </div>
            <div className="flex flex-wrap justify-center gap-3">
              <Button variant="outline" onClick={() => setResult(null)}>
                Register Another
              </Button>
              <Button
                variant="outline"
                onClick={() => router.push(`/parcels/${result.id}`)}
              >
                Open Parcel Detail
              </Button>
              <Button
                onClick={() => {
                  onViewQueue?.();
                  router.push("/parcels");
                }}
              >
                View Pre-load Queue
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <ParcelEditorForm
      onSubmit={handleSubmit}
      onCancel={onCancel}
      submitLabel="Register Parcel"
      pendingLabel="Registering..."
      error={registerParcel.isError ? registerParcel.error : undefined}
      isPending={registerParcel.isPending}
    />
  );
}
