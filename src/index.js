import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <BrowserRouter basename="https://alexeybilko.github.io/cryptopay">
    <App />
    </BrowserRouter>
);