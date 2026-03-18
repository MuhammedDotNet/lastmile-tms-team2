import { test, expect } from "@playwright/test";

test.describe("Navigation", () => {
  test("should redirect to login when not authenticated", async ({ page }) => {
    await page.goto("/");
    // Expect redirect to login page
    await expect(page).toHaveURL(/.*login/);
  });

  test("should have correct page title on login page", async ({ page }) => {
    await page.goto("/login");
    await expect(page).toHaveTitle(/Last Mile TMS/i);
  });
});
