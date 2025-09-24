using System.Linq.Expressions;

namespace Core.Interfaces;

public interface ISoftDeleteService
{
    /// <summary>
    /// Soft delete filtresini geçici olarak devre dışı bırakır (admin görünürlüğü için)
    /// </summary>
    IDisposable DisableSoftDeleteFilter<T>() where T : class;
    
    /// <summary>
    /// Soft delete filtresini geçici olarak devre dışı bırakır (belirli entity'ler için)
    /// </summary>
    IDisposable DisableSoftDeleteFilter(params Type[] entityTypes);
    
    /// <summary>
    /// Kullanıcının admin yetkisi olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsAdminAsync(Guid? userId);
    
    /// <summary>
    /// Kullanıcının belirli bir entity için admin yetkisi olup olmadığını kontrol eder
    /// </summary>
    Task<bool> CanViewDeletedAsync<T>(Guid? userId) where T : class;
}
