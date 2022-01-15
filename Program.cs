using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DSharpPlus.EventArgs;

class Program
{
    public readonly EventId BotEventId = new(42, "Bot");

    public DiscordClient? Client { get; set; }
    public InteractivityExtension? Interactivity { get; set; }
    public CommandsNextExtension? Commands { get; set; }
    public static void Main()
    {
        var prog = new Program();
        prog.MainAsync().GetAwaiter().GetResult();
    } 

    public async Task MainAsync()
    {
        var json = string.Empty;
        using (var fs = File.OpenRead("files/config.json"))
        using (var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false)))
            json = await sr.ReadToEndAsync();

        var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

        var cfg = new DiscordConfiguration
        {
            Token = cfgjson.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
            MinimumLogLevel = LogLevel.Debug
        };

        this.Client = new DiscordClient(cfg);

        this.Client.Ready += this.Client_Ready;
        this.Client.GuildAvailable += this.Client_GuildAvailable;
        this.Client.ClientErrored += this.Client_ClientError;

        this.Client.UseInteractivity(new InteractivityConfiguration()
        {
            PaginationBehaviour = PaginationBehaviour.Ignore,
            PaginationDeletion = PaginationDeletion.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(15)
        });

        var ccfg = new CommandsNextConfiguration() 
        {
            StringPrefixes = new[] { cfgjson.CommandPrefix }
        };

        this.Commands = this.Client.UseCommandsNext(ccfg);

        this.Commands.RegisterCommands<DiscordBot.Commands.Module>();
        this.Commands.RegisterCommands<DiscordBot.Commands.Gifs.ModuleGifs>();
        this.Commands.RegisterCommands<DiscordBot.Commands.Embed.ModuleEmbeds>();
        this.Commands.RegisterCommands<DiscordBot.Commands.Images.ModuleImages>();
        this.Commands.RegisterCommands<DiscordBot.Commands.Embed.Twitter.ModuleTwitter>();

        await this.Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
    {
        sender.Logger.LogInformation(BotEventId, "Client is ready to process events");

        return Task.CompletedTask;
    }

    private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

        return Task.CompletedTask;
    }

    private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

        return Task.CompletedTask;
    }

    private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

        return Task.CompletedTask;
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token {get; private set;}

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }

}
