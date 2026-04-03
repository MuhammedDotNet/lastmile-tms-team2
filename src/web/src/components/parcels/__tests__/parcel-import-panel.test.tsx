import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ParcelImportPanel } from "@/components/parcels/parcel-import-panel";

const mockUseDepots = vi.fn();
const mockUseParcelImports = vi.fn();
const mockUseParcelImport = vi.fn();
const uploadMutateAsync = vi.fn();
const templateMutateAsync = vi.fn();
const errorsMutateAsync = vi.fn();

vi.mock("@/queries/depots", () => ({
  useDepots: () => mockUseDepots(),
}));

vi.mock("@/queries/parcels", () => ({
  useParcelImports: () => mockUseParcelImports(),
  useParcelImport: (...args: unknown[]) => mockUseParcelImport(...args),
  useUploadParcelImport: () => ({
    mutateAsync: uploadMutateAsync,
    isPending: false,
  }),
  useDownloadParcelImportTemplate: () => ({
    mutateAsync: templateMutateAsync,
    isPending: false,
  }),
  useDownloadParcelImportErrors: () => ({
    mutateAsync: errorsMutateAsync,
    isPending: false,
  }),
}));

vi.mock("@/components/form/select-dropdown", () => ({
  SelectDropdown: ({
    options,
    value,
    onChange,
    id,
    className,
    disabled,
  }: {
    options: Array<{ value: string | number; label: string }>;
    value: string | number;
    onChange: (value: string | number) => void;
    id?: string;
    className?: string;
    disabled?: boolean;
  }) => (
    <select
      data-testid={id ?? "select-dropdown"}
      className={className}
      disabled={disabled}
      value={String(value)}
      onChange={(event) => {
        const option = options.find(
          (candidate) => String(candidate.value) === event.target.value,
        );
        onChange(option?.value ?? event.target.value);
      }}
    >
      {options.map((option) => (
        <option key={String(option.value)} value={String(option.value)}>
          {option.label}
        </option>
      ))}
    </select>
  ),
}));

describe("ParcelImportPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    mockUseDepots.mockReturnValue({
      data: [
        {
          id: "depot-1",
          name: "North Depot",
          addressId: "address-1",
          isActive: true,
        },
      ],
      isLoading: false,
    });

    mockUseParcelImports.mockReturnValue({
      data: [
        {
          id: "import-1",
          fileName: "parcels.csv",
          fileFormat: "Csv",
          status: "CompletedWithErrors",
          totalRows: 2,
          processedRows: 2,
          importedRows: 1,
          rejectedRows: 1,
          depotName: "North Depot",
          createdAt: "2030-01-15T00:00:00Z",
          startedAt: "2030-01-15T00:00:01Z",
          completedAt: "2030-01-15T00:00:03Z",
        },
      ],
      isLoading: false,
      error: null,
    });

    mockUseParcelImport.mockReturnValue({
      data: {
        id: "import-1",
        fileName: "parcels.csv",
        fileFormat: "Csv",
        status: "Processing",
        totalRows: 4,
        processedRows: 2,
        importedRows: 1,
        rejectedRows: 1,
        depotName: "North Depot",
        failureMessage: null,
        createdAt: "2030-01-15T00:00:00Z",
        startedAt: "2030-01-15T00:00:01Z",
        completedAt: null,
        createdTrackingNumbers: ["LM202604010001"],
        rowFailuresPreview: [
          {
            rowNumber: 3,
            errorMessage: "weight must be a valid number.",
            originalRowValues: "{\"recipient_street1\":\"17 Pitt Street\"}",
          },
        ],
      },
      isLoading: false,
      error: null,
    });

    uploadMutateAsync.mockResolvedValue({ importId: "import-1" });
  });

  it("uploads with the selected depot addressId and renders progress plus history", async () => {
    render(<ParcelImportPanel />);

    const user = userEvent.setup();
    const file = new File(["csv-content"], "parcels.csv", { type: "text/csv" });

    await user.selectOptions(screen.getByTestId("parcel-import-depot"), "address-1");

    await user.upload(screen.getByLabelText(/parcel import file/i), file);
    await user.click(screen.getByRole("button", { name: /start import/i }));

    await waitFor(() => {
      expect(uploadMutateAsync).toHaveBeenCalledWith({
        shipperAddressId: "address-1",
        file,
      });
    });

    expect(screen.getByText("50% complete")).toBeInTheDocument();
    expect(screen.getByText("LM202604010001")).toBeInTheDocument();
    expect(screen.getByText("weight must be a valid number.")).toBeInTheDocument();
    expect(screen.getAllByText("parcels.csv").length).toBeGreaterThan(0);
    expect(screen.getAllByText("North Depot").length).toBeGreaterThan(0);
  });
});
