import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import type { DeleteState } from "@/components/bin-locations/bin-locations-state";

interface BinLocationDeleteDialogProps {
  deleteState: DeleteState;
  isBusy: boolean;
  submitError?: string;
  onClose: () => void;
  onConfirm: () => void;
}

export function BinLocationDeleteDialog({
  deleteState,
  isBusy,
  submitError,
  onClose,
  onConfirm,
}: BinLocationDeleteDialogProps) {
  if (!deleteState) {
    return null;
  }

  const entityLabel =
    deleteState.kind === "zone"
      ? "storage zone"
      : deleteState.kind === "aisle"
        ? "storage aisle"
        : "bin";

  return (
    <Dialog
      open
      title={`Delete ${entityLabel}`}
      description={
        <>
          Are you sure you want to delete <strong>{deleteState.name}</strong>?
        </>
      }
      onClose={onClose}
      panelClassName="max-w-sm"
      footer={(
        <>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="destructive" disabled={isBusy} onClick={onConfirm}>
            {isBusy ? "Deleting..." : "Delete"}
          </Button>
        </>
      )}
    >
      {submitError ? (
        <p className="text-sm text-destructive">{submitError}</p>
      ) : null}
    </Dialog>
  );
}
