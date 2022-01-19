using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot.Commands.Embed.Twitch
{
    public class ModuleTwitch : BaseCommandModule
    {
        private Twitch? Twitch;

        [Command("twitch")]
        public async Task TwitchCommand(CommandContext ctx, [RemainingText] string input)
        {
            await ctx.TriggerTypingAsync();

            if (Twitch == null) { Twitch = new(); }
            
            try
            {
                var msg = await Twitch.GetStreamEmbed(input);
                msg.WithReply(ctx.Message.Id, true);
                await msg.SendAsync(ctx.Channel);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }

        }
    }
}