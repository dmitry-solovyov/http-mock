using System.IO.Pipelines;
using System.Text;

namespace HttpServerMock.Server.Infrastructure.Extensions;

public static class PipeReaderExtensions
{
    public static async Task<string> ReadPipeAsync(this PipeReader reader, CancellationToken cancellationToken)
    {
        List<byte>? output = null;

        cancellationToken.ThrowIfCancellationRequested();

        while (reader.TryRead(out var readResult))
        {
            var buffer = readResult.Buffer;
            var position = buffer.Start;

            while (readResult.Buffer.TryGet(ref position, out var memoryLine))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (memoryLine.IsEmpty)
                    continue;

                output ??= new List<byte>();

                output.AddRange(memoryLine.ToArray());
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (readResult.IsCompleted)
                break;
        }

        await reader.CompleteAsync().ConfigureAwait(false);

        if (output == null)
            return string.Empty;

        return Encoding.ASCII.GetString(output.ToArray());
    }
}