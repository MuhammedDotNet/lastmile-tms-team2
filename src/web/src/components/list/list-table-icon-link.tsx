import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

/** Ghost icon-sized control used for “view” links in data tables. */
export const listTableIconLinkClass = cn(
  buttonVariants({ variant: "ghost", size: "icon-sm" }),
  "shrink-0 text-muted-foreground hover:bg-muted hover:text-foreground",
);
