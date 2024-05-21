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
        const response = await axios.get(`/Transaction/bypage/${id}`, {
            headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        setTransactions(response.data);
      } catch (err) {
        setError('Failed to fetch transactions.');
      }
    };

    fetchTransactions();
  }, [id, auth.accessToken]);

  return (
    <Container component="main" maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>
        Transactions for Payment Page {id}
      </Typography>
      {error && <Typography color="error">{error}</Typography>}
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Transaction ID</TableCell>
            <TableCell>From Wallet</TableCell>
            <TableCell>To Wallet</TableCell>
            <TableCell>Amount</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Timestamp</TableCell>
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
          Back to Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default PaymentPageTransactions;
