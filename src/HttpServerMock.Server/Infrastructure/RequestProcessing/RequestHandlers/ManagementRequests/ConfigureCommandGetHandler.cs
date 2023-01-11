using HttpServerMock.Server.Infrastructure.ConfigurationManagement;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;

public class ConfigureCommandGetHandler : IRequestHandler
{
    private readonly ILogger<ConfigureCommandGetHandler> _logger;
    private readonly IConfigurationStorage _requestDefinitionProvider;
    private readonly IConfigurationWriterProvider _requestDefinitionWriteProvider;

    public ConfigureCommandGetHandler(
        ILogger<ConfigureCommandGetHandler> logger,
        IConfigurationStorage requestDefinitionProvider,
        IConfigurationWriterProvider requestDefinitionWriteProvider)
    {
        _logger = logger;
        _requestDefinitionProvider = requestDefinitionProvider;
        _requestDefinitionWriteProvider = requestDefinitionWriteProvider;
    }

    public ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(ProcessGetCommand(ref requestDetails, cancellationToken));
    }

    private HttpResponseDetails ProcessGetCommand(ref HttpRequestDetails requestDetails, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get configuration");

        var requestDefinitionWriter = _requestDefinitionWriteProvider.GetWriter();

        var definitionSets = _requestDefinitionProvider.GetDefinitionSets();
        var fisrstDefinition = definitionSets.FirstOrDefault();
        if (fisrstDefinition == null)
        {
            return ResponseDetailsFactory.Status404NotFound();
        }

        var configurationDefinition = ConfigurationDefinitionConverter.ToConfigurationDefinition(fisrstDefinition);
        if (configurationDefinition.IsEmpty)
        {
            return ResponseDetailsFactory.Status400BadRequest();
        }

        var content = requestDefinitionWriter.Write(ref configurationDefinition);
        if (content == null)
        {
            return ResponseDetailsFactory.Status400BadRequest();
        }

        return ResponseDetailsFactory.Status200OK(content);
    }
}