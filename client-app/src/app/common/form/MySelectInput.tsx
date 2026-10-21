import { useField } from 'formik';
import { ChevronDown } from 'lucide-react';
import FormField from './FormField';
import { fieldClass } from './fieldClass';

interface Props {
  placeholder: string;
  name: string;
  options: { text: string; value: string }[];
  label?: string;
}

const MySelectInput = ({ label, placeholder, options, name }: Props) => {
  const [field, meta] = useField(name);
  const hasError = meta.touched && !!meta.error;

  return (
    <FormField id={name} label={label} error={hasError ? meta.error : undefined}>
      <div className="relative">
        <select
          {...field}
          id={name}
          value={field.value ?? ''}
          aria-invalid={hasError}
          className={`${fieldClass(hasError)} appearance-none pr-9 ${field.value ? '' : 'text-slate-400'}`}
        >
          <option value="">{placeholder}</option>
          {options.map((o) => (
            <option key={o.value} value={o.value} className="text-slate-900">
              {o.text}
            </option>
          ))}
        </select>
        <ChevronDown className="pointer-events-none absolute top-1/2 right-3 size-4 -translate-y-1/2 text-slate-400" />
      </div>
    </FormField>
  );
};

export default MySelectInput;