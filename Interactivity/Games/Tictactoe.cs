using DSharpPlus;
using DSharpPlus.Entities;
using ConcurrentCollections;
using Microsoft.Extensions.Logging;
using DSharpPlus.EventArgs;

namespace DiscordBot.Interactivity.Games
{
    public class Tictactoe : IGame
    {
        readonly DiscordClient _client;
        readonly ConcurrentHashSet<TictactoeRequest> _requests;
        public Tictactoe(DiscordClient client) : base()
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<TictactoeRequest>();

            this._client.MessageReactionAdded += HandleReaction;
        }

        private Task HandleReaction(DiscordClient client, MessageReactionAddEventArgs e)
        {
            if ( this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run( async () => 
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    var usrs = await req.GetUsersAsync().ConfigureAwait(false);
                    var board = req.Board;

                    var emjlist = new List<DiscordEmoji>()
                    {
                        emojis.One,
                        emojis.Two,
                        emojis.Three,
                        emojis.Four,
                        emojis.Five,
                        emojis.Six,
                        emojis.Seven,
                        emojis.Eight,
                        emojis.Nine
                    };

                    if ( msg.Id == e.Message.Id )
                    {
                        if ( req.Turn == e.User )
                        {
                            if (emjlist.Any( x => x == e.Emoji ))
                            {
                                await ModifyMessage(req, e.Emoji);
                                bool win = await CheckWinner(req.Board, e.Emoji);
                                if (win)
                                {
                                    await msg.ModifyAsync($"{e.User.Mention} ganhou!");
                                    return;
                                }
                                req.Turn = usrs.Single( x => x != req.Turn );
                            }
                            else
                            {
                                await msg.DeleteReactionAsync(e.Emoji, e.User).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await msg.DeleteReactionAsync(e.Emoji, e.User).ConfigureAwait(false);
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        private async Task ModifyMessage(TictactoeRequest r, DiscordEmoji emoji)
        {
            var users = await r.GetUsersAsync();
            var emojis = await r.GetEmojisAsync();
            var m = await r.GetMessageAsync();
            var board = r.Board;
            DiscordEmoji play = emojis.BlackSquare;

            if ( r.Turn == users.ElementAt(0) )
                play = DiscordEmoji.FromUnicode("❌");
            else if ( r.Turn == users.ElementAt(1) )
                play = DiscordEmoji.FromUnicode("⭕");

            if (emoji == emojis.One)
            {
                await m.DeleteReactionsEmojiAsync(emojis.One);
                board[0, 0] = $"{play}";
            }
            if (emoji == emojis.Two)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Two);
                board[0, 1] = $"{play}";
            }
            if (emoji == emojis.Three)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Three);
                board[0, 2] = $"{play}";
            }
            if (emoji == emojis.Four)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Four);
                board[1, 0] = $"{play}";
            }
            if (emoji == emojis.Five)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Five);
                board[1, 1] = $"{play}";
            }
            if (emoji == emojis.Six)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Six);
                board[1, 2] = $"{play}";
            }
            if (emoji == emojis.Seven)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Seven);
                board[2, 0] = $"{play}";
            }
            if (emoji == emojis.Eight)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Eight);
                board[2, 1] = $"{play}";
            }
            if (emoji == emojis.Nine)
            {
                await m.DeleteReactionsEmojiAsync(emojis.Nine);
                board[2, 2] = $"{play}";
            }

            // var embed = new DiscordEmbedBuilder();
            // embed.AddField("0", board[0, 0] + board[0, 1] + board[0, 2], false);
            // embed.AddField("1", board[1, 0] + board[1, 1] + board[1, 2], false);
            // embed.AddField("2", board[2, 0] + board[2, 1] + board[2, 2], false);
            // string content = GetStringFromArray(board);

            string one = board[0, 0] + board[0, 1] + board[0, 2] + "\n";
            string two = board[1, 0] + board[1, 1] + board[1, 2] + "\n";
            string thr = board[2, 0] + board[2, 1] + board[2, 2];


            var builder = new DiscordMessageBuilder()
                .WithContent(one + two + thr);

            this._requests.Single(x => x == r).Board = board;

            await m.ModifyAsync(builder);
        }

        private static string[] GetRow(string[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        private static string[] GetColumn(string[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        private static async Task<bool> CheckWinner(string[,] board, DiscordEmoji emj)
        {
            await Task.Yield();

            // string[] preset01 = {$"{emj}", $"{emj}", $"{emj}\n"};
            // string[] preset02 = {$"{emj}", $"{emj}", $"{emj}"};
            // string[] preset03 = {$"{emj}\n", $"{emj}\n", $"{emj}\n"};

            // if (GetRow(board, 0) == preset01)
            //     return true;
            // else if (GetRow(board, 1) == preset01)
            //     return true;
            // else if (GetRow(board, 2) == preset02)
            //     return true;
            // else if (GetColumn(board, 0) == preset02)
            //     return true;
            // else if (GetColumn(board, 1) == preset02)
            //     return true;
            // else if (GetColumn(board, 2) == preset03)
            //     return true;

            if (GetRow(board, 0).All(x => x == emj.GetDiscordName()))
                return true;
            else if (GetRow(board, 1).All(x => x == emj.GetDiscordName()))
                return true;
            else if (GetRow(board, 2).All(x => x == emj.GetDiscordName()))
                return true;
            else if (GetColumn(board, 0).All(x => x == emj.GetDiscordName()))
                return true;
            else if (GetColumn(board, 1).All(x => x == emj.GetDiscordName()))
                return true;
            else if (GetColumn(board, 2).All(x => x == emj.GetDiscordName()))
                return true;
            else if (board[0,0] == emj.GetDiscordName() && board[1,1] == emj.GetDiscordName() && board[2,2] == emj.GetDiscordName())
                return true;
            else if (board[0,2] == emj.GetDiscordName() && board[1,1] == emj.GetDiscordName() && board[2,0] == emj.GetDiscordName())
                return true;
            else 
                return false;
        }

        public async Task InitializeTask(params object[] args)
        {
            var userone = (DiscordUser)args[0];
            var usertwo = (DiscordUser)args[1];
            var chn = (DiscordChannel)args[2];
            
            List<DiscordUser> users = new() { userone, usertwo };
            var request = await CreateRequest(users, chn);

            this._requests.Add(request);

            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(LoggerEvents.Misc, ex, "Tictactoe got a error while setting up");
            }
            finally
            {
                this._requests.TryRemove(request);
            }
        }

        private static async Task<TictactoeRequest> CreateRequest(IEnumerable<DiscordUser> users, DiscordChannel chn)
        {
            var emojis = new TicTacToeEmojis();
            var emj = emojis.BlackSquare;

            string[,] board = 
            { 
                {$"{emj}",$"{emj}",$"{emj}\n"}, 
                {$"{emj}",$"{emj}",$"{emj}\n"}, 
                {$"{emj}",$"{emj}",$"{emj}"}
            };

            // var embed = new DiscordEmbedBuilder();
            // embed.AddField("0", board[0, 0] + board[0, 1] + board[0, 2], false);
            // embed.AddField("1", board[1, 0] + board[1, 1] + board[1, 2], false);
            // embed.AddField("2", board[2, 0] + board[2, 1] + board[2, 2], false);

            string one = board[0, 0] + board[0, 1] + board[0, 2] + "\n";
            string two = board[1, 0] + board[1, 1] + board[1, 2] + "\n";
            string thr = board[2, 0] + board[2, 1] + board[2, 2];

            var msg = new DiscordMessageBuilder()
                .WithContent(one + two + thr);
                // .WithContent($"{emj}{emj}{emj}\n" + $"{emj}{emj}{emj}\n" + $"{emj}{emj}{emj}");
            
            var m = await msg.SendAsync(chn);

            await m.CreateReactionAsync(emojis.One);
            await m.CreateReactionAsync(emojis.Two);
            await m.CreateReactionAsync(emojis.Three);
            await m.CreateReactionAsync(emojis.Four);
            await m.CreateReactionAsync(emojis.Five);
            await m.CreateReactionAsync(emojis.Six);
            await m.CreateReactionAsync(emojis.Seven);
            await m.CreateReactionAsync(emojis.Eight);
            await m.CreateReactionAsync(emojis.Nine);

            await Task.Delay(TimeSpan.FromSeconds(1));

            return new TictactoeRequest(m, users, board, emojis, TimeSpan.FromSeconds(120));
        }
    }
    public class TictactoeRequest
    {
        // private readonly InteractivityExtension _interactivity;
        private TaskCompletionSource<bool> _tcs;
        private readonly CancellationTokenSource _ct;
        private readonly TimeSpan _timeout;
        private readonly DiscordMessage _botmsg;
        private readonly IEnumerable<DiscordUser> _users;
        private readonly TicTacToeEmojis _emojis;
        private DiscordUser _turn;
        private string[,] _board;

        public TictactoeRequest(DiscordMessage msg, IEnumerable<DiscordUser> users, string[,] board, TicTacToeEmojis emojis, TimeSpan timeout)
        {
            // this._interactivity = client.GetInteractivity();
            this._tcs = new();
            this._ct = new(timeout);
            this._ct.Token.Register( () => this._tcs.TrySetResult(true));
            this._timeout = timeout;

            this._botmsg = msg;
            this._users = users;
            this._emojis = emojis;
            this._board = board;
            this._turn = this._users.ElementAt(0);
        }

        public string[,] Board 
        {
            get { return this._board; } 
            set { this._board = value; }
        }

        public DiscordUser Turn
        {
            get { return this._turn; }
            set { this._turn = value; }
        }

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();

            return this._botmsg;
        }

        public async Task<IEnumerable<DiscordUser>> GetUsersAsync()
        {
            await Task.Yield();

            return this._users;
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();

            return this._tcs;
        }

        public async Task<TicTacToeEmojis> GetEmojisAsync()
        {
            await Task.Yield();

            return this._emojis;
        }

        ~TictactoeRequest()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this._ct.Dispose();
            // this._tcs = null;
        }
    }

    public class TicTacToeEmojis
    {
        public DiscordEmoji BlackSquare;
        public DiscordEmoji One;
        public DiscordEmoji Two;
        public DiscordEmoji Three;
        public DiscordEmoji Four;
        public DiscordEmoji Five;
        public DiscordEmoji Six;
        public DiscordEmoji Seven;
        public DiscordEmoji Eight;
        public DiscordEmoji Nine;

        public TicTacToeEmojis()
        {
            BlackSquare = DiscordEmoji.FromUnicode("⬛");
            One = DiscordEmoji.FromUnicode("1️⃣");
            Two = DiscordEmoji.FromUnicode("2️⃣");
            Three = DiscordEmoji.FromUnicode("3️⃣");
            Four = DiscordEmoji.FromUnicode("4️⃣");
            Five = DiscordEmoji.FromUnicode("5️⃣");
            Six = DiscordEmoji.FromUnicode("6️⃣");
            Seven = DiscordEmoji.FromUnicode("7️⃣");
            Eight = DiscordEmoji.FromUnicode("8️⃣");
            Nine = DiscordEmoji.FromUnicode("9️⃣");
        }
        
    }
}