import React, { useCallback, useState, useEffect } from 'react';
import {
    Container,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Button,
    TablePagination,
    Typography
} from '@mui/material';
import { jwtDecode } from "jwt-decode";
import DeleteIcon from '@mui/icons-material/Delete';
import { useNavigate } from 'react-router-dom';

const User = () => {
    const [users, setUsers] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    const navigate = useNavigate();

    const fetchUsers = useCallback(async () => {
        try {
            const token = localStorage.getItem("jwt");
            if (!token) throw new Error("Token não encontrado");

            const decodedToken = jwtDecode(token);
            const loggedInUserName = decodedToken?.unique_name;

            const response = await fetch(
                `http://localhost:5001/api/Authentication/GetUsersPaginated?pageNumber=${page + 1}&pageSize=${rowsPerPage}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                }
            );

            if (!response.ok) throw new Error("Erro ao carregar produtos");

            const data = await response.json();
            const filteredUsers =
                data?.users.filter((user) => user.userName !== loggedInUserName) || [];

            setUsers(filteredUsers);
            setTotalCount(data?.total || 0);
        } catch (error) {
            console.error(error);
            setUsers([]);
            setTotalCount(0);
        }
    }, [page, rowsPerPage]);

    useEffect(() => {
        fetchUsers();
    }, [fetchUsers]);

    const deleteUser = async (id) => {
        try {
            const token = localStorage.getItem('jwt');
            const response = await fetch(`http://localhost:5001/api/Authentication/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Error on delete user.');

            setUsers((prevUsers) => prevUsers.filter(user => user.id !== id));
            setTotalCount((prevCount) => prevCount - 1);
        } catch (error) {
            console.error(error);
        }
    };


    const handleChangePage = (event, newPage) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    const handleCreateClick = () => {
        navigate('/users/create');
    };

    return (
        <Container maxWidth="lg" sx={{ mt: 10, backgroundColor: '#1e1e1e', color: 'white', padding: 3, borderRadius: 2 }}>
            <Typography variant="h5" sx={{ mb: 2, textAlign: 'center' }}>Users List</Typography>

            <Button
                variant="contained"
                color="primary"
                onClick={handleCreateClick}
                sx={{ mb: 2 }}
            >
                Create User
            </Button>

            <TableContainer component={Paper} sx={{ backgroundColor: '#2c2c2c', color: 'white' }}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Name</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Username</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {users.length > 0 ? (
                            users.map((User) => (
                                <TableRow key={User.id} sx={{ height: '36px' }}>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{User.fullName}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{User.userName}</TableCell>
                                    <TableCell sx={{ padding: '6px 16px' }}>
                                        <Button
                                            variant="contained"
                                            color="error"
                                            size="small"
                                            sx={{ padding: '4px 8px', fontSize: '0.75rem', marginLeft: 1 }}
                                            onClick={() => deleteUser(User.id)}
                                        >
                                            <DeleteIcon sx={{ fontSize: '16px' }} />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={5} sx={{ textAlign: 'center', color: 'white' }}>
                                    No Users found
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </TableContainer>

            <TablePagination
                component="div"
                count={totalCount}
                page={page}
                onPageChange={handleChangePage}
                rowsPerPage={rowsPerPage}
                onRowsPerPageChange={handleChangeRowsPerPage}
                sx={{ color: 'white' }}
                disabled={totalCount === 0}
            />
        </Container>
    );
};

export default User;
