import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import CmsContent from '../components/CmsContent.jsx';
import { NotFound } from './Blog.jsx';

/** Any admin-managed CMS page by slug (about-us, services, …), shortcodes included. */
export default function CmsPage() {
  const { slug } = useParams();
  const [page, setPage] = useState(undefined);

  useEffect(() => {
    setPage(undefined);
    api.page(slug).then(setPage).catch(() => setPage(null));
  }, [slug]);

  if (page === undefined) return <section className="section container"><p className="muted">Loading…</p></section>;
  if (page === null) return <NotFound />;

  return (
    <section className="section container">
      <h1>{page.title}</h1>
      <CmsContent html={page.content} />
    </section>
  );
}
