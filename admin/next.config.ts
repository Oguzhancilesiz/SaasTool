import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Build’te ESLint’i ve TS hatalarını bloklamasın (demo/deploy için)
  eslint: { ignoreDuringBuilds: true },
  typescript: { ignoreBuildErrors: true },
};

export default nextConfig;
