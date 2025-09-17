using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Interfaces;
using ZONE.Entity.Context;
using ZONE.Repository.Interfaces;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ZONE.EntityDto.Model;
using System.Text.RegularExpressions;
using ZONE.DOMAIN.Model;
using System.Globalization;
using System.Linq.Dynamic.Core;


namespace ZONE.DOMAIN.Services
{
    public class EventDetailDomain : IEventDetailDomain
    {
        private readonly IRepositoryService _repository;
        private readonly IConfiguration _config;
        private readonly string _conn = string.Empty;
        private ZoneDbContext _context;
        private readonly IMapper _mapper;

        public EventDetailDomain(IRepositoryService repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _conn = _config.GetConnectionString("DbConnection") ?? "";
            var contextOptions = new DbContextOptionsBuilder<ZoneDbContext>().UseSqlServer(_conn).Options;
            _context = new ZoneDbContext(contextOptions);
            _mapper = mapper;
        }

        public async Task<(List<object> result, string message)> GetSummaryForLastTenDaysAsync()
        {
            try
            {
                var data = await (from e in _context.EventDetails
                                  join ot in _context.ObjectTypes
                                      on e.ObjectType equals ot.Type
                                  where e.EventTime != null
                                  group e by new { e.EventTime.Date, ot.Type } into g
                                  orderby g.Key.Date descending
                                  select new
                                  {
                                      Date = g.Key.Date,
                                      Type = g.Key.Type,
                                      Count = g.Count()
                                  })
                                 .AsNoTracking()
                                 .ToListAsync();

                var summary = data
                    .GroupBy(d => d.Date)
                    .Take(10) // last 10 days
                    .Select(g => new
                    {
                        Date = g.Key.ToString("MM-dd-yyyy"),
                        TotalVehicle = g.Where(x => x.Type.ToLower() == "vehicle").Sum(x => x.Count),
                        TotalPerson = g.Where(x => x.Type.ToLower() == "person").Sum(x => x.Count),
                        TotalAnimal = g.Where(x => x.Type.ToLower() == "animal").Sum(x => x.Count),
                        TotalOther = g.Where(x => x.Type.ToLower() != "vehicle"
                                               && x.Type.ToLower() != "person"
                                               && x.Type.ToLower() != "animal")
                                      .Sum(x => x.Count)
                    })
                    .ToList<object>();

                return (summary, "Data retrieved successfully.");
            }
            catch (Exception ex)
            {
                return (new List<object>(), $"Error: {ex.Message}");
            }
        }


        public async Task<(int totalVehicle, int totalPerson, int totalAnimal, int totalOther, string message)> GetTodayObjectTypeCountsAsync()
        {
            try
            {
                var today = DateTime.Today;

                var todayEvents = await _repository.EventDetail.FindAll(_context)
                    .Where(e => e.EventTime != null && e.EventTime.Date == today).AsNoTracking()
                    .ToListAsync();

                var totalVehicle = todayEvents.Count(e => (e.ObjectType ?? "").ToLower() == "vehicle");
                var totalPerson = todayEvents.Count(e => (e.ObjectType ?? "").ToLower() == "person");
                var totalAnimal = todayEvents.Count(e => (e.ObjectType ?? "").ToLower() == "animal");

                var totalOther = todayEvents.Count(e =>
                {
                    var objType = (e.ObjectType ?? "").ToLower();
                    return objType != "vehicle" && objType != "person" && objType != "animal";
                });

                return (totalVehicle, totalPerson, totalAnimal, totalOther, "Object type counts for today retrieved.");
            }
            catch (Exception ex)
            {
                return (0, 0, 0, 0, $"Error retrieving today's object type counts: {ex.Message}");
            }
        }

