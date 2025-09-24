using Application.DTOs.User;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Core.Exceptions;
using System.Security;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly ISoftDeleteService _softDeleteService;

    public UserService(IUnitOfWork unitOfWork, IPasswordService passwordService, ICacheService cacheService, IEmailService emailService, ISoftDeleteService softDeleteService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _cacheService = cacheService;
        _emailService = emailService;
        _softDeleteService = softDeleteService;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var cacheKey = $"user:{id}";
        var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
        
        if (cachedUser != null && !includeDeleted)
        {
            return cachedUser;
        }

        User? user;
        if (includeDeleted)
        {
            // Admin görünürlüğü için soft delete filtresini geçici olarak devre dışı bırak
            using var _ = _softDeleteService.DisableSoftDeleteFilter<User>();
            user = await _unitOfWork.Users.GetByIdAsync(id);
        }
        else
        {
            user = await _unitOfWork.Users.GetByIdAsync(id);
        }

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Admin değilse silinmiş kayıtları göremez
        if (user.IsDeleted && !includeDeleted)
        {
            throw new NotFoundException("User not found");
        }

        var userDto = await MapToUserDtoAsync(user);
        
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(5));

        return userDto;
    }

    public async Task<UserDto> GetByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == email && !u.IsDeleted);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return await MapToUserDtoAsync(user);
    }

    public async Task<List<UserDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var users = await _unitOfWork.Users.GetAllAsync(u => !u.IsDeleted);
        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        return userDtos;
    }

    public async Task<List<UserDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var users = await _unitOfWork.Users.GetAllAsync(u => 
            !u.IsDeleted && 
            ((u.FirstName != null && u.FirstName.Contains(searchTerm)) || 
             (u.LastName != null && u.LastName.Contains(searchTerm)) || 
             (u.Email != null && u.Email.Contains(searchTerm)) || 
             (u.UserName != null && u.UserName.Contains(searchTerm))));

        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        return userDtos;
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Users.GetAsync(u => 
            u.Email == request.Email || u.UserName == request.UserName);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email or username already exists");
        }

        // Validate password strength
        if (!_passwordService.IsPasswordStrong(request.Password))
        {
            throw new ArgumentException("Password does not meet security requirements");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordService.HashPassword(request.Password),
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign roles
        if (request.RoleIds.Any())
        {
            foreach (var roleId in request.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                };
                await _unitOfWork.UserRoles.AddAsync(userRole);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        // Clear cache
        await _cacheService.RemoveAsync($"user:{user.Id}");

        return await MapToUserDtoAsync(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw new ArgumentException("User not found");
        }

        // Check if email or username is already taken by another user
        var existingUser = await _unitOfWork.Users.GetAsync(u => 
            (u.Email == request.Email || u.UserName == request.UserName) && 
            u.Id != id && !u.IsDeleted);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email or username is already taken by another user");
        }

        user.Email = request.Email;
        user.UserName = request.UserName;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);

        // Update roles
        var existingRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == id);
        await _unitOfWork.UserRoles.DeleteRangeAsync(existingRoles);

        foreach (var roleId in request.RoleIds)
        {
            var userRole = new UserRole
            {
                UserId = id,
                RoleId = roleId
            };
            await _unitOfWork.UserRoles.AddAsync(userRole);
        }

        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return await MapToUserDtoAsync(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Soft delete
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Verify current password
        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash ?? ""))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Validate new password strength
        if (!_passwordService.IsPasswordStrong(request.NewPassword))
        {
            throw new ArgumentException("New password does not meet security requirements");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return true;
    }

    public async Task<bool> UpdateProfileAsync(Guid id, UpdateProfileRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{id}");

        return true;
    }

    public async Task<List<UserDto>> GetByRoleAsync(string roleName, int page = 1, int pageSize = 10)
    {
        var role = await _unitOfWork.Roles.GetAsync(r => r.Name == roleName && !r.IsDeleted);
        if (role == null)
        {
            return new List<UserDto>();
        }

        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.RoleId == role.Id);
        var userIds = userRoles.Select(ur => ur.UserId).ToList();
        var users = await _unitOfWork.Users.GetAllAsync(u => userIds.Contains(u.Id) && !u.IsDeleted);

        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        return userDtos;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId)
    {
        // Check if user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Check if role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null || role.IsDeleted || !role.IsActive)
        {
            return false;
        }

        // Check if user already has this role
        var existingUserRole = await _unitOfWork.UserRoles.GetAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (existingUserRole != null)
        {
            return true; // Already assigned
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        await _unitOfWork.UserRoles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{userId}");

        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await _unitOfWork.UserRoles.GetAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole == null)
        {
            return false;
        }

        await _unitOfWork.UserRoles.DeleteAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"user:{userId}");

        return true;
    }

    public async Task<List<UserDto>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 10)
    {
        var storeUsers = await _unitOfWork.StoreUsers.GetAllAsync(su => su.StoreId == storeId);
        var userIds = storeUsers.Select(su => su.UserId).ToList();
        var users = await _unitOfWork.Users.GetAllAsync(u => userIds.Contains(u.Id) && !u.IsDeleted);

        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

        var userDtos = new List<UserDto>();
        foreach (var user in pagedUsers)
        {
            userDtos.Add(await MapToUserDtoAsync(user));
        }

        return userDtos;
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }
        var cacheKey = $"email_confirm:{userId}";
        var stored = await _cacheService.GetAsync<string>(cacheKey);
        if (string.IsNullOrEmpty(stored) || !string.Equals(stored, token, StringComparison.Ordinal))
        {
            return false;
        }

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync($"user:{userId}");

        return true;
    }

    public Task<bool> ResendEmailConfirmationAsync(Guid userId)
    {
        return ResendEmailConfirmationInternalAsync(userId);
    }

    private async Task<bool> ResendEmailConfirmationInternalAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }
        var token = Guid.NewGuid().ToString("N");
        var cacheKey = $"email_confirm:{userId}";
        await _cacheService.SetAsync(cacheKey, token, TimeSpan.FromHours(24));

        var link = $"https://localhost:7001/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={token}";
        await _emailService.SendEmailConfirmationAsync(user.Email, link);
        return true;
    }

    public Task<bool> ForgotPasswordAsync(string email)
    {
        return ForgotPasswordInternalAsync(email);
    }

    public Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        return ResetPasswordInternalAsync(email, token, newPassword);
    }

    private async Task<bool> ForgotPasswordInternalAsync(string email)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == email && !u.IsDeleted);
        if (user == null)
        {
            return false;
        }
        var token = Guid.NewGuid().ToString("N");
        var cacheKey = $"pwd_reset:{user.Id}";
        await _cacheService.SetAsync(cacheKey, token, TimeSpan.FromHours(1));

        var link = $"https://localhost:7001/api/auth/reset-password?email={Uri.EscapeDataString(email)}&token={token}";
        await _emailService.SendPasswordResetAsync(email, link);
        return true;
    }

    private async Task<bool> ResetPasswordInternalAsync(string email, string token, string newPassword)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == email && !u.IsDeleted);
        if (user == null)
        {
            return false;
        }
        var cacheKey = $"pwd_reset:{user.Id}";
        var stored = await _cacheService.GetAsync<string>(cacheKey);
        if (string.IsNullOrEmpty(stored) || !string.Equals(stored, token, StringComparison.Ordinal))
        {
            return false;
        }
        if (!_passwordService.IsPasswordStrong(newPassword))
        {
            throw new ArgumentException("New password does not meet security requirements");
        }
        user.PasswordHash = _passwordService.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync($"user:{user.Id}");
        return true;
    }

    private async Task<UserDto> MapToUserDtoAsync(User user)
    {
        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == user.Id);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetAllAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted);

        // Get user addresses
        var addresses = await _unitOfWork.Addresses.GetAllAsync(a => a.UserId == user.Id && !a.IsDeleted);

        // Get order count
        var orderCount = await _unitOfWork.Orders.CountAsync(o => o.UserId == user.Id && !o.IsDeleted);

        // Get store count
        var storeCount = await _unitOfWork.StoreUsers.CountAsync(su => su.UserId == user.Id);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles.Select(r => r.Name).ToList(),
            Addresses = addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Title = a.Title,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Company = a.Company,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                PhoneNumber = a.PhoneNumber,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive
            }).ToList(),
            OrderCount = orderCount,
            StoreCount = storeCount
        };
    }
}
