using HttpMock.Models;

namespace HttpMock.RequestHandlers;

public static class RequestValidationRules
{
    public static bool IsDomainValid(ref readonly CommandRequestDetails commandRequestDetails, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(commandRequestDetails.Domain))
        {
            errorMessage = "Domain is required!";
            return false;
        }

        errorMessage = default;
        return true;
    }
}