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
            if (cameraId == null && cameraId == 0)
            {
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraId."));
            }
            try
            {
                var (camera, message) = await _cameraRoiDomain.GetByCamera(cameraId);
                if (camera == null)
                {
                    return Ok(new AlprResponse<CameraRoiDto>(HttpStatusCode.NotFound, null, message));
                }
                return Ok(new AlprResponse<CameraRoiDto>(HttpStatusCode.OK, camera, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CameraRoiDto cameraRoiDto)
        {
            if (cameraRoiDto == null)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Camera Roi data is required."));

            if (!ModelState.IsValid)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid camera roi data."));

            try
            {
                var (cameraResult, message) = await _cameraRoiDomain.CreateCameraRoi(cameraRoiDto);

                if (cameraResult != null)
                {
                    return Ok(new AlprResponse<CameraRoiDto>(HttpStatusCode.OK, cameraResult, message));
                }

                return Ok(new AlprResponse<CameraRoiDto>(HttpStatusCode.NotFound, cameraResult, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }


        [Authorize]
        [HttpPatch("{cameraId}")]
        public async Task<IActionResult> Update(int cameraRoiId, [FromBody] CameraRoiDto cameraRoiDto)
        {
            if (cameraRoiDto == null)
            {
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid data for update."));
            }

            try
            {
                var (updatedCamera, message) = await _cameraRoiDomain.UpdateCameraRoi(cameraRoiId, cameraRoiDto);

                if (updatedCamera == null)
                {
                    return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));
                }

                return Ok(new AlprResponse<CameraRoiDto>(HttpStatusCode.OK, updatedCamera, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, "Internal server error: " + ex.Message));
            }
        }

        [Authorize]
        [HttpDelete("{cameraRoiId}")]
        public async Task<IActionResult> Delete(int cameraRoiId)
        {
            if (cameraRoiId == null || cameraRoiId == 0)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraRoiId."));

            var (isDeleted, message) = await _cameraRoiDomain.DeleteCameraRoi(cameraRoiId);

            if (!isDeleted)
                return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));

            return Ok(new AlprResponse<string>(HttpStatusCode.OK, null, message));

        }
    }
}
