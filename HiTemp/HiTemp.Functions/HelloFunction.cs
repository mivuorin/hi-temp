using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HiTemp.Functions
{
    public class HelloFunction(ILogger<HelloFunction> logger)
    {
        [Function(nameof(HelloFunction))]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "hello")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            return new ObjectResult("Welcome to Azure Functions!");
        }
    }
}
