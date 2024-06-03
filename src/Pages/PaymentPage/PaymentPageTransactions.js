// src/Pages/PaymentPage/PaymentPageTransactions.js
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Typography, Table, TableBody, TableCell, TableHead, TableRow, Button, Box } from '@mui/material';
import axios from '../../api/axios';
import useAuth from '../../hooks/useAuth';

const PaymentPageTransactions = () => {
  const { id } = useParams();
  const [transactions, setTransactions] = useState([]);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const auth = useAuth();

  useEffect(() => {
    const fetchTransactions = async () => {
      try {
        if(id === "all"){
          const response = await axios.get('/Transaction/all', { headers: { Authorization: `Bearer ${auth.accessToken}` } });
          setTransactions(response.data);
        }
        else{
          const response = await axios.get(`/Transaction/bypage/${id}`, { headers: { Authorization: `Bearer ${auth.accessToken}` } });
          setTransactions(response.data);
        }
      } catch (err) {
        setError('Не вдалося завантажити транзакції.');
      }
    };

    fetchTransactions();
  }, [id, auth.accessToken]);

  return (
    <Container component="main" maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>
        {id ? `Транзакції для платіжної сторінки з ID: ${id}` : 'Всі транзакції'}
      </Typography>
      {error && <Typography color="error">{error}</Typography>}
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>ID Транзакції</TableCell>
            <TableCell>З Гаманця</TableCell>
            <TableCell>До Гаманця</TableCell>
            <TableCell>К-ть</TableCell>
            <TableCell>Статус</TableCell>
            <TableCell>Дата</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {transactions.map((transaction) => (
            <TableRow key={transaction.id}>
              <TableCell>{transaction.id}</TableCell>
              <TableCell>{transaction.senderWalletAddress}</TableCell>
              <TableCell>{transaction.paymentPage.systemWallet.walletNumber}</TableCell>
              <TableCell>{transaction.actualAmountCrypto}</TableCell>
              <TableCell>{transaction.status}</TableCell>
              <TableCell>{new Date(transaction.blockTimestamp).toLocaleString()}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <Box mt={2}>
        <Button variant="contained" color="primary" onClick={() => navigate('/dashboard')}>
          Назад до інформаційної панелі
        </Button>
      </Box>
    </Container>
  );
};

export default PaymentPageTransactions;
