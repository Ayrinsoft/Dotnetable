import { useState } from 'react';
import { api } from '../api/client.js';

// Field type ids mirror Dotnetable.Domain.Enums.FormFieldType.
const FieldType = {
  Text: 0, TextArea: 1, Number: 2, Email: 3, Phone: 4, Date: 5,
  Select: 6, Radio: 7, Checkbox: 8, Rating: 9, YesNo: 10, SectionTitle: 11,
};

/**
 * Renders an admin-built dynamic form/survey (api/forms DTO) and submits answers back to the API.
 * After a successful survey submission it shows the live aggregate results when the admin enabled
 * public results — the same behaviour as the MVC site's /form/{slug} page.
 */
export default function DynamicForm({ form }) {
  const [values, setValues] = useState({});
  const [status, setStatus] = useState({ state: 'idle' });
  const [results, setResults] = useState(null);

  if (!form) return null;

  if (!form.isOpen) {
    return <div className="alert error">This form is not accepting responses right now.</div>;
  }

  const setValue = (fieldId, value) => setValues((prev) => ({ ...prev, [fieldId]: value }));

  const toggleCheckbox = (fieldId, option) =>
    setValues((prev) => {
      const current = new Set(prev[fieldId] ?? []);
      current.has(option) ? current.delete(option) : current.add(option);
      return { ...prev, [fieldId]: [...current] };
    });

  async function submit(event) {
    event.preventDefault();
    setStatus({ state: 'saving' });

    const answers = Object.entries(values)
      .map(([fieldId, value]) => ({
        formFieldID: Number(fieldId),
        values: Array.isArray(value) ? value : [String(value)],
      }))
      .filter((answer) => answer.values.some((v) => v !== ''));

    try {
      const result = await api.submitForm(form.formID, answers);
      setStatus({
        state: 'done',
        message: result?.message || form.successMessage || 'Thank you! Your response has been recorded.',
      });
      if (form.showResults) setResults(await api.formResults(form.formID));
    } catch (error) {
      setStatus({ state: 'error', message: error.data?.message || error.message });
    }
  }

  if (status.state === 'done') {
    return (
      <div>
        <div className="alert success">{status.message}</div>
        {results && <FormResults results={results} />}
        {form.allowMultipleSubmissions && (
          <button className="btn secondary" onClick={() => { setStatus({ state: 'idle' }); setValues({}); setResults(null); }}>
            Submit another response
          </button>
        )}
      </div>
    );
  }

  return (
    <form className="card" onSubmit={submit}>
      <h2 style={{ marginTop: 0 }}>{form.title}</h2>
      {form.description && <p className="muted">{form.description}</p>}
      {status.state === 'error' && <div className="alert error">{status.message}</div>}

      {form.fields.map((field) => (
        <FormField key={field.formFieldID} field={field}
                   value={values[field.formFieldID]}
                   onChange={setValue} onToggle={toggleCheckbox} />
      ))}

      <button className="btn" type="submit" disabled={status.state === 'saving'}>
        {form.submitButtonText || 'Submit'}
      </button>
    </form>
  );
}

