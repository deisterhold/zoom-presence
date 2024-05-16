using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ZoomPresence.Models;

namespace ZoomPresence
{
    public class ZoomPresence
    {
        private readonly ILogger _logger;

        public ZoomPresence(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ZoomPresence>();
        }

        [Function("ZoomPresence")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var json = await req.ReadFromJsonAsync<dynamic>();

            if (json == null)
            {
                _logger.LogWarning($"Request body is required.");
                return await req.BadRequest($"Request body is required.");
            }

            switch (json.@event.ToString())
            {
                case "endpoint.url_validation":
                    break;
                default:
                    _logger.LogWarning("Unsupported event: {Event}", (object)json.@event);
                    return await req.BadRequest($"Unsupported event: {json.@event}");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }

        private string CreateHash(string key, string value)
        {
            using var alg = new HMACSHA256(Encoding.UTF8.GetBytes(key));

            var hash = alg.ComputeHash(Encoding.UTF8.GetBytes(value));
            var hex = Convert.ToHexString(hash);

            return hex;
        }
    }

    public static class HttpRequestDataExtensions
    {
        public static async Task<HttpResponseData> BadRequest(this HttpRequestData request, string message)
        {
            var response = request.CreateResponse(HttpStatusCode.BadRequest);

            await response.WriteAsJsonAsync<string> (message);

            return response;
        }
    }
}
