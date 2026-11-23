using Application.Interfaces;
using Application.Photos;
using Microsoft.AspNetCore.Http;

namespace Tests.Support;

public sealed class FakeUserAccessor(string username) : IUserAccessor
{
    public string GetUsername() => username;
}

public sealed class FakePhotoAccessor : IPhotoAccessor
{
    public List<string> Deleted { get; } = [];
    public bool DeleteSucceeds { get; set; } = true;

    public Task<PhotoUploadResult?> AddPhoto(IFormFile file) =>
        Task.FromResult<PhotoUploadResult?>(new PhotoUploadResult
        {
            PublicId = Guid.NewGuid().ToString("N"),
            Url = "https://example.com/photo.jpg"
        });

    public Task<string?> DeletePhoto(string publicId)
    {
        Deleted.Add(publicId);
        return Task.FromResult<string?>(DeleteSucceeds ? "ok" : null);
    }
}