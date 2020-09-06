using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Extensions
{
    public static class PipeReaderExtensions
    {
        public static async Task<string> ReadPipeAsync(this PipeReader reader)
        {
            List<byte>? output = null;

            while (reader.TryRead(out var readResult))
            {
                var buffer = readResult.Buffer;
                var position = buffer.Start;

                while (readResult.Buffer.TryGet(ref position, out var memoryLine))
                {
                    if (memoryLine.IsEmpty)
                        continue;

                    if (output == null)
                        output = new List<byte>();

                    output.AddRange(memoryLine.ToArray());
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync().ConfigureAwait(false);

            if (output == null)
                return string.Empty;

            return Encoding.ASCII.GetString(output.ToArray());
        }
    }
}