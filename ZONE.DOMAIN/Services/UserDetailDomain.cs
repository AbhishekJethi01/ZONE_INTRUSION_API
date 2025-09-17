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
using ZONE.Repository.Interfaces;
using System.Linq.Dynamic.Core;
using ZONE.EntityDto.Model;
using ZONE.DOMAIN.Extensions;
using ZONE.Entity.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ZONE.DOMAIN.Services
{
    public class UserDetailDomain: IUserDetailDomain
    {
        private readonly IRepositoryService _repository;
        private readonly IConfiguration _config;
        private readonly string _conn = string.Empty;
        private ZoneDbContext _context;
        private readonly IMapper _mapper;

        public UserDetailDomain(IRepositoryService repository, IConfiguration config, IMapper mapper)
        {
            _repository = repository;
            _config = config;
            _conn = _config.GetConnectionString("DbConnection") ?? "";
            var contextOptions = new DbContextOptionsBuilder<ZoneDbContext>().UseSqlServer(_conn).Options;
            _context = new ZoneDbContext(contextOptions);
            _mapper = mapper;
        }

        public async Task<(List<UserDetailView> users, int totalCount, string message)> GetAllUsers(RequestParam requestParam)
        {
            var query = _repository.UserDetail.FindAll(_context);

            if (!string.IsNullOrEmpty(requestParam.ColumnSearch))
            {
                var searchData = requestParam.ColumnSearch.Trim().ToLower();
                query = query.Where(t =>
                    t.UserName.ToLower().Trim().Contains(searchData)
                    || t.EmailId.ToLower().Contains(searchData)
                    || t.PhoneNumber.ToLower().Contains(searchData));
            }

            if (!string.IsNullOrEmpty(requestParam.SortBy))
            {   
                var sortOrder = requestParam.SortOrder?.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{requestParam.SortBy} {sortOrder}");
            }

            var totalCount = await query.CountAsync();
            query = query.Skip((int)requestParam.Start).Take((int)requestParam.Length);

            var users = await query.ToListAsync();
            var result = _mapper.Map<List<UserDetailView>>(users);
            string message = result.Any() ? "Users retrieved successfully" : "No users found";
            return (result, totalCount, message);
        }

        public async Task<(UserDetailView? user, string message)> GetUserById(int userId)
        {
            var userEntity = await _repository.UserDetail.FindAsync(_context, u => u.UserId == userId);
            var user = userEntity?.FirstOrDefault();

            if (user == null)
                return (null, "User not found.");

            var userDto = _mapper.Map<UserDetailView>(user);
            return (userDto, "User retrieved successfully.");
        }

        public async Task<(UserDetailView? result, string message)> CreateUser(UserDetailDto userDto)
        {
            var newUser = _mapper.Map<UserDetail>(userDto);
            await _repository.UserDetail.CreateAsync(_context, newUser);
            var saveResult = await _repository.UserDetail.SaveEntityAsync(_context);

            if (saveResult > 0)
            {
                var dto = _mapper.Map<UserDetailView>(newUser);
                return (dto, "User saved successfully.");
            }

            return (null, "Unable to save user.");
        }


        public async Task<(UserDetailView? updatedUser, string message)> UpdateUser(int userId, UserDetailDto userDto)
        {
            var userEntity = await _repository.UserDetail.FindAsync(_context, u => u.UserId == userId);
            var user = userEntity?.FirstOrDefault();

            if (user == null)
                return (null, "User not found");

            user.PatchEntity(userDto);

            _repository.UserDetail.Update(_context, user);
            var result = await _repository.UserDetail.SaveEntityAsync(_context);

            if (result > 0)
            {
                var dto = _mapper.Map<UserDetailView>(user);
                return (dto, "User updated successfully.");
            }

            return (null, "Unable to update user.");
        }

        public async Task<(UserLoginResponseModel? result, string message)> GetToken(string userName, string password)
        {
            UserLoginResponseModel? result = null;
            string message = "";

            var account = _repository.UserDetail.Find(_context, u => u.UserName == userName && u.Password == password);
            var entity = account.FirstOrDefault();
            if (entity == null)
            {
                return (null, "Invalid login credentials.");
            }
            if (entity != null)
            {
                var user = account.First();
                var claim = this.GetClaim(user);
                if (claim != null)
                {
                    var token = GenerateJSONWebToken(claim, _config["Jwt:Key"], _config["Jwt:Issuer"]);
                    result = new UserLoginResponseModel
                    {
                        Token = token,
                        //UserId = user.UserId,
                        UserName = user.UserName,
                        EmailId = user.EmailId,
                        PhoneNumber = user.PhoneNumber,
                    };
                }
                return (result, "Success");
            }
            else
            {
                return (null, "Unauthorized Access.");
            }
        }

        public IEnumerable<Claim> GetClaim(UserDetail user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserName", user.UserName ?? string.Empty),
                new Claim("EmailId", user.EmailId ?? string.Empty),
                new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                new Claim("GeneratedDateTime", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")),
            };
            return claims;
        }

        public string GenerateJSONWebToken(IEnumerable<Claim> claims, string key, string issuer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expireTimeConfig = int.Parse(_config["Jwt:ExpireTime"]);
            var expireTime = expireTimeConfig == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMinutes(expireTimeConfig);

            var token = new JwtSecurityToken(issuer,
              issuer,
              claims,
              expires: expireTime,
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool isSuccess, string message)> ChangePassword(ChangePasswordRequest request)
        {
            var user = _repository.UserDetail.Find(_context, u => u.UserName == request.UserName)?.FirstOrDefault();

            if (user == null)
                return (false, "User not found.");

            if (user.Password != request.OldPassword)
                return (false, "Old password is incorrect.");

            if (user.Password == request.NewPassword)
            {
                return (false, "New password cannot be the same as the old password.");
            }

            if (request.NewPassword != request.ConfirmPassword)
                return (false, "New and confirm passwords do not match.");

            user.Password = request.NewPassword;
            _repository.UserDetail.Update(_context, user);
            var result = await _repository.UserDetail.SaveEntityAsync(_context);

            return result > 0
                ? (true, "Password changed successfully.")
                : (false, "Failed to change password.");
        }
    }
}
