using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiGateway;
public class SwaggerDocumentMerger : IDocumentFilter
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SwaggerDocumentMerger(IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Info.Version != "v1") return;

        var services = new[]
        {
            new { Url = "http://localhost:8007/swagger/v1/swagger.json", PathPrefix = "/auth" },
            new { Url = "http://localhost:8001/swagger/v1/swagger.json", PathPrefix = "/admin" },
            //new { Url = "http://localhost:8002/swagger/v1/swagger.json", PathPrefix = "/booking" },
            //new { Url = "http://localhost:8003/swagger/v1/swagger.json", PathPrefix = "/notifications" },
            new { Url = "http://localhost:8004/swagger/v1/swagger.json", PathPrefix = "/passengers" },
            //new { Url = "http://localhost:8005/swagger/v1/swagger.json", PathPrefix = "/payments" },
            //new { Url = "http://localhost:8006/swagger/v1/swagger.json", PathPrefix = "/trainschedule" },
        };

        foreach (var service in services)
        {
            try
            {
                var swaggerJson = _httpClient.GetStringAsync(service.Url).Result;
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(swaggerJson)))
                {
                    var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
                    if (diagnostic.Errors.Count > 0)
                    {
                        continue;
                    }

                    var modifiedPaths = new OpenApiPaths();
                    foreach (var path in openApiDocument.Paths)
                    {
                        modifiedPaths.Add(service.PathPrefix + path.Key, path.Value);
                    }

                    foreach (var path in modifiedPaths)
                    {
                        if (!swaggerDoc.Paths.ContainsKey(path.Key))
                        {
                            swaggerDoc.Paths.Add(path.Key, path.Value);
                        }
                    }

                    if (openApiDocument.Components?.Schemas != null)
                    {
                        foreach (var schema in openApiDocument.Components.Schemas)
                        {
                            if (!swaggerDoc.Components.Schemas.ContainsKey(schema.Key))
                            {
                                swaggerDoc.Components.Schemas.Add(schema.Key, schema.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error merging swagger doc for {service.Url}: {ex.Message}");
            }
        }
    }
}