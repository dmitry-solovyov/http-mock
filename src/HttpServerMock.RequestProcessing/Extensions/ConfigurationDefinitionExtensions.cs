namespace HttpServerMock.RequestDefinitions.Extensions;

public static class ConfigurationDefinitionExtensions
{
    public static bool HasData(this ref ConfigurationDefinition configurationDefinition)
    {
        return configurationDefinition.Map?.Count > 0;
    }
}
