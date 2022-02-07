using DSharpPlus;
using ConcurrentCollections;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.Commands.Game
{
    public class Jokenpo
    {
        readonly DiscordClient _client;
        readonly ConcurrentHashSet<JokenpoRequest> _requests;
        public Jokenpo(DiscordClient client)
        {
            this._client = client;
            this._requests = new();
        }

        public async Task InitializeTask(DiscordMember user, DiscordMember usersecond, DiscordChannel chn)
        {
            if (this._client is null) { throw new Exception("Jokenpo -> client cannot be null at Jokenpo.cs"); }

            var emojis = new PaginationEmojis();

            var users = new List<DiscordMember>(2) { user, usersecond };

            var request = new JokenpoRequest(this._client, users, chn, emojis);

            _requests.Add(request);
            await _requests.Single(x => x == request).Setup();
            
            try
            {
                _requests.TryRemove(request);
            }
            catch (System.Exception)
            {
                throw new Exception("Something went wrong trying to remove Jokenpo Request from Concurrent Hash Set");
            }
        }

        
    }

    public class JokenpoRequest
    {
        private InteractivityExtension _interactivity;
        private DiscordMember _userone;
        private DiscordMember _usertwo;
        private DiscordChannel _chn;
        private PaginationEmojis _emojis;
        private DiscordMessage? _botmsg;
        public JokenpoRequest(DiscordClient client, IEnumerable<DiscordMember> users, DiscordChannel chn, PaginationEmojis emojis)
        {
            _interactivity = client.GetInteractivity();

            _userone = users.First();
            _usertwo = users.Last();

            _chn = chn;

            _emojis = emojis;
        }

        public async Task Setup()
        {
            if ( _userone is null || _usertwo is null || _userone == _usertwo || _usertwo.IsBot )
            {
                _botmsg = await new DiscordMessageBuilder()
                    .WithContent($"Opa, calma lá amigo, tem algo de errado com o comando que você digitou.")
                    .SendAsync(_chn);

                return;
            }

            _botmsg = await new DiscordMessageBuilder()
                .WithContent("Esperando o resultado")
                .SendAsync(_chn);
            
            var msg = new DiscordMessageBuilder()
                .WithContent("Escolha entre papel, pedra ou tesoura");

            var msgone = await _userone.SendMessageAsync(msg);
            var msgtwo = await _usertwo.SendMessageAsync(msg);

            var t1 = GetEmojiResult(msgone, _userone);
            var t2 = GetEmojiResult(msgtwo, _usertwo);
            await Task.WhenAll(t1, t2);

            DiscordEmoji? emjuserone = await t1;
            DiscordEmoji? emjusertwo = await t2;

            if ( emjuserone is null && emjusertwo is null ) { await _botmsg.ModifyAsync("Ninguém reagiu"); return; }
            if ( emjuserone is null && emjusertwo is not null ) { await _botmsg.ModifyAsync($"{_userone.Mention} não reagiu"); return; }
            if ( emjuserone is not null && emjusertwo is null ) { await _botmsg.ModifyAsync($"{_usertwo.Mention} não reagiu"); return; }

            if (emjuserone == _emojis.Scissors && emjusertwo == _emojis.Paper)
                await _botmsg.ModifyAsync($"{_userone.Mention} venceu! {emjuserone} x {emjusertwo}");

            else if (emjuserone == _emojis.Rock && emjusertwo == _emojis.Scissors)
                await _botmsg.ModifyAsync($"{_userone.Mention} venceu! {emjuserone} x {emjusertwo}");

            else if (emjuserone == _emojis.Paper && emjusertwo == _emojis.Rock)
                await _botmsg.ModifyAsync($"{_userone.Mention} venceu! {emjuserone} x {emjusertwo}");

            else if (emjuserone == emjusertwo)
                await _botmsg.ModifyAsync($"Empatou! {emjuserone} x {emjusertwo}");

            else
                await _botmsg.ModifyAsync($"{_usertwo.Mention} venceu! {emjusertwo} x {emjuserone}");

            // We don't this to flood users DM chat.
            await msgone.DeleteAsync();
            await msgtwo.DeleteAsync();
        }

        private async Task<DiscordEmoji?> GetEmojiResult(DiscordMessage msg, DiscordMember usr)
        {
            await msg.CreateReactionAsync(_emojis.Scissors);
            await msg.CreateReactionAsync(_emojis.Rock);
            await msg.CreateReactionAsync(_emojis.Paper);

            var emojiEvent = await _interactivity.WaitForReactionAsync(msg, usr, TimeSpan.FromSeconds(10));

            while (!emojiEvent.TimedOut)
            {
                if (emojiEvent.Result != null)
                {
                    return emojiEvent.Result.Emoji;
                }
            }

            return null;
        }


    }

    public class PaginationEmojis
    {
        public DiscordEmoji Scissors;
        public DiscordEmoji Rock;
        public DiscordEmoji Paper;

        public PaginationEmojis()
        {
            this.Scissors = DiscordEmoji.FromUnicode("✂️");
            this.Rock = DiscordEmoji.FromUnicode("🪨");
            this.Paper = DiscordEmoji.FromUnicode("📰");
        }
    }
}