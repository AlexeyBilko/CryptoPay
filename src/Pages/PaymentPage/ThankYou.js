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
          Дякуємо за ваш платіж!
        </Typography>
        <Typography variant="body1" sx={{ marginTop: 2 }}>
          Ваш платіж було успішно оброблено. Нижче наведені деталі вашої транзакції:
        </Typography>
        <Box sx={{ marginTop: 2 }}>
          <Typography variant="h6">Детальна інформація про транзакцію</Typography>
          <Typography variant="body1">Назва: {paymentPage?.title}</Typography>
          <Typography variant="body1">Опис: {paymentPage?.description}</Typography>
          <Typography variant="body1">Криптовалюта: {paymentPage?.amountDetails?.currency?.currencyCode}</Typography>
          <Typography variant="body1">Сума в ГРН: {paymentPage?.amountDetails?.amountUSD}</Typography>
          <Typography variant="body1">Кількість криптовалюти: {paymentPage?.amountDetails?.amountCrypto}</Typography>
          <Typography variant="body1">Адреса гаманця "До": {paymentPage?.systemWallet?.walletNumber}</Typography>
          <Typography variant="body1">Адреса гаманця "З" - Ваша: {guestWalletAddress}</Typography>
          <Typography variant="body1">Вага електронна адреса: {senderEmailAddress}</Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default ThankYouPage;
