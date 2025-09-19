using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;
using ZONE.Entity.Context;
using ZONE.EntityDto.Model;

namespace ZONE_API.Controllers
{
    [Route("api/Zone/[controller]")]
    [ApiController]
    public class CameraRoiController : ControllerBase
    {
        private readonly ICameraRoiDomain _cameraRoiDomain;
        private readonly ZoneDbContext _zoneDbContext;
        private readonly IConfiguration _config;

        public CameraRoiController(ICameraRoiDomain cameraRoiDomain, ZoneDbContext zoneDbContext, IConfiguration config)
        {
            _cameraRoiDomain = cameraRoiDomain;
            _zoneDbContext = zoneDbContext;
            _config = config;
        }

        [Authorize]
        [HttpGet("GetByCamera")]
        public async Task<IActionResult> GetByCamera([BindRequired, FromQuery] int cameraId)
        {
            if (cameraId <= 0)
            {
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraId."));
            }

            try
            {
                var (cameraRois, message) = await _cameraRoiDomain.GetByCamera(cameraId);

                if (cameraRois == null || !cameraRois.Any())
                {
                    return Ok(new AlprListResponse<List<CameraRoiDto>>(HttpStatusCode.NotFound, cameraRois, 0, message));
                }

                return Ok(new AlprListResponse<List<CameraRoiDto>>(HttpStatusCode.OK, cameraRois, 0, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BulkCreate([FromBody] List<CameraRoiDto> cameraRois)
        {
            if (cameraRois == null || !cameraRois.Any())
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Camera Roi data is required."));

            try
            {
                var (createdList, message) = await _cameraRoiDomain.CreateCameraRois(cameraRois);

                if (createdList != null && createdList.Any())
                    return Ok(new AlprResponse<List<CameraRoiDto>>(HttpStatusCode.OK, createdList, message));

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }


        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> BulkUpdate([FromBody] List<CameraRoiDto> cameraRois)
        {
            if (cameraRois == null || !cameraRois.Any())
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid bulk update data."));

            try
            {
                var (updatedList, message) = await _cameraRoiDomain.UpdateCameraRois(cameraRois);

                if (updatedList == null || !updatedList.Any())
                    return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));

                return Ok(new AlprResponse<List<CameraRoiDto>>(HttpStatusCode.OK, updatedList, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, "Internal server error: " + ex.Message));
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> cameraRoiIds)
        {
            if (cameraRoiIds == null || !cameraRoiIds.Any())
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraRoiIds for delete."));

            var (isDeleted, message) = await _cameraRoiDomain.DeleteCameraRois(cameraRoiIds);

            if (!isDeleted)
                return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));

            return Ok(new AlprResponse<string>(HttpStatusCode.OK, null, message));
        }
    }
}
