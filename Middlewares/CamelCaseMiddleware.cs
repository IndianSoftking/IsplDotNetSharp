using System.Text;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ISPL.NetCoreFramework.Middlewares
{
    public class CamelCaseMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip camel-casing for Swagger JSON and Swagger UI
            if(context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/index.html", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/odata/$metadata", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;

            using MemoryStream newBody = new();
            context.Response.Body = newBody;

            await _next(context);

            if(context.Response.ContentType != null &&
                context.Response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                _ = newBody.Seek(0, SeekOrigin.Begin);
                using StreamReader reader = new(newBody);
                var bodyText = await reader.ReadToEndAsync();

                if(!(string.IsNullOrWhiteSpace(bodyText) || !bodyText.TrimStart().StartsWith("{")))
                {
                    var json = JsonConvert.DeserializeObject<JToken>(bodyText);
                    var camelCased = CamelCaseJson(json!);

                    var modifiedBody = JsonConvert.SerializeObject(camelCased);

                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(modifiedBody);
                    context.Response.Body = originalBody;
                    await context.Response.WriteAsync(modifiedBody);
                    return;
                }
            }

            _ = newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }


        private static JToken CamelCaseJson(JToken token)
        {
            if(token is JObject obj)
            {
                JObject newObj = [];
                foreach(JProperty property in obj.Properties())
                {
                    string camelName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
                    newObj[camelName] = CamelCaseJson(property.Value);
                }
                return newObj;
            }
            else if(token is JArray arr)
            {
                for(int i = 0; i < arr.Count; i++)
                {
                    arr[i] = CamelCaseJson(arr[i]);
                }
                return arr;
            }
            else
            {
                return token;
            }
        }
    }
}