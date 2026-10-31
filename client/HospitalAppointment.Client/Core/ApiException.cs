using System.Net;

namespace HospitalAppointment.Client.Core;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
