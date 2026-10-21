import clsx from 'clsx';

interface Props {
  src?: string | null;
  alt: string;
  className?: string;
}

export default function Avatar({ src, alt, className }: Props) {
  return (
    <img
      src={src || '/assets/placeholder.png'}
      alt={alt}
      className={clsx('size-10 rounded-full object-cover ', className)}
    />
  );
}