import { test, expect } from "@playwright/test";

test.describe("Authentication Flow", () => {
  test.beforeEach(async ({ page }) => {
    // Clear localStorage and cookies before each test
    await page.goto("/");
    await page.evaluate(() => localStorage.clear());
    await page.context().clearCookies();
  });

  test("should redirect to login when not authenticated", async ({ page }) => {
    await page.goto("/dashboard");
    await expect(page).toHaveURL(/.*login/);
  });

  test("should show login page", async ({ page }) => {
    await page.goto("/login");
    
    // Verify that the login page has loaded
    await expect(page).toHaveURL(/.*login/);
    
    // Check for main elements (using more flexible selectors)
    const emailInput = page.locator('input[type="email"], input[name="email"]').first();
    const passwordInput = page.locator('input[type="password"], input[name="password"]').first();
    
    await expect(emailInput).toBeVisible({ timeout: 10000 });
    await expect(passwordInput).toBeVisible({ timeout: 10000 });
  });

  test("should have login button", async ({ page }) => {
    await page.goto("/login");
    
    // Search for login button by various text options
    const loginButton = page.getByRole("button").filter({ 
      hasText: /login|sign in/i 
    }).first();
    
    await expect(loginButton).toBeVisible({ timeout: 10000 });
  });
});
