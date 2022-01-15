using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace DiscordBot.Interactivity.Embeds
{
    internal class EmbedRequest : IEmbedRequest
    {
        private TaskCompletionSource<bool> _tcs;
        private readonly CancellationTokenSource _ct;
        private readonly TimeSpan _timeout;
        private readonly List<DiscordEmbed> _embeds;
        private readonly DiscordMessage _msg;
        private readonly PaginationEmojis _emojis;
        private readonly DiscordUser _user;
        private int _index = 0;
        public EmbedRequest(List<DiscordEmbed> embeds, DiscordMessage msg, DiscordUser user, PaginationEmojis emojis, TimeSpan timeout)
        {
            this._tcs = new();
            this._ct = new(timeout);
            this._ct.Token.Register( () => this._tcs.TrySetResult(true));
            this._timeout = timeout;

            this._embeds = embeds;
            this._msg = msg;
            this._user = user;

            this._emojis = emojis;

            // this._client.MessageReactionAdded += OnReactionAdded;
            // this._client.MessageReactionRemoved += OnReactionRemoved;
        }

        public int EmbedCount => this._embeds.Count;

        public async Task<DiscordEmbed> GetEmbedAsync()
        {
            await Task.Yield();

            return this._embeds[this._index];
        }

        public async Task NextEmbedAsync()
        {
            await Task.Yield();

            if (this._index == this._embeds.Count -1) { return; }
            else { this._index++; }
        }

        public async Task PreviousEmbedAsync()
        {
            await Task.Yield();

            if (this._index == 0) { return; }
            else { this._index--; }
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();

            return this._emojis;
        }

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();

            return this._msg;
        }

        public async Task<DiscordUser> GetUserAsync()
        {
            await Task.Yield();

            return this._user;
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();

            return this._tcs;
        }
        
        ~EmbedRequest()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null; 
            this._embeds.Clear();
            this._index = 0;
        }
    }

    public class PaginationEmojis
    {
        public DiscordEmoji Left;
        public DiscordEmoji Right;

        public PaginationEmojis()
        {
            this.Left = DiscordEmoji.FromUnicode("👈");
            this.Right = DiscordEmoji.FromUnicode("👉");
        }

    }
}