import Alert from '../../app/ui/Alert';

interface Props {
  errors: unknown;
}

export default function ValidationErrors({ errors }: Props) {
  if (!errors) return null;

  const messages = Array.isArray(errors) ? errors : [String(errors)];

  return (
    <Alert>
      <ul className="list-disc space-y-1 pl-5">
        {messages.map((err, i) => (
          <li key={i}>{String(err)}</li>
        ))}
      </ul>
    </Alert>
  );
}