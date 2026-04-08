import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { DeliveryZoneOption } from "@/types/bin-locations";

interface NameFormProps {
  id: string;
  label: string;
  submitLabel: string;
  value: string;
  onChange: (value: string) => void;
  onCancel: () => void;
  isPending: boolean;
}

export function NameForm({
  id,
  label,
  submitLabel,
  value,
  onChange,
  onCancel,
  isPending,
}: NameFormProps) {
  return (
    <div className="rounded-2xl border border-border/60 bg-background/80 p-4">
      <div className="space-y-3">
        <div>
          <Label htmlFor={id}>{label}</Label>
          <Input
            id={id}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            required
          />
        </div>
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {submitLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}

interface BinFormProps {
  value: string;
  isActive: boolean;
  deliveryZoneId: string;
  deliveryZoneOptions: DeliveryZoneOption[];
  disabledDeliveryZoneIds: Set<string>;
  onNameChange: (value: string) => void;
  onActiveChange: (value: boolean) => void;
  onDeliveryZoneChange: (value: string) => void;
  onCancel: () => void;
  submitLabel: string;
  isPending: boolean;
}

export function BinForm({
  value,
  isActive,
  deliveryZoneId,
  deliveryZoneOptions,
  disabledDeliveryZoneIds,
  onNameChange,
  onActiveChange,
  onDeliveryZoneChange,
  onCancel,
  submitLabel,
  isPending,
}: BinFormProps) {
  return (
    <div className="rounded-2xl border border-border/60 bg-background/80 p-4">
      <div className="grid gap-3 sm:grid-cols-2">
        <div>
          <Label htmlFor="bin-name">Bin name</Label>
          <Input
            id="bin-name"
            value={value}
            onChange={(event) => onNameChange(event.target.value)}
            required
          />
        </div>
        <div>
          <Label htmlFor="bin-status">Status</Label>
          <select
            id="bin-status"
            value={isActive ? "true" : "false"}
            onChange={(event) => onActiveChange(event.target.value === "true")}
            className="flex h-10 w-full items-center rounded-xl border border-input/90 bg-background px-3 py-2 text-sm"
          >
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </div>
        <div className="sm:col-span-2">
          <Label htmlFor="bin-delivery-zone">Delivery zone</Label>
          <select
            id="bin-delivery-zone"
            value={deliveryZoneId}
            onChange={(event) => onDeliveryZoneChange(event.target.value)}
            className="flex h-10 w-full items-center rounded-xl border border-input/90 bg-background px-3 py-2 text-sm"
          >
            <option value="">Unassigned</option>
            {deliveryZoneOptions.map((zone) => (
              <option
                key={zone.id}
                value={zone.id}
                disabled={disabledDeliveryZoneIds.has(zone.id)}
              >
                {zone.name}
              </option>
            ))}
          </select>
          <p className="mt-1 text-sm text-muted-foreground">
            Assign a delivery zone when this bin should be the future sorting target for that route area.
          </p>
        </div>
      </div>
      <div className="mt-4 flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isPending}>
          {submitLabel}
        </Button>
      </div>
    </div>
  );
}
