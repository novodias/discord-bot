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
        [Command("twitter")]
        public async Task FetchUserCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            await ctx.TriggerTypingAsync();

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
                    if (timeline.FirstOrDefault().Media.FirstOrDefault() != null)
                    {
                        var author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            IconUrl = user.User.ProfileImageUrl,
                            Name = $"@{strUser}",
                            Url = $"https://www.twitter.com/{strUser}"
                        };

                        var footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = "amogus",
                            IconUrl = "https://i.imgur.com/82HZ341.png"
                        };

                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = strUser,
                            Description = timeline.First().Text,
                            Color = DiscordColor.Aquamarine,
                            ImageUrl = timeline.First().Media.First().MediaURLHttps,
                            Timestamp = timeline.First().CreatedAt,
                            Author = author,
                            Footer = footer
                        };

                        var msg = await new DiscordMessageBuilder()
                            .WithEmbed(embed)
                            .WithReply(ctx.Member.Id, true)
                            .SendAsync(ctx.Channel);
                    }
                    else
                    {
                        var author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            IconUrl = user.User.ProfileImageUrl,
                            Name = $"@{strUser}",
                            Url = $"https://www.twitter.com/{strUser}"
                        };

                        var footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = "amogus",
                            IconUrl = "https://i.imgur.com/82HZ341.png"
                        };

                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = user.User.Name,
                            Description = timeline.First().Text,
                            Color = DiscordColor.Aquamarine,
                            Timestamp = timeline.First().CreatedAt,
                            Author = author,
                            Footer = footer
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