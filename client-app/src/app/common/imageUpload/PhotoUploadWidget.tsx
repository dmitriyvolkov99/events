import { useEffect, useState } from 'react';
import { Check, X } from 'lucide-react';
import PhotoWidgetCropper from './PhotoWidgetCropper';
import PhotoWidgetDropzone from './PhotoWidgetDropzone';
import Button from '../../ui/Button';
import type { PreviewFile } from './types';

interface Props {
  loading: boolean;
  uploadPhoto: (file: Blob) => void;
}

const stepTitle = 'mb-3 text-xs font-semibold tracking-wider text-brand-600 uppercase';

const PhotoUploadWidget = ({ loading, uploadPhoto }: Props) => {
const [files, setFiles] = useState<PreviewFile[]>([]);
  const [cropper, setCropper] = useState<Cropper>();

  function onCrop() {
    if (cropper) {
      cropper.getCroppedCanvas().toBlob((blob) => uploadPhoto(blob!));
    }
  }

useEffect(() => {
  return () => {
    files.forEach((file) => URL.revokeObjectURL(file.preview));
  };
}, [files]);

  return (
    <div className="grid gap-6 md:grid-cols-3">
      <div>
        <h4 className={stepTitle}>Step 1 - Add Photo</h4>
        <PhotoWidgetDropzone setFiles={setFiles} />
      </div>

      <div>
        <h4 className={stepTitle}>Step 2 - Resize image</h4>
        {files && files.length > 0 && (
          <PhotoWidgetCropper setCropper={setCropper} imagePreview={files[0].preview} />
        )}
      </div>

      <div>
        <h4 className={stepTitle}>Step 3 - Preview &amp; Upload</h4>
        {files && files.length > 0 && (
          <>
            <div className="img-preview min-h-[200px] overflow-hidden rounded-lg bg-slate-100 dark:bg-zinc-800" />
            <div className="mt-3 flex gap-2">
              <Button aria-label="Upload" loading={loading} onClick={onCrop}>
                <Check className="size-4" />
              </Button>
              <Button aria-label="Discard" variant="secondary" disabled={loading} onClick={() => setFiles([])}>
                <X className="size-4" />
              </Button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default PhotoUploadWidget;