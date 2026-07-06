import { createContext, useContext, useEffect, useState } from 'react';
import { api } from '../api/client.js';

// Built-in defaults, used until the admin activates a theme (Admin -> Themes).
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

/** Applies a token bag as CSS custom properties on <html>, so plain CSS uses var(--color-primary) etc. */
function applyTokens(tokens) {
  const root = document.documentElement;
  for (const [name, value] of Object.entries(tokens.colors ?? {})) {
    root.style.setProperty(`--color-${name}`, value);
  }
  if (tokens.fontFamily) root.style.setProperty('--font-family', tokens.fontFamily);
  if (tokens.radius) root.style.setProperty('--radius', tokens.radius);
  root.dataset.theme = tokens.darkMode ? 'dark' : 'light';
}

/**
 * Fetches the active admin-managed theme on boot and applies it at runtime. Because this happens
 * in the browser, a statically hosted (serverless) deployment is re-themed from the admin without
 * a rebuild.
 */
export function ThemeProvider({ children }) {
  const [tokens, setTokens] = useState(DEFAULT_TOKENS);

  useEffect(() => {
    applyTokens(DEFAULT_TOKENS);
    api.activeTheme().then((theme) => {
      if (!theme?.settings) return;
      try {
        const parsed = typeof theme.settings === 'string' ? JSON.parse(theme.settings) : theme.settings;
        const merged = {
          ...DEFAULT_TOKENS,
          ...parsed,
          colors: { ...DEFAULT_TOKENS.colors, ...(parsed.colors ?? {}) },
        };
        setTokens(merged);
        applyTokens(merged);
      } catch {
        // Malformed theme JSON — keep defaults.
      }
    });
  }, []);

  return <ThemeContext.Provider value={tokens}>{children}</ThemeContext.Provider>;
}
