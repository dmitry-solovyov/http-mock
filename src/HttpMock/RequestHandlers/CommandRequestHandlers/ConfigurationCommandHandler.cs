using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using HttpMock.Serializations;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class ConfigurationCommandHandler : ICommandRequestHandler
{
    private readonly IConfigurationStorage _configurationStorage;
    private readonly ISerializationProvider _serializationProvider;

    public ConfigurationCommandHandler(
        IConfigurationStorage configurationStorage,
        ISerializationProvider serializationProvider)
    {
        _configurationStorage = configurationStorage;
        _serializationProvider = serializationProvider;
    }

    public const string CommandName = "configurations";

    public async ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var handleResult = commandRequestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(commandRequestDetails, httpResponse, cancellationToken),
            HttpMethodType.Put => Put(commandRequestDetails, httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(httpResponse, cancellationToken),
            _ => Unknown(httpResponse),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetConfiguration(out var configuration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        var configurationDto = ConfigurationModelConverter.ModelToDto.Convert(configuration);
        if (configurationDto == default)
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

        await serializer.SerializeAsync(configurationDto, httpResponse.Body, cancellationToken).ConfigureAwait(false);

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

        var configurationDto = await serializer.DeserializeAsync(commandRequestDetails.RequestBody, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var configuration = ConfigurationModelConverter.DtoToModel.Convert(configurationDto);
        if (configuration == default)
        {
            httpResponse.WithStatusCode(StatusCodes.Status400BadRequest);
            return;
        }

        _configurationStorage.SetConfiguration(configuration);

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands)
            .WithContentAsync($"Configured endpoints: {configuration.Endpoints?.Length}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Delete(HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var existingEndpoints = _configurationStorage.TryGetConfiguration(out var configuration)
            ? configuration!.Endpoints?.Length
            : -1;

        _configurationStorage.RemoveConfiguration();

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands)
            .WithContentAsync($"Removed endpoints: {existingEndpoints}", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static ValueTask Unknown(HttpResponse httpResponse)
    {
        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownCommandMethodType);
        return ValueTask.CompletedTask;
    }
}