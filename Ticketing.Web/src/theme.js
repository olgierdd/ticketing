import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1565c0'
    },
    secondary: {
      main: '#00897b'
    },
    background: {
      default: '#f5f7fb',
      paper: '#ffffff'
    }
  },
  shape: {
    borderRadius: 12
  },
  typography: {
    fontFamily: 'Inter, Roboto, Arial, sans-serif',
    h4: {
      fontWeight: 700
    },
    h6: {
      fontWeight: 600
    }
  },
  components: {
    MuiPaper: {
      defaultProps: {
        elevation: 0
      },
      styleOverrides: {
        root: {
          border: '1px solid #e6ebf2'
        }
      }
    }
  }
});

