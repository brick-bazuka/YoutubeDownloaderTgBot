namespace YoutubeDownloaderTgBot
{
    public class Util
    {
        private static object _lock = new();

        public static void Log(string message, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
        {
            lock (_lock)
            {
                Console.ForegroundColor = foreground;
                Console.BackgroundColor = background;
                Console.WriteLine($"{DateTime.Now} | {message}");
                Console.ResetColor();
            }
        }
    }
}
