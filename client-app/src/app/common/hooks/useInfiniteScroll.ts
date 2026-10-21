import { useEffect, useEffectEvent, useState } from 'react';

export function useInfiniteScroll(onLoadMore: () => void, enabled: boolean) {
  const [element, setElement] = useState<HTMLDivElement | null>(null);
  const loadMore = useEffectEvent(onLoadMore);

  useEffect(() => {
    if (!element || !enabled) return;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) loadMore();
      },
      { rootMargin: '200px' }
    );
    observer.observe(element);
    return () => observer.disconnect();
  }, [element, enabled]);

  return setElement;
}