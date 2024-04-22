import { classNames } from "@shipmvp/lib";

export function Label(props: JSX.IntrinsicElements["label"]) {
  return (
    <label
      {...props} // Remove block HNH remove css label whitespace-nowrap
      className={classNames(
        "text-default text-emphasis mb-2  block whitespace-nowrap text-sm font-medium",
        props.className
      )}>
      {props.children}
    </label>
  );
}
