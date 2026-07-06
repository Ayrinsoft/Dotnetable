import { useEffect, useState } from 'react';

/** Slideshow player for the admin-managed slideshows (auto-play, arrows, dots). */
export default function Slideshow({ slideshow }) {
  const slides = slideshow?.slides ?? [];
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (index >= slides.length) setIndex(0);
  }, [slides.length, index]);

  useEffect(() => {
    if (!slideshow?.autoPlay || slides.length < 2) return;
    const timer = setInterval(
      () => setIndex((i) => (i + 1) % slides.length),
      slideshow.intervalMs || 5000,
    );
    return () => clearInterval(timer);
  }, [slideshow, slides.length]);

  if (slides.length === 0) return null;
  const slide = slides[index] ?? slides[0];
  const multi = slides.length > 1;
  const goTo = (i) => setIndex((i + slides.length) % slides.length);

  const image = (
    <img src={slide.imageUrl} alt={slide.altText || slide.title || ''} />
  );

  return (
    <div className="slideshow" style={{ '--slideshow-ratio': (slideshow.aspectRatio || '21:9').replace(':', '/') }}>
      <div className="slideshow-viewport">
        {slide.linkUrl ? (
          <a href={slide.linkUrl} target={slide.openInNewTab ? '_blank' : undefined} rel="noreferrer">
            {image}
          </a>
        ) : image}

        {(slide.title || slide.caption) && (
          <div className="slideshow-caption">
            {slide.title && <h3>{slide.title}</h3>}
            {slide.caption && <p>{slide.caption}</p>}
          </div>
        )}

        {slideshow.showArrows && multi && (
          <>
            <button type="button" className="slideshow-arrow slideshow-arrow-prev"
                    aria-label="Previous slide" onClick={() => goTo(index - 1)}>
              ‹
            </button>
            <button type="button" className="slideshow-arrow slideshow-arrow-next"
                    aria-label="Next slide" onClick={() => goTo(index + 1)}>
              ›
            </button>
          </>
        )}
      </div>

      {slideshow.showDots && multi && (
        <div className="slideshow-dots">
          {slides.map((_, i) => (
            <button key={i} type="button" className={i === index ? 'active' : ''}
                    onClick={() => goTo(i)} aria-label={`Slide ${i + 1}`} />
          ))}
        </div>
      )}
    </div>
  );
}
