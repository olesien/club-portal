import Dialog from "@mui/material/Dialog";
import { BootstrapDialogTitle } from "../../../components/BootstrapDialogTitle";
import { translate, translateText } from "../../../i18n";
import DialogContent from "@mui/material/DialogContent";
import Button from "@mui/material/Button";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { useState } from "react";
import axios from "axios";
import api, { getTemplates, Templates } from "../../../api";
import { toast } from "react-toastify";
import TextField from "@mui/material/TextField";
import BasicDateTimePicker from "../../../components/BasicDateTimePicker";
import dayjs from "dayjs";
import TipTapEditor from "../../../components/TipTapEditor";
import { useQuery } from "@tanstack/react-query";
import Autocomplete from "@mui/material/Autocomplete";
export type NewMeeting = {
    name: string;
    location: string;
    time: Date;
    description: string;
    agenda: string;
    meetingProtocols: string;
}
// };
export default function AddMeetingModal({ handleClose, refetch, clubId }: { handleClose: () => void; refetch: () => void; clubId: number }) {
    const {
        control,
        register,
        handleSubmit,
        formState: { errors },
        setValue
    } = useForm<NewMeeting>({
        defaultValues: {
            time: new Date(Date.now())
        },
    });
    const { data: templates } = useQuery({
        queryKey: ['club-templates'],
        queryFn: () => getTemplates(),
    });
    const [loading, setLoading] = useState(false);
    const onSubmit: SubmitHandler<NewMeeting> = async (data: NewMeeting) => {
        setLoading(true);
        try {
            const res = await api.post<unknown>
                ("/meeting/" + clubId, {
                    name: data.name,
                    description: data.description,
                    meetingTime: data.time,
                    location: data.location,
                    agenda: data.agenda,
                    meetingProtocol: data.meetingProtocols
                }, {
                    headers: {
                        "Access-Control-Allow-Origin": "*"
                    },
                    withCredentials: true
                });
            const resData = res.data;
            toast.success(translate["meeting_created_success"]);
            refetch();
            handleClose();
            console.log(resData);
        } catch (err) {
            if (axios.isAxiosError(err)) {
                if (err.response?.data) {
                    toast.error(translateText(err.response?.data?.title, err.response?.data?.title));
                } else {
                    toast.error(err.message);
                }
            } else {
                toast.error(translate["something_went_wrong"])
            }
        }
        setLoading(false);
    }
    const changeField = (id: keyof NewMeeting, template: Templates | null) => {
        if (template) {
            setValue(id, template.markdown);
        }
    }
    if (!templates) return <></>
    return (
        <Dialog
            open={true}
            // onClose={handleClose}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
            fullWidth
            maxWidth="sm"
        >
            <BootstrapDialogTitle
                id="alert-dialog-title"
                onClose={() => handleClose()}
            >
                {translate["add_meeting"]}
            </BootstrapDialogTitle>

            <DialogContent>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <TextField
                        fullWidth={true}
                        error={!!errors.name}
                        id="meeting_title"
                        label={translate["enter_title"]}
                        type="text"
                        variant="standard"
                        helperText={errors.name ? errors?.name.message : " "}
                        {...register("name", { required: true })}
                    />
                    <TextField
                        fullWidth={true}
                        error={!!errors.location}
                        id="meeting_location"
                        label={translate["enter_location"]}
                        type="text"
                        variant="standard"
                        helperText={errors.location ? errors?.location.message : " "}
                        {...register("location", { required: true })}
                    />
                    <TextField
                        fullWidth={true}
                        error={!!errors.description}
                        id="meeting_description"
                        label={translate["description"]}
                        type="text"
                        variant="standard"
                        helperText={errors.description ? errors?.description.message : " "}
                        {...register("description", { required: true })}
                    />
                    <Controller
                        name="agenda"
                        control={control}
                        rules={{ required: translate["agenda_required"] }}
                        render={({ field: { onChange, value } }) => (
                            <TipTapEditor content={value} label={translate["agenda"]} onChange={onChange} />

                        )}
                    />
                    <div className="dropdown-spacing">
                        <Autocomplete
                            onChange={(_v, r) => changeField("agenda", r)}
                            disablePortal
                            options={templates}
                            sx={{ width: 300 }}
                            getOptionLabel={(v) => v.title}
                            renderInput={(params) => <TextField {...params} label={translate["choose_agenda_template"]} />}
                        />
                    </div>
                    <Controller

                        name="meetingProtocols"
                        control={control}
                        render={({ field: { onChange, value } }) => (
                            <TipTapEditor content={value} label={translate["meeting_protocols"]} onChange={onChange} />

                        )}
                    />
                    <div className="dropdown-spacing">
                        <Autocomplete
                            onChange={(_v, r) => changeField("meetingProtocols", r)}
                            disablePortal
                            options={templates}
                            sx={{ width: 300 }}
                            getOptionLabel={(v) => v.title}
                            renderInput={(params) => <TextField {...params} label={translate["choose_meeting_protocol_template"]} />}
                        />
                    </div>


                    <Controller
                        name="time"
                        control={control}
                        rules={{ required: translate["meeting_time_required"] }}
                        render={({ field: { onChange, value } }) => (
                            <BasicDateTimePicker
                                error={errors.location ? errors?.location.message : undefined}
                                label={translate["meeting_time"]}
                                value={dayjs(value)}
                                onChange={(v) => onChange(v?.toDate())}
                            />
                        )}
                    />
                    <div className="align-center">
                        <Button sx={{ marginTop: "10px" }} type="submit" variant="contained" disabled={loading}>{loading ? translate["adding"] : translate["add"]}</Button>
                    </div>
                </form>


            </DialogContent>
            {/* <DialogActions>
                <Button
                    onClick={() => save({ title: "", content: "" })}
                >
                    Confirm
                </Button>
            </DialogActions> */}
        </Dialog>
    )
}