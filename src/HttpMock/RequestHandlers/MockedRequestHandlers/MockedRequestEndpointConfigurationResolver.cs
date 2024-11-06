﻿using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text.RegularExpressions;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestEndpointConfigurationResolver : IMockedRequestEndpointConfigurationResolver
{
    private readonly ILogger<MockedRequestEndpointConfigurationResolver> _logger;
    private readonly IConfigurationStorage _configurationStorage;

    public MockedRequestEndpointConfigurationResolver(
        ILogger<MockedRequestEndpointConfigurationResolver> logger,
        IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
        _logger = logger;
    }

    public bool TryGetEndpointConfiguration(ref readonly RequestDetails requestDetails, out EndpointConfiguration? foundEndpointConfiguration)
    {
        var domain = requestDetails.Domain;
        var httpMethod = requestDetails.HttpMethod;
        var url = requestDetails.QueryPath;

        ArgumentException.ThrowIfNullOrEmpty(domain);

        if (!_configurationStorage.TryGetDomainConfiguration(domain, out DomainConfiguration? domainConfiguration))
        {
            foundEndpointConfiguration = default;
            return false;
        }

        EndpointConfiguration? configurationCandidate = default;

        foreach (var endpointConfiguration in domainConfiguration!.Endpoints)
        {
            if (httpMethod != endpointConfiguration.When.HttpMethod)
                continue;

            if (!IsSameUrl(in requestDetails, endpointConfiguration))
                continue;

            if (configurationCandidate == default)
            {
                configurationCandidate = endpointConfiguration;
            }
            else if (endpointConfiguration.CallCounter < configurationCandidate.CallCounter)
            {
                configurationCandidate = endpointConfiguration;
            }
        }

        configurationCandidate?.IncreaseCounter();

        foundEndpointConfiguration = configurationCandidate;
        return configurationCandidate != default;
    }

    public bool IsSameUrl(ref readonly RequestDetails requestDetails, EndpointConfiguration endpointConfiguration)
    {
        var requestUrl = requestDetails.QueryPath;
        if (string.IsNullOrEmpty(requestUrl))
            return false;

        var endpointUrl = endpointConfiguration.When.Url;
        if (string.IsNullOrEmpty(endpointUrl))
            return false;

        var comparison = endpointConfiguration.When.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (string.Equals(requestUrl, endpointUrl, comparison))
        {
            return true;
        }

        if (endpointUrl.Contains('@') && !string.IsNullOrEmpty(endpointConfiguration.When.UrlRegexExpression))
        {
            var regexOptions = RegexOptions.Singleline;
            if (!endpointConfiguration.When.CaseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;

            var match = Regex.Match(requestUrl, endpointConfiguration.When.UrlRegexExpression, regexOptions);
            if (match.Success)
                return true;
        }

        return false;
    }

    private static string? GetUrlWithoutParameters(string? url)
    {
        return url?.IndexOf('?') switch
        {
            null => default,
            > 0 => url.Substring(0, url.IndexOf('?')),
            -1 => url,
            _ => default
        };
    }
}