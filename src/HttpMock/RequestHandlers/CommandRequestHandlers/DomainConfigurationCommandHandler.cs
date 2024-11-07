using HttpMock.Configuration;
using HttpMock.RequestProcessing;
using HttpMock.Serializations;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class DomainConfigurationCommandHandler : ICommandRequestHandler
{
    private readonly IConfigurationStorage _configurationStorage;
    private readonly ISerializationProvider _serializationProvider;

    public DomainConfigurationCommandHandler(
        IConfigurationStorage configurationStorage,
        ISerializationProvider serializationProvider)
    {
        _configurationStorage = configurationStorage;
        _serializationProvider = serializationProvider;
    }

    public const string CommandName = "domain-configurations";

    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!RequestValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse.WithStatusCode(StatusCodes.Status400BadRequest)
                .WithContentAsync(errorMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        var handleResult = requestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(requestDetails, httpResponse, cancellationToken),
            HttpMethodType.Put => Put(requestDetails, httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(requestDetails, httpResponse, cancellationToken),
            _ => Unknown(requestDetails, httpResponse, cancellationToken),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        var domainConfigurationDto = ConfigurationModelConverter.ModelToDto.Convert(domainConfiguration);
        if (domainConfigurationDto == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status400BadRequest);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var serializer = _serializationProvider.GetSerialization(requestDetails.ContentType);
        if (serializer == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status415UnsupportedMediaType);
            return;
        }

        await serializer.SerializeAsync(domainConfigurationDto, httpResponse.Body, cancellationToken).ConfigureAwait(false);

        httpResponse.WithStatusCode(StatusCodes.Status200OK);
    }

    private async ValueTask Put(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var serializer = _serializationProvider.GetSerialization(requestDetails.ContentType);
        if (serializer == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status415UnsupportedMediaType);
            return;
        }

        var domainConfigurationDto = await serializer.DeserializeAsync(requestDetails.RequestBody, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var domainConfiguration = ConfigurationModelConverter.DtoToModel.Convert(requestDetails.Domain!, domainConfigurationDto);
        if (domainConfiguration == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        domainConfiguration = domainConfiguration with { Domain = requestDetails.Domain ?? string.Empty };

        _configurationStorage.ConfigureDomain(domainConfiguration);

        await httpResponse.WithStatusCode(StatusCodes.Status200OK)
            .WithContentAsync($"Configured domain: {domainConfiguration.Domain}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Delete(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!RequestValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse.WithStatusCode(StatusCodes.Status400BadRequest)
                .WithContentAsync(errorMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!_configurationStorage.TryRemoveDomain(requestDetails.Domain!))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        await httpResponse.WithStatusCode(StatusCodes.Status200OK)
            .WithContentAsync($"Domain removed: {requestDetails.Domain}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Unknown(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        await httpResponse
            .WithStatusCode(StatusCodes.Status405MethodNotAllowed)
            .WithContentAsync($"Cannot handle command '{CommandName}'", cancellationToken: cancellationToken);
    }
}