using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DiscordBot.Interactivity.Games;

namespace DiscordBot.Commands.Games
{
    public class ModuleGames : BaseCommandModule
    {
        private Game? Game;

        [Command("jokenpo")]
        public async Task JokenpoCommand(CommandContext ctx, DiscordUser opponent)
        {
            await ctx.TriggerTypingAsync();

            if ( Game is null ) 
                Game = new(ctx.Client);

            var user_one = ctx.Member;
            var user_two = await ctx.Guild.GetMemberAsync(opponent.Id);

            Game.Start<Jokenpo>(user_one, user_two, ctx.Channel);
        }

        [Command("trivia")]
        public async Task TriviaCommand(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if ( Game is null ) 
                Game = new(ctx.Client);

            Game.Start<Trivia>(ctx.Channel);
        }

        [Command("tictactoe")]
        public async Task TictactoeCommand(CommandContext ctx, DiscordUser user)
        {
            await ctx.TriggerTypingAsync();

            if ( Game is null ) 
                Game = new(ctx.Client);

            try
            {
                Game.Start<Tictactoe>(ctx.User, user, ctx.Channel);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message + ex.StackTrace);
            }
        }
    }
}