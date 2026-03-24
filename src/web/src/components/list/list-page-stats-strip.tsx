import { BarChart3, Layers, ListFilter, Rows3 } from "lucide-react";

import { ListStatCard } from "./list-stat-card";

export function ListPageStatsStrip({
  totalLabel,
  totalCount,
  rangeEntityLabel,
  from,
  to,
  page,
  totalPages,
  pageSize,
  filterCardLabel,
  filterCardHint,
  activeFilterDisplay,
}: {
  totalLabel: string;
  totalCount: number;
  rangeEntityLabel: string;
  from: number;
  to: number;
  page: number;
  totalPages: number;
  pageSize: number;
  filterCardLabel: string;
  filterCardHint: string;
  activeFilterDisplay: string;
}) {
  return (
    <div className="list-page-stats-animate relative mb-8">
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4 lg:gap-4">
        <ListStatCard
          label={totalLabel}
          accent="teal"
          icon={<BarChart3 strokeWidth={1.75} />}
        >
          <p className="text-[1.65rem] font-semibold tabular-nums tracking-tight text-foreground">
            {totalCount.toLocaleString()}
          </p>
        </ListStatCard>
        <ListStatCard
          label="Showing"
          accent="sky"
          icon={<Rows3 strokeWidth={1.75} />}
          hint={`of ${totalCount.toLocaleString()} ${rangeEntityLabel}`}
        >
          <p className="text-[1.65rem] font-semibold tabular-nums tracking-tight text-foreground">
            {totalCount === 0 ? "—" : `${from}–${to}`}
          </p>
        </ListStatCard>
        <ListStatCard
          label="Page"
          accent="amber"
          icon={<Layers strokeWidth={1.75} />}
          hint={`${pageSize} per page`}
        >
          <p className="text-[1.65rem] font-semibold tabular-nums tracking-tight text-foreground">
            {page}
            <span className="text-lg font-medium text-muted-foreground/90">
              {" "}
              / {totalPages}
            </span>
          </p>
        </ListStatCard>
        <ListStatCard
          label={filterCardLabel}
          accent="violet"
          icon={<ListFilter strokeWidth={1.75} />}
          hint={filterCardHint}
        >
          <p className="text-sm font-semibold leading-snug text-foreground">
            {activeFilterDisplay}
          </p>
        </ListStatCard>
      </div>
    </div>
  );
}
