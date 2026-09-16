using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace YoutubeDownloaderTgBot.Services
{
    public class DownloadService
    {
        private YoutubeDL _ytdl;

        public DownloadService(YoutubeDL ytdl)
        {
            _ytdl = ytdl;
        }

        public async Task<RunResult<string>> DownloadVideo(string url, OptionSet options)
        {
            var videoInfo = await _ytdl.RunVideoDataFetch(url);
            var resVideo = await _ytdl.RunVideoDownload(url, mergeFormat: DownloadMergeFormat.Mp4, overrideOptions: options);

            return resVideo;
        }

        public async Task<RunResult<string>> DownloadAudio(string url, OptionSet options)
        {
            var resAudio = await _ytdl.RunAudioDownload(url, format: AudioConversionFormat.Mp3, overrideOptions: options);

            return resAudio;
        }
    }
}
