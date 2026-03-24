/**
 * Optional meta on React Query mutations; read by {@link makeQueryClient} MutationCache.
 */
export type MutationToastMeta = {
  successToast?: {
    title: string;
    description?: string;
    /** Prefer over `description` when the message depends on variables (e.g. plate). */
    describe?: (variables: unknown) => string | undefined;
  };
  /** When true, global error toast is skipped (use sparingly). */
  skipErrorToast?: boolean;
};
