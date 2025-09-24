using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Common;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : BaseController
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    public class CreateThreadRequest
    {
        public Guid OtherUserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
    }

    [HttpPost("threads")]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request)
    {
        var userId = GetCurrentUserId();
        var thread = await _messagingService.GetOrCreateThreadAsync(userId, request.OtherUserId, request.Subject, request.OrderId);
        return Ok(ApiResponse.Success(new { threadId = thread.Id }));
    }

    [HttpGet("threads/{threadId}/messages")]
    public async Task<IActionResult> GetThreadMessages(Guid threadId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var msgs = await _messagingService.GetThreadMessagesAsync(userId, threadId, page, pageSize);
            return Ok(ApiResponse.Success(msgs));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return HandleResult(ApiResponse.NotFound<object>(ex.Message));
        }
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
    }

    [HttpPost("threads/{threadId}/messages")]
    public async Task<IActionResult> SendMessage(Guid threadId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var msg = await _messagingService.SendMessageAsync(userId, threadId, request.Content, request.AttachmentUrl, request.AttachmentFileName);
            return Ok(ApiResponse.Success(msg));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { ex.Message }));
        }
    }

    [HttpPost("threads/{threadId}/messages/{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid threadId, Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ok = await _messagingService.MarkAsReadAsync(userId, threadId, messageId);
            if (!ok) return HandleResult(ApiResponse.NotFound<object>("Message not found"));
            return Ok(ApiResponse.Success(true));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _messagingService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse.Success(new { count }));
    }
}
