using Core.Entities;

namespace Application.Services;

public interface IFavoriteService
{
    Task<bool> AddAsync(Guid userId, FavoriteType type, Guid itemId);
    Task<bool> RemoveAsync(Guid userId, FavoriteType type, Guid itemId);
    Task<List<Guid>> GetUserFavoritesAsync(Guid userId, FavoriteType type);
}

