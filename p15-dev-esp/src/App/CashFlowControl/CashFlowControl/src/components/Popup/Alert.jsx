import React, { useEffect, useState } from "react";
import { Snackbar, Alert } from "@mui/material";

const AlertPopup = ({ message, duration = 3000, onClose }) => {
    const [open, setOpen] = useState(false);

    useEffect(() => {
        if (message) {
            setOpen(true);

            const timer = setTimeout(() => {
                setOpen(false);
                if (onClose && typeof onClose === "function") {
                    onClose();
                }
            }, duration);

            return () => clearTimeout(timer);
        }
    }, [message, duration, onClose]);

    return (
        <Snackbar open={open} autoHideDuration={duration} onClose={() => setOpen(false)}>
            <Alert severity="error" sx={{ width: "100%" }} onClose={() => setOpen(false)}>
                {message}
            </Alert>
        </Snackbar>
    );
};

export default AlertPopup;
