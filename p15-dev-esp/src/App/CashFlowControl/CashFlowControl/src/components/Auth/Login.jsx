import React, { useState, useEffect } from 'react';
import { Avatar, Button, CssBaseline, TextField, Link, Grid, Box, Typography, Container } from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { login } from '../../services/authService';
import { useAuth } from '../../context/AuthContext.jsx';
import AlertPopup from "../Popup/Alert";
import { jwtDecode } from "jwt-decode";

const Login = () => {
    const [showAlert, setShowAlert] = useState(false);
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState(null);
    const [isAdmin, setIsAdmin] = useState(false);
    const { login: loginContext } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('jwt');
        if (token) {
            navigate('/launch');
        }
    }, [navigate]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        setShowAlert(false);
        try {
            const result = await login(username, password);
            localStorage.setItem('jwt', result.token);

            const decodedToken = jwtDecode(result.token);
            const roles = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

            if (roles === "Admin") {
                setIsAdmin(true);
            } else {
                setIsAdmin(false);
            }

            loginContext(result.token);
            navigate('/launch');
        } catch (err) {
            setShowAlert(true);
            setError(err?.message || String(err) || "An unexpected error occurred.");
        }
    };

    return (
        <Container component="main" maxWidth="xs" sx={{ height: '100vh', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
            <CssBaseline />
            <Box
                sx={{
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    width: '100%',
                    maxWidth: '400px',
                }}
            >
                <Avatar sx={{ m: 1, bgcolor: 'primary.main' }}>
                    <LockOutlinedIcon />
                </Avatar>
                <Typography component="h1" variant="h5">
                    Sign in
                </Typography>

                <form noValidate onSubmit={handleSubmit}>
                    <TextField
                        variant="outlined"
                        margin="normal"
                        required
                        fullWidth
                        id="email"
                        label="Email Address"
                        onChange={(e) => setUsername(e.target.value)}
                        name="email"
                        autoComplete="email"
                        autoFocus
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        required
                        fullWidth
                        name="password"
                        label="Password"
                        onChange={(e) => setPassword(e.target.value)}
                        type="password"
                        id="password"
                        autoComplete="current-password"
                    />
                    {error && <Typography color="error">{error}</Typography>}
                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        color="primary"
                        sx={{ margin: '24px 0 16px' }}
                    >
                        Sign In
                    </Button>
                    <Grid container>
                        <Grid item xs>
                            <Link component={RouterLink} to="/resetpassword" variant="body2">
                                Reset Password
                            </Link>
                        </Grid>
                    </Grid>
                </form>

                {showAlert && (
                    <AlertPopup
                        message="Invalid credentials!"
                        duration={3000}
                        onClose={() => setShowAlert(false)}
                    />
                )}

            </Box>
        </Container>
    );
}

export default Login;
