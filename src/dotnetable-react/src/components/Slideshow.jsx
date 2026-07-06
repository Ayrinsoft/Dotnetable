import { useEffect, useState } from 'react';

/** Minimal slideshow player for the admin-managed slideshows (auto-play + dots). */
export default function Slideshow({ slideshow }) {
  const slides = slideshow?.slides ?? [];
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (!slideshow?.autoPlay || slides.length < 2) return;
    const timer = setInterval(
      () => setIndex((i) => (i + 1) % slides.length),
      slideshow.intervalMs || 5000,
    );
    return () => clearInterval(timer);
  }, [slideshow, slides.length]);

  if (slides.length === 0) return null;
  const slide = slides[index];

  const image = (
    <img src={slide.imageUrl} alt={slide.altText || slide.title || ''} />
  );

  return (
    <div className="slideshow" style={{ position: 'relative' }}>
      {slide.linkUrl ? (
        <a href={slide.linkUrl} target={slide.openInNewTab ? '_blank' : undefined} rel="noreferrer">
          {image}
        </a>
      ) : image}

      {(slide.title || slide.caption) && (
        <div style={{ position: 'absolute', left: 24, bottom: 24, color: '#fff', textShadow: '0 1px 6px rgba(0,0,0,.6)' }}>
          {slide.title && <h3 style={{ margin: 0 }}>{slide.title}</h3>}
          {slide.caption && <p style={{ margin: 0 }}>{slide.caption}</p>}
        </div>
      )}

      {slideshow.showDots && slides.length > 1 && (
        <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginTop: 10 }}>
          {slides.map((_, i) => (
            <button key={i} onClick={() => setIndex(i)} aria-label={`Slide ${i + 1}`}
                    style={{
                      width: 10, height: 10, borderRadius: '50%', border: 0, cursor: 'pointer',
                      background: i === index ? 'var(--color-primary)' : 'var(--color-muted)',
                    }} />
          ))}
        </div>
      )}
    </div>
  );
}
