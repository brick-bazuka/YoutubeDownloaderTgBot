

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
            if (msg.Text is null) return;   // we only handle Text messages here
            Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");
            // let's echo back received text in the chat

            var message = msg.Text;

            switch (message)
            {
                case "/download_video":
                    var args = message.Split(' ');
                    if(args.Length > 1)
                    {
                        string url = args[1];

                        var videoInfo = await _ytdl.RunVideoDataFetch(url);
                        var resVideo = await _ytdl.RunVideoDownload(url, mergeFormat: DownloadMergeFormat.Mp4);
                        if (resVideo.Success)
                        {
                            string videoPath = resVideo.Data;
                            
                            using(var stream = new FileStream(resVideo.Data, FileMode.Open, FileAccess.Read))
                            {
                                await _botClient.SendVideo(msg.Chat.Id, InputFile.FromStream(stream));
                            }
                            
                        }
                    }

                    

                    break;
            }



            //await _botClient.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
        }
    }
}
