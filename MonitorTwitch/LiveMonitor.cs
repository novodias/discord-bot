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
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _ct;
        public LiveMonitor(DiscordClient client, TwitchAPI API, List<string> list)
        {
            _api = API;
            _client = client;
            _cts = new();
            _ct = _cts.Token;

            Task.Run( () => ConfigLiveMonitorAsync(list) );
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
        }

        private async Task ConfigLiveMonitorAsync(List<string> ids)
        {         
            if (_api is null) { throw new Exception("twitch api is null"); }
            if (_client is null) { throw new Exception("discord client is null"); }
            
            _monitor = new LiveStreamMonitorService(_api, 60);
            
            _monitor.SetChannelsByName(ids);

            _monitor.OnStreamOnline += Monitor_OnStreamOnline;

            _monitor.Start();

            try
            {
                await Task.Delay(-1, _ct);
            }
            catch (TaskCanceledException) { }
        }

        private async void Monitor_OnStreamOnline(object? sender, OnStreamOnlineArgs e)
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