import { AlertCircle } from "lucide-react";

type QueryErrorAlertProps = {
  message: string;
  /** Short heading above the detailed message */
  title?: string;
};

/**
 * Prominent error block for failed React Query fetches (GraphQL/API).
 */
export function QueryErrorAlert({
  message,
  title = "Could not load data",
}: QueryErrorAlertProps) {
  return (
    <div className="container mx-auto max-w-6xl px-6 py-10">
      <div
        role="alert"
        className="flex gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-red-950 shadow-sm dark:border-red-900/60 dark:bg-red-950/50 dark:text-red-50"
      >
        <AlertCircle
          className="mt-0.5 size-5 shrink-0 text-red-600 dark:text-red-400"
          aria-hidden
        />
        <div className="min-w-0">
          <p className="font-semibold text-red-900 dark:text-red-100">{title}</p>
          <p className="mt-1.5 text-sm leading-relaxed text-red-800 dark:text-red-200/95">
            {message}
          </p>
        </div>
      </div>
    </div>
  );
}