function FormField({ field, value, onChange, onToggle }) {
  const id = `field_${field.formFieldID}`;
  const required = field.isRequired;

  if (field.fieldType === FieldType.SectionTitle) {
    return (
      <div>
        <h3>{field.label}</h3>
        {field.helpText && <p className="muted">{field.helpText}</p>}
      </div>
    );
  }

  const label = (
    <label htmlFor={id}>
      {field.label} {required && <span style={{ color: '#ef4444' }}>*</span>}
    </label>
  );
  const help = field.helpText ? <div className="help">{field.helpText}</div> : null;

  switch (field.fieldType) {
    case FieldType.TextArea:
      return (
        <div className="field">{label}
          <textarea id={id} rows={4} required={required} placeholder={field.placeholder ?? ''}
                    maxLength={field.maxValue ?? undefined}
                    value={value ?? ''} onChange={(e) => onChange(field.formFieldID, e.target.value)} />
          {help}
        </div>
      );

    case FieldType.Select:
      return (
        <div className="field">{label}
          <select id={id} required={required} value={value ?? ''}
                  onChange={(e) => onChange(field.formFieldID, e.target.value)}>
            <option value="">—</option>
            {field.options.map((option) => (
              <option key={option.formFieldOptionID} value={option.value}>{option.label}</option>
            ))}
          </select>
          {help}
        </div>
      );

    case FieldType.Radio:
      return (
        <div className="field">{label}
          {field.options.map((option) => (
            <div className="check-row" key={option.formFieldOptionID}>
              <input type="radio" id={`${id}_${option.formFieldOptionID}`} name={id} required={required}
                     checked={value === option.value}
                     onChange={() => onChange(field.formFieldID, option.value)} />
              <label htmlFor={`${id}_${option.formFieldOptionID}`}>{option.label}</label>
            </div>
          ))}
          {help}
        </div>
      );

    case FieldType.Checkbox:
      return (
        <div className="field">{label}
          {field.options.map((option) => (
            <div className="check-row" key={option.formFieldOptionID}>
              <input type="checkbox" id={`${id}_${option.formFieldOptionID}`}
                     checked={(value ?? []).includes(option.value)}
                     onChange={() => onToggle(field.formFieldID, option.value)} />
              <label htmlFor={`${id}_${option.formFieldOptionID}`}>{option.label}</label>
            </div>
          ))}
          {help}
        </div>
      );

    case FieldType.Rating: {
      const min = field.minValue ?? 1;
      const max = field.maxValue ?? 5;
      const scores = Array.from({ length: max - min + 1 }, (_, i) => min + i);
      return (
        <div className="field">{label}
          <div className="rating-row">
            {scores.map((score) => (
              <span className="check-row" key={score}>
                <input type="radio" id={`${id}_${score}`} name={id} required={required}
                       checked={value === String(score)}
                       onChange={() => onChange(field.formFieldID, String(score))} />
                <label htmlFor={`${id}_${score}`}>{score}</label>
              </span>
            ))}
          </div>
          {help}
        </div>
      );
    }

    case FieldType.YesNo:
      return (
        <div className="field">{label}
          <div className="rating-row">
            {['yes', 'no'].map((option) => (
              <span className="check-row" key={option}>
                <input type="radio" id={`${id}_${option}`} name={id} required={required}
                       checked={value === option}
                       onChange={() => onChange(field.formFieldID, option)} />
                <label htmlFor={`${id}_${option}`}>{option === 'yes' ? 'Yes' : 'No'}</label>
              </span>
            ))}
          </div>
          {help}
        </div>
      );

    default: {
      const inputType =
        field.fieldType === FieldType.Number ? 'number' :
        field.fieldType === FieldType.Email ? 'email' :
        field.fieldType === FieldType.Phone ? 'tel' :
        field.fieldType === FieldType.Date ? 'date' : 'text';
      return (
        <div className="field">{label}
          <input type={inputType} id={id} required={required} placeholder={field.placeholder ?? ''}
                 min={field.fieldType === FieldType.Number ? field.minValue ?? undefined : undefined}
                 max={field.fieldType === FieldType.Number ? field.maxValue ?? undefined : undefined}
                 maxLength={field.fieldType === FieldType.Text ? field.maxValue ?? undefined : undefined}
                 value={value ?? ''} onChange={(e) => onChange(field.formFieldID, e.target.value)} />
          {help}
        </div>
      );
    }
  }
}

/** Aggregate survey results (only served when the admin enabled public results). */
export function FormResults({ results }) {
  return (
    <div className="card">
      <h3 style={{ marginTop: 0 }}>{results.title}</h3>
      <p className="muted">{results.responseCount} responses</p>
      {results.fields.map((field) => (
        <div key={field.formFieldID}>
          <h4>
            {field.label}
            {field.average != null && <span className="muted"> (avg {field.average})</span>}
          </h4>
          {field.optionCounts.map((option) => (
            <div key={option.label}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span>{option.label}</span>
                <span className="muted">{option.count} ({option.percent}%)</span>
              </div>
              <div className="progress"><div style={{ width: `${option.percent}%` }} /></div>
            </div>
          ))}
        </div>
      ))}
    </div>
  );
}
