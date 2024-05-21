import axios from 'axios';

const API_URL = 'https://cryptopaymentgatewaybackend.azurewebsites.net';//'http://localhost:5000/api'; // Adjust according to API endpoint

const axiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default axiosInstance;
