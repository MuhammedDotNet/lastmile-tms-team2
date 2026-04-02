import { getSession } from "next-auth/react";

import { absoluteApiAssetUrl, apiBaseUrl, parseApiErrorMessage } from "@/lib/network/api";

/**
 * Uploads an image to the API and returns an absolute URL suitable for `Driver.photoUrl`.
 */
export async function uploadDriverPhoto(file: File): Promise<string> {
  const session = await getSession();
  const token = session?.accessToken;
  const form = new FormData();
  form.append("file", file);

  const res = await fetch(`${apiBaseUrl().replace(/\/$/, "")}/api/drivers/photo`, {
    method: "POST",
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: form,
  });

  if (!res.ok) {
    throw new Error(await parseApiErrorMessage(res));
  }

  const data = (await res.json()) as { url?: string };
  if (!data.url?.trim()) {
    throw new Error("Upload succeeded but no URL was returned.");
  }

  return absoluteApiAssetUrl(data.url)!;
}
