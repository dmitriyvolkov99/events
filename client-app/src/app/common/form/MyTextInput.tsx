import { useState, type InputHTMLAttributes } from 'react';
import { useField } from 'formik';
import { Eye, EyeOff } from 'lucide-react';
import FormField from './FormField';
import { fieldClass } from './fieldClass';
import { cn } from '../../ui/cn';

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  name: string;
  label?: string;
  hint?: string;
}

const MyTextInput = ({ label, hint, type, ...props }: Props) => {
  const [field, meta] = useField(props.name);
  const [visible, setVisible] = useState(false);
  const hasError = meta.touched && !!meta.error;
  const id = props.id ?? props.name;
  const isPassword = type === 'password';

  return (
    <FormField id={id} label={label} hint={hint} error={hasError ? meta.error : undefined}>
      <div className="relative">
        <input
          {...field}
          {...props}
          id={id}
          type={isPassword && visible ? 'text' : type}
          aria-invalid={hasError}
          aria-describedby={hasError ? `${id}-error` : hint ? `${id}-hint` : undefined}
          className={cn(fieldClass(hasError), isPassword && 'pr-10')}
        />
        {isPassword && (
          <button
            type="button"
            onClick={() => setVisible((v) => !v)}
            aria-label={visible ? 'Hide password' : 'Show password'}
            aria-pressed={visible}
            className="absolute inset-y-0 right-0 flex items-center rounded-r-lg px-3 text-slate-400 hover:text-slate-600 dark:text-zinc-500 dark:hover:text-zinc-300"
          >
            {visible ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
          </button>
        )}
      </div>
    </FormField>
  );
};

export default MyTextInput;