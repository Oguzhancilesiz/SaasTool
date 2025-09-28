"use client";

import { PropsWithChildren } from "react";
import { Toaster } from "sonner";
import { Provider as JotaiProvider } from "jotai";

export default function Providers({ children }: PropsWithChildren) {
  return (
    <JotaiProvider>
      {children}
      <Toaster richColors />
    </JotaiProvider>
  );
}
