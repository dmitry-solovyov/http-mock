using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers;

public static class RequestValidationRules
{
    public static bool IsDomainValid(ref readonly RequestDetails requestDetails, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(requestDetails.Domain))
        {
            errorMessage = "Domain is not specified!";
            return false;
        }

        errorMessage = default;
        return true;
    }
}