using System.Net;

namespace Core.Exceptions;

public class DomainException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }

    public DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? errorCode = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}


