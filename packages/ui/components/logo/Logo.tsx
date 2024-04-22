import classNames from "@shipmvp/lib/classNames";

export default function Logo({
  small,
  icon,
  inline = true,
  className,
}: {
  small?: boolean;
  icon?: boolean;
  inline?: boolean;
  className?: string;
}) {
  return (
    <h3 className={classNames("logo", inline && "inline", className)}>
      <strong>
        {icon ? (
          <img
            className="mx-auto w-9 dark:invert"
            alt="Tekfriend"
            title="Tekfriend"
            src="/api/logo?type=icon"
          />
        ) : (
          <img
            className={classNames(small ? "h-4 w-auto" : "h-5 w-auto", "dark:invert")}
            alt="Tekfriend"
            title="Tekfriend"
            src="/api/logo"
          />
        )}
      </strong>
    </h3>
  );
}
