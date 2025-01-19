using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNet.Grpc.Api.Filters
{
    /// <summary>
    /// A filter to remove the version parameter from the Swagger document.
    /// </summary>
    public class RemoveVersionParameterFilter : IDocumentFilter
    {
        /// <summary>
        /// Applies the filter to the Swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The Swagger document to filter.</param>
        /// <param name="context">The filter context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths
                .Where(pathItem => pathItem.Key.Contains($"/{swaggerDoc.Info.Version}/"))
                .ToDictionary(pathItem => pathItem.Key, pathItem => pathItem.Value);
            swaggerDoc.Paths = new OpenApiPaths();
            foreach (var path in paths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}
