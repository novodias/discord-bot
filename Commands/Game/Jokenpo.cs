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
        private readonly InteractivityExtension _interactivity;
        private readonly DiscordMember _userone;
        private readonly DiscordMember _usertwo;
        private readonly DiscordChannel _chn;
        private readonly PaginationEmojis _emojis;
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

            bool emjisnull = await CheckEmojiIsNullAsync(emjuserone, emjusertwo);
            
            switch ( emjisnull )
            {
                case true:
                    // Just goes to default to delete the DM messages.
                    goto default;

                case false:
                    await CheckWinnerAsync(emjuserone, emjusertwo);
                    goto default;

                default:
                    await DeletePrivateMessagesAsync(msgone, msgtwo);
                break;
            }
        }

        private async Task CheckWinnerAsync(DiscordEmoji? emj, DiscordEmoji? emjsecond)
        {
            if (this._botmsg is null) { throw new Exception("Jokenpo -> _botmsg is null at Jokenpo.cs"); }

            if (emj == _emojis.Scissors && emjsecond == _emojis.Paper)
                await this._botmsg.ModifyAsync($"{_userone.Mention} venceu! {emj} x {emjsecond}");

            else if (emj == _emojis.Rock && emjsecond == _emojis.Scissors)
                await this._botmsg.ModifyAsync($"{_userone.Mention} venceu! {emj} x {emjsecond}");

            else if (emj == _emojis.Paper && emjsecond == _emojis.Rock)
                await this._botmsg.ModifyAsync($"{_userone.Mention} venceu! {emj} x {emjsecond}");

            else if (emj == emjsecond)
                await this._botmsg.ModifyAsync($"Empatou! {emj} x {emjsecond}");

            else
                await this._botmsg.ModifyAsync($"{_usertwo.Mention} venceu! {emjsecond} x {emj}");
        }

        private async Task<bool> CheckEmojiIsNullAsync(DiscordEmoji? emj, DiscordEmoji? emjsecond)
        {
            if (this._botmsg is null) { throw new Exception("Jokenpo -> _botmsg is null at Jokenpo.cs"); }

            if ( emj is null && emjsecond is null ) 
            { 
                await this._botmsg.ModifyAsync("Ninguém reagiu"); 
                return true; 
            }
            else if ( emj is null && emjsecond is not null ) 
            { 
                await this._botmsg.ModifyAsync($"{this._userone.Mention} não reagiu"); 
                return true; 
            }
            else if ( emj is not null && emjsecond is null ) 
            { 
                await this._botmsg.ModifyAsync($"{this._usertwo.Mention} não reagiu"); 
                return true; 
            }
            else return false;
        }

        private async static Task DeletePrivateMessagesAsync(DiscordMessage msg, DiscordMessage msgsecond)
        {
            // Avoids rate limit
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Do this to not flood users DM chat.
            await msg.DeleteAsync();
            await msgsecond.DeleteAsync();
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