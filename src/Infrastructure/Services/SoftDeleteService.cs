using Core.Interfaces;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Services;

public class SoftDeleteService : ISoftDeleteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly Dictionary<Type, bool> _disabledFilters = new();

    public SoftDeleteService(IUnitOfWork unitOfWork, ICurrentUserContext currentUserContext)
    {
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
    }

    public IDisposable DisableSoftDeleteFilter<T>() where T : class
    {
        return DisableSoftDeleteFilter(typeof(T));
    }

    public IDisposable DisableSoftDeleteFilter(params Type[] entityTypes)
    {
        var disabledTypes = new List<Type>();
        
        foreach (var entityType in entityTypes)
        {
            if (!_disabledFilters.ContainsKey(entityType))
            {
                _disabledFilters[entityType] = true;
                disabledTypes.Add(entityType);
            }
        }

        return new SoftDeleteFilterDisposable(this, disabledTypes.ToArray());
    }

    public async Task<bool> IsAdminAsync(Guid? userId)
    {
        if (!userId.HasValue) return false;

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
        if (user == null) return false;

        // Admin rolü kontrolü
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId.Value);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetAllAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted);
        
        return roles.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                             r.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> CanViewDeletedAsync<T>(Guid? userId) where T : class
    {
        if (!userId.HasValue) return false;

        // Admin ise tüm silinmiş kayıtları görebilir
        if (await IsAdminAsync(userId)) return true;

        // Store admin ise sadece kendi store'undaki silinmiş kayıtları görebilir
        if (typeof(T) == typeof(Product) || typeof(T) == typeof(Order))
        {
            return await IsStoreAdminAsync(userId.Value);
        }

        return false;
    }

    private async Task<bool> IsStoreAdminAsync(Guid userId)
    {
        var storeUsers = await _unitOfWork.StoreUsers.GetAllAsync(su => su.UserId == userId);
        return storeUsers.Any(su => su.Role == StoreUserRole.Owner || 
                                   su.Role == StoreUserRole.Manager);
    }

    internal void EnableSoftDeleteFilter(Type entityType)
    {
        _disabledFilters.Remove(entityType);
    }

    internal bool IsSoftDeleteFilterDisabled(Type entityType)
    {
        return _disabledFilters.ContainsKey(entityType);
    }

    private class SoftDeleteFilterDisposable : IDisposable
    {
        private readonly SoftDeleteService _service;
        private readonly Type[] _entityTypes;

        public SoftDeleteFilterDisposable(SoftDeleteService service, Type[] entityTypes)
        {
            _service = service;
            _entityTypes = entityTypes;
        }

        public void Dispose()
        {
            foreach (var entityType in _entityTypes)
            {
                _service.EnableSoftDeleteFilter(entityType);
            }
        }
    }
}
