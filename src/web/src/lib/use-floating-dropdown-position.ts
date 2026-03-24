"use client";

import { useLayoutEffect, useState, type RefObject } from "react";

export type FloatingDropdownPosition = {
  top: number;
  left: number;
  width: number;
};

/**
 * Positions a fixed dropdown below the trigger (viewport coords).
 * Updates on scroll (capture), resize, and trigger resize.
 */
export function useFloatingDropdownPosition(
  open: boolean,
  triggerRef: RefObject<HTMLElement | null>,
  options?: { minWidth?: number },
): FloatingDropdownPosition | null {
  const [pos, setPos] = useState<FloatingDropdownPosition | null>(null);
  const minWidth = options?.minWidth ?? 0;

  useLayoutEffect(() => {
    if (!open || !triggerRef.current) {
      return;
    }

    const el = triggerRef.current;

    const update = () => {
      const r = el.getBoundingClientRect();
      const width = Math.max(r.width, minWidth);
      let left = r.left;
      const pad = 8;
      if (left + width > window.innerWidth - pad) {
        left = Math.max(pad, window.innerWidth - width - pad);
      }
      setPos({
        top: r.bottom + 6,
        left,
        width,
      });
    };

    update();

    window.addEventListener("resize", update);
    window.addEventListener("scroll", update, true);
    const ro = new ResizeObserver(update);
    ro.observe(el);

    return () => {
      window.removeEventListener("resize", update);
      window.removeEventListener("scroll", update, true);
      ro.disconnect();
    };
  }, [open, triggerRef, minWidth]);

  return pos;
}
