using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DiscordBot.MonitorTwitch;
using TwitchLib.Api;
using DiscordBot.Commands.Embed.Twitch;

class Program
{
    public readonly EventId BotEventId = new(42, "Bot");

    public DiscordClient? Client { get; set; }
    public InteractivityExtension? Interactivity { get; set; }
    public CommandsNextExtension? Commands { get; set; }
    public LiveMonitor? live {get; set;}
    public TwitchAPI? api {get; set;}
    private FileSystemWatcher watcher {get; set;}
    public static void Main()
    {
        var prog = new Program();
        prog.MainAsync().GetAwaiter().GetResult();
    } 

    public async Task MainAsync()
    {
        var json = string.Empty;
        using (var fsd = File.OpenRead("files/config.json"))
        using (var sr = new StreamReader(fsd, new System.Text.UTF8Encoding(false)))
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
        this.Commands.RegisterCommands<DiscordBot.Commands.Embed.Twitch.ModuleTwitch>();
        this.Commands.RegisterCommands<DiscordBot.Commands.Embed.Twitter.ModuleTwitter>();

        this.api = new();

        var strJson = string.Empty;
        using ( var fst = File.OpenRead("files/twitchkeys.json") )
        using ( var sr = new StreamReader(fst, new System.Text.UTF8Encoding(false) ) )
        {
            strJson = sr.ReadToEnd();
        }

        var cfgJson = JsonConvert.DeserializeObject<TwitchJson>(strJson);

        this.api.Settings.ClientId = cfgJson.ClientId;
        this.api.Settings.AccessToken = cfgJson.AccessToken;

        // --------------------------------------------------------------

        if (!File.Exists("files/channels.json"))
        {
            var chns = new TwitchChannels();

            chns.Channels.Add("twitch");

            strJson = JsonConvert.SerializeObject(chns);

            using ( var fs = File.Open("files/channels.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using ( var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(false) ) )
                {
                    await sw.WriteLineAsync(strJson);
                    await sw.DisposeAsync();
                    fs.Dispose();
                    fs.Close();
                }
            }

            this.live = new LiveMonitor(this.Client, api, chns.Channels);
        }
        else
        {
            using ( var fs = File.Open("files/channels.json", FileMode.Open, FileAccess.Read))
            {
                using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
                {
                    strJson = sr.ReadToEnd();

                    sr.Dispose();
                    fs.Dispose();
                    fs.Close();
                }
            }

            var list = JsonConvert.DeserializeObject<TwitchChannels>(strJson);

            if (list is null)
            {
                list = new();
            }

            this.live = new LiveMonitor(this.Client, api, list.Channels);
        }

        // await SetWatcherHotLoad();

        await this.Client.ConnectAsync();
        await Task.Delay(-1);
    }

    // private async void OnChanged(object sender, FileSystemEventArgs e)
    // {
    //     if (e.ChangeType != WatcherChangeTypes.Changed)
    //     {
    //         return;
    //     }

    //     try
    //     {
            
    //         await Task.Delay(TimeSpan.FromSeconds(5));

    //         if ( this.live is not null ) 
    //         {
    //             this.live.CancelToken();
    //             this.live.Dispose();
    //         }

    //         var strJson = string.Empty;

    //         using ( var fs = File.OpenRead("files/channels.json"))
    //         {
    //             using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
    //             {
    //                 strJson = await sr.ReadToEndAsync();

    //                 sr.Dispose();
    //                 await fs.DisposeAsync();
    //             }
    //         }

    //         var list = JsonConvert.DeserializeObject<TwitchChannels>(strJson) ?? 
    //             throw new Exception("channels.json is null?");

    //         // if (list.Channels.First() == string.Empty)
    //         //     list.Channels.RemoveAt(0);
    //         if ( this.Client is null || this.api is null) 
    //             throw new Exception("Not possible to ctor LiveMonitor because DiscordClient or TwitchClient is null");

    //         this.live = new(this.Client, this.api, list.Channels);
    //     }
    //     catch (Exception ex)
    //     {
    //         File.AppendAllText("files/changed.txt", ex.Message + ex.StackTrace);
    //     }
    // }

    // private Task SetWatcherHotLoad()
    // {
    //     _ = Task.Run( () => 
    //     {
    //         watcher = new FileSystemWatcher(@"files/")
    //         {
    //             NotifyFilter = NotifyFilters.LastWrite,
    //             EnableRaisingEvents = true,
    //             Filter = "channels.json"
    //         };

    //         watcher.Changed += OnChanged;
    //     });
    //     return Task.CompletedTask;
    // }

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

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token {get; private set;}

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }
}
