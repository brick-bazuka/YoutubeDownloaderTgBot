
using Newtonsoft.Json;
using YoutubeDownloaderTgBot.Core;
using YoutubeDownloaderTgBot.Models;

namespace YoutubeDownloaderTgBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Config config = null;
            var cfgJson = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            if (!Path.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
            }
            var cfgPath = Path.Combine("Config", "config.json");
            if (!File.Exists(cfgPath))
            {
                File.WriteAllText(cfgPath, cfgJson);
                Console.WriteLine($"The config needs to be filled out {cfgPath}");
                return;
            }
            else
            {
                cfgJson = File.ReadAllText(cfgPath);
                config = JsonConvert.DeserializeObject<Config>(cfgJson);
            }

            if(config != null)
            {
                if(config.BotToken != null)
                {
                    TgBot bot = new(config.BotToken);
                    await bot.StartBot();

                }
                else
                {
                    Console.WriteLine("Telegram bot token not specified");
                    return;
                }
            }



        }
    }
}