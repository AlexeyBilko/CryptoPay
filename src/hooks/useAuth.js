import { useState, useEffect } from 'react';
import axios from '../api/axios';
import { useNavigate } from 'react-router-dom';

const useAuth = () => {
  const [auth, setAuth] = useState({
    accessToken: localStorage.getItem('accessToken'),
    refreshToken: localStorage.getItem('refreshToken'),
    userId: localStorage.getItem('userId')
  });
  const navigate = useNavigate();

  useEffect(() => {
    const verifyToken = async () => {
      try {
        const response = await axios.post('/auth/verify-token', { token: auth.accessToken });
        if (response.status !== 200 || !response.data.isValid) {
          throw new Error('Token verification failed');
        }
        const userResponse = await axios.get('/auth/user-details', {
          headers: { Authorization: `Bearer ${auth.accessToken}` }
        });
        localStorage.setItem('userId', userResponse.data.id);
        setAuth(auth => ({
          ...auth,
          userId: userResponse.data.id
        }));
      } catch (err) {
        try {
          const response = await axios.post('/auth/renew-token', { token: auth.refreshToken });
          localStorage.setItem('accessToken', response.data.accessToken);
          setAuth({
            ...auth,
            accessToken: response.data.accessToken
          });
        } catch (refreshError) {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('userId');
          navigate('/login');
        }
      }
    };

    if (auth.accessToken) {
      verifyToken();
    } else {
      navigate('/login');
    }
  }, [auth.accessToken, auth.refreshToken, navigate]);

  return auth;
};

export default useAuth;
