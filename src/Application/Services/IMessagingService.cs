using Core.Entities;

namespace Application.Services;

public interface IMessagingService
{
    Task<MessageThread> GetOrCreateThreadAsync(Guid userId, Guid otherUserId, string subject, Guid? orderId = null);
    Task<List<Message>> GetThreadMessagesAsync(Guid userId, Guid threadId, int page = 1, int pageSize = 50);
    Task<Message> SendMessageAsync(Guid userId, Guid threadId, string content, string? attachmentUrl = null, string? attachmentFileName = null);
    Task<bool> MarkAsReadAsync(Guid userId, Guid threadId, Guid messageId);
    Task<int> GetUnreadCountAsync(Guid userId);
}
