import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { describe, expect, it } from 'vitest';
import MyTextInput from './MyTextInput';

describe('MyTextInput', () => {
  it('shows a validation error after the field loses focus', async () => {
    render(
      <Formik
        initialValues={{ title: '' }}
        validationSchema={Yup.object({ title: Yup.string().required('Title is required') })}
        onSubmit={() => {}}
      >
        <Form>
          <MyTextInput name="title" label="Title" placeholder="Title" />
        </Form>
      </Formik>
    );

    await userEvent.click(screen.getByLabelText('Title'));
    await userEvent.tab();

    expect(await screen.findByText('Title is required')).toBeInTheDocument();
  });

  it('toggles password visibility', async () => {
    render(
      <Formik initialValues={{ password: '' }} onSubmit={() => {}}>
        <Form>
          <MyTextInput name="password" label="Password" type="password" />
        </Form>
      </Formik>
    );
    const input = screen.getByLabelText('Password');
    expect(input).toHaveAttribute('type', 'password');

    await userEvent.click(screen.getByRole('button', { name: 'Show password' }));
    expect(input).toHaveAttribute('type', 'text');

    await userEvent.click(screen.getByRole('button', { name: 'Hide password' }));
    expect(input).toHaveAttribute('type', 'password');
  });
});