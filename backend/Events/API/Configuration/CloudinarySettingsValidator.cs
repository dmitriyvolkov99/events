using Infrastructure.Photos;
using Microsoft.Extensions.Options;

namespace API.Configuration;

public class CloudinarySettingsValidator(IHostEnvironment environment, ILogger<CloudinarySettingsValidator> logger)
    : IValidateOptions<CloudinarySettings>
{
    public ValidateOptionsResult Validate(string? name, CloudinarySettings options)
    {
        List<string> missing = [];

        if (string.IsNullOrWhiteSpace(options.CloudName)) missing.Add("Cloudinary:CloudName");
        if (string.IsNullOrWhiteSpace(options.ApiKey)) missing.Add("Cloudinary:ApiKey");
        if (string.IsNullOrWhiteSpace(options.ApiSecret)) missing.Add("Cloudinary:ApiSecret");

        if (missing.Count == 0) return ValidateOptionsResult.Success;

        var message = $"Missing Cloudinary settings: {string.Join(", ", missing)}";

        if (environment.IsDevelopment())
        {
            logger.LogWarning("{Message}. Photo uploads will not work.", message);
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(message);
    }
}