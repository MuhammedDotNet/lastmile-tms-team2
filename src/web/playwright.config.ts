import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  // The local E2E stack shares backend state across specs, so we keep the
  // suite serial to avoid reset/create races between workers.
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: "html",
  use: {
    baseURL: "http://127.0.0.1:3100",
    trace: "on-first-retry",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: {
    command: "node ./e2e/start-stack.mjs",
    url: "http://127.0.0.1:3100/login",
    reuseExistingServer: false,
    timeout: 180 * 1000,
  },
});
