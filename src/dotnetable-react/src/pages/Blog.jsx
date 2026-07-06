import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import CmsContent from '../components/CmsContent.jsx';

const PAGE_SIZE = 12;

export function BlogList() {
  const [page, setPage] = useState(1);
  const [data, setData] = useState(null);

  useEffect(() => {
    api.posts(page, PAGE_SIZE).then(setData).catch(() => setData({ items: [], totalCount: 0 }));
  }, [page]);

  const pageCount = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));

  return (
    <section className="section container">
      <h1>Blog</h1>
      {!data ? <p className="muted">Loading…</p> : data.items.length === 0 ? (
        <p className="muted">No posts yet.</p>
      ) : (
        <>
          <div className="grid cols-3">
            {data.items.map((post) => (
              <article className="card" key={post.slug}>
                {post.featuredImageUrl && (
                  <img src={post.featuredImageUrl} alt={post.title}
                       style={{ borderRadius: 'var(--radius)', marginBottom: 12 }} />
                )}
                <h3 style={{ marginTop: 0 }}><Link to={`/blog/${post.slug}`}>{post.title}</Link></h3>
                <p className="muted">{post.excerpt}</p>
              </article>
            ))}
          </div>
          {pageCount > 1 && (
            <p style={{ display: 'flex', gap: 8, marginTop: 24 }}>
              {Array.from({ length: pageCount }, (_, i) => i + 1).map((n) => (
                <button key={n} className={n === page ? 'btn' : 'btn secondary'} onClick={() => setPage(n)}>
                  {n}
                </button>
              ))}
            </p>
          )}
        </>
      )}
    </section>
  );
}

export function BlogPost() {
  const { slug } = useParams();
  const [post, setPost] = useState(undefined);

  useEffect(() => {
    setPost(undefined);
    api.post(slug).then(setPost).catch(() => setPost(null));
  }, [slug]);

  if (post === undefined) return <section className="section container"><p className="muted">Loading…</p></section>;
  if (post === null) return <NotFound />;

  return (
    <section className="section container">
      <h1>{post.title}</h1>
      {post.publishedAt && <p className="muted">{new Date(post.publishedAt).toLocaleDateString()}</p>}
      {post.featuredImageUrl && <img src={post.featuredImageUrl} alt={post.title} style={{ borderRadius: 'var(--radius)' }} />}
      <CmsContent html={post.content} />
      <p><Link to="/blog">← All posts</Link></p>
    </section>
  );
}

export function NotFound() {
  return (
    <section className="section container">
      <h1>Page not found</h1>
      <p className="muted">The content you are looking for doesn't exist or is no longer available.</p>
      <Link className="btn" to="/">Back to home</Link>
    </section>
  );
}
