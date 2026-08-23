using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.OpenApi;

public sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        };

        var bearerReference = new OpenApiSecuritySchemeReference("Bearer", document);

        var anonymousEndpoints = new HashSet<(string Path, string HttpMethod)>(
            ComparerTuple.Instance
        );

        foreach (var group in context.DescriptionGroups)
        {
            foreach (var items in group.Items)
            {
                var hasAllowAnonymous = items.ActionDescriptor?.EndpointMetadata?
                    .OfType<IAllowAnonymous>()
                    .Any() ?? false;

                if (hasAllowAnonymous && !string.IsNullOrEmpty(items.RelativePath))
                {
                    var formattedPath = items.RelativePath;
                    if (!formattedPath.StartsWith('/'))
                    {
                        formattedPath = "/" + formattedPath;
                    }

                    var method = (items.HttpMethod ?? "GET").ToUpperInvariant();
                    anonymousEndpoints.Add((formattedPath, method));
                }
            }
        }

        foreach (var path in document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var httpMethodStr = operation.Key.ToString().ToUpperInvariant();

                if (anonymousEndpoints.Contains((path.Key, httpMethodStr)))
                {
                    continue;
                }

                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [bearerReference] = []
                });
            }
        }

        return Task.CompletedTask;
    }

    private sealed class ComparerTuple : IEqualityComparer<(string Path, string HttpMethod)>
    {
        public static readonly ComparerTuple Instance = new();

        public bool Equals((string Path, string HttpMethod) x, (string Path, string HttpMethod) y) =>
            string.Equals(x.Path, y.Path, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.HttpMethod, y.HttpMethod, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Path, string HttpMethod) obj) =>
            HashCode.Combine(
                obj.Path.ToLowerInvariant(),
                obj.HttpMethod.ToLowerInvariant()
            );
    }
}
