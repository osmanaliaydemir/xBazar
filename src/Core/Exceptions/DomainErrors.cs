using System.Net;

namespace Core.Exceptions;

public abstract class DomainExceptionBase : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }
    protected DomainExceptionBase(string message, HttpStatusCode statusCode, string? errorCode = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public sealed class NotFoundException : DomainExceptionBase
{
    public NotFoundException(string message, string? errorCode = null) : base(message, HttpStatusCode.NotFound, errorCode) {}
}

public sealed class ConflictException : DomainExceptionBase
{
    public ConflictException(string message, string? errorCode = null) : base(message, HttpStatusCode.Conflict, errorCode) {}
}

public sealed class ValidationException : DomainExceptionBase
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(string message, IEnumerable<string> errors, string? errorCode = null)
        : base(message, HttpStatusCode.BadRequest, errorCode)
    {
        Errors = errors.ToList();
    }
}

public sealed class ForbiddenException : DomainExceptionBase
{
    public ForbiddenException(string message, string? errorCode = null) : base(message, HttpStatusCode.Forbidden, errorCode) {}
}


