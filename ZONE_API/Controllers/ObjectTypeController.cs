using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;

namespace ZONE_API.Controllers
{
    [Route("api/Zone/[controller]")]
    [ApiController]
    public class ObjectTypeController : ControllerBase
    {
        private readonly IObjectTypeDomain _objectTypeDomain;

        public ObjectTypeController(IObjectTypeDomain objectTypeDomain)
        {
            _objectTypeDomain = objectTypeDomain;
        }

        [Authorize]
        [HttpGet("GetObjectTypeDropdownList")]
        public async Task<IActionResult> GetObjectTypeDropdownList()
        {
            try
            {
                var (result, message) = await _objectTypeDomain.GetObjectTypeDropdownList();

                if (result == null)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new AlprResponse<string>(HttpStatusCode.InternalServerError, null, message));
                }

                var status = result.Any() ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                return Ok(new AlprResponse<List<object>>(status, result, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }
    }
}
