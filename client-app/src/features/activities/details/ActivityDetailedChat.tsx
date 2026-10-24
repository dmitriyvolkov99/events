import { Formik, Form, Field, FieldProps } from 'formik';
import { observer } from 'mobx-react-lite';
import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import * as Yup from 'yup';
import { formatDistanceToNow } from 'date-fns';
import { useStore } from '../../../app/stores/store';
import Card from '../../../app/ui/Card';
import Avatar from '../../../app/ui/Avatar';
import Spinner from '../../../app/ui/Spinner';
import { cn } from '../../../app/ui/cn';
import { fieldClass } from '../../../app/common/form/fieldClass';

interface Props {
  activityId: string;
}

export default observer(function ActivityDetailedChat({ activityId }: Props) {
  const { commentStore, userStore } = useStore();

  useEffect(() => {
    if (activityId) commentStore.createHubConnection(activityId);
    return () => commentStore.clearComments();
  }, [commentStore, activityId]);

  return (
    <Card className="overflow-hidden p-0">
      <div className="bg-brand-600 px-5 py-3 text-center text-sm font-semibold text-white">
        Chat about this event
      </div>

      <div className="p-5">
        <Formik
          onSubmit={(values, { resetForm }) => commentStore.addComment(values).then(() => resetForm())}
          initialValues={{ body: '' }}
          validationSchema={Yup.object({ body: Yup.string().required() })}
        >
          {({ isSubmitting, isValid, handleSubmit }) => (
            <Form>
              <Field name="body">
                {(props: FieldProps) => (
                  <div className="relative">
                    <textarea
                      {...props.field}
                      rows={2}
                      placeholder="Enter your comment (Enter to send, Shift+Enter for a new line)"
                      className={fieldClass(false)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' && !e.shiftKey) {
                          e.preventDefault();
                          if (isValid) handleSubmit();
                        }
                      }}
                    />
                    {isSubmitting && (
                      <div className="absolute top-2 right-2 text-brand-600">
                        <Spinner className="size-4" />
                      </div>
                    )}
                  </div>
                )}
              </Field>
            </Form>
          )}
        </Formik>

        <ul className="mt-6 max-h-[28rem] space-y-2 overflow-y-auto pr-1">
          {commentStore.comments.map((comment) => {
            const isOwn = comment.username === userStore.user?.username;
            return (
              <li
                key={comment.id}
                className={cn('flex gap-3 rounded-xl p-3', isOwn && 'bg-brand-50 dark:bg-brand-500/10')}
              >
                <Avatar src={comment.image} alt={comment.displayName} className="size-9 shrink-0" />
                <div className="min-w-0">
                  <div className="flex flex-wrap items-baseline gap-x-2">
                    <Link
                      to={`/profiles/${comment.username}`}
                      className="text-sm font-semibold text-slate-900 hover:text-brand-600 dark:text-zinc-100"
                    >
                      {comment.displayName}
                    </Link>
                    <span className="text-xs text-slate-400 dark:text-zinc-500">
                      {formatDistanceToNow(comment.createdAt, { addSuffix: true })}
                    </span>
                  </div>
                  <p className="mt-0.5 text-sm break-words whitespace-pre-wrap text-slate-700 dark:text-zinc-300">
                    {comment.body}
                  </p>
                </div>
              </li>
            );
          })}
        </ul>
      </div>
    </Card>
  );
});