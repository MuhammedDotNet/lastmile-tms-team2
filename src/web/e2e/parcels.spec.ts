import { expect, test, type APIRequestContext, type Page, type Response } from "@playwright/test";

const backendUrl = "http://127.0.0.1:5100";
const supportKey = process.env.TEST_SUPPORT_KEY ?? "e2e-test-support-key";

interface ParcelFixture {
  adminEmail: string;
  adminPassword: string;
  depotName: string;
  zoneName: string;
}

interface RegisteredParcelFixture {
  trackingNumber: string;
  zoneName: string;
}

function getFutureDate(daysAhead = 7): string {
  const date = new Date();
  date.setDate(date.getDate() + daysAhead);
  return date.toISOString().slice(0, 10);
}

async function resetAndSeedFixture(request: APIRequestContext): Promise<ParcelFixture> {
  const response = await request.post(
    `${backendUrl}/api/test-support/user-management/reset-and-seed`,
    {
      headers: {
        "X-Test-Support-Key": supportKey,
      },
    },
  );

  expect(response.ok()).toBeTruthy();
  return (await response.json()) as ParcelFixture;
}

async function loginAsAdmin(page: Page, fixture: ParcelFixture) {
  await page.goto("/login");
  await page.getByLabel(/email/i).fill(fixture.adminEmail);
  await page.getByLabel(/password/i).fill(fixture.adminPassword);
  await page.getByRole("button", { name: /^login$/i }).click();
  await expect(page).toHaveURL(/\/dashboard$/, { timeout: 15_000 });
}

async function selectDepot(page: Page, depotName: string) {
  await page.locator("#shipperAddressId").click();
  await page.getByRole("option", { name: depotName, exact: true }).click();
}

async function registerParcel(
  page: Page,
  fixture: ParcelFixture,
  suffix: string,
): Promise<RegisteredParcelFixture> {
  await expect(page.getByRole("heading", { name: /register parcel/i })).toBeVisible();

  await selectDepot(page, fixture.depotName);
  await page.getByLabel(/street address/i).first().fill(`${suffix} Market Street`);
  await page.getByLabel(/^city/i).fill("Sydney");
  await page.getByLabel(/state \/ province/i).fill("NSW");
  await page.getByLabel(/postal code/i).fill("2000");
  await page.getByLabel(/country code/i).fill("AU");
  await page.getByLabel(/recipient name/i).fill(`Jamie ${suffix}`);
  await page.getByLabel(/company name/i).fill("Parcel E2E Pty");
  await page.getByLabel(/phone/i).fill("+61123456789");
  await page.getByLabel(/email/i).fill(`parcel-${suffix.toLowerCase()}@lastmile.test`);
  await page.getByLabel(/parcel type/i).fill("Box");
  await page.getByLabel(/notes \/ description/i).fill(`Handle ${suffix} parcel with care`);
  await page.getByLabel(/est\. delivery date/i).fill(getFutureDate());

  await page.getByRole("button", { name: /register parcel/i }).click();
  await expect(page.getByRole("heading", { name: /parcel registered/i })).toBeVisible({
    timeout: 15_000,
  });

  const trackingNumber = (await page
    .locator("p", { hasText: /^Tracking Number$/ })
    .locator("xpath=following-sibling::p[1]")
    .textContent())?.trim();
  const zoneName = (await page
    .locator("p", { hasText: /^Zone$/ })
    .locator("xpath=following-sibling::p[1]")
    .textContent())?.trim();

  expect(trackingNumber).toBeTruthy();
  expect(zoneName).toBeTruthy();
  await expect(page.getByText(zoneName!, { exact: true })).toBeVisible();

  return {
    trackingNumber: trackingNumber!,
    zoneName: zoneName!,
  };
}

async function expectFileResponse(
  page: Page,
  trigger: () => Promise<void>,
  options: {
    method?: "GET" | "POST";
    pathPattern: RegExp;
    contentTypePattern: RegExp;
    fileNamePattern: RegExp;
  },
) {
  const responsePromise = page.waitForResponse((response) => {
    const request = response.request();
    const pathname = new URL(response.url()).pathname;

    return (
      request.method() === (options.method ?? "GET") &&
      options.pathPattern.test(pathname)
    );
  });

  await trigger();
  const response = await responsePromise;

  await expect(response.ok()).toBeTruthy();
  await expectFileHeaders(response, options.contentTypePattern, options.fileNamePattern);
}

async function expectFileHeaders(
  response: Response,
  contentTypePattern: RegExp,
  fileNamePattern: RegExp,
) {
  const headers = response.headers();
  const contentType = headers["content-type"] ?? "";
  const contentDisposition = headers["content-disposition"] ?? "";

  await expect(contentType).toMatch(contentTypePattern);
  await expect(contentDisposition).toMatch(fileNamePattern);
}

