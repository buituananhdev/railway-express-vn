using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiGateway;
public class SwaggerDocumentMerger : IDocumentFilter
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SwaggerDocumentMerger> _logger;

    public SwaggerDocumentMerger(IConfiguration configuration, ILogger<SwaggerDocumentMerger> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(1) // Add timeout to avoid long waits
        };
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Info.Version != "v1") return;

        var services = GetServicesFromConfiguration();

        foreach (var service in services)
        {
            try
            {
                var swaggerJson = FetchSwaggerDoc(service.Url);
                if (swaggerJson == null) continue;

                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(swaggerJson));
                var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

                if (diagnostic.Errors.Count > 0)
                {
                    _logger.LogWarning("Swagger validation errors for {ServiceUrl}: {Errors}",
                        service.Url,
                        string.Join(", ", diagnostic.Errors));
                    continue;
                }

                MergePaths(swaggerDoc, openApiDocument, service.PathPrefix);
                MergeComponents(swaggerDoc, openApiDocument);

                _logger.LogInformation("Successfully merged swagger doc for {ServiceUrl}", service.Url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to merge swagger doc for {ServiceUrl}: {Error}",
                    service.Url,
                    ex.Message);
                continue; // Skip this service and continue with others
            }
        }
    }

    private string? FetchSwaggerDoc(string url)
    {
        try
        {
            return _httpClient.GetStringAsync(url).Result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("HTTP request failed for {Url}: {Error}", url, ex.Message);
            return null;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request timed out for {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unexpected error fetching swagger doc from {Url}: {Error}", url, ex.Message);
            return null;
        }
    }

    private List<(string Url, string PathPrefix)> GetServicesFromConfiguration()
    {
        var services = new List<(string Url, string PathPrefix)>();
        var clusters = _configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        var routes = _configuration.GetSection("ReverseProxy:Routes").GetChildren();

        foreach (var cluster in clusters)
        {
            try
            {
                var clusterName = cluster.Key;
                var destination = cluster.GetSection("Destinations").GetChildren().First();
                var address = destination.GetSection("Address").Value;

                if (string.IsNullOrEmpty(address))
                {
                    _logger.LogWarning("Missing address for cluster {ClusterName}", clusterName);
                    continue;
                }

                var matchingRoute = routes.FirstOrDefault(r => r.GetSection("ClusterId").Value == clusterName);
                if (matchingRoute == null)
                {
                    _logger.LogWarning("No matching route found for cluster {ClusterName}", clusterName);
                    continue;
                }

                var pathPrefix = matchingRoute.GetSection("Match:Path").Value?.Split("/{**")[0];
                if (string.IsNullOrEmpty(pathPrefix))
                {
                    _logger.LogWarning("Invalid path prefix for route {ClusterName}", clusterName);
                    continue;
                }

                var swaggerUrl = $"{address.TrimEnd('/')}/swagger/v1/swagger.json";
                services.Add((swaggerUrl, pathPrefix));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error processing configuration for cluster {ClusterName}: {Error}",
                    cluster.Key,
                    ex.Message);
                continue;
            }
        }

        return services;
    }

    private void MergePaths(OpenApiDocument swaggerDoc, OpenApiDocument sourceDoc, string pathPrefix)
    {
        var modifiedPaths = new OpenApiPaths();
        foreach (var path in sourceDoc.Paths)
        {
            modifiedPaths.Add(pathPrefix + path.Key, path.Value);
        }

        foreach (var path in modifiedPaths)
        {
            if (!swaggerDoc.Paths.ContainsKey(path.Key))
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }

    private void MergeComponents(OpenApiDocument swaggerDoc, OpenApiDocument sourceDoc)
    {
        if (sourceDoc.Components?.Schemas == null) return;

        foreach (var schema in sourceDoc.Components.Schemas)
        {
            if (!swaggerDoc.Components.Schemas.ContainsKey(schema.Key))
            {
                swaggerDoc.Components.Schemas.Add(schema.Key, schema.Value);
            }
        }
    }
}
