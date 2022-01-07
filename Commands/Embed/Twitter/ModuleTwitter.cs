using Tweetinvi;
using HtmlAgilityPack;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tweetinvi.Parameters.V2;
using Tweetinvi.Parameters;

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

        [Command("twitter")]
        public async Task FetchUserCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            string[] keys = File.ReadAllLines("files/twitterkey.txt");

            string message = string.Empty;

            try
            {
                var userClient = new TwitterClient(keys[0], keys[1], keys[2], keys[3]);
                var client = new TwitterClient(keys[0], keys[1], keys[4]);
                userClient.Config.TweetMode = TweetMode.Extended;

                var user = await userClient.UsersV2.GetUserByNameAsync(new GetUserByNameV2Parameters(strUser)
                {
                    Expansions =
                    {
                        UserResponseFields.Expansions.PinnedTweetId,
                    },
                    UserFields =
                    {
                        UserResponseFields.User.Description,
                        UserResponseFields.User.Entities,
                        UserResponseFields.User.Name,
                        UserResponseFields.User.ProfileImageUrl,
                        UserResponseFields.User.PinnedTweetId,
                    }
                });


                var timeline = await client.Timelines.GetUserTimelineAsync(new GetUserTimelineParameters(strUser)
                {
                    IncludeEntities = true,
                    PageSize = 5,
                    IncludeRetweets = true,

                }); 

                try
                {
                    if (timeline.First().Media != null)
                    {
                        var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Height = 48,
                            Width = 48,
                            Url = user.User.ProfileImageUrl
                        };

                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = strUser,
                            Description = timeline.First().Text,
                            Thumbnail = thumbnail,
                            Color = DiscordColor.Blue,
                            ImageUrl = timeline.First().Media.First().MediaURL,
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
                            Url = user.User.ProfileImageUrl
                        };

                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = strUser,
                            Description = timeline.First().Text,
                            Thumbnail = thumbnail,
                            Color = DiscordColor.Blue,
                            Url = $"https://www.twitter.com/{strUser}"
                        };

                        var msg = await new DiscordMessageBuilder()
                            .WithEmbed(embed)
                            .WithReply(ctx.Member.Id, true)
                            .SendAsync(ctx.Channel);
                    }

                    //await ctx.RespondAsync(message);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync(ex.Message + ex.StackTrace);
                }
            }
            catch (Tweetinvi.Exceptions.TwitterException ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
            



        }
    }

}