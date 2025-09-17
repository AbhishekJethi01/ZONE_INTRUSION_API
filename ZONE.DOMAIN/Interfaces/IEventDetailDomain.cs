using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Model;
using ZONE.EntityDto.Model;

namespace ZONE.DOMAIN.Interfaces
{
    public interface IEventDetailDomain
    {
        Task<(List<object> result, string message)> GetSummaryForLastTenDaysAsync();
        Task<(int totalVehicle, int totalPerson, int totalAnimal, int totalOther, string message)> GetTodayObjectTypeCountsAsync();
        Task<(List<object> summaries, string message)> GetTodayEventSummaryByCameraAsync();
        Task<(List<object> result, string message)> GetMonthWiseEventSummaryAsync();
        Task<(List<CameraLatestEventsView> result, string message)> GetLatestEvents(int? personCameraId, int? vehicleCameraId, int? animalCameraId, int? otherCameraId);
        Task<(List<EventDetailView> events, int totalCount, string message)> GetAllEvents(RequestParam requestParam);

    }
}
