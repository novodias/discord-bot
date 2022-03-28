using DSharpPlus;

namespace DiscordBot.Interactivity.Games
{
    public interface IGame
    {
        public Task InitializeTask(params object[] args);
    }

    public class Game
    {
        public Game( DiscordClient client )
        {
            _jokenpo = new( client );
            _trivia = new( client );
            _tictactoe = new( client );
        }

        private readonly Jokenpo _jokenpo;
        private readonly Trivia _trivia;
        private readonly Tictactoe _tictactoe;

        public async void Start<T>(params object[] args) where T : IGame
        {
            if ( typeof(T) == typeof(Trivia) )
            {
                await this._trivia.InitializeTask(args);
            }
            else if ( typeof(T) == typeof(Jokenpo) )
            {
                await this._jokenpo.InitializeTask(args);
            }
            else if ( typeof(T) == typeof(Tictactoe) )
            {
                await this._tictactoe.InitializeTask(args);
            }
            else { }
        }
    }
}