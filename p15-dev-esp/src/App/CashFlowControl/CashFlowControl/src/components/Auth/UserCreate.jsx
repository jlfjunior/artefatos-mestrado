import { Container, TextField, Button, Typography, Box } from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const CreateUser = () => {
    const navigate = useNavigate();

    const [User, setUser] = useState({
        name: '',
        fullName: '',
        password: ''
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setUser({
            ...User,
            [name]: value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const token = localStorage.getItem('jwt');
            const url = `http://localhost:5001/api/Authentication/register`;

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    userName: User.name,
                    fullName: User.fullName,
                    password: User.password
                })
            });

            if (!response.ok) throw new Error('Error on create User');
            navigate('/users');
        } catch (error) {
            console.error(error);
        }
    };

    return (
        <Container maxWidth="sm" sx={{ mt: 4 }}>
            <Typography variant="h5" align="center" sx={{ mb: 2 }}>
                Create User
            </Typography>
            <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                    label="User Name"
                    variant="outlined"
                    fullWidth
                    name="name"
                    value={User.name}
                    onChange={handleChange}
                    required
                />
                <TextField
                    label="Full Name"
                    variant="outlined"
                    fullWidth
                    name="fullName"
                    value={User.fullName}
                    onChange={handleChange}
                    required
                />
                <TextField
                    label="Password"
                    variant="outlined"
                    fullWidth
                    name="password"
                    type="password"
                    value={User.password}
                    onChange={handleChange}
                    required
                />
                <Button type="submit" variant="contained" color="primary" sx={{ mt: 2 }}>
                    Create
                </Button>
            </Box>
        </Container>
    );
};

export default CreateUser;
