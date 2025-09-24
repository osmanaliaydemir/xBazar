using Core.Entities;
using Core.Interfaces;
using Core.Exceptions;
using System.Linq;

namespace Application.Services;

public class MessagingService : IMessagingService
{
    private readonly IUnitOfWork _uow;

    public MessagingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<MessageThread> GetOrCreateThreadAsync(Guid userId, Guid otherUserId, string subject, Guid? orderId = null)
    {
        var existing = await _uow.MessageThreads.GetAsync(t =>
            ((t.User1Id == userId && t.User2Id == otherUserId) || (t.User1Id == otherUserId && t.User2Id == userId)) &&
            t.Subject == subject && t.OrderId == orderId && t.IsActive);
        if (existing != null) return existing;

        var thread = new MessageThread
        {
            Subject = subject,
            User1Id = userId,
            User2Id = otherUserId,
            OrderId = orderId,
            IsActive = true,
            LastMessageAt = DateTime.UtcNow
        };
        await _uow.MessageThreads.AddAsync(thread);
        await _uow.SaveChangesAsync();
        return thread;
    }

    public async Task<List<Message>> GetThreadMessagesAsync(Guid userId, Guid threadId, int page = 1, int pageSize = 50)
    {
        var thread = await _uow.MessageThreads.GetByIdAsync(threadId);
        if (thread == null || !thread.IsActive) throw new NotFoundException("Thread not found");
        if (thread.User1Id != userId && thread.User2Id != userId) throw new ForbiddenException("Not a participant");
        var msgs = await _uow.Messages.GetAllAsync(m => m.ThreadId == threadId && !m.IsDeleted);
        return msgs.OrderBy(m => m.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<Message> SendMessageAsync(Guid userId, Guid threadId, string content, string? attachmentUrl = null, string? attachmentFileName = null)
    {
        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(attachmentUrl))
        {
            throw new ValidationException("Content or attachment is required", new[] { "Content or attachment is required" });
        }
        var thread = await _uow.MessageThreads.GetByIdAsync(threadId);
        if (thread == null || !thread.IsActive) throw new NotFoundException("Thread not found");
        if (thread.User1Id != userId && thread.User2Id != userId) throw new ForbiddenException("Not a participant");
        var receiverId = thread.User1Id == userId ? thread.User2Id : thread.User1Id;
        var msg = new Message
        {
            ThreadId = threadId,
            SenderId = userId,
            ReceiverId = receiverId,
            Type = string.IsNullOrWhiteSpace(attachmentUrl) ? MessageType.Text : MessageType.File,
            Content = content ?? string.Empty,
            AttachmentUrl = attachmentUrl,
            AttachmentFileName = attachmentFileName,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.Messages.AddAsync(msg);
        thread.LastMessageAt = DateTime.UtcNow;
        await _uow.MessageThreads.UpdateAsync(thread);
        await _uow.SaveChangesAsync();
        return msg;
    }

    public async Task<bool> MarkAsReadAsync(Guid userId, Guid threadId, Guid messageId)
    {
        var msg = await _uow.Messages.GetByIdAsync(messageId);
        if (msg == null || msg.ThreadId != threadId) return false;
        if (msg.ReceiverId != userId) throw new ForbiddenException("Only receiver can mark as read");
        if (msg.IsRead) return true;
        msg.IsRead = true;
        msg.ReadAt = DateTime.UtcNow;
        await _uow.Messages.UpdateAsync(msg);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var msgs = await _uow.Messages.GetAllAsync(m => m.ReceiverId == userId && !m.IsDeleted && !m.IsRead);
        return msgs.Count();
    }
}
