using HtmlAgilityPack;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;


namespace DiscordBot.Commands.Embed.Twitter
{
    public class ModuleTwitter : BaseCommandModule
    {
        [Command("twitteruser")]
        public async Task TwitterCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            string path = $"files/data/guilds/{ctx.Guild.Id}";

            try
            {
                string twtUrl = $"https://twitter.com/{strUser}";
                
                using ( var client = new HttpClient() )
                {
                    using ( var response = await client.GetAsync(twtUrl) )
                    {
                        response.EnsureSuccessStatusCode();

                        var doc = await response.Content.ReadAsStringAsync();
                        //response.Dispose();

                        File.WriteAllText($"{path}/twitter.html", doc);

                        var html = new HtmlDocument();
                        html.Load($"{path}/twitter.html");

                        try
                        {
                            var profileavatar = html.DocumentNode.SelectSingleNode("//div[@class='ProfileAvatar']//a//img").GetAttributeValue("src", "");
                            var screename = html.DocumentNode.SelectSingleNode("//head/title").InnerText;
                            var atname = $"@{strUser}";
                            screename = screename.Remove(screename.IndexOf(" ("));
                            var tweet = html.DocumentNode.SelectSingleNode("//div[@class='js-tweet-text-container']//p").InnerText;

                            if (html.DocumentNode.SelectSingleNode("//div[contains(@class,'AdaptiveMedia-video')]") != null)
                            {
                                var media = html.DocumentNode.SelectSingleNode("//div[contains(@class,'PlayableMedia-player')]").GetAttributes("style", "");
                                var mediaUrl = media.First().Value;
                                mediaUrl = mediaUrl.Substring(mediaUrl.IndexOf("https://"));

                                // mediaUrl refuses to get the last index.
                                mediaUrl = mediaUrl.Remove(mediaUrl.LastIndexOf(@".jpg"));
                                mediaUrl += ".jpg";

                                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                {
                                    Height = 48,
                                    Width = 48,
                                    Url = profileavatar
                                };

                                var embed = new DiscordEmbedBuilder()
                                {
                                    Title = atname,
                                    Description = tweet,
                                    Thumbnail = thumbnail,
                                    ImageUrl = mediaUrl,
                                    Url = $"https://www.twitter.com/{strUser}"
                                };

                                var msg = await new DiscordMessageBuilder()
                                    .WithEmbed(embed)
                                    .WithReply(ctx.Member.Id, true)
                                    .SendAsync(ctx.Channel);   
                            }
                            else if (html.DocumentNode.SelectSingleNode("//div[contains(@class,'AdaptiveMedia-singlePhoto')]") != null)
                            {

                                var media = html.DocumentNode.SelectSingleNode("//div[contains(@class,'AdaptiveMedia-photoContainer js-adaptive-photo')]").GetAttributeValue("data-image-url", "");
                                
                                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                {
                                    Height = 48,
                                    Width = 48,
                                    Url = profileavatar
                                };

                                var embed = new DiscordEmbedBuilder()
                                {
                                    Title = atname,
                                    Description = tweet,
                                    Thumbnail = thumbnail,
                                    ImageUrl = media,
                                    Url = $"https://www.twitter.com/{strUser}"
                                };

                                var msg = await new DiscordMessageBuilder()
                                    .WithEmbed(embed)
                                    .WithReply(ctx.Member.Id, true)
                                    .SendAsync(ctx.Channel);
                            }
                            else
                            {
                                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                {
                                    Height = 48,
                                    Width = 48,
                                    Url = profileavatar
                                };

                                var embed = new DiscordEmbedBuilder()
                                {
                                    Title = atname,
                                    Description = tweet,
                                    Thumbnail = thumbnail,
                                    Url = $"https://www.twitter.com/{strUser}"
                                };

                                var msg = await new DiscordMessageBuilder()
                                    .WithEmbed(embed)
                                    .WithReply(ctx.Member.Id, true)
                                    .SendAsync(ctx.Channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText("files/dump.txt", ex.StackTrace);   
                        }

                    }
                }
            }
            catch (Exception)
            {
                await ctx.RespondAsync("Usuario nao encontrado");
            }
        }
    }
}