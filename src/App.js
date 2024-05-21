import React, { Component } from 'react';
import {
  BrowserRouter as Router,
  Route,
  Routes
} from "react-router-dom";

import LoginPage from './Pages/AuthPage/LoginPage';
import RegisterPage from './Pages/AuthPage/RegisterPage';
import Dashboard from './Pages/Dashboard/Dashboard';
import CreatePaymentPage from './Pages/Dashboard/CreatePaymentPage';
import ProfilePage from './Pages/Dashboard/ProfilePage';
import PaymentPage from './Pages/PaymentPage/PaymentPage';
import ThankYou from './Pages/PaymentPage/ThankYou';
import NotFound from './Pages/ErrorPages/NotFound';
import PaymentPageTransactions from './Pages/PaymentPage/PaymentPageTransactions'
import EarningsPage from './Pages/Dashboard/EarningsPage'


function App() {
  return (
      <Router basename="https://alexeybilko.github.io/cryptopay">
        <Routes>
          <Route path="/" element={<LoginPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/profile" element={<ProfilePage />}/>
          <Route path="/earnings" element={<EarningsPage />}/>
          <Route path="/create-payment-page" element={<CreatePaymentPage />} />
          <Route path="/edit-payment-page/:id" element={<CreatePaymentPage />} />
          <Route path="/payment-page-transactions/:id" element={<PaymentPageTransactions />} />
          <Route path="/payment/:id" element={<PaymentPage />} />
          <Route path="/thank-you" element={<ThankYou />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </Router>
  );
}

export default App;
