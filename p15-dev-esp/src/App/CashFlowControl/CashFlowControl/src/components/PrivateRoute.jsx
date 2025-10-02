import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useEffect } from "react";
import { useAuth } from "../context/AuthContext.jsx";

const PrivateRoute = () => {
    const location = useLocation();
    const { auth, loading, logout } = useAuth();

    useEffect(() => {
        if (!loading && !auth) {
            console.log("Usuário não autenticado, realizando logout...");
            logout();
        }
    }, [auth, loading, logout]);

    if (loading) {
        return <div>Carregando...</div>;
    }

    if (auth && location.pathname === "/login") {
        return <Navigate to="/launch" replace />;
    }

    if (!auth) {
        return <Navigate to="/login" replace />;
    }

    return <Outlet />;
};

export default PrivateRoute;
