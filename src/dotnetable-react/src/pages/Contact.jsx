import { useEffect, useRef, useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { api } from '../api/client.js';

const PENDING_KEY = 'dotnetable:contact:pending';
const PENDING_MS = 24 * 60 * 60 * 1000;

function readPending() {
  try {
    const raw = localStorage.getItem(PENDING_KEY);
    if (!raw) return null;
    const data = JSON.parse(raw);
    if (!data?.submittedAt || Date.now() - data.submittedAt > PENDING_MS) {
      localStorage.removeItem(PENDING_KEY);
      return null;
    }
    return data;
  } catch {
    return null;
  }
}

let turnstileScriptPromise = null;
function loadTurnstileScript() {
  if (window.turnstile) return Promise.resolve();
  if (turnstileScriptPromise) return turnstileScriptPromise;
  turnstileScriptPromise = new Promise((resolve, reject) => {
    const s = document.createElement('script');
    s.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js';
    s.async = true;
    s.defer = true;
    s.onload = resolve;
    s.onerror = reject;
    document.head.appendChild(s);
  });
  return turnstileScriptPromise;
}

/** Contact form posting into the API contact inbox (Admin → Messages). */
export default function Contact() {
  const { site } = useOutletContext();
  const [values, setValues] = useState({ name: '', email: '', phone: '', subject: '', message: '', website: '' });
  const [status, setStatus] = useState({ state: 'idle' });
  const [pending, setPending] = useState(readPending);
  const [challenge, setChallenge] = useState(null);
  const [mathAnswer, setMathAnswer] = useState('');
  const turnstileHost = useRef(null);
  const turnstileWidgetId = useRef(null);

  const set = (key) => (event) => setValues((prev) => ({ ...prev, [key]: event.target.value }));

  async function fetchChallenge() {
    try {
      const c = await api.captchaChallenge();
      setChallenge(c);
    } catch {
      setChallenge(null);
    }
  }

  useEffect(() => {
    if (!pending) fetchChallenge();
  }, [pending]);

  useEffect(() => {
    if (challenge?.provider !== 'turnstile' || !challenge.siteKey || !turnstileHost.current) return;
    let cancelled = false;
    loadTurnstileScript()
      .then(() => {
        if (cancelled || !turnstileHost.current) return;
        turnstileWidgetId.current = window.turnstile.render(turnstileHost.current, { sitekey: challenge.siteKey });
      })
      .catch(fetchChallenge);
    return () => {
      cancelled = true;
      if (turnstileWidgetId.current != null && window.turnstile) {
        window.turnstile.remove(turnstileWidgetId.current);
        turnstileWidgetId.current = null;
      }
    };
  }, [challenge]);

  function resetCaptcha() {
    if (challenge?.provider === 'turnstile' && turnstileWidgetId.current != null && window.turnstile) {
      window.turnstile.reset(turnstileWidgetId.current);
    } else {
      fetchChallenge();
    }
  }

  async function submit(event) {
    event.preventDefault();
    setStatus({ state: 'saving' });

    const captchaToken = challenge?.provider === 'turnstile'
      ? (turnstileWidgetId.current != null && window.turnstile ? window.turnstile.getResponse(turnstileWidgetId.current) : null)
      : challenge?.token;

    const message = {
      senderName: values.name,
      emailAddress: values.email,
      cellphoneNumber: values.phone,
      messageSubject: values.subject,
      messageBody: values.message,
      website: values.website,
      captchaToken,
      captchaAnswer: mathAnswer,
    };

    try {
      await api.contact(message);
      const record = { fields: message, submittedAt: Date.now() };
      try { localStorage.setItem(PENDING_KEY, JSON.stringify(record)); } catch { /* storage unavailable */ }
      setPending(record);
      setStatus({ state: 'idle' });
    } catch (error) {
      setStatus({ state: 'error', message: error.data?.message || error.message });
      resetCaptcha();
    }
  }

  if (pending) {
    return (
      <section className="section container" style={{ maxWidth: 760 }}>
        <h1>Contact us</h1>
        <p className="muted">Your last message:</p>
        <div className="card">
          <div style={{ fontWeight: 600 }}>{pending.fields.messageSubject || '(no subject)'}</div>
          <div className="muted" style={{ fontSize: '0.9em', marginBottom: 8 }}>
            {pending.fields.senderName} — {new Date(pending.submittedAt).toLocaleString()}
          </div>
          <div style={{ whiteSpace: 'pre-wrap' }}>{pending.fields.messageBody}</div>
        </div>
        <div className="alert" style={{ marginTop: 12 }}>Awaiting review by the site admin.</div>
      </section>
    );
  }

  return (
    <section className="section container" style={{ maxWidth: 760 }}>
      <h1>Contact us</h1>
      {site?.email && <p className="muted">Or email us directly at <a href={`mailto:${site.email}`}>{site.email}</a>.</p>}

      {status.state === 'error' && <div className="alert error">{status.message}</div>}

      <form className="card" onSubmit={submit}>
        <div className="field">
          <label htmlFor="c-name">Name *</label>
          <input id="c-name" type="text" required value={values.name} onChange={set('name')} />
        </div>
        <div className="field">
          <label htmlFor="c-email">Email *</label>
          <input id="c-email" type="email" required value={values.email} onChange={set('email')} />
        </div>
        <div className="field">
          <label htmlFor="c-phone">Phone</label>
          <input id="c-phone" type="tel" value={values.phone} onChange={set('phone')} />
        </div>
        <div className="field">
          <label htmlFor="c-subject">Subject</label>
          <input id="c-subject" type="text" value={values.subject} onChange={set('subject')} />
        </div>
        <div className="field">
          <label htmlFor="c-message">Message *</label>
          <textarea id="c-message" rows={5} required value={values.message} onChange={set('message')} />
        </div>

        {/* Honeypot: hidden off-screen (not display:none) so simple bots that skip hidden fields still fill it in. */}
        <div style={{ position: 'absolute', left: -9999, top: -9999 }} aria-hidden="true">
          <label htmlFor="c-website">Website</label>
          <input id="c-website" type="text" tabIndex={-1} autoComplete="off" value={values.website} onChange={set('website')} />
        </div>

        {challenge?.provider === 'turnstile' && <div className="field" ref={turnstileHost} />}
        {challenge?.provider === 'math' && (
          <div className="field">
            <label>Solve the captcha</label>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <span dangerouslySetInnerHTML={{ __html: challenge.svg }} />
              <input
                type="text"
                inputMode="numeric"
                autoComplete="off"
                placeholder="?"
                style={{ maxWidth: 100 }}
                value={mathAnswer}
                onChange={(e) => setMathAnswer(e.target.value)}
              />
            </div>
          </div>
        )}

        <button className="btn" type="submit" disabled={status.state === 'saving'}>Send message</button>
      </form>
    </section>
  );
}
