
using System.Diagnostics;

namespace YoutubeDownloaderTgBot.Services
{
    public static class CommandService
    {
        public static async Task<string> RunCommand(string command, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,  
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"RunCommand Error: {error}");

            return output;
        }
    }
}
