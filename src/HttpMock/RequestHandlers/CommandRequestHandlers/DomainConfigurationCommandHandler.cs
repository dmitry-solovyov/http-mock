using HttpMock.Configuration;
using HttpMock.Models;
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

    public async ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!RequestValidationRules.IsDomainValid(ref commandRequestDetails, out var errorMessage))
        {
            await httpResponse.WithStatusCode(StatusCodes.Status400BadRequest)
                .WithContentAsync(errorMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        var handleResult = commandRequestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(commandRequestDetails, httpResponse, cancellationToken),
            HttpMethodType.Put => Put(commandRequestDetails, httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(commandRequestDetails, httpResponse, cancellationToken),
            _ => Unknown(httpResponse),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(commandRequestDetails.Domain, out var domainConfiguration))
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

        var serializer = _serializationProvider.GetSerialization(commandRequestDetails.ContentType);
        if (serializer == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status415UnsupportedMediaType);
            return;
        }

        await serializer.SerializeAsync(domainConfigurationDto, httpResponse.Body, cancellationToken).ConfigureAwait(false);

        httpResponse.WithStatusCode(StatusCodes.Status200OK);
    }

    private async ValueTask Put(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var serializer = _serializationProvider.GetSerialization(commandRequestDetails.ContentType);
        if (serializer == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status415UnsupportedMediaType);
            return;
        }

        var domainConfigurationDto = await serializer.DeserializeAsync(commandRequestDetails.RequestBody, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var domainConfiguration = ConfigurationModelConverter.DtoToModel.Convert(commandRequestDetails.Domain!, domainConfigurationDto);
        if (domainConfiguration == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status400BadRequest);
            return;
        }

        domainConfiguration = domainConfiguration with { Domain = commandRequestDetails.Domain ?? string.Empty };

        _configurationStorage.ConfigureDomain(domainConfiguration);

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands)
            .WithContentAsync($"Configured domain: {domainConfiguration.Domain}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Delete(CommandRequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!RequestValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse
                .WithStatusCode(StatusCodes.Status400BadRequest)
                .WithContentAsync(errorMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!_configurationStorage.TryRemoveDomain(requestDetails.Domain!))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands)
            .WithContentAsync($"Domain removed: {requestDetails.Domain}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static ValueTask Unknown(HttpResponse httpResponse)
    {
        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledMethod);
        return ValueTask.CompletedTask;
    }
}