using Application.Activities;
using Application.Core;
using Domain;
using FluentValidation;
using FluentValidation.TestHelper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Tests.Support;
using Xunit;
using ActivityCreate = Application.Activities.Create;
using ActivityEdit = Application.Activities.Edit;
using CommentCreate = Application.Comments.Create;
using PhotoAdd = Application.Photos.Add;

namespace Tests;

public class ActivityValidatorTests
{
    private readonly ActivityValidator _validator = new();

    private static Activity Valid() => TestData.NewActivity("Picnic");

    private static void Set(Activity activity, string property, object? value) =>
        typeof(Activity).GetProperty(property)!.SetValue(activity, value);

    [Fact]
    public void A_valid_activity_passes() =>
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(nameof(Activity.Title))]
    [InlineData(nameof(Activity.Description))]
    [InlineData(nameof(Activity.Category))]
    [InlineData(nameof(Activity.City))]
    [InlineData(nameof(Activity.Venue))]
    public void Required_text_fields_cannot_be_empty(string property)
    {
        var activity = Valid();
        Set(activity, property, "");

        Assert.False(_validator.Validate(activity).IsValid);
    }

    [Theory]
    [InlineData(nameof(Activity.Title), 100)]
    [InlineData(nameof(Activity.Description), 2000)]
    [InlineData(nameof(Activity.City), 100)]
    [InlineData(nameof(Activity.Venue), 200)]
    public void Text_fields_have_a_maximum_length(string property, int max)
    {
        var tooLong = Valid();
        Set(tooLong, property, new string('x', max + 1));
        Assert.False(_validator.Validate(tooLong).IsValid);

        var atLimit = Valid();
        Set(atLimit, property, new string('x', max));
        Assert.True(_validator.Validate(atLimit).IsValid);
    }

    [Fact]
    public void The_date_is_required()
    {
        var activity = Valid();
        activity.Date = default;

        _validator.TestValidate(activity).ShouldHaveValidationErrorFor(a => a.Date);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void The_date_must_be_in_the_allowed_range(int year)
    {
        var activity = Valid();
        activity.Date = new DateTime(year, 6, 1);

        _validator.TestValidate(activity).ShouldHaveValidationErrorFor(a => a.Date);
    }

    [Theory]
    [InlineData("drinks")]
    [InlineData("culture")]
    [InlineData("film")]
    [InlineData("food")]
    [InlineData("music")]
    [InlineData("travel")]
    public void Known_categories_are_accepted(string category)
    {
        var activity = Valid();
        activity.Category = category;

        _validator.TestValidate(activity).ShouldNotHaveValidationErrorFor(a => a.Category);
    }

    [Fact]
    public void An_unknown_category_is_rejected()
    {
        var activity = Valid();
        activity.Category = "bad";

        _validator.TestValidate(activity).ShouldHaveValidationErrorFor(a => a.Category);
    }

    [Fact]
    public void Create_and_edit_commands_validate_the_nested_activity()
    {
        var invalid = Valid();
        invalid.Title = "";

        new ActivityCreate.CommandValidator()
            .TestValidate(new ActivityCreate.Command { Activity = invalid })
            .ShouldHaveValidationErrorFor("Activity.Title");

        new ActivityEdit.CommandValidator()
            .TestValidate(new ActivityEdit.Command { Activity = invalid })
            .ShouldHaveValidationErrorFor("Activity.Title");
    }
}

public class CommentValidatorTests
{
    private readonly CommentCreate.CommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void The_body_is_required(string body) =>
        _validator.TestValidate(new CommentCreate.Command { Body = body })
            .ShouldHaveValidationErrorFor(c => c.Body);

    [Fact]
    public void The_body_is_limited_to_2000_characters()
    {
        Assert.True(_validator.Validate(new CommentCreate.Command { Body = new string('x', 2000) }).IsValid);
        Assert.False(_validator.Validate(new CommentCreate.Command { Body = new string('x', 2001) }).IsValid);
    }
}

public class AddPhotoValidatorTests
{
    private const long TenMegabytes = 10 * 1024 * 1024;
    private readonly PhotoAdd.CommandValidator _validator = new();

    private static PhotoAdd.Command CommandFor(long size = 1024, string contentType = "image/png") => new()
    {
        // длина задаётся отдельно от потока: огромные массивы в памяти не нужны
        File = new FormFile(Stream.Null, 0, size, "File", "photo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        }
    };

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public void Allowed_image_types_pass(string contentType) =>
        Assert.True(_validator.Validate(CommandFor(contentType: contentType)).IsValid);

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/pdf")]
    [InlineData("image/svg+xml")]
    public void Other_types_are_rejected(string contentType) =>
        Assert.False(_validator.Validate(CommandFor(contentType: contentType)).IsValid);

    [Fact]
    public void A_missing_file_is_rejected() =>
        Assert.False(_validator.Validate(new PhotoAdd.Command { File = null! }).IsValid);

    [Fact]
    public void An_empty_file_is_rejected() =>
        Assert.False(_validator.Validate(CommandFor(size: 0)).IsValid);

    [Fact]
    public void The_size_limit_is_ten_megabytes()
    {
        Assert.True(_validator.Validate(CommandFor(size: TenMegabytes)).IsValid);
        Assert.False(_validator.Validate(CommandFor(size: TenMegabytes + 1)).IsValid);
    }
}

public class ValidationBehaviorTests
{
    public record Ping(string Name) : IRequest<string>;

    public class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }

    public class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) =>
            Task.FromResult($"pong {request.Name}");
    }

    public record Silent : IRequest<string>;

    public class SilentHandler : IRequestHandler<Silent, string>
    {
        public Task<string> Handle(Silent request, CancellationToken cancellationToken) => Task.FromResult("ok");
    }

    private static IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ValidationBehaviorTests>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddScoped<IValidator<Ping>, PingValidator>();

        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task A_valid_request_reaches_the_handler() =>
        Assert.Equal("pong Bob", await CreateMediator().Send(new Ping("Bob")));

    [Fact]
    public async Task An_invalid_request_is_rejected_with_the_validation_errors()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => CreateMediator().Send(new Ping("")));

        Assert.Equal("Name is required", Assert.Single(exception.Errors).ErrorMessage);
    }

    [Fact]
    public async Task A_request_without_validators_passes_through() =>
        Assert.Equal("ok", await CreateMediator().Send(new Silent()));
}