using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Interfaces;
using ZONE.DOMAIN.Model;
using ZONE.Entity.Context;
using ZONE.EntityDto.Model;
using ZONE.Repository.Interfaces;
using System.Linq.Dynamic.Core;
using System.Diagnostics;
using ZONE.Entity.Model;
using ZONE.DOMAIN.Extensions;

namespace ZONE.DOMAIN.Services
{
    public class CameraDetailDomain : ICameraDetailDomain
    {
        private readonly IRepositoryService _repository;
        private readonly IConfiguration _config;
        private readonly string _conn = string.Empty;
        private ZoneDbContext _context;
        private readonly IMapper _mapper;

        public CameraDetailDomain(IRepositoryService repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _conn = _config.GetConnectionString("DbConnection") ?? "";
            var contextOptions = new DbContextOptionsBuilder<ZoneDbContext>().UseSqlServer(_conn).Options;
            _context = new ZoneDbContext(contextOptions);
            _mapper = mapper;
        }

        public async Task<(List<object> result, string message)> GetCameraDropdownList()
        {
            try
            {
                var cameras = await _context.CameraDetails
                    .AsNoTracking()
                    .ToListAsync(); // fetch to memory

                var result = cameras
                    .DistinctBy(c => c.CameraName.Trim().ToLower())
                    .Select(v => new
                    {
                        v.CameraId,
                        v.CameraName,
                        v.Url
                    })
                    .OrderBy(c => c.CameraName)
                    .Cast<object>()
                    .ToList();

                var message = result.Any()
                    ? "Camera name retrieved successfully."
                    : "No Camera name found.";

                return (result, message);
            }
            catch (Exception ex)
            {
                return (new List<object>(), $"Error retrieving camera name: {ex.Message}");
            }
        }

        public async Task<(List<CameraDetailView> camera, int totalCount, string message)> GetAllCamera(RequestParam requestParam)
        {
            try
            {
                var query = from camera in _repository.CameraDetail.FindAll(_context).AsNoTracking()
                            select new CameraDetailView
                            {
                                CameraId = camera.CameraId,
                                CameraName = camera.CameraName,
                                Url = camera.Url,
                                CameraIpAddress = camera.CameraIpAddress,
                            };

                // 🔍 Search
                if (!string.IsNullOrEmpty(requestParam.ColumnSearch))
                {
                    var searchData = requestParam.ColumnSearch.Trim().ToLower();
                    query = query.Where(t =>
                        (t.CameraName ?? "").ToLower().Contains(searchData) ||
                        (t.CameraIpAddress ?? "").ToLower().Contains(searchData) ||
                        (t.Url ?? "").ToLower().Contains(searchData));
                }

                // ↕️ Sort
                if (!string.IsNullOrEmpty(requestParam.SortBy))
                {
                    var sortOrder = requestParam.SortOrder?.ToLower() == "desc" ? "descending" : "ascending";
                    query = query.OrderBy($"{requestParam.SortBy} {sortOrder}");
                }

                var totalCount = await query.CountAsync();
                var data = await query.Skip((int)requestParam.Start).Take((int)requestParam.Length).ToListAsync();

                string message = data.Any() ? "Camera retrieved successfully." : "No camera found.";
                return (data, totalCount, message);

            }
            catch (Exception ex)
            {
                return (null, 0, $"Error retrieving cameras: {ex.Message}");
            }
        }


        public async Task<List<CameraStatusView>> GetAllCameraStatusesAsync()
        {
            var query = _repository.CameraDetail.FindAll(_context);
            var cameras = await query.ToListAsync();
            var mapped = _mapper.Map<List<CameraDetailView>>(cameras);

            var tasks = mapped.Select(async cam => new CameraStatusView
            {
                CameraId = cam.CameraId,
                CameraName = cam.CameraName,
                CameraStatus = await CheckRtspWithFfprobeAsync(cam.Url)
            });

            return (await Task.WhenAll(tasks)).ToList();
        }


        private static readonly SemaphoreSlim _semaphore = new(5); // Limit concurrency

