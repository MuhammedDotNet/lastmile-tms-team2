import path from "node:path";
import { fileURLToPath } from "node:url";
import type { NextConfig } from "next";

const projectRoot = path.dirname(fileURLToPath(import.meta.url));

/** Allow next/image to load driver photos from the API host (NEXT_PUBLIC_API_URL). */
function apiImageRemotePatterns(): NonNullable<
  NextConfig["images"]
>["remotePatterns"] {
  const patterns: NonNullable<
    NextConfig["images"]
  >["remotePatterns"] = [
    { protocol: "http", hostname: "localhost", pathname: "/uploads/**" },
    { protocol: "https", hostname: "localhost", pathname: "/uploads/**" },
    { protocol: "http", hostname: "127.0.0.1", pathname: "/uploads/**" },
    { protocol: "https", hostname: "127.0.0.1", pathname: "/uploads/**" },
    { protocol: "http", hostname: "localhost", pathname: "/api/drivers/photo/**" },
    { protocol: "https", hostname: "localhost", pathname: "/api/drivers/photo/**" },
    { protocol: "http", hostname: "127.0.0.1", pathname: "/api/drivers/photo/**" },
    { protocol: "https", hostname: "127.0.0.1", pathname: "/api/drivers/photo/**" },
  ];

  const url = process.env.NEXT_PUBLIC_API_URL;
  if (url) {
    try {
      const u = new URL(url);
      patterns.push({
        protocol: u.protocol === "https:" ? "https" : "http",
        hostname: u.hostname,
        ...(u.port ? { port: u.port } : {}),
        pathname: "/uploads/**",
      });
      patterns.push({
        protocol: u.protocol === "https:" ? "https" : "http",
        hostname: u.hostname,
        ...(u.port ? { port: u.port } : {}),
        pathname: "/api/drivers/photo/**",
      });
    } catch {
      // ignore invalid URL at build time
    }
  }

  return patterns;
}

const nextConfig: NextConfig = {
  reactCompiler: true,
  output: "standalone",
  outputFileTracingRoot: projectRoot,
  turbopack: {
    root: projectRoot,
  },
  images: {
    remotePatterns: apiImageRemotePatterns(),
  },
};

export default nextConfig;
