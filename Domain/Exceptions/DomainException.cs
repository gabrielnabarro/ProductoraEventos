using System.Net;

namespace Domain.Exceptions;

public sealed class DomainException : Exception
{
    public DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
