using DSharpPlus.Entities;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace DiscordBot.Interactivity.TwitchEvent
{
    public class TwitchEmbed
    {
        public static DiscordMessageBuilder GenerateEmbed(OnStreamOnlineArgs e)
        {
            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "🟢  -  LIVE"
            };

            var author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = e.Stream.UserName,
                IconUrl = $"https://avatar-resolver.vercel.app/twitch/{e.Stream.UserName}",
                Url = $"https://www.twitch.tv/{e.Stream.UserName}"
            };

            var gameboxArt = $"https://static-cdn.jtvnw.net/ttv-boxart/{e.Stream.GameId}-144x192.jpg";

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = gameboxArt
            };
            
            var color = new DiscordColor(145, 70, 255);

            var embed = new DiscordEmbedBuilder()
            {
                Author = author,
                Title = e.Stream.Title,
                Thumbnail = thumbnail,
                Description = $"`{e.Stream.UserName}` is playing/streaming in the category: `{e.Stream.GameName}`",
                Color = color,
                Footer = footer,
            };

            var msg = new DiscordMessageBuilder()
                .WithContent($"{e.Stream.UserName} iniciou a stream!")
                .WithEmbed(embed);

            return msg;
        }
    }
}