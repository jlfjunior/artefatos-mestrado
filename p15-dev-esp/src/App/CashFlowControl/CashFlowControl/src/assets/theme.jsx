import { createTheme } from "@mui/material/styles";

const theme = createTheme({
    palette: {
        primary: { main: "#f2c94c" },
        secondary: { main: "#388E3C" },
        error: { main: "#D32F2F" },
    },
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    borderRadius: "6px",
                    padding: "8px 16px",
                    fontWeight: "bold"
                },
            },
        },
        MuiOutlinedInput: {
            styleOverrides: {
                root: {
                    "& fieldset": { borderColor: "#f2c94c" },
                    "&:hover fieldset": { borderColor: "#135BA1" },
                    "&.Mui-focused fieldset": { borderColor: "#388E3C" },
                },
            },
        },
    },
});

export default theme;
