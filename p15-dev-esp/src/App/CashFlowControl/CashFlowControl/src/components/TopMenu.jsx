import React, { useState, useEffect } from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import Logo from '../assets/Logo.jsx';
import { jwtDecode } from "jwt-decode";

const TopMenu = () => {
    const { auth, logout } = useAuth();
    const [isAdmin, setIsAdmin] = useState(false);
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    useEffect(() => {
        const token = localStorage.getItem("jwt");
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                const roles = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

                if (roles === "Admin") {
                    setIsAdmin(true);
                } else {
                    setIsAdmin(false);
                }
            } catch (error) {
                console.error("Error decoding JWT:", error);
            }
        } else {
            setIsAdmin(false);
        }
    }, [auth]);


    return (
        <AppBar position="fixed" sx={{ top: 0, backgroundColor: '#333' }}>
            <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Logo />
                    <Typography
                        variant="h6"
                        sx={{
                            color: '#f2c94c',
                            fontFamily: '"Montserrat", sans-serif',
                            fontWeight: '600',
                            fontSize: '22px'
                        }}
                    >
                        Cash Flow Control
                    </Typography>
                </Box>

                {auth && (
                    <Box sx={{ display: 'flex', gap: 2 }}>
                        <Button
                            component={Link}
                            to="/launch"
                            color="inherit"
                            sx={{ '&:hover': { backgroundColor: '#555' } }}
                        >
                            Launch
                        </Button>
                        <Button
                            component={Link}
                            to="/product"
                            color="inherit"
                            sx={{ '&:hover': { backgroundColor: '#555' } }}
                        >
                            Product
                        </Button>
                        <Button
                            component={Link}
                            to="/consolidation"
                            color="inherit"
                            sx={{ '&:hover': { backgroundColor: '#555' } }}
                        >
                            Consolidation
                        </Button>
                        {isAdmin && (
                            <Button
                                component={Link}
                                to="/users"
                                color="inherit"
                                sx={{ '&:hover': { backgroundColor: '#555' } }}
                            >
                                Users
                            </Button>
                        )}
                    </Box>
                )}

                {auth && (
                    <Button
                        color="inherit"
                        onClick={handleLogout}
                        sx={{
                            backgroundColor: '#D32F2F',
                            '&:hover': { backgroundColor: '#C62828' },
                            borderRadius: '5px',
                            padding: '6px 16px',
                        }}
                    >
                        Logout
                    </Button>
                )}
            </Toolbar>
        </AppBar>
    );
};

export default TopMenu;
