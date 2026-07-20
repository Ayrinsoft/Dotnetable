import { createContext, useContext } from 'react';

// Static design tokens for the React SPA. Site-wide theming for the MVC storefront is
// WordPress-style package themes (Admin → Themes). This SPA keeps fixed CSS variables only.
const DEFAULT_TOKENS = {
  colors: {
    primary: '#4f46e5',
    secondary: '#0ea5e9',
    background: '#ffffff',
    surface: '#f8fafc',
    text: '#1f2937',
    muted: '#6b7280',
  },
  fontFamily: "system-ui, -apple-system, 'Segoe UI', sans-serif",
  radius: '12px',
  darkMode: false,
};

const ThemeContext = createContext(DEFAULT_TOKENS);
export const useTheme = () => useContext(ThemeContext);

export function ThemeProvider({ children }) {
  return <ThemeContext.Provider value={DEFAULT_TOKENS}>{children}</ThemeContext.Provider>;
}
