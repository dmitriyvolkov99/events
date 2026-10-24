import { cn } from './cn';
import SafeImage from './SafeImage';

interface Props {
  src?: string | null;
  alt: string;
  className?: string;
}

export default function Avatar({ src, alt, className }: Props) {
  return (
    <SafeImage
      src={src}
      fallback="/assets/user.png"
      alt={alt}
      className={cn('size-10 rounded-full object-cover', className)}
    />
  );
}