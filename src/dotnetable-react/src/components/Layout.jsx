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
    const root = document.documentElement;
    const primary = normalizeColor(site?.primaryColor);
    const secondary = normalizeColor(site?.secondaryColor);
    const background = normalizeColor(site?.backgroundColor);
    const backgroundDark = normalizeColor(site?.darkBackgroundColor);
    if (primary) root.style.setProperty('--color-primary', primary);
    else root.style.removeProperty('--color-primary');
    if (secondary) root.style.setProperty('--color-secondary', secondary);
    else root.style.removeProperty('--color-secondary');
    if (background) root.style.setProperty('--site-bg', background);
    else root.style.removeProperty('--site-bg');
    if (backgroundDark) root.style.setProperty('--site-bg-dark', backgroundDark);
    else root.style.removeProperty('--site-bg-dark');
    const image = cssImage(site?.backgroundImageUrl);
    const imageDark = cssImage(site?.darkBackgroundImageUrl);
    if (image) root.style.setProperty('--page-image-light', image);
    else root.style.removeProperty('--page-image-light');
    if (imageDark) root.style.setProperty('--page-image-dark', imageDark);
    else root.style.removeProperty('--page-image-dark');
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
            {site?.contactInfos?.length ? (
              site.contactInfos.map((info) => (
                <p key={`${info.groupTitle ?? ''}-${info.title}-${info.value}`}>
                  {info.groupTitle ? `${info.groupTitle} · ` : ''}
                  {info.title}: {contactValue(info)}
                </p>
              ))
            ) : (
              <>
                {site?.email && <p><a href={`mailto:${site.email}`}>{site.email}</a></p>}
                {site?.phone && <p><a href={`tel:${site.phone}`}>{site.phone}</a></p>}
              </>
            )}
          </div>
        </div>
        <div className="container" style={{ marginTop: 24 }}>
          <small className="muted">© {new Date().getFullYear()} {brand}. All rights reserved.</small>
        </div>
      </footer>
    </>
  );
}

function cssImage(url) {
  if (typeof url !== 'string') return null;
  if (!/^https?:\/\//i.test(url)) return null;
  return `url('${url.replace(/['"();\\]/g, '')}')`;
}

function normalizeColor(value) {
  if (typeof value !== 'string') return null;
  const text = value.trim();
  const short = /^#([0-9a-fA-F]{3})$/.exec(text);
  if (short) {
    const [r, g, b] = short[1];
    return `#${r}${r}${g}${g}${b}${b}`.toLowerCase();
  }
  return /^#[0-9a-fA-F]{6}$/.test(text) ? text.toLowerCase() : null;
}

function contactValue(info) {
  const type = Number(info.type);
  if (type === 4 && info.latitude != null && info.longitude != null) {
    const lat = Number(info.latitude);
    const lng = Number(info.longitude);
    const href = `https://www.openstreetmap.org/?mlat=${lat}&mlon=${lng}#map=17/${lat}/${lng}`;
    return <a href={href} target="_blank" rel="noreferrer">{info.value || `${lat}, ${lng}`}</a>;
  }
  if (type === 1 && info.value) return <a href={`tel:${info.value}`}>{info.value}</a>;
  if (type === 2 && info.value) return <a href={`mailto:${info.value}`}>{info.value}</a>;
  return info.value;
}
