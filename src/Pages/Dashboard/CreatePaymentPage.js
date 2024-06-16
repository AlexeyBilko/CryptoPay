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
        <Button component={RouterLink} to="/dashboard" variant="contained" sx={{ m: 2, bgcolor: '#003366', color: '#FAF8FC' }}>
          Повернутися Назад
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
  const [conversionRate, setConversionRate] = useState({ USD: 0, Crypto: 0 });
  const [error, setError] = useState('');
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const navigate = useNavigate();
  const auth = useAuth();

  const fetchConversionRates = async (currencyCode) => {
    try {
      const response = await axios.post('/PaymentPage/convertToUSD', {
        cryptoAmount: 1,
        currencyCode
      }, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      setConversionRate({ USD: response.data.amountUSD, Crypto: 1 });
    } catch (err) {
      if(currencyCode === "btc"){
          setConversionRate({ USD: 66575.98, Crypto: 1 });
      }
      else {
        setConversionRate({ USD: 3757.98, Crypto: 1 });
      }
      setError('API токени от-от закінчатся');
    }
  };

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
        setError('Не вдалося відобразити платіжну сторінку');
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
      fetchConversionRates(paymentPage.currencyCode);
    }

    const interval = setInterval(() => {
      fetchConversionRates(paymentPage.currencyCode);
    }, 180000); // Кожні 3 хвилини

  }, [id, auth.accessToken, auth.userId, navigate]);

  const handleChange = async (e) => {
    const { name, value, checked, type } = e.target;
    setPaymentPage(prevState => ({
      ...prevState,
      [name]: type === 'checkbox' ? checked : value
    }));

    if (name === 'amountCrypto' && value) {
      setPaymentPage(prevState => ({
        ...prevState,
        amountUSD: (parseFloat(value) * conversionRate.USD).toFixed(2)
      }));
    } else if (name === 'amountUSD' && value) {
      setPaymentPage(prevState => ({
        ...prevState,
        amountCrypto: (parseFloat(value) / conversionRate.USD).toFixed(6)
      }));
    } else if (name === 'currencyCode') {
      await fetchConversionRates(value);
      setPaymentPage(prevState => ({
        ...prevState,
        amountCrypto: '',
        amountUSD: ''
      }));
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
      setError('Не вдалося конвертувати криптовалюту в ГРН: ' + err.response?.data?.Error || err.message);
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
      setError('Не вдалося конвертувати ГРН в криптовалюту: ' + err.response?.data?.Error || err.message);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!paymentPage.isDonation && (paymentPage.amountUSD < 0.5 || paymentPage.amountCrypto < 0.0000099)) {
      setError('Сума USD повинна бути не менше 0.5, а сума Crypto - не менше 0.0000099.');
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
      setError('Не вдалося зберегти платіжну сторінку');
    }
  };

  const handleDelete = async () => {
    try {
      await axios.delete(`/PaymentPage/delete/${id}`, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      navigate('/dashboard');
    } catch (err) {
      setError('Не вдалося видалити платіжну сторінку');
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
        {id ? 'Оновити' : 'Створити'} Платіжну Сторінку
      </Typography>
      {error && <Typography color="error">{error}</Typography>}
      <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
        <TextField
          fullWidth
          label="Назва"
          name="title"
          value={paymentPage.title}
          onChange={handleChange}
          margin="normal"
          required
        />
        <TextField
          fullWidth
          label="Опис"
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
          label="Чи є благодійним збором?"
        />
        {!paymentPage.isDonation && (
          <>
            <TextField
              fullWidth
              label="Сума в USD"
              name="amountUSD"
              value={paymentPage.amountUSD}
              onChange={handleChange}
              margin="normal"
              required
              disabled={!!id} // Disable amount fields when editing
            />
            <TextField
              fullWidth
              label="К-ть криптовалюти"
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
          label="Обрана Криптовалюта"
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
          {id ? 'Оновити' : 'Створити'} Платіжну Сторінку
        </Button>
        {id && (
          <Button
            variant="contained"
            color="primary"
            sx={{ mt: 2, ml: 2, bgcolor: '#E65B40', color: '#FAF8FC'  }}
            onClick={() => setOpenDeleteDialog(true)}
          >
            Видалити Платіжну Сторінку
          </Button>
        )}
      </Box>
      <Dialog
        open={openDeleteDialog}
        onClose={() => setOpenDeleteDialog(false)}
      >
        <DialogTitle sx={{ color: '#003366' }}>Підтвердіть Видалення</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ color: '#E65B40' }}>
          Ви впевнені, що хочете видалити цю платіжну сторінку? Ця дія не може бути скасована.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteDialog(false)} color="primary" sx={{ color: '#003366' }}>
            Відмінити
          </Button>
          <Button onClick={handleDelete} color="secondary" sx={{ color: '#E65B40' }}>Видалити все одно</Button>
        </DialogActions>
      </Dialog>
        </Box>
    </Container>
    </Box>
  );
};

export default CreatePaymentPage;
