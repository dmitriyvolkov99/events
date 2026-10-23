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
});