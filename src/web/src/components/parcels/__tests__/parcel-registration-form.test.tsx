import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ParcelRegistrationForm } from "@/components/parcels/parcel-registration-form";

const push = vi.fn();
const back = vi.fn();

const mockUseDepots = vi.fn();
const mutateAsync = vi.fn();
const mockUseRegisterParcel = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push,
    back,
  }),
}));

vi.mock("@/queries/depots", () => ({
  useDepots: () => mockUseDepots(),
}));

vi.mock("@/queries/parcels", () => ({
  useRegisterParcel: () => mockUseRegisterParcel(),
}));

describe("ParcelRegistrationForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    mockUseDepots.mockReturnValue({
      data: [
        {
          id: "depot-1",
          name: "North Depot",
          addressId: "address-1",
          address: {
            street1: "1 Depot Road",
            street2: null,
            city: "Sydney",
            state: "NSW",
            postalCode: "2000",
            countryCode: "AU",
          },
          isActive: true,
        },
      ],
      isLoading: false,
    });

    mutateAsync.mockResolvedValue({
      id: "parcel-1",
      trackingNumber: "LM202604010001",
      barcode: "LM202604010001",
      status: "Registered",
      serviceType: "STANDARD",
      weight: 1,
      weightUnit: "KG",
      length: 10,
      width: 10,
      height: 10,
      dimensionUnit: "CM",
      declaredValue: 0,
      currency: "USD",
      description: null,
      parcelType: null,
      estimatedDeliveryDate: "2030-01-15T00:00:00Z",
      createdAt: "2030-01-15T00:00:00Z",
      zoneId: "zone-1",
      zoneName: "Zone A",
      depotId: "depot-1",
      depotName: "North Depot",
    });

    mockUseRegisterParcel.mockReturnValue({
      mutateAsync,
      isPending: false,
      isError: false,
      error: null,
    });
  });

  it("submits the selected depot addressId as shipperAddressId", async () => {
    render(<ParcelRegistrationForm />);

    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /select depot/i }));
    await user.click(await screen.findByRole("option", { name: /north depot/i }));

    fireEvent.change(screen.getAllByLabelText(/street address/i)[0], {
      target: { value: "15 George Street" },
    });
    fireEvent.change(screen.getByLabelText(/^city/i), {
      target: { value: "Sydney" },
    });
    fireEvent.change(screen.getByLabelText(/state \/ province/i), {
      target: { value: "NSW" },
    });
    fireEvent.change(screen.getByLabelText(/postal code/i), {
      target: { value: "2000" },
    });
    fireEvent.change(screen.getByLabelText(/est\. delivery date/i), {
      target: { value: "2030-01-15" },
    });

    await user.click(screen.getByRole("button", { name: /register parcel/i }));

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({
          shipperAddressId: "address-1",
        }),
      );
    });
  });
});
