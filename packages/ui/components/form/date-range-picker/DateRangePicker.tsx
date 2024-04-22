// @see: https://github.com/wojtekmaj/react-daterange-picker/issues/91
// import "@wojtekmaj/react-daterange-picker/dist/DateRangePicker.css";
import * as picker from "@wojtekmaj/react-daterange-picker";

import { ArrowRight, Calendar, ChevronLeft, ChevronRight } from "../../icon";
import "./styles.css";

type Props = {
  disabled?: boolean | undefined;
  startDate: Date;
  endDate: Date;
  onDatesChange?: ((arg: { startDate: Date; endDate: Date }) => void) | undefined;
};

const DateRangePicker = ({ disabled, startDate, endDate, onDatesChange }: Props) => {
  return (
    <>
      <picker.DateRangePicker
        disabled={disabled || false}
        className="border-default rounded-sm text-sm"
        clearIcon={null}
        calendarIcon={<Calendar className="text-subtle h-4 w-4" />}
        rangeDivider={<ArrowRight className="text-muted h-4 w-4 ltr:mr-2 rtl:ml-2" />}
        value={[startDate, endDate]}
        onChange={(): void => {
          if (typeof onDatesChange === "function") onDatesChange({ startDate, endDate });
        }}
        nextLabel={<ChevronRight className="text-subtle h-4 w-4" />}
        prevLabel={<ChevronLeft className="text-subtle h-4 w-4" />}
      />
    </>
  );
};

export default DateRangePicker;
