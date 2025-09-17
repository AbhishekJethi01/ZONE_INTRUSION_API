using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;
using ZONE.Entity.Context;
using ZONE.EntityDto.Model;

namespace ZONE_API.Controllers
{
    [Route("api/Zone/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly ICameraDetailDomain _cameraDetailDomain;
        private readonly ZoneDbContext _zoneDbContext;
        private readonly IConfiguration _config;

        public CameraController(ICameraDetailDomain cameraDetailDomain, ZoneDbContext zoneDbContext, IConfiguration config)
        {
            _cameraDetailDomain = cameraDetailDomain;
            _zoneDbContext = zoneDbContext;
            _config = config;   
        }

        [Authorize]
        [HttpGet("GetCameraDropdownList")]
        public async Task<IActionResult> GetCameraDropdownList()
        {
            try
            {
                var (result, message) = await _cameraDetailDomain.GetCameraDropdownList();

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

        [Authorize]
        [HttpPost("Get")]
        public async Task<IActionResult> Get(RequestParam requestParam)
        {
            try
            {
                var (camera, totalCount, message) = await _cameraDetailDomain.GetAllCamera(requestParam);
                if (camera != null && camera.Any())
                {
                    return Ok(new AlprListResponse<List<CameraDetailView>>(HttpStatusCode.OK, camera, totalCount, message));
                }
                else
                {
                    return Ok(new AlprListResponse<List<CameraDetailView>>(HttpStatusCode.NotFound, camera, totalCount, message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetCameraStatuses()
        {
            try
            {
                var cameraStatuses = await _cameraDetailDomain.GetAllCameraStatusesAsync();

                if (cameraStatuses == null || !cameraStatuses.Any())
                {
                    return Ok(new AlprListResponse<object>(HttpStatusCode.NotFound, cameraStatuses, 0, "No camera status found."));
                }

                return Ok(new AlprListResponse<object>(HttpStatusCode.OK, cameraStatuses, cameraStatuses.Count, "Camera statuses retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("GetCameraByCameraId")]
        public async Task<IActionResult> GetCameraByCameraName([BindRequired, FromQuery] int cameraId)
        {
            if (cameraId == null && cameraId == 0)
            {
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraId."));
            }
            try
            {
                var (camera, message) = await _cameraDetailDomain.GetCameraByName(cameraId);
                if (camera == null)
                {
                    return NotFound(new AlprResponse<CameraDetailDto>(HttpStatusCode.NotFound, null, message));
                }
                return Ok(new AlprResponse<CameraDetailDto>(HttpStatusCode.OK, camera, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CameraDetailDto cameraDetailDto)
        {
            if (cameraDetailDto == null)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Camera data is required."));

            if (!ModelState.IsValid)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid camera data."));

            try
            {
                if (!string.IsNullOrEmpty(cameraDetailDto.CameraName))
                {
                    var camera = _zoneDbContext.CameraDetails.Where(a => a.CameraName.ToString().ToLower().Trim() == cameraDetailDto.CameraName.ToString().ToLower().Trim()).FirstOrDefault();
                    if (camera != null)
                    {
                        var isCameraNameConflict = !string.IsNullOrEmpty(cameraDetailDto.CameraName) && camera.CameraName.Trim().ToLower() == cameraDetailDto.CameraName.Trim().ToLower();
                        var conflicts = new List<string>();
                        if (isCameraNameConflict) conflicts.Add("CameraName");
                        var conflictPayload = new { Conflicts = conflicts };
                        var conflictMessage = $"{string.Join(" and ", conflicts)} already exists.";
                        return Ok(new AlprResponse<object>(HttpStatusCode.Found, conflictPayload, conflictMessage));
                    }
                }
                var (cameraResult, message) = await _cameraDetailDomain.CreateCamera(cameraDetailDto);

                if (cameraResult != null)
                {
                    return Ok(new AlprResponse<CameraDetailDto>(HttpStatusCode.OK, cameraResult, message));
                }

                return Ok(new AlprResponse<CameraDetailDto>(HttpStatusCode.NotFound, cameraResult, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }


        [Authorize]
        [HttpPatch("{cameraId}")]
        public async Task<IActionResult> Update(int cameraId, [FromBody] CameraDetailDto cameraDetailDto)
        {
            if (cameraDetailDto == null)
            {
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid data for update."));
            }

            try
            {
                // Check for duplicate CameraName
                if (!string.IsNullOrWhiteSpace(cameraDetailDto.CameraName))
                {
                    var existingCamera = await _zoneDbContext.CameraDetails
                        .Where(a => a.CameraId != cameraId && a.CameraName.Trim().ToLower() == cameraDetailDto.CameraName.Trim().ToLower())
                        .FirstOrDefaultAsync();

                    if (existingCamera != null)
                    {
                        var conflicts = new List<string> { "CameraName" };
                        var conflictPayload = new { Conflicts = conflicts };
                        var conflictMessage = $"{string.Join(" and ", conflicts)} already exists.";

                        return Ok(new AlprResponse<object>(HttpStatusCode.Found, conflictPayload, conflictMessage));
                    }
                }

                // Perform the update
                var (updatedCamera, message) = await _cameraDetailDomain.UpdateCamera(cameraId, cameraDetailDto);

                if (updatedCamera == null)
                {
                    return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));
                }

                return Ok(new AlprResponse<CameraDetailDto>(HttpStatusCode.OK, updatedCamera, message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, "Internal server error: " + ex.Message));
            }
        }

        [Authorize]
        [HttpDelete("{cameraId}")]
        public async Task<IActionResult> Delete(int cameraId)
        {
            if (cameraId == null || cameraId == 0)
                return BadRequest(new AlprResponse<string>(HttpStatusCode.BadRequest, null, "Invalid cameraId."));

            var (isDeleted, message) = await _cameraDetailDomain.DeleteCamera(cameraId);

            if (!isDeleted)
                return NotFound(new AlprResponse<string>(HttpStatusCode.NotFound, null, message));

            return Ok(new AlprResponse<string>(HttpStatusCode.OK, null, message));

        }


        [Authorize]
        [HttpGet("GetCameraFrame")]
        public async Task<IActionResult> GetSnapshot([BindRequired, FromQuery] string cameraName)
        {
            if (string.IsNullOrWhiteSpace(cameraName))
                return BadRequest("Camera Name is required.");

            var camera = _zoneDbContext.CameraDetails
                .FirstOrDefault(a => a.CameraName.ToLower().Trim() == cameraName.ToLower().Trim());

            if (camera == null)
                return NotFound("Camera not found.");

            var ffmpegPath = _config.GetSection("CameraConfig")["ffmpegPath"];
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                return StatusCode(500, "FFmpeg path not configured.");

            var tempFile = Path.GetTempFileName() + ".jpg";

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -rtsp_transport tcp -timeout 5000000 -i \"{camera.Url}\" -frames:v 1 -q:v 2 \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(psi);

                // Set max wait duration (e.g., 5 seconds)
                var timeout = TimeSpan.FromSeconds(5);
                var exited = await Task.WhenAny(
                    process.WaitForExitAsync(),
                    Task.Delay(timeout)
                ) == process.WaitForExitAsync();

                if (!System.IO.File.Exists(tempFile))
                {
                    var stderr = await process.StandardError.ReadToEndAsync();
                    return StatusCode(504, "Snapshot timeout. Camera may be disconnected.");
                }

                if (!System.IO.File.Exists(tempFile))
                    return StatusCode(500, "Failed to capture snapshot.");

                var image = await System.IO.File.ReadAllBytesAsync(tempFile);
                System.IO.File.Delete(tempFile);

                return File(image, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Snapshot capture error.");
            }
        }
    }
}
