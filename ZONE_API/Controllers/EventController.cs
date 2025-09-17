using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;
using ZONE.Entity.Context;
using ZONE.EntityDto.Model;

namespace ZONE_API.Controllers
{
    [Route("api/Zone/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventDetailDomain _eventDetailDomain;
        private readonly ZoneDbContext _zoneDbContext;

        public EventController(IEventDetailDomain eventDetailDomain, ZoneDbContext zoneDbContext)
        {
            _eventDetailDomain = eventDetailDomain;
            _zoneDbContext = zoneDbContext;
        }

        [Authorize]
        [HttpGet("GetLastTenDaySummary")]
        public async Task<IActionResult> GetLastTenDaySummary()
        {
            try
            {
                var (result, message) = await _eventDetailDomain.GetSummaryForLastTenDaysAsync();
                return Ok(new AlprResponse<List<object>>(HttpStatusCode.OK, result, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("GetTodayDashboardSummary")]
        public async Task<IActionResult> GetTodayDashboardSummary()
        {
            try
            {
                var (totalVehicle, totalPerson, totalAnimal, totalOther, message1) = await _eventDetailDomain.GetTodayObjectTypeCountsAsync();
                var (cameraSummaries, message2) = await _eventDetailDomain.GetTodayEventSummaryByCameraAsync();
                int grandTotal = totalVehicle + totalPerson + totalAnimal + totalOther;

                double vehiclePct = 0, personPct = 0, animalPct = 0, otherPct = 0;

                if (grandTotal > 0)
                {
                    vehiclePct = Math.Round((double)totalVehicle / grandTotal * 100, 2);
                    personPct = Math.Round((double)totalPerson / grandTotal * 100, 2);
                    animalPct = Math.Round((double)totalAnimal / grandTotal * 100, 2);
                    otherPct = Math.Round((double)totalOther / grandTotal * 100, 2);

                    // Ensure total = 100 (fix rounding drift)
                    double sum = vehiclePct + personPct + animalPct + otherPct;
                    double diff = 100 - sum;

                    // Adjust last category
                    otherPct += diff;
                }
                var response = new
                {
                    Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    TotalVehicle = totalVehicle,
                    TotalPerson = totalPerson,
                    TotalAnimal = totalAnimal,
                    TotalOther = totalOther,
                    VehiclePercentage = vehiclePct.ToString("0.00"),
                    PersonPercentage = personPct.ToString("0.00"),
                    AnimalPercentage = animalPct.ToString("0.00"),
                    OtherPercentage = otherPct.ToString("0.00"),
                    CameraSummaries = cameraSummaries
                };

                return Ok(new AlprResponse<object>(HttpStatusCode.OK, response, $"{message1} {message2}"));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("GetMonthWiseEventSummary")]
        public async Task<IActionResult> GetMonthWiseEventSummary()
        {
            try
            {
                var (result, message) = await _eventDetailDomain.GetMonthWiseEventSummaryAsync();
                return Ok(new AlprResponse<List<object>>(HttpStatusCode.OK, result, message));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("GetLatestEvents")]
        public async Task<IActionResult> GetLatestEvents([FromQuery] int? personCameraId = null, [FromQuery] int? vehicleCameraId = null, [FromQuery] int? animalCameraId = null, [FromQuery] int? otherCameraId = null)
        {
            try
            {
                var (result, message) = await _eventDetailDomain.GetLatestEvents(personCameraId,vehicleCameraId,animalCameraId,otherCameraId);
                if(result != null && result.Any())
                {
                    return Ok(new AlprResponse<List<CameraLatestEventsView>>(HttpStatusCode.OK, result, message));
                }
                else
                {
                    return Ok(new AlprResponse<List<CameraLatestEventsView>>(HttpStatusCode.NotFound, result, message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPost("Get")]
        public async Task<IActionResult> Get(RequestParam requestParam)
        {
            try
            {
                var (events, totalCount, message) = await _eventDetailDomain.GetAllEvents(requestParam);

                if (events != null && events.Any())
                {
                    return Ok(new AlprListResponse<List<EventDetailView>>(HttpStatusCode.OK, events, totalCount, message));
                }
                else
                {
                    return Ok(new AlprListResponse<List<EventDetailView>>(HttpStatusCode.NotFound, events, totalCount, message));
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new AlprResponse<string>(HttpStatusCode.InternalServerError, null, $"An error occurred: {ex.Message}"));
            }
        }
    }
}
