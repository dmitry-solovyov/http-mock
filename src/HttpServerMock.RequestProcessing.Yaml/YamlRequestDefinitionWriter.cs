﻿using HttpServerMock.RequestDefinitions;
using System.IO;

namespace HttpServerMock.RequestDefinitionProcessing.Yaml
{
    public class YamlRequestDefinitionWriter : IRequestDefinitionWriter
    {
        public string ContentType => "application/yaml";

        public void Write(RequestDefinitionItemSet requestDefinitionSet, TextWriter textWriter)
        {
        }
    }
}
