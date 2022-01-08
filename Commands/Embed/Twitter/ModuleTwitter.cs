using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Parameters.V2;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class ModuleTwitter : BaseCommandModule
    {
        [Command("twitter")]
        public async Task FetchUserCommand(CommandContext ctx, [RemainingText] string strUser)
        {
            await ctx.TriggerTypingAsync();

            var interactivity = ctx.Client.GetInteractivity();

            string[] keys = File.ReadAllLines("files/twitterkey.txt");

            //string message = string.Empty;

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
                    // var tweets = timeline.OrderByDescending(x => x.CreatedAt <= DateTime.Now);
                    List<DiscordEmbed> embeds = new(); 
                    foreach (var tweet in timeline)
                    {
                        if (tweet.Media.FirstOrDefault() != null)
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
                                Description = tweet.Text,
                                Color = DiscordColor.Aquamarine,
                                ImageUrl = tweet.Media.First().MediaURL,
                                Timestamp = tweet.CreatedAt,
                                Author = author,
                                Footer = footer
                            };

                            embeds.Add(embed);
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
                                Description = tweet.Text,
                                Color = DiscordColor.Aquamarine,
                                Timestamp = tweet.CreatedAt,
                                Author = author,
                                Footer = footer
                            };

                            embeds.Add(embed);
                        }
                    }

                    var msg = new DiscordMessageBuilder()
                        .WithEmbed(embeds.ElementAt(0))
                        .WithReply(ctx.Member.Id, true);

                    var message = await ctx.RespondAsync(msg);

                    var pointleft = DiscordEmoji.FromName(ctx.Client, ":point_left:");
                    var pointright = DiscordEmoji.FromName(ctx.Client, ":point_right:");

                    await message.CreateReactionAsync(pointleft);
                    await message.CreateReactionAsync(pointright);

                    var left = await interactivity.WaitForReactionAsync(x => x.Emoji == pointleft, ctx.User, TimeSpan.FromSeconds(15));
                    var right = await interactivity.WaitForReactionAsync(x => x.Emoji == pointright, ctx.User, TimeSpan.FromSeconds(15));

                    int j = 0;

                    
                    while (!left.TimedOut && !right.TimedOut)
                    {
                        var result = await interactivity.CollectReactionsAsync(message, TimeSpan.FromSeconds(1));
                        foreach (var emoji in result)
                        {
                            if (emoji.Emoji == pointleft)
                            {
                                if (j != 0)
                                {
                                    j--;
                                    await message.ModifyAsync(embeds.ElementAt(j));
                                }
                            }
                            else if (emoji.Emoji == pointright)
                            {
                                if (j != 4)
                                {
                                    j++;
                                    await message.ModifyAsync(embeds.ElementAt(j));
                                }
                            }
                        }
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