        public async Task<(List<object> summaries, string message)> GetTodayEventSummaryByCameraAsync()
        {
            try
            {
                var today = DateTime.Today;

                // Get all cameras (we'll left join events later)
                var cameras = await _context.CameraDetails
                    .Select(c => new { c.CameraId, c.CameraName })
                    .AsNoTracking()
                    .ToListAsync();

                // Group today's events by CameraId + Type (avoid duplicates from ObjectTypes table)
                var todayEvents = await (from e in _context.EventDetails
                                         join ot in _context.ObjectTypes
                                             on e.ObjectType equals ot.Type
                                         where e.EventTime != null && e.EventTime.Date == today
                                         group e by new { e.CameraId, ot.Type } into g
                                         select new
                                         {
                                             g.Key.CameraId,
                                             Type = g.Key.Type,
                                             Count = g.Count()
                                         })
                                        .AsNoTracking()
                                        .ToListAsync();

                var result = new List<object>();

                foreach (var camera in cameras)
                {
                    var matchingEvents = todayEvents.Where(e => e.CameraId == camera.CameraId);

                    var cameraSummary = new
                    {
                        CameraName = camera.CameraName,
                        TotalVehicle = matchingEvents.Where(x => x.Type.ToLower() == "vehicle").Sum(x => x.Count),
                        TotalPerson = matchingEvents.Where(x => x.Type.ToLower() == "person").Sum(x => x.Count),
                        TotalAnimal = matchingEvents.Where(x => x.Type.ToLower() == "animal").Sum(x => x.Count),
                        TotalOther = matchingEvents
                                       .Where(x => x.Type.ToLower() != "vehicle"
                                                && x.Type.ToLower() != "person"
                                                && x.Type.ToLower() != "animal")
                                       .Sum(x => x.Count)
                    };

                    result.Add(cameraSummary);
                }

                return (result, "Camera-wise event summary retrieved.");
            }
            catch (Exception ex)
            {
                return (new List<object>(), $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(List<object> result, string message)> GetMonthWiseEventSummaryAsync()
        {
            try
            {
                var monthlyData = await (from e in _context.EventDetails
                                         join ot in _context.ObjectTypes
                                             on e.ObjectType equals ot.Type
                                         where e.EventTime != null
                                         group e by new
                                         {
                                             Year = e.EventTime.Year,
                                             Month = e.EventTime.Month,
                                             ot.Type
                                         } into g
                                         select new
                                         {
                                             Year = g.Key.Year,
                                             Month = g.Key.Month,
                                             Type = g.Key.Type,
                                             Count = g.Count()
                                         })
                                        .AsNoTracking()
                                        .ToListAsync();

                var monthlySummary = monthlyData
                    .GroupBy(d => new { d.Year, d.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat
                                      .GetMonthName(g.Key.Month),
                        TotalVehicle = g.Where(x => x.Type.ToLower() == "vehicle").Sum(x => x.Count),
                        TotalPerson = g.Where(x => x.Type.ToLower() == "person").Sum(x => x.Count),
                        TotalAnimal = g.Where(x => x.Type.ToLower() == "animal").Sum(x => x.Count),
                        TotalOther = g.Where(x => x.Type.ToLower() != "vehicle"
                                                && x.Type.ToLower() != "person"
                                                && x.Type.ToLower() != "animal")
                                      .Sum(x => x.Count)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList<object>();

                return (monthlySummary, "Month-wise event summary retrieved.");
            }
            catch (Exception ex)
            {
                return (new List<object>(), $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(List<CameraLatestEventsView> result, string message)> GetLatestEvents(int? personCameraId, int? vehicleCameraId, int? animalCameraId, int? otherCameraId)
        {
            try
            {
                // ✅ Fetch all events (joined with Camera + ObjectType) in one query
                var allEvents = await (
                    from e in _context.EventDetails.AsNoTracking()
                    join c in _context.CameraDetails.AsNoTracking()
                        on e.CameraId equals c.CameraId
                    join ot in _context.ObjectTypes.AsNoTracking()
                        on e.ObjectType equals ot.Type
                    orderby e.EventTime descending
                    select new EventDetailDto
                    {
                        EventId = e.EventId,
                        Img = e.Img,
                        EventTime = e.EventTime,
                        ObjectType = e.ObjectType,
                        CameraId = e.CameraId,
                        CameraName = c.CameraName,
                        Link = e.Link
                    }
                ).ToListAsync();

                // ✅ Build response grouped by object type
                var result = new List<CameraLatestEventsView>();

                result.Add(new CameraLatestEventsView
                {
                    CameraId = personCameraId?.ToString() ?? string.Empty,
                    Object = "person",
                    CameraName = personCameraId.HasValue
                        ? allEvents.FirstOrDefault(x => x.CameraId == personCameraId.Value)?.CameraName ?? "All"
                        : "All",
                    Events = allEvents
                        .Where(x => x.ObjectType.ToLower() == "person" &&
                                   (!personCameraId.HasValue || x.CameraId == personCameraId.Value))
                        .Take(20)
                        .ToList()
                });

                result.Add(new CameraLatestEventsView
                {
                    CameraId = vehicleCameraId?.ToString() ?? string.Empty,
                    Object = "vehicle",
                    CameraName = vehicleCameraId.HasValue
                        ? allEvents.FirstOrDefault(x => x.CameraId == vehicleCameraId.Value)?.CameraName ?? "All"
                        : "All",
                    Events = allEvents
                        .Where(x => x.ObjectType.ToLower() == "vehicle" &&
                                   (!vehicleCameraId.HasValue || x.CameraId == vehicleCameraId.Value))
                        .Take(20)
                        .ToList()
                });

                result.Add(new CameraLatestEventsView
                {
                    CameraId = animalCameraId?.ToString() ?? string.Empty,
                    Object = "animal",
                    CameraName = animalCameraId.HasValue
                        ? allEvents.FirstOrDefault(x => x.CameraId == animalCameraId.Value)?.CameraName ?? "All"
                        : "All",
                    Events = allEvents
                        .Where(x => x.ObjectType.ToLower() == "animal" &&
                                   (!animalCameraId.HasValue || x.CameraId == animalCameraId.Value))
                        .Take(20)
                        .ToList()
                });

                result.Add(new CameraLatestEventsView
                {
                    CameraId = otherCameraId?.ToString() ?? string.Empty,
                    Object = "other",
                    CameraName = otherCameraId.HasValue
                        ? allEvents.FirstOrDefault(x => x.CameraId == otherCameraId.Value)?.CameraName ?? "All"
                        : "All",
                    Events = allEvents
                        .Where(x => x.ObjectType.ToLower() == "other" &&
                                   (!otherCameraId.HasValue || x.CameraId == otherCameraId.Value))
                        .Take(20)
                        .ToList()
                });

                return (result, "Events retrieved successfully.");
            }
            catch (Exception ex)
            {
                return (new List<CameraLatestEventsView>(), $"Error retrieving Events: {ex.Message}");
            }
        }



        public async Task<(List<EventDetailView> events, int totalCount, string message)> GetAllEvents(RequestParam requestParam)
        {
            try
            {
                // Start with EventDetails
                var query = _repository.EventDetail.FindAll(_context).AsNoTracking();

                // 🔍 Column Search
                if (!string.IsNullOrEmpty(requestParam.ColumnSearch))
                {
                    var searchData = requestParam.ColumnSearch.Trim().ToLower();
                    DateTime? fullDateTime = null;
                    DateTime? searchDate = null;
                    TimeSpan? searchTime = null;

                    // Case 1: Full datetime
                    if (Regex.IsMatch(searchData, @"\d{1,2}[-/]\d{1,2}[-/]\d{4}\s+\d{1,2}:\d{2}\s?(am|pm)", RegexOptions.IgnoreCase))
                    {
                        fullDateTime = TryParseDateTimeFromSearch(searchData);
                    }
                    // Case 2: Only date
                    else if (Regex.IsMatch(searchData, @"\d{1,2}[-/]\d{1,2}[-/]\d{4}", RegexOptions.IgnoreCase))
                    {
                        searchDate = TryParseDateOnlyFromSearch(searchData);
                    }
                    // Case 3: Only time
                    else if (Regex.IsMatch(searchData, @"\d{1,2}:\d{2}\s?(am|pm)", RegexOptions.IgnoreCase))
                    {
                        searchTime = TryParseTimeFromSearch(searchData);
                    }

                    query = query.Where(t =>
                        (t.ObjectType ?? "").ToLower().Contains(searchData) ||
                        (t.CameraName ?? "").ToLower().Contains(searchData) ||

                        (fullDateTime.HasValue &&
                         t.EventTime != null &&
                         t.EventTime >= fullDateTime.Value &&
                         t.EventTime < fullDateTime.Value.AddMinutes(1)) ||

                        (searchDate.HasValue &&
                         t.EventTime != null &&
                         t.EventTime.Date == searchDate.Value.Date) ||

                        (searchTime.HasValue &&
                         t.EventTime != null &&
                         t.EventTime.TimeOfDay >= searchTime.Value &&
                         t.EventTime.TimeOfDay < searchTime.Value.Add(TimeSpan.FromMinutes(1)))
                    );
                }

                // Dynamic Params (date range filter)
                if (requestParam.DynamicParams != null && requestParam.DynamicParams.Count > 0)
                {
                    DateTime startDateTime = default;
                    DateTime endDateTime = default;

                    bool hasStart = requestParam.DynamicParams.TryGetValue("startDateTime", out var startObj) &&
                                    DateTime.TryParse(startObj?.ToString(), out startDateTime);

                    bool hasEnd = requestParam.DynamicParams.TryGetValue("endDateTime", out var endObj) &&
                                  DateTime.TryParse(endObj?.ToString(), out endDateTime);

                    if (hasStart && hasEnd)
                    {
                        startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day,
                                                     startDateTime.Hour, startDateTime.Minute, 0);
                        endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day,
                                                   endDateTime.Hour, endDateTime.Minute, 0).AddMinutes(1);

                        query = query.Where(t =>
                            t.EventTime != null &&
                            t.EventTime >= startDateTime &&
                            t.EventTime <= endDateTime);
                    }
                    else if (hasStart)
                    {
                        startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day,
                                                     startDateTime.Hour, startDateTime.Minute, 0);

                        query = query.Where(t =>
                            t.EventTime != null &&
                            t.EventTime >= startDateTime);
                    }
                    else if (hasEnd)
                    {
                        endDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day,
                                                   endDateTime.Hour, endDateTime.Minute, 0).AddMinutes(1);

                        query = query.Where(t =>
                            t.EventTime != null &&
                            t.EventTime <= endDateTime);
                    }
                }

                // Join with master tables (only matched records)
                var joinedQuery =
                    from eventDetail in query
                    join camera in _context.CameraDetails.AsNoTracking()
                        on eventDetail.CameraId equals camera.CameraId
                    join objectType in _context.ObjectTypes.AsNoTracking()
                        on eventDetail.ObjectType equals objectType.Type
                    select new EventDetailView
                    {
                        EventId = eventDetail.EventId,
                        Img = eventDetail.Img,
                        EventTime = eventDetail.EventTime,
                        ObjectType = objectType.Type,   // from master table
                        CameraId = eventDetail.CameraId,
                        CameraName = camera.CameraName,     // from master table
                        Link = eventDetail.Link,
                    };

                // Sorting
                if (!string.IsNullOrEmpty(requestParam.SortBy))
                {
                    var sortOrder = requestParam.SortOrder?.ToLower() == "desc" ? "descending" : "ascending";
                    joinedQuery = joinedQuery.OrderBy($"{requestParam.SortBy} {sortOrder}");
                }

                // Pagination with correct total count
                var totalCount = await joinedQuery.CountAsync();
                if (requestParam.Length > 0)
                {
                    joinedQuery = joinedQuery.Skip((int)requestParam.Start).Take((int)requestParam.Length);
                }
                else
                {
                    joinedQuery = joinedQuery.Skip((int)requestParam.Start);
                }

                // Final fetch
                var events = await joinedQuery.AsNoTracking().ToListAsync();
                //var result = _mapper.Map<List<EventDetailView>>(events);
                var result = events;

                string message = result.Any() ? "Events retrieved successfully." : "No events found.";
                return (result, totalCount, message);
            }
            catch (Exception ex)
            {
                return (null, 0, $"Error retrieving events: {ex.Message}");
            }
        }


        private static DateTime? TryParseDateTimeFromSearch(string input)
        {
            string[] formats = {
                "dd-MM-yyyy h:mm tt", "dd/MM/yyyy h:mm tt",
                "yyyy-MM-dd h:mm tt", "d-M-yyyy h:mm tt",
                "d/M/yyyy h:mm tt", "M/d/yyyy h:mm tt"
            };

            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed;

            return null;
        }

        private static DateTime? TryParseDateOnlyFromSearch(string input)
        {
            string[] formats = {
                "dd-MM-yyyy", "dd/MM/yyyy",
                "yyyy-MM-dd", "d-M-yyyy", "d/M/yyyy"
            };

            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.Date;

            return null;
        }

        private static TimeSpan? TryParseTimeFromSearch(string input)
        {
            string[] formats = { "h:mm tt", "hh:mm tt" };

            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.TimeOfDay;

            return null;
        }

    }
}
