using HttpMock.Configuration;
using HttpMock.RequestProcessing;
using HttpMock.Serializations;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class SetDomainConfigurationCommandHandler : ICommandRequestHandler
{
    private readonly ILogger<SetDomainConfigurationCommandHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;
    private readonly ISerializationProvider _serializationProvider;

    public SetDomainConfigurationCommandHandler(
        ILogger<SetDomainConfigurationCommandHandler> logger,
        IConfigurationStorage configurationStorage,
        ISerializationProvider serializationProvider)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
        _serializationProvider = serializationProvider;
    }

    public static string CommandName => "set-configuration";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        if (!CommandValidationRules.IsDomainValid(ref requestDetails, out var errorMessage) ||
            !CommandValidationRules.IsDomainValid(ref requestDetails, out errorMessage))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, errorMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        var serializer = _serializationProvider.GetSerialization(requestDetails.ContentType);
        if (serializer == default)
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, $"Cannot find serializer for content type: {requestDetails.ContentType}", cancellationToken).ConfigureAwait(false);
            return;
        }

        var domainConfigurationDto = await serializer.DeserializeAsync(httpRequest.Body, cancellationToken).ConfigureAwait(false);

        var domainConfiguration = ConfigurationModelConverter.DtoToModel.Convert(requestDetails.Domain!, domainConfigurationDto);
        if (domainConfiguration == default)
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        domainConfiguration = domainConfiguration with { Domain = requestDetails.Domain ?? string.Empty };

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug($"Set configuration for domain: {domainConfiguration.Domain}");

        _configurationStorage.ConfigureDomain(domainConfiguration);

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, $"Configured domain: {domainConfiguration.Domain}", cancellationToken).ConfigureAwait(false);
    }
}