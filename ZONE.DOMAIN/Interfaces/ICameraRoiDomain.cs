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
        Task<(CameraRoiDto? cameraRoi, string message)> GetByCamera(int? cameraId);
        Task<(CameraRoiDto? result, string message)> CreateCameraRoi(CameraRoiDto cameraRoiDto);
        Task<(CameraRoiDto? cameraRoiView, string message)> UpdateCameraRoi(int cameraId, CameraRoiDto cameraRoiDto);
        Task<(bool isDeleted, string message)> DeleteCameraRoi(int? cameraId);
    }
}
