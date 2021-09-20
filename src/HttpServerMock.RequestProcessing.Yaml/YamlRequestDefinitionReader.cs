using HttpServerMock.RequestDefinitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace HttpServerMock.RequestDefinitionProcessing.Yaml
{
    public class YamlRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => "application/yaml";

        public RequestDefinitionItemSet Read(TextReader textReader, CancellationToken cancellationToken)
        {
            var yaml = new YamlStream();
            yaml.Load(textReader);

            cancellationToken.ThrowIfCancellationRequested();

            if (yaml.Documents.Count == 0)
                throw new Exception("No documents found!");

            string? documentName = null;
            var definitions = new List<RequestDefinitionItem>();

            foreach (var yamlDocument in yaml.Documents)
            {
                documentName ??= ((YamlScalarNode)yamlDocument.RootNode["info"]).Value;
                var mapMode = (YamlSequenceNode)yamlDocument.RootNode["map"];

                foreach (var mapNode in mapMode.Children.OfType<YamlMappingNode>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var requestDefinition = ParseDefinitionNode(mapNode, cancellationToken);
                    definitions.Add(requestDefinition);
                }
            }

            return new RequestDefinitionItemSet(documentName, definitions);
        }

        private static RequestDefinitionItem ParseDefinitionNode(YamlMappingNode yamlMappingNode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var description = ScalarField(yamlMappingNode, "description");
            var url = ScalarField(yamlMappingNode, "url");
            var payload = GetPayloadField(yamlMappingNode);
            var delay = GetDelay(yamlMappingNode);
            var statusCode = ScalarField(yamlMappingNode, "status-code", "status");
            var contentType = ScalarField(yamlMappingNode, "content-type");
            contentType = string.IsNullOrWhiteSpace(contentType)
                ? MediaTypeNames.Application.Json
                : new ContentType(contentType).MediaType;
            var method = ScalarField(yamlMappingNode, "http-method", "method");
            var headers = GetDictionaryField(yamlMappingNode, "headers", cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var requestDefinition = new RequestDefinitionItem(
                description,
                new RequestCondition(url, false),
                new ResponseDetails
                (
                    contentType,
                    method,
                    payload,
                    int.TryParse(statusCode, out var parsedStatusCode) ? parsedStatusCode : (int)HttpStatusCode.OK,
                    delay,
                    null,
                    headers
                ));

            return requestDefinition;
        }

        private static Dictionary<string, string>? GetDictionaryField(YamlMappingNode yamlMappingNode, string tagName, CancellationToken cancellationToken)
        {
            if (!yamlMappingNode.Children.ContainsKey(tagName))
                return null;

            var headersNode = yamlMappingNode.Children[tagName];
            if (headersNode is YamlSequenceNode sequenceNode)
            {
                var result = new Dictionary<string, string>();
                foreach (var sequenceNodeChild in sequenceNode.Children)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!(sequenceNodeChild is YamlScalarNode sequenceScalarNode))
                        continue;

                    var headerDefinitionValue = sequenceScalarNode.Value;
                    if (string.IsNullOrWhiteSpace(headerDefinitionValue))
                        continue;

                    var headerParts = headerDefinitionValue.Split(':');
                    if (headerParts.Length != 2)
                        continue;

                    var headerName = headerParts[0];
                    var headerValue = headerParts[1];
                    if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
                        continue;

                    result[headerName] = headerValue;
                }

                return result;
            }

            return null;
        }

        private static int GetDelay(YamlMappingNode yamlMappingNode)
        {
            var delay = ScalarField(yamlMappingNode, "delay", "wait");

            return int.TryParse(delay, out var parsedDelay) ? parsedDelay : 0;
        }
        private static string? GetPayloadField(YamlMappingNode yamlMappingNode)
        {
            var payload = ScalarField(yamlMappingNode, "payload", "body");
            return payload;
        }

        private static string? ScalarField(YamlNode node, params string[] keys)
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
