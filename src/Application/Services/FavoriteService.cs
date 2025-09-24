using Core.Entities;
using Core.Interfaces;

namespace Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _uow;

    public FavoriteService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> AddAsync(Guid userId, FavoriteType type, Guid itemId)
    {
        var exists = await _uow.Favorites.GetAsync(f => f.UserId == userId && f.Type == type && f.ItemId == itemId);
        if (exists != null) return true;
        var fav = new Favorite { UserId = userId, Type = type, ItemId = itemId };
        await _uow.Favorites.AddAsync(fav);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(Guid userId, FavoriteType type, Guid itemId)
    {
        var fav = await _uow.Favorites.GetAsync(f => f.UserId == userId && f.Type == type && f.ItemId == itemId);
        if (fav == null) return false;
        await _uow.Favorites.DeleteAsync(fav);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<Guid>> GetUserFavoritesAsync(Guid userId, FavoriteType type)
    {
        var favs = await _uow.Favorites.GetAllAsync(f => f.UserId == userId && f.Type == type);
        return favs.Select(f => f.ItemId).ToList();
    }
}

