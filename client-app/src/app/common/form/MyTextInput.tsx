import { useField } from 'formik';
import type { InputHTMLAttributes } from 'react';
import FormField from './FormField';
import { fieldClass } from './fieldClass';

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  name: string;
  label?: string;
}

const MyTextInput = ({ label, ...props }: Props) => {
  const [field, meta] = useField(props.name);
  const hasError = meta.touched && !!meta.error;
  const id = props.id ?? props.name;

  return (
    <FormField id={id} label={label} error={hasError ? meta.error : undefined}>
      <input {...field} {...props} id={id} aria-invalid={hasError} className={fieldClass(hasError)} />
    </FormField>
  );
};

export default MyTextInput;