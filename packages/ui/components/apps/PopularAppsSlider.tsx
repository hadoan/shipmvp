import { useLocale } from "@shipmvp/lib/hooks/useLocale";
import type { AppFrontendPayload as App } from "@shipmvp/types/App";

import { AppCard } from "./AppCard";
import { Slider } from "./Slider";

export const PopularAppsSlider = <T extends App>({ items }: { items: T[] }) => {
  const { t } = useLocale();

  return (
    <Slider<T>
      title={t("most_popular")}
      items={items.sort((a, b) => (b.installCount || 0) - (a.installCount || 0))}
      itemKey={(app) => app.name}
      renderItem={(app) => <AppCard app={app} />}
    />
  );
};
