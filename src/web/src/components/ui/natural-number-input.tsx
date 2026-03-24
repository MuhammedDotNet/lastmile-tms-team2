"use client";

import * as React from "react";

import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import {
  formatNaturalIntForInput,
  parseNaturalIntInput,
} from "@/lib/validation/natural-number";

export type NaturalNumberInputProps = Omit<
  React.ComponentProps<typeof Input>,
  "type" | "value" | "onChange" | "inputMode"
> & {
  value: number;
  onChange: (value: number) => void;
  max?: number;
  /** Show blank instead of `0` when value is 0 (e.g. parcel capacity before input). */
  treatZeroAsEmpty?: boolean;
};

export const NaturalNumberInput = React.forwardRef<
  HTMLInputElement,
  NaturalNumberInputProps
>(function NaturalNumberInput(
  {
    value,
    onChange,
    max,
    treatZeroAsEmpty = false,
    className,
    ...rest
  },
  ref,
) {
  const shown =
    treatZeroAsEmpty && value === 0 ? "" : formatNaturalIntForInput(value);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const parsed = parseNaturalIntInput(e.target.value, { min: 0, max });
    if (parsed === null) {
      onChange(0);
      return;
    }
    onChange(parsed);
  };

  return (
    <Input
      ref={ref}
      type="text"
      inputMode="numeric"
      autoComplete="off"
      className={cn(className)}
      value={shown}
      onChange={handleChange}
      {...rest}
    />
  );
});
