using Core.Entities;

namespace Core.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<RefreshToken?> GetByJwtIdAsync(string jwtId);
    Task RevokeTokenFamilyAsync(string parentTokenHash, string reason = "Token family revoked");
    Task CleanupExpiredTokensAsync();
    Task RevokeAllUserTokensAsync(Guid userId, string reason = "User logout");
}
