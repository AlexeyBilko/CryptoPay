// src/Pages/ThankYouPage.js
import React from 'react';
import { Container, Typography, Box, Paper } from '@mui/material';
import { useLocation } from 'react-router-dom';

const ThankYouPage = () => {
  const location = useLocation();
  const { paymentPage, guestWalletAddress, senderEmailAddress } = location.state || {};

  return (
    <Container component="main" maxWidth="sm" sx={{ marginTop: 8 }}>
      <Paper sx={{ padding: 4 }}>
        <Typography component="h1" variant="h4" align="center">
          Thank You for Your Payment!
        </Typography>
        <Typography variant="body1" sx={{ marginTop: 2 }}>
          Your payment has been successfully processed. Below are the details of your transaction:
        </Typography>
        <Box sx={{ marginTop: 2 }}>
          <Typography variant="h6">Transaction Details</Typography>
          <Typography variant="body1">Title: {paymentPage?.title}</Typography>
          <Typography variant="body1">Description: {paymentPage?.description}</Typography>
          <Typography variant="body1">Cryptocurrency: {paymentPage?.amountDetails?.currency?.currencyCode}</Typography>
          <Typography variant="body1">Amount USD: {paymentPage?.amountDetails?.amountUSD}</Typography>
          <Typography variant="body1">Amount Crypto: {paymentPage?.amountDetails?.amountCrypto}</Typography>
          <Typography variant="body1">System Wallet Address: {paymentPage?.systemWallet?.walletNumber}</Typography>
          <Typography variant="body1">Your Wallet Address: {guestWalletAddress}</Typography>
          <Typography variant="body1">Your Email: {senderEmailAddress}</Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default ThankYouPage;
