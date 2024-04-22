const StepCard: React.FC<{ children: React.ReactNode }> = (props) => {
  return (
    <div className="border-subtle bg-default mt-10  rounded-md border p-4 sm:p-8 dark:bg-black">
      {props.children}
    </div>
  );
};

export { StepCard };
