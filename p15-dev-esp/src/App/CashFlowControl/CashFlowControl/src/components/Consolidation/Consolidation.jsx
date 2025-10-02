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
    TablePagination,
    Typography
} from '@mui/material';

const Consolidation = () => {
    const [consolidations, setConsolidations] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);

    const fetchConsolidations = useCallback(async () => {
        try {
            const token = localStorage.getItem('jwt');
            const response = await fetch(`http://localhost:5003/api/Consolidation/Paginated?pageNumber=${page + 1}&pageSize=${rowsPerPage}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Erro ao carregar consolidações');
            const data = await response.json();

            setConsolidations(data?.consolidations || []);
            setTotalCount(data?.total || 0);
        } catch (error) {
            console.error(error);
            setConsolidations([]);
            setTotalCount(0);
        }
    }, [page, rowsPerPage]);

    useEffect(() => {
        fetchConsolidations();
    }, [fetchConsolidations]);

    const handleChangePage = (event, newPage) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    return (
        <Container maxWidth="lg" sx={{ mt: 10, backgroundColor: '#1e1e1e', color: 'white', padding: 3, borderRadius: 2 }}>
            <Typography variant="h5" sx={{ mb: 2, textAlign: 'center' }}>Consolidations List</Typography>

            <TableContainer component={Paper} sx={{ backgroundColor: '#2c2c2c', color: 'white' }}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>ID</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Credit Amount</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Debit Amount</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Total Amount</TableCell>
                            <TableCell sx={{ color: 'white', padding: '8px 16px' }}>Date</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {consolidations.length > 0 ? (
                            consolidations.map((consolidation) => (
                                <TableRow key={consolidation.id} sx={{ height: '36px' }}>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{consolidation.id}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{consolidation.totalCreditAmount}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{consolidation.totalDebitAmount}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{consolidation.totalAmount}</TableCell>
                                    <TableCell sx={{ color: 'white', padding: '6px 16px' }}>{consolidation.date}</TableCell>
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={5} sx={{ textAlign: 'center', color: 'white' }}>
                                    No consolidations found
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

export default Consolidation;
