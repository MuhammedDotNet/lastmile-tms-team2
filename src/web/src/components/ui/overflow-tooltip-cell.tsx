"use client";

import {
  useRef,
  useState,
  useLayoutEffect,
  type ReactNode,
} from "react";
import { Tooltip } from "radix-ui";

import { cn } from "@/lib/utils";

type OverflowTooltipCellProps = {
  /** Full string for tooltip when the cell is visually clipped */
  fullText: string;
  className?: string;
  /** Visible (truncated) content; defaults to fullText */
  children?: ReactNode;
  /**
   * When true, width follows content (e.g. badges) instead of stretching to the cell.
   */
  shrinkToContent?: boolean;
};

/**
 * Truncates with ellipsis; shows a Radix tooltip with fullText only when
 * content overflows the cell (resize-safe).
 */
export function OverflowTooltipCell({
  fullText,
  className,
  children,
  shrinkToContent = false,
}: OverflowTooltipCellProps) {
  const ref = useRef<HTMLDivElement>(null);
  const [overflow, setOverflow] = useState(false);

  useLayoutEffect(() => {
    const el = ref.current;
    if (!el) return;
    const measure = () => {
      setOverflow(el.scrollWidth > el.clientWidth + 1);
    };
    measure();
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    return () => ro.disconnect();
  }, [fullText]);

  const inner = (
    <div
      ref={ref}
      className={cn(
        "min-w-0 max-w-full truncate",
        shrinkToContent ? "w-fit" : "w-full",
        className
      )}
    >
      {children ?? fullText}
    </div>
  );

  if (!overflow) {
    return inner;
  }

  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>{inner}</Tooltip.Trigger>
      <Tooltip.Portal>
        <Tooltip.Content
          side="top"
          sideOffset={6}
          className="z-50 max-w-sm rounded-md border border-border bg-popover px-2.5 py-1.5 text-sm leading-snug text-popover-foreground shadow-md"
        >
          {fullText}
          <Tooltip.Arrow className="fill-popover" />
        </Tooltip.Content>
      </Tooltip.Portal>
    </Tooltip.Root>
  );
}
