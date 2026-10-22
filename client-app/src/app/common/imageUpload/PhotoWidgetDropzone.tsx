import { useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload } from 'lucide-react';
import clsx from 'clsx';
import type { PreviewFile } from './types';

interface Props {
  setFiles: (files: PreviewFile[]) => void;
}

export default function PhotoWidgetDropzone({ setFiles }: Props) {
  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      setFiles(
        acceptedFiles.map((file) => Object.assign(file, { preview: URL.createObjectURL(file) }))
      );
    },
    [setFiles]
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
    multiple: false,
  });

  return (
    <div
      {...getRootProps()}
      className={clsx(
        'flex h-[200px] cursor-pointer flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed text-center transition-colors',
        isDragActive ? 'border-brand-500 bg-brand-50 dark:bg-brand-500/10 text-brand-700 dark:text-brand-300' : 'border-slate-300 text-slate-500 dark:text-zinc-400 hover:border-brand-300'
      )}
    >
      <input {...getInputProps()} />
      <Upload className="size-10" />
      <p className="text-sm font-medium">Drop image here</p>
    </div>
  );
}