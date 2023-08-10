using ApiGateway.Services;
using Models.DatabaseModels.Logging;
using Models.ViewModels.Identity;
using Newtonsoft.Json;
using SharedLib.APIs;
using System.Net;
using System.Text;

namespace APIGateway.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private VwUser GetVwUser(HttpContext httpContext)
        {
            return (VwUser)httpContext.Items["User"];
            //return httpContext?.User?.Identity?.Name ?? String.Empty;
        }

        public HttpLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ILoggingService loggingService)
        {
            var user = this.GetVwUser(context);

            HttpRequestLog httpRequestLog = new HttpRequestLog();

            try
            {
                httpRequestLog.Method = context.Request.Method;
                httpRequestLog.Path = context.Request.PathBase + context.Request.Path;
                httpRequestLog.RequestHeaders = JsonConvert.SerializeObject(context.Request.Headers);
                httpRequestLog.ClientIP = context.Connection.RemoteIpAddress.ToString();

                context.Request.EnableBuffering();

                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true))
                {
                    httpRequestLog.RequestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                try
                {
                    var originalBodyStream = context.Response.Body;

                    using var modifiedResponseBodyStream = new MemoryStream();
                    
                    context.Response.Body = modifiedResponseBodyStream;

                    await _next(context);

                    httpRequestLog.ResponseHeaders = JsonConvert.SerializeObject(context.Response.Headers);
                    httpRequestLog.ResponseStatusCode = context.Response.StatusCode;

                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

                    context.Response.Body.Seek(0, SeekOrigin.Begin);


                    switch (context.Response.StatusCode)
                    {
                        case (int)HttpStatusCode.InternalServerError:

                            var exceptionDetails = JsonConvert.DeserializeObject<Models.ViewModels.Security.ExceptionDetails>(responseBody);
                            var newResponseBody = ApiResponse.GetExceptionGenericResponse(ApiResponseType.EXCEPTION, exceptionDetails.TraceId);
                            var newResponseBodyJsonStr = JsonConvert.SerializeObject(newResponseBody);

                            httpRequestLog.ResponseBody = newResponseBodyJsonStr;
                            httpRequestLog.ExceptionMessage = exceptionDetails.ExceptionMessage;
                            httpRequestLog.StackTrace = exceptionDetails.StackTrack;
                            httpRequestLog.TraceId = exceptionDetails.TraceId;
                            httpRequestLog.IsExceptionRaised = true;

                            var memoryStreamModified = new MemoryStream();
                            var sw = new StreamWriter(memoryStreamModified);
                            sw.Write(newResponseBodyJsonStr);
                            sw.Flush();
                            memoryStreamModified.Position = 0;

                            context.Response.ContentLength = newResponseBodyJsonStr.Length;

                            await memoryStreamModified.CopyToAsync(originalBodyStream);

                            break;

                        default:

                            httpRequestLog.ResponseBody = responseBody;

                            await modifiedResponseBodyStream.CopyToAsync(originalBodyStream);

                            break;
                    }

                    context.Response.Body = originalBodyStream;
                }
                catch (Exception ex)
                {
                    httpRequestLog.IsExceptionRaised = true;
                    httpRequestLog.StackTrace = ex.StackTrace;

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    httpRequestLog.ExceptionMessage = ex.Message;
                    httpRequestLog.TraceId = Guid.NewGuid().ToString();

                    context.Response.StatusCode = 500;
                    var exceptionResponse = ApiResponse.GetExceptionGenericResponse(ApiResponseType.EXCEPTION, httpRequestLog.TraceId);
                    await context.Response.WriteAsJsonAsync(exceptionResponse);
                }
                finally
                {
                    var currentUser = context.Items["User"] as VwUser;
                    httpRequestLog.CreatedBy = currentUser is not null ? currentUser.UserId : 1;

                    context.Response.OnCompleted(async () =>
                    {
                        await loggingService.LogHttpRequest(httpRequestLog);
                    });

                    //await loggingService.LogHttpRequest(httpRequestLog);
                }

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
