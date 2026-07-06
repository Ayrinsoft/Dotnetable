import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import CmsContent from '../components/CmsContent.jsx';
import { NotFound } from './Blog.jsx';

const PAGE_SIZE = 12;

const price = (money) =>
  money == null ? null : `${money.amount ?? money} ${money.currencyCode ?? 'USD'}`;

export function ShopList() {
  const [page, setPage] = useState(1);
  const [data, setData] = useState(null);

  useEffect(() => {
    api.products(page, PAGE_SIZE).then(setData).catch(() => setData({ items: [], totalCount: 0 }));
  }, [page]);

  const pageCount = Math.max(1, Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE));

  return (
    <section className="section container">
      <h1>Shop</h1>
      {!data ? <p className="muted">Loading…</p> : data.items.length === 0 ? (
        <p className="muted">No products yet.</p>
      ) : (
        <>
          <div className="grid cols-3">
            {data.items.map((product) => (
              <article className="card" key={product.slug}>
                {product.featuredImageUrl && (
                  <img src={product.featuredImageUrl} alt={product.title}
                       style={{ borderRadius: 'var(--radius)', marginBottom: 12 }} />
                )}
                <h3 style={{ marginTop: 0 }}>
                  <Link to={`/shop/${product.slug}`}>{product.title}</Link>
                </h3>
                {product.brandName && <p className="muted">{product.brandName}</p>}
                {price(product.minPrice) && <p><strong>{price(product.minPrice)}</strong></p>}
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

export function ProductDetail() {
  const { slug } = useParams();
  const [product, setProduct] = useState(undefined);

  useEffect(() => {
    setProduct(undefined);
    api.product(slug).then(setProduct).catch(() => setProduct(null));
  }, [slug]);

  if (product === undefined) return <section className="section container"><p className="muted">Loading…</p></section>;
  if (product === null) return <NotFound />;

  return (
    <section className="section container">
      <div style={{ display: 'grid', gap: 32, gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))' }}>
        <div>
          {product.featuredImageUrl && (
            <img src={product.featuredImageUrl} alt={product.title} style={{ borderRadius: 'var(--radius)' }} />
          )}
        </div>
        <div>
          <h1 style={{ marginTop: 0 }}>{product.title}</h1>
          {product.brandName && <p className="muted">{product.brandName}</p>}
          {price(product.minPrice) && <p style={{ fontSize: '1.4rem' }}><strong>{price(product.minPrice)}</strong></p>}
          {product.shortDescription && <p>{product.shortDescription}</p>}
          {product.ratingCount > 0 && (
            <p className="muted">★ {product.avgRating} ({product.ratingCount} reviews)</p>
          )}
        </div>
      </div>

      {product.contentSections?.map((section, i) => (
        <div key={i} className="section">
          {section.title && <h2>{section.title}</h2>}
          <CmsContent html={section.content ?? section.body} />
        </div>
      ))}

      <p><Link to="/shop">← Back to shop</Link></p>
    </section>
  );
}
