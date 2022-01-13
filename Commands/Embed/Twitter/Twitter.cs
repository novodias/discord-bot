using ConcurrentCollections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Embed.Twitter
{
    public class TwitterSend
    {
        private readonly Twitter Twitter;

        public TwitterSend(DiscordClient client)
        {
            this.Twitter = new Twitter(client);
        }

        public async Task SendEmbedMessageAsync(DiscordChannel channel, DiscordUser user, IEnumerable<DiscordEmbed> embeds, TimeSpan timeout)
        {
            var builder = new DiscordMessageBuilder()
                .WithEmbed(embeds.First());
            var m = await builder.SendAsync(channel).ConfigureAwait(false);

            PaginationEmojis emojis = new();

            var prequest = new TwitterRequest(embeds.ToList(), m, user, emojis, timeout);

            await this.Twitter.DoPaginationAsync(prequest).ConfigureAwait(false);
        }
    }
    internal class Twitter : ITwitter
    {
        readonly DiscordClient _client;
        readonly ConcurrentHashSet<ITwitterRequest> _requests;

        public Twitter(DiscordClient client)
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<ITwitterRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
        }

        public async Task DoPaginationAsync(ITwitterRequest request)
        {
            await ResetReactionsAsync(request).ConfigureAwait(false);
            this._requests.Add(request);
            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");   
            }
            finally
            {
                this._requests.TryRemove(request);
            }
        }

        private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs e)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () => 
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    var usr = await req.GetUserAsync().ConfigureAwait(false);

                    if (msg.Id == e.Message.Id)
                    {
                        if (e.User.Id == usr.Id)
                        {
                            if (req.EmbedCount > 1 && e.Emoji == emojis.Left ||
                            e.Emoji == emojis.Right)
                            {
                                await PaginateAsync(req, e.Emoji).ConfigureAwait(false);
                            }
                            else
                            {
                                await msg.DeleteReactionAsync(e.Emoji, e.User).ConfigureAwait(false);
                            }
                        }
                        else if (e.User.Id != this._client.CurrentUser.Id)
                        {
                            if (e.Emoji != emojis.Left && e.Emoji != emojis.Right)
                            {
                                await msg.DeleteReactionAsync(e.Emoji, e.User).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs e)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () => 
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    var usr = await req.GetUserAsync().ConfigureAwait(false);

                    if (msg.Id == e.Message.Id)
                    {
                        if (e.User.Id == usr.Id)
                        {
                            if (req.EmbedCount > 1 && e.Emoji == emojis.Left ||
                            e.Emoji == emojis.Right)
                            {
                                await PaginateAsync(req, e.Emoji).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        private static async Task ResetReactionsAsync(ITwitterRequest p)
        {
            var msg = await p.GetMessageAsync().ConfigureAwait(false);
            var emojis = await p.GetEmojisAsync().ConfigureAwait(false);

            var chn = msg.Channel;
            var gld = chn?.Guild;
            var mbr = gld?.CurrentMember;

            if ( mbr != null && (chn.PermissionsFor(mbr) & Permissions.ManageChannels) != 0)
                await msg.DeleteAllReactionsAsync("Pagination").ConfigureAwait(false);


            if (p.EmbedCount > 1)
            {
                if (emojis.Left != null)
                    await msg.CreateReactionAsync(emojis.Left).ConfigureAwait(false);
                if (emojis.Right != null)
                    await msg.CreateReactionAsync(emojis.Right).ConfigureAwait(false);
            }
        }

        private static async Task PaginateAsync(ITwitterRequest p, DiscordEmoji emoji)
        {
            var emojis = await p.GetEmojisAsync().ConfigureAwait(false);
            var msg = await p.GetMessageAsync().ConfigureAwait(false);

            if (emoji == emojis.Left)
                await p.PreviousEmbedAsync().ConfigureAwait(false);
            else if (emoji == emojis.Right)
                await p.NextEmbedAsync().ConfigureAwait(false);

            var embed = await p.GetEmbedAsync().ConfigureAwait(false);
            var builder = new DiscordMessageBuilder()
                .WithEmbed(embed);

            await builder.ModifyAsync(msg).ConfigureAwait(false);
        }

        ~Twitter()
        {
            this.Dispose();
        }
        
        public void Dispose()
        {
            this._client.MessageReactionAdded -= this.HandleReactionAdd;
            this._client.MessageReactionRemoved -= this.HandleReactionRemove;
            // this._client = null;
            this._requests.Clear();
            // this._requests = null;
        }
    }
}