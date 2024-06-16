import React, { useState, useEffect } from 'react';
import { Container, AppBar, Divider, Toolbar, Button, Typography, Table, TableBody, TableCell, TableHead, TableRow, Link, Box, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField, MenuItem, IconButton, CircularProgress } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import axios from '../../api/axios';
import useAuth from '../../hooks/useAuth';
import { Link as RouterLink } from 'react-router-dom';
import UpdateIcon from '@mui/icons-material/Update';

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

const EarningsPage = () => {
  const [earnings, setEarnings] = useState(null);
  const [withdrawals, setWithdrawals] = useState([]);
  const [error, setError] = useState('');
  const [withdrawError, setWithdrawError] = useState('');
  const [walletError, setWalletError] = useState('');
  const [openWithdrawDialog, setOpenWithdrawDialog] = useState(false);
  const [withdrawAmount, setWithdrawAmount] = useState('');
  const [withdrawCurrency, setWithdrawCurrency] = useState('BTC');
  const [withdrawWallet, setWithdrawWallet] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [loading, setLoading] = useState(false);
  const auth = useAuth();

  useEffect(() => {
    const fetchEarnings = async () => {
      try {
        const response = await axios.get('/Earnings/view-earnings', {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setEarnings(response.data);
      } catch (err) {
        setError('Не вдалося одержати інформацію про заробіток');
      }
    };

    const fetchWithdrawals = async () => {
      try {
        const response = await axios.get('/Earnings/view-withdrawal-history', {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setWithdrawals(response.data);
      } catch (err) {
        if (err.response.data === "No withdrawal history found." && err.response.status === 404) {
          console.log('No withdrawals found for this user.');
        } else {
          setError('Не вдалося отримати інформацію про виведення коштів');
        }
      }
    };

    fetchEarnings();
    fetchWithdrawals();
  }, [auth.accessToken]);

  const handleWithdraw = async () => {
    if (!withdrawWallet) {
      setWalletError("Адреса гаманця є обов'язковою");
      return;
    }

    if (withdrawCurrency === 'BTC' && parseFloat(withdrawAmount) > earnings.currentBalanceBTC) {
      setWithdrawError(`Недостатній баланс. Поточний баланс BTC: ${earnings.currentBalanceBTC}`);
      return;
    }
  
    if (withdrawCurrency === 'ETH' && parseFloat(withdrawAmount) > earnings.currentBalanceETH) {
      setWithdrawError(`Недостатній баланс. Поточний баланс ETH: ${earnings.currentBalanceETH}`);
      return;
    }
  
    setLoading(true);
    try {
      await axios.post('/Earnings/withdraw-earnings', {
        WalletNumber: withdrawWallet,
        Amount: parseFloat(withdrawAmount),
        CurrencyCode: withdrawCurrency
      }, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      setOpenWithdrawDialog(false);
      setWithdrawError('');
      window.location.reload();
    } catch (err) {
      setWithdrawError('Не вдалося вивести зароблену криптовалюту.');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (transactionId) => {
    try {
      const response = await axios.get(`/Earnings/check-transaction-status/${transactionId}`, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });

      const updatedStatus = response.data.Status;
      const completedDate = response.data.CompletedDate;
  
      setWithdrawals(prevWithdrawals => 
        prevWithdrawals.map(w => 
          w.id === transactionId ? { ...w, status: updatedStatus, completedDate: completedDate ? new Date(completedDate) : w.completedDate } : w
        )
      );
    } catch (err) {
      setError('Не вдалося оновити стан транзакції');
    }
  };

  const handleGenerateReport = async () => {
    if (new Date(startDate) >= new Date(endDate)){
      setError('Початкова дата має бути перед Кінцевою датою');
      return;
    }
    
    setIsLoading(true);
    try {
      const axios1 = axios.create({
        baseURL: "http://localhost:5000/api",
        headers: {
          'Content-Type': 'application/json'
        }
      });
      const response = await axios1.get('/Earnings/generate-earnings-report', {
        params: {
          startDate: startDate,
          endDate: endDate
        },
        responseType: 'arraybuffer',
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      setIsLoading(false);
      console.log(response);
      console.log(response.data);
      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'EarningsReport.pdf');
      document.body.appendChild(link);
      link.click();
    } catch (err) {
        setError('Не вдалося сформувати звіт (Закінчилися API токени, спробуйте пізніше)');
        setIsLoading(false);
    }
  };

  const handleLogout = async (e) => {
    e.preventDefault();
    try {
      await axios.post('/auth/logout', {}, {
        headers: { Authorization: `Bearer ${auth.accessToken}` }
      });
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    navigate('/login');
    } catch (err) {
      setError('Не вдалося вийти з облікового запису');
    }
  };

  return (
    <Box sx={{ bgcolor: '#FAF8FC', minHeight: '100vh', color: '#003366', fontFamily: 'Montserrat, sans-serif' }}>
      <Navigation handleLogout={handleLogout} />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {error && <Typography color="error">{error}</Typography>}
        <Typography variant="h4" sx={{ mb: 2, color: '#003366' }}>Інформація про заробіток</Typography>
        {earnings && (
          <Box sx={{ mb: 4 }}>
            <Typography variant="h6">Загальний заробіток</Typography>
            <Typography variant="body1">Загальна кількість зароблених BTC: {earnings.totalEarnedBTC}</Typography>
            <Typography variant="body1">Загальна кількість зароблених ETH: {earnings.totalEarnedETH}</Typography>
            <Typography variant="h6">Поточний баланс</Typography>
            <Typography variant="body1">Поточний баланс BTC: {earnings.currentBalanceBTC}</Typography>
            <Typography variant="body1">Поточний баланс ETH: {earnings.currentBalanceETH}</Typography>
          </Box>
        )}
        <Button variant="contained" sx={{ mb: 2, bgcolor: '#003366', color: '#FAF8FC' }} onClick={() => setOpenWithdrawDialog(true)}>
          Вивести зароблену криптовалюту
        </Button>
        <Divider sx={{ my: 4 }} />
        <Box sx={{ display: 'flex', my: 2 }}>
          <TextField
            label="Початкова дата"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            sx={{ mr: 2 }}
          />
          <TextField
            label="Кінцева дата"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            sx={{ mr: 2 }}
            inputProps={{ max: new Date().toISOString().split("T")[0] }} // Restrict future dates
          />
        </Box>
        <Button variant="contained" sx={{ mb: 2, bgcolor: '#003366', color: '#FAF8FC' }} onClick={handleGenerateReport} disabled={isLoading}>
          {isLoading ? <CircularProgress size={24} /> : 'Згенерувати PDF звіт'}
        </Button>
        <Divider sx={{ my: 4 }} />
        <Typography variant="h4" sx={{ mb: 2, color: '#003366' }}>Історія виведення криптовалюти</Typography>
        {withdrawals.length === 0 ? (
          <Typography variant="body1">Виведення криптовалют ще не здійснювалися.</Typography>) : (
          <Table sx={{ minWidth: 650, bgcolor: '#FAF8FC', borderRadius: 2 }}>
            <TableHead>
              <TableRow>
                <TableCell sx={{ color: '#003366' }}>ID</TableCell>
                <TableCell sx={{ color: '#003366' }}>К-ть</TableCell>
                <TableCell sx={{ color: '#003366' }}>Криптовалюта</TableCell>
                <TableCell sx={{ color: '#003366' }}>Статус</TableCell>
                <TableCell sx={{ color: '#003366' }}>Дата запиту</TableCell>
                <TableCell sx={{ color: '#003366' }}>Дата завершення</TableCell>
                <TableCell sx={{ color: '#003366' }}>Хеш транзакції</TableCell>
                <TableCell sx={{ color: '#003366' }}>Оновити статус</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {withdrawals.map((withdrawal) => (
                <TableRow key={withdrawal.id}>
                  <TableCell sx={{ color: '#003366' }}>{withdrawal.id}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>{withdrawal.amountDetails.amountCrypto}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>{withdrawal.amountDetails.currency.currencyCode}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>{withdrawal.status}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>{new Date(withdrawal.requestedDate).toLocaleString()}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>{withdrawal.completedDate ? new Date(withdrawal.completedDate).toLocaleDateString() : 'Pending'}</TableCell>
                  <TableCell sx={{ color: '#003366' }}>
                    <Link 
                      href={withdrawal.amountDetails.currency.currencyCode === 'BTC' ? 
                        `https://live.blockcypher.com/btc-testnet/tx/${withdrawal.transactionId}` : 
                        `https://sepolia.etherscan.io/tx/${withdrawal.transactionId}`
                      } 
                      target="_blank" 
                      sx={{ color: '#003366' }}
                    >
                      {`${withdrawal.transactionId.slice(0, 6)}...`}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <IconButton onClick={() => handleUpdateStatus(withdrawal.id)} sx={{ color: '#003366' }}>
                      <UpdateIcon />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
        <Divider sx={{ my: 4 }} />
        <Dialog
          open={openWithdrawDialog}
          onClose={() => setOpenWithdrawDialog(false)}
        >
          <DialogTitle>Виведення криптовалюти</DialogTitle>
          <DialogContent>
            <DialogContentText>
              Введіть дані для виведення заробленої криптовалюти.
            </DialogContentText>
            {withdrawError && <Typography color="error">{withdrawError}</Typography>}
            <Typography variant="body1">
              Поточний баланс BTC: {earnings ? earnings.currentBalanceBTC : 'Loading...'}
            </Typography>
            <Typography variant="body1">
              Поточний баланс ETH: {earnings ? earnings.currentBalanceETH : 'Loading...'}
            </Typography>
            <TextField
              autoFocus
              margin="dense"
              label="Кількість"
              type="number"
              fullWidth
              value={withdrawAmount}
              onChange={(e) => setWithdrawAmount(e.target.value)}
            />
            <TextField
              select
              margin="dense"
              label="Криптовалюта"
              fullWidth
              value={withdrawCurrency}
              onChange={(e) => setWithdrawCurrency(e.target.value)}
            >
              <MenuItem value="BTC">BTC</MenuItem>
              <MenuItem value="ETH">ETH</MenuItem>
            </TextField>
            <TextField
              margin="dense"
              label="Адреса Гаманця"
              type="text"
              fullWidth
              value={withdrawWallet}
              onChange={(e) => setWithdrawWallet(e.target.value)}
            />
            {walletError && <Typography color="error">{walletError}</Typography>}
            <DialogContentText sx={{ mt: 2, color: '#E65B40' }}>
            Дуже важливо вставити правильну адресу гаманця для правильної криптовалюті. Якщо ви помилитеся, криптовалюта буде втрачена.
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenWithdrawDialog(false)} color="primary">
              Відмінити
            </Button>
            <Button onClick={handleWithdraw} color="primary">
              {loading ? <CircularProgress size={24} /> : 'Вивести'}
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  );
};

export default EarningsPage;

