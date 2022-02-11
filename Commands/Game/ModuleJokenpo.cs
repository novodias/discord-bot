using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DiscordBot.Interactivity.Games;

namespace DiscordBot.Commands.Game
{
    public class ModuleJokenpo : BaseCommandModule
    {
        private Jokenpo? Jokenpo;
        [Command("jokenpo")]
        public async Task JokenpoCommand(CommandContext ctx, DiscordUser opponent)
        {
            await ctx.TriggerTypingAsync();

            if ( Jokenpo is null ) { Jokenpo = new(ctx.Client); }

            var user_one = ctx.Member;
            var user_two = await ctx.Guild.GetMemberAsync(opponent.Id);

            await Jokenpo.InitializeTask(user_one, user_two, ctx.Channel);
        }
    }
}