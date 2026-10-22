import { observer } from 'mobx-react-lite';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { Form, Formik } from 'formik';
import * as Yup from 'yup';
import { v4 as uuid } from 'uuid';
import LoadingComponent from '../../../app/layout/LoadingComponent';
import { ActivityFormValues } from '../../../app/models/activity';
import { useStore } from '../../../app/stores/store';
import MyTextInput from '../../../app/common/form/MyTextInput';
import MyTextArea from '../../../app/common/form/MyTextArea';
import MySelectInput from '../../../app/common/form/MySelectInput';
import MyDateInput from '../../../app/common/form/MyDateInput';
import { categoryOptions } from '../../../app/common/options/category.Options';
import Card from '../../../app/ui/Card';
import Button from '../../../app/ui/Button';
import { toast } from 'react-toastify';

const validationSchema = Yup.object({
  title: Yup.string().required('The activity title is required'),
  description: Yup.string().required('The activity description is required'),
  category: Yup.string().required('Category is required'),
  date: Yup.date().required('Date is required').nullable(),
  venue: Yup.string().required('Venue is required'),
  city: Yup.string().required('City is required'),
});

const ActivityForm = () => {
  const { activityStore } = useStore();
  const { createActivity, updateActivity, loadActivity, loadingInitial } = activityStore;

  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [activity, setActivity] = useState<ActivityFormValues>(new ActivityFormValues());

  useEffect(() => {
    if (id) loadActivity(id).then((activity) => setActivity(new ActivityFormValues(activity)));
  }, [id, loadActivity]);

async function handleFormSubmit(values: ActivityFormValues) {
  try {
    if (values.id) {
      await updateActivity(values);
      navigate(`/activities/${values.id}`);
    } else {
      const id = uuid();
      await createActivity({ ...values, id });
      navigate(`/activities/${id}`);
    }
  } catch (error) {
    if (Array.isArray(error)) toast.error(error.join(', '));
  }
}

  if (loadingInitial) return <LoadingComponent content="Loading event..." />;

  return (
    <Card className="mx-auto max-w-2xl">
      <title>{id ? 'Edit event - Events' : 'Create event - Events'}</title>
      <h2 className="mb-6 text-xl font-semibold text-slate-900 dark:text-zinc-100">{id ? 'Edit event' : 'Create event'}</h2>

      <Formik
        validationSchema={validationSchema}
        enableReinitialize
        initialValues={activity}
        onSubmit={handleFormSubmit}
      >
        {({ handleSubmit, isValid, isSubmitting, dirty }) => (
          <Form onSubmit={handleSubmit} autoComplete="off">
            <h3 className="mb-3 text-xs font-semibold tracking-wider text-brand-600 uppercase">Event details</h3>
            <MyTextInput name="title" placeholder="Title" />
            <MyTextArea rows={3} placeholder="Description" name="description" />
            <MySelectInput options={categoryOptions} placeholder="Category" name="category" />
            <MyDateInput
              name="date"
              placeholderText="Date"
              showTimeSelect
              dateFormat="MMMM d, yyyy, h:mm aa"
            />

            <h3 className="mt-6 mb-3 text-xs font-semibold tracking-wider text-brand-600 uppercase">Location details</h3>
            <MyTextInput placeholder="City" name="city" />
            <MyTextInput placeholder="Venue" name="venue" />

            <div className="mt-6 flex justify-end gap-3">
              <Link
                to="/activities"
                className="inline-flex items-center rounded-lg  bg-white dark:bg-zinc-900 px-4 py-2 text-sm font-medium text-slate-700 dark:text-zinc-300 ring-1 ring-slate-300 dark:ring-zinc-700 transition-colors hover:bg-slate-50 dark:hover:bg-zinc-800"
              >
                Cancel
              </Link>
              <Button type="submit" disabled={!dirty || !isValid} loading={isSubmitting}>
                Submit
              </Button>
            </div>
          </Form>
        )}
      </Formik>
    </Card>
  );
};

export default observer(ActivityForm);