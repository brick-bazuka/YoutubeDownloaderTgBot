using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeDownloaderTgBot.Services;

namespace YoutubeDownloaderTgBot.Core
{
    public class TgBot
    {
        private string _token;
        private string _ytDlpScriptPath;
        private string _ffmpegScriptPath;
        private TelegramBotClient _botClient;
        private CancellationTokenSource _cts;
        private YoutubeDL _ytdl;
        private DownloadService _download;

        OptionSet options = new OptionSet()
        {
            RestrictFilenames = true, // --restrict-filenames
        };

        public TgBot(string token)
        {
            _token = token;
            _cts = new();
            _botClient = new(token, cancellationToken: _cts.Token);

            _ytDlpScriptPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                Path.Combine("yt-dlp", "yt-dlp.exe") : Path.Combine("yt-dlp", "yt-dlp");

            _ffmpegScriptPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                Path.Combine("yt-dlp", "ffmpeg.exe") : Path.Combine("yt-dlp", "ffmpeg");

            if (!Directory.Exists("Download")) Directory.CreateDirectory("Download");

            _ytdl = new()
            {
                YoutubeDLPath = _ytDlpScriptPath,
                FFmpegPath = _ffmpegScriptPath,
                OutputFolder = "Download"
            };

            _download = new(_ytdl);

        }

        public async Task StartBot()
        {
            _botClient.OnMessage += OnMessage;
            var me = await _botClient.GetMe();

            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            Console.ReadLine();

            _cts.Cancel();
        }

        private async Task OnMessage(Message msg, UpdateType type)
        {
            try
            {
                if (msg.Text is null) return;   // we only handle Text messages here
                Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");
                // let's echo back received text in the chat

                var message = msg.Text;
                var args = message.Split(' ');
                if (args.Count() > 0)
                {
                    if (args[0].Contains("download") && args.Length > 1)
                    {
                        string url = args[1];
                        if (string.IsNullOrEmpty(url))
                        {
                            await _botClient.SendMessage(msg.Chat.Id, $"Указан не верный url");
                            return;
                        }

                        switch (args[0])
                        {
                            case "/download_video":

                                await DownloadVideo(msg, url);

                                break;
                            case "/download_audio":
                                await DownloadAudio(msg, url);
                                break;
                        }
                    }
                    else
                    {
                        if (args[0].Contains("youtube.com"))
                        {
                            await DownloadAudio(msg, args[0]);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке тг: {ex.Message}");
            }

            //await _botClient.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
        }

        private async Task DownloadVideo(Message msg, string url)
        {
            await _botClient.SendMessage(msg.Chat.Id, $"Скачивание видео: {url}");

            var resVideo = await _download.DownloadVideo(url, options);
            if (resVideo.Success)
            {
                string videoPath = resVideo.Data;
                Util.Log($"Видео установлено: {videoPath}");

                using (var stream = new FileStream(resVideo.Data, FileMode.Open, FileAccess.Read))
                {
                    await _botClient.SendVideo(msg.Chat.Id, InputFile.FromStream(stream));
                    Util.Log($"Видео отправлено: {msg.Chat.Id}");
                }

                try
                {
                    File.Delete(videoPath);
                }
                catch
                {
                    Util.Log($"Ошибка при удалении файла: {videoPath}");
                }

            }
            else
            {
                await _botClient.SendMessage(msg.Chat.Id, "Ошибка при скачивании видео");
                foreach (var error in resVideo.ErrorOutput)
                {
                    Console.WriteLine(error);
                }
            }
        }

        private async Task DownloadAudio(Message msg, string url)
        {
            await _botClient.SendMessage(msg.Chat.Id, $"Скачивание аудио: {url}");
            var resAudio = await _download.DownloadAudio(url, options);
            if (resAudio.Success)
            {
                var videoInfo = await _ytdl.RunVideoDataFetch(url);
                string videoPath = resAudio.Data;
                Util.Log($"Аудио установлено: {videoPath}");

                using (var stream = new FileStream(resAudio.Data, FileMode.Open, FileAccess.Read))
                {
                    var inputFile = InputFile.FromStream(stream, $"{videoInfo.Data.AltTitle}.mp3");
                    await _botClient.SendAudio(msg.Chat.Id, inputFile);
                    Util.Log($"Аудио отправлено: {msg.Chat.Id}");
                }

                try
                {
                    File.Delete(videoPath);
                }
                catch
                {
                    Util.Log($"Ошибка при удалении файла: {videoPath}");
                }
            }
            else
            {
                await _botClient.SendMessage(msg.Chat.Id, "Ошибка при скачивании аудио");
                foreach (var error in resAudio.ErrorOutput)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
