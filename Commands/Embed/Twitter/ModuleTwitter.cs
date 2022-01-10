using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Parameters.V2;
using DSharpPlus.Entities;
using ConcurrentCollections;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;
using DSharpPlus.EventArgs;
using DSharpPlus;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class ModuleTwitter : BaseCommandModule
    {
        private List<DiscordEmbed>? embeds;
        private int j;
        private ulong MessageId = 0;
        private async Task OnReactionAdded(DiscordClient client, MessageReactionAddEventArgs e)
        {
            var message = e.Message;
            var pointleft = DiscordEmoji.FromName(client, ":point_left:");
            var pointright = DiscordEmoji.FromName(client, ":point_right:");

            if (embeds != null && this.MessageId == e.Message.Id)
            {
                if (this.j != 0 && e.Emoji == pointleft && !e.User.IsBot)
                {
                    this.j--;
                    await message.ModifyAsync(embeds.ElementAt(this.j));
                }
                else if (this.j != 4 && e.Emoji == pointright && !e.User.IsBot)
                {
                    this.j++;
                    await message.ModifyAsync(embeds.ElementAt(this.j));
                }
            }
            
        }

        private async Task OnReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs e)
        {
            var message = e.Message;
            var pointleft = DiscordEmoji.FromName(client, ":point_left:");
            var pointright = DiscordEmoji.FromName(client, ":point_right:");


            if (embeds != null && this.MessageId == e.Message.Id)
            {
                if (this.j != 0 && e.Emoji == pointleft && !e.User.IsBot)
                {
                    this.j--;
                    await message.ModifyAsync(embeds.ElementAt(this.j));
                }
                else if (this.j != 4 && e.Emoji == pointright && !e.User.IsBot)
                {
                    this.j++;
                    await message.ModifyAsync(embeds.ElementAt(this.j));
                }
            }
        }

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
                    this.embeds = new(); 
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

                            var mediaurl = tweet.Media.First().MediaURL;

                            if (mediaurl.Contains("tweet_video_thumb"))
                            {
                                // mediaurl = mediaurl.Replace("pbs.", "video.");
                                // mediaurl = mediaurl.Remove(mediaurl.IndexOf("_thumb"), 6);
                                // mediaurl = mediaurl.Replace(".jpg", ".mp4");
                                var embed = new DiscordEmbedBuilder()
                                {
                                    Title = user.User.Name,
                                    Description = $"{tweet.Text} {tweet.Media.First().VideoDetails.Variants.First().URL}",
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
                                var embed = new DiscordEmbedBuilder()
                                {
                                    Title = user.User.Name,
                                    Description = tweet.Text,
                                    Color = DiscordColor.Aquamarine,
                                    ImageUrl = mediaurl,
                                    Timestamp = tweet.CreatedAt,
                                    Author = author,
                                    Footer = footer
                                };
                                embeds.Add(embed);
                            }


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

                    this.MessageId = message.Id;

                    var pointleft = DiscordEmoji.FromName(ctx.Client, ":point_left:");
                    var pointright = DiscordEmoji.FromName(ctx.Client, ":point_right:");

                    await message.CreateReactionAsync(pointleft);
                    await message.CreateReactionAsync(pointright);

                    TimeSpan time = TimeSpan.FromSeconds(DateTime.Now.Second + 30);
                    TimeSpan now = TimeSpan.Zero;

                    ctx.Client.MessageReactionAdded += this.OnReactionAdded;
                    ctx.Client.MessageReactionRemoved += this.OnReactionRemoved;

                    while (now < time)
                    {
                        now = TimeSpan.FromSeconds(DateTime.Now.Second);
                    }

                    ctx.Client.MessageReactionAdded -= this.OnReactionAdded;
                    ctx.Client.MessageReactionRemoved -= this.OnReactionRemoved;

                    this.embeds = null;
                    this.j = 0;
                    

                    //await ctx.RespondAsync(message);
                }
                catch (Exception ex)
                {
                    await ctx.RespondAsync(ex.Message + ex.StackTrace);
                }
            }
            catch (Tweetinvi.Exceptions.TwitterException ex)
            {
                await ctx.RespondAsync("Twitter user não encontrado ou API não funcionando caso aconteça novamente");
            }

        }
    }

}