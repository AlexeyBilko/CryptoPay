import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link as RouterLink } from 'react-router-dom';
import { Container, AppBar, Toolbar, Button, Typography, useMediaQuery, TextField, Link, Select, MenuItem, Box, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions, Checkbox, FormControlLabel } from '@mui/material';
import axios from '../../api/axios';
import useAuth from '../../hooks/useAuth';
import { useTheme } from '@mui/material/styles';

const Navigation = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  return (
    <AppBar position="static" sx={{ bgcolor: '#FAF8FC' }}>
    <Toolbar>
      {!isMobile && (
        <Typography variant="h6" sx={{ flexGrow: 1, color: '#003366' }}>
          Crypto Payment Gateway
        </Typography>
      )}
          <Box sx={{ display: 'flex', flexDirection: isMobile ? 'column' : 'row', alignItems: 'center', mx: 'auto' }}>
            <Link component={RouterLink} to="/dashboard" color="inherit" sx={{ m: 3, color: '#003366' }}>
              Dashboard
            </Link>
            <Link component={RouterLink} to="/earnings" color="inherit" sx={{ m: 3, color: '#003366' }}>
              Earnings
            </Link>
            <Link component={RouterLink} to="/profile" color="inherit" sx={{ m: 3, color: '#003366' }}>
              Profile
            </Link>
          </Box>
        <Button component={RouterLink} to="/dashboard" variant="contained" sx={{ m: 2, bgcolor: '#003366', color: '#FAF8FC' }}>
          Back to Dashboard
        </Button>
      </Toolbar>
    </AppBar>
  );
};

