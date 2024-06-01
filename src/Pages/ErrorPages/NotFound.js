// src/Pages/NotFound.js
import React from 'react';
import { Container, Box, Typography, Button } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { styled } from '@mui/system';

const NotFoundContainer = styled(Container)(({ theme }) => ({
  textAlign: 'center',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100vh',
  backgroundColor: theme.palette.background.paper,
  padding: theme.spacing(4),
}));

const Illustration = styled('img')({
  maxWidth: '100%',
  height: 'auto',
  marginBottom: '20px',
});

const NotFound = () => {
  return (
    <NotFoundContainer>
      <Illustration src="/assets/404-illustration.png" alt="404 Not Found" />
      <Typography variant="h1" color="primary" gutterBottom>
        404
      </Typography>
      <Typography variant="h5" color="textSecondary" paragraph>
        Ой! Сторінка, яку ви шукаєте, не існує.
      </Typography>
      <Typography variant="body1" color="textSecondary" paragraph>
        Схоже, що сторінка, на яку ви намагаєтеся перейти, неіснує або недоступна.
      </Typography>
      <Button component={RouterLink} to="/" variant="contained" color="primary">
        Повернутися на головну сторінку
      </Button>
    </NotFoundContainer>
  );
};

export default NotFound;
