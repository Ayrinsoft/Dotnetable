// Thin fetch wrapper for the Dotnetable API. Every request carries the per-site key header
// (X-Website-Key = the website's AuthCode) so the API can resolve the caller website.

const BASE_URL = (import.meta.env.VITE_API_URL || '').replace(/\/+$/, '');
const WEBSITE_KEY = import.meta.env.VITE_WEBSITE_KEY || '';

async function request(path, options = {}) {
  const response = await fetch(`${BASE_URL}/${path.replace(/^\/+/, '')}`, {
    ...options,
    headers: {
      'X-Website-Key': WEBSITE_KEY,
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  });

  if (response.status === 204) return null;
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const error = new Error(data?.message || `Request failed (${response.status})`);
    error.status = response.status;
    error.data = data;
    throw error;
  }
  return data;
}

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: 'POST', body: JSON.stringify(body) }),

  // ── Site chrome ───────────────────────────────────────────────
  siteInfo: () => request('api/siteinfo').catch(() => null),
  menu: (location) => request(`api/menu/${encodeURIComponent(location)}`).catch(() => null),
  activeTheme: () => request('api/theme/active').catch(() => null),

  // ── Content ───────────────────────────────────────────────────
  posts: (page = 1, pageSize = 12) => request(`api/posts?page=${page}&pageSize=${pageSize}`),
  post: (slug) => request(`api/posts/${encodeURIComponent(slug)}`),
  page: (slug) => request(`api/pages/${encodeURIComponent(slug)}`),
  slideshowByPlacement: (key) => request(`api/slideshow/placement/${encodeURIComponent(key)}`).catch(() => null),

  // ── Shop ──────────────────────────────────────────────────────
  products: (page = 1, pageSize = 12) => request(`api/products?page=${page}&pageSize=${pageSize}`),
  product: (slug) => request(`api/products/${encodeURIComponent(slug)}`),

  // ── Dynamic forms & surveys ───────────────────────────────────
  formBySlug: (slug) => request(`api/forms/${encodeURIComponent(slug)}`),
  submitForm: (formId, answers) => request(`api/forms/${formId}/submit`, {
    method: 'POST',
    body: JSON.stringify({ answers }),
  }),
  formResults: (formId) => request(`api/forms/${formId}/results`).catch(() => null),

  // ── Contact ───────────────────────────────────────────────────
  contact: (message) => request('api/contact', { method: 'POST', body: JSON.stringify(message) }),
  captchaChallenge: () => request('api/captcha/challenge'),
};
