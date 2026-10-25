import { ErrorMessage, Form, Formik } from 'formik';
import { observer } from 'mobx-react-lite';
import MyTextInput from '../../app/common/form/MyTextInput';
import { useStore } from '../../app/stores/store';
import Button from '../../app/ui/Button';
import Alert from '../../app/ui/Alert';

const LoginForm = () => {
  const { userStore } = useStore();
  return (
    <Formik
      initialValues={{ email: '', password: '', error: null }}
      onSubmit={(values, { setErrors }) =>
        userStore.login(values).catch((error) => {
          if (error?.response?.status !== 429) setErrors({ error: 'Invalid email or password' });
        })
      }
    >
      {({ handleSubmit, isSubmitting, errors }) => (
        <Form onSubmit={handleSubmit} autoComplete="off">
          <h2 className="mb-6 text-center text-2xl font-bold text-slate-900 dark:text-zinc-100">Log in to Events</h2>
          <MyTextInput name="email" placeholder="Email" autoComplete="email" autoFocus />
          <MyTextInput name="password" placeholder="Password" type="password" autoComplete="current-password" />
          <ErrorMessage name="error" render={() => <Alert>{errors.error}</Alert>} />
          <Button loading={isSubmitting} type="submit" className="w-full">
            Log in
          </Button>
        </Form>
      )}
    </Formik>
  );
};

export default observer(LoginForm);