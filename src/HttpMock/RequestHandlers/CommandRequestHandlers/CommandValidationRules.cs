using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public static class CommandValidationRules
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

    public static bool IsContentTypeValid(ref readonly RequestDetails requestDetails, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(requestDetails.ContentType))
        {
            errorMessage = "ContentType is invalid!";
            return false;
        }

        errorMessage = default;
        return true;
    }
}