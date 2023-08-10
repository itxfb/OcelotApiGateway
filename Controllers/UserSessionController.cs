using ApiGateway.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLib.APIs;

namespace ApiGateway.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserSessionController : ControllerBase
    {
        readonly UserSession userSession;

        public UserSessionController(UserSession userSession)
        {
            this.userSession = userSession;
        }

        [HttpPost("AddUserSession")]
        public async Task<ApiResponse> AddUserSession(string sessionKey, long userId)
        {
            try
            {
                var isUserSessionAdded = this.userSession.Sessions.TryAdd(sessionKey, userId);

                if (isUserSessionAdded)
                { 
                    return ApiResponse.GetApiResponse(ApiResponseType.SUCCESS, null, string.Empty);
                }

                return ApiResponse.GetApiResponse(ApiResponseType.FAILED, null, string.Empty);
            }
            catch
            {
                return ApiResponse.GetApiResponse(ApiResponseType.FAILED, null, string.Empty);
            }
        }

        [HttpPost("RemoveUserSession")]

        public async Task<ApiResponse> RemoveUserSession(string sessionKey, long userId)
        {
            try
            {
                var isUserSessionRemove = this.userSession.Sessions.TryRemove(sessionKey, out userId);

                if (isUserSessionRemove)
                {
                    return ApiResponse.GetApiResponse(ApiResponseType.SUCCESS, null, string.Empty);
                }

                return ApiResponse.GetApiResponse(ApiResponseType.FAILED, null, string.Empty);
            }
            catch
            {
                return ApiResponse.GetApiResponse(ApiResponseType.FAILED, null, string.Empty);
            }
        }
    }
}
