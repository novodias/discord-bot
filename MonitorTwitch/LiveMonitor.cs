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
        private readonly TwitchAPI? _api;
        private readonly DiscordClient? _client;
        private List<string> _ids;
        // private FileSystemWatcher _watcher;
        // private CancellationTokenSource _cts;
        // private CancellationToken _ct;
        public LiveMonitor(DiscordClient client, TwitchAPI API, List<string> list)
        {
            _api = API;
            _client = client;
            _ids = list;
            // _cts = new();
            // _ct = _cts.Token;

            // _watcher = new FileSystemWatcher(@"files/")
            // {
            //     NotifyFilter = NotifyFilters.LastWrite,
            //     Filter = "channels.json",
            //     EnableRaisingEvents = true,
            // };

            // MEMORY LEAK!!!
            // _watcher.Changed += OnChanged;
            // _watcher.Created += OnCreated;

            // Task.Run( () => ConfigLiveMonitorAsync(_ids), _ct );

            Task.Run( () => ConfigLiveMonitorAsync(_ids) );
        }

        public LiveMonitor(List<string> list)
        {
            _ids = list;

            // if (_ct.CanBeCanceled && _cts is not null)
            // {
            //     _cts.Cancel();
            //     _cts.Dispose();
            //     _cts = new();
            //     _ct = _cts.Token;
            // }

            if (_monitor is not null)
            {
                if (_monitor.Enabled)
                {
                    _monitor.Stop();
                }
                _monitor.OnStreamOnline -= Monitor_OnStreamOnline;
            }

            // Task.Run( () => ConfigLiveMonitorAsync(_ids), _ct);

            Task.Run( () => ConfigLiveMonitorAsync(_ids) );
        }

        // public async static Task<JsonChannels> GetJson()
        // {
        //     var strJson = string.Empty;

        //     using ( var fs = File.OpenRead("files/channels.json"))
        //     {
        //         using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
        //         {
        //             strJson = await sr.ReadToEndAsync();

        //             sr.Dispose();
        //             await fs.DisposeAsync();
        //         }
        //     }

        //     var list = JsonConvert.DeserializeObject<JsonChannels>(strJson) ?? 
        //         throw new Exception("list OnChanged is null");

        //     return list;
        // }

        // private async void OnChanged(object sender, FileSystemEventArgs e)
        // {
        //     if (e.ChangeType != WatcherChangeTypes.Changed)
        //     {
        //         return;
        //     }

        //     if (_ct.CanBeCanceled)
        //     {
        //         _cts.Cancel();
        //     }
        //     _cts.Dispose();   
        //     _cts = new();
        //     _ct = _cts.Token;

        //     try
        //     {
                
        //         if (_monitor is null) { throw new Exception("_monitor can't be null"); }
        //         _monitor.OnStreamOnline -= Monitor_OnStreamOnline;
                
        //         var strJson = string.Empty;

        //         var list = await GetJson();

        //         // if (list.Channels.First() == string.Empty)
        //         //     list.Channels.RemoveAt(0);

        //         _ids = list.Channels;
                
        //         await Task.Run( () => ConfigLiveMonitorAsync(_ids), _ct );
        //     }
        //     catch (Exception ex)
        //     {
        //         File.AppendAllText("files/changed.txt", ex.Message + ex.StackTrace);
        //     }
        // }

        // private void OnCreated(object sender, FileSystemEventArgs e)
        // {
        //     var strJson = string.Empty;
            
        //     var fs = File.Open("files/channels.json", FileMode.Open, FileAccess.Read);
        //     using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
        //     {
        //         strJson = sr.ReadToEnd();

        //         sr.Dispose();
        //         fs.Dispose();
        //     }

        //     var list = JsonConvert.DeserializeObject<JsonChannels>(strJson) ?? 
        //         throw new Exception("list OnChanged is null");

        //     if (list.Channels.First() == string.Empty)
        //         list.Channels.RemoveAt(0);

        //     File.AppendAllText("files/created.txt", "criou");

        //     if (_monitor is null) { throw new Exception("_monitor can't be null"); }
        //     _monitor.SetChannelsByName(list.Channels);
        // }

        // public void UpdateList(List<string> list)
        // {
        //     if (_ids.Equals(list))
        //         return;
            
        //     _ids = list;

        //     _monitor.SetChannelsByName(_ids);

        //     Task.Run( () => ConfigLiveMonitorAsync(_ids) );
        // }

        private async Task ConfigLiveMonitorAsync(List<string> ids)
        {         
            if (_api is null) { throw new Exception("twitch api is null"); }
            if (_client is null) { throw new Exception("discord client is null"); }
            
            _monitor = new LiveStreamMonitorService(_api, 60);
            
            _monitor.SetChannelsByName(ids);

            _monitor.OnStreamOnline += Monitor_OnStreamOnline;

            _monitor.Start();

            // _monitor.OnStreamOffline += Monitor_OnStreamOffline;
            // _monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
            // _monitor.OnServiceStarted += Monitor_OnServiceStarted;
            // _monitor.OnChannelsSet += Monitor_OnChannelsSet;

            // while (!_ct.IsCancellationRequested)
            // {
            //     _ct.ThrowIfCancellationRequested();
            //     await Task.Delay(-1);
            // }

            await Task.Delay(-1);
        }

        private async void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {

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

            if (queryMatchingFiles is null) { return; }
            
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

        // public void Dispose()
        // {
        //     this._monitor.OnStreamOnline -= Monitor_OnStreamOnline;
        //     this._monitor.Stop();
        //     this._api = null;
        //     this._client = null;
        //     this._ids = null;
        //     this._monitor = null;
        // }

        // private void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        // {
        //     throw new NotImplementedException();
        // }

        // private void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        // {
        //     throw new NotImplementedException();
        // }

        // private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)       
        // {
        //     throw new NotImplementedException();
        // }

        // private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        // {
        //     throw new NotImplementedException();
        // }
    }
}