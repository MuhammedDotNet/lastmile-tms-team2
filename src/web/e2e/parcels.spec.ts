import fs from "node:fs/promises";

import { expect, test, type APIRequestContext, type Page } from "@playwright/test";

const backendUrl = "http://127.0.0.1:5100";
const testSupportKey = process.env.TEST_SUPPORT_KEY ?? "e2e-test-support-key";

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

async function resetFixture(request: APIRequestContext) {
  let lastStatus = 0;
  let lastBody = "";

  for (let attempt = 0; attempt < 10; attempt += 1) {
    const response = await request.post(`${backendUrl}/api/test-support/user-management/reset-and-seed`, {
      headers: {
        "X-Test-Support-Key": testSupportKey,
      },
    });

    if (response.ok()) {
      return response.json() as Promise<{
        adminEmail: string;
        adminPassword: string;
        depotName: string;
        zoneName: string;
      }>;
    }

    lastStatus = response.status();
    lastBody = await response.text();
    await new Promise((resolve) => setTimeout(resolve, 1_000));
  }

  throw new Error(`Could not reset the E2E fixture. Status: ${lastStatus}. Body: ${lastBody}`);
}

async function login(page: Page, email: string, password: string) {
  await page.goto("/login");
  await page.getByLabel(/email/i).fill(email);
  await page.getByLabel(/password/i).fill(password);
  await page.getByRole("button", { name: /^login$/i }).click();
  await expect(page).toHaveURL(/\/dashboard$/, { timeout: 15_000 });
}

test.describe("Bulk parcel import", () => {
  test("can import a mixed CSV file and download the error report", async ({ page, request }, testInfo) => {
    const fixture = await resetFixture(request);

    await login(page, fixture.adminEmail, fixture.adminPassword);
    await page.goto("/parcels");

    await expect(
      page.getByRole("heading", { name: /warehouse intake queue/i }),
    ).toBeVisible();
    await expect(page.getByText(/bulk parcel import/i)).toBeVisible();

    const templateDownloadPromise = page.waitForEvent("download");
    await page.getByRole("button", { name: /csv template/i }).click();
    const templateDownload = await templateDownloadPromise;
    expect(templateDownload.suggestedFilename()).toBe("parcel-import-template.csv");

    const csvPath = testInfo.outputPath("parcel-import.csv");
    await fs.writeFile(
      csvPath,
      [
        "recipient_street1,recipient_street2,recipient_city,recipient_state,recipient_postal_code,recipient_country_code,recipient_is_residential,recipient_contact_name,recipient_company_name,recipient_phone,recipient_email,description,parcel_type,service_type,weight,weight_unit,length,width,height,dimension_unit,declared_value,currency,estimated_delivery_date",
        "15 George Street,,Sydney,NSW,2000,AU,true,Taylor Smith,Acme,+61000000000,taylor@example.com,Box,Package,STANDARD,2.5,KG,20,10,5,CM,100,AUD,2030-01-15",
        "17 Pitt Street,,Sydney,NSW,2000,AU,true,Jordan Lee,Acme,+61000000001,jordan@example.com,Box,Package,STANDARD,abc,KG,20,10,5,CM,100,AUD,2030-01-15",
      ].join("\n"),
      "utf8",
    );

    await page.getByRole("button", { name: /select depot/i }).click();
    await page.getByRole("option", {
      name: new RegExp(`^${escapeRegExp(fixture.depotName)}$`),
    }).click();
    await page.getByLabel(/parcel import file/i).setInputFiles(csvPath);
    await page.getByRole("button", { name: /start import/i }).click();

    await expect(page.getByText("100% complete")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByText(/completed with errors/i).first()).toBeVisible();
    await expect(page.getByText("weight must be a valid number.")).toBeVisible();
    await expect(page.getByText(/^LM\d{8}/).first()).toBeVisible();
    await expect(page.getByText("parcel-import.csv").first()).toBeVisible();
    await expect(page.getByText(fixture.depotName).first()).toBeVisible();

    const errorsDownloadPromise = page.waitForEvent("download");
    await page.getByRole("button", { name: /download error report/i }).click();
    const errorsDownload = await errorsDownloadPromise;
    await expect(errorsDownload.suggestedFilename()).toContain("-errors.csv");
  });
});
