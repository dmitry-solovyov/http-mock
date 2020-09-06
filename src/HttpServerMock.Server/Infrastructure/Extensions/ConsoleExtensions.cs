using System;
using System.Threading;

namespace HttpServerMock.Server.Infrastructure.Extensions
{
    public static class ConsoleExtensions
    {
        private static readonly SemaphoreSlim LoggerSemaphore = new SemaphoreSlim(1, 1);

        public static void Write(string text, ConsoleColor? color = null)
        {
            if (color == null)
            {
                Console.WriteLine(text);
                return;
            }

            LoggerSemaphore.Wait();

            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color.Value;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultColor;

            LoggerSemaphore.Release();
        }
    }
}
