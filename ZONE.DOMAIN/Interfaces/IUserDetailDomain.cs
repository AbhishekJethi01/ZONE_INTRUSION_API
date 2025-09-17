using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.DOMAIN.Model;
using ZONE.EntityDto.Model;

namespace ZONE.DOMAIN.Interfaces
{
    public interface IUserDetailDomain
    {
        Task<(List<UserDetailView> users, int totalCount, string message)> GetAllUsers(RequestParam requestParam);
        Task<(UserDetailView? user, string message)> GetUserById(int userId);
        Task<(UserDetailView? result, string message)> CreateUser(UserDetailDto userDto);
        Task<(UserDetailView? updatedUser, string message)> UpdateUser(int userId, UserDetailDto userDto);
        Task<(UserLoginResponseModel? result, string message)> GetToken(string userName, string password);
        Task<(bool isSuccess, string message)> ChangePassword(ChangePasswordRequest request);
    }
}
