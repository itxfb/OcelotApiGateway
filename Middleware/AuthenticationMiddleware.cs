using ApiGateway.Models;
using ApiGateway.Services;
using Models.DatabaseModels.Logging;
using Models.ViewModels.Identity;
using Newtonsoft.Json;
using SharedLib.APIs;
using SharedLib.Common;
using ApiResponseType = SharedLib.APIs.ApiResponseType;

namespace APIGateway.Middleware
{
    public class AuthenticationMiddleware
    {
        readonly RequestDelegate _next;
        readonly UserSession userSession;

        private const string AUTHORIZATION_HEADER = "Authorization";

        private VwUser GetVwUser(HttpContext httpContext)
        {
            return (VwUser)httpContext.Items["User"];
        }

        public AuthenticationMiddleware(RequestDelegate next, UserSession userSession)
        {
            _next = next;
            this.userSession = userSession;
        }

        public async Task Invoke(HttpContext context, ILoggingService loggingService)
        {
            string? authToken = context.Request.Headers[AUTHORIZATION_HEADER].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(authToken))
            {
                await _next(context);

                return;
            }

            var sessionKey = authToken.Substring(0, 24);

            if (this.userSession.Sessions.Any(x => x.Key == sessionKey))    // user is valid. let him proceed.
            {
                await _next(context);
            }
            else     // user is not valid. log him and send unauthorized response.
            {
                context.Response.StatusCode = 401;

                var responseBody = ApiResponse.GetApiResponse(ApiResponseType.UN_AUTHORIZED, null, Constants.UN_AUTHORIZED_MESSAGE);

                HttpRequestLog httpRequestLog = new HttpRequestLog();

                try
                {
                    httpRequestLog.Method = context.Request.Method;
                    httpRequestLog.Path = context.Request.PathBase + context.Request.Path;
                    httpRequestLog.ClientIP = context.Connection.RemoteIpAddress.ToString();

                    httpRequestLog.RequestHeaders = JsonConvert.SerializeObject(context.Request.Headers);
                    httpRequestLog.RequestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    httpRequestLog.ResponseHeaders = JsonConvert.SerializeObject(context.Response.Headers);
                    httpRequestLog.ResponseBody = JsonConvert.SerializeObject(responseBody);

                    var currentUser = context.Items["User"] as VwUser;
                    httpRequestLog.CreatedBy = currentUser is not null ? currentUser.UserId : 1;

                    context.Response.OnCompleted(async () =>
                    {
                        await loggingService.LogHttpRequest(httpRequestLog);
                    });

                    await context.Response.WriteAsJsonAsync(responseBody);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                }
            }
        }
    }
}
