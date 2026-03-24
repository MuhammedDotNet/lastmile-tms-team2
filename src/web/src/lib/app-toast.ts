import { toast as sonnerToast } from "sonner";

import { getErrorMessage } from "./error-message";

type SonnerErrorOptions = NonNullable<Parameters<typeof sonnerToast.error>[1]>;

const errorToastLayout = {
  duration: 12_000,
  classNames: {
    toast: "!items-start !w-[min(100vw-2rem,28rem)]",
    title: "whitespace-pre-wrap break-words text-left !leading-snug",
    description: "whitespace-pre-wrap break-words text-left !leading-snug",
  },
} satisfies SonnerErrorOptions;

export const appToast = {
  error(message: string, options?: SonnerErrorOptions) {
    return sonnerToast.error(message, { ...errorToastLayout, ...options });
  },

  errorFromUnknown(error: unknown) {
    const message = getErrorMessage(error);
    return sonnerToast.error(message, errorToastLayout);
  },

  success(title: string, options?: { description?: string }) {
    return sonnerToast.success(
      title,
      options?.description ? { description: options.description } : undefined,
    );
  },
};
