import React, { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Container, Box, Link, TextField, Button, Typography } from '@mui/material';
import axios from '../../api/axios';
import { useNavigate } from 'react-router-dom';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post('/Auth/login', { email, password });
      if (response.status === 200) {
        localStorage.setItem('accessToken', response.data.jwtToken);
        console.log(response);
        console.log(localStorage.getItem('accessToken'));
        localStorage.setItem('refreshToken', response.data.refreshToken);
        navigate('/dashboard'); // Redirect to dashboard upon successful login
      } else {
        throw new Error('Упс.. Помилка входу ' + response.status + response.message);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Упс.. Помилка входу ');
    }
  };

  return (
<Container component="main" maxWidth="sm" sx={{
      height: '100vh', // full height of the viewport
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'center', // centers vertically
      alignItems: 'center', // centers horizontally
      color: '#003366', fontFamily: 'Montserrat, sans-serif' 
    }}>
      <Box 
        sx={{ 
          height: '100vh',
          display: 'flex', 
          flexDirection: 'column', 
          alignItems: 'center', 
          justifyContent: 'center', // Centers vertically
          width: { xs: '95%', sm: '50%' }, // Responsive width
          margin: 'auto' // Centering the form
        }}
      >
        <Typography component="h1" variant="h5" align="center" sx={{ mb: 3, color: '#003366' }}>
          Вхід в обліковий запис
        </Typography>
        <form onSubmit={handleLogin} style={{ width: '100%' }}>
          <TextField
            label="Електронна пошта"
            variant="outlined"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Пароль"
            variant="outlined"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2, mb: 2, bgcolor: '#003366', color: '#FAF8FC' }}>
            Login
          </Button>
          {error && <Typography color="error" style={{ marginTop: '10px' }}>{error}</Typography>}
          <Link component={RouterLink} to="/register" variant="body2" style={{ marginBottom: '20px' }}>
            Немає облікового запису? Зареструйся тут
          </Link>
        </form>
      </Box>
    </Container>
  );
};

export default LoginPage;
