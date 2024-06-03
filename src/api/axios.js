import axios from 'axios';


//const API_URL = 'http://localhost:5000/api';
const API_URL = 'https://cryptopaymentgatewaybackend.azurewebsites.net/api';


const axiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default axiosInstance;
