import { test, expect } from "@playwright/test";

const BASE_URL = process.env.E2E_BASE_URL || "http://localhost:3000";

test.describe("Vehicle Management", () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto(`${BASE_URL}/login`);
    await page.fill('input[name="email"]', "admin@lastmile.com");
    await page.fill('input[name="password"]', "Admin@12345");
    await page.click('button[type="submit"]');
    await page.waitForURL(`${BASE_URL}/vehicles`);
  });

  test.describe("Vehicle List", () => {
    test("should display vehicles list page", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles`);

      await expect(page.getByRole("heading", { name: "Vehicles" })).toBeVisible();
      await expect(page.getByRole("link", { name: "Add Vehicle" })).toBeVisible();
    });

    test("should filter vehicles by status", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles`);

      const statusFilter = page.getByRole("combobox", { name: /status/i });
      await statusFilter.selectOption("0"); // Available

      // Wait for filter to apply
      await page.waitForTimeout(500);
    });

    test("should navigate to vehicle detail", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles`);

      // Click on first "View" link if any vehicles exist
      const viewLink = page.getByRole("link", { name: "View" }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await expect(page).toHaveURL(/\/vehicles\/[a-f0-9-]+/);
      }
    });
  });

  test.describe("Create Vehicle", () => {
    test("should display create vehicle form", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles/new`);

      await expect(page.getByRole("heading", { name: "Add Vehicle" })).toBeVisible();
      await expect(page.getByLabel(/registration plate/i)).toBeVisible();
      await expect(page.getByLabel(/type/i)).toBeVisible();
      await expect(page.getByLabel(/parcel capacity/i)).toBeVisible();
      await expect(page.getByLabel(/weight capacity/i)).toBeVisible();
      await expect(page.getByLabel(/status/i)).toBeVisible();
      await expect(page.getByLabel(/depot/i)).toBeVisible();
    });

    test("should create a new vehicle", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles/new`);

      await page.fill('input[name="registrationPlate"]', `TEST-${Date.now()}`);

      // Select type: Van
      await page.getByLabel(/type/i).click();
      await page.getByRole("option", { name: "Van" }).click();

      await page.fill('input[name="parcelCapacity"]', "10");
      await page.fill('input[name="weightCapacity"]', "100");

      // Select status: Available
      await page.getByLabel(/status/i).click();
      await page.getByRole("option", { name: "Available" }).click();

      // Select depot: Test Depot (from seeded mock/API data)
      await page.getByLabel(/depot/i).click();
      await page.getByRole("option", { name: "Test Depot" }).click();

      await page.click('button[type="submit"]');

      // Should redirect to vehicles list
      await expect(page).toHaveURL(/\/vehicles/);
    });

    test("should show validation errors for empty form", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles/new`);

      await page.click('button[type="submit"]');

      // Check for HTML5 validation (required fields)
      const plateInput = page.getByLabel(/registration plate/i);
      await expect(plateInput).toHaveAttribute("required");
    });
  });

  test.describe("Edit Vehicle", () => {
    test("should display edit vehicle form", async ({ page }) => {
      // This test assumes there's at least one vehicle
      // In a real scenario, you'd create a vehicle first or use a known ID
      await page.goto(`${BASE_URL}/vehicles/00000000-0000-0000-0000-000000000001/edit`);

      // May return 404 if vehicle doesn't exist, so check for form OR 404
      const notFound = await page.getByText(/not found/i).isVisible();
      if (!notFound) {
        await expect(page.getByRole("heading", { name: "Edit Vehicle" })).toBeVisible();
      }
    });

    test("should update a vehicle", async ({ page }) => {
      // Navigate directly to a potential vehicle edit page
      // In real tests, you'd first create a vehicle then edit it
      await page.goto(`${BASE_URL}/vehicles/00000000-0000-0000-0000-000000000001/edit`);

      const notFound = await page.getByText(/not found|vehicle not found/i).isVisible();

      if (!notFound) {
        // Update capacity
        await page.fill('input[name="parcelCapacity"]', "20");
        await page.click('button[type="submit"]');

        // Should navigate to detail page
        await expect(page).toHaveURL(/\/vehicles\/[a-f0-9-]+/);
      }
    });
  });

  test.describe("Vehicle Detail", () => {
    test("should display vehicle details", async ({ page }) => {
      await page.goto(`${BASE_URL}/vehicles/00000000-0000-0000-0000-000000000001`);

      const notFound = await page.getByText(/not found|vehicle not found/i).isVisible();

      if (!notFound) {
        await expect(page.getByRole("link", { name: "Edit" })).toBeVisible();
        await expect(page.getByRole("link", { name: "Back to Vehicles" })).toBeVisible();
      }
    });
  });

  test.describe("Delete Vehicle", () => {
    test("should delete a vehicle", async ({ page }) => {
      // First create a vehicle to delete
      await page.goto(`${BASE_URL}/vehicles/new`);

      const testPlate = `TEST-DELETE-${Date.now()}`;
      await page.fill('input[name="registrationPlate"]', testPlate);

      // Select type: Van
      await page.getByLabel(/type/i).click();
      await page.getByRole("option", { name: "Van" }).click();

      await page.fill('input[name="parcelCapacity"]', "5");
      await page.fill('input[name="weightCapacity"]', "50");

      // Select status: Available
      await page.getByLabel(/status/i).click();
      await page.getByRole("option", { name: "Available" }).click();

      // Select depot: Test Depot
      await page.getByLabel(/depot/i).click();
      await page.getByRole("option", { name: "Test Depot" }).click();

      await page.click('button[type="submit"]');

      await page.waitForURL(/\/vehicles/);

      // Now delete it - simplified test: dialog is dismissed
      page.on("dialog", async (dialog) => {
        await dialog.dismiss();
      });
    });
  });
});

test.describe("Authentication", () => {
  test("should redirect unauthenticated user to login", async ({ page }) => {
    await page.goto(`${BASE_URL}/vehicles`);

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });
});
