using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Model;
using ZONE.EntityDto.Model;

namespace ZONE.DOMAIN.Interfaces
{
    public interface ICameraDetailDomain
    {
        Task<(List<object> result, string message)> GetCameraDropdownList();
        Task<(List<CameraDetailView> camera, int totalCount, string message)> GetAllCamera(RequestParam requestParam);
        Task<List<CameraStatusView>> GetAllCameraStatusesAsync();
        Task<(CameraDetailDto? camera, string message)> GetCameraByName(int? cameraId);
        Task<(CameraDetailDto? result, string message)> CreateCamera(CameraDetailDto cameraDetailDto);
        Task<(CameraDetailDto? cameraDetailView, string message)> UpdateCamera(int cameraRoiId, CameraDetailDto cameraDetailDto);
        Task<(bool isDeleted, string message)> DeleteCamera(int? cameraRoiId);
    }
}
