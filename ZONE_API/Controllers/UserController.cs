using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Net;
using System.Xml;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;
using ZONE.Entity.Context;
using ZONE.EntityDto.Model;

namespace ZONE_API.Controllers
{
    [Route("api/Zone/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserDetailDomain _userDetailDomain;
        private readonly ZoneDbContext _zoneDbContext;

        public UserController(IUserDetailDomain userDetailDomain, ZoneDbContext zoneDbContext)
        {
            _userDetailDomain = userDetailDomain;
            _zoneDbContext = zoneDbContext;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AlprResponse<UserLoginResponseModel>>> Login([BindRequired, FromBody] UserLoginRequestModel loginModel)
        {
            var requestUrl = HttpContext.Request.Path.Value;


            if (!ModelState.IsValid
                || string.IsNullOrEmpty(loginModel.UserName?.Trim())
                || string.IsNullOrEmpty(loginModel.Password?.Trim()))
            {
                return new AlprResponse<UserLoginResponseModel>(System.Net.HttpStatusCode.BadRequest, null, "UserName/Password is missing!");
            }

            try
            {
                var (result, message) = await _userDetailDomain.GetToken(loginModel.UserName, loginModel.Password);
                if (result == null)
                {
                    return Unauthorized(new AlprResponse<UserLoginResponseModel>(System.Net.HttpStatusCode.Unauthorized, null, "message"));
                }
                return new AlprResponse<UserLoginResponseModel>(System.Net.HttpStatusCode.OK, result, "Success");
            }
            catch (Exception ex)
            {
                return new AlprResponse<UserLoginResponseModel>(System.Net.HttpStatusCode.BadRequest, null, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("Get")]
        public async Task<IActionResult> Get(RequestParam requestParam)
        {
            var claims = User.Claims;
            var userName = claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? string.Empty;
            var requestUrl = HttpContext.Request.Path.Value;
            var requestParamJson = JsonConvert.SerializeObject(requestParam, Newtonsoft.Json.Formatting.None);

            //_serilogger.LogDebug($"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, RequestParam: {requestParamJson}");

            try
            {
                var (users, totalCount, message) = await _userDetailDomain.GetAllUsers(requestParam);

                if (users != null && users.Any())
                {
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: POST, Status: {HttpStatusCode.OK}");
                    return Ok(new AlprListResponse<List<UserDetailView>>(HttpStatusCode.OK, users, totalCount, message));
                }
                else
                {
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: POST, Status: {HttpStatusCode.NotFound}");
                    return Ok(new AlprListResponse<List<UserDetailView>>(HttpStatusCode.NotFound, users, totalCount, message));
                }
            }
            catch (Exception ex)
            {
                //_serilogger.LogError($"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, RequestParam: {requestParamJson}, Status: {HttpStatusCode.InternalServerError}, Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("GetUserByUserId")]
        public async Task<IActionResult> GetUserByUserId([BindRequired, FromQuery] int id)
        {
            var claims = User.Claims;
            var userName = claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? string.Empty;
            var requestUrl = HttpContext.Request.Path.Value;

            //_serilogger.LogDebug($"Module: User, Name: {userName}, Method: GET, RequestUrl: {requestUrl}, Request: {id}");

            if (id <= 0)
            {
                //_serilogger.LogWarning($"Module: User, Name: {userName}, Method: GET, RequestUrl: {requestUrl}, RequestParam: {id}, Status: {HttpStatusCode.BadRequest}");
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid user ID."));
            }

            try
            {
                var (user, message) = await _userDetailDomain.GetUserById(id);

                if (user == null)
                {
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: GET, RequestUrl: {requestUrl}, RequestParam: {id}, Status: {HttpStatusCode.NotFound}");
                    return NotFound(new AlprResponse<UserDetailView>(HttpStatusCode.NotFound, null, message));
                }

                //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: GET, RequestUrl: {requestUrl}, RequestParam: {id}, Status: {HttpStatusCode.OK}");
                return Ok(new AlprResponse<UserDetailView>(HttpStatusCode.OK, user, message));
            }
            catch (Exception ex)
            {
                //_serilogger.LogError($"Module: User, Name: {userName}, Method: GET, RequestUrl: {requestUrl}, Request: {id}, Status: {HttpStatusCode.InternalServerError}");

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDetailDto userDto)
        {
            var claims = User.Claims;
            var userName = claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? string.Empty;
            var requestUrl = HttpContext.Request.Path.Value;
            var userDtoJson = JsonConvert.SerializeObject(userDto, Newtonsoft.Json.Formatting.None);

            //_serilogger.LogDebug($"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, Request: {userDtoJson}");

            if (userDto == null)
            {
                //_serilogger.LogWarning($"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, Request: null, Status: {HttpStatusCode.BadRequest}");
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "User data is required."));
            }

            if (!ModelState.IsValid)
            {
                //_serilogger.LogWarning($"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, Request: {userDtoJson}, Status: {HttpStatusCode.BadRequest}");
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid user data."));
            }

            try
            {
                var conflicts = new List<string>();

                if (!string.IsNullOrEmpty(userDto.UserName) &&
                    userDto.UserName.Trim().ToLower() == "admin")
                {
                    conflicts.Add("UserName");
                }

                if (!string.IsNullOrEmpty(userDto.EmailId) || !string.IsNullOrEmpty(userDto.PhoneNumber))
                {
                    var user = _zoneDbContext.UserDetails.FirstOrDefault(a =>
                        (!string.IsNullOrEmpty(userDto.EmailId) && a.EmailId.Trim().ToLower() == userDto.EmailId.Trim().ToLower()) ||
                        (!string.IsNullOrEmpty(userDto.PhoneNumber) && a.PhoneNumber.Trim().ToLower() == userDto.PhoneNumber.Trim().ToLower()));

                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(userDto.EmailId) && user.EmailId.Trim().ToLower() == userDto.EmailId.Trim().ToLower())
                            conflicts.Add("EmailId");
                        if (!string.IsNullOrEmpty(userDto.PhoneNumber) && user.PhoneNumber.Trim().ToLower() == userDto.PhoneNumber.Trim().ToLower())
                            conflicts.Add("Phone");
                    }
                }

                if (conflicts.Any())
                {
                    var conflictPayload = new { Conflicts = conflicts };
                    var conflictMessage = $"{string.Join(" and ", conflicts)} already exists or is not allowed.";
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: POST, Conflict: {conflictMessage}, Status: {HttpStatusCode.Found}");
                    return Ok(new AlprResponse<object>(HttpStatusCode.Found, conflictPayload, conflictMessage));
                }

                var (userDtoResult, message) = await _userDetailDomain.CreateUser(userDto);

                if (userDtoResult != null)
                {
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: POST, Status: {HttpStatusCode.OK}");
                    return Ok(new AlprResponse<UserDetailView>(HttpStatusCode.OK, userDtoResult, message));
                }

                //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: POST, Status: {HttpStatusCode.NotFound}");
                return Ok(new AlprResponse<UserDetailView>(HttpStatusCode.NotFound, null, message));
            }
            catch (Exception ex)
            {
                //_serilogger.LogError(ex, $"Module: User, Name: {userName}, Method: POST, RequestUrl: {requestUrl}, Request: {userDtoJson}, Status: {HttpStatusCode.InternalServerError}");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }




        [Authorize]
        [HttpPatch("{userId}")]
        public async Task<IActionResult> Update(int userId, [FromBody] UserDetailDto userDto)
        {
            var claims = User.Claims;
            var userName = claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? string.Empty;
            var requestUrl = HttpContext.Request.Path.Value;
            var requestJson = JsonConvert.SerializeObject(userDto);

            //_serilogger.LogDebug($"Module: User, Name: {userName}, Method: PATCH, RequestUrl: {requestUrl}, UserId: {userId}, Request: {requestJson}");

            if (userDto == null)
            {
                //_serilogger.LogWarning($"Module: User, Name: {userName}, Method: PATCH, RequestUrl: {requestUrl}, Status: {HttpStatusCode.BadRequest}");
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid data for update."));
            }

            try
            {
                var conflicts = new List<string>();

                if (!string.IsNullOrEmpty(userDto.UserName) && userDto.UserName.Trim().ToLower() == "admin")
                {
                    conflicts.Add("UserName");
                }

                if (!string.IsNullOrEmpty(userDto.EmailId) || !string.IsNullOrEmpty(userDto.PhoneNumber))
                {
                    var conflictUser = _zoneDbContext.UserDetails
                        .FirstOrDefault(a => a.UserId != userId &&
                            (
                                (!string.IsNullOrEmpty(userDto.EmailId) && a.EmailId.ToLower() == userDto.EmailId.ToLower()) ||
                                (!string.IsNullOrEmpty(userDto.PhoneNumber) && a.PhoneNumber.ToLower() == userDto.PhoneNumber.ToLower())
                            ));

                    if (conflictUser != null)
                    {
                        if (!string.IsNullOrEmpty(userDto.EmailId) && conflictUser.EmailId.ToLower() == userDto.EmailId.ToLower())
                            conflicts.Add("EmailId");

                        if (!string.IsNullOrEmpty(userDto.PhoneNumber) && conflictUser.PhoneNumber.ToLower() == userDto.PhoneNumber.ToLower())
                            conflicts.Add("Phone");
                    }
                }

                if (conflicts.Any())
                {
                    var conflictPayload = new { Conflicts = conflicts };
                    var conflictMessage = $"{string.Join(" and ", conflicts)} already exists or is not allowed.";

                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: PATCH, RequestUrl: {requestUrl}, Conflict: {conflictMessage}, Status: {HttpStatusCode.Found}");
                    return Ok(new AlprResponse<object>(HttpStatusCode.Found, conflictPayload, conflictMessage));
                }

                var (updatedUser, message) = await _userDetailDomain.UpdateUser(userId, userDto);

                if (updatedUser == null)
                {
                    //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: PATCH, RequestUrl: {requestUrl}, Status: {HttpStatusCode.NotFound}");
                    return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));
                }

                //_serilogger.LogInformation($"Module: User, Name: {userName}, Method: PATCH, RequestUrl: {requestUrl}, Status: {HttpStatusCode.OK}");
                return Ok(new AlprResponse<UserDetailView>(HttpStatusCode.OK, updatedUser, message));
            }
            catch (Exception ex)
            {
                //_serilogger.LogError(ex,"Module: User, Name: {UserName}, Method: PATCH, RequestUrl: {RequestUrl}, UserId: {UserId}, Request: {RequestJson}, Status: {Status}", userName, requestUrl, userId, requestJson, HttpStatusCode.InternalServerError);

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }


        [Authorize]
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userNameFromToken = User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? string.Empty;

            if (userNameFromToken != request.UserName)
                return Unauthorized(new AlprResponse<string>(HttpStatusCode.Unauthorized, null, "Unauthorized request."));

            if (!ModelState.IsValid)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid request data."));

            try
            {
                var (isSuccess, message) = await _userDetailDomain.ChangePassword(request);

                if (isSuccess)
                    return Ok(new AlprResponse<string>(HttpStatusCode.OK, null, message));
                else
                    return Ok(new AlprResponse<string>(HttpStatusCode.BadRequest, null, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, "An unexpected error occurred."));
            }
        }

    }
}
