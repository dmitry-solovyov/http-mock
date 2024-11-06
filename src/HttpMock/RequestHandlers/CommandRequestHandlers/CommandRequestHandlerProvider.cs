namespace HttpMock.RequestHandlers.CommandRequestHandlers
{
    public class CommandRequestHandlerProvider : ICommandRequestHandlerProvider
    {
        public Type? GetCommandHandlerType(string commandName, HttpMethodType httpMethod)
        {
            return httpMethod switch
            {
                HttpMethodType.Get when string.Equals(commandName, GetDomainsCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(GetDomainsCommandHandler),

                HttpMethodType.Get when string.Equals(commandName, GetDomainConfigurationCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(GetDomainConfigurationCommandHandler),

                HttpMethodType.Put when string.Equals(commandName, SetDomainConfigurationCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(SetDomainConfigurationCommandHandler),

                HttpMethodType.Get when string.Equals(commandName, GetStatisticsCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(GetStatisticsCommandHandler),

                HttpMethodType.Post when string.Equals(commandName, ResetCountersCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(ResetCountersCommandHandler),

                HttpMethodType.Post when string.Equals(commandName, RemoveDomainConfigurationCommandHandler.CommandName, StringComparison.OrdinalIgnoreCase)
                    => typeof(RemoveDomainConfigurationCommandHandler),

                _ => default
            };
        }
    }
}
