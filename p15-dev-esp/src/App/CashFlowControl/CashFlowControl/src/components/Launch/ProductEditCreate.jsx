import React, { useState, useEffect } from 'react';
import { Container, TextField, Button, Typography, Box } from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';

const EditProduct = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [product, setProduct] = useState({
        name: '',
        price: 0,
        stock: 0
    });
    const [loading, setLoading] = useState(true);
    const [isEditing, setIsEditing] = useState(false);

    useEffect(() => {
        if (id) {
            setIsEditing(true);
            fetchProduct(id);
        } else {
            setIsEditing(false);
            setLoading(false);
        }
    }, [id]);

    const fetchProduct = async (id) => {
        try {
            const token = localStorage.getItem('jwt');
            const response = await fetch(`http://localhost:5002/api/Product/${id}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Product not found');
            const data = await response.json();
            setProduct(data);
            setLoading(false);
        } catch (error) {
            console.error(error);
            navigate('/products');
        }
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setProduct((prevProduct) => ({
            ...prevProduct,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const token = localStorage.getItem('jwt');
            const url = isEditing
                ? `http://localhost:5002/api/Product/${id}`
                : `http://localhost:5002/api/Product`;

            const method = isEditing ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name: product.name,
                    price: product.price,
                    stock: product.stock
                })
            });

            if (!response.ok) throw new Error(isEditing ? 'Error on edit product' : 'Error on create product');
            navigate('/product');
        } catch (error) {
            console.error(error);
        }
    };

    if (loading && isEditing) {
        return <Typography variant="h6" align="center">Loading...</Typography>;
    }

    return (
        <Container maxWidth="sm" sx={{ mt: 4 }}>
            <Typography variant="h5" align="center" sx={{ mb: 2 }}>
                {isEditing ? 'Edit Product' : 'Create Product'}
            </Typography>
            <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                    label="Product Name"
                    variant="outlined"
                    fullWidth
                    name="name"
                    value={product.name}
                    onChange={handleChange}
                    required
                />
                <TextField
                    label="Price"
                    variant="outlined"
                    fullWidth
                    name="price"
                    type="number"
                    value={product.price}
                    onChange={handleChange}
                    required
                />
                <TextField
                    label="Stock"
                    variant="outlined"
                    fullWidth
                    name="stock"
                    type="number"
                    value={product.stock}
                    onChange={handleChange}
                    required
                />
                <Button type="submit" variant="contained" color="primary" sx={{ mt: 2 }}>
                    {isEditing ? 'Update' : 'Create'}
                </Button>
            </Box>
        </Container>
    );
};

export default EditProduct;
