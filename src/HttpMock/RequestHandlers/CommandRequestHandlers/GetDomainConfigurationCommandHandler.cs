using HttpMock.Configuration;
using HttpMock.RequestProcessing;
using HttpMock.Serializations;
using System.Net.Mime;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class GetDomainConfigurationCommandHandler : ICommandRequestHandler
{
    private readonly ILogger<GetDomainConfigurationCommandHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;
    private readonly ISerializationProvider _serializationProvider;

    public GetDomainConfigurationCommandHandler(
        ILogger<GetDomainConfigurationCommandHandler> logger,
        IConfigurationStorage configurationStorage,
        ISerializationProvider serializationProvider)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
        _serializationProvider = serializationProvider;
    }

    public static string CommandName => "get-configuration";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!CommandValidationRules.IsDomainValid(ref requestDetails, out var errorMessage) ||
            !CommandValidationRules.IsContentTypeValid(ref requestDetails, out errorMessage))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, errorMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        var domainConfigurationDto = ConfigurationModelConverter.ModelToDto.Convert(domainConfiguration);
        if (domainConfigurationDto == default)
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var serializer = _serializationProvider.GetSerialization(requestDetails.ContentType);
        if (serializer == default)
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, $"Cannot find serializer for content type: {requestDetails.ContentType}", cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await serializer.SerializeAsync(domainConfigurationDto, httpResponse.Body, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug($"Get configuration for domain: {domainConfiguration.Domain}");

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, requestDetails.ContentType ?? MediaTypeNames.Text.Plain, cancellationToken).ConfigureAwait(false);
    }
}