import { useEffect, useState } from 'react';
import { api } from '../api/client.js';
import DynamicForm from './DynamicForm.jsx';
import Slideshow from './Slideshow.jsx';

const SHORTCODE = /\[(slideshow|form|survey):(\d+)\]/g;

/**
 * Renders CMS HTML (pages/posts) and expands the same shortcodes the MVC site supports —
 * [slideshow:ID], [form:ID], [survey:ID] — into live React widgets.
 */
export default function CmsContent({ html }) {
  if (!html) return null;

  const parts = [];
  let last = 0;
  for (const match of html.matchAll(SHORTCODE)) {
    if (match.index > last) parts.push({ html: html.slice(last, match.index) });
    parts.push({ kind: match[1], id: Number(match[2]) });
    last = match.index + match[0].length;
  }
  if (last < html.length) parts.push({ html: html.slice(last) });

  return (
    <div className="cms-content">
      {parts.map((part, i) =>
        part.html !== undefined
          ? <div key={i} dangerouslySetInnerHTML={{ __html: part.html }} />
          : part.kind === 'slideshow'
            ? <EmbeddedSlideshow key={i} id={part.id} />
            : <EmbeddedForm key={i} id={part.id} />,
      )}
    </div>
  );
}

function EmbeddedForm({ id }) {
  const [form, setForm] = useState(null);
  useEffect(() => { api.get(`api/forms/id/${id}`).then(setForm).catch(() => {}); }, [id]);
  return form ? <DynamicForm form={form} /> : null;
}

function EmbeddedSlideshow({ id }) {
  const [slideshow, setSlideshow] = useState(null);
  useEffect(() => { api.get(`api/slideshow/${id}`).then(setSlideshow).catch(() => {}); }, [id]);
  return slideshow ? <Slideshow slideshow={slideshow} /> : null;
}
