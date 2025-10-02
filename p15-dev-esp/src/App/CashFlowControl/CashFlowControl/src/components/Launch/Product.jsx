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
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useNavigate } from 'react-router-dom';

const Product = () => {
    const [products, setProducts] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    const navigate = useNavigate();

    const fetchProducts = useCallback(async () => {
        try {
            const token = localStorage.getItem('jwt');
            const response = await fetch(`http://localhost:5002/api/Product/Paginated?pageNumber=${page + 1}&pageSize=${rowsPerPage}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Erro ao carregar produtos');
            const data = await response.json();

            setProducts(data?.products || []);
            setTotalCount(data?.total || 0);
        } catch (error) {
            console.error(error);
            setProducts([]);
            setTotalCount(0);
        }
    }, [page, rowsPerPage]);

    useEffect(() => {
        fetchProducts();
    }, [fetchProducts]);


    const deleteProduct = async (id) => {
        try {
            const token = localStorage.getItem('jwt');
            const response = await fetch(`http://localhost:5002/api/Product/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Erro ao deletar o produto');

            setProducts((prevProducts) => prevProducts.filter(product => product.id !== id));
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

    const handleEditClick = (id) => {
        navigate(`/products/edit/${id}`);
    };

    const handleCreateClick = () => {
        navigate('/products/create');
    };

    return (
        <Container maxWidth="lg" sx={{ mt: 10, backgroundColor: '#1e1e1e', color: 'white', padding: 3, borderRadius: 2 }}>
            <Typography variant="h5" sx={{ mb: 2, textAlign: 'center' }}>Products List</Typography>

            <Button
                variant="contained"
                color="primary"
                onClick={handleCreateClick}
                sx={{ mb: 2 }}
            >
                Create Product
            </Button>

            <TableContainer component={Paper} sx={{ backgroundColor: '#2c2c2c', color: 'white' }}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>ID</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Name</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Stock</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Price</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {products.length > 0 ? (
                            products.map((product) => (
                                <TableRow key={product.id} sx={{ height: '36px' }}>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{product.id}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{product.name}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{product.stock}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{product.price}</TableCell>
                                    <TableCell sx={{ padding: '6px 16px' }}>
                                        <Button
                                            variant="contained"
                                            color="primary"
                                            size="small"
                                            sx={{ padding: '4px 8px', fontSize: '0.75rem' }}
                                            onClick={() => handleEditClick(product.id)}
                                        >
                                            <EditIcon sx={{ fontSize: '16px' }} />
                                        </Button>
                                        <Button
                                            variant="contained"
                                            color="error"
                                            size="small"
                                            sx={{ padding: '4px 8px', fontSize: '0.75rem', marginLeft: 1 }}
                                            onClick={() => deleteProduct(product.id)}
                                        >
                                            <DeleteIcon sx={{ fontSize: '16px' }} />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={5} sx={{ textAlign: 'center', color: 'white' }}>
                                    No products found
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

export default Product;
