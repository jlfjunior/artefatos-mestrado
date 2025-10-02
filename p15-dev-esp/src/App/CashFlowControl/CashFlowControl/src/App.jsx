import React from 'react';
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import theme from "./assets/theme.jsx";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from './context/AuthContext';
import TopMenu from './components/TopMenu';
import Login from "./components/Auth/Login.jsx";
import Launch from "./components/Launch/Launch";
import Product from "./components/Launch/Product";
import Consolidation from "./components/Consolidation/Consolidation";
import User from "./components/Auth/Users";
import PrivateRoute from "./components/PrivateRoute";
import Alert from "./components/Popup/Alert.jsx";
import EditProduct from "./components/Launch/ProductEditCreate.jsx";
import EditUser from "./components/Auth/UserCreate.jsx";
import ResetPassword from "./components/Auth/ResetPassword";

function App() {
    return (
        <AuthProvider>
            <Alert />
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Router>
                    <TopMenu />
                    <div style={{ marginTop: '64px', paddingTop: 5 }}>
                        <Routes>
                            <Route path="/login" element={<Login />} />
                            <Route path="/resetpassword" element={<ResetPassword />} />

                            <Route element={<PrivateRoute />}>
                                <Route path="/launch" element={<Launch />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/product" element={<Product />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/users" element={<User />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/consolidation" element={<Consolidation />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/products/edit/:id" element={<EditProduct /> } />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/products/create" element={<EditProduct />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/users/edit/:id" element={<EditUser />} />
                            </Route>
                            <Route element={<PrivateRoute />}>
                                <Route path="/users/create" element={<EditUser />} />
                            </Route>
                            <Route path="/" element={<Navigate to="/launch" />} />
                        </Routes>
                    </div>
                </Router>
            </ThemeProvider>
        </AuthProvider>
    );
}

export default App;
