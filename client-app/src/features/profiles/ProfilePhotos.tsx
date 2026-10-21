import { observer } from 'mobx-react-lite';
import { SyntheticEvent, useState } from 'react';
import { Image as ImageIcon, Trash2 } from 'lucide-react';
import PhotoUploadWidget from '../../app/common/imageUpload/PhotoUploadWidget';
import { Photo, Profile } from '../../app/models/profile';
import { useStore } from '../../app/stores/store';
import Panel from '../../app/ui/Panel';
import Button from '../../app/ui/Button';
import Badge from '../../app/ui/Badge';

interface Props {
  profile: Profile;
}

const ProfilePhotos = ({ profile }: Props) => {
  const { profileStore: { isCurrentUser, uploadPhoto, uploading, loading, setMainPhoto, deletePhoto } } = useStore();
  const [addPhotoMode, setAddPhotoMode] = useState(false);
  const [target, setTarget] = useState('');

  function handlePhotoUpload(file: Blob) {
    uploadPhoto(file).then(() => setAddPhotoMode(false));
  }

  function handleSetMainPhoto(photo: Photo, e: SyntheticEvent<HTMLButtonElement>) {
    setTarget(e.currentTarget.name);
    setMainPhoto(photo);
  }

  function handleDeletePhoto(photo: Photo, e: SyntheticEvent<HTMLButtonElement>) {
    setTarget(e.currentTarget.name);
    deletePhoto(photo);
  }

  return (
    <Panel>
      <div className="mb-5 flex items-center justify-between">
        <h3 className="flex items-center gap-2 text-lg font-semibold text-slate-900">
          <ImageIcon className="size-5 text-brand-600" /> Photos
        </h3>
        {isCurrentUser && (
          <Button variant="secondary" onClick={() => setAddPhotoMode(!addPhotoMode)}>
            {addPhotoMode ? 'Cancel' : 'Add Photo'}
          </Button>
        )}
      </div>

      {addPhotoMode ? (
        <PhotoUploadWidget uploadPhoto={handlePhotoUpload} loading={uploading} />
      ) : profile.photos?.length ? (
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          {profile.photos.map((photo) => (
            <li key={photo.id} className="overflow-hidden rounded-xl ring-1 ring-slate-200">
              <div className="relative">
                <img src={photo.url} alt="" className="aspect-square w-full object-cover" />
                {photo.isMain && (
                  <div className="absolute top-2 left-2">
                    <Badge tone="green">Main</Badge>
                  </div>
                )}
              </div>
              {isCurrentUser && (
                <div className="flex gap-2 p-2">
                  <Button
                    variant="secondary"
                    className="flex-1 px-2 py-1.5"
                    name={'main' + photo.id}
                    disabled={photo.isMain}
                    loading={target === 'main' + photo.id && loading}
                    onClick={(e) => handleSetMainPhoto(photo, e)}
                  >
                    Main
                  </Button>
                  <Button
                    variant="danger"
                    className="px-2.5 py-1.5"
                    aria-label="Delete photo"
                    name={photo.id}
                    disabled={photo.isMain}
                    loading={target === photo.id && loading}
                    onClick={(e) => handleDeletePhoto(photo, e)}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </div>
              )}
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-sm text-slate-500">No photos yet.</p>
      )}
    </Panel>
  );
};

export default observer(ProfilePhotos);