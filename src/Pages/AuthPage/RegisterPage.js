import React, { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Container, Box, Link, TextField, Button, Typography, Dialog, DialogTitle, DialogContent, DialogActions, CircularProgress } from '@mui/material';
import axios from '../../api/axios';
import { useNavigate } from 'react-router-dom';

const RegisterPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [generatedCode, setGeneratedCode] = useState('');
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    if (!/^(?=.*[A-Z])(?=.*\d).{8,}$/.test(password)) {
      setError("Password must be at least 8 characters long, include a number and an uppercase letter.");
      setIsLoading(false);
      return;
    }

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      setIsLoading(false);
      return;
    }

    try {
      const response = await axios.post('/Auth/verify-email', { email });
      setGeneratedCode(response.data.verificationCode);
      setIsDialogOpen(true);
    } catch (err) {
      setError(err.response?.data?.message || 'Email verification failed');
    } finally {
      setIsLoading(false);
    }
  };

  const handleVerifyCode = async () => {
    if (verificationCode === generatedCode) {
      try {
        setIsLoading(true);
        await axios.post('/Auth/register', { email, passwordHash: password, displayName });
        const loginResponse = await axios.post('/Auth/login', { email, password });
        if (loginResponse.status === 200) {
          localStorage.setItem('accessToken', loginResponse.data.jwtToken);
          localStorage.setItem('refreshToken', loginResponse.data.refreshToken);
          navigate('/dashboard');
        } else {
          throw new Error(`Something went wrong, try to log in ${loginResponse.status} ${loginResponse.data.message}`);
        }
      } catch (err) {
        setError(err.response?.data?.message || 'Registration failed');
      } finally {
        setIsLoading(false);
        setIsDialogOpen(false);
      }
    } else {
      setError('Verification code is incorrect.');
    }
  };

  return (
    <Container component="main" maxWidth="sm" sx={{
      height: '100vh',
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'center',
      alignItems: 'center'
    }}>
      <Box
        sx={{
          height: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          width: { xs: '95%', sm: '50%' },
          margin: 'auto'
        }}
      >
        <Typography component="h1" variant="h5" align="center" sx={{ mb: 3, color: '#003366' }}>
          Реєстрація
        </Typography>
        <form onSubmit={handleRegister} style={{ width: '100%' }}>
          <TextField
            label="Електронна пошта"
            variant="outlined"
            type="email"
            name="email"
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
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Підтвердження паролю"
            variant="outlined"
            type="password"
            name="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Ім'я користувача"
            variant="outlined"
            type="text"
            name="displayName"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2, mb: 2, bgcolor: '#003366', color: '#FAF8FC' }} disabled={isLoading}>
            {isLoading ? <CircularProgress size={24} /> : 'Зареєструватися'}
          </Button>
          {error && <Typography color="error" style={{ marginTop: '10px' }}>{error}</Typography>}
          <Link component={RouterLink} to="/login" variant="body2" style={{ marginBottom: '20px' }}>
            Вже зареєстровані? Увійдіть тут
          </Link>
        </form>
        <Dialog open={isDialogOpen} onClose={() => setIsDialogOpen(false)}>
          <DialogTitle>Підтвердження Електронної пошти</DialogTitle>
          <DialogContent>
            <TextField
              label="Код верифікації"
              variant="outlined"
              type="text"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              fullWidth
              margin="normal"
              required
            />
            {isLoading && <CircularProgress sx={{ mt: 2 }} />}
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setIsDialogOpen(false)} color="primary">
              Відміна
            </Button>
            <Button onClick={handleVerifyCode} color="primary" disabled={isLoading}>
              Перевірити
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Container>
  );
};

export default RegisterPage;