test.describe("Parcel label flow", () => {
  test.describe.configure({ mode: "serial" });

  test("can register a parcel and reprint labels from success and detail views", async ({
    page,
    request,
  }) => {
    const fixture = await resetAndSeedFixture(request);

    await loginAsAdmin(page, fixture);
    await page.goto("/parcels");
    await expect(
      page.getByRole("heading", { name: /warehouse intake queue/i }),
    ).toBeVisible();

    await page.getByRole("button", { name: /register parcel/i }).click();

    const { trackingNumber, zoneName } = await registerParcel(page, fixture, "Alpha");

    await expectFileResponse(
      page,
      () => page.getByRole("button", { name: /download 4x6 zpl/i }).click(),
      {
        pathPattern: /\/api\/parcels\/[^/]+\/labels\/4x6\.zpl$/,
        contentTypePattern: /^text\/plain/i,
        fileNamePattern: new RegExp(`parcel-${trackingNumber}\\.zpl`),
      },
    );

    await expectFileResponse(
      page,
      () => page.getByRole("button", { name: /download a4 pdf/i }).click(),
      {
        pathPattern: /\/api\/parcels\/[^/]+\/labels\/a4\.pdf$/,
        contentTypePattern: /^application\/pdf$/i,
        fileNamePattern: new RegExp(`parcel-${trackingNumber}-a4\\.pdf`),
      },
    );

    await page.getByRole("button", { name: /open parcel detail/i }).click();
    await expect(page).toHaveURL(/\/parcels\/[0-9a-f-]+$/, { timeout: 15_000 });
    await expect(page.getByRole("heading", { name: trackingNumber })).toBeVisible();
    await expect(page.getByText(zoneName, { exact: true }).first()).toBeVisible();
    await expect(page.getByText("Jamie Alpha", { exact: true })).toBeVisible();
    await expect(page.getByText("Parcel E2E Pty", { exact: true })).toBeVisible();

    await expectFileResponse(
      page,
      () => page.getByRole("button", { name: /download a4 pdf/i }).click(),
      {
        pathPattern: /\/api\/parcels\/[^/]+\/labels\/a4\.pdf$/,
        contentTypePattern: /^application\/pdf$/i,
        fileNamePattern: new RegExp(`parcel-${trackingNumber}-a4\\.pdf`),
      },
    );
  });

  test("can bulk download labels for multiple registered parcels from the intake queue", async ({
    page,
    request,
  }) => {
    const fixture = await resetAndSeedFixture(request);

    await loginAsAdmin(page, fixture);
    await page.goto("/parcels");
    await expect(
      page.getByRole("heading", { name: /warehouse intake queue/i }),
    ).toBeVisible();

    await page.getByRole("button", { name: /register parcel/i }).click();
    const firstParcel = await registerParcel(page, fixture, "Alpha");

    await page.getByRole("button", { name: /register another/i }).click();
    const secondParcel = await registerParcel(page, fixture, "Beta");

    await page.getByRole("button", { name: /view intake queue/i }).click();
    await expect(page).toHaveURL(/\/parcels$/, { timeout: 15_000 });

    await expect(page.getByRole("link", { name: firstParcel.trackingNumber })).toBeVisible();
    await expect(page.getByRole("link", { name: secondParcel.trackingNumber })).toBeVisible();

    await page.getByLabel(`Select parcel ${firstParcel.trackingNumber}`).check();
    await page.getByLabel(`Select parcel ${secondParcel.trackingNumber}`).check();

    const selectedText = page.getByText(/2 selected:/i);
    await expect(selectedText).toContainText(firstParcel.trackingNumber);
    await expect(selectedText).toContainText(secondParcel.trackingNumber);

    await expectFileResponse(
      page,
      () => page.getByRole("button", { name: /^download 4x6 zpl$/i }).click(),
      {
        method: "POST",
        pathPattern: /\/api\/parcels\/labels\/4x6\.zpl$/,
        contentTypePattern: /^text\/plain/i,
        fileNamePattern: /parcel-labels-4x6\.zpl/,
      },
    );

    await expectFileResponse(
      page,
      () => page.getByRole("button", { name: /^download a4 pdf$/i }).click(),
      {
        method: "POST",
        pathPattern: /\/api\/parcels\/labels\/a4\.pdf$/,
        contentTypePattern: /^application\/pdf$/i,
        fileNamePattern: /parcel-labels-a4\.pdf/,
      },
    );
  });
});
