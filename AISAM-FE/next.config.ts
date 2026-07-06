import path from "path";
import type { NextConfig } from "next";

const apiBaseUrl = process.env.BACKEND_API_URL || "http://127.0.0.1:5027";

const nextConfig: NextConfig = {
  turbopack: { root: path.resolve(__dirname) },

  allowedDevOrigins: [
    "upon-lyricist-bottle.ngrok-free.dev",
  ],

  async rewrites() {
    return [
      {
        source: "/backend-api/:path*",
        destination: `${apiBaseUrl.replace(/\/$/, "")}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;