        private async Task<string> CheckRtspWithFfprobeAsync(string rtspUrl)
        {
            await _semaphore.WaitAsync();
            Process? process = null;

            try
            {
                var ffprobePath = _config["CameraConfig:ffprobePath"];
                if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
                    return "Disconnected";

                var psi = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v error -show_entries stream=codec_name -of default=noprint_wrappers=1 \"{rtspUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process = new Process { StartInfo = psi };

                if (!process.Start())
                    return "Disconnected";

                var timeoutTask = Task.Delay(20000);
                var exitTask = Task.Run(() => process.WaitForExit());

                var completed = await Task.WhenAny(exitTask, timeoutTask);

                if (completed != exitTask)
                {
                    try { process.Kill(true); } catch { /* ignore */ }
                    return "Disconnected";
                }

                return process.ExitCode == 0 ? "Connected" : "Disconnected";
            }
            catch (Exception ex)
            {
                // You can log ex.ToString() here for debugging
                return "Disconnected";
            }
            finally
            {
                try { process?.Dispose(); } catch { }
                _semaphore.Release();
            }
        }

        public async Task<(CameraDetailDto? camera, string message)> GetCameraByName(int? cameraId)
        {
            try
            {
                var cameraEntity = await _repository.CameraDetail.FindAsync(_context, u => u.CameraId == cameraId);
                var camera = cameraEntity?.FirstOrDefault();
                if (camera == null)
                    return (null, "Camera not found.");

                var cameraDto = _mapper.Map<CameraDetailDto>(camera);
                return (cameraDto, "Camera retrieved successfully.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera: {ex.Message}");
            }
        }

        public async Task<(CameraDetailDto? result, string message)> CreateCamera(CameraDetailDto cameraDetailDto)
        {
            try
            {
                var newCamera = _mapper.Map<CameraDetail>(cameraDetailDto);
                newCamera.RoistartPercentageHeight = 10;
                newCamera.RoiendPercentageHeight = 90;
                newCamera.RoistartPercentageWidth = 10;
                newCamera.RoiendPercentageWidth = 90;
                await _repository.CameraDetail.CreateAsync(_context, newCamera);
                var saveResult = await _repository.CameraDetail.SaveEntityAsync(_context);

                if (saveResult > 0)
                {
                    var dto = _mapper.Map<CameraDetailDto>(newCamera);
                    return (dto, "Camera saved successfully.");
                }

                return (null, "Unable to save Camera.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera: {ex.Message}");
            }
        }

        public async Task<(CameraDetailDto? cameraDetailView, string message)> UpdateCamera(int cameraId, CameraDetailDto cameraDetailDto)
        {
            try
            {
                var cameraEntity = await _repository.CameraDetail.FindAsync(_context, u => u.CameraId == cameraId);
                var camera = cameraEntity?.FirstOrDefault();
                if (camera == null)
                    return (null, "Camera not found.");

                camera.PatchEntity(cameraDetailDto);

                _repository.CameraDetail.Update(_context, camera);
                var result = await _repository.CameraDetail.SaveEntityAsync(_context);
                if (result > 0)
                {
                    var dto = _mapper.Map<CameraDetailDto>(camera);
                    return (dto, "Camera updated successfully.");
                }
                return (null, "Unable to update camera.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera: {ex.Message}");
            }
        }

        public async Task<(bool isDeleted, string message)> DeleteCamera(int? cameraId)
        {
            try
            {
                var cameraEntity = await _repository.CameraDetail.FindAsync(_context, u => u.CameraId == cameraId);
                var camera = cameraEntity?.FirstOrDefault();

                if (camera == null)
                    return (false, "Camera not found.");

                _repository.CameraDetail.Delete(_context, camera);
                var result = await _repository.CameraDetail.SaveEntityAsync(_context);

                return result > 0
                    ? (true, "Camera deleted successfully.")
                    : (false, "Failed to delete camera.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting camera: {ex.Message}");
            }
        }
    }
}
