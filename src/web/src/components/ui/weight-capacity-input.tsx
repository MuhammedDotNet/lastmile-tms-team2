"use client";

import * as React from "react";

import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { sanitizePositiveDecimalInput } from "@/lib/validation/positive-decimal";

export type WeightCapacityInputProps = Omit<
  React.ComponentProps<typeof Input>,
  "type" | "value" | "onChange" | "inputMode"
> & {
  value: string;
  onChange: (raw: string) => void;
};

/** Digits and one decimal point; value is the raw string (parse on submit). */
export const WeightCapacityInput = React.forwardRef<
  HTMLInputElement,
  WeightCapacityInputProps
>(function WeightCapacityInput({ value, onChange, className, ...rest }, ref) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(sanitizePositiveDecimalInput(e.target.value));
  };

  return (
    <Input
      ref={ref}
      type="text"
      inputMode="decimal"
      autoComplete="off"
      className={cn(className)}
      value={value}
      onChange={handleChange}
      {...rest}
    />
  );
});
