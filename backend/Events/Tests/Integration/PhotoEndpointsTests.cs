using System.Net;
using System.Net.Http.Headers;
using Tests.Support;
using Xunit;

namespace Tests.Integration;

public class PhotoEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static MultipartFormDataContent PhotoForm(string contentType = "image/png")
    {
        // содержимое неважно: Cloudinary заменён фейком, а формат проверяется по заголовку
        var file = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47]);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "File", "photo.png" } };
    }

    private static async Task<(string Id, bool IsMain)> UploadAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/photos", PhotoForm());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        return (json.GetProperty("id").GetString()!, json.GetProperty("isMain").GetBoolean());
    }

    private static async Task<int> PhotoCountAsync(HttpClient client, string username) =>
        (await (await client.GetAsync($"/api/profiles/{username}")).ReadJsonAsync())
            .GetProperty("photos").GetArrayLength();

    [Fact]
    public async Task Uploading_requires_a_token()
    {
        var response = await factory.ClientFor().PostAsync("/api/photos", PhotoForm());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task The_first_photo_becomes_the_main_one_and_the_second_does_not()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var first = await UploadAsync(client);
        var second = await UploadAsync(client);

        Assert.True(first.IsMain);
        Assert.False(second.IsMain);
    }

    [Fact]
    public async Task A_file_that_is_not_an_image_is_rejected()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.PostAsync("/api/photos", PhotoForm("text/plain"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_user_cannot_delete_someone_elses_photo()
    {
        var owner = await factory.CreateUserAsync();
        var ownerClient = factory.ClientFor(owner);
        await UploadAsync(ownerClient);
        var (photoId, _) = await UploadAsync(ownerClient);
        var intruder = factory.ClientFor(await factory.CreateUserAsync());

        var response = await intruder.DeleteAsync($"/api/photos/{photoId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(photoId, factory.Photos.Deleted);
        Assert.Equal(2, await PhotoCountAsync(ownerClient, owner.Username));
    }

    [Fact]
    public async Task The_owner_can_delete_a_photo_that_is_not_the_main_one()
    {
        var owner = await factory.CreateUserAsync();
        var client = factory.ClientFor(owner);
        await UploadAsync(client);
        var (photoId, _) = await UploadAsync(client);

        var response = await client.DeleteAsync($"/api/photos/{photoId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(photoId, factory.Photos.Deleted);
        Assert.Equal(1, await PhotoCountAsync(client, owner.Username));
    }
}