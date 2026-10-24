import { useState, type ImgHTMLAttributes } from 'react';

interface Props extends Omit<ImgHTMLAttributes<HTMLImageElement>, 'src'> {
  src?: string | null;
  fallback: string;
}

export default function SafeImage({ src, fallback, alt, ...rest }: Props) {
  const [failedSrc, setFailedSrc] = useState<string | null>(null);
  const current = src && src !== failedSrc ? src : fallback;

  return (
    <img
      loading="lazy"
      decoding="async"
      {...rest}
      src={current}
      alt={alt}
      onError={() => {
        if (current !== fallback) setFailedSrc(current);
      }}
    />
  );
}