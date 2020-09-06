using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using HttpServerMock.RequestDefinitions;
using YamlDotNet.RepresentationModel;

namespace HttpServerMock.RequestProcessing.Yaml
{
    public class YamlRequestDefinitionReader : IRequestDefinitionReader
    {
        public IEnumerable<RequestDefinition> Read(TextReader textReader)
        {
            return ReadImpl(textReader);
        }

        private IEnumerable<RequestDefinition> ReadImpl(TextReader textReader)
        {
            var yaml = new YamlStream();
            yaml.Load(textReader);

            if (yaml.Documents.Count == 0)
                throw new Exception("No documents found!");

            foreach (var yamlDocument in yaml.Documents)
            {
                var mapMode = (YamlSequenceNode)yamlDocument.RootNode["map"];
                foreach (var mapNode in mapMode.Children.OfType<YamlMappingNode>())
                {
                    var requestDefinition = ParseDefinitionNode(mapNode);
                    yield return requestDefinition;
                }
            }
        }

        private static RequestDefinition ParseDefinitionNode(YamlMappingNode yamlMappingNode)
        {
            var url = ScalarField(yamlMappingNode, "url");
            var payload = GetPayloadField(yamlMappingNode);
            var delay = GetDelay(yamlMappingNode);
            var statusCode = ScalarField(yamlMappingNode, "status-code", "status");
            var contentType = ScalarField(yamlMappingNode, "content-type");
            var method = ScalarField(yamlMappingNode, "http-method", "method");
            var headers = GetDictionaryField(yamlMappingNode, "headers");

            var requestDefinition = new RequestDefinition(
                new RequestDefinitionWhen
                {
                    Url = url
                },
                new RequestDefinitionThen
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/json" : new ContentType(contentType).MediaType,
                    StatusCode = int.TryParse(statusCode, out var parsedStatusCode) ? parsedStatusCode : (int)HttpStatusCode.OK,
                    Delay = delay,
                    Method = method,
                    Payload = payload,
                    ProxyUrl = null,
                    Headers = headers
                });

            //var (adjustedUrl, adjustedUrlVariables) = NormalizeUrl(requestDefinition);
            //requestDefinition.SetUrlDetails(adjustedUrl, adjustedUrlVariables);

            //var (adjustedMethod, _) = NormalizeSearchExpression(requestDefinition.Method);
            //requestDefinition.SetMethod(adjustedMethod);

            return requestDefinition;
        }

        private static Dictionary<string, string> GetDictionaryField(YamlMappingNode yamlMappingNode, string tagName)
        {
            if (yamlMappingNode.Children.ContainsKey(tagName))
            {
                var headersNode = yamlMappingNode.Children[tagName];
                if (headersNode != null && headersNode is YamlSequenceNode sequenceNode)
                {
                    var result = new Dictionary<string, string>();
                    foreach (var sequenceNodeChild in sequenceNode.Children)
                    {
                        if (!(sequenceNodeChild is YamlScalarNode sequenceScalarNode)) continue;

                        var headerDefinitionValue = sequenceScalarNode.Value;
                        if (string.IsNullOrWhiteSpace(headerDefinitionValue)) continue;

                        var headerParts = headerDefinitionValue.Split(':');
                        if (headerParts.Length != 2) continue;

                        var headerName = headerParts[0];
                        var headerValue = headerParts[1];
                        if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue)) continue;

                        result[headerName] = headerValue;
                    }

                    return result;
                }
            }

            return null;
        }

        private static int GetDelay(YamlMappingNode yamlMappingNode)
        {
            var delay = ScalarField(yamlMappingNode, "delay", "wait");

            return int.TryParse(delay, out var parsedDelay) ? parsedDelay : 0;
        }
        private static string GetPayloadField(YamlMappingNode yamlMappingNode)
        {
            var payload = ScalarField(yamlMappingNode, "payload", "body");
            return payload;
        }

        private static string ScalarField(YamlNode node, params string[] keys)
        {
            foreach (var key in keys)
            {
                switch (node)
                {
                    case YamlMappingNode mappingNode:
                        if (mappingNode.Children.ContainsKey(key))
                            return (node[key] as YamlScalarNode)?.Value;
                        break;

                    default:
                        if (node[key] is YamlScalarNode scalar)
                            return scalar.Value;
                        return null;
                }
            }

            return null;
        }
    }
}
