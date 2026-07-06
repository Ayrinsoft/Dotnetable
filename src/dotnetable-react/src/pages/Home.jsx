import { useEffect, useState } from 'react';
import { Link, useOutletContext } from 'react-router-dom';
import { api } from '../api/client.js';
import Slideshow from '../components/Slideshow.jsx';
import CmsContent from '../components/CmsContent.jsx';

/**
 * Mirrors Dotnetable.Web's homepage: slideshow assigned to the "home_top" placement, the CMS page
 * with slug "home" as the admin-managed body (with a generic hero as fallback), latest blog posts.
 */
export default function Home() {
  const { site } = useOutletContext();
  const [slideshow, setSlideshow] = useState(null);
  const [homePage, setHomePage] = useState(undefined); // undefined = loading, null = not configured
  const [posts, setPosts] = useState([]);

  useEffect(() => {
    api.slideshowByPlacement('home_top').then(setSlideshow);
    api.page('home').then(setHomePage).catch(() => setHomePage(null));
    api.posts(1, 3).then((page) => setPosts(page?.items ?? [])).catch(() => {});
  }, []);

  return (
    <>
      {slideshow && (
        <section className="section container">
          <Slideshow slideshow={slideshow} />
        </section>
      )}

      {homePage ? (
        <section className="section container">
          <CmsContent html={homePage.content} />
        </section>
      ) : homePage === null && (
        <section className="hero">
          <div className="container">
            <h1>{site?.brandName || 'Welcome'}</h1>
            <p>{site?.defaultMetaDescription || 'Create a CMS page with slug "home" in the admin to replace this hero.'}</p>
            <Link className="btn" to="/contact" style={{ background: '#fff', color: 'var(--color-primary)' }}>
              Get in touch
            </Link>
          </div>
        </section>
      )}

      {posts.length > 0 && (
        <section className="section container">
          <h2>Latest articles</h2>
          <div className="grid cols-3">
            {posts.map((post) => (
              <article className="card" key={post.slug}>
                {post.featuredImageUrl && (
                  <img src={post.featuredImageUrl} alt={post.title}
                       style={{ borderRadius: 'var(--radius)', marginBottom: 12 }} />
                )}
                <h3 style={{ marginTop: 0 }}>
                  <Link to={`/blog/${post.slug}`}>{post.title}</Link>
                </h3>
                <p className="muted">{post.excerpt}</p>
              </article>
            ))}
          </div>
          <p><Link to="/blog">View all posts →</Link></p>
        </section>
      )}
    </>
  );
}
