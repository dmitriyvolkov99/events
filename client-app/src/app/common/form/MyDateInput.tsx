import { useField } from 'formik';
import DatePicker from 'react-datepicker';
import FormField from './FormField';
import { fieldClass } from './fieldClass';

interface Props {
  name: string;
  label?: string;
  placeholderText?: string;
  showTimeSelect?: boolean;
  dateFormat?: string;
}

const MyDateInput = ({ label, ...props }: Props) => {
  const [field, meta, helpers] = useField(props.name);
  const hasError = meta.touched && !!meta.error;

  return (
    <FormField id={props.name} label={label} error={hasError ? meta.error : undefined}>
      <DatePicker
        {...props}
        id={props.name}
        autoComplete="off"
        wrapperClassName="block w-full"
        className={fieldClass(hasError)}
        selected={field.value ? new Date(field.value) : null}
        onChange={(date: Date | null) => helpers.setValue(date)}
        onBlur={() => helpers.setTouched(true)}
      />
    </FormField>
  );
};

export default MyDateInput;