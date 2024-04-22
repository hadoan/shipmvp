import type {
  QueryObserverLoadingErrorResult,
  QueryObserverLoadingResult,
  QueryObserverRefetchErrorResult,
  QueryObserverSuccessResult,
  UseQueryResult,
} from "@tanstack/react-query";
import type { ReactNode } from "react";

import type { TRPCClientErrorLike } from "@shipmvp/trpc/client";
import type { DecorateProcedure } from "@shipmvp/trpc/react/shared";
import type { AnyQueryProcedure, inferProcedureInput, inferProcedureOutput } from "@shipmvp/trpc/server";
import type { AppRouter } from "@shipmvp/trpc/server/routers/_app";
import { Alert, Loader } from "@shipmvp/ui";

import type { UseTRPCQueryOptions } from "@trpc/react-query/shared";

type ErrorLike = {
  message: string;
};
type JSXElementOrNull = JSX.Element | null;

interface QueryCellOptionsBase<TData, TError extends ErrorLike> {
  query: UseQueryResult<TData, TError>;
  customLoader?: ReactNode;
  error?: (
    query: QueryObserverLoadingErrorResult<TData, TError> | QueryObserverRefetchErrorResult<TData, TError>
  ) => JSXElementOrNull;
  loading?: (query: QueryObserverLoadingResult<TData, TError> | null) => JSXElementOrNull;
}

interface QueryCellOptionsNoEmpty<TData, TError extends ErrorLike>
  extends QueryCellOptionsBase<TData, TError> {
  success: (query: QueryObserverSuccessResult<TData, TError>) => JSXElementOrNull;
}

interface QueryCellOptionsWithEmpty<TData, TError extends ErrorLike>
  extends QueryCellOptionsBase<TData, TError> {
  success: (query: QueryObserverSuccessResult<NonNullable<TData>, TError>) => JSXElementOrNull;
  /**
   * If there's no data (`null`, `undefined`, or `[]`), render this component
   */
  empty: (query: QueryObserverSuccessResult<TData, TError>) => JSXElementOrNull;
}

export function QueryCell<TData, TError extends ErrorLike>(
  opts: QueryCellOptionsWithEmpty<TData, TError>
): JSXElementOrNull;
export function QueryCell<TData, TError extends ErrorLike>(
  opts: QueryCellOptionsNoEmpty<TData, TError>
): JSXElementOrNull;

/** @deprecated Use `trpc.useQuery` instead. */
export function QueryCell<TData, TError extends ErrorLike>(
  opts: QueryCellOptionsNoEmpty<TData, TError> | QueryCellOptionsWithEmpty<TData, TError>
) {
  const { query } = opts;
  const StatusLoader = opts.customLoader || <Loader />; // Fixes edge case where this can return null form query cell
  console.log(query.status);
  if (query.status === "loading") {
    return opts.loading?.(query.status === "loading" ? query : null) ?? StatusLoader;
  }

  if (query.status === "success") {
    if ("empty" in opts && (query.data == null || (Array.isArray(query.data) && query.data.length === 0))) {
      return opts.empty(query);
    }
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const data = opts.success(query as any);
    console.log(data);
    return data;
  }

  if (query.status === "error") {
    return (
      opts.error?.(query) ?? (
        <Alert severity="error" title="Something went wrong" message={query.error.message} />
      )
    );
  }

  // impossible state
  return null;
}

type TError = TRPCClientErrorLike<AppRouter>;

const withQuery = <
  TQuery extends AnyQueryProcedure,
  TInput = inferProcedureInput<TQuery>,
  TOutput = inferProcedureOutput<TQuery>
>(
  queryProcedure: DecorateProcedure<TQuery, inferProcedureInput<TQuery>, inferProcedureOutput<TQuery>>,

  input?: TInput,
  params?: UseTRPCQueryOptions<TQuery, TInput, TOutput, TOutput, TError>
) => {
  return function WithQuery(
    opts: Omit<
      Partial<QueryCellOptionsWithEmpty<TOutput, TError>> & QueryCellOptionsNoEmpty<TOutput, TError>,
      "query"
    >
  ) {
    const query = queryProcedure.useQuery(input, params);
    return <QueryCell query={query} {...opts} />;
  };
};

export { withQuery };
