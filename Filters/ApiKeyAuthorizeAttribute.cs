using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace api_aguas.Filters
{
    public class ApiKeyAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public string RequiredKey { get; set; } = "Any";

        public override void OnAuthorization(HttpActionContext httpContext)
        {
            var providedApiKey = httpContext.Request.Headers.Contains("X-Api-Key")
                ? httpContext.Request.Headers.GetValues("X-Api-Key").FirstOrDefault()
                : null;

            var defaultApiKey = ConfigurationManager.AppSettings["DefaultApiKey"];
            var adminApiKey = ConfigurationManager.AppSettings["AdminApiKey"];

            bool isAuthorized;

            // API key specific case
            if (RequiredKey == "Default")
                isAuthorized = providedApiKey == defaultApiKey;
            else if (RequiredKey == "Administrator")
                isAuthorized = providedApiKey == adminApiKey;
            else
                isAuthorized = providedApiKey == defaultApiKey || providedApiKey == adminApiKey;

            if (isAuthorized)
                return;

            // Unauthorized
            httpContext.Response = httpContext.Request.CreateResponse(HttpStatusCode.Unauthorized, new
            {
                success = false,
                message = "Unauthorized: Invalid or Insufficient API Key"
            });
        }
    }
}