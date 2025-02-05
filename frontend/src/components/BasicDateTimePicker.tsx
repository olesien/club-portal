import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { Dayjs } from "dayjs";
import { DateOrTimeView } from "@mui/x-date-pickers/models";
import { renderTimeViewClock } from "@mui/x-date-pickers/timeViewRenderers";

export default function BasicDateTimePicker({
    value,
    onChange,
    label,
    error,
    views = ['year', 'day', 'hours', 'minutes']
}: {
    value: Dayjs | null;
    onChange: (newDate: Dayjs | null) => void;
    label: string;
    error?: string | null;
    views?: DateOrTimeView[]
}) {
    return (
        <div className="pt-1">
            <p className="error text-right">{!!error && error}</p>
            <div className="align-left">
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                    <DateTimePicker
                        orientation="landscape"
                        label={label}
                        // sx={{ padding: 0 }}
                        value={value}
                        onChange={(e) => onChange(e)}
                        views={views}
                        viewRenderers={{
                            hours: renderTimeViewClock,
                            minutes: renderTimeViewClock,
                            seconds: renderTimeViewClock,
                        }}
                    />
                </LocalizationProvider>
            </div>
        </div>
    );
}
