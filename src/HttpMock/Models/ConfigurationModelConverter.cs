using System.Buffers;
using System.Net.Mime;

namespace HttpMock.Models;

internal static class ConfigurationModelConverter
{
    internal static class ModelToDto
    {
        public static DomainConfigurationDto? Convert(DomainConfiguration? domainConfiguration)
        {
            if (domainConfiguration == default)
                return default;

            var endpoints = domainConfiguration.Endpoints?.Select(Convert).ToArray() ?? [];
            return new DomainConfigurationDto { Endpoints = endpoints };
        }

        private static EndpointConfigurationDto Convert(EndpointConfiguration endpointConfiguration)
        {
            return new EndpointConfigurationDto
            {
                Path = endpointConfiguration.When.Path,
                Method = endpointConfiguration.When.HttpMethod.GetMethodName(),
                ContentType = endpointConfiguration.Then.ContentType,
                Status = endpointConfiguration.Then.StatusCode,
                Delay = endpointConfiguration.Then.Delay,
                Payload = endpointConfiguration.Then.Payload,
                Headers = endpointConfiguration.Then.Headers?.ToDictionary(x => x.Key, x => x.Value)
            };
        }
    }

    internal static class DtoToModel
    {
        private const int DefaultStatusCode = StatusCodes.Status200OK;
        private const HttpMethodType DefaultHttpMethodType = HttpMethodType.Get;
        private const string DefaultContentType = MediaTypeNames.Application.Json;

        public static DomainConfiguration? Convert(string domain, DomainConfigurationDto? domainConfigurationDto)
        {
            ArgumentException.ThrowIfNullOrEmpty(domain);

            if (domainConfigurationDto == default)
                return default;

            var endpoints = domainConfigurationDto.Endpoints?.Select(Convert).ToArray() ?? [];
            return new DomainConfiguration(domain, endpoints);
        }

        private static EndpointConfiguration Convert(EndpointConfigurationDto endpointConfigurationDto)
        {
            ArgumentException.ThrowIfNullOrEmpty(endpointConfigurationDto.Path);

            var statusCode = endpointConfigurationDto.Status ?? DefaultStatusCode;

            var contentType = endpointConfigurationDto.ContentType ?? DefaultContentType;

            var httpMethodType = HttpMethodTypeParser.Parse(endpointConfigurationDto.Method, DefaultHttpMethodType);

            var path = endpointConfigurationDto.Path.Trim();

            var when = new EndpointRequestConfiguration(httpMethodType, path);

            var then = new EndpointResponseConfiguration(
                statusCode,
                contentType,
                endpointConfigurationDto.Payload,
                endpointConfigurationDto.Delay,
                endpointConfigurationDto.Headers
            );

            return new EndpointConfiguration(when, then);
        }
    }
}
