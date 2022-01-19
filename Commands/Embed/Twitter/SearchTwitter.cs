using DSharpPlus.Entities;
using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Parameters.V2;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class SearchTwitter
    {
        public static async Task<List<DiscordEmbed>> GenerateEmbed(string strUser)
        {
            string[] keys = File.ReadAllLines("files/twitterkey.txt");

            List<DiscordEmbed> embeds = new();

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

            return embeds;
        }
    }
}