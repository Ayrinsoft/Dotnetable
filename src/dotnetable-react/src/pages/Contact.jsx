import { useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { api } from '../api/client.js';

/** Contact form posting into the API contact inbox (Admin → Messages). */
export default function Contact() {
  const { site } = useOutletContext();
  const [values, setValues] = useState({ name: '', email: '', phone: '', subject: '', message: '' });
  const [status, setStatus] = useState({ state: 'idle' });

  const set = (key) => (event) => setValues((prev) => ({ ...prev, [key]: event.target.value }));

  async function submit(event) {
    event.preventDefault();
    setStatus({ state: 'saving' });
    try {
      await api.contact({
        senderName: values.name,
        emailAddress: values.email,
        cellphoneNumber: values.phone,
        messageSubject: values.subject,
        messageBody: values.message,
      });
      setStatus({ state: 'done' });
      setValues({ name: '', email: '', phone: '', subject: '', message: '' });
    } catch (error) {
      setStatus({ state: 'error', message: error.data?.message || error.message });
    }
  }

  return (
    <section className="section container" style={{ maxWidth: 760 }}>
      <h1>Contact us</h1>
      {site?.email && <p className="muted">Or email us directly at <a href={`mailto:${site.email}`}>{site.email}</a>.</p>}

      {status.state === 'done' && <div className="alert success">Your message has been sent. Thank you!</div>}
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
        <button className="btn" type="submit" disabled={status.state === 'saving'}>Send message</button>
      </form>
    </section>
  );
}
