import { useEffect, useState } from 'react';
import { Link, NavLink, Outlet } from 'react-router-dom';
import { api } from '../api/client.js';

/**
 * Site chrome mirroring Dotnetable.Web's _Layout: brand/logo/contact/socials come from
 * api/siteinfo, header & footer navigation from the admin-managed menus, with static fallbacks so
 * the SPA has navigation before any menu is configured.
 */
export default function Layout() {
  const [site, setSite] = useState(null);
  const [headerMenu, setHeaderMenu] = useState(null);
  const [footerMenu, setFooterMenu] = useState(null);

  useEffect(() => {
    api.siteInfo().then(setSite);
    api.menu('Header').then(setHeaderMenu);
    api.menu('Footer').then(setFooterMenu);
  }, []);

  useEffect(() => {
    if (site?.brandName) document.title = site.defaultMetaTitle || site.brandName;
  }, [site]);

  const brand = site?.brandName || 'Website';

  return (
    <>
      <header className="site-header">
        <div className="container inner">
          <Link to="/" className="brand">
            {site?.logoUrl && <img src={site.logoUrl} alt={brand} />}
            <span>{brand}</span>
          </Link>
          <nav className="nav">
            {headerMenu?.items?.length ? (
              headerMenu.items.map((item) => (
                <a key={item.menuItemID ?? item.url} href={item.url}
                   target={item.openInNewTab ? '_blank' : undefined} rel="noreferrer">
                  {item.title}
                </a>
              ))
            ) : (
              <>
                <NavLink to="/" end>Home</NavLink>
                <NavLink to="/shop">Shop</NavLink>
                <NavLink to="/blog">Blog</NavLink>
                <NavLink to="/page/about-us">About</NavLink>
                <NavLink to="/contact">Contact</NavLink>
              </>
            )}
          </nav>
        </div>
      </header>

      <main>
        <Outlet context={{ site }} />
      </main>

      <footer className="site-footer">
        <div className="container cols">
          <div>
            <h3>{brand}</h3>
            {site?.defaultMetaDescription && <p className="muted">{site.defaultMetaDescription}</p>}
            {site?.socialLinks?.length > 0 && (
              <p>
                {site.socialLinks.map((social) => (
                  <a key={social.url} href={social.url} target="_blank" rel="noreferrer"
                     style={{ marginRight: 12 }}>
                    {social.name || social.url}
                  </a>
                ))}
              </p>
            )}
          </div>
          <div>
            <h4>Links</h4>
            <ul>
              {footerMenu?.items?.length ? (
                footerMenu.items.map((item) => (
                  <li key={item.menuItemID ?? item.url}>
                    <a href={item.url} target={item.openInNewTab ? '_blank' : undefined} rel="noreferrer">
                      {item.title}
                    </a>
                  </li>
                ))
              ) : (
                <>
                  <li><Link to="/page/about-us">About</Link></li>
                  <li><Link to="/blog">Blog</Link></li>
                  <li><Link to="/contact">Contact</Link></li>
                </>
              )}
            </ul>
          </div>
          <div>
            <h4>Get in touch</h4>
            {site?.email && <p><a href={`mailto:${site.email}`}>{site.email}</a></p>}
            {site?.phone && <p><a href={`tel:${site.phone}`}>{site.phone}</a></p>}
          </div>
        </div>
        <div className="container" style={{ marginTop: 24 }}>
          <small className="muted">© {new Date().getFullYear()} {brand}. All rights reserved.</small>
        </div>
      </footer>
    </>
  );
}
