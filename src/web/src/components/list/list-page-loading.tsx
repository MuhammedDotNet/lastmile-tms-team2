export function ListPageLoading() {
  return (
    <div
      className="space-y-8"
      aria-busy="true"
      aria-label="Loading page"
    >
      <div className="animate-pulse overflow-hidden rounded-2xl border border-border/40 bg-muted/30 p-8 dark:bg-muted/20">
        <div className="mb-4 h-3 w-24 rounded-full bg-muted-foreground/15" />
        <div className="mb-3 h-9 max-w-md rounded-lg bg-muted-foreground/20" />
        <div className="h-4 max-w-lg rounded bg-muted-foreground/12" />
      </div>
      <div className="grid animate-pulse gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div
            key={i}
            className="h-28 rounded-xl border border-border/30 bg-muted/25 dark:bg-muted/15"
          />
        ))}
      </div>
      <div className="h-12 max-w-xs animate-pulse rounded-lg bg-muted/30" />
      <div className="animate-pulse overflow-hidden rounded-2xl border border-border/40">
        <div className="h-10 border-b border-border/40 bg-muted/35" />
        <div className="space-y-0 divide-y divide-border/35">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-12 bg-muted/10" />
          ))}
        </div>
      </div>
    </div>
  );
}
