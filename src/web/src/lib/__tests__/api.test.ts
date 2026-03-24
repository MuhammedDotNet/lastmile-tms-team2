import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("next-auth/react", () => ({
  getSession: vi.fn(() => Promise.resolve(null)),
}));

import { apiFetch, parseApiErrorMessage } from "../api";

const mockFetch = vi.fn();
global.fetch = mockFetch;

describe("apiFetch", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    process.env.NEXT_PUBLIC_API_URL = "http://localhost:8080";
  });

  it("should fetch data successfully", async () => {
    const mockData = { id: 1, name: "Test" };

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockData,
    });

    const result = await apiFetch("/test");

    expect(result).toEqual(mockData);
    expect(global.fetch).toHaveBeenCalledWith(
      "http://localhost:8080/test",
      expect.objectContaining({ headers: expect.any(Headers) }),
    );
    const [, init] = mockFetch.mock.calls[0] as [string, RequestInit];
    const headers = init.headers as Headers;
    expect(headers.get("Content-Type")).toBe("application/json");
  });

  it("should throw error on failed request", async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 404,
      text: async () => "",
    });

    await expect(apiFetch("/not-found")).rejects.toThrow("Request failed (404)");
  });

  it("should surface FluentValidation messages from error body", async () => {
    const validationBody = JSON.stringify({
      errors: [
        {
          field: "Dto.Status",
          message: "New vehicles must be created with status Available.",
        },
      ],
    });

    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 400,
      text: async () => validationBody,
    });

    await expect(apiFetch("/api/vehicles", { method: "POST" })).rejects.toThrow(
      "New vehicles must be created with status Available.",
    );
  });

  it("should surface ASP.NET model-state dictionary in errors", async () => {
    const body = JSON.stringify({
      title: "One or more validation errors occurred.",
      status: 400,
      errors: {
        DepotId: ["The DepotId field is required."],
      },
    });

    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 400,
      text: async () => body,
    });

    await expect(apiFetch("/api/vehicles", { method: "POST" })).rejects.toThrow(
      "The DepotId field is required.",
    );
  });
});

describe("parseApiErrorMessage", () => {
  it("reads RFC 7807 detail when errors is empty", async () => {
    const res = {
      status: 404,
      text: async () =>
        JSON.stringify({
          title: "Not Found",
          detail: "Vehicle abc was not found.",
        }),
    } as Response;

    await expect(parseApiErrorMessage(res)).resolves.toBe(
      "Vehicle abc was not found.",
    );
  });
});
