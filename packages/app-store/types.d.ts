import type React from "react";
import type { z } from "zod";

import type { RouterOutputs } from "@shipmvp/trpc/react";
import type { ButtonProps } from "@shipmvp/ui";

export type IntegrationOAuthCallbackState = {
  returnTo: string;
  installGoogleVideo?: boolean;
};

type AppScript = { attrs?: Record<string, string> } & { src?: string; content?: string };

export type Tag = {
  scripts: AppScript[];
};

export interface InstallAppButtonProps {
  render: (
    renderProps: ButtonProps & {
      /** Tells that the default render component should be used */
      useDefaultComponent?: boolean;
    }
  ) => JSX.Element;
  onChanged?: () => unknown;
  disableInstall?: boolean;
}
