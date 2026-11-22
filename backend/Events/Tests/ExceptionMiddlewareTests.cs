using System.Text.Json;
using API.Middleware;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests;

public class ExceptionMiddlewareTests
{
    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static async Task<(DefaultHttpContext Context, string Text)> InvokeAsync(
        RequestDelegate next,
        string environment = "Production",
        CancellationToken aborted = default)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestAborted = aborted;

        var middleware = new ExceptionMiddleware(
            next, NullLogger<ExceptionMiddleware>.Instance, new TestEnvironment(environment));
        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context, text);
    }

    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact]
    public async Task Passes_through_when_nothing_is_thrown()
    {
        var (context, text) = await InvokeAsync(_ => Task.CompletedTask);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Empty(text);
    }

    [Fact]
    public async Task Production_returns_a_generic_500_without_details()
    {
        var (context, text) = await InvokeAsync(_ => throw new InvalidOperationException("secret"));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = Json(text);
        Assert.Equal("Server Error", body.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("details").ValueKind);
        Assert.DoesNotContain("secret", text);
    }

    [Fact]
    public async Task Development_returns_the_message_and_the_stack_trace()
    {
        var (context, text) = await InvokeAsync(_ => throw new InvalidOperationException("boom"), "Development");

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = Json(text);
        Assert.Equal("boom", body.GetProperty("message").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("details").GetString()));
    }

    [Fact]
    public async Task Validation_errors_become_a_400_with_messages_grouped_by_property()
    {
        var failures = new[]
        {
            new ValidationFailure("Title", "Title is required"),
            new ValidationFailure("Title", "Title is too short"),
            new ValidationFailure("City", "City is required")
        };

        var (context, text) = await InvokeAsync(_ => throw new ValidationException(failures));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        var errors = Json(text).GetProperty("errors");
        Assert.Equal(2, errors.GetProperty("Title").GetArrayLength());
        Assert.Equal(1, errors.GetProperty("City").GetArrayLength());
    }

    [Fact]
    public async Task A_request_aborted_by_the_client_is_not_an_error()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var (context, text) = await InvokeAsync(_ => throw new OperationCanceledException(cts.Token), aborted: cts.Token);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Empty(text);
    }

    [Fact]
    public async Task A_cancellation_while_the_client_is_still_connected_is_a_server_error()
    {
        var (context, _) = await InvokeAsync(_ => throw new OperationCanceledException());

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }
}