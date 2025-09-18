using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Extensions;
using ZONE.DOMAIN.Interfaces;
using ZONE.Entity.Context;
using ZONE.Entity.Model;
using ZONE.EntityDto.Model;
using ZONE.Repository.Interfaces;

namespace ZONE.DOMAIN.Services
{
    public class CameraRoiDomain : ICameraRoiDomain
    {
        private readonly IRepositoryService _repository;
        private readonly IConfiguration _config;
        private readonly string _conn = string.Empty;
        private ZoneDbContext _context;
        private readonly IMapper _mapper;

        public CameraRoiDomain(IRepositoryService repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _conn = _config.GetConnectionString("DbConnection") ?? "";
            var contextOptions = new DbContextOptionsBuilder<ZoneDbContext>().UseSqlServer(_conn).Options;
            _context = new ZoneDbContext(contextOptions);
            _mapper = mapper;
        }
        public async Task<(CameraRoiDto? cameraRoi, string message)> GetByCamera(int? cameraId)
        {
            try
            {
                var cameraEntity = await _repository.CameraRoi.FindAsync(_context, u => u.CameraId == cameraId);
                var camera = cameraEntity?.FirstOrDefault();
                if (camera == null)
                    return (null, "Camera not found.");

                var cameraDto = _mapper.Map<CameraRoiDto>(camera);
                return (cameraDto, "Camera retrieved successfully.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera: {ex.Message}");
            }
        }

        public async Task<(CameraRoiDto? result, string message)> CreateCameraRoi(CameraRoiDto cameraRoiDto)
        {
            try
            {
                var newCameraRoi = _mapper.Map<CameraRoi>(cameraRoiDto);
                //newCameraRoi.RoiStartPercentageWidth = 10;
                //newCameraRoi.RoiEndPercentageHeight = 90;
                //newCameraRoi.RoiStartPercentageWidth = 10;
                //newCameraRoi.RoiEndPercentageWidth = 90;
                await _repository.CameraRoi.CreateAsync(_context, newCameraRoi);
                var saveResult = await _repository.CameraRoi.SaveEntityAsync(_context);

                if (saveResult > 0)
                {
                    var dto = _mapper.Map<CameraRoiDto>(newCameraRoi);
                    return (dto, "Camera Roi saved successfully.");
                }

                return (null, "Unable to save Camera Roi.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera roi: {ex.Message}");
            }
        }

        public async Task<(CameraRoiDto? cameraRoiView, string message)> UpdateCameraRoi(int cameraRoiId, CameraRoiDto cameraRoiDto)
        {
            try
            {
                var cameraEntity = await _repository.CameraRoi.FindAsync(_context, u => u.Id == cameraRoiId);
                var camera = cameraEntity?.FirstOrDefault();
                if (camera == null)
                    return (null, "Camera Roi not found.");

                camera.PatchEntity(cameraRoiDto);

                _repository.CameraRoi.Update(_context, camera);
                var result = await _repository.CameraRoi.SaveEntityAsync(_context);
                if (result > 0)
                {
                    var dto = _mapper.Map<CameraRoiDto>(camera);
                    return (dto, "Camera Roi updated successfully.");
                }
                return (null, "Unable to update camera roi.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera roi: {ex.Message}");
            }
        }

        public async Task<(bool isDeleted, string message)> DeleteCameraRoi(int? cameraRoiId)
        {
            try
            {
                var cameraEntity = await _repository.CameraRoi.FindAsync(_context, u => u.Id == cameraRoiId);
                var camera = cameraEntity?.FirstOrDefault();

                if (camera == null)
                    return (false, "Camera roi not found.");

                _repository.CameraRoi.Delete(_context, camera);
                var result = await _repository.CameraRoi.SaveEntityAsync(_context);

                return result > 0
                    ? (true, "Camera roi deleted successfully.")
                    : (false, "Failed to delete camera roi.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting camera roi: {ex.Message}");
            }
        }
    }
}
