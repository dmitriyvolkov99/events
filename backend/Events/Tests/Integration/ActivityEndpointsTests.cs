using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tests.Support;
using Xunit;

namespace Tests.Integration;

public class ActivityEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static async Task<int> AttendeesAsync(HttpClient client, Guid id) =>
        (await (await client.GetAsync($"/api/activities/{id}")).ReadJsonAsync())
            .GetProperty("attendees").GetArrayLength();

    [Fact]
    public async Task A_created_activity_can_be_read_back_with_its_host()
    {
        var host = await factory.CreateUserAsync();
        var client = factory.ClientFor(host);
        var id = await client.CreateActivityAsync("Picnic");

        var response = await client.GetAsync($"/api/activities/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        Assert.Equal("Picnic", json.GetProperty("title").GetString());
        Assert.Equal(host.Username, json.GetProperty("hostUsername").GetString());
        Assert.False(json.GetProperty("isCancelled").GetBoolean());
    }

    [Fact]
    public async Task Dates_are_returned_as_utc_with_a_Z_suffix()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());
        var id = Guid.NewGuid();

        var created = await client.PostAsJsonAsync("/api/activities", new
        {
            id,
            title = "UTC",
            date = "2030-05-01T15:00:00Z",
            description = "d",
            category = "music",
            city = "c",
            venue = "v"
        });
        created.EnsureSuccessStatusCode();

        var json = await (await client.GetAsync($"/api/activities/{id}")).ReadJsonAsync();
        Assert.Equal("2030-05-01T15:00:00Z", json.GetProperty("date").GetString());
    }

    [Fact]
    public async Task Invalid_data_is_rejected_with_a_list_of_errors()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.PostAsJsonAsync(
            "/api/activities", ApiHelpers.ActivityBody(Guid.NewGuid(), title: new string('x', 101), category: "bad"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.ReadJsonAsync()).GetProperty("errors");
        Assert.True(errors.TryGetProperty("Activity.Title", out _));
        Assert.True(errors.TryGetProperty("Activity.Category", out _));
    }

    [Fact]
    public async Task Reusing_an_existing_id_is_rejected()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());
        var id = await client.CreateActivityAsync();

        var response = await client.PostAsJsonAsync("/api/activities", ApiHelpers.ActivityBody(id, "Copy"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Extra_fields_in_the_body_cannot_add_attendees_or_cancel_the_activity()
    {
        var host = await factory.CreateUserAsync();
        var other = await factory.CreateUserAsync();
        var client = factory.ClientFor(host);
        var id = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/api/activities", new
        {
            id,
            title = "Sneaky",
            date = DateTime.UtcNow.AddDays(3),
            description = "d",
            category = "music",
            city = "c",
            venue = "v",
            isCancelled = true,
            attendees = new[] { new { isHost = true, appUserId = other.Username } }
        });
        response.EnsureSuccessStatusCode();

        var json = await (await client.GetAsync($"/api/activities/{id}")).ReadJsonAsync();
        Assert.Equal(1, json.GetProperty("attendees").GetArrayLength());
        Assert.Equal(host.Username, json.GetProperty("hostUsername").GetString());
        Assert.False(json.GetProperty("isCancelled").GetBoolean());
    }

    [Fact]
    public async Task The_host_can_edit_the_activity()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());
        var id = await client.CreateActivityAsync("Old title");

        var response = await client.PutAsJsonAsync($"/api/activities/{id}", ApiHelpers.ActivityBody(id, "New title"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await (await client.GetAsync($"/api/activities/{id}")).ReadJsonAsync();
        Assert.Equal("New title", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Another_user_cannot_edit_the_activity()
    {
        var hostClient = factory.ClientFor(await factory.CreateUserAsync());
        var id = await hostClient.CreateActivityAsync("Original");
        var intruder = factory.ClientFor(await factory.CreateUserAsync());

        var response = await intruder.PutAsJsonAsync($"/api/activities/{id}", ApiHelpers.ActivityBody(id, "Hacked"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var json = await (await hostClient.GetAsync($"/api/activities/{id}")).ReadJsonAsync();
        Assert.Equal("Original", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Another_user_cannot_delete_the_activity()
    {
        var hostClient = factory.ClientFor(await factory.CreateUserAsync());
        var id = await hostClient.CreateActivityAsync();
        var intruder = factory.ClientFor(await factory.CreateUserAsync());

        var response = await intruder.DeleteAsync($"/api/activities/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await hostClient.GetAsync($"/api/activities/{id}")).StatusCode);
    }

    [Fact]
    public async Task The_host_can_delete_the_activity()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());
        var id = await client.CreateActivityAsync();

        var response = await client.DeleteAsync($"/api/activities/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/activities/{id}")).StatusCode);
    }

    [Fact]
    public async Task An_unknown_activity_is_a_404()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.GetAsync($"/api/activities/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task A_malformed_id_is_a_400_with_an_id_error()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.GetAsync("/api/activities/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await response.ReadJsonAsync()).GetProperty("errors").TryGetProperty("id", out _));
    }

    [Fact]
    public async Task Another_user_can_join_and_leave()
    {
        var id = await factory.ClientFor(await factory.CreateUserAsync()).CreateActivityAsync();
        var guest = factory.ClientFor(await factory.CreateUserAsync());

        Assert.Equal(HttpStatusCode.OK, (await guest.PostAsync($"/api/activities/{id}/attend", null)).StatusCode);
        Assert.Equal(2, await AttendeesAsync(guest, id));

        Assert.Equal(HttpStatusCode.OK, (await guest.PostAsync($"/api/activities/{id}/attend", null)).StatusCode);
        Assert.Equal(1, await AttendeesAsync(guest, id));
    }

    [Fact]
    public async Task The_host_can_cancel_and_nobody_can_join_a_cancelled_activity()
    {
        var hostClient = factory.ClientFor(await factory.CreateUserAsync());
        var id = await hostClient.CreateActivityAsync();
        var guest = factory.ClientFor(await factory.CreateUserAsync());

        await hostClient.PostAsync($"/api/activities/{id}/attend", null);

        var json = await (await hostClient.GetAsync($"/api/activities/{id}")).ReadJsonAsync();
        Assert.True(json.GetProperty("isCancelled").GetBoolean());
        Assert.Equal(HttpStatusCode.BadRequest, (await guest.PostAsync($"/api/activities/{id}/attend", null)).StatusCode);
    }

    [Fact]
    public async Task Paging_values_out_of_range_are_corrected_and_reported_in_a_header()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.GetAsync("/api/activities?pageNumber=-1&pageSize=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var header = Assert.Single(response.Headers.GetValues("Pagination"));
        var pagination = JsonDocument.Parse(header).RootElement;
        Assert.Equal(1, pagination.GetProperty("currentPage").GetInt32());
        Assert.Equal(10, pagination.GetProperty("itemsPerPage").GetInt32());
    }
}