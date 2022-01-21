using DSharpPlus.Entities;
using Newtonsoft.Json;
using TwitchLib.Api;

namespace DiscordBot.Commands.Embed.Twitch
{
    public class Twitch
    {
        private readonly TwitchAPI twitchAPI;
        public Twitch()
        {
            twitchAPI = new TwitchAPI();

            var strJson = string.Empty;
            using ( var fs = File.OpenRead("files/twitchkeys.json") )
            using ( var sr = new StreamReader(fs, new System.Text.UTF8Encoding(false) ) )
            {
                strJson = sr.ReadToEnd();
            }

            var cfgJson = JsonConvert.DeserializeObject<TwitchJson>(strJson);

            twitchAPI.Settings.ClientId = cfgJson.ClientId;
            twitchAPI.Settings.AccessToken = cfgJson.AccessToken;
        }

        private async Task<TwitchLib.Api.Helix.Models.Search.Channel?> GetChannelAsync(string twitchChannel)
        {
            var channel = await twitchAPI.Helix.Search.SearchChannelsAsync(twitchChannel, false, null, 1);
            var streamer = channel.Channels.FirstOrDefault();

            return streamer;
        }

        private async Task<DiscordMessageBuilder> GetStreamEmbedOff(string twitchChannel)
        {
            List<string> login = new(1) { twitchChannel.ToLower() };
            var userList = await twitchAPI.Helix.Users.GetUsersAsync(null, login);
            var user = userList.Users.First();

            var author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = user.DisplayName,
                Url = $"https://www.twitch.tv/{user.DisplayName}"
            };

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Height = 64,
                Width = 64,
                Url = user.ProfileImageUrl
            };

            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "🔴  -  not LIVE"
            };

            var embed = new DiscordEmbedBuilder() 
            {
                Author = author,
                Title = $"`{user.DisplayName}` is not LIVE on Twitch",
                Thumbnail = thumbnail,
                Color = DiscordColor.Red,
                Footer = footer,
            };

            var msg = new DiscordMessageBuilder()
                .WithEmbed(embed);

            return msg;
        }

        private async Task<DiscordMessageBuilder> StreamNull(string TypeNull)
        {
            await Task.Yield();
            
            var msg = new DiscordMessageBuilder().WithContent("Stream não foi encontrada");

            if (TypeNull == "stream")
            {
                msg.Content = "Stream não foi encontrada";
                return msg;
            }
            else if (TypeNull == "user")
            {
                msg.Content = "User não foi encontrado";
                return msg;
            }
            else
            {
                msg.Content = "StreamNull parâmetro inválido";
                return msg;
            }
        }

        public async Task<DiscordMessageBuilder> GetStreamEmbed(string twitchChannel)
        {
            var channel = await GetChannelAsync(twitchChannel);

            if (channel is null) 
            { 
                var msgnull = new DiscordMessageBuilder().WithContent("Usuário não encontrado!"); 
                return msgnull; 
            }
            
            if (channel.IsLive is false) { return await GetStreamEmbedOff(twitchChannel); }

            List<string> login = new(1) { twitchChannel.ToLower() };

            var streamList = await twitchAPI.Helix.Streams.GetStreamsAsync(null, null, 1, null, null, "all", null, login);
            if (streamList.Streams == null || streamList.Streams.Length == 0) { return await StreamNull("stream"); }

            var userList = await twitchAPI.Helix.Users.GetUsersAsync(null, login);
            if (userList.Users == null || userList.Users.Length == 0) { return await StreamNull("user"); }

            var stream = streamList.Streams.First();
            var user = userList.Users.First();

            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "🟢  -  LIVE"
            };

            var author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = user.DisplayName,
                IconUrl = user.ProfileImageUrl,
                Url = $"https://www.twitch.tv/{user.DisplayName}"
            };

            var thumburl = stream.ThumbnailUrl.Replace("{width}", "512").Replace("{height}", "256");
            var gameboxArt = $"https://static-cdn.jtvnw.net/ttv-boxart/{stream.GameId}-144x192.jpg";

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = gameboxArt
            };

            var color = new DiscordColor(145, 70, 255);

            var embed = new DiscordEmbedBuilder() 
            {
                Author = author,
                Title = stream.Title,
                Thumbnail = thumbnail,
                Description = $"`{stream.UserName}` is playing/streaming in the category: `{stream.GameName}`",
                ImageUrl = thumburl,
                Color = color,
                Footer = footer,
            };

            var msg = new DiscordMessageBuilder()
                .WithEmbed(embed);

            return msg;
        }

    }

    public struct TwitchJson
    {
        [JsonProperty("clientId")]
        public string ClientId {get; private set;}

        [JsonProperty("accessToken")]
        public string AccessToken {get; private set;}

    }
}