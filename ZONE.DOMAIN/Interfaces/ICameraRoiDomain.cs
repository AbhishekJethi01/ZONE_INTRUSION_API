using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.EntityDto.Model;

namespace ZONE.DOMAIN.Interfaces
{
    public interface ICameraRoiDomain
    {
        Task<(List<CameraRoiDto>? cameraRois, string message)> GetByCamera(int? cameraId);
        Task<(List<CameraRoiDto>? created, string message)> CreateCameraRois(List<CameraRoiDto> cameraRoiDtos);
        Task<(List<CameraRoiDto>? updated, string message)> UpdateCameraRois(List<CameraRoiDto> cameraRoiDtos);
        Task<(bool isDeleted, string message)> DeleteCameraRois(List<int> cameraRoiIds);
    }
}
