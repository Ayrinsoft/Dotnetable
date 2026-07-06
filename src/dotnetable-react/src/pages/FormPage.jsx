import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import DynamicForm from '../components/DynamicForm.jsx';
import { NotFound } from './Blog.jsx';

/** Standalone page for an admin-built dynamic form/survey — mirrors the MVC site's /form/{slug}. */
export default function FormPage() {
  const { slug } = useParams();
  const [form, setForm] = useState(undefined);

  useEffect(() => {
    setForm(undefined);
    api.formBySlug(slug).then(setForm).catch(() => setForm(null));
  }, [slug]);

  if (form === undefined) return <section className="section container"><p className="muted">Loading…</p></section>;
  if (form === null) return <NotFound />;

  return (
    <section className="section container" style={{ maxWidth: 760 }}>
      <DynamicForm form={form} />
    </section>
  );
}
