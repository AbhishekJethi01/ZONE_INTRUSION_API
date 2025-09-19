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
        public async Task<(List<CameraRoiDto>? cameraRois, string message)> GetByCamera(int? cameraId)
        {
            try
            {
                var cameraEntities = await _repository.CameraRoi.FindAsync(_context, u => u.CameraId == cameraId);

                if (cameraEntities == null || !cameraEntities.Any())
                    return (null, "No ROIs found for the camera.");

                var cameraDtos = _mapper.Map<List<CameraRoiDto>>(cameraEntities);
                return (cameraDtos, "Camera ROIs retrieved successfully.");
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving camera ROIs: {ex.Message}");
            }
        }

        public async Task<(List<CameraRoiDto>? created, string message)> CreateCameraRois(List<CameraRoiDto> cameraRoiDtos)
        {
            try
            {
                var entities = _mapper.Map<List<CameraRoi>>(cameraRoiDtos);
                await _repository.CameraRoi.CreateMultipleAsync(_context, entities);
                var result = await _repository.CameraRoi.SaveEntityAsync(_context);

                if (result > 0)
                {
                    var dtos = _mapper.Map<List<CameraRoiDto>>(entities);
                    return (dtos, "Camera Rois saved successfully.");
                }

                return (null, "Failed to save camera rois.");
            }
            catch (Exception ex)
            {
                return (null, $"Error saving camera rois: {ex.Message}");
            }
        }

        public async Task<(List<CameraRoiDto>? updated, string message)> UpdateCameraRois(List<CameraRoiDto> cameraRoiDtos)
        {
            try
            {
                var ids = cameraRoiDtos.Select(x => x.Id).ToList();
                var existingEntities = await _context.CameraRois.Where(x => ids.Contains(x.Id)).ToListAsync();

                if (!existingEntities.Any())
                    return (null, "No camera rois found to update.");

                foreach (var entity in existingEntities)
                {
                    var dto = cameraRoiDtos.First(x => x.Id == entity.Id);
                    entity.PatchEntity(dto); // Update entity properties
                }

                await _context.SaveChangesAsync();

                var updatedDtos = _mapper.Map<List<CameraRoiDto>>(existingEntities);
                return (updatedDtos, "Camera Rois updated successfully.");
            }
            catch (Exception ex)
            {
                return (null, $"Error updating camera rois: {ex.Message}");
            }
        }


        public async Task<(bool isDeleted, string message)> DeleteCameraRois(List<int> cameraRoiIds)
        {
            try
            {
                if (cameraRoiIds == null || !cameraRoiIds.Any())
                    return (false, "No camera roi IDs provided for deletion.");

                // Fetch all matching entities directly from context
                var entities = await _context.CameraRois
                    .Where(x => cameraRoiIds.Contains(x.Id))
                    .ToListAsync();

                if (!entities.Any())
                    return (false, "No camera rois found to delete.");

                // Remove in bulk
                _context.CameraRois.RemoveRange(entities);

                var result = await _context.SaveChangesAsync();

                return result > 0
                    ? (true, "Camera rois deleted successfully.")
                    : (false, "Failed to delete camera rois.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting camera rois: {ex.Message}");
            }
        }

    }
}
