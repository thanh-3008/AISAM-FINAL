import type { Metadata } from "next";
import { ToastViewport } from "@/components/feedback/toast-viewport";
import { AppQueryProvider } from "@/lib/query/providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "AISAM FE",
  description: "Frontend foundation for AISAM"
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <AppQueryProvider>
          {children}
          <ToastViewport />
        </AppQueryProvider>
      </body>
    </html>
  );
}
