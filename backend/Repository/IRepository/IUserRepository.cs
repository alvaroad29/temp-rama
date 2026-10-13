using System;
using backend.Models.Dtos;
using backend.Models.Dtos.UserQueries;

namespace backend.Repository;

public interface IUserRepository
{
    Task<PagedResponse<UserDataDto>> GetUsers(UserQueryParameters parameters);
    Task<UserDataDto?> GetUser(string id);
    Task<bool> IsUsernameUnique(string username);
    Task<bool> IsEmailUnique(string email);
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);
    Task<UserDataDto> Register(CreateUserDto userRegisterDto);
    Task<UserLoginResponseDto> CheckStatus(string userId);
    Task<UserLoginResponseDto?> UpdateUser(string userId, UpdateUserDto updateUserDto, bool isAdmin = false);
    Task<bool> updateUserProfile(string userId, string imageUrl);
    Task<bool> BanUser(string userId);
    Task<bool> UnbanUser(string userId);
    Task<bool> DeleteUser(string userId);
    Task<bool> AssignRole(string userId, string roleName);
    Task<bool> RemoveRole(string userId, string roleName); 

    // estadis
    Task<AdminStatisticsDto> GetAdminStatistics();
    Task<AuthorStatisticsDto> GetAuthorStatistics(string authorId);

}
