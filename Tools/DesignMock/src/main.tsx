import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

// Import the existing CSS (tokens, base, core, feature styles)
import '../styles/design-mock.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