const CreatePaymentPage = () => {
  const { id } = useParams();
  const [paymentPage, setPaymentPage] = useState({
    title: '',
    description: '',
    amountUSD: '',
    amountCrypto: '',
    currencyCode: 'BTC',
    isDonation: false,
    pageId: ''
  });
  const [error, setError] = useState('');
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const navigate = useNavigate();
  const auth = useAuth();

  useEffect(() => {
    const fetchPaymentPage = async () => {
      try {
        const response = await axios.get(`/PaymentPage/${id}`, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        if (response.data.userId !== auth.userId) {
          navigate('/dashboard');
        } else {
          setPaymentPage({
            title: response.data.title,
            description: response.data.description,
            amountUSD: response.data.amountDetails.amountUSD,
            amountCrypto: response.data.amountDetails.amountCrypto,
            currencyCode: response.data.amountDetails.currency.currencyCode,
            isDonation: response.data.isDonation,
            pageId: id
          });
        }
      } catch (err) {
        console.error('Failed to fetch payment page:', err);
        setError('Failed to fetch payment page');
        navigate('/dashboard');
      }
    };

    if (id) {
      fetchPaymentPage();
    } else {
      setPaymentPage(prevState => ({
        ...prevState,
        pageId: -1
      }));
    }
  }, [id, auth.accessToken, auth.userId, navigate]);

  const handleChange = (e) => {
    const { name, value, checked, type } = e.target;
    setPaymentPage(prevState => ({
      ...prevState,
      [name]: type === 'checkbox' ? checked : value
    }));

    if (name === 'amountCrypto' && value) {
      convertCryptoToUSD(value);
    } else if (name === 'amountUSD' && value) {
      convertUSDToCrypto(value);
    }
  };

  const convertCryptoToUSD = async (cryptoAmount) => {
    try {
      const response = await axios.post('/PaymentPage/convertToUSD', {
        cryptoAmount: parseFloat(cryptoAmount),
        currencyCode: paymentPage.currencyCode
      }, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      setPaymentPage(prevState => ({
        ...prevState,
        amountUSD: response.data.amountUSD
      }));
    } catch (err) {
      setError('Failed to convert crypto to USD: ' + err.response?.data?.Error || err.message);
    }
  };

  const convertUSDToCrypto = async (usdAmount) => {
    try {
      const response = await axios.post('/PaymentPage/convertToCrypto', {
        usdAmount: parseFloat(usdAmount),
        currencyCode: paymentPage.currencyCode
      }, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      setPaymentPage(prevState => ({
        ...prevState,
        amountCrypto: response.data.amountCrypto
      }));
    } catch (err) {
      setError('Failed to convert USD to crypto: ' + err.response?.data?.Error || err.message);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!paymentPage.isDonation && (paymentPage.amountUSD < 10 || paymentPage.amountCrypto < 0.00001)) {
      setError('Amount USD must be at least 100 and Amount Crypto must be at least 0.001.');
      return;
    }

    try {
      const payload = {
        title: paymentPage.title,
        description: paymentPage.description,
        amountUSD: paymentPage.isDonation ? 0 : paymentPage.amountUSD,
        amountCrypto: paymentPage.isDonation ? 0 : paymentPage.amountCrypto,
        currencyCode: paymentPage.currencyCode,
        isDonation: paymentPage.isDonation,
        pageId: paymentPage.pageId
      };

      if (id) {
        await axios.put('/PaymentPage/update', payload, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
      } else {
        await axios.post('/PaymentPage/create', payload, {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
      }
      navigate('/dashboard');
    } catch (err) {
      setError('Failed to save payment page');
    }
  };

  const handleDelete = async () => {
    try {
      await axios.delete(`/PaymentPage/delete/${id}`, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      navigate('/dashboard');
    } catch (err) {
      setError('Failed to delete payment page');
    }
  };

  return (
    <Box sx={{ bgcolor: '#FAF8FC', minHeight: '100vh', display: 'flex', flexDirection: 'column', alignItems: 'center', color: '#003366', fontFamily: 'Montserrat, sans-serif' }}>
    <Navigation />
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
        {id ? 'Update' : 'Create'} Payment Page
      </Typography>
      {error && <Typography color="error">{error}</Typography>}
      <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
        <TextField
          fullWidth
          label="Title"
          name="title"
          value={paymentPage.title}
          onChange={handleChange}
          margin="normal"
          required
        />
        <TextField
          fullWidth
          label="Description"
          name="description"
          value={paymentPage.description}
          onChange={handleChange}
          margin="normal"
          required
        />
        <FormControlLabel
          control={
            <Checkbox
              checked={paymentPage.isDonation}
              onChange={handleChange}
              name="isDonation"
              color="primary"
              disabled={!!id} // Disable when updating
            />
          }
          label="Is Donation"
        />
        {!paymentPage.isDonation && (
          <>
            <TextField
              fullWidth
              label="Amount USD"
              name="amountUSD"
              value={paymentPage.amountUSD}
              onChange={handleChange}
              margin="normal"
              required
              disabled={!!id} // Disable amount fields when editing
            />
            <TextField
              fullWidth
              label="Amount Crypto"
              name="amountCrypto"
              value={paymentPage.amountCrypto}
              onChange={handleChange}
              margin="normal"
              required
              disabled={!!id} // Disable amount fields when editing
            />
          </>
        )}
        <Select
          fullWidth
          label="Currency Code"
          name="currencyCode"
          value={paymentPage.currencyCode}
          onChange={handleChange}
          margin="normal"
          required
          disabled={!!id} // Disable currency selection when editing
        >
          <MenuItem value="BTC">BTC</MenuItem>
          <MenuItem value="ETH">ETH</MenuItem>
        </Select>
        <Button type="submit" variant="contained" color="primary" sx={{ mt: 2, bgcolor: '#003366', color: '#FAF8FC'  }}>
          {id ? 'Update' : 'Create'} Payment Page
        </Button>
        {id && (
          <Button
            variant="contained"
            color="primary"
            sx={{ mt: 2, ml: 2, bgcolor: '#E65B40', color: '#FAF8FC'  }}
            onClick={() => setOpenDeleteDialog(true)}
          >
            Delete Payment Page
          </Button>
        )}
      </Box>
      <Dialog
        open={openDeleteDialog}
        onClose={() => setOpenDeleteDialog(false)}
      >
        <DialogTitle sx={{ color: '#003366' }}>Confirm Delete</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ color: '#E65B40' }}>
            Are you sure you want to delete this payment page? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteDialog(false)} color="primary" sx={{ color: '#003366' }}>
            Cancel
          </Button>
          <Button onClick={handleDelete} color="secondary" sx={{ color: '#E65B40' }}>Delete</Button>
        </DialogActions>
      </Dialog>
        </Box>
    </Container>
    </Box>
  );
};

export default CreatePaymentPage;
