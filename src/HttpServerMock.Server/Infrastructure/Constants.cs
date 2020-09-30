namespace HttpServerMock.Server.Infrastructure
{
    public static class Constants
    {
        public static class HeaderNames
        {
            public const string CommandHeader = "httpmock-command";
        }

        public static class HeaderValues
        {
            public const string ConfigureCommandName = "configure-routing";
            public const string ResetCounterCommandName = "reset-counters";
            public const string ResetConfigurationCommandName = "reset-configuration";
        }
    }
}
