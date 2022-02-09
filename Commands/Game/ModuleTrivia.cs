using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands.Game
{
    public class ModuleTrivia : BaseCommandModule
    {
        private Trivia? Trivia;
        [Command("trivia")]
        public async Task TriviaCommand(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if ( Trivia is null ) { Trivia = new(ctx.Client); }

            await Trivia.InitializeTask(ctx.Channel);
        }
    }
}