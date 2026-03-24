import type { ReactNode } from "react";

import {
  DetailContainer,
  DetailShell,
  DETAIL_PAGE_CONTENT_PADDING,
  type DetailSection,
} from "./detail-layout";
import { cn } from "@/lib/utils";

/**
 * Narrow column for form panels + actions only.
 * Breadcrumb and `ListPageHeader` sit above at full dashboard content width (same as list pages).
 */
export const FORM_PAGE_FORM_COLUMN_CLASS = "mx-auto w-full max-w-3xl";

export function DetailFormPageShell({
  children,
  className,
  variant = "neutral",
}: {
  children: ReactNode;
  className?: string;
  /** Fleet (teal) / dispatch (violet) / generic — matches detail views. */
  variant?: DetailSection;
}) {
  return (
    <DetailShell variant={variant}>
      <DetailContainer className={DETAIL_PAGE_CONTENT_PADDING}>
        <div className={cn("w-full", className)}>{children}</div>
      </DetailContainer>
    </DetailShell>
  );
}
