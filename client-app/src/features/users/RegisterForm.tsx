import { ErrorMessage, Form, Formik } from 'formik';
import { observer } from 'mobx-react-lite';
import * as Yup from 'yup';
import MyTextInput from '../../app/common/form/MyTextInput';
import { useStore } from '../../app/stores/store';
import Button from '../../app/ui/Button';
import ValidationErrors from '../errors/ValidationErrors';

const RegisterForm = () => {
  const { userStore } = useStore();
  return (
    <Formik
      initialValues={{ dispayName: '', username: '', email: '', password: '', error: null }}
      onSubmit={(values, { setErrors }) =>
        userStore.register(values).catch((error) => {
          if (error?.response?.status !== 429) setErrors({ error });
        })
      }
      validationSchema={Yup.object({
        dispayName: Yup.string().required(),
        username: Yup.string().required(),
        email: Yup.string().required().email(),
        password: Yup.string().required(),
      })}
    >
      {({ handleSubmit, isSubmitting, errors, isValid, dirty }) => (
        <Form onSubmit={handleSubmit} autoComplete="off">
          <h2 className="mb-6 text-center text-2xl font-bold text-slate-900 dark:text-zinc-100">Sign up to Events</h2>
          <MyTextInput name="dispayName" placeholder="Display Name" autoComplete="name" autoFocus />
          <MyTextInput name="username" placeholder="Username" autoComplete="username" />
          <MyTextInput name="email" placeholder="Email" autoComplete="email" />
          <MyTextInput
            name="password"
            placeholder="Password"
            type="password"
            autoComplete="new-password"
            hint="At least 8 characters, with upper and lower case letters and a digit"
          />
          <ErrorMessage name="error" render={() => <ValidationErrors errors={errors.error} />} />
          <Button disabled={!isValid || !dirty} loading={isSubmitting} type="submit" className="w-full">
            Register
          </Button>
        </Form>
      )}
    </Formik>
  );
};

export default observer(RegisterForm);