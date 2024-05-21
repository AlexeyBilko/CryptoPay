import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Typography, TextField, Button, Box, Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions, Paper } from '@mui/material';
import axios from '../../api/axios';

const PaymentPage = () => {
  const { id } = useParams();
  const [paymentPage, setPaymentPage] = useState({});
  const [guestWalletAddress, setGuestWalletAddress] = useState('');
  const [senderEmailAddress, setSenderEmailAddress] = useState('');
  const [copyText, setCopyText] = useState('Copy');
  const [copyCryptoText, setCopyCryptoText] = useState('Copy');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMessage, setDialogMessage] = useState('');

  const navigate = useNavigate();

  useEffect(() => {
    const fetchPaymentPage = async () => {
      try {
        const response = await axios.get(`/PaymentPage/${id}`);
        setPaymentPage(response.data);
      } catch (err) {
        console.error('Failed to fetch payment page:', err);
      }
    };

    if (id) {
      fetchPaymentPage();
    }
  }, [id]);

  const handleCopy = (text, type) => {
    navigator.clipboard.writeText(text).then(() => {
      if (type === 'address') {
        setCopyText('Copied!');
        setTimeout(() => setCopyText('Copy'), 2000);
      } else if (type === 'crypto') {
        setCopyCryptoText('Copied!');
        setTimeout(() => setCopyCryptoText('Copy'), 2000);
      }
    });
  };

  const handleVerifyPayment = async () => {
    if (!guestWalletAddress || !senderEmailAddress) {
      setDialogMessage('Please provide both wallet address and email address.');
      setDialogOpen(true);
      return;
    }

    try {
      const requestBody = {
        pageId: id,
        type: paymentPage.amountDetails.currency.currencyCode.toLowerCase(),
        fromWallet: guestWalletAddress,
        toWallet: paymentPage.systemWallet.walletNumber,
        amountCrypto: paymentPage.amountDetails.amountCrypto,
        senderEmail: senderEmailAddress,
        isTestnet: true,
        isDonation: paymentPage.isDonation
      };
      
      const response = await axios.post('/Transaction/verify-tr', requestBody);

      if (response.data.status === 'not found') {
        setDialogMessage('Transaction not found.');
      } else if (response.data.status === 'pending') {
        setDialogMessage('Transaction pending.');
      } else if (response.data.status === 'successful') {
        navigate('/thank-you', { state: { paymentPage, guestWalletAddress, senderEmailAddress } });
      }
    } catch (err) {
      setDialogMessage('Failed to verify payment.');
    }
    setDialogOpen(true);
  };

  return (
    <Container component="main" maxWidth="md" sx={{
      height: '100vh',
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'center',
      alignItems: 'center'
    }}>
      <Paper sx={{ padding: 4, width: '100%', textAlign: 'center', backgroundColor: '#f9f9f9' }}>
        <Typography component="h1" variant="h4" color="primary">{paymentPage.title}</Typography>
        <Typography component="h2" variant="h6" color="textSecondary">{paymentPage.description}</Typography>
        <Box sx={{ mt: 4 }}>
          <Typography variant="body1" sx={{ fontWeight: 'bold' }}>Cryptocurrency: {paymentPage.amountDetails?.currency?.currencyCode}</Typography>
          {!paymentPage.isDonation && (
            <>
              <Typography variant="body1">Amount USD: {paymentPage.amountDetails?.amountUSD}</Typography>
              <TextField
                fullWidth
                label="Amount Crypto:"
                value={paymentPage.amountDetails?.amountCrypto || ''}
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <Button onClick={() => handleCopy(paymentPage.amountDetails?.amountCrypto, 'crypto')}>{copyCryptoText}</Button>
                  )
                }}
                sx={{ mt: 2 }}
              />
            </>
          )}
          <TextField
            fullWidth
            label="System Wallet Address"
            value={paymentPage.systemWallet?.walletNumber || ''}
            InputProps={{
              readOnly: true,
              endAdornment: (
                <Button onClick={() => handleCopy(paymentPage.systemWallet?.walletNumber, 'address')}>{copyText}</Button>
              )
            }}
            sx={{ mt: 2 }}
          />
          <TextField
            fullWidth
            label="Your Wallet Address"
            value={guestWalletAddress}
            onChange={(e) => setGuestWalletAddress(e.target.value)}
            sx={{ mt: 2 }}
          />
          <TextField
            fullWidth
            label="Your Email Address"
            value={senderEmailAddress}
            onChange={(e) => setSenderEmailAddress(e.target.value)}
            sx={{ mt: 2 }}
          />
          <Button variant="contained" color="primary" onClick={handleVerifyPayment} sx={{ mt: 2 }}>Verify Payment</Button>
        </Box>
      </Paper>
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)}>
        <DialogTitle>Payment Status</DialogTitle>
        <DialogContent>
          <DialogContentText>{dialogMessage}</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)} color="primary">Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default PaymentPage;
