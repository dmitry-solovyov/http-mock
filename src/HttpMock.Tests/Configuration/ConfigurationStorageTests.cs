using FluentAssertions;
using HttpMock.Configuration;
using Xunit;

namespace HttpMock.Tests.Configuration
{
    public class ConfigurationStorageTests
    {
        [Fact]
        public void TryGetConfiguration_WhenConfigurationEmpty_ExpectFalse()
        {
            var storage = new ConfigurationStorage();

            var result = storage.TryGetConfiguration(out var _);

            result.Should().BeFalse();
        }
    }
}
