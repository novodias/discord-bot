using DiscordBot.Interactivity.TwitchEvent;
using DSharpPlus;
using DSharpPlus.Entities;
using TwitchLib.Api;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using Newtonsoft.Json;

namespace DiscordBot.MonitorTwitch
{
    public class LiveMonitor
    {
        private LiveStreamMonitorService? _monitor;
        private TwitchAPI? _api;
        private readonly DiscordClient? _client;
        private CancellationTokenSource _cts;
        private CancellationToken _ct;
        private readonly FileSystemWatcher watcher;
        private HashSet<string> _streams;
        public LiveMonitor(DiscordClient client, TwitchAPI API, List<string> list)
        {
            _api = API;
            _client = client;
            _streams = new();
            _cts = new();
            _ct = _cts.Token;

            watcher = new FileSystemWatcher(@"files/")
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
                Filter = "channels.json"
            };

            watcher.Changed += OnChanged;

            Task.Run( () => ConfigLiveMonitorAsync(list) );
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            try
            {
                CancelToken();

                var strJson = string.Empty;

                await JsonChannels.WaitTasks();
                using ( var fs = File.OpenRead("files/channels.json"))
                {
                    using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
                    {
                        strJson = await sr.ReadToEndAsync();

                        sr.Dispose();
                        await fs.DisposeAsync();
                    }
                }

                var list = JsonConvert.DeserializeObject<JsonChannels>(strJson) ?? 
                    throw new Exception("channels.json is null?");

                // if (list.Channels.First() == string.Empty)
                //     list.Channels.RemoveAt(0);
                // if ( this.Client is null || this.api is null) 
                    // throw new Exception("Not possible to ctor LiveMonitor because DiscordClient or TwitchClient is null");

                // this.live = new(this.Client, this.api, list.Channels);
                if (_monitor is not null)
                {
                    _monitor.OnStreamOnline -= Monitor_OnStreamOnline;
                    _monitor.OnStreamOffline -= Monitor_OnStreamOffline;
                }
                
                await Task.Run( () => ConfigLiveMonitorAsync(list.Channels) );
            }
            catch (Exception ex)
            {
                File.AppendAllText("files/changed.txt", ex.Message + ex.StackTrace);
            }
        }

        public void CancelToken()
        {
            try
            {
                _cts.Cancel();
            }
            finally
            {
                _cts.Dispose();    
            }

            _cts = new();
            _ct = _cts.Token;
        }

        private async Task ConfigLiveMonitorAsync(List<string> ids)
        {         
            if (_api is null) { throw new Exception("twitch api is null"); }
            if (_client is null) { throw new Exception("discord client is null"); }
            
            _monitor = new LiveStreamMonitorService(_api, 60);
            
            _monitor.SetChannelsByName(ids);

            _monitor.OnStreamOnline += Monitor_OnStreamOnline;
            _monitor.OnStreamOffline += Monitor_OnStreamOffline;

            _monitor.Start();

            try
            {
                await Task.Delay(-1, _ct);
            }
            catch (TaskCanceledException) { }
        }

        private async void Monitor_OnStreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            if (_streams.Contains(e.Stream.UserName.ToLower())) return;

            _streams.Add(e.Stream.UserName.ToLower());

            string startFolder = @"files/data/guilds";

            DirectoryInfo dir = new(startFolder);

            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            string searchTerm = e.Stream.UserName.ToLower();

            var queryMatchingFiles =  
                from file in fileList  
                where file.Name == "twitchchannels.json"  
                let fileText = GetFileText(file.FullName)  
                where fileText.Contains(searchTerm)  
                select file.Directory.Name; 

            if ( queryMatchingFiles is null ) { return; }
            
            List<ulong> gldsId = new( queryMatchingFiles.Count() );

            foreach (var item in queryMatchingFiles)
            {
                gldsId.Add(Convert.ToUInt64(item));
            }

            if (gldsId is null) { return; }

            foreach (var id in gldsId)
            {
                await SendDSMessage(id, e);
            }
        }

        private void Monitor_OnStreamOffline(object? sender, OnStreamOfflineArgs e)
        {
            if (_streams.Contains(e.Stream.UserName.ToLower()))
            {
                _streams.Remove(e.Stream.UserName.ToLower());
            }
        }

        private Task SendDSMessage(ulong id, OnStreamOnlineArgs e)
        {
            _ = Task.Run(async () => 
            {
                if (_client is null) { throw new Exception("_client is null"); }
                var gld = await _client.GetGuildAsync(id);

                var chn = gld.Channels.Single(x => x.Value.Name == "twitch");
                var embed = TwitchEmbed.GenerateEmbed(e);
                await chn.Value.SendMessageAsync(embed);
            });

            return Task.CompletedTask;
        }

        private static string GetFileText(string name) 
        {  
            string fileContents = String.Empty;  
 
            if (File.Exists(name))  
            {  
                fileContents = File.ReadAllText(name);  
            }  
            return fileContents;  
        }

        public void Dispose()
        {
            if (this._monitor != null && this._client != null && this._api != null)
            {
                this._monitor.OnStreamOnline -= Monitor_OnStreamOnline;
                this._monitor = null;
                this._api = null;
            }
        }

    }
}