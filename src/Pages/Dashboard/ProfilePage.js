import React, { useState, useEffect } from 'react';
import {
  Container, AppBar, Toolbar, Link, TextField, Button, Typography, Dialog, DialogActions,
  DialogContent, DialogContentText, DialogTitle, Box, useMediaQuery
} from '@mui/material';
import axios from '../../api/axios';
import useAuth from '../../hooks/useAuth';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { useTheme } from '@mui/material/styles';

const Navigation = ({ handleLogout }) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <AppBar position="static" sx={{ bgcolor: '#FAF8FC' }}>
      <Toolbar>
        {!isMobile && (
          <Typography variant="h6" sx={{ flexGrow: 1, color: '#003366' }}>
            CryptoPay
          </Typography>
        )}
        <Box sx={{ display: 'flex', flexDirection: isMobile ? 'column' : 'row', alignItems: 'center', mx: 'auto' }}>
          <Link component={RouterLink} to="/dashboard" color="inherit" sx={{ m: 3, color: '#003366' }}>
          Інформаційна панель
          </Link>
          <Link component={RouterLink} to="/earnings" color="inherit" sx={{ m: 3, color: '#003366' }}>
          Мій заробіток
          </Link>
          <Link component={RouterLink} to="/profile" color="inherit" sx={{ m: 3, color: '#003366' }}>
          Профіль
          </Link>
        </Box>
        <Button onClick={handleLogout} variant="contained" sx={{ m: 2, bgcolor: '#003366', color: '#FAF8FC' }}>
        Вийти з акаунту
        </Button>
      </Toolbar>
    </AppBar>
  );
};

const ProfilePage = () => {
  const auth = useAuth();
  const [displayName, setDisplayName] = useState('');
  const [originalDisplayName, setOriginalDisplayName] = useState('');
  const [error, setError] = useState('');
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [isEditingDisplayName, setIsEditingDisplayName] = useState(false);
  const [isEditingPassword, setIsEditingPassword] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogType, setDialogType] = useState('');

  const navigate = useNavigate();

  useEffect(() => {
    // Fetch user data
    const fetchUserData = async () => {
      try {
        const response = await axios.get(`/auth/user-details`, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setDisplayName(response.data.displayName);
        setOriginalDisplayName(response.data.displayName);
      } catch (err) {
        console.error('Не вдалося отримати дані користувача:', err);
      }
    };

    fetchUserData();
  }, [auth.accessToken]);

  const handleDisplayNameChange = (e) => {
    setDisplayName(e.target.value);
    setIsEditingDisplayName(e.target.value !== originalDisplayName);
  };

  const handleOldPasswordChange = (e) => setOldPassword(e.target.value);
  const handleNewPasswordChange = (e) => setNewPassword(e.target.value);
  const handleConfirmNewPasswordChange = (e) => setConfirmNewPassword(e.target.value);

  const handleSaveDisplayName = () => {
    setDialogType('displayName');
    setOpenDialog(true);
  };

  const handleSavePassword = () => {
    setDialogType('password');
    setOpenDialog(true);
  };

  const handleDialogClose = () => setOpenDialog(false);

  const handleDialogConfirm = async () => {
    if (dialogType === 'displayName') {
      // Update display name
      try {
        await axios.put('/auth/updateDisplayName', { displayName }, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setIsEditingDisplayName(false);
        setOriginalDisplayName(displayName);
      } catch (err) {
        console.error('Failed to update display name:', err);
      }
    } else if (dialogType === 'password') {
      // Update password
      try {
        if (newPassword !== confirmNewPassword) {
          throw new Error('New passwords do not match');
        }

        await axios.put('/auth/updatePassword', { oldPassword, newPassword }, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setOldPassword('');
        setNewPassword('');
        setConfirmNewPassword('');
        setIsEditingPassword(false);
      } catch (err) {
        console.error('Failed to update password:', err);
      }
    }
    setOpenDialog(false);
  };

  const handleLogout = async (e) => {
    e.preventDefault();
    try {
      await axios.post('/auth/logout', {}, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userId');
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message || 'Не вдалося вийти з облікового запису');
    }
  };

  return (
    <Box sx={{ bgcolor: '#FAF8FC', minHeight: '100vh', display: 'flex', flexDirection: 'column', alignItems: 'center', color: '#003366', fontFamily: 'Montserrat, sans-serif' }}>
      <Navigation handleLogout={handleLogout} />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box
          display="flex"
          flexDirection="column"
          alignItems="center"
          justifyContent="center"
          minHeight="80vh"
          maxWidth="sm"
          mx="auto"
        >
          <Typography variant="h4" sx={{ mt: 4, mb: 2 }}>
          Профіль користувача
          </Typography>
          {error && <Typography color="error">{error}</Typography>}
          <Box component="form" sx={{ mt: 3 }}>
            <TextField
              fullWidth
              label="Ім'я користувача"
              name="displayName"
              value={displayName}
              onChange={handleDisplayNameChange}
              margin="normal"
            />
            {isEditingDisplayName && (
              <Button variant="contained" sx={{ mt: 2, bgcolor: '#003366', color: '#FAF8FC' }} onClick={handleSaveDisplayName}>
                Зберегти Нове Ім'я
              </Button>
            )}
            <Typography variant="h6" sx={{ mt: 4, mb: 2 }}>
              Змінити пароль
            </Typography>
            <TextField
              fullWidth
              label="Поточний пароль"
              type="password"
              name="oldPassword"
              value={oldPassword}
              onChange={handleOldPasswordChange}
              margin="normal"
            />
            <TextField
              fullWidth
              label="Новий пароль"
              type="password"
              name="newPassword"
              value={newPassword}
              onChange={handleNewPasswordChange}
              margin="normal"
            />
            <TextField
              fullWidth
              label="Підтвердження нового паролю"
              type="password"
              name="confirmNewPassword"
              value={confirmNewPassword}
              onChange={handleConfirmNewPasswordChange}
              margin="normal"
            />
            <Button variant="contained" sx={{ mt: 2, bgcolor: '#003366', color: '#FAF8FC' }} onClick={handleSavePassword}>
              Підтвердити Зміну паролю
            </Button>
          </Box>

          <Dialog open={openDialog} onClose={handleDialogClose}>
            <DialogTitle>Confirm {dialogType === 'displayName' ? 'Display Name Change' : 'Password Change'}</DialogTitle>
            <DialogContent>
              <DialogContentText>
              Ви впевнені, що бажаєте {dialogType === 'displayName' ? ' змінити своє ім\'я для відображення?' : 'змінити пароль?'}
              </DialogContentText>
            </DialogContent>
            <DialogActions>
              <Button onClick={handleDialogClose} color="primary">
                Відмінити
              </Button>
              <Button onClick={handleDialogConfirm} color="primary">
                Підтвердити
              </Button>
            </DialogActions>
          </Dialog>
        </Box>
      </Container>
    </Box>
  );
};

export default ProfilePage;
