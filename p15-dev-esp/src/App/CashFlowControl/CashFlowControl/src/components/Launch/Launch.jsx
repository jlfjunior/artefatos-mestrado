import React, { useState, useEffect, useRef } from 'react';
import {
    CssBaseline,
    TextField,
    Button,
    Box,
    Typography,
    FormControl,
    Select,
    MenuItem,
    Container,
    List,
    ListItem,
    ListItemText,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions
} from '@mui/material';
import AddShoppingCartIcon from '@mui/icons-material/AddShoppingCart';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { useNavigate } from 'react-router-dom';
import SuccessPopup from "../Popup/Success";

const LaunchSale = () => {
    const [showAlert, setShowAlert] = useState(false);
    const [productName, setProductName] = useState('');
    const [quantity, setQuantity] = useState('');
    const [error, setError] = useState(null);
    const [suggestions, setSuggestions] = useState([]);
    const [selectedProductId, setSelectedProductId] = useState(null);
    const [transactionType, setTransactionType] = useState(0);
    const [productsOrder, setProductsOrder] = useState([]);
    const [cartOpen, setCartOpen] = useState(false);
    const navigate = useNavigate();
    const textFieldRef = useRef(null);

    useEffect(() => {
        if (productName.length < 2) {
            setSuggestions([]);
            return;
        }

        const fetchProducts = async () => {
            const token = localStorage.getItem('jwt');
            if (!token) {
                console.error("Token de autenticação não encontrado.");
                navigate("/login");
                return;
            }

            try {
                const response = await fetch(`http://localhost:5002/api/Product/ByName/${productName}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json',
                    },
                });

                if (!response.ok) throw new Error("Erro ao buscar produtos");

                const data = await response.json();
                setSuggestions(data);
            } catch (error) {
                console.error("Erro na requisição:", error);
            }
        };

        const delayDebounce = setTimeout(fetchProducts, 500);
        return () => clearTimeout(delayDebounce);
    }, [productName, navigate]);

    const handleSelectProduct = (selectedProduct) => {
        setProductName(`${selectedProduct.name} - R$ ${selectedProduct.price} - Stock: ${selectedProduct.stock} Un.`);
        setSelectedProductId(selectedProduct.id);
        setSuggestions([]);
    };

    const handleRemoveProduct = (productId) => {
        setProductsOrder(productsOrder.filter(product => product.productId !== productId));
    };

    const handleAddProduct = () => {
        if (!quantity || !selectedProductId || !productName) {
            setError('All fields are required.');
            return;
        }

        setProductsOrder([...productsOrder, {
            productId: selectedProductId,
            quantity: parseInt(quantity, 10),
            name: productName
        }]);

        setProductName('');
        setQuantity('');
        setSelectedProductId(null);
        setError(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        setShowAlert(false);

        if (productsOrder.length === 0) {
            setError('At least one product must be added.');
            return;
        }

        const token = localStorage.getItem('jwt');
        if (!token) {
            setError('User is not authenticated.');
            return;
        }

        try {
            const response = await fetch('http://localhost:5002/api/Launch', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({ launchType: transactionType, productsOrder }),
            });

            const result = await response.json();
            if (!result.success) throw new Error(result.message);

            setShowAlert(true);
            setTimeout(() => {
                window.location.reload();
            }, 3000);
        } catch (err) {
            setError(err.message || 'An unexpected error occurred.');
        }
    };

    return (
        <Container component="main" maxWidth="xs">
            <CssBaseline />
            <Box sx={{ textAlign: 'center', width: '100%' }}>
                <AddShoppingCartIcon sx={{ fontSize: 50, mb: 2 }} />
                <Typography variant="h5">Launch Sale</Typography>
                <form noValidate onSubmit={handleSubmit}>
                    <TextField
                        ref={textFieldRef}
                        variant="outlined"
                        margin="normal"
                        required
                        fullWidth
                        label="Product Name"
                        value={productName}
                        onChange={(e) => setProductName(e.target.value)}
                        autoFocus
                    />
                    {suggestions.length > 0 && (
                        <List>
                            {suggestions.map((product) => (
                                <ListItem button key={product.id} onClick={() => handleSelectProduct(product)}>
                                    <ListItemText primary={`${product.name} - R$ ${product.price} - Stock: ${product.stock} Un.`} />
                                </ListItem>
                            ))}
                        </List>
                    )}
                    <TextField
                        variant="outlined"
                        margin="normal"
                        required
                        fullWidth
                        type="number"
                        label="Quantity"
                        value={quantity}
                        onChange={(e) => setQuantity(e.target.value)}
                    />
                    {error && <Typography color="error">{error}</Typography>}
                    <Button fullWidth variant="contained" color="secondary" onClick={handleAddProduct}>
                        Add to Cart ({productsOrder.length})
                    </Button>
                    <Button fullWidth variant="outlined" color="primary" onClick={() => setCartOpen(true)} startIcon={<ShoppingCartIcon />} disabled={!productsOrder.length}>
                        View Cart
                    </Button>
                    <FormControl fullWidth margin="normal">
                        <Select value={transactionType} onChange={(e) => setTransactionType(e.target.value)} required>
                            <MenuItem value={0}>Debit</MenuItem>
                            <MenuItem value={1}>Credit</MenuItem>
                        </Select>
                    </FormControl>
                    <Button type="submit" fullWidth variant="contained" color="primary">Launch Sale</Button>
                </form>
                {showAlert && <SuccessPopup message="Sale finished successfully!" duration={3000} onClose={() => setShowAlert(false)} />}
            </Box>

            <Dialog open={cartOpen} onClose={() => setCartOpen(false)}>
                <DialogTitle>Shopping Cart</DialogTitle>
                <DialogContent>
                    <List>
                        {productsOrder.map((product, index) => (
                            <ListItem key={index}>
                                <ListItemText primary={`${product.name} - Quantity: ${product.quantity}`} />
                                <Button color="error" onClick={() => handleRemoveProduct(product.productId)}>Remove</Button>
                            </ListItem>
                        ))}
                    </List>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setCartOpen(false)}>Close</Button>
                </DialogActions>
            </Dialog>
        </Container>
    );
};

export default LaunchSale;
