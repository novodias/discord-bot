using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    } 

    static async Task MainAsync()
    {
        var json = string.Empty;
        using (var fs = File.OpenRead("files/config.json"))
        using (var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)))
            json = await sr.ReadToEndAsync();

        var cfg = JsonConvert.DeserializeObject<ConfigJson>(json);

        var discord = new DiscordClient(new DiscordConfiguration() 
        {
            MinimumLogLevel = LogLevel.Debug,
            LogTimestampFormat = "dd MM yyyy - hh:mm:ss tt",
            Token = cfg.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
        });

        var commands = discord.UseCommandsNext(new CommandsNextConfiguration() 
        {
            StringPrefixes = new[] { cfg.CommandPrefix }
        });

        commands.RegisterCommands<DiscordBot.Commands.Module>();
        commands.RegisterCommands<DiscordBot.Commands.Gifs.ModuleGifs>();
        commands.RegisterCommands<DiscordBot.Commands.Embed.ModuleEmbeds>();

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token {get; private set;}

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }

}
